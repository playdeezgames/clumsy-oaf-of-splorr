#!/usr/bin/env python3
"""Draws the 630 by 500 itch.io cover from the game's own font sheet (DINOBYTE, unmodified)."""
import sys
from PIL import Image

SHEET = "assets/dinobyte/dbyte_1x.png"
OUT = sys.argv[1] if len(sys.argv) > 1 else "assets/cover.png"
BG = (11, 11, 18)
INK = {
    "beige": (172, 172, 145),
    "white": (255, 255, 255),
    "yellow": (255, 215, 95),
    "green": (95, 215, 95),
    "red": (255, 95, 95),
    "gray": (128, 128, 128),
    "dim": (60, 60, 72),
    "cyan": (95, 215, 215),
    "magenta": (215, 95, 215),
}

sheet = Image.open(SHEET).convert("RGBA")
W, H = 630, 500
img = Image.new("RGB", (W, H), BG)


def glyph(code, color, scale):
    cx, cy = (code % 16) * 6, (code // 16) * 8
    cell = sheet.crop((cx, cy, cx + 6, cy + 8))
    out = Image.new("RGBA", (6, 8), color + (0,))
    out.putalpha(cell.getchannel("A"))
    return out.resize((6 * scale, 8 * scale), Image.NEAREST)


def put(x, y, s, color, scale):
    for ch in s:
        code = ch if isinstance(ch, int) else ord(ch)
        g = glyph(code, INK[color], scale)
        img.paste(g, (x, y), g)
        x += 6 * scale


def centered(y, s, color, scale):
    put((W - len(s) * 6 * scale) // 2, y, s, color, scale)


# Title
centered(22, "CLUMSY OAF", "yellow", 9)
centered(104, "of SPLORR!!", "yellow", 5)

# A maze corridor: walls above and below, the oaf facing two slimes that show their next move.
S = 5
cols = W // (6 * S)
left = (W - cols * 6 * S) // 2
scene_y = 200
for i in range(cols):
    put(left + i * 6 * S, scene_y, [0xB1], "dim", S)
    put(left + i * 6 * S, scene_y + 18 * S, [0xB1], "dim", S)
row = scene_y + 10 * S
put(left + 2 * 6 * S, row, [0x02], "white", S)  # the oaf
put(left + 9 * 6 * S, row, [0x09], "green", S)  # a slime
put(left + 16 * 6 * S, row, [0x09], "green", S)  # another


def tag(cell, text):
    """A posture label centred over the slime in the given corridor cell."""
    cx = left + cell * 6 * S + 3 * S
    put(cx - len(text) * 6, row - 22, text, "magenta", 2)


tag(9, "STRONG_ATTACK")
tag(16, "FAST_ATTACK")

# Tagline and credits
centered(366, "Read the slime.", "white", 3)
centered(398, "Losing is fun.", "white", 3)
centered(440, "Soulslike Game Jam #3", "beige", 2)
centered(466, "TheGrumpyGameDev", "beige", 2)
img.save(OUT)
print("wrote", OUT, img.size)
