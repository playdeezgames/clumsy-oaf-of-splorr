package clumsy

import "core:fmt"

// Pure rules of Clumsy Oaf of SPLORR!!, ported from the VB.NET game (Soulslike Game Jam #3,
// August 2026). No platform imports: the clock is an argument, the RNG lives in the Game.
// Everything is plain data (no pointers, no allocations) so saving is a straight copy.
// Deliberate changes from the original are listed in QUIRKS.md.

TITLE :: "Clumsy Oaf of SPLORR!!"
DEFAULT_NAME :: "Olen Kyrpa"
NAME_CAP :: 32
INF :: 1 << 30
MAX_MESSAGES :: 64
MESSAGE_CAP :: 400
AD_DURATION_MS :: 2 * 60 * 1000
HEALTH_MULTIPLIER :: 10 // the original's Grimoire: health is improved 10 at a time

MAZE_COLUMNS :: 4
MAZE_ROWS :: 4
BLUE_ROOM :: 0 // location 0; maze rooms are 1 + column * MAZE_ROWS + row
LOCATION_COUNT :: 1 + MAZE_COLUMNS * MAZE_ROWS
MAX_FEATURES :: 128
GRAVESTONE :: MAX_FEATURES // the feature "index" of the gravestone, which lives apart

SLIME_HEALTH :: 25
SLIME_ATTACK :: 25
SLIME_DEFEND :: 5
SLIME_CRUOR :: 1

Counter :: enum {
	Health,
	Stamina,
	Attack,
	Defend,
	Cruor,
	Clumsiness,
}

counter_names := [Counter]string {
	.Health     = "health",
	.Stamina    = "stamina",
	.Attack     = "attack",
	.Defend     = "defend",
	.Cruor      = "cruor",
	.Clumsiness = "clumsiness",
}

Counter_State :: struct {
	value, min, max: int,
}

Posture :: enum {
	Dodge,
	Fast_Attack,
	Parry,
	Rest,
	Strong_Attack,
}

// Shown to the player as the original showed them, so the next move can be read.
posture_names := [Posture]string {
	.Dodge         = "DODGE",
	.Fast_Attack   = "FAST_ATTACK",
	.Parry         = "PARRY",
	.Rest          = "REST",
	.Strong_Attack = "STRONG_ATTACK",
}

Direction :: enum {
	North,
	East,
	South,
	West,
	In,
	Out,
}

direction_names := [Direction]string {
	.North = "NORTH",
	.East  = "EAST",
	.South = "SOUTH",
	.West  = "WEST",
	.In    = "IN",
	.Out   = "OUT",
}

Feature_Kind :: enum {
	Checkpoint,
	Door,
	Spawner, // hidden; keeps one slime alive
}

Feature :: struct {
	kind:      Feature_Kind,
	location:  int,
	direction: Direction, // doors
	dest:      int, // doors
}

// The slime a spawner keeps. Dead slimes stay dead until a checkpoint's Restore Health.
Slime :: struct {
	alive:   bool,
	health:  int,
	posture: Posture, // what it will do next, shown in combat
	seq:     int, // creation order; enemies are listed and act in this order
}

Gravestone :: struct {
	active:   bool,
	location: int,
	cruor:    int,
}

Link :: enum {
	None,
	Umlaut,
	Itch_Profile,
	Jam,
	Dinobyte,
}

link_urls := [Link]string {
	.None         = "",
	.Umlaut       = "https://umlaut.fyi/",
	.Itch_Profile = "https://thegrumpygamedev.itch.io/",
	.Jam          = "https://itch.io/jam/soulslike-game-jam-3",
	.Dinobyte     = "https://mby.itch.io/dinobyte",
}

Message :: struct {
	text: [MESSAGE_CAP]u8,
	len:  int,
	link: Link,
}

