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
- [ ] Every move leaves three thin wind lines that stay attached to the player (never ahead of it), parallel to the move, with the top line at the shoulders and the bottom line at the upper legs (not at the feet), at slightly different heights and thicknesses; they grow out of the start as the player moves, then shorten and fade within a quarter of a second after it arrives. A wall bump leaves short lines, a slip leaves short lines behind the player, a step onto a gap or the goal leaves full-length lines. Pressing the key the player stands on, or any key before the game started, shows none. Changing the list in the "Dash gust" section of the player settings changes them.
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
- [ ] Spaces and line breaks are never needed: finish a word and the next one is ready. Before the split the accept answer waits in green and the decline answer in red; after it the other answer fades out but its space stays.
- [ ] A case whose accept answer is the start of the decline answer (the `Kidnapped` case, `...` and `..............`): typing a dot takes the long answer, Enter or Space takes the short one, and both finish the letter. The case inspector warns about the overlap.
- [ ] Declining returns to the desk; accepting wipes into the level; reaching the goal returns to the desk with the case solved.
- [ ] F5 with the sample desk selected plays the overworld; F5 on a level still plays the level.

## Overworld briefing (round 2)

- Run Set Up Overworld twice: the second dialog says nothing needed changing.
- Fonts: the client's message (the request) and the investigation findings use JMH Typewriter (`Request Font`, `Findings Font`); a case with `Description Font Override` set (the Kidnapped case: `1942_report`) draws its description in that font on every screen, while its findings stay JMH; the grey waiting text uses JMH Typewriter (`Placeholder Font`); the letters you type use Mom's Typewriter (`Typed Font`), so a typed letter changes font where it replaces its grey placeholder. Letters stay on the same spots: each letter sits in a fixed cell, so a typed letter lands exactly on its grey placeholder, the pitch is even across the whole line, and no typed line runs past the right edge of the paper. If the typed and grey letters sit at different heights, adjust `Typed Font Baseline Offset Share` / `Placeholder Font Baseline Offset Share`. A case without a level shows its notice in the 1942 report font. If Console says a font has no letter width, assign a real font on `Overworld Settings.config.asset` (section Fonts); run `Set Up Overworld` if the Font fields are empty. If the findings still show the 1942 font, point `Findings Font` at `jmh_typewriter.ttf`.
- Case colours: on a case asset open `Look`, tick `Use` on Description and pick red; the description is red on the letter, in the report and in the solved review, while the findings keep the colour of `Findings Text Color`. Untick it: the colour returns to the settings colour. Tick Typed and Placeholder, play the response: typed letters and grey letters use the case colours. Tick Accept and Decline pending: the answers show those colours until the player chooses, and the unchosen one still fades out. `Look preview` shows a sample per colour in the right font on the paper colour; pick a colour close to `Paper Reference Color` and the preview shows a contrast warning. Another case without overrides is unchanged.
- Text fit: open a case with long findings (after the level and from a solved case). Nothing runs past the bottom or the right edge of either paper, the findings are a little smaller than the description, and the size does not jump while the findings type in. If a case is too long even at the smallest size, Console shows one warning naming that case.
- Dwell on a case: the description types at a steady speed with soft clicks (no click faster than the minimum interval, also while skipping); a key press speeds it up; the letter paper appears only afterwards and typing before that does nothing.
- Both papers are slightly rotated on the desk; untyped text is grey, typed text black; each valid key clicks with a slightly different pitch, volume and clip every time, Backspace plays the softer clip. If anything is silent the Console says why (empty click list, no Audio Listener, volume 0).
- Finish a level: back in the overworld the left paper already shows the description and the findings type in below it (steady speed, soft clicks, a key press speeds it up). The right paper appears only when the findings are done and typing before that does nothing.
- Then type the response on the right paper: only the current word (or sentence or character, per `responseUnit`) is visible in grey, typed letters turn black one by one, and the next words are not shown. Backspace turns a letter grey again. Only when the response is finished does the folder show the solved picture.
- Walk onto the solved case and wait for the dwell: the left paper shows the description and the findings, the right paper the response, all in black; nothing can be typed, no typewriter sound plays, and any key slides the papers away and returns to the desk. Progress is unchanged.
- The magnifier is on Left Shift again and twice as big; the detective shows a gust when dashing, the magnifier does not.

