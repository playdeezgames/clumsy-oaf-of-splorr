# Clumsy Oaf of SPLORR!!

A Production of TheGrumpyGameDev

For Soulslike Game Jam #3: a turn-based, text-based soulslike. Fight slimes in a maze, read their postures, die, respawn at the checkpoint, and spend the cruor you won (if you got it back from your gravestone) on getting less clumsy.

- Play: https://playdeezgames.github.io/clumsy-oaf-of-splorr/ (GitHub Pages, the latest build) or https://thegrumpygamedev.itch.io/clumsy-oaf-of-splorr
- The original (August 2026) is VB.NET in `src/`, with a Spectre console client and a Blazor browser client.
- The browser version (November 2026) is Odin compiled to WebAssembly in `odin/`, ported by Claude Code (an AI agent) at the author's direction. What changed is in `QUIRKS.md`.

## Building the browser version

Needs Odin on the PATH (or `ODIN=/path/to/odin`), Python 3 with Pillow for the images, and `zip`.

```bash
tools/test.sh          # native tests
tools/build.sh         # build/web (serve it with any static server)
tools/ship.sh          # tests, size build, zip; pushes to itch.io only with --push
tools/pages.sh         # copies the size build into docs/ for GitHub Pages
tools/make_shots.sh    # assets/screenshots/ from the real game
tools/make_cover.py    # assets/cover.png
```

Font: DINOBYTE by mby, https://mby.itch.io/dinobyte, CC BY 4.0, unmodified (`assets/dinobyte/`).
