# Quirks of the original, and what the port did with them

The original is the VB.NET game in `src/` (Soulslike Game Jam #3, August 2026). The rule for ports (the user's, as for Handies for Cash!, November 5, 2026): **fix quirks as we go and log them here**, keep the message voice and the deliberate design. Each entry says what the original did and what the port does.

## Fixed

1. **Improve Health did nothing.** The checkpoint offered "Improve Health" once you had the cruor, but its handler was never wired into the verb table, so choosing it changed nothing. The port does what the unused handler says: it spends a tenth of your maximum health in cruor (10 at the start) and adds 10 maximum health.
2. **Improve Attack said "gains 1 defend".** It raised attack correctly; the message was copied from Improve Defend. Now "gains 1 attack".
3. **Slimes kept hitting a corpse.** When several slimes struck in one turn and the first one killed you, the rest kept attacking. Each further "kill" ran the death handler again, which replaced your gravestone (holding your cruor) with a new one holding the 0 cruor you now had, so the cruor was lost for good. The port stops the slimes' turn when you die: one death, one gravestone with everything you carried.
4. **Stamina messages reported the requested change, not the real one.** Resting at 9/10 said "gains 5 stamina". The port says what actually changed, and nothing at all when nothing changed (resting at full stamina just "rests").
5. **An empty name.** The original had a `DEFAULT_NAME` of "Olen Kyrpa" that nothing used, so an empty name gave an empty name. The port uses it (as the Handies for Cash! port does).
6. **The DEBUG start** (100 cruor) is gone; the port starts with the release value, 0.

## Added

- **Saving.** The original never saved. The port saves to the browser's local storage after every choice and resumes straight into play, mid-fight included.
- **A Credits screen** on the main menu (the font's CC BY credit, the jam, the sponsor, the port).
- **Phones:** a portrait layout (36 by 54) when the window is tall, and tapping a choice picks it.
- **Colour:** the slimes you face are listed in magenta, and damage you take is red. The words are the original's.

## Removed

- **Quit** on the main menu (the Blazor build hid it too).
- **The Characters, Ground and Inventory menus.** The framework had them, but nothing in this metaphor ever put a character other than slimes (which start combat) or an item anywhere, so they were never available.
- **The cactus** ("feels a prick" and dies): defined in the code, never placed in the world.
- The Windows, macOS and Linux (Spectre console) builds are not part of the port. The VB source stays in `src/` for now.

## Kept on purpose

- No ending: you fight slimes, die, respawn, and get stronger at the checkpoint. "Losing is fun." (the author, in the page's comments).
- A new 4 by 4 maze each time you embark, every room called "Maze Room"; the Blue Room's door going OUT leads to a random room with a door going IN. Dead ends get more slimes: 1d4 for one door, 1d3 for two, 1d2 for three, one for four (the entrance's IN door counts).
- Slimes show what they will do next as the original's raw posture names (`FAST_ATTACK`, `STRONG_ATTACK`, `PARRY`, `DODGE`, `REST`), picked at random each turn. Fast attacks are halved by a parry and miss a dodge; strong attacks miss a parry and are halved by a dodge; any attack does half again on someone resting. Halves round as VB's `CInt` does (to the even neighbour: 37.5 is 38, 12.5 is 12).
- You attack the first slime in the room; then every slime answers in the order they were spawned.
- Clumsiness (the best three of six d6, out of 20) lets only the room under the cap through: at 16 clumsiness you do 4/20 of your damage. Improving it costs that room in cruor, so it gets dearer as you get less clumsy.
- A move needs more stamina than it costs (strong attack costs 6 of 10, so it needs 7). Rest is always open and gives back half your maximum.
- Respawning puts you at the checkpoint with full health and stamina, and leaves the slimes as they were; only the checkpoint's Restore Health brings dead slimes back (and it does not restore stamina).
- Dying drops all your cruor in a gravestone where you fell; dying again before you get it back loses it.
- There is one checkpoint, so Set Checkpoint is never offered.
- No game menu or ad break in the middle of a fight.
- The 2-minute ad break that only counts down when you press OK, with the umlaut.fyi sponsor link.
- The wordy messages: every stamina change says what it gained and what it now has.