## Overworld desk look and hints
- Run Set Up Overworld: it fills Shift Keycap Sprite; the second run says nothing needed changing.
- Play the overworld: Left Shift and Right Shift show a keycap picture; the magnifier stands on Left Shift.
- A dashed line runs along the keys from the magnifier to the nearest open case, the dashes slowly flow towards the case; it never covers the magnifier or the guards' sorting.
- Case files bob and tilt a little, each at its own rhythm (none move in step). Dwelling on a file still works at the same keys.
- Route Hint Visibility UntilCaseReached: move one key along the line, the line shortens from the magnifier; reach the case, the line disappears. UntilFirstMove: the line disappears at the first move.
- Come back from a solved case: the line shows again towards the next open case; with no open case there is no line. It is hidden while a briefing is open.
- Turn off Route Hint Enabled and Case Float Enabled: no line, the files stay still.

## Desk design view
- Open Assets/Overworld/Sample Desk.prefab: the Scene view shows the desk surface, the key outlines of the preview keyboard, a blue START on Left Shift and the sample case's keys filled in its colour with the client name.
- Move, rotate and scale the case file: the covered keys follow live.
- Drag a second case file over the first: the shared keys turn red and the inspector lists them.
- Move a case file off the keyboard: its label says it covers no key.
- Move a case file over Left Shift: START turns red and says it is covered.
- Change the Preview keyboard dropdown: the outlines and covered keys change to that layout.
- Remove the case from a marker: its label says No case assigned.
- Free keys (no case on them) are filled light grey with a dark outline everywhere on the keyboard, so you can see where a case file can go. A legend in the bottom-left corner of the Scene view explains the colours. Zoom in: the free keys show their labels.
- Desk inspector, Cases section: every case in the project is listed with its colour and whether it is on the desk. Press Add to desk on a case that is not on it: a case file appears on the first free keys, selected, and Undo removes it. Add empty marker adds one without a case.
- Drag a case asset from the Project window onto the Scene view while the desk is open: a case file appears where you drop it. A case that is already on the desk is ignored.
- Select the desk prefab in the Project window without opening it: the Cases section says to open the desk first and the buttons are disabled.

## Overworld desk designer path
- Overworld Settings has a Desk field (tooltip: the desk the game starts on). Set Up Overworld fills it only when empty. Shift+F5 with nothing selected plays that desk; F5 with a custom desk open or selected plays that desk instead.
- Accept a case that has no level: the briefing shows a short grey line "This case has no level yet", the case stays on the desk and nothing starts. Declining such a case is normal.
- The desk inspector and the marker label in the Scene view say "No level yet" for such a case.
- Desk inspector Preview as: Open, Locked, Solved and By progress file change how the case files look in the Scene view. Leave Prefab Mode and check the prefab shows no changes (not marked as modified).
- Desk inspector Case chain: make two cases require each other, require a case that is not on the desk, make every case locked, and give two cases the same id: each is listed.
- Case inspector: an old case keeps its asset name as the id; a new case gets an id like case-1a2b3c4d; renaming the asset does not change the id; two cases with the same id warn.
- Case Marker menu: in Prefab Mode with nothing selected the marker lands under the open desk; in a scene with one desk it lands under it; with no desk a dialog explains why nothing happened.
- Move or scale the desk root: the inspector and the Scene view warn; Reset root transform puts it back at position 0 and scale 1.

