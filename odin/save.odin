package clumsy

import "core:encoding/json"
import "core:fmt"
import "core:mem"
import "core:strconv"

// Saving (new in the port; the original never saved): a versioned JSON document. Enums are
// stored as integers and every field is range-checked on load; a failed load leaves the
// game untouched.

SAVE_VERSION :: 1

Save_Message :: struct {
	text: string,
	link: int,
}

Save_Feature :: struct {
	kind, location, direction, dest: int,
}

Save_Slime :: struct {
	alive:                 bool,
	health, posture, seq: int,
}

Save_Data :: struct {
	version:      int,
	rng:          string, // hex, JSON numbers lose the top bits of a u64
	embarked:     bool,
	name:         string,
	counters:     [len(Counter)][3]int, // value, min, max (maxima grow at the checkpoint)
	location:     int,
	posture:      int,
	checkpoint:   int,
	features:     []Save_Feature,
	slimes:       []Save_Slime, // one per feature, by index
	next_seq:     int,
	grave:        [3]int, // active (0/1), location, cruor
	ad_finish_ms: f64,
	messages:     []Save_Message,
}

// Scratch space for encode and decode (never both at once).
@(private = "file")
scratch: [1 << 17]u8

@(private = "file")
scratch_allocator :: proc() -> mem.Allocator {
	@(static) arena: mem.Arena
	mem.arena_init(&arena, scratch[:])
	return mem.arena_allocator(&arena)
}

// The result lives in a package buffer until the next save_encode or save_decode.
save_encode :: proc(g: ^Game) -> (text: string, ok: bool) {
	alloc := scratch_allocator()
	d: Save_Data
	d.version = SAVE_VERSION
	d.rng = fmt.aprintf("%x", g.rng.state, allocator = alloc)
	d.embarked = g.embarked
	d.name = name_of(g)
	for c in Counter {
		s := g.counters[c]
		d.counters[int(c)] = {s.value, s.min, s.max}
	}
	d.location = g.location
	d.posture = int(g.posture)
	d.checkpoint = g.checkpoint
	d.features = make([]Save_Feature, g.feature_count, alloc)
	d.slimes = make([]Save_Slime, g.feature_count, alloc)
	for i in 0 ..< g.feature_count {
		f := g.features[i]
		d.features[i] = {int(f.kind), f.location, int(f.direction), f.dest}
		s := g.slimes[i]
		d.slimes[i] = {s.alive, s.health, int(s.posture), s.seq}
	}
	d.next_seq = g.next_seq
	d.grave = {g.grave.active ? 1 : 0, g.grave.location, g.grave.cruor}
	d.ad_finish_ms = g.ad_finish_ms
	d.messages = make([]Save_Message, g.message_count, alloc)
	for i in 0 ..< g.message_count {
		m := &g.messages[i]
		d.messages[i] = Save_Message{message_text(m), int(m.link)}
	}

	data, err := json.marshal(d, {}, alloc)
	if err != nil {
		return "", false
	}
	return string(data), true
}

@(private = "file")
in_range :: proc(v, lo, hi_exclusive: int) -> bool {
	return v >= lo && v < hi_exclusive
}

// Replaces g only if the whole document is valid.
save_decode :: proc(g: ^Game, text: string) -> bool {
	alloc := scratch_allocator()
	d: Save_Data
	if json.unmarshal(transmute([]u8)text, &d, allocator = alloc) != nil {
		return false
	}
	if d.version != SAVE_VERSION {
		return false
	}
	out: Game
	state, state_ok := strconv.parse_u64_of_base(d.rng, 16)
	if !state_ok {
		return false
	}
	out.rng.state = state
	out.embarked = d.embarked
	if len(d.name) == 0 || len(d.name) > NAME_CAP {
		return false
	}
	out.name_len = copy(out.name[:], d.name)

	// Minima never change; maxima only grow from where they start.
	least_max := [Counter]int {
		.Health     = 100,
		.Stamina    = 10,
		.Attack     = INF,
		.Defend     = INF,
		.Cruor      = INF,
		.Clumsiness = 20,
	}
	for c in Counter {
		v := d.counters[int(c)]
		value, lo, hi := v[0], v[1], v[2]
		if lo != 0 || hi < least_max[c] || hi > INF || value < lo || value > hi {
			return false
		}
		if c == .Clumsiness && hi != 20 {
			return false
		}
		out.counters[c] = Counter_State{value, lo, hi}
	}
	if !in_range(d.location, 0, LOCATION_COUNT) || !in_range(d.posture, 0, len(Posture)) {
		return false
	}
	out.location = d.location
	out.posture = Posture(d.posture)

	if len(d.features) > MAX_FEATURES || len(d.slimes) != len(d.features) {
		return false
	}
	out.feature_count = len(d.features)
	for f, i in d.features {
		if !in_range(f.kind, 0, len(Feature_Kind)) ||
		   !in_range(f.location, 0, LOCATION_COUNT) ||
		   !in_range(f.direction, 0, len(Direction)) ||
		   !in_range(f.dest, 0, LOCATION_COUNT) {
			return false
		}
		out.features[i] = Feature{Feature_Kind(f.kind), f.location, Direction(f.direction), f.dest}
		s := d.slimes[i]
		if !in_range(s.posture, 0, len(Posture)) || !in_range(s.health, 0, SLIME_HEALTH + 1) || s.seq < 0 {
			return false
		}
		if s.alive && (Feature_Kind(f.kind) != .Spawner || s.health == 0) {
			return false
		}
		out.slimes[i] = Slime{s.alive, s.health, Posture(s.posture), s.seq}
	}
	if !in_range(d.checkpoint, 0, out.feature_count) || out.features[d.checkpoint].kind != .Checkpoint {
		return false
	}
	out.checkpoint = d.checkpoint
	if d.next_seq < 0 {
		return false
	}
	out.next_seq = d.next_seq
	if !in_range(d.grave[0], 0, 2) || !in_range(d.grave[1], 0, LOCATION_COUNT) || !in_range(d.grave[2], 0, INF + 1) {
		return false
	}
	out.grave = Gravestone{d.grave[0] == 1, d.grave[1], d.grave[2]}
	if d.ad_finish_ms < 0 {
		return false
	}
	out.ad_finish_ms = d.ad_finish_ms
	if len(d.messages) > MAX_MESSAGES {
		return false
	}
	for s in d.messages {
		if !in_range(s.link, 0, len(Link)) {
			return false
		}
		m := &out.messages[out.message_count]
		out.message_count += 1
		m.len = copy(m.text[:], s.text)
		m.link = Link(s.link)
	}
	g^ = out
	return true
}
