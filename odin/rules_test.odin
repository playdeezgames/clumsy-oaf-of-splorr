#+build !js
package clumsy

import "core:strings"
import "core:testing"

@(private = "file")
new_game :: proc(seed: u64 = 2026, name := "Oaf") -> Game {
	g: Game
	rng_seed(&g.rng, seed)
	embark(&g, name)
	return g
}

@(private = "file")
first_spawner :: proc(g: ^Game) -> int {
	for i in 0 ..< g.feature_count {
		if g.features[i].kind == .Spawner {
			return i
		}
	}
	return -1
}

// Puts the avatar in front of exactly one slime (the given spawner's), with a known posture.
@(private = "file")
face_one_slime :: proc(g: ^Game, spawner: int, posture: Posture) -> ^Slime {
	for i in 0 ..< g.feature_count {
		g.slimes[i].alive = false
	}
	g.slimes[spawner] = Slime{alive = true, health = SLIME_HEALTH, posture = posture, seq = 0}
	g.location = g.features[spawner].location
	return &g.slimes[spawner]
}

@(private = "file")
has_message :: proc(g: ^Game, text: string) -> bool {
	for i in 0 ..< g.message_count {
		if message_text(&g.messages[i]) == text {
			return true
		}
	}
	return false
}

@(private = "file")
count_messages_containing :: proc(g: ^Game, text: string) -> int {
	n := 0
	for i in 0 ..< g.message_count {
		if strings.contains(message_text(&g.messages[i]), text) {
			n += 1
		}
	}
	return n
}

@(test)
embark_starts_in_the_blue_room :: proc(t: ^testing.T) {
	g := new_game()
	testing.expect(t, g.embarked)
	testing.expect_value(t, g.location, BLUE_ROOM)
	testing.expect_value(t, counter(&g, .Health), 100)
	testing.expect_value(t, counter(&g, .Stamina), 10)
	testing.expect_value(t, counter(&g, .Attack), 20)
	testing.expect_value(t, counter(&g, .Defend), 5)
	testing.expect_value(t, counter(&g, .Cruor), 0) // the DEBUG build started with 100
	c := counter(&g, .Clumsiness)
	testing.expect(t, c >= 3 && c <= 18)
	testing.expect_value(t, counter_max(&g, .Clumsiness), 20)
	testing.expect_value(t, g.features[g.checkpoint].kind, Feature_Kind.Checkpoint)
	testing.expect(t, has_message(&g, "Welcome to Clumsy Oaf of SPLORR!!"))
	testing.expect(t, has_message(&g, "Oaf is in The Blue Room."))
	testing.expect(t, has_message(&g, "- Checkpoint"))
	testing.expect(t, has_message(&g, "- Door going OUT"))
	testing.expect(t, !in_combat(&g))
}

@(test)
empty_name_gets_the_default :: proc(t: ^testing.T) {
	g := new_game(name = "")
	testing.expect_value(t, name_of(&g), DEFAULT_NAME)
}

@(test)
maze_is_a_connected_tree_with_one_entrance :: proc(t: ^testing.T) {
	for seed in u64(1) ..= 200 {
		g := new_game(seed)
		maze_doors, ins, outs := 0, 0, 0
		for i in 0 ..< g.feature_count {
			f := g.features[i]
			if f.kind != .Door {
				continue
			}
			switch f.direction {
			case .In:
				ins += 1
				testing.expect_value(t, f.dest, BLUE_ROOM)
			case .Out:
				outs += 1
				testing.expect_value(t, f.location, BLUE_ROOM)
			case .North, .East, .South, .West:
				maze_doors += 1
				// Every passage has a door back.
				back := false
				for j in 0 ..< g.feature_count {
					h := g.features[j]
					back ||= h.kind == .Door && h.location == f.dest && h.dest == f.location
				}
				testing.expect(t, back)
			}
		}
		testing.expect_value(t, maze_doors, 2 * (MAZE_COLUMNS * MAZE_ROWS - 1)) // a spanning tree
		testing.expect_value(t, ins, 1)
		testing.expect_value(t, outs, 1)

		// Everything is reachable from the Blue Room.
		seen: [LOCATION_COUNT]bool
		stack: [LOCATION_COUNT]int
		sp := 1
		stack[0] = BLUE_ROOM
		seen[BLUE_ROOM] = true
		for sp > 0 {
			sp -= 1
			l := stack[sp]
			for i in 0 ..< g.feature_count {
				f := g.features[i]
				if f.kind == .Door && f.location == l && !seen[f.dest] {
					seen[f.dest] = true
					stack[sp] = f.dest
					sp += 1
				}
			}
		}
		for s in seen {
			testing.expect(t, s)
		}
	}
}

