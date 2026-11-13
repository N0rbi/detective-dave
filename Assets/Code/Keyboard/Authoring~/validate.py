#!/usr/bin/env python3
"""Validates the keyboard layout files in Assets/Game/Keyboard/Data.

Usage: python3 validate.py [path/to/Assets/Game/Keyboard]
Exit code 0 = no errors. Unverified keys are reported but are not errors.
"""
import glob, json, os, re, sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = sys.argv[1] if len(sys.argv) > 1 else os.path.normpath(os.path.join(HERE, '..', '..', '..', 'Game', 'Keyboard'))
PROJECT = os.path.normpath(os.path.join(HERE, '..', '..', '..', '..'))
EPS = 1e-6
errors, warnings = [], []

def err(layout, msg): errors.append('%s: %s' % (layout, msg))

def unity_key_names():
    hits = glob.glob(os.path.join(PROJECT, 'Library', 'PackageCache', 'com.unity.inputsystem@*', 'InputSystem', 'Runtime', 'Devices', 'Keyboard.cs'))
    if not hits: return None
    text = open(hits[0], encoding='utf-8-sig').read()
    start = text.index('public enum Key')
    body = text[text.index('{', start) + 1:]
    body = body[:body.index('\n    }')]
    return set(re.findall(r'^\s{8}([A-Za-z0-9]+)\s*(?:=\s*\d+)?\s*,?\s*$', body, re.M))

key_names = unity_key_names()
if key_names is None:
    warnings.append('Unity Input System package not found in Library/PackageCache: key names were not checked against the Key enum.')

layouts = {}
for path in sorted(glob.glob(os.path.join(ROOT, 'Data', '*.json'))):
    name = os.path.splitext(os.path.basename(path))[0]
    try:
        layouts[name] = json.load(open(path, encoding='utf-8'))
    except Exception as e:
        err(name, 'invalid JSON: %s' % e)

if not layouts:
    errors.append('no layout files found in %s' % os.path.join(ROOT, 'Data'))

def rects_of(k): return [(r['x'], r['y'], r['w'], r['h']) for r in k['rects']]

for name, L in layouts.items():
    for f in ('id', 'name', 'digit', 'form', 'unitsWide', 'unitsHigh', 'keys'):
        if f not in L: err(name, 'missing field "%s"' % f)
    if any(f not in L for f in ('id', 'digit', 'form', 'keys')): continue
    if L['id'] != name: err(name, 'id "%s" does not match file name' % L['id'])
    if L['form'] not in ('ansi', 'iso', 'jis'): err(name, 'unknown form "%s"' % L['form'])
    if not (1 <= L['digit'] <= 9): err(name, 'digit %s is not 1..9' % L['digit'])
    W, H = L['unitsWide'], L['unitsHigh']

    codes, allrects = set(), []
    for k in L['keys']:
        c = k.get('code', '')
        if not c: err(name, 'a key has no code'); continue
        if c in codes: err(name, 'duplicate key code %s' % c)
        codes.add(c)
        if key_names is not None and c not in key_names: err(name, '%s is not a Unity Key name' % c)
        if not k.get('label'): err(name, '%s has no label' % c)
        if not k.get('rects'): err(name, '%s has no rects' % c); continue
        for (x, y, w, h) in rects_of(k):
            if w <= 0 or h <= 0: err(name, '%s has an empty rect' % c)
            if x < -EPS or y < -EPS or x + w > W + EPS or y + h > H + EPS: err(name, '%s is outside the board' % c)
            allrects.append((c, x, y, w, h))
        cx, cy = k['center']['x'], k['center']['y']
        if not any(x - EPS <= cx <= x + w + EPS and y - EPS <= cy <= y + h + EPS for (x, y, w, h) in rects_of(k)):
            err(name, '%s centre is not inside the key' % c)

    for i in range(len(allrects)):
        for j in range(i + 1, len(allrects)):
            a, b = allrects[i], allrects[j]
            if a[1] < b[1] + b[3] - EPS and b[1] < a[1] + a[3] - EPS and a[2] < b[2] + b[4] - EPS and b[2] < a[2] + a[4] - EPS:
                err(name, '%s and %s overlap' % (a[0], b[0]))

    for row in range(H):
        spans = sorted((x, w) for (_, x, y, w, h) in allrects if abs(y - row) < EPS)
        pos = 0.0
        for x, w in spans:
            if abs(x - pos) > EPS: err(name, 'row %d has a gap or overlap at x=%s' % (row, pos)); break
            pos = x + w
        else:
            if abs(pos - W) > EPS: err(name, 'row %d ends at %s, expected %s' % (row, pos, W))

    for need in ('LeftShift', 'RightShift'):
        if need not in codes: err(name, 'missing %s' % need)
    extras = {c for c in codes if c.startswith('OEM')}
    want = {'ansi': set(), 'iso': {'OEM1'}, 'jis': {'OEM2', 'OEM3'}}.get(L['form'])
    if want is not None and extras != want: err(name, 'extra keys are %s, expected %s for %s' % (sorted(extras), sorted(want), L['form']))

    for k in L['keys']:
        for a in k.get('alsoCodes', []):
            if a not in codes: err(name, '%s: alsoCodes entry %s is not a key of this layout' % (k['code'], a))
            if a == k['code']: err(name, '%s lists itself in alsoCodes' % a)

    unv = [k['code'] for k in L['keys'] if k.get('unverified')]
    if unv: warnings.append('%s: unverified keys: %s' % (name, ', '.join(unv)))

digits = {}
for name, L in layouts.items():
    if 'digit' in L: digits.setdefault(L['digit'], []).append(name)
for d, names in sorted(digits.items()):
    if len(names) > 1: errors.append('digit %d is used by %s' % (d, ', '.join(names)))

ref = None
for name, L in layouts.items():
    pos = {}
    for k in L.get('keys', []):
        if k.get('code') in ['Digit%d' % i for i in range(1, 10)] and k.get('rects'):
            pos[k['code']] = tuple(sorted(rects_of(k)))
    if len(pos) != 9: err(name, 'Digit1..Digit9 are not all present'); continue
    if ref is None: ref = (name, pos)
    elif pos != ref[1]: err(name, 'Digit1..Digit9 positions differ from %s' % ref[0])

print('layouts: %s' % ', '.join('%d=%s' % (L['digit'], n) for n, L in sorted(layouts.items(), key=lambda t: t[1].get('digit', 99))))
for w in warnings: print('note:', w)
for e in errors: print('ERROR:', e)
print('%d error(s)' % len(errors))
sys.exit(1 if errors else 0)
