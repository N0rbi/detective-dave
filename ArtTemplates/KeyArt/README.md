# Key art templates

Starter canvases for the key pictures. Draw on the blank files; use the guides as a reference layer.

## What to know first
- Keys in the game are **twice as tall as they are wide** (per key unit). So a normal letter key is 16 x 32 px, not square. Draw tall keys.
- Draw all the way to the edge of the canvas. The game already leaves the little gap between keys, so the picture should fill its canvas.
- If a picture is not exactly the right size the game stretches it to fit, so details get squashed. Keep the sizes below.
- Two sets: **pixel16** (16 px per key width, matches the existing pixel art) and **hires64** (4 times bigger, for painting).

## Shapes (pixel16 / hires64)
| File | pixel16 | hires64 | Used for |
|---|---|---|---|
| key_1x1 | 16 x 32 | 64 x 128 | letters, digits |
| key_1_25x1 | 20 x 32 | 80 x 128 | a few edge keys |
| key_1_5x1 | 24 x 32 | 96 x 128 | Tab, Ctrl-size keys |
| key_1_75x1 | 28 x 32 | 112 x 128 | Caps Lock |
| key_2x1 | 32 x 32 | 128 x 128 | Backspace |
| key_2_25x1 | 36 x 32 | 144 x 128 | Shift, ANSI Enter |
| key_2_75x1 | 44 x 32 | 176 x 128 | right Shift |
| key_enter_1_5x2 | 24 x 64 | 96 x 256 | the L-shaped Enter |

The L-shaped Enter has a notch at the bottom left: leave it transparent (see the guide).

## Folders
- `blank/` empty canvases of the right size. Paint on these.
- `guides/` outlines only: pink = canvas edge, blue = keep important detail inside, cross = centre. Use as a reference layer.
- `template/` the guides on a grey key body, for looking at.
- `sheet_pixel16.png`, `sheet_hires64.png` overview of all shapes.

## Names
Each finished picture is named after the tile it is for, then the shape: replace `key` with `wall`, `gap`, `walkable` or `wallgutter`. Examples: `wall_1x1.png`, `gap_2_25x1.png`, `wall_enter_1_5x2.png`. The `_` stands for the decimal point. The game reads the size from the name.

## Handing over
Put the finished PNGs in `Assets/Staging`. The team moves them to the Key Art folder and assigns them (Key Art inspector, Fill From Folder).

## Unity import settings (what the existing art uses)
Texture Type Sprite (2D and UI), Sprite Mode Single, Pixels Per Unit 16 (pixel16) or 64 (hires64), Filter Mode Point (no filter) for pixel art, Compression None, Mesh Type Full Rect. Sprites are stretched by size, not 9-sliced, so borders are not needed.

## Regenerating
`python3 make_templates.py` recreates everything (needs Pillow).
