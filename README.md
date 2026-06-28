# Brotato-like Survivor

A 2D top-down roguelite survivor game built with Unity, inspired by
[Brotato](https://store.steampowered.com/app/1942280/Brotato/) and
[Vampire Survivors](https://store.steampowered.com/app/1794680/Vampire_Survivors/).

> Pet project focused on clean architecture and game systems design.

---

## 🎮 Download

The latest playable build is available on the [Releases](../../releases/latest) page.

---

## Gameplay

- Navigate a procedurally generated branching map (Slay the Spire style)
- Survive waves of enemies using melee and ranged weapons
- Collect XP orbs dropped by enemies to level up and choose upgrades
- Spend gold in the shop between runs to buy weapons and items
- Rest at campfire nodes to heal or receive a random upgrade
- Defeat the boss to complete the map

**4 enemy types:** Standard, Tank, Ranger, Kamikaze  
**2 hero archetypes:** Melee Fighter, Archer

---

## Architecture

The project uses a **State Machine** + **Service Locator** pattern
without third-party DI frameworks.

### Game Flow
```
Bootstrap → LoadProgress → LoadoutSelect → LevelMap →
LoadLevel → GameLoop → Upgrade / Shop / Campfire → Win / GameOver
```

### Core Services
| Service | Responsibility |
|---|---|
| `IProgressService` | XP, leveling, experience thresholds |
| `IPersistentProgressService` | Player state persistence (JSON) |
| `IUpgradeService` | Weighted-random upgrade generation and application |
| `IBalanceService` | Gold tracking and spending |
| `IShopService` | Shop item filtering and purchase logic |
| `IMapService` | Procedural map generation, node selection and progression |
| `IStaticDataService` | Loading and caching ScriptableObject configs |

### Key Systems

**GameFactory** — all game objects are created via factory,
loaded through Unity Addressables.

**Object Pooling** — enemies, projectiles, and XP orbs use
`ObjectPoolManager` with `IPoolable` interface (`OnSpawn` / `OnDespawn`).

**Wave System** — budget-based enemy generation. Each wave has a point
budget; enemies are selected by cost. The wave ends only when the spawn
queue is empty and all alive enemies are dead.

**Procedural Map** — branching node map generated from a seed.
Node types (Combat, Elite, Shop, Campfire, Boss) are assigned
probabilistically. The same seed always produces the same map,
so only the seed and completed node IDs need to be saved.

**XP Orb System** — on enemy death, XP is split into denominations
(Large=10, Medium=5, Small=1) using greedy decomposition. Orbs
magnetically attract to the player when they enter the pickup radius.

**Upgrade System** — upgrades are ScriptableObjects with a modifier
type, value and weight. Applied upgrades respect weapon ownership
(e.g., damage bonus only applies to weapons the player actually owns).

**Save System** — `ISavedProgress` / `ISavedProgressReader` pattern.
Player state is serialized to JSON via `PlayerPrefs`.

---

## Tech Stack

| | |
|---|---|
| **Engine** | Unity 6000.3.8f1, URP |
| **Language** | C# |
| **Pathfinding** | A* Pathfinding Project |
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
│   ├── Ability/            # IAbility, AbilityBase, DashAbility
│   ├── Enemy/              # EnemyAttack, EnemyDeath, EnemyMover
│   ├── Infrastructure/     # State machine, factory, services
│   │   ├── Factory/
│   │   ├── Map/            # MapGenerator, MapNode, GeneratedMap
│   │   ├── Services/
│   │   └── States/
│   ├── Loot/               # XpOrb, XpOrbSet
│   ├── Player/             # Movement, health, stamina, abilities
│   ├── Logic/              # ObjectPoolManager, WaveController
│   ├── StaticData/         # ScriptableObject definitions
│   └── UI/                 # HUD, map view, upgrade/shop/campfire screens
```

---

## What I Learned Building This

- Structuring a Unity project with scalable architecture
(State Machine, Service Locator, Factory)
- Unity Addressables for async asset loading and memory management
- Object pooling to handle hundreds of enemies and projectiles
without GC spikes
- Designing data-driven systems with ScriptableObjects
- Procedural map generation with deterministic seeding
- Implementing save/load with a clean `ISavedProgress` interface
- A* Pathfinding integration for enemy navigation
- Balancing a game loop through playtesting and iteration
