#+build !js
package clumsy

import "core:log"
import "core:testing"

@(private = "file")
labels_have :: proc(a: ^App, label: string) -> int {
	for i in 0 ..< a.ui.choice_count {
		if choice_label(&a.ui.choices[i]) == label {
			return i
		}
	}
	return -1
}

@(private = "file")
press :: proc(a: ^App, label: string) {
	i := labels_have(a, label)
	if i >= 0 {
		app_key(a, i32(choice_hotkey(i)))
	}
}

@(private = "file")
type_name :: proc(a: ^App, name: string) {
	for ch in transmute([]u8)name {
		app_key(a, i32(ch))
	}
	app_key(a, i32(Key.Enter))
}

@(test)
title_to_play :: proc(t: ^testing.T) {
	a: App
	app_init(&a, 7, 0)
	testing.expect_value(t, a.ui.screen, Screen.Title)
	testing.expect(t, labels_have(&a, "Soulslike Game Jam #3") >= 0)
	app_key(&a, '1')
	testing.expect_value(t, a.ui.screen, Screen.Main_Menu)
	app_key(&a, '1')
	testing.expect_value(t, a.ui.screen, Screen.Name_Prompt)
	type_name(&a, "Oaf")
	testing.expect_value(t, a.ui.screen, Screen.Play)
	testing.expect_value(t, name_of(&a.game), "Oaf")
	testing.expect(t, labels_have(&a, "Status...") == 0)
	testing.expect(t, labels_have(&a, "Checkpoint...") >= 0)
	testing.expect(t, labels_have(&a, "Door going OUT...") >= 0)
	testing.expect(t, labels_have(&a, "Gämë Mënü") >= 0)
}

@(test)
feature_menu_lists_only_what_can_be_done :: proc(t: ^testing.T) {
	a: App
	app_init(&a, 7, 0)
	embark(&a.game, "Oaf")
	ui_enter(&a, .Play)
	press(&a, "Checkpoint...")
	testing.expect_value(t, a.ui.screen, Screen.Feature)
	testing.expect_value(t, string(a.ui.prompt[:a.ui.prompt_len]), "Do what with Checkpoint?")
	testing.expect_value(t, a.ui.choice_count, 1) // Never Mind: no cruor, full health
	app_key(&a, i32(Key.Escape))
	testing.expect_value(t, a.ui.screen, Screen.Play)

	counter_set(&a.game, .Cruor, 5)
	press(&a, "Checkpoint...")
	testing.expect(t, labels_have(&a, "Improve Defend") >= 0)
	testing.expect(t, labels_have(&a, "Improve Attack") < 0)
	press(&a, "Improve Defend")
	testing.expect_value(t, a.ui.screen, Screen.Play)
	testing.expect_value(t, counter(&a.game, .Defend), 6)
}

@(test)
walking_into_the_maze_starts_a_fight :: proc(t: ^testing.T) {
	a: App
	app_init(&a, 7, 0)
	embark(&a.game, "Oaf")
	ui_enter(&a, .Play)
	press(&a, "Door going OUT...")
	press(&a, "Enter")
	testing.expect_value(t, a.ui.screen, Screen.Combat)
	testing.expect_value(t, labels_have(&a, "Strong Attack"), 0)
	testing.expect(t, labels_have(&a, "Rest") >= 0)
	testing.expect(t, labels_have(&a, "Gämë Mënü") < 0) // none mid-fight, as in the original
}

@(test)
dying_offers_respawn :: proc(t: ^testing.T) {
	a: App
	app_init(&a, 7, 0)
	embark(&a.game, "Oaf")
	counter_set(&a.game, .Health, 0)
	ui_enter(&a, play_screen(&a.game))
	testing.expect_value(t, a.ui.screen, Screen.Dead)
	testing.expect_value(t, labels_have(&a, "Respawn"), 0)
	app_key(&a, '1')
	testing.expect_value(t, a.ui.screen, Screen.Play)
	testing.expect_value(t, counter(&a.game, .Health), 100)
}

@(test)
abandon_returns_to_the_main_menu :: proc(t: ^testing.T) {
	a: App
	app_init(&a, 7, 0)
	embark(&a.game, "Oaf")
	ui_enter(&a, .Play)
	press(&a, "Gämë Mënü")
	app_key(&a, '2')
	testing.expect_value(t, a.ui.screen, Screen.Confirm_Abandon)
	app_key(&a, '2')
	testing.expect_value(t, a.ui.screen, Screen.Main_Menu)
	testing.expect(t, !a.game.embarked)
}

@(test)
resume_lands_in_play :: proc(t: ^testing.T) {
	a: App
	app_init(&a, 7, 0)
	embark(&a.game, "Oaf")
	text, _ := save_encode(&a.game)
	saved := string(transmute([]u8)text)
	b: App
	app_init(&b, 8, 0)
	buf: [1 << 16]u8
	n := copy(buf[:], saved)
	testing.expect(t, app_resume(&b, string(buf[:n])))
	testing.expect_value(t, b.ui.screen, Screen.Play)
	testing.expect_value(t, name_of(&b.game), "Oaf")
}

// Random play: thousands of choices over many seeds. Nothing may crash, and the counters and
// the world must stay in shape whatever is chosen.
@(test)
random_play_keeps_the_world_sound :: proc(t: ^testing.T) {
	grid := new(Grid)
	defer free(grid)
	kills, deaths, improvements := 0, 0, 0
	for seed in u64(1) ..= 60 {
		a: App
		app_init(&a, seed, 0)
		embark(&a.game, "Bot")
		ui_enter(&a, .Play)
		pick: Rng
		rng_seed(&pick, seed * 31)
		for step in 0 ..< 1500 {
			a.now_ms = f64(step) * 10_000
			g := &a.game
			if a.ui.screen == .Main_Menu || a.ui.screen == .Confirm_Abandon || a.ui.screen == .Game_Menu {
				ui_enter(&a, play_screen(g))
			}
			// Avoid the links and the menus that leave the game; prefer fighting and the checkpoint.
			options: [MAX_CHOICES]int
			n := 0
			for i in 0 ..< a.ui.choice_count {
				c := a.ui.choices[i]
				if c.kind == .Open_Link || c.kind == .Watch_Ad && rng_range(&pick, 0, 20) != 0 {
					continue
				}
				if c.kind == .Go && Screen(c.a) == .Game_Menu {
					continue
				}
				options[n] = i
				n += 1
			}
			if n == 0 {
				ui_enter(&a, play_screen(g))
				continue
			}
			cruor_before := counter(g, .Cruor)
			was_dead := is_dead(g)
			ui_choose(&a, options[rng_range(&pick, 0, n - 1)])
			if !was_dead && is_dead(g) {
				deaths += 1
			}
			if counter(g, .Cruor) == cruor_before + 1 {
				kills += 1
			}
			if counter(g, .Cruor) < cruor_before && !is_dead(g) {
				improvements += 1
			}
			for c in Counter {
				s := g.counters[c]
				testing.expect(t, s.value >= s.min && s.value <= s.max)
			}
			testing.expect(t, g.location >= 0 && g.location < LOCATION_COUNT)
			testing.expect(t, g.message_count <= MAX_MESSAGES)
			app_draw(&a, grid)
		}
	}
	// A random clicker fights and dies a lot; each death leaves its cruor in a gravestone, so
	// it seldom gets back to the checkpoint with enough to spend (improvements have their own
	// tests).
	log.infof("random play: %d kills, %d deaths, %d improvements", kills, deaths, improvements)
	testing.expect(t, kills > 0)
	testing.expect(t, deaths > 0)
}