Game :: struct {
	rng:           Rng,
	embarked:      bool,
	name:          [NAME_CAP]u8,
	name_len:      int,
	counters:      [Counter]Counter_State,
	location:      int,
	posture:       Posture,
	checkpoint:    int, // feature index
	features:      [MAX_FEATURES]Feature,
	feature_count: int,
	slimes:        [MAX_FEATURES]Slime, // indexed by spawner feature
	next_seq:      int,
	grave:         Gravestone,
	ad_finish_ms:  f64, // 0 means no ad break
	messages:      [MAX_MESSAGES]Message,
	message_count: int,
}

name_of :: proc(g: ^Game) -> string {
	return string(g.name[:g.name_len])
}

location_name :: proc(l: int) -> string {
	return l == BLUE_ROOM ? "The Blue Room" : "Maze Room"
}

maze_location :: proc(column, row: int) -> int {
	return 1 + column * MAZE_ROWS + row
}

// ---------------------------------------------------------------- messages

message_text :: proc(m: ^Message) -> string {
	return string(m.text[:m.len])
}

clear_messages :: proc(g: ^Game) {
	g.message_count = 0
}

@(private = "file")
next_message :: proc(g: ^Game) -> ^Message {
	if g.message_count == MAX_MESSAGES {
		copy(g.messages[:], g.messages[1:])
		g.message_count -= 1
	}
	m := &g.messages[g.message_count]
	g.message_count += 1
	m.len = 0
	m.link = .None
	return m
}

say :: proc(g: ^Game, format: string, args: ..any) {
	m := next_message(g)
	s := fmt.bprintf(m.text[:], format, ..args)
	m.len = len(s)
}

say_link :: proc(g: ^Game, text: string, link: Link) {
	m := next_message(g)
	m.len = copy(m.text[:], text)
	m.link = link
}

// ---------------------------------------------------------------- counters

counter :: proc(g: ^Game, c: Counter) -> int {
	return g.counters[c].value
}

counter_max :: proc(g: ^Game, c: Counter) -> int {
	return g.counters[c].max
}

counter_set :: proc(g: ^Game, c: Counter, v: int) {
	s := &g.counters[c]
	s.value = clamp(v, s.min, s.max)
}

counter_add :: proc(g: ^Game, c: Counter, delta: int) {
	counter_set(g, c, g.counters[c].value + delta)
}

counter_at_min :: proc(g: ^Game, c: Counter) -> bool {
	return g.counters[c].value <= g.counters[c].min
}

counter_at_max :: proc(g: ^Game, c: Counter) -> bool {
	return g.counters[c].value >= g.counters[c].max
}

counter_capacity :: proc(g: ^Game, c: Counter) -> int {
	return g.counters[c].max - g.counters[c].value
}

// "value/max", the original's GetCounterStatistic.
@(private = "file")
stat :: proc(buf: []u8, g: ^Game, c: Counter) -> string {
	return fmt.bprintf(buf, "%d/%d", counter(g, c), counter_max(g, c))
}

clumsiness_percent :: proc(g: ^Game) -> int {
	return 100 * counter(g, .Clumsiness) / counter_max(g, .Clumsiness)
}

is_dead :: proc(g: ^Game) -> bool {
	return counter_at_min(g, .Health)
}

in_ad :: proc(g: ^Game) -> bool {
	return g.ad_finish_ms != 0
}

// ---------------------------------------------------------------- the world

game_reset :: proc(g: ^Game) {
	rng := g.rng
	g^ = {}
	g.rng = rng
}

@(private = "file")
add_feature :: proc(g: ^Game, f: Feature) -> int {
	assert(g.feature_count < MAX_FEATURES)
	i := g.feature_count
	g.features[i] = f
	g.feature_count += 1
	return i
}

@(private = "file")
add_door :: proc(g: ^Game, location: int, d: Direction, dest: int) {
	add_feature(g, Feature{kind = .Door, location = location, direction = d, dest = dest})
}

@(private = "file")
DELTAS := [4][2]int{{0, -1}, {1, 0}, {0, 1}, {-1, 0}} // North, East, South, West

@(private = "file")
OPPOSITE := [4]int{2, 3, 0, 1}

