#!/usr/bin/env python3
"""Renders one recolorable glyph mask per keyboard layout into Assets/Game/Keyboard/Art,
plus a preview contact sheet (default colors) in ./out.

Usage: python3 render_glyphs.py [--latin-font FILE] [--cjk-font FILE] [--cjk-index N]

The PNGs are masks, not pictures. Every color role is drawn on its own layer and packed into one
channel of an RGBA texture; KeyboardGlyph.shader layers them over the panel color at runtime.
Needs Pillow, a font covering Latin-1 + Hungarian (e.g. DejaVu Sans Bold) and one covering Japanese.
"""
import argparse
import glob
import json
import os
import sys
from typing import NamedTuple

from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
KEYBOARD_DIR = os.path.normpath(os.path.join(HERE, '..', '..', '..', 'Game', 'Keyboard'))
DATA_DIR = os.path.join(KEYBOARD_DIR, 'Data')
ART_DIR = os.path.join(KEYBOARD_DIR, 'Art')
PREVIEW_DIR = os.path.join(HERE, 'out')


# ---------------------------------------------------------------- measurements

class Metrics(NamedTuple):
    """All sizes, in texture pixels unless the name says otherwise."""
    px_per_key_unit: int = 64        # one key is 64 px wide; with 64 pixels per Unity unit that is one unit
    margin: int = 16                 # empty border around the whole panel
    header_height: int = 72          # strip above the keys: number badge and layout name
    supersample: int = 2             # everything is drawn at 2x and scaled down for smooth edges

    panel_corner_radius: int = 18    # keep in sync with _CornerRadius in KeyboardGlyph.shader
    keycap_inset: int = 3            # gap between a key's cell and its cap, on every side
    keycap_corner_radius: int = 9
    keycap_outline: int = 2          # thickness of the edge-colored outline around a cap

    label_side_padding: int = 14     # a label may use the key width minus this
    label_font_max: int = 30         # labels shrink from this size ...
    label_font_min: int = 14         # ... down to this size until they fit

    badge_radius: int = 28           # the number badge is a circle
    badge_inset: int = 8             # badge distance from the margin, both directions
    badge_digit_font: int = 34
    title_font: int = 30
    title_gap: int = 18              # between the badge and the layout name


M = Metrics()
S = M.supersample


# ---------------------------------------------------------------- colors and layers

# Color roles, in the channel order of the mask: R, G, B, A. The panel is the background, not a layer.
LAYER_ORDER = ('edge', 'key', 'text', 'accent')

# Only used for the preview contact sheet. The game reads KeyboardPalette.asset instead.
PREVIEW_PALETTE = {
    'panel': (20, 23, 32),
    'key': (48, 54, 70),
    'edge': (84, 92, 116),
    'text': (232, 236, 244),
    'accent': (242, 184, 75),
}

COVERED = 255   # drawing 255 on a layer means "this role is fully present here"
HOLE = 0        # drawing 0 cuts a hole in a layer (the digit inside the badge)

# Preview contact sheet
SHEET_SCALE = 0.75
SHEET_GAP = 24
SHEET_COLUMNS = 2
SHEET_BACKGROUND = (96, 100, 110, 255)


# ---------------------------------------------------------------- fonts

LATIN_FONT_CANDIDATES = [
    '/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf',
    '/Library/Fonts/Arial Unicode.ttf',
    '/System/Library/Fonts/Supplemental/Arial Bold.ttf',
    'C:/Windows/Fonts/arialbd.ttf',
    'C:/Windows/Fonts/segoeuib.ttf',
]
CJK_FONT_CANDIDATES = [
    '/usr/share/fonts/opentype/noto/NotoSansCJK-Bold.ttc',
    '/System/Library/Fonts/ヒラギノ角ゴシック W6.ttc',
    '/System/Library/Fonts/Hiragino Sans GB.ttc',
    'C:/Windows/Fonts/YuGothB.ttc',
    'C:/Windows/Fonts/meiryob.ttc',
]
FIRST_CJK_CODEPOINT = 0x2E80   # anything from here up needs the Japanese font


class Fonts(NamedTuple):
    latin_path: str
    cjk_path: str
    cjk_index: int   # which face of a .ttc collection to use


