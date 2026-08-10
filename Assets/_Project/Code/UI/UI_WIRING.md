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
| Pause Root | `[UI]PauseMenu` itself |
| Buttons Panel | `PauseScreen > Buttons` |
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