// A 4 by 4 maze by Prim's algorithm, as the original's TGGD Maze.Generate. open[cell][dir],
// cell = column + row * MAZE_COLUMNS.
@(private = "file")
generate_maze :: proc(g: ^Game) -> (open: [MAZE_COLUMNS * MAZE_ROWS][4]bool) {
	CELLS :: MAZE_COLUMNS * MAZE_ROWS
	neighbor :: proc(cell, d: int) -> int {
		c := cell % MAZE_COLUMNS + DELTAS[d][0]
		r := cell / MAZE_COLUMNS + DELTAS[d][1]
		if c < 0 || r < 0 || c >= MAZE_COLUMNS || r >= MAZE_ROWS {
			return -1
		}
		return c + r * MAZE_COLUMNS
	}
	inside: [CELLS]bool
	queued: [CELLS]bool
	frontier: [CELLS]int
	n := 0
	add_neighbors :: proc(cell: int, inside, queued: ^[CELLS]bool, frontier: ^[CELLS]int, n: ^int) {
		for d in 0 ..< 4 {
			nb := neighbor(cell, d)
			if nb >= 0 && !inside[nb] && !queued[nb] {
				queued[nb] = true
				frontier[n^] = nb
				n^ += 1
			}
		}
	}
	start := rng_range(&g.rng, 0, CELLS - 1)
	inside[start] = true
	add_neighbors(start, &inside, &queued, &frontier, &n)
	for n > 0 {
		pick := rng_range(&g.rng, 0, n - 1)
		cell := frontier[pick]
		frontier[pick] = frontier[n - 1]
		n -= 1
		dirs: [4]int
		dn := 0
		for d in 0 ..< 4 {
			nb := neighbor(cell, d)
			if nb >= 0 && inside[nb] {
				dirs[dn] = d
				dn += 1
			}
		}
		d := dirs[rng_range(&g.rng, 0, dn - 1)]
		open[cell][d] = true
		open[neighbor(cell, d)][OPPOSITE[d]] = true
		inside[cell] = true
		add_neighbors(cell, &inside, &queued, &frontier, &n)
	}
	return
}

@(private = "file")
create_maze :: proc(g: ^Game) {
	open := generate_maze(g)
	for column in 0 ..< MAZE_COLUMNS {
		for row in 0 ..< MAZE_ROWS {
			cell := column + row * MAZE_COLUMNS
			for d in 0 ..< 4 {
				if open[cell][d] {
					dest := maze_location(column + DELTAS[d][0], row + DELTAS[d][1])
					add_door(g, maze_location(column, row), Direction(d), dest)
				}
			}
		}
	}
	entrance := maze_location(rng_range(&g.rng, 0, MAZE_COLUMNS - 1), rng_range(&g.rng, 0, MAZE_ROWS - 1))
	add_door(g, BLUE_ROOM, .Out, entrance)
	add_door(g, entrance, .In, BLUE_ROOM)

	// Spawners: the fewer the ways out, the more slimes (the entrance's IN door counts).
	for l in 1 ..< LOCATION_COUNT {
		doors := 0
		for i in 0 ..< g.feature_count {
			if g.features[i].kind == .Door && g.features[i].location == l {
				doors += 1
			}
		}
		spawners := 0
		switch doors {
		case 1:
			spawners = roll_dice(&g.rng, 1, 4)
		case 2:
			spawners = roll_dice(&g.rng, 1, 3)
		case 3:
			spawners = roll_dice(&g.rng, 1, 2)
		case 4, 5:
			spawners = 1
		}
		for _ in 0 ..< spawners {
			add_feature(g, Feature{kind = .Spawner, location = l})
		}
	}
}

// Brings back every slime whose spawner has none, at full health with a fresh posture.
respawn_enemies :: proc(g: ^Game) {
	for i in 0 ..< g.feature_count {
		if g.features[i].kind == .Spawner && !g.slimes[i].alive {
			g.slimes[i] = Slime {
				alive   = true,
				health  = SLIME_HEALTH,
				posture = random_posture(g),
				seq     = g.next_seq,
			}
			g.next_seq += 1
		}
	}
}

