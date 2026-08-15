# Flowers vs Corruption — Project Instructions

Game jam project for **Farm Jam 2026: Summer Madness** (Jul 15 – Aug 15 2026).
Read the full design in [GDD — Farm Jam 2026_ Summer Madness.md](<GDD — Farm Jam 2026_ Summer Madness.md>) — it is the source of truth for scope. **If a feature is not in the Must-Have column, do not build it unless explicitly asked.**

## The game in one paragraph

2D side-view tiny round planet. The player (last human alive) walks around the circumference (gravity points to the planet center). The world is divided into ~24 arc-segment tiles: the player's house on one side, the corruption base on the opposite side. Every night the corruption spreads tile by tile toward the house; every day the player farms magical flowers that push it back. Survive hunger by eating harvested crops. Win when all corruption is cleansed; lose when health hits 0 or the house is corrupted.

## Design decisions (clarified with the team — these override ambiguity in the GDD)

- **World**: 2D side-view circular planet, ~24 tiles around the circumference (must be trivially tunable). House at one pole, corruption base at the opposite pole, so corruption advances on two fronts.
- **The house is always tile index 0** (the top pole). `WorldGrid` enforces the invariant — its constructor throws if index 0 is not a House tile — so every layout, hand-authored or generated, must respect it and systems may rely on it.
- **Movement**: continuous tangential walking (Left/Right). The tile under the player is highlighted; Action (Down) applies to that tile. Not tile-by-tile stepping.
- **Cycle**: **Day 90 s → Night 30 s → Dawn 3 s → next Day** (all tunable in `GameConfig`). The game starts at the Dawn of day 1, so the dawn sequence (weather roll, cleanse, growth) runs identically for every day including the first. Contract: corruption spread listens to `NightStarted`; the dawn sequence hangs off `DawnStarted`.
- **Night**: corruption spreads to adjacent tiles; the player's job is to *avoid hazards* (no combat in Must-Have). Hunger ticks down at night.
- **Starting corruption**: the CorruptionBase tile plus N tiles on each side start corrupted; N is authored on `WorldLayout` (`Initial Corruption Per Side`, default 1).
- **Corruption spread**: on `NightStarted`, every frontier of every corrupted region advances `Corruption Spread Per Night` tiles (wave-based; each wave corrupts all clean tiles adjacent to corruption, computed from a snapshot). No tile is immune — rocks and the house corrupt like any tile. The house corrupting raises `HouseCorrupted` (the future lose screen listens). `CorruptionSystem.IsTileGuarded` is the injectable hook for living flowers to block a front.
- **Starting farmland**: the world generator guarantees N Soil tiles (no rocks) on each side of the house; authored on `WorldLayout` (`Soil Next To House Per Side`, default 1).
- **Dawn sequence**: weather for the new day is rolled and shown → grown flowers cleanse adjacent corrupted tile(s) → surviving crops grow +1 level.
- **Crops** (data-driven via `CropDefinition` flags — Flower, FoodCrop, CorruptedCrop as Must-Have content): planting is free until the inventory feature; only Soil tiles are plantable (clean Soil for regular seeds, corrupted Soil for the corrupted crop — rocks/house/base never). A growing crop only gains +1 stage at dawn if watered that day (flag resets each dawn).
- **Flowers**: only a **fully-grown** flower guards its tile — each spread wave that tries to enter costs it 1 health (max authored on its `CropDefinition`, default 3); at 0 it dies and the tile is exposed (the killing wave is spent breaking it, the tile stays clean that wave). A grown flower cleanses BOTH adjacent corrupted tiles at every dawn (cleanse runs before growth: a flower that finishes growing this dawn first cleanses next dawn) and persists while alive. While growing it is as vulnerable as any crop. **Flowers are harvestable**: harvesting a grown one is a deliberate trade — the shield is given up for **flower powder** (future item: with the inventory, powder becomes a selectable "seed" that cleanses the tile under the player).
- **Crop death**: corruption entering a tile kills its crop (food or growing flower) — hook for the future corrupted-seed drop. A corrupted crop dies when its tile is cleansed (exact mirror).
- **Contextual Action** (one button, priority): grown harvestable crop → harvest · living unwatered crop → water · empty plantable tile → plant **the selected seed** (cycled with PreviousSeed/NextSeed — Q/E, gamepad shoulders). The corrupted crop is part of the seed cycle like any other — planting simply fails if the selected seed doesn't match the ground (clean vs corrupted). The cycle will also hold non-seed selectables later (flower powder).
- **Weather** (one state per day, random, telegraphed at dawn): Rain waters all crops · Sun charges sunflowers · Cloudy does nothing.
- **Health** (the only stat that kills you): drains from standing on corrupted tiles, from eating corrupted crops, and slowly while hunger is at 0.
- **Hunger**: refilled by *manually* eating a crop from the inventory (deliberate choice: eat vs replant/use).
- **Corrupted crops** (Mutated Crops jam mechanic): plantable only on corrupted Soil; give more food but damage health and make corruption spread more (effects land with the hunger/health features). Full design: their seeds drop from crops killed by corruption — that economy arrives with the inventory feature; until then planting them is free like everything else.
- **Lose**: health reaches 0, OR the house tile is corrupted. **Win**: every tile cleansed.
- **Screens**: Start, Pause, Game Over/Win.

