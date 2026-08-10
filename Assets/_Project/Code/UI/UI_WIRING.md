# UI Wiring — Menus, Pain HUD, Pause

Scripts are written and compiling. Everything below is Inspector work.

## 0. Build Settings (do this first, nothing else works without it)

`File > Build Settings` currently lists `Game_OLD.unity`. It needs:

| # | Scene |
|---|---|
| 0 | `MainMenu` |
| 1 | `SettingsMenu` |
| 2 | `Game` ← the new one, not `Game_OLD` |
| 3 | `EndScreenA` |
| 4 | `EndScreenB` |

Scene **names** are what the scripts use, not paths.

## 1. Main menu

Put **`MainMenuUI`** on the Canvas in `MainMenu.unity`. Fields default to `Game`, `SettingsMenu`, `MainMenu`.

Wire each button's `OnClick`: drag the Canvas into the object slot, then pick from the dropdown.

| Button | Function |
|---|---|
| Play | `MainMenuUI.PlayGame` |
| Settings | `MainMenuUI.OpenSettings` |
| Quit | `MainMenuUI.QuitGame` |

In `SettingsMenu.unity`, add another `MainMenuUI` and point the Back button at `MainMenuUI.BackToMainMenu`.

If a method does not show in the dropdown, it is under the **Dynamic** heading rather than **Static**, or the object slot is empty.

## 2. Scene managers (game scene)

Empty GameObject called `[Managers]`:

- `InputManager` (probably already there)
- `PauseManager` — owns `Time.timeScale`, nothing else should touch it

## 3. Pain HUD

On `[UI]GameVer3 > Canvas > GameScreen`:

**`BloodBarUI`**

| Field | Assign |
|---|---|
| Main Fill | The bar Image once it exists. Image Type **Filled**, Fill Method **Horizontal**. Leave empty for now. |
| Chip Fill | Duplicate of the bar in a lighter colour, placed **above** the main fill in the hierarchy so it draws behind. Optional. |
| Readout | The existing `BloodAmount` TMP object. Works today with no bar art. |
| Readout Format | `{0}%` |

**`DamageFlashUI`** — new child of `Canvas` named `HitVignette`, stretched full screen, last in the hierarchy:

- `Image` with a soft edged vignette sprite. A flat white sprite works but washes out the screen and hides the thing that just hit you.
- **Raycast Target off.** A transparent full screen image with it on silently eats every button click.
- Assign to `Flash Image`.

## 4. Pause overlay

On `[UI]GameVer3 > Canvas > [UI]PauseMenu`, add **`PauseMenuUI`**:

| Field | Assign |
|---|---|
| Pause Root | `PauseScreen`, the **child**. Not `[UI]PauseMenu` itself. |
| Buttons Panel | `PauseScreen > Buttons` |
| Settings / Controls Panel | Leave **empty** until those panels exist |
| Resume / Settings / Controls / Main Menu Button | Drag the four button objects from the Hierarchy |

> **Do not wire the buttons' OnClick lists.** Drag the buttons *into this script* instead. A button prefab cannot hold a reference to anything outside itself, so wiring OnClick in Prefab Mode saves the method name and silently drops the target to `None`, giving you a button that looks correct and does nothing. The script subscribes at runtime instead, which keeps every reference inside `[UI]GameVer3`.

> `[UI]PauseMenu` must stay **active** in the hierarchy. It holds the script. If Pause Root points at the same object the script is on, hiding the menu also switches the script off, Unity skips `OnEnable`, and it never hears the unpause. The script now refuses to run in that setup and says so.

> **Settings Panel and Controls Panel take panels, not buttons.** Assigning `SettingsButton` there makes that button disappear, because the field is hidden whenever the button column is shown. Leave both empty until the real panels are instanced.

> **The scene needs an EventSystem** (`GameObject > UI > Event System`). Without one, no UI anywhere in the scene is clickable and buttons will not even highlight. Kyle's level scene did not have one.

| Settings Panel | `[UI]Settings` instance under `[UI]PauseMenu` |
| Controls Panel | Holder containing the `[UI]ControlsA` instance |
| First Selected | Resume button |
| Main Menu Scene Name | `MainMenu` |

`PauseScreen > Buttons` is an empty layout group. Drop the button prefabs in and wire:

- Resume → `PauseMenuUI.OnResumePressed`
- `SettingsButton` → `PauseMenuUI.OnSettingsPressed`
- `ControlsButton` → `PauseMenuUI.OnControlsPressed`
- `MainMenuButton` → `PauseMenuUI.OnQuitToMenuPressed`
- Back buttons → `PauseMenuUI.OnBackPressed`

### Nesting `[UI]Settings` and `[UI]ControlsA` as panels

These two were built as standalone screens, not panels. Their root is a plain `Transform` wrapping a child `Canvas`, and that child's RectTransform is serialised at **scale 0, size 0** because a live Canvas drives those values and overwrites them. Drag the wrapper in and the layout chain breaks; delete the Canvas component and the zeros go live and collapse it to an invisible point.

Convert each one like this:

1. Drag the **inner `Canvas` child** into `PauseScreen`, not the `[UI]Settings` wrapper. Discard the wrapper.
2. Remove `Canvas`, `Canvas Scaler`, and `Graphic Raycaster`. Keep the RectTransform.
3. **Set Scale to `1, 1, 1`.** This is the step that makes it appear.
4. Set anchors to stretch, `min 0,0` / `max 1,1`, and Left/Right/Top/Bottom to 0.
5. Rename to `SettingsPanel` / `ControlsPanel` and assign to the matching field.

`PauseMenuUI` now warns on open if a panel has zero scale, zero size, or a non-RectTransform.

## 5. Settings

On the `[UI]Settings` instance add **`SettingsPanelUI`**, assign `BGMSlider` and `SFXSlider`. Leave the AudioMixer empty until a mixer exists; sliders still work and still save, BGM just drives `AudioListener.volume` as a stopgap.

## 6. Controls

On the controls holder add **`ControlsPanelUI`**, assign the `[UI]ControlsA` instance to `Diagram`, and point the Controls button at `ControlsPanelUI.Show` (or `Toggle`).

Both key layouts are live at once, so there is nothing to switch:

| | Move | Mist | Jump | Pause |
|---|---|---|---|---|
| Left hand | WASD | Z | X | Esc / P |
| Right hand | Arrows | `,` | `.` | Esc / P |
| Gamepad | Stick / Dpad | West | South | Start |

## Known data issues

- **`HitInfo1` and `HitInfo-Kyle` both have `MaxBloodPoints: 0`.** The player would start on empty and the HUD would read NaN. `PlayerController` now falls back to 100 and logs an error. Set a real value on the assets.
- Invincibility is now honoured. `InvincibilityTime` is 1 second on both assets, which is long for a fast game. Tune it down if hits feel unresponsive.

## Gotchas

- `PauseManager` sets `AudioListener.pause`. UI click sounds need **Ignore Listener Pause** on their AudioSource or they go silent in the menu. Untick `Pause Audio` if that is a nuisance.
- HUD animation runs on `unscaledDeltaTime`, so the bar keeps settling while paused instead of freezing mid lerp.
- The blood bar seeds itself from `PlayerController.Start`, so it is correct on load without waiting for the first hit.
