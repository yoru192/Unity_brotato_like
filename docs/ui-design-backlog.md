# UI design backlog

Findings from a full UI review on 2026-11-16 (every screen walked in Play Mode and screenshotted).
Tick items off or delete them as they are done.

Done on 2026-11-17: canvas scaling (Menu, LoadoutSelect, Win, GameOver → Scale With Screen Size 1920×1080, match 0.5), essence reward text on Win / Game Over, "YOU ARE WIN" → "VICTORY".

## Where the UI lives

| Screen | Asset | Script | Canvas scaler |
|---|---|---|---|
| Main menu | `Assets/Scenes/Menu.unity` | `MenuUI` | 1920×1080, match 0.5 |
| Heroes (hub before every run) | `Assets/Scenes/LoadoutSelect.unity` | `LoadoutSelectUI`, `CharacterPanel`, `RoleButton` | 1920×1080, match 0.5 |
| Meta upgrade tree | `LoadoutSelect.unity` → `LoadoutSelectUI/MetaUpgradeScreen`, node prefab `Assets/Resources/UI/MetaUpgradeNode.prefab` | `MetaUpgradeScreen`, `MetaUpgradeNodeView` | (same canvas) |
| Map | `Assets/Scenes/LevelMap.unity`, `Resources_moved/Map/MapNode.prefab` | `MapView`, `MapNodeView` | 1920×1080, match 0 |
| HUD | `Resources_moved/Hud/Hud.prefab` | `HudUI`, `HPBar`, `StaminaBar` | 1920×1080, match 0 |
| Pause + Settings | `Resources_moved/UI/PauseScreen.prefab` | `PauseUI`, `SettingsScreen` | 1920×1080, match 0 |
| Choose Upgrade | `Resources_moved/UI/UpgradeScreenCanvas.prefab`, card `Assets/Resources/UI/UpgradeButton.prefab` | `UpgradeScreen`, `UpgradeButton` | 1920×1080, match 0.5 |
| Shop | `Resources_moved/UI/ShopScreenCanvas.prefab`, card `Assets/Resources/UI/ShopItem.prefab` | `ShopScreen`, `ShopItem` | 1920×1080, match 0.5 |
| Campfire | `Resources_moved/UI/CampfireScreen.prefab` | `CampfireScreen` | 1920×1080, match 0.5 |
| Win / Game Over | `Resources_moved/UI/WinScreen.prefab`, `GameOverScreen.prefab` | `WinUI`, `GameOverUI` | 1920×1080, match 0.5 |

All screen canvases are Scale With Screen Size now (match 0 vs 0.5 only differs on non-16:9 aspect ratios). The `Main` scene canvas and enemy health-bar canvases are world-space; `Curtain` is a full-screen fade and stays Constant Pixel Size.

UI kit: `Assets/Layer Lab/GUI Pro-FantasyRPG/ResourcesData/` (sprites under `Sprites/Component/`, font `Fonts/Alata-Regular SDF.asset`). Third-party — use its assets, do not edit them.

## Priorities (suggested order)

1. ~~Canvas scaling~~ — done.
2. ~~Show the essence reward~~ — done (`Text_EssenceReward` under `PlayContinue/Continue` in both prefabs, wired to `_essenceRewardText`). Follow-ups: replace the word "essence" with the gem icon used in the Heroes header; consider a small run summary (nodes cleared, essence gained, total).
3. ~~In-combat HUD readability~~ — done 2026-11-17 (see "Style reference" below; `HudUI` now sizes bar fills via anchors instead of `fillAmount`, which also fixed the HP bar never shrinking).
4. **One visual style.** Four styles are mixed: flat cartoon purple (Menu, Heroes, Pause, Game Over), teal/gold "book" (Choose Upgrade), realistic painted art (Shop, Campfire), dark ink map — all over a lime pixel battlefield. **Chosen base (2026-11-17): dark purple + gold accent** (Heroes/Pause look). Converted on 2026-11-17: HUD, Choose Upgrade, Shop, Campfire, Map, Meta upgrade tree (painted backgrounds kept under a purple dim). Not yet: the Menu / Heroes / Pause / Win leftovers listed below.

