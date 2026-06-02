# Brotato-like Survivor

A 2D top-down bullet survivor game built with Unity, inspired by [Brotato](https://store.steampowered.com/app/1942280/Brotato/) and [Vampire Survivors](https://store.steampowered.com/app/1794680/Vampire_Survivors/).

> Pet project focused on clean architecture and game systems design.

---

## Gameplay

- Survive waves of enemies using melee and ranged weapons
- Collect XP orbs dropped by enemies to level up
- Spend gold in the shop between waves to buy weapons and items
- Choose upgrades to power up your character
- Survive all 10 waves to win

**3 enemy types:** standard, ranged, kamikaze  
**3 hero archetypes:** Melee fighter, Archer, Unknown  
**1 active ability:** Dash (stamina-based)

---

## Architecture

The project uses a **State Machine** + **Service Locator** pattern without third-party DI frameworks.

### Game Flow
```
Bootstrap → LoadProgress → LoadoutSelect → LoadLevel → GameLoop → Shop/Upgrade → Win / GameOver
```

### Core Services
| Service | Responsibility |
|---|---|
| `IProgressService` | XP, leveling, experience thresholds |
| `IPersistentProgressService` | Save/load player state (JSON) |
| `IUpgradeService` | Weighted-random upgrade generation and application |
| `IBalanceService` | Per-level stat scaling |
| `IStaticDataService` | Loading ScriptableObject configs |
| `IShopService` | Shop item filtering, purchase logic |

### Key Systems

**GameFactory** — all game objects are created via factory, loaded through Unity Addressables.

**Object Pooling** — enemies, projectiles, and XP orbs use `ObjectPoolManager` with `IPoolable` interface (`OnSpawn` / `OnDespawn`).

**Wave System** — budget-based enemy generation. Each wave has a point budget; enemies are selected by cost. Wave ends only when spawn queue is empty and all alive enemies are dead.

**XP Orb System** — on enemy death, XP is split into denominations (Large=10, Medium=5, Small=1) using greedy decomposition. Orbs spawn in a radius around the death position, then magnetically attract to the player with acceleration when the player enters their pickup radius.

**Upgrade System** — upgrades are ScriptableObjects with a modifier type, value, and weight. Applied upgrades respect weapon ownership (e.g., cooldown reduction only applies if the player owns that weapon type).

**Save System** — `ISavedProgress` / `IProgressReader` pattern. Player state is serialized to JSON and persisted across sessions.

---

## Tech Stack

| | |
|---|---|
| **Engine** | Unity 2022 LTS, URP |
| **Language** | C# |
| **Pathfinding** | A* Pathfinding Project |
| **Procedural** | Feel / MMTilemapGenerator (MoreMountains) |
| **Feedbacks** | Feel / MMFeedbacks |
| **Input** | Unity New Input System |
| **Asset Loading** | Unity Addressables |
| **UI** | uGUI + TextMeshPro |
| **Version Control** | Git |

---

## Project Structure

```
Assets/
├── CodeBase/
│   ├── Enemy/              # EnemyAttack, EnemyDeath, EnemyMover, wave spawning
│   ├── Infrastructure/     # State machine, factory, services, asset management
│   │   ├── Factory/
│   │   ├── Services/
│   │   └── States/
│   ├── Loot/               # XpOrb, XpOrbSet
│   ├── Player/             # Movement, health, stamina, abilities, weapons
│   ├── Logic/              # ObjectPoolManager, shared interfaces
│   ├── StaticData/         # ScriptableObject definitions
│   └── UI/                 # HUD, menus, upgrade/shop screens
├── Resources_moved/        # Addressable prefabs (enemies, player, orbs, UI)
└── Assets/
    ├── HeroEditor/         # Character sprite system
    └── FantasyMonsters/    # Enemy sprites
```

---

## Getting Started

1. Clone the repository
2. Open in **Unity 2022.3 LTS** or newer
3. Install packages via Package Manager if prompted (Addressables, Input System, A* Pathfinding Project)
4. Open `Assets/Scenes/Main/Bootstrap.unity`
5. Press Play

> **Note:** The project uses Unity Addressables. If assets fail to load, go to  
> `Window → Asset Management → Addressables → Groups` and click **Build → New Build → Default Build Script**.

---

## What I Learned Building This

- Structuring a Unity project with scalable architecture (State Machine, Service Locator, Factory)
- Unity Addressables for async asset loading and memory management
- Object pooling to handle hundreds of enemies and projectiles without GC spikes
- Designing data-driven systems with ScriptableObjects
- Implementing save/load with a clean `ISavedProgress` interface
- A* Pathfinding integration for enemy navigation
- Balancing a game loop through playtesting and iteration