@(test)
dead_ends_get_more_slimes :: proc(t: ^testing.T) {
	for seed in u64(1) ..= 200 {
		g := new_game(seed)
		testing.expect(t, g.feature_count <= MAX_FEATURES)
		for l in 1 ..< LOCATION_COUNT {
			doors, spawners := 0, 0
			for i in 0 ..< g.feature_count {
				f := g.features[i]
				if f.location != l {
					continue
				}
				if f.kind == .Door {
					doors += 1
				}
				if f.kind == .Spawner {
					spawners += 1
					testing.expect(t, g.slimes[i].alive)
					testing.expect_value(t, g.slimes[i].health, SLIME_HEALTH)
				}
			}
			lo, hi := 1, 1
			switch doors {
			case 1:
				hi = 4
			case 2:
				hi = 3
			case 3:
				hi = 2
			}
			testing.expect(t, spawners >= lo && spawners <= hi)
		}
	}
}

@(test)
effectiveness_rounds_like_vb :: proc(t: ^testing.T) {
	testing.expect_value(t, effective_attack(25, .Fast_Attack, .Rest), 38) // 37.5 -> 38
	testing.expect_value(t, effective_attack(25, .Fast_Attack, .Parry), 12) // 12.5 -> 12
	testing.expect_value(t, effective_attack(21, .Strong_Attack, .Rest), 32) // 31.5 -> 32
	testing.expect_value(t, effective_attack(21, .Strong_Attack, .Dodge), 10) // 10.5 -> 10
	testing.expect_value(t, effective_attack(20, .Fast_Attack, .Dodge), 0)
	testing.expect_value(t, effective_attack(20, .Strong_Attack, .Parry), 0)
	testing.expect_value(t, effective_attack(20, .Strong_Attack, .Strong_Attack), 20)
	testing.expect_value(t, effective_attack(20, .Fast_Attack, .Fast_Attack), 20)
}

@(test)
posture_costs_need_more_stamina_than_the_cost :: proc(t: ^testing.T) {
	g := new_game()
	face_one_slime(&g, first_spawner(&g), .Rest)
	testing.expect_value(t, dodge_cost(&g), 1)
	testing.expect_value(t, parry_cost(&g), 2)
	testing.expect_value(t, fast_attack_cost(&g), 3)
	testing.expect_value(t, strong_attack_cost(&g), 6)
	testing.expect_value(t, rest_recovery(&g), 5)
	counter_set(&g, .Stamina, 6)
	testing.expect(t, !can_take_posture(&g, .Strong_Attack))
	testing.expect(t, can_take_posture(&g, .Fast_Attack))
	counter_set(&g, .Stamina, 0)
	testing.expect(t, !can_take_posture(&g, .Dodge))
	testing.expect(t, can_take_posture(&g, .Rest))
	// Out of combat, no combat moves at all.
	g.location = BLUE_ROOM
	testing.expect(t, !can_take_posture(&g, .Rest))
}

@(test)
clumsiness_scales_the_avatars_damage :: proc(t: ^testing.T) {
	g := new_game()
	s := face_one_slime(&g, first_spawner(&g), .Parry) // a parrying slime: it does nothing back
	counter_set(&g, .Clumsiness, 10)
	combat(&g, .Strong_Attack) // 20 attack, strong vs parry is 0
	testing.expect(t, has_message(&g, "Oaf does 0 damage to Slime."))
	testing.expect_value(t, s.health, SLIME_HEALTH)

	s.posture = .Rest
	counter_set(&g, .Stamina, 10)
	combat(&g, .Fast_Attack) // 30 - 5 = 25, times 10/20
	testing.expect(t, has_message(&g, "Oaf does 12 damage to Slime."))
	testing.expect(t, has_message(&g, "Slime has 13/25 health."))
	testing.expect_value(t, s.health, 13)
}