@(private = "file")
random_posture :: proc(g: ^Game) -> Posture {
	return Posture(rng_range(&g.rng, 0, len(Posture) - 1))
}

// Starts a new game. An empty name gets the original's unused default.
embark :: proc(g: ^Game, chosen_name: string) {
	game_reset(g)
	name := chosen_name
	if len(name) == 0 {
		name = DEFAULT_NAME
	}
	g.name_len = copy(g.name[:], name)
	g.embarked = true

	g.checkpoint = add_feature(g, Feature{kind = .Checkpoint, location = BLUE_ROOM})
	g.location = BLUE_ROOM
	g.counters[.Health] = {100, 0, 100}
	g.counters[.Stamina] = {10, 0, 10}
	g.counters[.Attack] = {20, 0, INF}
	g.counters[.Defend] = {5, 0, INF}
	g.counters[.Cruor] = {0, 0, INF}
	// Clumsiness: the best three of six d6, out of 20.
	rolls: [6]int
	for &r in rolls {
		r = roll_dice(&g.rng, 1, 6)
	}
	for i in 1 ..< len(rolls) {
		for j := i; j > 0 && rolls[j] > rolls[j - 1]; j -= 1 {
			rolls[j], rolls[j - 1] = rolls[j - 1], rolls[j]
		}
	}
	g.counters[.Clumsiness] = {rolls[0] + rolls[1] + rolls[2], 0, 20}
	g.posture = .Rest

	create_maze(g)
	respawn_enemies(g)
	clear_messages(g)
	say(g, "Welcome to %s", TITLE)
	look(g)
}

abandon :: proc(g: ^Game) {
	game_reset(g)
}

// ---------------------------------------------------------------- what is here

// Spawner indices of the living slimes here, in the order they were spawned.
enemies_here :: proc(g: ^Game, out: []int) -> int {
	n := 0
	for i in 0 ..< g.feature_count {
		if g.features[i].kind == .Spawner && g.features[i].location == g.location && g.slimes[i].alive && n < len(out) {
			out[n] = i
			n += 1
		}
	}
	for i in 1 ..< n {
		for j := i; j > 0 && g.slimes[out[j]].seq < g.slimes[out[j - 1]].seq; j -= 1 {
			out[j], out[j - 1] = out[j - 1], out[j]
		}
	}
	return n
}

in_combat :: proc(g: ^Game) -> bool {
	buf: [MAX_FEATURES]int
	return enemies_here(g, buf[:]) > 0
}

// Visible features here, in creation order, with the gravestone (if any) last.
features_here :: proc(g: ^Game, out: []int) -> int {
	n := 0
	for i in 0 ..< g.feature_count {
		f := g.features[i]
		if f.kind != .Spawner && f.location == g.location && n < len(out) {
			out[n] = i
			n += 1
		}
	}
	if g.grave.active && g.grave.location == g.location && n < len(out) {
		out[n] = GRAVESTONE
		n += 1
	}
	return n
}

feature_here :: proc(g: ^Game, target: int) -> bool {
	buf: [MAX_FEATURES + 1]int
	n := features_here(g, buf[:])
	for i in 0 ..< n {
		if buf[i] == target {
			return true
		}
	}
	return false
}

feature_name :: proc(buf: []u8, g: ^Game, target: int) -> string {
	if target == GRAVESTONE {
		return fmt.bprintf(buf, "%s's Gravestone", name_of(g))
	}
	f := g.features[target]
	switch f.kind {
	case .Checkpoint:
		return fmt.bprintf(buf, "Checkpoint")
	case .Door:
		return fmt.bprintf(buf, "Door going %s", direction_names[f.direction])
	case .Spawner:
		return fmt.bprintf(buf, "SLIME Spawner")
	}
	return ""
}

