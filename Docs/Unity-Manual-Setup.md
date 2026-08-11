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
