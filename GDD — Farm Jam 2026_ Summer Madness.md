# **GDD — Farm Jam 2026: Summer Madness**

Jam-scoped design document.  
Jam: Jul 15 – Aug 15 2026 (30 days) · Hosted by Figerox Studios  
Team: Gus (engineering & sound), Janhavi (development), Irene (art)

## **0\. Jam Requirements (non-negotiable — lock this before designing anything)**

This section overrides any creative idea: if an idea doesn't fit here, it doesn't go in the game.

> - [x] ~~**Farm theme** confirmed in the concept (genre is free: cozy, horror, sci-fi, survival, etc.)~~  
> - [x] ~~Includes **growable crops** and/or **raisable animals** → which one(s):~~   
      - [x] ~~**Growable crops**~~  
      - [ ] **Raisable Animals**  
> - [x] ~~Mandatory system (pick ONE):~~  
      - [x] ~~**Day/Night** — how it affects crops, animals, or farm activity~~  
      - [ ] **Seasons** — how it changes how the farm works/behaves  
> - [ ] **Secret theme** (revealed at jam start, Jul 15):  
      - [x] ~~If NOT using the secret theme → pick **2 mechanics** from the official list:~~  
            1. **Dynamic Weather**  
            2. **Mutated/Experimental Crops**

## **1\. Game Identity / Mantra**

A single sentence used to arbitrate design decisions for the whole jam. If something doesn't fit this sentence, it gets cut.

**Our mantra:** A farming sim where you have to fight corruption to survive and revive a round world by planting and harvesting crops.

## **2\. Design Pillars**

Up to 3 words/phrases describing the ***feeling*** the player should have. Our whole team (design, art, sound, code) should use these as a filter for decisions.

> 1. As the last person alive, the player will **feel like a nature hero** for saving the world bit by bit.  
> 2. As night comes everyday, the player will **feel progressive fear** from an unknown and powerful entity.  
> 3. 

## **3\. Genre / Mechanics / (Story, if relevant) Summary**

2–4 lines. Should make it clear what the game is about without detailing systems.

Summary:  
The player is the last human on a tiny round an tiled world where an entity tries to kill everything there by corrupting tile by tile every night. The only way to stop it is planting special flowers that pushes the corruption away every day and surviving the hunger by eating harvested crops.

Story:  
Every night, the world slowly dies.  
The only place that stays alive is your magical farm. Your crops don't just feed you—they keep the world itself alive. Every harvest lets you live another day but letting them grow pushes back the spreading corruption from an unknown entity. Can you keep surviving and save the world?

## **4\. Core Loop (the most important section of this document)**

The 30–90 second cycle the player repeats throughout the game. If this isn't clear and fun in the abstract (no art, no sound), nothing else will save the project.

**Our loop:**

1. Dawn (transition):  
   1. Today's weather is rolled and shown  
   2. Fully-grown flowers cleanse adjacent corrupted tile(s)  
   3. Surviving crops grow \+1 level  
2. Day (90 s):  
   1. Harvest Blessed or Surviving Crops  
   2. Plant  
   3. Water  
   4. Gather Resources  
   5. Eat from inventory to refill hunger (manual, deliberate choice: eat vs replant)  
3. Night (30 s):  
   1. Corruption Spreads to adjacent tiles (cannot enter tiles guarded by a living flower)  
   2. Avoid hazards (corrupted ground damages the player on contact — no combat in Must-Have)  
   3. Hunger goes down

## **5\. Features**

Sort into three columns from day one. This replaces a backlog: everything is born already classified.