### Style reference (dark purple + gold)

Used by Pause/Settings and the HUD — reuse for other screens.

| Role | Value |
|---|---|
| Card (Upgrade / Shop / Campfire) | child `Bg` = `Frame/CardFrame_01_Bg` `(0.16, 0.13, 0.19)` inset 8 → `Frame/CardFrame_01_BorderGray` (perks: `BorderPurple` + `DecoLeft` mirrored) → icon slot `Frame/ItemFrame_01_Bg` `(0.36, 0.30, 0.42)` + `ItemFrame_01_Border_Yellow`; button `Button/Button_Rectangle_01_Convex_Yellow` with brown text `(0.592, 0.208, 0.016)` |
| Plate | outer `UI_Etc/Switch_Bg_White_Border` tinted frame `(0.432, 0.347, 0.462)`, inner child `Bg` = `UI_Etc/Switch_Bg_White_Bg` `(0.09, 0.071, 0.094, 0.9)` inset 2px (the border sprite is solid, so it must be the bottom layer). Below 60px height set `pixelsPerUnitMultiplier` 1.6 |
| Bar | `Slider/Slider_Handle_Bg` dark → `Slider_Handle_Fill` (Sliced, inset 4px) → `Slider_Handle_Border` frame colour on top |
| Accent / headings | gold `(0.965, 0.882, 0.612)`, orange-gold fill `(0.98, 0.69, 0.216)`, panel `(0.224, 0.204, 0.255)` |
| Gotchas (editor scripting) | Setting `TMP_Text.font` on an object in an open **scene** from editor code may not persist — set `m_fontAsset` / `m_sharedMaterial` through `SerializedObject` and re-check after reopening the scene. Prefab contents (`LoadPrefabContents`) were not affected |
| Gotchas | Kit frame/border sprites (`Switch_Bg_White_Border`, `Listframe_01~02_Border`, `SlotFrame_Circle_White_Border`) are **solid shapes**, not rings: put the border underneath and the dark fill on top, inset 2px. The project renders UI in linear colour space, so a "nearly opaque" dim (alpha 0.95) still visibly leaks bright text — use alpha 1 when the screen behind must disappear |
| Fonts on the battlefield | `Alata-Regular-Outline 28 SDF` (small), `Alata-Regular-Outline 50 SDF` (headings) |
5. ~~Fonts~~ — done 2026-11-17. Two families now: **Alata** (body, buttons, numbers; `Alata-Regular-Outline *` variants on the battlefield, incl. damage numbers) and **Cinzel** (display: screen and section titles only). Lora, MedievalSharp and LiberationSans are no longer used by any scene/prefab. Note: TMP's default font in `TMP Settings` is still LiberationSans (third-party folder, left alone) — new TMP objects must get Alata assigned by hand.

## Per screen

**Menu** — done 2026-11-17: working title **Duskward** (Cinzel text, kit logo images disabled), "PLAY", labelled round Settings / Quit buttons. Settings is now its own prefab `Resources_moved/UI/SettingsScreen.prefab`, nested in `PauseScreen` and instanced in the Menu scene (`MenuUI.settingsScreen`).

**Heroes (LoadoutSelect)** — done 2026-11-17: slots without a hero (`HeroTypeId.Unknown`) are hidden by `LoadoutSelectUI` (a slot shows up once its `heroTypeId` is set), the first hero is pre-selected, stat bars show the real value (`StatBar.valueText`) with reference maxima tuned in the scene, hero descriptions rewritten.

**Map** — restyled 2026-11-17 (round kit nodes 96 px: ring `SlotFrame_Circle_White_Border` tinted by state + inner `SlotFrame_Circle_White_Bg`, gold `FocusGlow` and a type caption only on available nodes; "LVL n" / "LOCKED" labels removed; palette edge colours; light purple tint; HUD-style balance plate). `MapNodeView` no longer builds visuals in code — the hierarchy lives in `MapNode.prefab`.
- The map is a vertical `ScrollRect` (10 rows), so the top rows are reachable by scrolling — a `TopFade` gradient now hints at that. Scrollbars are invisible; consider a visible one or auto-scroll to the current row if players miss it.

