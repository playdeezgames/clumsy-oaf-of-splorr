#+build !js
package clumsy

import "core:strings"
import "core:testing"

@(private = "file")
swap :: proc(s, old, new: string) -> string {
	out, _ := strings.replace(s, old, new, 1, context.temp_allocator)
	return out
}

@(test)
save_round_trips :: proc(t: ^testing.T) {
	g: Game
	rng_seed(&g.rng, 99)
	embark(&g, "Oaf")
	counter_set(&g, .Cruor, 12)
	g.counters[.Health].max = 120
	g.grave = Gravestone{active = true, location = 3, cruor = 4}
	g.slimes[g.feature_count - 1].alive = false
	ad_start(&g, 5000)

	text, ok := save_encode(&g)
	testing.expect(t, ok)
	copy_ := strings.clone(text, context.temp_allocator)
	back: Game
	testing.expect(t, save_decode(&back, copy_))
	testing.expect(t, back == g)
}

@(test)
bad_saves_are_refused :: proc(t: ^testing.T) {
	g: Game
	rng_seed(&g.rng, 5)
	embark(&g, "Oaf")
	text, _ := save_encode(&g)
	good := strings.clone(text, context.temp_allocator)

	keep := g
	bad := []string {
		"",
		"{",
		swap(good, `"version":1`, `"version":2`),
		swap(good, `"location":0`, `"location":99`),
		swap(good, `"checkpoint":0`, `"checkpoint":1`), // a door
		swap(good, `"name":"Oaf"`, `"name":""`),
	}
	for b in bad {
		testing.expect(t, b != good || b == "")
		testing.expect(t, !save_decode(&g, b))
		testing.expect(t, g == keep)
	}
	testing.expect(t, save_decode(&g, good))
}