| Must-Have (no game without it) | Nice-to-Have (if time allows) | Cut (consciously dropped)   |
| :---- | :---- | :---- |
| A round world with different types of grounds separated in tiles. On one side the players house, on the other side the corruption base. 2D side-view tiny planet: the player walks around the circumference (gravity points to the center), tiles are arc segments of the ring. \~24 tiles to start — tile count must be trivially tunable for prototyping. House at one pole, corruption base at the opposite pole (corruption advances on two fronts). | River tiles |  |
| Player Input (movement tangential to the surface of the world and action). Continuous walking (not tile-by-tile); the tile under the player is highlighted and Action applies to it. | Tools system |  |
| Health system. The only stat that kills you. Drains continuously while standing on corrupted ground (any phase), slowly while hunger is at 0 (both stack), and in a lump when eating a corrupted crop. Eating restores some back. | Fighting corruption |  |
| Day/Night time system |  |  |
| Inventory system |  |  |
| Corruption system. Spreads at nightfall: every frontier of every corrupted region advances N tiles (tunable, default 1). No tile type is immune — rocks and the house corrupt like any tile (house corrupted \= defeat). Cannot spread into a tile guarded by a living flower; pushed back by grown flowers at dawn. |  |  |
| Planting system. Two base crops: the Flower (not edible; only when fully grown it guards its tile — each corruption attempt costs it 1 life, tunable — and cleanses both adjacent corrupted tiles at every dawn, persisting while alive) and the Food crop (edible, no protection). Growing crops need to be watered that day to gain \+1 level at dawn. Crops die when corruption enters their tile. One contextual Action button: harvest \> water \> plant the selected seed (seed cycle with Q/E — includes the corrupted seed; planting fails if the seed doesn't match the ground). Harvesting a grown flower gives up the shield in exchange for flower powder (with the inventory, powder becomes a selectable that cleanses the tile under the player). |  |  |
| Corrupted crops. Plantable only on corrupted soil; die if their tile is cleansed. Full design: their seeds drop from crops killed by corruption (economy arrives with the inventory system). |  |  |
| Dynamic weather system |  |  |
| Hunger system. Drains continuously through the whole cycle; refilled by manually eating a crop from the inventory. At 0 it does not kill — it slowly drains health instead. Nutrition and damage values are authored per crop. |  |  |

Jam rule: **Must-Haves should be playable start to finish well before the final week.** Everything else is a bonus.

## **6\. Interface / Controls**

> * **Input platform:** keyboard / gamepad  
> * **Core controls:**  
  * Move: Left / Right  
  * Action: Down  
> * **Minimum required menus/screens:**   
  * Start  
  * Pause  
  * Game Over/Win

## **7\. Art**

> * **Visual references (links/images):** 
> * **Color palette:** 
> * **Style:** **Pixel art** (Aseprite workflow — the project uses Unity's Aseprite importer)  
> * **Art scope constraint:** (e.g. max 1 character, 3 terrain tiles, 2 animals) 
> * **Notes on asset scope:** keep the asset list short and explicit (list exact sprites/animations needed, not "a farm"). Favor reusable/modular pieces (tileable ground, one rig reused for multiple crops) over one-off hero assets.

## **8\. Sound & Music**

> * **Overall sonic mood:**  (reference tracks/games)  
> * **Day/Night differentiation:**  
> * **Critical SFX for the core loop** (prioritize these — they get heard the most):  
  * Plant/interact:   
  * Harvest/reward:   
  * Time transition (day-night / season change):   
  * Error or loss feedback:   
> * **Progress feedback:** is there a sound cue that signals "you're doing well" or "night is approaching"? Cheap to make, sells well in jams.  
> * **Technical note:** keep audio modular (layers that can be toggled) instead of long single tracks — easier to adapt if the secret theme shifts the game's tone.

## **9\. Mandatory Jam Systems — implementation detail**

### **Day/Night**

> * What exactly changes?  
  * At Night corrupted ground expand to adjacent tiles (blocked by tiles guarded by a living flower)  
  * At Dawn: weather is rolled and shown → grown flowers cleanse adjacent corrupted tiles → surviving crops grow 1 level  
  * At Night hunger goes down; corrupted ground damages the player on contact  
> * How long is one cycle in real time?  
  * ~2 minutes: **90 s day / 30 s night / 3 s dawn** — Dawn is a short phase of its own between night and day where the dawn sequence plays out; all durations tunable for prototyping. The game starts at the Dawn of day 1\.  
> * Is it reversible, or is there pressure (e.g. crops dying if not harvested in time)?  
  * Corruption is reversible tile by tile (flower cleanse at dawn); the pressure is the nightly spread toward the house on two fronts

### **Mechanics chosen from the official list (if not using the secret theme)**

#### **Dynamic Weather** 

How it connects to the core loop:

> * One weather state per day, rolled at random and telegraphed at dawn so the player can plan  
> * Rain: waters all crops automatically  
> * Sun: charges sunflowers (protecting and pusher crops)  
> * Cloudy: nothing

#### **Mutated/Experimental Crops**

How it connects to the core loop:

> * Corrupted crops give more food but expands the corruption more  
> * Eating a corrupted crop also damages the player's health (risk/reward)  
> * Corrupted seeds drop when corruption kills a crop; they can only be planted on corrupted soil, and the crop dies if its tile is purified — farming the enemy's land is a deliberate gamble

## **10\. Win / Lose Conditions (if applicable)**

> * **Win when:**   
  * All corruption is destroyed  
> * **Lose when:**   
  * Your house has been corrupted  
  * Your life goes to 0  
> * Note: hunger at 0 is **not** a direct lose condition — it slowly drains health instead (health is the only stat that kills you)

## **11\. Platform & Audience**

**Platform:** PC     **Audience:** Everyone

## **12\. Team Roles & Ownership**

| Area | Owner | Notes   |
| :---- | :---- | :---- |
| Engineering / systems | Gus |  |
| Gameplay programming | Janhavi |  |
| Art direction & assets | Irene | Needs explicit, scoped asset lists (see section 7\) |
| Sound design & music | Gus | Treat as a first-class system, not a last-minute pass |

