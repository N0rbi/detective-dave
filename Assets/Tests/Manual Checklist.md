# Manual checks: making and playing a level

Run these in the Editor after changes to the level tools. Each line says what to do and what you should see.

## Setup
- [ ] Window > Detective Dave > Levels opens and docks. The list shows `Level 01`; "Keyboard" shows UK.
- [ ] Project Settings > Detective Dave > Levels shows the folders, template, palette, game scene.

## New Level
- [ ] Press New Level: `Assets/Levels/Level 02.prefab` appears, is added at the end of the list, and opens in Prefab Mode.
- [ ] Every key of the UK keyboard is drawn as an outline with a grey dot in the middle.
- [ ] The Tile Palette window is open on "Level Tiles" with the "Key" brush selected and Shape as the paint target.

## Paint
- [ ] Pick Wall, click a key: the whole key fills (the ISO Enter fills both parts). The hovered key is outlined while moving.
- [ ] Click between keys: nothing is painted. Erase (shift-click or the eraser) clears the whole key.
- [ ] Undo / Redo restore one key per click.
- [ ] Checks in the Levels window update as you paint (e.g. a Gap painted on a Shift key is warned).
- [ ] Switch "Keyboard" to Japanese: outlines change; painting still fills whole keys of that layout.
- [ ] Paint half a key with the normal brush: the "partly painted or mixed" check appears; "Fill with centre kind" fixes it.

## Background
- [ ] Drag a sprite from the Project window onto Background: it shows behind the keyboard in Prefab Mode.

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

## Player
Play with F5 on a level with a Gap and a WallGutter key between the two Shifts. The player is wired automatically.
- [ ] The player is shown on the start Shift. Nothing works before that Shift is pressed: other keys do nothing.
- [ ] Press the start Shift: the game starts.
- [ ] Step to a neighbouring walkable key: the three walk frames play as a yoyo (1, 2, 3, 2, 1), the player ends on the key, no pop or stretch.
- [ ] Press a key that is not a neighbour: the player slips backwards a little (tilted, away from the way it faces), then drops in from above onto the start Shift.
- [ ] Moving left shows the player flipped; moving right unflipped; the facing persists while standing. The first facing is towards the End.
- [ ] Step onto a Wall key (or an empty key): the player leans towards it, squashes face-first (about 0.6 scale), then slides back to the centre of the key it was on. Nothing resets, and the keys work again straight away.
- [ ] Every move leaves three thin wind lines that stay attached to the player (never ahead of it), parallel to the move, at slightly different heights and thicknesses; they grow out of the start as the player moves, then shorten and fade within a quarter of a second after it arrives. A wall bump leaves short lines, a slip leaves short lines behind the player, a step onto a gap or the goal leaves full-length lines. Pressing the key the player stands on, or any key before the game started, shows none. Changing the list in the "Dash gust" section of the player settings changes them.
- [ ] Standing still on any key (also on the start Shift before the game starts, and on a gutter), the player breathes: a slow, gentle up and down with the feet fixed. It stops while moving and never leaves the player stretched.
- [ ] Step onto a Gap key: the player walks onto it, fades out (about 0.8 s), then drops in from above onto the start Shift.
- [ ] Step onto the End Shift: the star spins behind the player and the evidence (folder with magnifying glass) pops up above the player's head. Keys are ignored from now on until the next level has started.
- [ ] At the goal the evidence pops up above the player's head, then the camera zooms in on the player holding it (the evidence stays in the hand and grows a little; it does not fly to the screen centre).
- [ ] After the zoom a chunky spiral of dark strips winds from the outer edge of the screen inwards like a snail house and ends on the player. Then the next level loads (the first again after the last; the same level when started with Play This Level).
- [ ] Every level start, including the very first one after F5: the screen starts covered and zoomed on the player at the start key, the spiral unwinds outwards while the camera zooms back out, and keys do nothing until it is done. Then press the Start Shift: the game plays.
- [ ] The keyboard does not change size or jump when the zoom ends.
- [ ] Stop Play Mode in the middle of the spiral and play again: nothing stays covered.
- [ ] Step onto a WallGutter key: the player stands on it and the player switches to the arms-up climb pictures, which advance one picture per move while on or leaving the gutter. The climbing player is drawn half a player height lower, so the raised arms stay inside the key (setting Climb Drop In Heights). Stepping back onto a normal key switches to the walk pictures again at normal height.
- [ ] The drain grate lies on the middle of the start Shift (under the player) and the folder icon on the End Shift. Both follow the Start key setting and disappear when no level is loaded.
- [ ] Step onto the End Shift: a yellow star grows behind the player and keeps turning slowly. Replaying or changing the level hides it again.
- [ ] Change the level's Start key: start and goal flip (player shown on the other Shift; the other is the End).
- [ ] Paint a Gap on a Shift key: it is ignored in play and Level Checks warns, with a "Make it Floor" fix.
- [ ] Change a value in `Assets/Player/Player Movement Settings.config.asset` (e.g. Step Seconds): the change takes effect (check that the field is read live; otherwise on the next Play).

## Overworld
- [ ] Set Up Overworld runs without errors; Assets/Scenes/Overworld.unity exists and is in the build list.
- [ ] Shift+F5 shows the desk and the magnifier; the ring fills on the sample case and resets when leaving.
- [ ] After 2 s the desk slides left, the description types in, the grey paper shows the letter.
- [ ] Only valid keys tint and click; Backspace removes a letter; the wrong path stays grey after the split.
- [ ] Declining returns to the desk; accepting wipes into the level; reaching the goal returns to the desk with the case solved.
- [ ] F5 with the sample desk selected plays the overworld; F5 on a level still plays the level.

## Overworld briefing (round 2)

- Run Set Up Overworld twice: the second dialog says nothing needed changing.
- Dwell on a case: the description types at a steady speed with soft clicks; a key press speeds it up; the letter paper appears only afterwards and typing before that does nothing.
- Both papers are slightly rotated on the desk; untyped text is grey, typed text black; each valid key clicks, Backspace plays the softer clip.
- Finish a level: back in the overworld the findings type on the paper. Only the current word (or sentence or character, per `findingsUnit`) is visible in grey, typed letters turn black one by one, and the next words are not shown. Backspace turns a letter grey again. When finished the folder shows the solved picture.
- Walk onto the solved case and wait for the dwell: the whole findings are shown in black, nothing can be typed, no typewriter sound plays, and any key slides the paper away and returns to the desk. Progress is unchanged.
- The magnifier is on Left Shift again and twice as big; the detective shows a gust when dashing, the magnifier does not.