look :: proc(g: ^Game) {
	buf: [64]u8
	if in_combat(g) {
		say(g, "Health: %s", stat(buf[:], g, .Health))
		say(g, "Stamina: %s", stat(buf[:], g, .Stamina))
		say(g, "%s is in combat with:", name_of(g))
		enemies: [MAX_FEATURES]int
		n := enemies_here(g, enemies[:])
		for i in 0 ..< n {
			s := g.slimes[enemies[i]]
			say(g, "- Slime(Health: %d/%d, Posture: %s)", s.health, SLIME_HEALTH, posture_names[s.posture])
		}
		return
	}
	say(g, "%s is in %s.", name_of(g), location_name(g.location))
	fs: [MAX_FEATURES + 1]int
	n := features_here(g, fs[:])
	if n == 0 {
		return
	}
	say(g, "Features:")
	for i in 0 ..< n {
		say(g, "- %s", feature_name(buf[:], g, fs[i]))
	}
}

look_action :: proc(g: ^Game) {
	clear_messages(g)
	look(g)
}

status :: proc(g: ^Game) {
	clear_messages(g)
	buf: [64]u8
	say(g, "Status:")
	say(g, "Health: %s", stat(buf[:], g, .Health))
	say(g, "Stamina: %s", stat(buf[:], g, .Stamina))
	say(g, "Clumsiness: %d%%", clumsiness_percent(g))
	say(g, "Cruor: %d", counter(g, .Cruor))
	say(g, "Attack: %d", counter(g, .Attack))
	say(g, "Defend: %d", counter(g, .Defend))
}

describe_feature :: proc(g: ^Game, target: int) {
	clear_messages(g)
	buf: [64]u8
	say(g, "It is a %s.", feature_name(buf[:], g, target))
	if target != GRAVESTONE && g.features[target].kind == .Checkpoint && g.checkpoint == target {
		say(g, "This is %s's current checkpoint.", name_of(g))
	}
}

// ---------------------------------------------------------------- stamina

// The original's SpendStamina and RecoverStamina. They report the change that actually
// happened (the original reported the requested one) and say nothing when nothing changed.
@(private = "file")
change_stamina :: proc(g: ^Game, delta: int) {
	before := counter(g, .Stamina)
	counter_add(g, .Stamina, delta)
	actual := counter(g, .Stamina) - before
	if actual == 0 {
		return
	}
	buf: [32]u8
	say(g, "%s %s %d stamina.", name_of(g), actual > 0 ? "gains" : "loses", abs(actual))
	say(g, "%s now has %s stamina.", name_of(g), stat(buf[:], g, .Stamina))
}

dodge_cost :: proc(g: ^Game) -> int {return max(1, counter_max(g, .Stamina) / 10)}
parry_cost :: proc(g: ^Game) -> int {return max(1, counter_max(g, .Stamina) / 5)}
fast_attack_cost :: proc(g: ^Game) -> int {return max(1, counter_max(g, .Stamina) / 3)}
strong_attack_cost :: proc(g: ^Game) -> int {return max(1, counter_max(g, .Stamina) * 2 / 3)}
rest_recovery :: proc(g: ^Game) -> int {return max(1, counter_max(g, .Stamina) / 2)}

// What a posture costs the avatar; a posture is open only with more stamina than that.
posture_cost :: proc(g: ^Game, p: Posture) -> int {
	switch p {
	case .Dodge:
		return dodge_cost(g)
	case .Parry:
		return parry_cost(g)
	case .Fast_Attack:
		return fast_attack_cost(g)
	case .Strong_Attack:
		return strong_attack_cost(g)
	case .Rest:
		return 0
	}
	return 0
}

can_take_posture :: proc(g: ^Game, p: Posture) -> bool {
	if !g.embarked || is_dead(g) || !in_combat(g) || in_ad(g) {
		return false
	}
	return p == .Rest || counter(g, .Stamina) > posture_cost(g, p)
}

// ---------------------------------------------------------------- combat

// Attack effectiveness in halves: fast attacks are halved by a parry and miss a dodge, strong
// attacks miss a parry and are halved by a dodge, and either does half again on someone resting.
@(private = "file")
effectiveness_halves :: proc(attack, defend: Posture) -> int {
	#partial switch attack {
	case .Fast_Attack:
		#partial switch defend {
		case .Parry:
			return 1
		case .Dodge:
			return 0
		case .Rest:
			return 3
		}
	case .Strong_Attack:
		#partial switch defend {
		case .Parry:
			return 0
		case .Dodge:
			return 1
		case .Rest:
			return 3
		}
	}
	return 2
}

