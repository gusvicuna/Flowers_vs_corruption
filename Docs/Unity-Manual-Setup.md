# Unity Manual Setup

Editor work (assets, prefabs, scenes) is done by hand — scripts only create code, never assets.
Each feature appends its own section here.

---

## Feature 1 — World Ring

Everything below lives under `Assets/_Project/`. Sprite pixels-per-unit does **not** matter:
`SpriteFitter` scales every sprite to the size defined in `GameConfig`.

### 1. Placeholder sprites → `Art/Placeholders/`

- `Assets > Create > 2D > Sprites > Square`
- `Assets > Create > 2D > Sprites > Circle`

Leave them white — all coloring comes from `TileVisuals` tints, so Irene can later drop real
sprites into that one asset without touching prefabs or scenes.

### 2. Data assets → `Data/`

**GameConfig** — `Assets > Create > FvC > Game Config`, name it `GameConfig`. Defaults are fine:

| Field | Value |
| --- | --- |
| Tile Width | 1 |
| Tile Height | 0.6 |
| Tile Overlap Scale | 1.08 |

**TileVisuals** — `Assets > Create > FvC > Tile Visuals`, name it `TileVisuals`.
Set `Entries` size to **4**, sprite = `Square` on all of them:

| # | Type | Tint |
| --- | --- | --- |
| 0 | Soil | `8B5A2B` |
| 1 | Rock | `7F7F7F` |
| 2 | House | `4FA9E0` |
| 3 | CorruptionBase | `6B2FA0` |

Then: `Corruption Overlay Sprite` = `Square`, `Corruption Color` = `3D0A5E` with **alpha ≈ 180**.

**WorldLayout** — `Assets > Create > FvC > World Layout`, name it `WorldLayout_Default`.
Under *Generator*: Tile Count `24`, Rock Count `5`, Seed `12345`.
Then click the **gear icon** at the top-right of the component → **Generate Default Layout**.
The `Tiles` array fills with 24 entries: House at 0, CorruptionBase at 12, 5 rocks scattered.

> Re-run *Generate Default Layout* any time you change the generator values. This is the knob for
> prototyping different world sizes — try 12 or 36 later to confirm everything still fits.

### 3. `Tile` prefab → `Prefabs/`

Build it in an empty scene, then drag it into `Prefabs/` and delete it from the scene.

```
Tile                     ← empty GameObject + TileView component
├── Ground               ← SpriteRenderer, sprite = Square, Sorting Order = 10
└── CorruptionOverlay    ← SpriteRenderer, sprite = Square, Sorting Order = 11, component UNCHECKED
```

On `TileView`, drag `Ground` into the **Ground** field and `CorruptionOverlay` into the
**Corruption Overlay** field.

> The root stays at scale 1 on purpose — it is the tile's logical anchor (position and rotation on
> the ring). Only the sprite children get resized. Crops and the player will attach to the root later.

### 4. `World` prefab → `Prefabs/`

```
World                    ← empty GameObject + WorldView component
└── PlanetCore           ← SpriteRenderer, sprite = Circle, Sorting Order = 0, color 3B2A20
```

Fill the `WorldView` fields:

| Field | Value |
| --- | --- |
| Layout | `WorldLayout_Default` |
| Config | `GameConfig` |
| Visuals | `TileVisuals` |
| Tile Prefab | `Tile` prefab |
| Planet Core | the `PlanetCore` child |
| Core Bleed | 0.1 |

### 5. `Game` scene → `Scenes/`

