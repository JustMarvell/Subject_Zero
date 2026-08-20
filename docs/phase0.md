# Subject Zero — Phase 0 Consolidation

Status as of end of Phase 0. This document tracks what was actually built, where it
deviated from the original design plan, and what's still an open TODO before Phase 1
evaluation data can be trusted. Written to be pulled from directly when drafting the
thesis methodology chapter later.

---

## 1. Architecture deviations from the original plan

### 1.1 Rule-based baseline is now the single source of truth for labels

**Original plan:** Python would compute the `stress_score` proxy-label formula
independently, and Unity's rule-based baseline would be a *separate* reimplementation
of the same formula — two implementations, flagged at the time as a "can silently
drift apart" risk.

**What was actually built:** Unity computes `stress_score` and the
`too_easy/balanced/too_hard` label at the moment each telemetry sample is logged
(`StressScoreCalculator.cs`), using weights loaded from a single shared JSON file
(`Assets/StreamingAssets/DDA/stress_weights.json`). This one computation now serves
three roles at once:
- the rule-based baseline / control condition
- the ground-truth label written into every training row
- a runtime fallback score before any ML model is wired in

Python's `train.py` no longer computes labels — it just trains on the
`difficulty_label` column Unity already wrote. **One implementation, not two.**

**Why this matters for the thesis:** this is actually a *stronger* methodological
claim than the original plan — "the baseline and the label source are provably
identical" is a clean thing to state in a methods section, rather than something that
needs a drift-consistency argument.

### 1.2 DDA Controller only adjusts knobs it has systems for

**Original plan (5 knobs):** enemy spawn rate/aggression/speed, resource
availability, puzzle complexity/hints, jump scare frequency, checkpoint frequency,
visibility/fog.

**What's actually wired up (2 of those, mapped to what exists):**
| Knob | Status |
|---|---|
| Enemy speed (patrol/alert/search/chase) | ✅ Implemented — `EnemyController.ApplyDifficultyAdjustment()` |
| Detection radius (vision range) | ✅ Implemented — same method |
| Resource drop rate | ⏸ Deferred — no resource-spawning system exists yet |
| Encounter/spawn frequency | ⏸ Deferred — no spawner system yet |
| Fog/visibility density | ⏸ Deferred — no environmental fog system yet |
| Puzzle complexity/hints | ⏸ Deferred — Zone 3 content doesn't exist yet |
| Checkpoint frequency | ⏸ Deferred — no checkpoint/save system yet |

**Design detail carried over correctly:** adjustments only ever apply while every
tracked entity is in a "safe" state (`Patrol` or `Lost`), never mid-chase — matches
the original design note that adjusting difficulty during active pursuit would feel
like the game cheating.

### 1.3 DDA adjustment uses classifier probabilities, not a regression score

Confirmed earlier in planning: classifier-only (Decision Tree / Random Forest), no
regression model for now. The knob formulas still expect a `[-1, +1]`-ish score, so:

```
adjustment_score = P(too_hard) - P(too_easy)
```

Currently, since there's no trained classifier yet, `stress_score` from the rule-based
formula is used directly in its place — same shape of value, same downstream formulas.
Once a classifier exists, its output slots in without changing `ApplyDifficultyAdjustment()`.

### 1.4 Telemetry feature set changed after locking in binary health

**Original 7 features:** `death_rate`, `hit_taken_rate`, `avg_reaction_time`,
`hide_ratio`, `movement_erraticism`, `resource_usage_rate`, `idle_ratio`.

**Problem:** binary catch/fail health (locked in during mechanics design) makes
`hit_taken_rate` redundant with `death_rate` — a "hit" and a "death" became the same
event.

**Resolution:** `hit_taken_rate` was replaced with `near_miss_rate` — logged whenever
a Chase state ends *without* a catch (`EnemyChaseState.Exit()`). This is arguably a
better signal than the original anyway: it captures close calls distinctly from hard
failures, giving the model more graduated data despite binary health.

**Current feature set (7, as logged in every `.jsonl` row):**
`death_rate`, `near_miss_rate`, `avg_reaction_time`, `hide_ratio`,
`movement_erraticism`, `resource_usage_rate`, `idle_ratio`.

### 1.5 Reaction time is inferred, not explicitly instrumented per-event

Not fully specified in the original design. What got built: `EnemyAlertState.Enter()`
calls `TelemetryManager.ArmReactionWindow()` the moment the entity first registers the
player (sight or sound). `TelemetryManager` then watches for the player's *next*
locomotion-mode change or crouch toggle as the "reaction," and logs the elapsed time.
This is a reasonable proxy but a real methodological assumption worth stating
explicitly in the thesis — it assumes the player's first mode/stance change after a
threat appears **is** their reaction to that threat, which won't always be true (e.g.
a player who was already mid-sprint when Alert triggers won't register a new "reaction"
until their next state change).