// attack * effectiveness rounded as VB's CInt does: halves go to the even neighbour.
effective_attack :: proc(attack: int, a, d: Posture) -> int {
	doubled := attack * effectiveness_halves(a, d)
	q := doubled / 2
	if doubled % 2 != 0 && q % 2 != 0 {
		q += 1
	}
	return q
}

@(private = "file")
first_enemy :: proc(g: ^Game) -> int {
	buf: [MAX_FEATURES]int
	if enemies_here(g, buf[:]) == 0 {
		return -1
	}
	return buf[0]
}

@(private = "file")
avatar_attacks :: proc(g: ^Game) {
	target := first_enemy(g)
	if target < 0 {
		return
	}
	s := &g.slimes[target]
	who := name_of(g)
	damage := max(effective_attack(counter(g, .Attack), g.posture, s.posture) - SLIME_DEFEND, 0)
	// Clumsiness takes its share: only the room left under the 20 cap gets through.
	damage = damage * counter_capacity(g, .Clumsiness) / counter_max(g, .Clumsiness)
	say(g, "%s does %d damage to Slime.", who, damage)
	if damage <= 0 {
		return
	}
	s.health = max(s.health - damage, 0)
	if s.health > 0 {
		say(g, "Slime has %d/%d health.", s.health, SLIME_HEALTH)
		return
	}
	say(g, "%s kills Slime.", who)
	s.alive = false
	counter_add(g, .Cruor, SLIME_CRUOR)
	say(g, "%s receives %d cruor.", who, SLIME_CRUOR)
	say(g, "%s now has %d cruor.", who, counter(g, .Cruor))
}

@(private = "file")
slime_attacks :: proc(g: ^Game, s: ^Slime) {
	who := name_of(g)
	damage := max(effective_attack(SLIME_ATTACK, s.posture, g.posture) - counter(g, .Defend), 0)
	say(g, "Slime does %d damage to %s.", damage, who)
	if damage <= 0 {
		return
	}
	counter_add(g, .Health, -damage)
	if !is_dead(g) {
		buf: [32]u8
		say(g, "%s has %s health.", who, stat(buf[:], g, .Health))
		return
	}
	say(g, "Slime kills %s.", who)
	// The old gravestone (and its cruor) is lost; a new one holds what was carried.
	g.grave = Gravestone {
		active   = true,
		location = g.location,
		cruor    = counter(g, .Cruor),
	}
	counter_set(g, .Cruor, 0)
}

// One slime acts on the posture it showed, then picks its next one.
@(private = "file")
slime_turn :: proc(g: ^Game, s: ^Slime) {
	switch s.posture {
	case .Rest:
		say(g, "Slime rests.")
	case .Dodge:
		say(g, "Slime dodges.")
	case .Parry:
		say(g, "Slime parries.")
	case .Fast_Attack:
		say(g, "Slime does fast attack.")
		slime_attacks(g, s)
	case .Strong_Attack:
		say(g, "Slime does strong attack.")
		slime_attacks(g, s)
	}
	s.posture = random_posture(g)
}

