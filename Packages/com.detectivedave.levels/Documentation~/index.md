# Making levels (designer and artist guide)

No code needed. A level is one **Level Definition** asset: its name, its background, how each kind of key looks, and its
shape, which you paint with tilemaps.

## Words

- **Tile Type**: a kind of key: Walkable, Gap, Wall, WallGutter, Start, Goal, or any you add. Lives in `Assets/Game/Level/Types`.
- **Terrain Tile**: a tile you paint with. Each one carries a Tile Type. Lives in `Assets/Game/Level/Data`.
- **Tile Map Group**: a Grid with tilemaps on it. One group is the shape of one level.
- **Level Definition**: the level asset. Lives in `Assets/Game/Level/Levels`.
- **Key shape**: the size of a key in key units. A letter key is 1 x 1, Tab 1.5 x 1, left Shift 2.25 x 1. The ISO Enter is
  an L made of two rectangles inside a 1.5 x 2 box.

## Make a new level

1. Open a scene with the keyboard in it (`Assets/Scenes/KeyboardLevel.unity`), or a prefab of your own.
2. Select the keyboard object (**Keyboard Grid**) and choose **GameObject > Detective Dave > Level > Tile Map Group**.
   It is created as a child, at the origin, with a 0.25 cell size, so each key is 4 x 4 tiles. A white outline with one
   line per key row shows where the keyboard is.
3. In the new group's inspector press **Create Level…** and save the asset in `Assets/Game/Level/Levels`. The new level
   already uses the project's key art (its **Style Template**), so it looks right straight away.
4. Under **Layers** press **Add Tilemap Per Tile Type**. You get one tilemap per type (Goal, Start, Wall, ...), each already
   set to its type. The list is the priority: the top layer wins where two layers overlap. Drag rows to reorder.
   (Prefer painting with Terrain Tiles on one tilemap? Keep one layer and leave its **Type** empty.)
5. Open **Window > 2D > Tile Palette**, pick a tilemap as the active target and paint. Paint the *middle* of each key: that is
   what the key becomes. Keys with nothing painted are hidden in the game.
6. Press **Bake Into '…'** on the group. You don't have to: pressing Play or saving the scene bakes too. The **Preview** at
   the bottom of the group shows what will be baked, coloured by type.
7. Select the level asset and fill in **Display Name** and **Background**. Read the **Checks** box at the top: every warning
   says what to do.
8. To play it, select **Keyboard Grid** and drag the level into **Tile Map Keyboard Mapper > Level**. That is the only wiring.

You can have as many groups as you like, in the same scene or in different scenes and prefabs, each baking into its own
level. Two groups must not bake into the same level: the Checks box will tell you if they do.

## Make the keys look different in one level

Open the level, go to **Key Art**, press **Add Style** (or **Add Styles For All Tile Types**), pick the type, and assign only
the sprites you want to change. Anything you leave empty comes from the Style Template. The strip of small squares under each
style shows how every key shape will look in this level; red means no sprite anywhere, so that key shape is drawn as a flat
square.

To restyle every level at once, edit the template level (`DefaultKeyArt`).

## Add a new kind of key

1. **Assets > Create > Detective Dave > Level > Tile Type**. Name it, choose an **Editor Colour** (used in previews only) and
   tick **Walkable** if players can stand on it.
2. Either create a **Terrain Tile** for it (**Assets > Create > Detective Dave > Level > Terrain Tile**, set its Type and a
   sprite to see while painting) or press **Add Tilemap Per Tile Type** on your group to get a layer for it.
3. Give it a style in the template level (or in a single level) with **Add Styles For All Tile Types**, then add its sprites.

## For artists: key sprites

- One sprite per key shape per type. The shapes the shipped keyboards use: 1, 1.25, 1.5, 1.75, 2, 2.25 and 2.75 units wide,
  1 high, plus the whole L-shaped Enter (1.5 x 2). **Add Standard Key Shapes** creates these rows for you.
- Sprites are stretched to fill the key and multiplied by the style's **Tint**. Draw in grayscale to recolour with the tint, or
  in colour with a white tint.
- Name files `<anything>_<width>x<height>.png`, with `_` for the decimal point (`wall_1_25x1.png`), and
  `<anything>_enter_1_5x2.png` for the whole Enter. Then **Fill From Folder…** on a style assigns a whole folder in one go.
- Without a whole-key Enter sprite the Enter is drawn from its two rectangles (1.5 x 1 on top, 1.25 x 1 below).

## When something looks wrong

| You see | Do this |
|---|---|
| "No shape yet" | The level was never baked. Pick it in a group's **Level** field and press Bake. |
| "Out of date" | The tilemaps changed after the last bake. Press Bake (or Play / save). |
| "… tile(s) on 'X' have no type" | Those tiles are plain tiles on an untyped layer. Set the layer's Type or paint with Terrain Tiles. |
| "Another group also bakes into …" | Two groups share a level. Give one of them a new level (**Create Level…**). |
| "… has no sprite for: …" | Assign the missing sprites (or **Fill From Folder…**). Until then those keys are flat squares. |
| "is under a different Grid" | All tilemaps of a group must be children of the same Grid. |
| All keys hidden in Play | The level's shape is empty: bake it, and check the group is a child of the keyboard object. |

**Tools > Detective Dave > Levels > Validate All Levels** runs every check on every level and lists the problems in the Console;
click a message to select the asset.
