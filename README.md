# Poolrooms / 漂浮之间

Open this directory in Unity **6000.6.0f1**. Open `Assets/Poolcore/Scenes/Poolrooms.unity` and press Play. The Chinese menu is created at runtime; click 进入空间 to explore. The original `Foundation.unity` remains a separate regression-test scene.

Poolrooms contains a daylight atrium, a warm pool and a quiet column room connected by a loop of passages. Three baked lightmaps provide indirect light. Water uses depth-aware refraction and captured static cubemaps (not real-time planar reflections). Wading creates procedural ripples and changes walking speed and footsteps. Ambient water emitters are spatial, with room reverb.

## Controls
- In Poolrooms, click 进入空间 to capture the mouse. In Foundation, click inside the Game view. When the Editor Game view is not focused, the first click may focus it and the next activates the button.
- WASD: walk; mouse: look; Left Shift: faster walk.
- Esc: release mouse and open settings. Click 进入空间 to resume.
- Switching applications releases capture; click again after returning.
- F2: save a photograph to the Photos directory under Application.persistentDataPath (the in-game confirmation shows the path).
- F3: toggle frame-rate display.
- No jumping or swimming. Use the pool entry steps to enter/exit shallow water.

Foundation only: the opening view faces the empty basin. A ramp is on the left; an L-shaped wall collision test is on the right.

Tune base speeds in `Assets/Poolcore/Settings/Movement.asset`. Poolrooms saves sensitivity, field of view, volume and quality through PlayerPrefs; saved user settings override those defaults. Make persistent asset changes outside Play Mode. Runtime quality uses a cloned URP asset and restores the original when the session ends.

## Development
Authored assets live under `Assets/Poolcore`. `Player.prefab` is the reusable player setup. The template SampleScene remains available. `Poolcore/Create Foundation Scene` regenerates the baseline test scene and overwrites its generated layout; preserve hand-authored changes separately before regenerating.

Use the connected Unity CLI with `--project-path` pointing at this directory. The local CLI executable is `C:/Users/15792/AppData/Local/Unity/bin/unity.exe`.

In Play Mode, invoke `Poolcore.Editor.MovementValidation.Run()` through editor eval/reflection to exercise the actual CharacterController against the test scene. Results go to `artifacts/movement-validation.txt`.

The verified CLI build command is `unity command build --target StandaloneWindows64 --outputPath Builds/Windows/Poolcore.exe --confirm true --project-path <project-root> --format json`. Poll `unity command build_status` with the same project path to confirm completion. The latest report is saved in `artifacts/windows-build-report.json`. Keep the Windows executable together with its Data folder and required DLLs when sharing; the BackUpThisFolder_ButDontShipItWithYourGame folder is not needed for distribution.

Poolrooms builds target `Builds/Poolrooms/Poolrooms.exe`; its separate report is `artifacts/poolrooms-build-report.json`. The older `Builds/Windows` folder contains the Foundation release.

## Automated checks
- Play Mode: `ExperienceValidation.ValidateRoute()` walks 20 waypoints using the real CharacterController, checks wading and HDR/Volume setup. Output: artifacts/experience-validation.txt.
- Built player: launch with `-poolcore-qa -qa-seconds 180 -qa-output <absolute-directory>` to run a visible rendered route benchmark, capture screenshots and write report.json/errors.txt before exiting. This opt-in mode does not run on ordinary launches.
- `Poolcore/Bake Soft Lighting` runs the CPU lightmapper. Save the scene after completion. `Poolcore/Capture Pool Reflections` refreshes water cubemaps after lighting changes.
- `ExperienceValidation.CaptureViews()` captures five camera views. These exclude screen-space overlay UI; use an actual Game-view capture to validate the menu.

## Manual acceptance
Walk through the room for ten minutes. Check corner sliding, turning while walking, ramp transitions and stair comfort. Test Esc, clicking to resume and Alt-Tab with a movement key held. Verify no movement sticks after returning. Mouse feel, focus transitions and sustained frame rate require this interactive playtest; the automated locomotion checks do not establish them.
