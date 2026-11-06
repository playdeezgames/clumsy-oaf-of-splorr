#+build !js
package clumsy

import "core:fmt"
import "core:os"
import "core:testing"

// tools/make_shots.sh runs this with -define:DUMP_DIR=<dir>; it writes the real game's grid for a
// few screens as raw cells (cols, rows, then a glyph and an attribute byte per cell), which the
// tool turns into PNGs with the font sheet. Without DUMP_DIR it does nothing.
DUMP_DIR :: #config(DUMP_DIR, "")

@(private = "file")
dump :: proc(a: ^App, name: string) {
	grid := new(Grid)
	defer free(grid)
	app_draw(a, grid)
	buf := make([dynamic]u8, 0, 2 + MAX_ROWS * MAX_COLS * 2)
	defer delete(buf)
	append(&buf, u8(grid.cols), u8(grid.rows))
	for r in 0 ..< grid.rows {
		for c in 0 ..< grid.cols {
			append(&buf, grid.cells[r][c].ch, grid.cells[r][c].attr)
		}
	}
	path := fmt.tprintf("%s/%s.cells", DUMP_DIR, name)
	_ = os.write_entire_file(path, buf[:])
}

@(test)
dump_screens :: proc(t: ^testing.T) {
	when DUMP_DIR == "" {
		return
	}
	a: App
	app_init(&a, 2026, 0)
	dump(&a, "01-title")

	embark(&a.game, "Olen Kyrpa")
	ui_enter(&a, .Play)
	dump(&a, "02-blue-room")

	// Through the door, into the first fight.
	g := &a.game
	out := -1
	for i in 0 ..< g.feature_count {
		if g.features[i].kind == .Door && g.features[i].location == BLUE_ROOM {
			out = i
		}
	}
	perform(g, .Enter, out)
	ui_enter(&a, play_screen(g))
	dump(&a, "03-combat")

	// A few rounds, answering what each slime shows.
	for _ in 0 ..< 3 {
		if !in_combat(g) || is_dead(g) {
			break
		}
		enemies: [MAX_FEATURES]int
		enemies_here(g, enemies[:])
		move := Posture.Rest
		#partial switch g.slimes[enemies[0]].posture {
		case .Fast_Attack:
			move = .Dodge
		case .Strong_Attack:
			move = .Parry
		case .Rest:
			move = .Fast_Attack
		}
		if !can_take_posture(g, move) {
			move = .Rest
		}
		combat(g, move)
	}
	ui_enter(&a, play_screen(g))
	dump(&a, "04-fight")

	// Back at the checkpoint with some cruor to spend.
	g.location = BLUE_ROOM
	counter_set(g, .Cruor, 24)
	clear_messages(g)
	ui_enter(&a, .Play)
	a.ui.sel_feature = g.checkpoint
	ui_enter(&a, .Feature)
	dump(&a, "05-checkpoint")

	status(g)
	ui_enter(&a, .Play)
	dump(&a, "06-status")

	counter_set(g, .Health, 0)
	clear_messages(g)
	say(g, "Slime does 33 damage to Olen Kyrpa.")
	say(g, "Slime kills Olen Kyrpa.")
	ui_enter(&a, play_screen(g))
	dump(&a, "07-dead")
}