// The avatar's combat move: Rest, Dodge, Parry, Fast Attack or Strong Attack. Every slime here
// then answers with the posture it showed. Does nothing if the move is not open.
combat :: proc(g: ^Game, p: Posture) {
	if !can_take_posture(g, p) {
		return
	}
	clear_messages(g)
	who := name_of(g)
	g.posture = p
	switch p {
	case .Rest:
		change_stamina(g, rest_recovery(g))
		say(g, "%s rests.", who)
	case .Dodge:
		change_stamina(g, -dodge_cost(g))
		say(g, "%s dodges.", who)
	case .Parry:
		change_stamina(g, -parry_cost(g))
		say(g, "%s parries.", who)
	case .Fast_Attack:
		change_stamina(g, -fast_attack_cost(g))
		say(g, "%s does fast attack.", who)
		avatar_attacks(g)
	case .Strong_Attack:
		change_stamina(g, -strong_attack_cost(g))
		say(g, "%s does strong attack.", who)
		avatar_attacks(g)
	}
	enemies: [MAX_FEATURES]int
	n := enemies_here(g, enemies[:])
	for i in 0 ..< n {
		// The original let the rest keep swinging at a corpse, and a second "death" replaced
		// the gravestone with an empty one.
		if is_dead(g) {
			break
		}
		slime_turn(g, &g.slimes[enemies[i]])
	}
	if !is_dead(g) {
		look(g)
	}
}

// Back to the current checkpoint at full health and stamina. Slimes stay as they are.
respawn :: proc(g: ^Game) {
	if !g.embarked || !is_dead(g) {
		return
	}
	clear_messages(g)
	g.location = g.features[g.checkpoint].location
	say(g, "%s respawns in %s.", name_of(g), location_name(g.location))
	counter_set(g, .Health, counter_max(g, .Health))
	counter_set(g, .Stamina, counter_max(g, .Stamina))
}

// ---------------------------------------------------------------- feature verbs

Verb :: enum {
	Set_Checkpoint,
	Restore_Health,
	Improve_Health,
	Improve_Stamina,
	Improve_Attack,
	Improve_Defend,
	Improve_Clumsiness,
	Enter,
	Restore_Cruor,
}

verb_names := [Verb]string {
	.Set_Checkpoint     = "Set Checkpoint",
	.Restore_Health     = "Restore Health",
	.Improve_Health     = "Improve Health",
	.Improve_Stamina    = "Improve Stamina",
	.Improve_Attack     = "Improve Attack",
	.Improve_Defend     = "Improve Defend",
	.Improve_Clumsiness = "Improve Clumsiness",
	.Enter              = "Enter",
	.Restore_Cruor      = "Restore Cruor",
}

@(private = "file")
CHECKPOINT_VERBS := [?]Verb {
	.Set_Checkpoint,
	.Restore_Health,
	.Improve_Health,
	.Improve_Stamina,
	.Improve_Attack,
	.Improve_Defend,
	.Improve_Clumsiness,
}
@(private = "file")
DOOR_VERBS := [?]Verb{.Enter}
@(private = "file")
GRAVESTONE_VERBS := [?]Verb{.Restore_Cruor}

// The verbs a feature offers, in the original's order.
feature_verbs :: proc(g: ^Game, target: int) -> []Verb {
	if target == GRAVESTONE {
		return GRAVESTONE_VERBS[:]
	}
	if target < 0 || target >= g.feature_count {
		return nil
	}
	switch g.features[target].kind {
	case .Checkpoint:
		return CHECKPOINT_VERBS[:]
	case .Door:
		return DOOR_VERBS[:]
	case .Spawner:
	}
	return nil
}

health_improvement_cost :: proc(g: ^Game) -> int {return counter_max(g, .Health) / HEALTH_MULTIPLIER}
stamina_improvement_cost :: proc(g: ^Game) -> int {return counter_max(g, .Stamina)}
attack_improvement_cost :: proc(g: ^Game) -> int {return counter(g, .Attack)}
defend_improvement_cost :: proc(g: ^Game) -> int {return counter(g, .Defend)}
clumsiness_improvement_cost :: proc(g: ^Game) -> int {return counter_capacity(g, .Clumsiness)}