def find_fonts():
    parser = argparse.ArgumentParser()
    parser.add_argument('--latin-font')
    parser.add_argument('--cjk-font')
    parser.add_argument('--cjk-index', type=int, default=0)
    args = parser.parse_args()
    latin = args.latin_font or next((p for p in LATIN_FONT_CANDIDATES if os.path.exists(p)), None)
    cjk = args.cjk_font or next((p for p in CJK_FONT_CANDIDATES if os.path.exists(p)), None)
    if not latin or not cjk:
        sys.exit('Could not find fonts. Pass --latin-font and --cjk-font.')
    return Fonts(latin, cjk, args.cjk_index)


FONTS = find_fonts()


def load_font(path, size, collection_index=0):
    """A font at `size` output pixels (loaded at supersampled size)."""
    if path.lower().endswith(('.ttc', '.otc')):
        return ImageFont.truetype(path, size * S, index=collection_index)
    return ImageFont.truetype(path, size * S)


def needs_cjk_font(text):
    return any(ord(char) >= FIRST_CJK_CODEPOINT for char in text)


def font_that_fits(text, max_width):
    """The biggest font, from label_font_max down to label_font_min, in which `text` fits `max_width`."""
    path, index = (FONTS.cjk_path, FONTS.cjk_index) if needs_cjk_font(text) else (FONTS.latin_path, 0)
    for size in range(M.label_font_max, M.label_font_min - 1, -1):
        font = load_font(path, size, index)
        if font.getlength(text) <= max_width * S:
            return font
    return load_font(path, M.label_font_min, index)


# ---------------------------------------------------------------- drawing

class Box(NamedTuple):
    """A rectangle in output pixels."""
    left: float
    top: float
    right: float
    bottom: float

    def grown(self, amount):
        return Box(self.left - amount, self.top - amount, self.right + amount, self.bottom + amount)

    def widened(self, amount):
        """Grown sideways only."""
        return Box(self.left - amount, self.top, self.right + amount, self.bottom)


class Point(NamedTuple):
    x: float
    y: float


def draw_rounded_rect(draw, box, value, corner_radius, origin):
    """A filled rectangle with rounded corners, `origin` being where (0, 0) of `box` sits on the layer."""
    draw.rounded_rectangle(
        [(box.left + origin.x) * S, (box.top + origin.y) * S, (box.right + origin.x) * S, (box.bottom + origin.y) * S],
        radius=corner_radius * S, fill=value)


def draw_rect(draw, box, value, origin):
    """A filled rectangle with square corners."""
    draw.rectangle(
        [(box.left + origin.x) * S, (box.top + origin.y) * S, (box.right + origin.x) * S, (box.bottom + origin.y) * S],
        fill=value)


def keycap_boxes(key):
    """One box per rectangle of the key (two for the L-shaped Enter), shrunk by the cap inset."""
    unit, inset = M.px_per_key_unit, M.keycap_inset
    return [Box(r['x'] * unit + inset, r['y'] * unit + inset,
                (r['x'] + r['w']) * unit - inset, (r['y'] + r['h']) * unit - inset)
            for r in key['rects']]


def seam_box(upper, lower):
    """For an L-shaped key: the strip that joins its two parts, so the corners between them are not rounded."""
    reach = M.keycap_corner_radius * 2
    return Box(max(upper.left, lower.left), upper.bottom - reach, min(upper.right, lower.right), lower.top + reach)


def draw_key(layers, key, origin):
    caps = keycap_boxes(key)
    seams = [seam_box(*caps)] if len(caps) == 2 else []
    outline = M.keycap_outline

    for cap in caps:
        draw_rounded_rect(layers['edge'], cap.grown(outline), COVERED, M.keycap_corner_radius + outline, origin)
    for seam in seams:
        draw_rect(layers['edge'], seam.widened(outline), COVERED, origin)
    for cap in caps:
        draw_rounded_rect(layers['key'], cap, COVERED, M.keycap_corner_radius, origin)
    for seam in seams:
        draw_rect(layers['key'], seam, COVERED, origin)

    narrowest_cap = min(r['w'] for r in key['rects']) * M.px_per_key_unit
    label_font = font_that_fits(key['label'], narrowest_cap - M.label_side_padding)
    center = Point((key['center']['x'] * M.px_per_key_unit + origin.x) * S,
                   (key['center']['y'] * M.px_per_key_unit + origin.y) * S)
    layers['text'].text(center, key['label'], font=label_font, fill=COVERED, anchor='mm')


