# Gameplay backlog

Boss work and battlefield ideas, collected on 2026-11-23. Tick items off or delete them as they are done.

## Boss: Brood Queen

### Done (2026-11-23)

- **Minimal boss** (`925ef1bc`): `Wave_Boss` has `spawnBoss`; `WaveController` spawns the queen ~9 units from the player (`bossSpawnDistance`), the run ends when she dies (`bossDefeatedDelay` 3 s), background waves stop and all spawned enemies are killed. `BossBrain` loop: Chase → Telegraph (tint pulse) → attack → Recover. HUD `BossHealthBar` replaces the wave counter.
- **Larva**: new fast, fragile melee minion (`EnemyTypeId.Larva`, prefab `Resources_moved/Enemy/Larva.prefab`). Its attack animation has no `AttackEnded` event, so `EnemyAttack._endAttackOnHitEvent` is enabled on it.
- **Step 2** (`3500baeb`): attack rotation Spit → Brood → Eggs.
  - **Egg barrage:** pooled `GroundTelegraph` circles (area + growing fill + ring, prefab `Resources_moved/Enemy/EggWarning.prefab`); the impact deals area damage and one egg hatches a larva.
  - **Phase 2 at 50% HP:** roar with `EnemyHealth.IsInvulnerable`, poison cloud VFX and shake; after it, tempo ×0.7, 5-way spit, 5 eggs, green tint.
  - **Presentation:** intro banner `BossIntroBanner`, phase marker on the HUD bar, death VFX + hit stop + shake. The world HP bar above the queen is hidden.
- All tuning lives in `Resources/StaticData/Bosses/BroodQueen.asset` (`BossStaticData`). HP, rewards and prefab come from `Resources/StaticData/Enemy/BroodQueen.asset`.

### To check / tune

- **Play the fight by hand.** It was only verified with forced damage via MCP, so the real length and difficulty are unknown.
  - Start numbers: HP 180, spit damage 2, egg damage 3, attack interval 2.5 s.
  - In the test, the player dropped to 59/100 quickly, mostly from larvae and background enemies. Levers: `summonCount`, `maxAliveMinions`, Larva damage/HP, `Wave_Boss` budget.
- **`Wave_Boss` background waves were weakened** for the boss fight: `waveValueMultiplier` 16 → 6, `maxAlive` 18 → 8.
- **Egg spread was raised after the last visual check** (`eggScatterRadius` 3.5, `eggImpactRadius` 1.6) and has not been looked at in game since.

### Ideas not done yet

- **Elite nodes:** add a "champion" modifier (more HP, an aura, a guaranteed reward). Right now Elite only differs from Combat by enemy count.
- **Roar animation:** BugQueen has no roar animation, so the phase shift plays her normal attack. A scale/shake pulse on the sprite child would read better.
- **Telegraph readability:** circles can hide under characters. Consider a brighter outline in phase 2 or a short "drop" sprite for the egg.
- **Boss music/sounds:** `MusicTrackId.Boss` is already played; no SFX yet for spit, egg impact, roar or death.
- **More bosses:** the same `BossBrain` + `BossStaticData` pattern. The FantasyMonsters pack also has Giants (Juggernaut, Dreadnought) and MossQueen/SlugQueen. Titan and Dragon controllers exist, but their sprites are not in the project.

## Battlefield

### Current state (2026-11-23)

- **One map for every node:** a single ~200×200 map baked into `Assets/Scenes/Main.unity` (Grid → Ground / Path / Wall Shadows / Walls tilemaps, sorting −10…−7). It uses rule tiles from the Avesta Tilemap Generator **sample** (`Plugins/BerserkPixel/.../Samples/Tilemap/Tiles`), with 14 spawn points. Every map node uses this map.
- **Camera:** orthographic size 20 (checked 2026-11-24; an earlier note said 5), so about 71×40 units are visible. Decided in PR #2 review (reference: Vampire Survivors): a big map, `Arena_Default` is 324×180 (~4.5 views per axis) with ortho 20 kept.

### Problems