@(test)
killing_a_slime_pays_cruor_and_ends_the_fight :: proc(t: ^testing.T) {
	g := new_game()
	sp := first_spawner(&g)
	s := face_one_slime(&g, sp, .Rest)
	s.health = 1
	counter_set(&g, .Clumsiness, 0)
	combat(&g, .Fast_Attack)
	testing.expect(t, !s.alive)
	testing.expect_value(t, counter(&g, .Cruor), 1)
	testing.expect(t, has_message(&g, "Oaf kills Slime."))
	testing.expect(t, has_message(&g, "Oaf receives 1 cruor."))
	testing.expect(t, has_message(&g, "Oaf now has 1 cruor."))
	testing.expect(t, !in_combat(&g))
	testing.expect(t, has_message(&g, "Oaf is in Maze Room."))
}

@(test)
slimes_answer_with_the_posture_they_showed :: proc(t: ^testing.T) {
	g := new_game()
	s := face_one_slime(&g, first_spawner(&g), .Fast_Attack)
	combat(&g, .Parry) // 25 halved is 12 (banker's), minus 5 defend
	testing.expect(t, has_message(&g, "Oaf parries."))
	testing.expect(t, has_message(&g, "Slime does fast attack."))
	testing.expect(t, has_message(&g, "Slime does 7 damage to Oaf."))
	testing.expect_value(t, counter(&g, .Health), 93)
	testing.expect_value(t, counter(&g, .Stamina), 8)
	testing.expect(t, s.alive)

	s.posture = .Strong_Attack
	combat(&g, .Parry)
	testing.expect(t, has_message(&g, "Slime does 0 damage to Oaf."))
	testing.expect_value(t, counter(&g, .Health), 93)

	s.posture = .Strong_Attack
	combat(&g, .Rest) // 25 * 1.5 = 37.5 -> 38, minus 5
	testing.expect(t, has_message(&g, "Slime does 33 damage to Oaf."))
	testing.expect(t, has_message(&g, "Oaf has 60/100 health."))
	testing.expect(t, has_message(&g, "Oaf rests."))
	testing.expect_value(t, counter(&g, .Stamina), 10)
}

@(test)
resting_at_full_stamina_says_no_gain :: proc(t: ^testing.T) {
	g := new_game()
	face_one_slime(&g, first_spawner(&g), .Dodge)
	combat(&g, .Rest)
	testing.expect_value(t, count_messages_containing(&g, "stamina"), 0)
	testing.expect(t, has_message(&g, "Oaf rests."))
	testing.expect(t, has_message(&g, "Slime dodges."))
}

@(test)
death_leaves_a_gravestone_with_the_cruor :: proc(t: ^testing.T) {
	g := new_game()
	sp := first_spawner(&g)
	where_ := g.features[sp].location
	face_one_slime(&g, sp, .Strong_Attack)
	counter_set(&g, .Cruor, 7)
	counter_set(&g, .Health, 1)
	combat(&g, .Rest)
	testing.expect(t, is_dead(&g))
	testing.expect(t, has_message(&g, "Slime kills Oaf."))
	testing.expect(t, g.grave.active)
	testing.expect_value(t, g.grave.location, where_)
	testing.expect_value(t, g.grave.cruor, 7)
	testing.expect_value(t, counter(&g, .Cruor), 0)

	respawn(&g)
	testing.expect(t, !is_dead(&g))
	testing.expect_value(t, g.location, BLUE_ROOM)
	testing.expect_value(t, counter(&g, .Health), 100)
	testing.expect_value(t, counter(&g, .Stamina), 10)
	testing.expect(t, has_message(&g, "Oaf respawns in The Blue Room."))
	testing.expect(t, g.slimes[sp].alive) // the slime is still there

	// Dying again before getting it back loses it.
	g.location = where_
	counter_set(&g, .Cruor, 2)
	counter_set(&g, .Health, 1)
	g.slimes[sp].posture = .Fast_Attack
	combat(&g, .Rest)
	testing.expect(t, is_dead(&g))
	testing.expect_value(t, g.grave.cruor, 2)
}