## Guards: placing them in the Scene view
- Levels window with a level selected: Add Guard opens the level in Prefab Mode (if needed), adds "Guard 1" and enters Patrol Edit mode: the Scene view gets an orange wash and the banner "PATROL EDIT: Guard 1: click a key to set START". The Inspector has an Edit Patrol button at the top and no key dropdowns.
- In the mode: clicks never select anything. First click sets START, the next clicks set END. Start|End in the Scene panel overrides the target. Shift+click sets END. Right-click, Esc or Exit leaves (wash and banner disappear).
- In the mode, select another guard in the Hierarchy: the banner shows that guard and clicks edit it. Select a non-guard object outside the level: the mode ends. Selecting the level or its children keeps it.
- Enter Play mode or close Prefab Mode: the mode is off afterwards.
- Click a key: it turns green with START and the wait time. Shift+click another key: it turns orange with END. A white line with arrows shows the path in between. Nothing else gets selected by the clicks. Undo moves the key back.
- Change the Keyboard dropdown in the Levels window: the same guard stays on the same physical key, and the labels show that keyboard's letters.
- Click a wall, gap or gutter key as START or END: a red warning appears in the Scene view, the Inspector and the Levels window checks. Surround the START with gaps: "no way from START to END" and a red dotted line.
- A key that exists on UK but not on another layout (for example UK's extra keys on JIS): the Levels window checks name the layouts that break the guard.
- Select something other than a guard: every guard in the level shows a small green dot on its START key.
- Play This Level: the guard patrols between START and END along the line shown.
- Step next to a guard while it is between two keys (not only when it reaches a key): it switches to the chase animation at once, finishes the step smoothly (turning back to the key it left when it had walked less than half of it) and then follows the shortest key path to you. No jump in its position. Step onto either key it is between: you are caught. Step onto a gutter key mid-chase: it finishes its step, then goes back to patrolling from where it stands.

## Level lighting
- Run Window > Detective Dave > Set Up Lighting (or Set Up Player). Game.unity: the object with LevelLoader has a LevelLightingView, and every Keyboard Grid View there has Cell Material = KeyboardCellLit. Assets/Game/Keyboard/Art/KeyboardCellLit.mat exists.
- Play Game: the keys are lit by the 2D lights (move or dim a light in the scene: the keys change). A level with the default Lighting looks as before the change.
- Level prefab > Lighting: set Intensity 0.5 and a tint: play, all lights follow; stop, the lights in the scene are back as authored.
- Turn Flicker on with no pattern: the checks show an error with Add a Blink pattern. Click the fix: a Blink pattern with a timeline appears.
- Timeline: click empty space adds a point, drag moves it (snapping to the grid when Snap To Grid is on), right-click a point removes it, select a point and press Delete removes it. Start From replaces the points with a preset and sets the duration.
- Preview on a pattern: the green playhead runs over the timeline, Final Look dims, and the lights of the open scene dim. Preview Like In Game: random waits, random patterns. Stop, enter Play Mode or save the scene: the lights are back to their own values.
- Two patterns, one with Weight 0: only the other is ever picked. Weights 3 and 1: the first is picked about three times as often.
- Wait At Least 9 and Wait At Most 4: the checks show an error with Swap the waits.
- Play a level with flicker: the lights flicker now and then at random intervals; load the next level: the flicker restarts with that level's settings.

## Reactive music
- Assets/Staging no longer holds JazzLoop.wav or the Jazz - Piano files; they are in Assets/Audio/Music and Assets/Audio/Music/Piano.
- Run Window > Detective Dave > Set Up Music. Assets/Audio/Music/Music Settings.config.asset exists with the Loop Clip and 12 Piano Clips. JazzLoop.wav imports as Streaming, PCM. The object with PlayerController in Game.unity and the object with OverworldController in Overworld.unity each have a MusicLink with the settings assigned.
- Play the Overworld (Shift+F5): the loop fades in after about 2 seconds and plays all the time. Move the magnifying glass: a piano note plays on every move and never the same note twice in a row. Press a key that is not next to the glass: a quieter note (half volume) plays.
- Open a case: the loop keeps playing without a restart while the briefing is open. Press keys during the briefing: no piano notes.
- Start the level from the overworld: the loop does not restart on the scene change, and there is only one Music Director in the Hierarchy (under DontDestroyOnLoad). Walk the player: a note per step. Walk into a wall and press the key you stand on: half volume notes. While the player falls or the spiral plays, key presses make no sound.
- Play a level directly with F5 (Play This Level): the loop plays and the notes react.
- Set Min Seconds Between Piano Notes to 0.3 and press keys quickly: notes are skipped to keep 0.3 seconds apart. Set Piano Clips to one clip: it plays every time. Empty Piano Clips: a warning in the Console and no notes, the loop still plays.