1. **Style clash:** pixel-art lime ground under HD cartoon characters (HeroEditor / FantasyMonsters) and the dark-purple + gold UI. The brown wall blobs don't read as walls or holes.
2. **Readability:** the saturated lime ground competes with gameplay colours (green boss acid, green stamina bar, red warning circles). Ground should be the quietest layer on screen.
3. **Spawning far away:** on the old map enemies spawned at fixed points far from the player (the boss first spawned 77 units off). Fixed for `Arena`: enemies spawn just off-screen around the view.
4. **No variety:** 10 map rows, Elite and Boss all play on the same field.
5. **Flat image:** no lighting, no vignette, no decor.

### Decisions (2026-11-24)

- **Hybrid arenas:** hand-made arena layouts (prefabs) + procedural dressing chosen by seed. Not full procedural generation (it gives little for a survivor arena and easily makes bad pockets/chokepoints), not only hand-made maps (they get repetitive over 10 map rows).
- **Existing art first**, new ground art later — **do not forget the art step** (see Step 3): without it the style clash stays.
- **Obstacles matter for gameplay** (cover, kiting around them, pulling enemies through gaps), not just decor like Brotato.

### Level design rules for arenas

- Big map like Vampire Survivors: 324×180 at ortho 20 (~4.5 views per axis), bounded by invisible walls; the player roams, enemies follow.
- Obstacles are scattered landmarks and maneuver points across the map, not a maze.
- No dead ends or narrow pockets: every passage at least 3–4 units wide.
- Enemies spawn just outside the camera view (done: `EnemySpawner.SpawnOffscreen`).
- Ground is the quietest layer; orientation comes from big obstacles/landmarks.
- Boss arena: round/oval, more open, atmosphere on the edges.

### Plan

**Step 1: hybrid arena system on existing art.**
- **One `Arena` scene** that loads an arena layout prefab via the factory (Addressables); `Main` stays for comparison until the new one is better. No scene per map.
- **Layout prefab:** bounds/border, ground (`ArenaGround` fills the tilemap at load), big obstacles (with colliders), allowed zones for random obstacles/props.
- **Procedural dressing by `mapSeed + node.Id`** (like `MapLevelLibrary.WaveConfigFor`): choose layout from the pool, mirror/rotate, scatter small obstacles and decor in allowed zones, slight ground tint variation. Same node → same arena.
- **Validation:** after placing obstacles, keep passages ≥ 3–4 units and no unreachable areas; then `AstarPath.Scan`.
- **Look on existing art:** darker, desaturated ground (`Tilemap.color`) to match "Duskward" and the purple UI; URP 2D lighting (darker cooler global light + warm light around the player); soft vignette.
- **Camera / arena size:** done — 324×180 at ortho 20. Revisit only if characters feel too small.
- **Spawn off-screen:** done — `EnemySpawner.SpawnOffscreen` picks a point on a ring just outside the camera view (`offscreenMargin`), clamped to the arena; centred on the camera, since it is clamped near the edges. The boss still spawns `bossSpawnDistance` (9) from the player, i.e. on screen (intended).

**Step 2: biomes and a boss arena.**
- **Biomes:** 2–3 layout pools that change by map row (forest → swamp → hive) / node type. Boss nodes always get their own arena.
- **Brood Queen arena:** organic hive floor, egg clutches around the edge, slime, green spores in the air, so the boss feels like an event.

**Step 3: new ground art (TODO, needed to close the style gap).**
- **Art set:** a hand-painted top-down cartoon set close to FantasyMonsters, with 1–2 quiet ground tiles plus separate props (rocks, stumps, bushes, bones, mushrooms). The project has nothing suitable; `Assets/Assets/SomeTiles/0x72_DungeonTilesetII` is pixel art too. The user will look for / buy a set.
- **Props:** some purely decorative (no collider), some obstacles (with colliders). Y-sorting already works (`TransparencySortMode.CustomAxis` on Y). Swap the art into the existing layout prefabs.

**Small touches:** ambient particles (leaves, spores, fireflies), fading death stains or remains, light fog at the arena edges.

### Technical notes

- After changing walls or obstacles, rescan the A* graph (`AstarPath.Scan`), or enemies will path through the new obstacles.
- The Avesta generator is not needed for arenas; layout prefabs + seeded dressing are simpler.
- Legacy `Main` still uses fixed spawn points (`minDistanceFromPlayer` 5); `Arena` uses off-screen spawning. A big map needs a big A* grid (324×180 nodes), scanned at load.
