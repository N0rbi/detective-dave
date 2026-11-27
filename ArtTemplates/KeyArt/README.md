# Key art templates

For every kind of key (wall, gap, walkable, wallgutter...) the game needs only **two pictures**:

1. **Box**: one small 9-sliced picture that is used for every normal key, whatever its width. The game stretches the middle and keeps the border at the same thickness.
2. **Enter**: one picture for the L-shaped Enter key.

Draw on the blank files; use the guides as a reference layer.

## The Box
- Keys are drawn **twice as tall as wide**, and keys come in 7 widths (1 to 2.75 key units, even a 6.25 space bar). The Box stretches to all of them.
- Draw the Box as a tiny keycap: a **border (the rim)** around a **middle**.
  - pixel16: the canvas is **12 x 12 px**, the border is **3 px** on every side.
  - hires64: the canvas is **48 x 48 px**, the border is **12 px** on every side.
- Pink corners never stretch. Blue edges stretch in one direction. The green middle stretches both ways. So keep detail (shine, shadow, rivets) in the corners and the rim; the middle should be a flat colour or a simple gradient.
- The border has the same thickness on every key, also when the key is tall or wide.
- Look at `sheet_pixel16.png`: it shows an example box on a 1, 2.25 and 6.25 key.

## The Enter
- One picture over the whole L-shaped Enter: **24 x 64 px** (pixel16) or **96 x 256 px** (hires64). It is stretched to the key, not sliced.
- The notch at the bottom left stays transparent (see the guide).
- Draw it with the same look as the Box.

## Folders
- `pixel16/` and `hires64/` each hold `blank/` (paint here), `guides/` (outlines only, a reference layer) and `template/` (guides on a grey body, for looking at): files `key_box_9slice.png` and `key_enter.png`.
- `sheet_pixel16.png`, `sheet_hires64.png`: overview, with the box stretched on three keys.
- `legacy_shapes/`: the old templates, one picture per key size. Only needed for old art.

## Names
Name your finished pictures after the key kind: `wall_box.png`, `wall_enter.png`, `gap_box.png`, `gap_enter.png`... The names are for people; the team assigns them in the Key Art inspector.

## Handing over
Put the finished PNGs in `Assets/Staging`. The team moves them and assigns Box and Enter per key kind.

## Unity import settings
Texture Type Sprite (2D and UI), Sprite Mode Single, Pixels Per Unit 16 (pixel16) or 64 (hires64), Filter Mode Point (no filter) for pixel art, Compression None, **Mesh Type Full Rect**.
For the Box also set the 9-slice border: in the Sprite Editor set Border L, R, T, B to **3** (pixel16) or **12** (hires64), or select the Key Art, put the Box on the style and press **Set 9-Slice Border** (it asks for the number of border pixels).
In the Key Art, `Border Thickness In Key Units` is 0.1875 (3/16 of a key width), which draws the 3 px border of pixel16 art at its own size. Change it to make all rims thicker or thinner.

## Regenerating
`python3 make_templates.py` recreates everything (needs Pillow).