---

## 2. New mechanics added that weren't in the original design

### 2.1 Flashlight-on suppresses hiding effectiveness

Not part of the original plan — emerged naturally while building the flashlight
system. `EnemyPerception.CanSeePlayer()` normally returns `false` immediately for a
hidden player, **except** when their flashlight is on, in which case normal
sight-cone/range checks still apply. This creates a genuine risk/reward decision
(light lets you see, but can blow cover while hiding) and links three previously
separate systems (interaction, hiding, flashlight) into one interaction. Worth
mentioning in the thesis as an emergent design outcome rather than upfront design.

### 2.2 Flashlight boosts effective entity vision range while on

`EnemyConfig.flashlightVisionMultiplier` (default 1.5×) — the entity can spot the
player from further away while their flashlight is lit, independent of the hiding
interaction above. Currently set from playtesting feel (`flickerAmount` was manually
tuned to 1.2 during testing), not from data — same "TODO: calibrate from playtest
data" category as everything else in Section 4.

### 2.3 Entity is physically excluded from crouch-only passages

Not an explicit mechanic, but a deliberate emergent consequence of NavMesh bake
settings: baking with Agent Height ~1.8m makes any passage with less vertical
clearance (e.g. the greybox's low passage at 1.2m) unwalkable for the entity, while
the player can still get through by crouching. Confirmed working in the greybox test.
Flagged at the time as worth watching for "is this too safe/exploitable" once real
content exists — not yet re-evaluated.

---

## 3. Default decisions made without explicit design discussion

These were reasonable defaults picked to keep moving, not deliberated the way most
other decisions were — listed here so they're visible and easy to revisit:

| Decision | Default chosen | Easy to change? |
|---|---|---|
| Crouch input | Toggle (press C), not hold | Yes — one line in `PlayerInputReader` |
| Flashlight input | Toggle (press F), not hold | Yes — same pattern |
| Direction-change threshold for `movement_erraticism` | 35° between frames | Yes — `TelemetryManager` Inspector field |
| Flashlight battery capacity | 180 seconds full charge | Yes — `FlashlightConfig` asset |
| Battery pickup charge amount | 60 seconds per pickup | Yes — per-object field on `BatteryPickup` |

---

## 4. Confirmed TODOs (unchanged from earlier planning, still open)

Nothing in this section is new — restating what was already flagged, so it isn't lost:

- **`stress_weights.json` weights and thresholds** — currently uniform/symmetric
  placeholders. Needs deriving from real playtest data (percentile-based thresholds,
  as discussed).
- **`Normalize()` ceiling constants** in `StressScoreCalculator` (6, 10, 20, 2000ms) —
  also placeholders, same calibration TODO.
- **Sanity/stress meter** — explicitly flagged as out of scope, revisit later.
- **Regression model** — stretch goal only if time allows; classifier is primary.
- **Ending profile thresholds** — reuse the same `too_easy/balanced/too_hard`
  boundaries once calibrated; not yet implemented since no ending logic exists yet.

---

## 5. What Phase 0 actually delivered (confirmed working end-to-end)

- First-person player controller: walk/crouch/sneak/sprint, no jump, state-machine
  architecture reused from the souls-like project's `IState`/`StateMachine` core.
- Entity AI: 5-state (`Patrol → Alert → Search → Chase → Lost`), NavMesh-driven,
  vision cone + hearing range perception.
- Telemetry: 7-feature rolling-window sampling every 10s, written to per-session
  `.jsonl` files, with automatic label computation.
- Rule-based DDA baseline: adjusts entity speed and vision range based on
  `stress_score`, only during safe states.
- Interaction system: raycast + `IInteractable`, with on-screen prompt UI.
- Flashlight/battery system: toggle, drain, low-battery flicker, pickup recharge,
  detection interactions with hiding and vision range.
- Greybox test room proving all of the above work together (patrol hub, sightline
  pillar, crouch-only passage, two hiding spots, dead-end alcove).

## 6. Known environment gotcha (for future reference)

Enter Play Mode Settings must be on **Reload Domain and Scene** (or code must avoid
depending on one-time `ScriptableObject` initialization flags surviving across Play
sessions). A stale `_initialized` guard in `PlayerInputReader` caused a full input
outage that took a long debugging session to trace — root cause was cached action
references silently not being re-fetched between Play sessions. Worth remembering if
similar "worked once, broke on the second try" symptoms show up again in the
telemetry or DDA systems, which have their own instance/session state.