## Tech stack

- **Unity 6000.5.0f1**, URP (2D renderer), **pixel art** via the Aseprite importer.
- **New Input System** (`InputSystem_Actions.inputactions` at Assets root) — never `Input.GetKey`.
- Keyboard + gamepad. PC target.
- Test Framework is installed — plain-C# game logic should be unit-testable (EditMode tests).

## Architecture rules

Pragmatic jam architecture: SOLID and clean separation where it pays off, zero ceremony where it doesn't. No DI containers, no third-party frameworks.

1. **Tunables live in ScriptableObjects**, never hardcoded. One `GameConfig` (tile count, day/night durations, hunger/health rates, spread rates…) plus per-content definitions (`CropDefinition`, `WeatherDefinition`…). Designers must be able to rebalance without touching code.
2. **Simulation vs presentation**: game rules (corruption spread, crop growth, hunger/health math, win/lose checks) go in plain C# classes with no UnityEngine scene dependencies so they are unit-testable. MonoBehaviours are thin adapters that own lifecycles, input, and visuals ("humble object").
3. **Decouple with events**: systems communicate through C# events raised by a small number of owners (e.g. `TimeSystem.DawnStarted`, `CorruptionSystem.TileCorrupted`). UI and audio only *listen*. No system reaches into another's internals; no `FindObjectOfType` in gameplay code.
4. **State machine for game flow**: Day/Night phases and Start/Playing/Paused/GameOver as explicit states, not scattered booleans.
5. **The tile ring is the central model**: a single `WorldGrid` (plain C#) owns the ordered list of tiles and their state (ground type, occupant crop, corruption). Everything queries/mutates through it — no per-tile MonoBehaviours holding authoritative state.
6. **Composition over inheritance** for crops/tiles: behavior differences come from data (`CropDefinition` flags/curves), not subclass trees.
7. Follow the four OOP pillars and SOLID, but **jam rule: three strikes then refactor** — don't build abstractions for a second use that may never come.

## Division of work with AI

**AI writes code; Gus does all Unity Editor work by hand.** Never generate editor automation
scripts (`[MenuItem]` setup tools) to create assets, prefabs, or scenes. Instead, append precise
step-by-step instructions to [Docs/Unity-Manual-Setup.md](Docs/Unity-Manual-Setup.md) — one section
per feature — covering exactly which assets to create, where, and which fields to wire.

Corollary: keep code independent of hand-authored values where cheap. Example: `SpriteFitter`
scales sprites to the size in `GameConfig`, so nobody has to match pixels-per-unit by hand.

## Project layout

All our assets live under `Assets/_Project/` (keeps them separated and sorted above package junk):

```
Assets/_Project/
  Art/            (sprites, aseprite files, palettes)
  Audio/          (music, sfx)
  Data/           (ScriptableObject assets: GameConfig, crop defs…)
  Prefabs/
  Scenes/
  Scripts/
    Core/         (time system, game state machine, event plumbing)
    World/        (WorldGrid, tiles, corruption)
    Farming/      (crops, planting, watering, weather)
    Player/       (movement, input adapter, health/hunger)
    Inventory/
    UI/
    Audio/
  Tests/          (EditMode tests for the plain-C# simulation)
```

- Root namespace: `FlowersVsCorruption`, sub-namespaces mirror the script folders (`FlowersVsCorruption.World`, …).
- One asmdef for `Scripts` (`FlowersVsCorruption.asmdef`) + one for `Tests` — keeps compile times fast and tests isolated.

## C# conventions

- PascalCase for types/methods/properties, camelCase for locals/parameters, `_camelCase` for private fields.
- `[SerializeField] private` over public fields.
- Events named as past/ongoing facts: `DawnStarted`, `TileCorrupted`.
- Comments only for non-obvious constraints; the code should read on its own.

## Team & workflow

| Area | Owner |
| --- | --- |
| Engineering/systems, sound & music | Gus |
| Gameplay programming | Janhavi |
| Art direction & assets | Irene |

- **Git**: both devs commit **directly to `dev`** (no PRs during the jam); `main` holds releasable builds. Always `git pull --rebase` before pushing.
- **Unity merge-conflict prevention** (critical with two devs and no PRs): only one person edits a given scene at a time; build content as prefabs and add them to the scene once; keep scenes thin. Asset Serialization is Force Text (default).
- **One-time git setup per clone** (line endings + Unity SmartMerge): [Docs/Git-Setup.md](Docs/Git-Setup.md).
- Commit messages: `feat|fix|chore(scope): message` (see git log).

## Jam guardrails

- Must-Haves playable start-to-finish **well before the final week** — bias every decision toward shippable.
- The design mantra arbitrates disputes: *"A farming sim where you have to fight corruption to survive and revive a round world by planting and harvesting crops."*
- Design pillars: the player should **feel like a nature hero** by day and **progressive fear** by night.
- Art/sound requests to teammates must be explicit scoped lists (exact sprites/animations, exact SFX), never "a farm".
