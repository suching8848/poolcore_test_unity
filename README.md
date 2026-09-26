# Poolrooms / 漂浮之间

Latest addition: the 40×40 m **深浅水大厅** is connected to the east side of the warm pool. Current spawn is in the new hall for immediate inspection. Water gradually deepens from 0.37 m to 3.12 m over a sloping basin. A safety rope limits wading; walk around the bank to inspect the deep end. See DEPTH_HALL.md.

Open this directory in Unity **6000.6.0f1**. Open `Assets/Poolcore/Scenes/Poolrooms.unity` and press Play. The Chinese menu is created at runtime; click 进入空间 to explore. The original `Foundation.unity` remains a separate regression-test scene.

Poolrooms contains a daylight atrium, a warm pool and a quiet column room connected by a loop of passages. Three baked lightmaps provide indirect light. Still water combines real-time planar reflections with depth-dependent absorption and transmission. Wading changes walking speed; water footsteps and interaction ripples are currently disabled. Land footsteps and spatial room ambience with reverb remain.

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
- `Tools/diag/recess-route.cs` walks the new low gallery and warm bench recess; `hall-finish-views.cs` captures six architectural/material views. The built-player `-qa-depth-hall` route includes the recess.
- `Tools/diag/water-angle-regression.cs` renders 70 valid height/FOV/pitch samples and checks a fixed reflected patch for abrupt changes. `reflection-turn-regression.cs` renders 144 turning views and checks native rendering errors. See WATER_LOOK.md for the oblique-projection fog fix.
- `Poolcore/Refine Depth Hall Atmosphere` applies the hall lighting, local color grade, coping/drains, static depth signs and quiet room tone. It replaces its own generated detail root; preserve hand edits first. See DEPTH_HALL.md.
- For a hidden/background player, add `-qa-offscreen` to the route benchmark. It explicitly renders the scene and water reflections into a 1920×1080 target and captures that target; ordinary background backbuffer screenshots can be black and are not valid visual/performance evidence.
- Play Mode: `ExperienceValidation.ValidateRoute()` walks 20 waypoints using the real CharacterController, checks wading and HDR/Volume setup. Output: artifacts/experience-validation.txt.
- Built player: launch with `-poolcore-qa -qa-seconds 180 -qa-output <absolute-directory>` to run a visible rendered route benchmark, capture screenshots and write report.json/errors.txt before exiting. This opt-in mode does not run on ordinary launches.
- `Poolcore/Bake Soft Lighting` runs the CPU lightmapper. Save the scene after completion. `Poolcore/Capture Pool Reflections` refreshes water cubemaps after lighting changes.
- Poolrooms water now uses live planar reflections; cubemaps are a fallback. Water is still, with depth-dependent absorption and no animated caustic lines or foot ripples. Wading footsteps are temporarily silent; land footsteps and room ambience remain. See WATER_LOOK.md. `Poolcore/Apply Calm Reflective Water` reapplies the current water material/component defaults.
- `ExperienceValidation.CaptureViews()` captures five camera views. These exclude screen-space overlay UI; use an actual Game-view capture to validate the menu.

## Manual acceptance
Walk through the room for ten minutes. Check corner sliding, turning while walking, ramp transitions and stair comfort. Test Esc, clicking to resume and Alt-Tab with a movement key held. Verify no movement sticks after returning. Mouse feel, focus transitions and sustained frame rate require this interactive playtest; the automated locomotion checks do not establish them.


## Immersion audio (2026-09-13)

Poolrooms has no world-space depth lettering or automatic location captions. Esc settings remain available. Hall ventilation and circulation use quiet 3D recorded loops with wall occlusion; the west recess has a shorter reverb than the main hall. Wading uses six recorded step excerpts, triggered only by grounded movement. Dry ceramic steps are retained. No visible water interaction has been added.

Audio provenance and processing: `Assets/Poolcore/Resources/ImmersionAudio/CREDITS.md`. Test reports: `artifacts/immersion-audio/`.


## Window contrast route

The current spawn is the small side-window room. Walk forward through the narrow passage to reach the new milk-white pool hall. The passage behind the spawn returns to the older depth hall. The new pool has a walkable entry ramp opposite the broad south deck. No jumping or swimming is needed. Editor menu: Poolcore → Add Window Contrast Wing. `-qa-contrast-wing` selects its standalone regression route.