def draw_header(layers, layout):
    """Number badge (digit cut out of it) and the layout name."""
    r = M.badge_radius
    left = top = M.margin + M.badge_inset
    center = Point((left + r) * S, (top + r) * S)

    layers['accent'].ellipse([left * S, top * S, (left + 2 * r) * S, (top + 2 * r) * S], fill=COVERED)
    layers['accent'].text(center, str(layout['digit']), font=load_font(FONTS.latin_path, M.badge_digit_font),
                          fill=HOLE, anchor='mm')   # a hole, so the panel color shows through as the digit

    title_left = (left + 2 * r + M.title_gap) * S
    layers['text'].text((title_left, center.y), layout['name'], font=load_font(FONTS.latin_path, M.title_font),
                        fill=COVERED, anchor='lm')


def render_mask(layout):
    """The RGBA mask texture for one layout: one channel per entry of LAYER_ORDER."""
    unit = M.px_per_key_unit
    width = layout['unitsWide'] * unit + 2 * M.margin
    height = layout['unitsHigh'] * unit + M.header_height + 2 * M.margin
    layers = {name: Image.new('L', (width * S, height * S), 0) for name in LAYER_ORDER}
    drawers = {name: ImageDraw.Draw(image) for name, image in layers.items()}

    draw_header(drawers, layout)
    keys_origin = Point(M.margin, M.margin + M.header_height)
    for key in layout['keys']:
        draw_key(drawers, key, keys_origin)

    channels = [layers[name].resize((width, height), Image.LANCZOS) for name in LAYER_ORDER]
    return Image.merge('RGBA', channels)


def preview(mask, palette=PREVIEW_PALETTE):
    """What the shader does with a mask: the layers over the panel color, then the rounded-corner cut."""
    width, height = mask.size
    picture = Image.new('RGB', (width, height), palette['panel'])
    for channel, name in enumerate(LAYER_ORDER):
        picture = Image.composite(Image.new('RGB', (width, height), palette[name]), picture, mask.getchannel(channel))

    corners = Image.new('L', (width * S, height * S), 0)
    ImageDraw.Draw(corners).rounded_rectangle([0, 0, width * S - 1, height * S - 1],
                                              radius=M.panel_corner_radius * S, fill=COVERED)
    picture = picture.convert('RGBA')
    picture.putalpha(corners.resize((width, height), Image.LANCZOS))
    return picture


def contact_sheet(pictures):
    thumb_w, thumb_h = int(pictures[0].width * SHEET_SCALE), int(pictures[0].height * SHEET_SCALE)
    rows = (len(pictures) + SHEET_COLUMNS - 1) // SHEET_COLUMNS
    sheet = Image.new('RGBA', (SHEET_COLUMNS * thumb_w + (SHEET_COLUMNS + 1) * SHEET_GAP,
                               rows * thumb_h + (rows + 1) * SHEET_GAP), SHEET_BACKGROUND)
    for i, picture in enumerate(pictures):
        thumb = picture.resize((thumb_w, thumb_h), Image.LANCZOS)
        sheet.alpha_composite(thumb, (SHEET_GAP + (i % SHEET_COLUMNS) * (thumb_w + SHEET_GAP),
                                      SHEET_GAP + (i // SHEET_COLUMNS) * (thumb_h + SHEET_GAP)))
    return sheet


# ---------------------------------------------------------------- main

def load_layouts():
    layouts = [json.load(open(path, encoding='utf-8')) for path in sorted(glob.glob(os.path.join(DATA_DIR, '*.json')))]
    return sorted(layouts, key=lambda layout: layout['digit'])


def main():
    os.makedirs(ART_DIR, exist_ok=True)
    os.makedirs(PREVIEW_DIR, exist_ok=True)
    pictures = []
    for layout in load_layouts():
        mask = render_mask(layout)
        pictures.append(preview(mask))
        path = os.path.join(ART_DIR, 'keyboard_%s.png' % layout['id'])
        mask.save(path)
        print('wrote', os.path.relpath(path, KEYBOARD_DIR), mask.size)
    contact_sheet(pictures).save(os.path.join(PREVIEW_DIR, 'contact_sheet.png'))
    print('wrote Authoring~/out/contact_sheet.png')


if __name__ == '__main__':
    main()
