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
