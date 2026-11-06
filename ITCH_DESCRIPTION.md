Page: https://thegrumpygamedev.itch.io/clumsy-oaf-of-splorr
Jam: Soulslike Game Jam #3 (https://itch.io/jam/soulslike-game-jam-3). Original release August 2026 (VB.NET, Blazor in the browser plus Windows, macOS and Linux downloads). This is the November 2026 browser port, replacing the `html` channel.
Pasted in by hand; nothing here is published by the repo. Everything above "Notes (not for the page)" is for the page.

**Short description**

A turn-based, text-based soulslike. Read the slime, dodge, parry, die, respawn. Now saves in your browser.

**Description**

**Latest version:** https://playdeezgames.github.io/clumsy-oaf-of-splorr/

Hello.

So, I don't play Soulslike games, so I had to learn about the core conceits and conventions.

Also, I only make turn based, text based metaphors, which is the opposite of what soulslikes expect, according to what I learned.

I think I've managed to get to an approximation of the core game loop within the medium that I work in.

But you will be the judge. Literally.

**Controls:**

Press the number or letter of a choice, or use the arrow keys and Enter. Escape is Never Mind. Page Up and Page Down scroll back through the messages. On a phone or tablet, tap a choice.

**New: rebuilt for the browser**

The metaphor has been rebuilt in Odin and WebAssembly. The maze, the slimes, the postures and the numbers are the original's, with a few fixes:

- It saves as you play, in this browser, and picks up where you left off (even mid-fight).
- Improve Health at the checkpoint now works (it used to do nothing).
- When several slimes kill you in one turn, your cruor stays in your gravestone (it used to be lost).
- Messages say what actually changed.
- It has a layout for phones held upright.
- The old Windows, macOS and Linux downloads have been removed: it plays right here in the browser. The original source is still on GitHub.

The full list is in QUIRKS.md in the source.

**AI use**

This browser version was written with Claude Code (Anthropic's AI coding assistant) at my direction: the port, its tests, the cover and the page. The metaphor, its design and its text are the original's, and the August 2026 original was written without AI.

**Credits**

A production of TheGrumpyGameDev, made for Soulslike Game Jam #3. Sponsored by UMLAUT.FYI! (https://umlaut.fyi/). Source: https://github.com/playdeezgames/clumsy-oaf-of-splorr

Font: DINOBYTE by mby, https://mby.itch.io/dinobyte, CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/), unmodified.

---

**Notes (not for the page)**

- The short description is 106 characters (itch.io allows 120).
- The four paragraphs from "Hello." to "Literally." are the page's current text, kept as they are.
- Ship day: `./shippit.sh --push` replaces the `html` channel. The page already embeds the browser build ("Run Metaphor"), so Kind of project stays HTML. Then remove the windows, linux and mac downloads (the 2026 Spectre console builds, `Metaphor.Spectre`) in the dashboard: the user's rule for every port (November 5, 2026).
- Set the page's AI generation disclosure to Yes (Code), as for Handies for Cash!.
- GitHub Pages: the repo is already public. Once the user enables Pages (branch main, /docs) and pushes, add the "Latest version" line above as an external link on the page too.
- Upload `assets/cover.png` (new, drawn from the DINOBYTE sheet by `tools/make_cover.py`; the old cover was a screenshot of the Blazor title, still in git history as `ss/cover.png`) and the screenshots in `assets/screenshots/` (rendered from the real game by `tools/make_shots.sh`).
- The page is a Soulslike Game Jam #3 submission; check the jam's rules on post-jam updates before ship day if unsure.