1. `File > New Scene` → template **Basic 2D (URP)** (it already includes a *Global Light 2D*, which
   URP's lit sprites need — without it everything renders black).
2. Save as `Assets/_Project/Scenes/Game.unity`.
3. Drag the `World` prefab in, position `(0, 0, 0)`.
4. Main Camera: Projection **Orthographic**, Size **5**, position `(0, 0, -10)`,
   Background type **Solid Color**, color `0B0E1A`.
5. `File > Build Profiles` → **Scene List** → add `Game.unity`.

### 6. Verify

Press **Play**. Expected: a closed ring of 24 tiles around a dark circle — blue House at the top,
purple CorruptionBase at the bottom, 5 gray rocks scattered, no gaps between tiles.

- Gaps showing? Raise `Tile Overlap Scale` in `GameConfig` (1.08 → 1.12).
- Test the model → event → view path: with the game **running**, select the `World` object, click the
  gear on the `WorldView` component → **Debug: Corrupt Random Tile**. A dark purple overlay should
  appear on a random tile each click.

### 7. Tests

`Window > General > Test Runner` → **EditMode** tab → **Run All**. 17 tests, all green.

### Addendum — initial corruption (added after first playtest)

`WorldLayout` has a new field: **Initial Corruption Per Side** (default `1`).

- Select `Data/WorldLayout_Default` and check the value — `1` means the CorruptionBase tile plus
  1 tile on each side start corrupted (3 purple-overlay tiles at the bottom when you press Play).
- Tune it freely for testing: `0` = only the base, `3` = a 7-tile corrupted arc, etc.

### Addendum — guaranteed soil next to the house

The `WorldLayout` *Generator* section has a new field: **Soil Next To House Per Side**
(default `1`) — how many tiles on each side of the house the generator keeps rock-free,
i.e. the player's guaranteed starting farmland.

- It only takes effect when you re-run **gear icon → Generate Default Layout** on
  `Data/WorldLayout_Default` (it's a generator input, not a live value).
- At the default `1` the generated layout is identical to before — no need to regenerate
  unless you change the value.
- Test count is now **23**.

---

## Feature 2 — Player Movement & Camera

The code drives every position and size from `GameConfig` (`SpriteFitter` again), so as with
tiles: never hand-set positions or scales on the sprites — only what's listed below.

### 1. Data assets → `Data/`

**GameConfig** — two new sections appeared with correct defaults. Just verify:

| Field | Value |
| --- | --- |
| Walk Speed Tiles Per Second | 2 |
| Player Size | (0.6, 1) |
| Camera Ortho Size | 2.5 |
| Camera Smooth Time | 0.15 |
| Camera Vertical Offset | 1 |

**TileVisuals** — new *Highlight* section:

| Field | Value |
| --- | --- |
| Highlight Sprite | `Square` |
| Highlight Color | `FFE97F`, **alpha ≈ 90** |

### 2. Input asset — nothing to do, just check

Select `Assets/InputSystem_Actions` and open it: the **Player** map now has only **Move**
(A/D, arrows, stick, d-pad) and **Action** (S, ↓, gamepad South). The **UI** map is untouched.
Don't edit anything.

### 3. `Player` prefab → `Prefabs/`

Build in the scene, drag to `Prefabs/`, **keep the instance in the scene**:

```
Player                   ← empty GameObject + PlayerController
└── Sprite               ← SpriteRenderer, sprite = Square, color F2E9DC, Sorting Order = 20
```

Wire on `PlayerController`:

| Field | Value |
| --- | --- |
| World View | *leave empty in the prefab* (wired per scene) |
| Config | `GameConfig` |
| Sprite | the `Sprite` child |
| Move Action | expand `Assets/InputSystem_Actions` in the Project window (arrow on the asset) → drag **Player/Move** |
| Tile Action | same → drag **Player/Action** |

### 4. Scene changes (`Game.unity`)

1. On the **Player instance**: set **World View** = the scene's `World` object. (Root position
   doesn't matter — code snaps it every frame.)
2. New empty GameObject `TileHighlight` at the scene root:
   - Add `SpriteRenderer`: leave the sprite **empty** (code assigns it), **Sorting Order = 12**.
   - Add `TileHighlightView` and wire: Player = `Player` instance · World View = `World` ·
     Visuals = `TileVisuals` · Config = `GameConfig` · Marker = its own SpriteRenderer.
3. `Main Camera`: add the `CameraRig` component and wire: Player = `Player` instance ·
   World View = `World` · Config = `GameConfig` · Camera = its own Camera component.
   Leave position/size as they are — the rig drives them in Play Mode.

No prefab for `TileHighlight` or the camera: they only reference scene objects, so a prefab
buys nothing. Save the scene.

### 5. Verify (Play Mode)

- The player stands upright on the House tile (top), surface across the lower third of the
  screen, sky above.
- Hold **D / → / stick right**: the player walks right and the planet rotates underneath;
  the camera follows with a slight ease. **A / ←** mirrors. Do a full lap in each direction —
  no hitch when crossing the bottom of the planet (the 0°/360° seam).
- The warm highlight sits under the player and hops tile by tile.
- Press **S / ↓ / gamepad South**: the Console logs the tile index and the tile toggles its
  purple corruption overlay (temporary debug action, removed when planting arrives).
- Gamepad: half-deflecting the stick still walks at full speed (constant-speed rule).

### 6. Tests

Test Runner → EditMode → Run All → **37 green**.

---

## Feature 3 — Day/Night Time System

Role flip for this feature: **Gus writes the code and does the editor work**; the AI delivered
the plan, the pre-written tests (`Tests/DayNightClockTests.cs`), and this checklist. The tests
are the spec — green means the clock is correct.

### 0. Code order (red → green)

1. Create `Scripts/Core/DayPhase.cs` (enum `Dawn, Day, Night`) and the `DayNightClock` skeleton
   with the exact API from the plan (bodies `throw new NotImplementedException()`), so the test
   assembly compiles.
2. Test Runner: 15 new red, 37 old green.
3. Implement `DayNightClock` until **52 green**.
4. Only then write `TimeSystem`, `SkyView`, `TimeDebugHud` (no tests — verified in Play Mode).
5. asmdef: add `Unity.RenderPipelines.Universal.2D.Runtime` to `Scripts/FlowersVsCorruption.asmdef`
   references, or `Light2D` in `SkyView` won't compile (CS0246).

### 1. Data — `GameConfig.asset`

New sections appear once the code lands (*Time*: dawn/day/night durations; *Sky*: a sky color
and a light color per phase, plus **Sky Transition Seconds** — how long the tint takes to ease
into each phase's palette). Code defaults are sensible; current tuning lives in the asset
(durations were hand-tuned to 10/80/40 while playtesting).

### 2. Scene changes (`Game.unity`)

New empty GameObject `TimeSystem` at the scene root with three components:

| Component | Field | Value |
| --- | --- | --- |
| `TimeSystem` | Config | `GameConfig` |
| `SkyView` | Time / Camera / Global Light / Config | `TimeSystem` (same GO) / `Main Camera` / `Global Light 2D` (existing scene object) / `GameConfig` |
| `TimeDebugHud` | Time | `TimeSystem` (same GO) |

Nothing else by hand. Save the scene.

### 3. Verify (Play Mode)

- Starts at **Dawn of day 1**: warm tint for ~3 s → sky lerps to light blue (Day) → after 90 s
  it darkens (Night) → after 30 s warm again and the HUD reads **Day 2**.
- HUD (top-left) counts down the current phase; sprites' lighting follows the Global Light tint.
- Tuning: sky colors and lerp speed apply live in Play Mode (read every frame). Durations are
  captured when the clock is created — change them and **re-enter Play Mode** (set Day Duration
  to 10 for fast iteration while testing).

### 4. Tests

Test Runner → EditMode → Run All → **52 green**.

---

## Feature 4 — Corruption Spread

Same split as Feature 3: **Gus writes the code and does the editor work**; the tests
(`Tests/CorruptionSystemTests.cs`) are the pre-written spec.

### 0. Code order (red → green)

1. Create `Scripts/World/CorruptionSystem.cs` with the exact API from the plan
   (ctor `WorldGrid`, `Func<int,bool> IsTileGuarded`, `event Action<int> HouseCorrupted`,
   `void Spread(int waves)`; bodies `throw new NotImplementedException()`) so tests compile.
2. Test Runner: new red, old green.
3. Implement until **all green** (69 as of this feature). Key semantics the tests enforce:
   - A wave collects every clean, unguarded tile with ≥1 corrupted neighbor **first**, then
     corrupts them all via `grid.SetCorrupted(i, true)` — collect-then-apply, or one wave
     snowballs into an avalanche.
   - Use `grid.NextIndex`/`PreviousIndex` for neighbors (ring wraparound), never `i±1`.
   - `HouseCorrupted` comes from subscribing to `grid.TileChanged` in the constructor — NOT
     from inside `Spread` — so corruption from any source (debug toggle included) triggers it.
4. Then `CorruptionController` (MonoBehaviour): create the system in `Start` with
   `_worldView.Grid`, subscribe `_timeSystem.NightStarted` → `System.Spread(_config.CorruptionSpreadPerNight)`,
   unsubscribe in `OnDestroy`, and log something loud in a `HouseCorrupted` handler
   (placeholder for the future lose screen).
5. `GameConfig`: `[Header("Corruption")] int _corruptionSpreadPerNight` (`Min(0)`, default 1)
   + getter.

### 1. Data — `GameConfig.asset`

Verify the new *Corruption* section: **Corruption Spread Per Night = 1**.

### 2. Scene changes (`Game.unity`)

Add the `CorruptionController` component to the existing `TimeSystem` GameObject and wire:
World View = `World` · Time System = `TimeSystem` (same GO) · Config = `GameConfig`.
Save the scene.

### 3. Verify (Play Mode)

Shorten durations first for fast iteration (Day 10 / Night 5, re-enter Play Mode):

- The instant each night falls, the corrupted arc grows by 1 tile on each end.
- Corrupt a lone far-away tile with the debug Action (S) → next night that island grows on
  both sides too.
- Let corruption reach the house → Console logs the `HouseCorrupted` placeholder, exactly once.
- Set Spread Per Night = 3 → each front jumps 3 tiles per night.

### 4. Tests

Test Runner → EditMode → Run All → **69 green**.

---

## Feature 5 — Planting & Farming

Same split: **Gus writes code and editor**; the pre-written spec lives in
`Tests/FarmingSystemTests.cs` plus additions to `CorruptionSystemTests` and `WorldGridTests`.
The `.inputactions` already has the new **PreviousSeed** (Q / left shoulder) and **NextSeed**
(E / right shoulder) actions — nothing to edit there.

### 0. Code order (red → green)

1. Skeletons so the test assembly compiles, exact signatures from the plan:
   - `Scripts/Farming/CropDefinition.cs` (SO + `PlantableGround` enum `{ CleanSoil, CorruptedSoil }`)
   - `Scripts/Farming/Crop.cs`
   - `Scripts/Farming/FarmingSystem.cs`
   - `WorldGrid`: add `GetCrop(int)` / `SetCrop(int, Crop)` + `Tile.Crop`
   - `CorruptionSystem`: add `event Action<int> SpreadBlocked`
2. Test Runner: ~34 new red, 69 old green. Implement until **103 green**.
3. Implementation notes the tests enforce:
   - `SetCrop` fires `TileChanged` (same-crop set is silent; removal fires) — the view depends on it.
   - **FarmingSystem subscribes to `grid.TileChanged` in its constructor** and kills the occupant
     when a tile becomes corrupted — the tests corrupt tiles directly and expect the crop to die
     with no extra calls (no public "OnCorruptionEntered" needed).
   - In `CorruptionSystem.FindTilesToCorrupt`, check **adjacency before guard**: a guarded tile
     far from any front must not receive `SpreadBlocked`.
   - `OnDawn` order: cleanse → growth → watered reset (a flower finishing growth this dawn
     cleanses only from the next dawn).
   - A flower dying to `OnSpreadBlocked` leaves its tile CLEAN — the killing wave is spent.
4. Then the presentation layer:
   - `FarmingController` (creates the system, wires guard + `SpreadBlocked` into
     `CorruptionController.System`, subscribes `DawnStarted` and `TileActionPerformed`,
     seed cycling, harvest counters).
   - ⚠️ Make `CorruptionController.System` **lazy** (getter creates it from `_worldView.Grid`
     if null; `Start` just ensures creation) — `Start` order between controllers is undefined.
   - **Delete the TEMP corruption-toggle block** in `PlayerController.OnTileAction` — otherwise
     every plant also flips the tile's corruption.
   - `TileView`: third child renderer for the crop (sprite/tint from the definition, height
     scaled by stage ~0.35→0.8 of tile height via `SpriteFitter`), ground darkened while watered.
   - `FarmDebugHud` (TEMP): selected seed + harvest counts.

### 1. Data assets → `Data/Crops/`

Create three via `Assets > Create > FvC > Crop Definition`:

| Field | Flower | FoodCrop | CorruptedCrop |
| --- | --- | --- | --- |
| Display Name | Flower | Food | Corrupted |
| Sprite | `Circle` | `Circle` | `Circle` |
| Tint | `E84D8A` | `7BC950` | `9B30B0` |
| Growth Stages | 3 | 3 | 3 |
| Plantable On | Clean Soil | Clean Soil | Corrupted Soil |
| Guards When Grown | ✔ | ✘ | ✘ |
| Cleanses When Grown | ✔ | ✘ | ✘ |
| Grown Health | 3 | – | – |
| Harvestable | ✘ | ✔ | ✔ |

### 2. Prefab `Tile`

Add a third child `Crop`: SpriteRenderer, sprite **empty** (code assigns it), **Sorting Order = 15**.
Wire it into the `TileView` **Crop** field. (Ground = 10, Overlay = 11, Crop = 15, Highlight = 12,
Player = 20 — the crop draws above the tile but under the player.)

### 3. Scene changes (`Game.unity`)

On the `TimeSystem` GameObject add `FarmingController` and `FarmDebugHud`, then wire:

| Component | Field | Value |
| --- | --- | --- |
| `FarmingController` | World View / Time System / Corruption Controller / Player | scene objects |
| | Seed Cycle | size 2: `Flower`, `FoodCrop` |
| | Corrupted Crop | `CorruptedCrop` |
| | Previous Seed / Next Seed | expand `InputSystem_Actions` → drag `Player/PreviousSeed`, `Player/NextSeed` |
| `FarmDebugHud` | Farming Controller | same GO |

Save the scene.

### 4. Verify (Play Mode — short durations help: Day 15 / Night 8 / Dawn 2)

- Plant food (E to select) on clean soil, water it (Action again), watch the wet-soil darkening;
  at dawn it grows only if watered; after 2 watered dawns harvest it → HUD counter +1.
- Plant a flower next to the front; while growing, the night wave kills it. Grow one further
  back: once grown it blocks the wave (front stalls), and each dawn it cleanses its neighbors.
- Walk onto corrupted soil → Action plants the corrupted crop regardless of selection; when a
  flower cleanses that tile, the corrupted crop dies.
- Q/E cycle the selected seed in the HUD.

### 5. Tests

Test Runner → EditMode → Run All → **106 green**.

### Addendum — corrupted seed in the cycle, flowers harvestable

Design change: no more auto-planting the corrupted crop on corrupted soil — the corrupted
seed now lives in the seed cycle (Q/E) like any other, and planting simply fails when the
selected seed doesn't match the ground. This paves the way for non-seed selectables (flower
powder). Flowers are now **harvestable**: harvesting a grown one trades the shield for
powder (item arrives with the inventory; today it just counts in the HUD).

**Editor steps**: on `FarmingController` the *Corrupted Crop* field is gone — set **Seed
Cycle** to size **3**: `Flower`, `FoodCrop`, `CorruptedCrop`. (`Flower.asset` is already
flipped to Harvestable ✔.) Save the scene. Tests: **108 green**.

### Addendum — grown-crop health bars

Grown crops show a small bar above them: full = ready to harvest (food) / unhurt (flower);
a guarding flower's bar drains as spread waves hit it (red→green by fraction). Sizes,
positions, sorting orders and colors are all code-driven.

**Editor steps**: in the `Tile` prefab add two children — `HealthBarBg` and `HealthBarFill` —
each with a SpriteRenderer, sprite = `Square`, everything else untouched. Wire them into
`TileView`'s **Health Bar Background** / **Health Bar Fill** fields. Save.
Tests are now **107 green**.

### Addendum — HUD layout & first-day controls

The farming HUD is now right-anchored (the time HUD owns the top-left). New `ControlsHud`
(TEMP, `Scripts/UI/`) shows the control reference in a bottom-center box during day 1 only.
**One editor step**: add the `ControlsHud` component to the `TimeSystem` GameObject and wire
its **Time System** field. Save the scene.

### Addendum — in-place crop mutations must notify the view

Found in playtest: watering and dawn growth mutate the `Crop` object directly, so they never
fired `TileChanged` and the wet-soil/growth visuals froze (planting worked because `SetCrop`
fires). Fix: `WorldGrid.NotifyTileChanged` (internal) re-broadcasts a tile whose crop changed
in place; `FarmingSystem` calls it on water, on growth, **and on the watered reset** — views
redraw synchronously per notification, so the dawn's final notification must come after the
flag reset or the soil stays dark. Three tests pin this. `TileView` also stopped using
`Renderer.bounds` (world-space AABB — wrong on rotated tiles) in favor of the sizes `Init`
receives, and crops now sit on the tile surface.

Second playtest catch: `CropDefinition._tint` had no default, and an uninitialized `Color` is
`(0,0,0,0)` — the hex picker leaves alpha at 0, so all three crop assets rendered fully
transparent. The field now defaults to white, and the assets were fixed to alpha 1. Rule of
thumb adopted: **every serialized `Color` gets an explicit default.**

---

### Addendum — the house is always tile 0

Decided while building this feature: layouts where index 0 is not a House tile are invalid.
`WorldGrid`'s constructor now throws on them, `WorldGridTests` covers it, and systems may rely
on the invariant. During the review a latent bug was also fixed: `HouseCorrupted` fired on ANY
change to the house tile — including a future cleanse. It now checks the tile is actually
corrupted (covered by `HouseCorrupted_CleansingHouse_DoesNotFire`).