**Pause** — done 2026-11-17: purple dim (alpha 0.9) over the frozen game instead of the opaque panel, Cinzel title, Continue is a big gold button, Settings / Restart / Give Up are a row of smaller round buttons. The HUD is hidden while paused (`HudUI.SetVisible` toggles only its Canvas — `PauseInputHandler` lives on the HUD object and must keep running).

**Settings**
- Resolution dropdown is the unstyled default and very small.
- "Stats" row touches the bottom edge; Music and SFX share the same icon.

**Choose Upgrade** — restyled 2026-11-17 (purple dim over the paused game, kit `CardFrame_01` cards: grey frame for stats, purple frame + corner decor + "PERK" tag for perks via `UpgradeButton.perkFrame/statFrame`, whole card is the click target, gold Reroll with coin). Remaining:
- (Done 2026-11-17: all 11 perks use label-free icons from `Assets/500FreeSkillIcons/Icons` instead of the labelled Gemini sheet; stat upgrades keep the labelled icons since their labels match. Description box is 80 px with auto-size down to 13 so long texts never reach the button.)
- Stat upgrade names mix "sprint" and "stamina" (`+ 100 max sprint`, `+ 20 regen sprint rate`) while the HUD and descriptions say Stamina; the asset file `+10 regen sprint rate` actually gives 20. Consider renaming for consistency.

**Shop** — restyled 2026-11-17 (painted background kept under a purple dim, opaque kit cards with an icon slot, coin + gold price that turns red only when unaffordable and hides the coin when SOLD, gold Buy / Leave buttons, balance plate like the HUD; `[Upgrade]` prefix removed from the three `itemName`s). Remaining:
- (Descriptions for all stat upgrades and shop items were filled on 2026-11-17 from what the code actually does; shop cards grew to 460 px to fit them.)

**Campfire** — restyled 2026-11-17 (background stretched 16% past the right/bottom edge so the Gemini watermark is off-screen, light purple dim, kit cards like the Shop, gold buttons, reward button "Get" → "Continue"; the rest label now shows the real heal % incl. the `CampfireWarmth` meta upgrade). Remaining:
- The reward card always uses the grey frame — could use the purple perk frame like Choose Upgrade when the rolled upgrade is a perk.

**Win / Game Over** — done 2026-11-17: HUD hidden (via `IGameFactory.Hud`), purple dim 0.9, Cinzel title, "Play Again" is the primary gold button, Menu a smaller dark one below. Remaining:
- The title frame (`LineTextFrame_06_Demo`) is red on both screens — fine for Game Over, odd for Victory; a gold/neutral frame would read better.

**Meta upgrade tree** — restyled 2026-11-17 (gold buy button with essence gem, `statusText` shows MAX / branch requirement instead of a dead button, Cinzel title and branch headers, framed dark branch panels and nodes, essence plate like the HUD balance).

## Other notes

- Feel gotcha (cause of the old "cards don't flip" bug): an `MMF_Player` with `AutoChangeDirectionOnEnd` keeps its current `Direction` in the prefab. If it gets saved as `BottomToTop`, the autoplay runs the sequence backwards. Keep `Direction = TopToBottom` in prefabs.

## How to review screens through Unity MCP

- Open `Assets/Scenes/Initial.unity`, enter Play Mode, then set `Application.runInBackground = true` via `execute_code` — otherwise frames barely advance while the Editor is unfocused.
- Jump between screens with `AllServices.Container.Single<IGameStateMachine>().Enter<TState>()` (`PauseState`, `UpgradeState`, `ShopState`, `CampfireState`, `GameOverState`, `MenuState`); click buttons with `button.onClick.Invoke()`.
- Screens load asynchronously: take the screenshot one call after entering the state. Use `manage_camera` `screenshot` without a camera (overlay canvases are only captured in Play Mode).
- `GameOverState` / `WinState` call `CompleteRun` and write the `MetaProgress` PlayerPrefs key — back it up before and restore after.
