# Manual checks: making and playing a level

Run these in the Editor after changes to the level tools. Each line says what to do and what you should see.

## Setup
- [ ] Window > Detective Dave > Levels opens and docks. The list shows `Level 01`; "Keyboard" shows UK.
- [ ] Project Settings > Detective Dave > Levels shows the folders, template, palette, game scene, Start / Goal kinds.

## New Level
- [ ] Press New Level: `Assets/Levels/Level 02.prefab` appears, is added at the end of the list, and opens in Prefab Mode.
- [ ] Every key of the UK keyboard is drawn as an outline with a grey dot in the middle.
- [ ] The Tile Palette window is open on "Level Tiles" with the "Key" brush selected and Shape as the paint target.

## Paint
- [ ] Pick Wall, click a key: the whole key fills (the ISO Enter fills both parts). The hovered key is outlined while moving.
- [ ] Click between keys: nothing is painted. Erase (shift-click or the eraser) clears the whole key.
- [ ] Undo / Redo restore one key per click.
- [ ] Checks in the Levels window update as you paint (e.g. "No Start key on the UK keyboard").
- [ ] Switch "Keyboard" to Japanese: outlines change; painting still fills whole keys of that layout.
- [ ] Paint half a key with the normal brush: the "partly painted or mixed" check appears; "Fill with centre kind" fixes it.

## Background
- [ ] Drag a sprite from Assets/Backgrounds onto Background: it shows behind the keyboard in Prefab Mode.

## Play This Level
- [ ] Press F5 (or Play This Level): the game scene starts on this level, on the preview keyboard, with no layout picker.
  The Console shows `[Play This Level] Before scene load: layout 'uk' locked` and `Scene 'Game' loaded: handing 'Level 02' to the Level Loader`.
- [ ] No outlines, no debug overlay, no authoring tilemap are drawn in the Game view; keys show the Key Art.
- [ ] A level with nothing painted shows the red banner "Level 'Level 02': shape is empty".
- [ ] While playing, paint a key in Prefab Mode (Auto Save on): the key changes in the Game view within a frame; the Console logs `[Live reload]`.
  Keys stay responsive (the keyboard is not rebuilt; no exception from KeyboardKeySource.Begin).
- [ ] Edit a sprite or tint in Default Key Art while playing: keys redraw.
- [ ] Stop, then press the normal Play button in KeyboardSelect: the picker shows, and after picking, Game plays Level 01 (first of the sequence).
- [ ] Rebind F5 in Edit > Shortcuts > Detective Dave > Play This Level; the new key works.
