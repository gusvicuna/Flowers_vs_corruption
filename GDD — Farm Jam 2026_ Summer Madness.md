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

1. Day:  
   1. Corruption is pushed back  
   2. Harvest Blessed or Surviving Crops  
   3. Plant  
   4. Water  
   5. Gather Resources  
2. Night:  
   1. Corruption Spreads  
   2. Explore/Defend   
   3. Use Harvest to Restore the World  
   4. Hunger goes down

## **5\. Features**

Sort into three columns from day one. This replaces a backlog: everything is born already classified.

| Must-Have (no game without it) | Nice-to-Have (if time allows) | Cut (consciously dropped)   |
| :---- | :---- | :---- |
| A round world with different types of grounds separated in tiles. On one side the players house, on the other side the corruption base. | River tiles |  |
| Player Input (movement tangential to the surface of the world and action) | Tools system |  |
| Health system | Fighting corruption |  |
| Day/Night time system |  |  |
| Inventory system |  |  |
| Corruption system |  |  |
| Planting system |  |  |
| Corrupted crops |  |  |
| Dynamic weather system |  |  |
| Hunger system |  |  |

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
> * **Style:** pixel art / low poly / vector / hand-drawn / etc.  
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
  * At Night corrupted ground expand to adjacent tiles  
  * At Day surviving crops grow 1 level  
> * How long is one cycle in real time?  
  * 2 minutes  
> * Is it reversible, or is there pressure (e.g. crops dying if not harvested in time)?

### **Mechanics chosen from the official list (if not using the secret theme)**

#### **Dynamic Weather** 

How it connects to the core loop:

> * Rain: waters all crops automatically  
> * Sun: charges sunflowers (protecting and pusher crops)  
> * Cloudy: nothing

#### **Mutated/Experimental Crops**

How it connects to the core loop:

> * Corrupted crops give more food but expands the corruption more

## **10\. Win / Lose Conditions (if applicable)**

> * **Win when:**   
  * All corruption is destroyed  
> * **Lose when:**   
  * Your house has been corrupted  
  * Your hunger goes to 0  
  * Your life goes to 0

## **11\. Platform & Audience**

**Platform:** PC     **Audience:** Everyone

## **12\. Team Roles & Ownership**

| Area | Owner | Notes   |
| :---- | :---- | :---- |
| Engineering / systems | Gus |  |
| Gameplay programming | Janhavi |  |
| Art direction & assets | Irene | Needs explicit, scoped asset lists (see section 7\) |
| Sound design & music | Gus | Treat as a first-class system, not a last-minute pass |