verb_enabled :: proc(g: ^Game, v: Verb, target: int) -> bool {
	if !g.embarked || is_dead(g) || in_combat(g) || in_ad(g) || !feature_here(g, target) {
		return false
	}
	offered := false
	for fv in feature_verbs(g, target) {
		offered ||= fv == v
	}
	if !offered {
		return false
	}
	cruor := counter(g, .Cruor)
	switch v {
	case .Set_Checkpoint:
		return g.checkpoint != target
	case .Restore_Health:
		return !counter_at_max(g, .Health)
	case .Improve_Health:
		return cruor >= health_improvement_cost(g)
	case .Improve_Stamina:
		return cruor >= stamina_improvement_cost(g)
	case .Improve_Attack:
		return cruor >= attack_improvement_cost(g)
	case .Improve_Defend:
		return cruor >= defend_improvement_cost(g)
	case .Improve_Clumsiness:
		return !counter_at_min(g, .Clumsiness) && cruor >= clumsiness_improvement_cost(g)
	case .Enter, .Restore_Cruor:
		return true
	}
	return false
}

@(private = "file")
spend_cruor :: proc(g: ^Game, cruor: int) {
	say(g, "%s spends %d cruor.", name_of(g), cruor)
	counter_add(g, .Cruor, -cruor)
}

// Does nothing when the verb is not enabled. Messages are cleared first, as in the original.
perform :: proc(g: ^Game, v: Verb, target: int) {
	if !verb_enabled(g, v, target) {
		return
	}
	clear_messages(g)
	who := name_of(g)
	buf: [64]u8
	switch v {
	case .Set_Checkpoint:
		say(g, "%s sets checkpoint.", who)
		g.checkpoint = target

	case .Restore_Health:
		counter_set(g, .Health, counter_max(g, .Health))
		say(g, "%s now has %s health.", who, stat(buf[:], g, .Health))
		respawn_enemies(g)

	case .Improve_Health:
		spend_cruor(g, health_improvement_cost(g))
		say(g, "%s gains %d maximum health.", who, HEALTH_MULTIPLIER)
		g.counters[.Health].max += HEALTH_MULTIPLIER
		say(g, "%s now has %s health.", who, stat(buf[:], g, .Health))

	case .Improve_Stamina:
		spend_cruor(g, stamina_improvement_cost(g))
		say(g, "%s gains 1 maximum stamina.", who)
		g.counters[.Stamina].max += 1
		say(g, "%s now has %s stamina.", who, stat(buf[:], g, .Stamina))

	case .Improve_Attack:
		spend_cruor(g, attack_improvement_cost(g))
		say(g, "%s gains 1 attack.", who)
		counter_add(g, .Attack, 1)
		say(g, "%s now has %d attack.", who, counter(g, .Attack))

	case .Improve_Defend:
		spend_cruor(g, defend_improvement_cost(g))
		say(g, "%s gains 1 defend.", who)
		counter_add(g, .Defend, 1)
		say(g, "%s now has %d defend.", who, counter(g, .Defend))

	case .Improve_Clumsiness:
		spend_cruor(g, clumsiness_improvement_cost(g))
		say(g, "%s loses 1 clumsiness.", who)
		counter_add(g, .Clumsiness, -1)
		say(g, "%s now has %d%% clumsiness.", who, clumsiness_percent(g))

	case .Enter:
		f := g.features[target]
		say(g, "%s goes through %s.", who, feature_name(buf[:], g, target))
		g.location = f.dest
		look(g)

	case .Restore_Cruor:
		say(g, "%s regains %d cruor.", who, g.grave.cruor)
		counter_add(g, .Cruor, g.grave.cruor)
		say(g, "%s now has %d cruor.", who, counter(g, .Cruor))
		g.grave = {}
	}
}

// ---------------------------------------------------------------- ad break

ad_start :: proc(g: ^Game, now_ms: f64) {
	g.ad_finish_ms = now_ms + AD_DURATION_MS
}

ad_show :: proc(g: ^Game, now_ms: f64) {
	clear_messages(g)
	if g.ad_finish_ms > now_ms {
		left := int((g.ad_finish_ms - now_ms) / 1000)
		say(g, "Time left in ad break: %02d:%02d", left / 60, left % 60)
		say(g, "(This is a turn based game. As such, this counter will not automatically change. You have to click the OK button to refresh.)")
		say_link(g, "For all yer umlauting needs! umlaut.fyi", .Umlaut)
	} else {
		say(g, "Ad break is complete! You may return to yer metaphor!")
		g.ad_finish_ms = 0
	}
}