@(test)
the_rest_stop_swinging_once_you_are_dead :: proc(t: ^testing.T) {
	g := new_game()
	// Three slimes in one room, all about to strike.
	l := g.features[first_spawner(&g)].location
	for i in 0 ..< g.feature_count {
		g.slimes[i].alive = false
	}
	n := 0
	for i in 0 ..< g.feature_count {
		if g.features[i].kind == .Spawner && n < 3 {
			g.features[i].location = l
			g.slimes[i] = Slime{alive = true, health = SLIME_HEALTH, posture = .Strong_Attack, seq = n}
			n += 1
		}
	}
	g.location = l
	counter_set(&g, .Cruor, 9)
	counter_set(&g, .Health, 1)
	combat(&g, .Rest)
	testing.expect(t, is_dead(&g))
	testing.expect_value(t, count_messages_containing(&g, "kills Oaf"), 1)
	testing.expect_value(t, g.grave.cruor, 9) // the original replaced it with an empty one
}

@(test)
gravestone_gives_the_cruor_back :: proc(t: ^testing.T) {
	g := new_game()
	g.grave = Gravestone{active = true, location = BLUE_ROOM, cruor = 5}
	fs: [MAX_FEATURES + 1]int
	n := features_here(&g, fs[:])
	testing.expect_value(t, fs[n - 1], GRAVESTONE)
	look_action(&g)
	testing.expect(t, has_message(&g, "- Oaf's Gravestone"))
	perform(&g, .Restore_Cruor, GRAVESTONE)
	testing.expect_value(t, counter(&g, .Cruor), 5)
	testing.expect(t, has_message(&g, "Oaf regains 5 cruor."))
	testing.expect(t, has_message(&g, "Oaf now has 5 cruor."))
	testing.expect(t, !g.grave.active)
}

@(test)
checkpoint_improvements :: proc(t: ^testing.T) {
	g := new_game()
	cp := g.checkpoint
	testing.expect(t, !verb_enabled(&g, .Set_Checkpoint, cp)) // already the current one
	testing.expect(t, !verb_enabled(&g, .Restore_Health, cp)) // already at full health
	testing.expect(t, !verb_enabled(&g, .Improve_Defend, cp)) // 0 cruor

	counter_set(&g, .Cruor, 100)
	counter_set(&g, .Clumsiness, 15)

	perform(&g, .Improve_Health, cp) // a no-op in the original
	testing.expect_value(t, counter_max(&g, .Health), 110)
	testing.expect_value(t, counter(&g, .Cruor), 90)
	testing.expect(t, has_message(&g, "Oaf spends 10 cruor."))
	testing.expect(t, has_message(&g, "Oaf gains 10 maximum health."))
	testing.expect(t, has_message(&g, "Oaf now has 100/110 health."))

	perform(&g, .Improve_Stamina, cp)
	testing.expect_value(t, counter_max(&g, .Stamina), 11)
	testing.expect_value(t, counter(&g, .Cruor), 80)
	testing.expect(t, has_message(&g, "Oaf now has 10/11 stamina."))

	perform(&g, .Improve_Attack, cp)
	testing.expect_value(t, counter(&g, .Attack), 21)
	testing.expect_value(t, counter(&g, .Cruor), 60)
	testing.expect(t, has_message(&g, "Oaf gains 1 attack.")) // the original said "defend"

	perform(&g, .Improve_Defend, cp)
	testing.expect_value(t, counter(&g, .Defend), 6)
	testing.expect_value(t, counter(&g, .Cruor), 55)

	perform(&g, .Improve_Clumsiness, cp) // costs the room under the cap: 20 - 15
	testing.expect_value(t, counter(&g, .Clumsiness), 14)
	testing.expect_value(t, counter(&g, .Cruor), 50)
	testing.expect(t, has_message(&g, "Oaf now has 70% clumsiness."))

	status(&g)
	testing.expect(t, has_message(&g, "Health: 100/110"))
	testing.expect(t, has_message(&g, "Clumsiness: 70%"))
	testing.expect(t, has_message(&g, "Cruor: 50"))
	testing.expect(t, has_message(&g, "Attack: 21"))
	testing.expect(t, has_message(&g, "Defend: 6"))

	counter_set(&g, .Clumsiness, 0)
	testing.expect(t, !verb_enabled(&g, .Improve_Clumsiness, cp))
}

