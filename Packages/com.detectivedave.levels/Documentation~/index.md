# Making levels (designer and artist guide)

A level is **one file** in `Assets/Levels` (`Level 01.prefab`, ...). It holds the level's shape, its key art, its background
and its name. Nothing has to be baked, wired into a scene or saved by hand.

## Where things live

| Folder | What is in it |
|---|---|
| `Assets/Levels` | One prefab per level. `Templates/Level Template.prefab` is what new levels start from: edit it to change every new level. |
| `Assets/Tiles` | One file per kind of key (`Walkable`, `Gap`, `Wall`, `WallGutter`) and the **Level Tiles** palette. The Start key (Left or Right Shift) is a setting on the Level, not a tile; the other Shift is the End. |
| `Assets/Key Art` | `Default Key Art` and one sprite folder per kind |

## Make a level

1. Open **Window > Detective Dave > Levels** (once; dock it) and press **New Level**. `Assets/Levels/Level 0N.prefab` opens
   for editing with the keyboard drawn as key outlines and the **Level Tiles** palette ready.
2. **Paint**: pick a kind of key in the palette and click keys in the Scene view. Each key takes the kind you clicked it with.
   The level's **Checks** (in the Levels window and in the level's inspector) update while you paint; most have a button that fixes them.
3. **Background**: drag a sprite from the Project window onto **Background**.
4. **Play This Level** (or F5) starts the game on your level.

Rename a level with F2 in the Project window: the name shown in game follows the file name, unless you type a **Display Name**.

## The level's inspector

Select the level root (in Prefab Mode or in the Project window). **Checks** lists what is wrong and how to fix it, for example:

| Check | What to do |
|---|---|
| No Key Art | Press **Use 'Default Key Art'**, or pick another Key Art. |
| No Shape tilemap | Press the button: it uses the tilemap under the level, or creates one. |
| ... cells hold tiles that are not a kind of key | Press **Erase them** and paint with the Level Tiles palette. |
| ... a kind of key that was deleted | Press **Erase them** and paint those keys again. |
| 'X' is painted but the Key Art has no style for it | Press **Open Key Art** and add a style (see below). |
| 'X' is not in the Level Tiles palette | Press **Add to Level Tiles**. |
| This level is in another folder | Press **Move to Assets/Levels**. |

## Lighting

The **Lighting** section of the Level inspector sets how the 2D lights look in this level: **Intensity** (a multiple of how each light is set up in the scene), **Color Tint**, and an optional **Flicker**. A flicker waits a random time between **Wait At Least** and **Wait At Most**, then plays one of the level's **flicker patterns** picked at random (by **Weight**). Draw a pattern on its timeline: click to add a point, drag to move, right-click to remove, **Start From** to begin from a preset. **Preview** and **Preview Like In Game** play it in the editor. The checks tell you when flicker has no patterns, the waits are reversed, a pattern has no duration or only weight 0.

## Add a new kind of key

1. In `Assets/Tiles`, **Assets > Create > Detective Dave > Tile Type**. Name it, give it a **Sprite** and **Color** (what you
   see while painting), an **Editor Colour** (previews), and tick **Walkable** if players can stand on it.
2. Any level's Checks then offer **Add to Level Tiles**, so it appears in the palette.
3. Open `Assets/Key Art/Default Key Art`, press **Add Styles For All Tile Types**, and give the new style its sprites.

## Change how keys look

Edit `Assets/Key Art/Default Key Art` and every level that uses it follows. To give one level its own look, duplicate the Key
Art (Ctrl+D), change the copy, and pick the copy in that level's **Key Art** field.

In the Key Art inspector, each kind has a strip of small squares: every key shape as it will look in game; red means no sprite.

## For artists: key sprites

- One sprite per key shape per kind. The shapes the shipped keyboards use: 1, 1.25, 1.5, 1.75, 2, 2.25 and 2.75 units wide,
  1 high, plus the whole L-shaped Enter (1.5 x 2). **Add Standard Key Shapes** creates these rows.
- Sprites are stretched to fill the key and multiplied by the style's **Tint**. Draw in grayscale to recolour with the tint.
- Name files `<anything>_<width>x<height>.png` with `_` for the decimal point (`wall_1_25x1.png`), and
  `<anything>_enter_1_5x2.png` for the whole Enter. **Fill From Folder…** on a style then assigns a whole folder at once.
- Without a whole-key Enter sprite the Enter is drawn from its two rectangles (1.5 x 1 on top, 1.25 x 1 below).