@(test)
restore_health_brings_the_slimes_back :: proc(t: ^testing.T) {
	g := new_game()
	sp := first_spawner(&g)
	g.slimes[sp].alive = false
	counter_set(&g, .Health, 40)
	testing.expect(t, verb_enabled(&g, .Restore_Health, g.checkpoint))
	perform(&g, .Restore_Health, g.checkpoint)
	testing.expect_value(t, counter(&g, .Health), 100)
	testing.expect(t, has_message(&g, "Oaf now has 100/100 health."))
	testing.expect(t, g.slimes[sp].alive)
	testing.expect_value(t, g.slimes[sp].health, SLIME_HEALTH)
}

@(test)
doors_take_you_through :: proc(t: ^testing.T) {
	g := new_game()
	out := -1
	for i in 0 ..< g.feature_count {
		if g.features[i].kind == .Door && g.features[i].location == BLUE_ROOM {
			out = i
		}
	}
	perform(&g, .Enter, out)
	testing.expect(t, has_message(&g, "Oaf goes through Door going OUT."))
	testing.expect_value(t, g.location, g.features[out].dest)
	testing.expect(t, in_combat(&g)) // every maze room has at least one slime
	testing.expect(t, has_message(&g, "Oaf is in combat with:"))
	// No walking off mid-fight.
	fs: [MAX_FEATURES + 1]int
	n := features_here(&g, fs[:])
	testing.expect(t, n > 0)
	testing.expect(t, !verb_enabled(&g, .Enter, fs[0]))
}

@(test)
verbs_are_checked_against_the_place :: proc(t: ^testing.T) {
	g := new_game()
	// A feature somewhere else, or of the wrong kind, does nothing.
	far := -1
	for i in 0 ..< g.feature_count {
		if g.features[i].location != BLUE_ROOM && g.features[i].kind == .Door {
			far = i
		}
	}
	testing.expect(t, !verb_enabled(&g, .Enter, far))
	testing.expect(t, !verb_enabled(&g, .Enter, g.checkpoint))
	testing.expect(t, !verb_enabled(&g, .Restore_Cruor, GRAVESTONE))
	testing.expect(t, !verb_enabled(&g, .Enter, -1))
	before := g
	perform(&g, .Enter, far)
	testing.expect_value(t, g.location, before.location)
}

@(test)
ad_break_counts_down_and_ends :: proc(t: ^testing.T) {
	g := new_game()
	ad_start(&g, 1000)
	testing.expect(t, in_ad(&g))
	ad_show(&g, 1000 + 30_000)
	testing.expect(t, has_message(&g, "Time left in ad break: 01:30"))
	testing.expect_value(t, g.messages[2].link, Link.Umlaut)
	ad_show(&g, 1000 + AD_DURATION_MS)
	testing.expect(t, !in_ad(&g))
	testing.expect(t, has_message(&g, "Ad break is complete! You may return to yer metaphor!"))
}

// The fight is meant to be read: the slime shows what it will do next. Answering a fast attack
// with a dodge, a strong attack with a parry, and attacking only a resting slime wins nearly
// every one-on-one fight from the start, whatever the clumsiness roll.
@(test)
reading_the_tells_wins :: proc(t: ^testing.T) {
	wins, fights := 0, 0
	for seed in u64(1) ..= 300 {
		g := new_game(seed)
		sp := first_spawner(&g)
		s := face_one_slime(&g, sp, Posture(seed % 5))
		for _ in 0 ..< 200 {
			if !in_combat(&g) || is_dead(&g) {
				break
			}
			move := Posture.Rest
			switch s.posture {
			case .Fast_Attack:
				move = .Dodge
			case .Strong_Attack:
				move = .Parry
			case .Rest:
				move = .Fast_Attack
			case .Dodge, .Parry:
				move = .Rest
			}
			if !can_take_posture(&g, move) {
				move = .Rest
			}
			combat(&g, move)
		}
		fights += 1
		if !is_dead(&g) && !s.alive {
			wins += 1
		}
	}
	testing.expectf(t, wins * 10 >= fights * 9, "won %d of %d", wins, fights)
}
