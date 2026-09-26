# Progress

## 2026-09-27 — remote preservation and local cleanup

User requested Git synchronization followed by deleting the local project. Prepared all project source/assets/settings and their metadata for the existing origin/main. Preserved unique ignored audio sources, historical scene snapshots, diagnostic code and selected validation evidence in Archive/Development with SHA-256 manifest. Added RESTORE.md. No gameplay code or scene changes in this archival pass. Remote checkout verification precedes local removal; the operation result is reported in the task conversation.

## 2026-09-13 — window room, narrow passage and milk-white pool hall

Added ContrastWingBuilder as a targeted live Editor scene pass and as the final step of the Poolrooms generator. New wing connects through the original hall north wall. The 6×6 m window room (3.2 m high) leads through a 13 m passage (1.7 m overall width, approximately 1.48 m clear, 2.5 m high) into a 40×36 m, 12 m high hall. Main pool is 28×22 m with a shallow floor and six-metre-wide entry ramp. Windows are actual wall openings; collider-only panes prevent leaving the room. Thick sills/mullions, open-air white courtyard screens and terraces hide unfinished exterior edges. Two high passage windows and a restrained bounce light preserve orientation in the dark transition. No world labels added. Spawn now starts in the small window room; the old halls remain reachable behind it.

New surfaces and local grading are independent of the previously approved water materials. The new water clones Atrium settings, has its own planar reflection and WaterZone. Reverb blends through the old-hall link, short small-room/passage acoustics and a 3.1 s new-hall tail. Footstep feedback: stopped selecting Wade1/3/5, kept Wade2/4/6, reduced their below-700-Hz component by 75%, narrowed pitch variance to ±1%, and reduced runtime gain. This targets the prominent short bubbly transients, but the user's subjective identification has not been confirmed by listening together.

Initial 14-point CharacterController route passed: small room, passage, both banks, pool entry/exit, return through the old hall and back. First visual pass revealed a too-dark passage and exposed external building edges; added high windows/courtyard screens, then rebaked. Ramp includes secondary lightmap UVs. Five comparison views inspected; the passage now remains readable and exterior screens hide floating/default-world edges. Bake audit: 83 new non-water renderers assigned to four active lightmaps, no missing lightmaps, zero world labels, five water zones. Initial build exposed a null Volume component during URP post-build analytics: the new profile overrides had not been added as subassets. Fixed generator and live profile to persist both overrides, enabled triangle collision prebaking on the ramp, and moved the small courtyard south screen one metre off the old hall wall to avoid overlapping faces. Final rebake and five reflection captures completed. Build build_9a884f12248c succeeded with zero errors and the four existing package/internal shader warnings; the missing-volume and ramp-collision issues no longer appear. New-water turning test passed 108 views (three positions including wading height, two pitches, full yaw sweeps), zero rendering errors. Standalone offscreen route PASS at 180.0008 seconds: 30 waypoints, 346 dry steps, 19 wading steps, three selected wading variants, 11 environment emitters, zero world labels, zero runtime errors, exit code 0. Median offscreen frame 1.362 ms / p95 3.236 ms (not a visible-play FPS claim). Runtime screenshots revealed exposed courtyard sides at grazing angles near the large windows; added four exterior end walls. These do not change the walkable route. Final rebake completed: 87 non-water renderers assigned to lightmaps, none missing. Seven final captures include the two formerly exposed grazing-angle window views; both now show enclosed white courtyards. The full 180-second route predates these four non-walkable exterior end walls; the final exterior correction is covered by rebaking, visual inspection and a fresh build. Release build build_a448e84078c8 succeeded with zero errors and the same four existing package/internal shader warnings. Output: Builds/Poolrooms/Poolrooms.exe. Editor left outside Play, saved Poolrooms, camera aligned to the new small-room spawn (40.5, 0.06, 42). A final Editor-only change marks existing volume subassets dirty when rerunning the generator; player code/assets are identical to the tested build.


## 2026-09-13 — spatial recorded audio and no world text

Removed all four world-space depth labels and their backing plates from Poolrooms through live Editor APIs; removed their generator code and obsolete repositioning code. Disabled automatic area-name captions; pause/settings and explicitly requested photo/performance feedback remain. Rebaked lighting and refreshed fallback reflection cubemaps after removing the plates.

Replaced procedural ambient loops with processed CC0 field recordings of ventilation (morosopher) and a swimming-pool pump (kijjaz). Added six short recorded wading excerpts (craigsmith), varying without immediate repetition, triggered by grounded travel distance with a minimum interval and depth-dependent gain. Stationary movement resets the partial stride and does not trigger water sounds. Existing ceramic footstep retained. Source URLs, provenance and edits are in Assets/Poolcore/Resources/ImmersionAudio/CREDITS.md; public HQ MP3 previews were used, not lossless originals. 24 kHz mono PCM clips are preloaded and shared. Numeric checks show finite samples and no clipping.

RoomSound provides nine physical sources with distance attenuation, wall-ray occlusion at 5 Hz, smoothed gain and low-pass filtering. RoomAcoustics provides one listener-centred reverb zone with smooth spatial transitions: hall target 2.9 s, low gallery/recess 0.85 s, other rooms 1.65 s. Live checks: one AudioListener, one AudioReverbZone, nine loaded/playing ambient sources, zero world TextMeshPro objects. Recess snapshot confirmed 0.85 s decay and blocked-source filtering to 650 Hz / 22% gain; open sources remain at 7 kHz. Audio sample/trigger checks do not substitute for a subjective headphone listening pass. Occasional drip one-shots are not included in this pass; the ambient bed stays restrained.

Build build_583b72ca4ab3 succeeded with zero errors; it exposed four deprecation warnings in the new QA inventory code, subsequently changed to the Unity 6.6 overloads. Standalone offscreen 1920×1080 route PASS at 180.0004 seconds: 29 waypoints, 329 dry steps, 34 wading steps, all six wading variants loaded, nine room emitters, zero world labels, zero runtime errors, exit code 0. This counts trigger events, not subjective listening. Median render frame 1.449 ms / p95 3.242 ms; offscreen uncapped metrics are not visible gameplay FPS. Profile sweep across 900 centimetre steps passed, maximum decay delta 0.00909 s per centimetre. Four baked lightmaps remain active. Source diff whitespace checks pass; Unity's own scene serialization retains empty-field trailing spaces and was not manually edited.

Final build build_cf25884c05b8 succeeded with zero errors and one existing warning: Player Pipeline is disabled because no RuntimePipelineConfig is supplied. The only final source change was replacing deprecated QA object-search overloads with their equivalent unsorted overloads; gameplay/audio code is identical to the 180-second tested build. Output: Builds/Poolrooms/Poolrooms.exe.


## 2026-09-12 evening — hall architecture and material finish

Implemented HallArchitecturePass on the current Poolrooms scene, integrated after HallAtmospherePass in the generator. Added a west-side low gallery and a walkable 4×6 m recess with a bench and restrained warm fixture. Kept the warm-room connector, pool floor, wading boundary and still-water shader unchanged. Slimmed the hall columns, added capitals/base collars, aligned the five main beams with the column rows, and narrowed the eastern skylight to 4 m while retaining the 6 m western slot. Moved the west deep-water sign back onto solid wall after opening the recess.

Extended Porcelain with rectangular tile proportions, joint width/contrast, subtle per-tile tone and glaze variation, matte upper walls and a static damp-edge material mask. Hall walls now use 0.6×0.3 m lower tiles with matte upper walls; deck tiles are 0.55 m and pool tiles 0.22 m. Filtered fixed-width grout rather than widening it with distance, and faded subpixel joints. The damp edge only changes color/smoothness; it does not add reflections, waves or movement friction.

Rebaked four active lightmaps and refreshed all four fallback reflection cubemaps. Inspected six hall/recess/material views at artifacts/hall-finish/view-0.png through view-5.png. Passed nine new recess/gallery movement points, all 15 existing hall route points plus the wading barrier, and all 23 original experience checks. Column/pool geometry audit still passed. Reflection regression passed 70 valid height/FOV/pitch samples (maximum RGB range 0.0880 below 0.10) and 144 turning views with zero errors. Source diff whitespace checks passed.

First Windows build build_290e4ac321c6 succeeded with zero errors and four existing package/internal shader warnings (disabled Player Pipeline, two stripped internal debug shaders, TMP deprecated debug-symbol pragma). Report: artifacts/hall-finish/build-report.json. The standalone hall QA route now includes entering and leaving the new recess. Initial 180-second run PASS: 29 waypoints, zero runtime errors, exit code 0. The runtime log exposed an additional-light shadow-atlas downscaling warning; explicitly set the small recess point light to URP's low shadow-resolution tier through SerializedObject (the public tier setter is Play-only). No geometry, baked lightmap or light intensity changed in this final budget adjustment. Rebuilt successfully as build_5ebd41818371, zero errors, same four build warnings; report artifacts/hall-finish/final-build-report.json.

Final rerun PASS: 180.000 seconds, 1920×1080, 29 waypoints, zero runtime errors, exit code 0. Shadow-atlas downscaling no longer appears. Report: artifacts/hall-finish/final-runtime/report.json; log: artifacts/hall-finish/final-player.log. Recorded 121,761 explicit offscreen frames, median 1.322 ms, p95 2.915 ms, p99 3.595 ms, three frames above 100 ms. These are offscreen QA timings on the RTX 4080 Laptop GPU, not visible-window refresh-rate claims. Updated Builds/Poolrooms/Poolrooms.exe; current Editor scene saved, outside Play Mode. No changes staged or committed.

## 2026-09-12 — upward-look reflection fix and hall atmosphere

Reproduced the user's reflection disappearance at camera (58.37, 0.59, 11.63). The architecture shader incorrectly converted oblique projection clip Z into fog distance. Disabling fog isolated the cause; using actual per-pixel view depth with ComputeFogFactorZ0ToFar fixed it while preserving fog, planar reflections and water transmission. The same fixed world-space reflected patch used to drop from red 0.583 to 0.159 between 5 and 15 degrees upward; after the fog fix it changed only from 0.548 to 0.538. Before/after views and metrics are in artifacts/water-angle.

Applied HallAtmospherePass to the existing scene and integrated it with the full-scene generator. Reduced hall fill lights, warmed/softened wall and ceiling materials, added a blended local Volume, skylight reveals, wall friezes, coping, combined drain-grate mesh, wall vents and four static-font world-space depth signs. Added HallRoomTone with two quiet ventilation sources sharing one 24-second mono loop and a restrained long-room reverb. Removed the old repetitive synthesized water ambience from this hall only; wading steps remain silent. Audio validation confirms both sources playing, peak 0.0644, RMS 0.01456, loop-edge difference 0.000335. These signal checks do not establish subjective audio quality.

Rebaked scene lighting (three active maps), refreshed all four fallback reflection cubemaps and inspected four hall views plus built-player screenshots. All 15 hall route points plus wading boundary passed; all 23 existing experience checks passed. Geometry still reports no columns over pool openings and depths 0.37 / 1.745 / 3.12 m.

First 180-second standalone run completed 23 waypoints but FAILED because of two native screen-frustum errors. Kept that report at artifacts/depth-hall/atmosphere-runtime/report.json. Added an explicit ordinary mirrored cullingMatrix to PlanarWaterReflection, retaining the oblique projection only for raster clipping. Subsequent 144-view rotation sweep had zero errors; the 70-sample height/FOV/pitch reflection check passed with maximum RGB range 0.0697 (threshold 0.10). Final incremental Windows build succeeded with zero errors and one disabled-Player-Pipeline warning, build id build_1e694d67dc91. Report: artifacts/depth-hall/atmosphere-final-build-report.json.

Final standalone rerun PASS: 180.000 seconds, 1920×1080, 23 waypoints, zero runtime errors, exit code 0. Report: artifacts/depth-hall/atmosphere-final-runtime/report.json; player log: artifacts/depth-hall/atmosphere-final-player.log. It recorded 139,396 offscreen-rendered frames, median 1.197 ms, p95 2.462 ms, p99 2.893 ms and four frames above 100 ms. These are this machine's explicit offscreen QA timings, not a claim about visible-window refresh rate or a like-for-like comparison with prior VSync-limited runs. The Editor scene is saved outside Play Mode. Normal launch remains Poolrooms at the large hall south bank. No parent-project changes were made or Git changes staged.

## 2026-09-11 — large hall with shallow/deep transition

Added a connected 40×40 m, 11 m high depth-test hall east of the warm pool. The 28×28 m basin has a dry-to-shallow entry slope, 0.37 m shelf, 10 m transition slope (15.38 degrees), and 3.12 m deep end. All use one continuous still-water plane and depth-dependent absorption, with the hall's tint calibrated toward blue at depth. Two wide skylights, structural beams and baked fill lighting provide large-space shadow and reflection comparisons. Current player spawn is the new hall's south bank; the west door returns to the existing warm pool.

Fixed the Quiet chamber's eight-column row positions so their entire footprints are on the banks; the geometry audit found zero pillar footprints over pool openings. Split the warm room bench around the new doorway and added bench supports. All four custom basin meshes have flat face normals, secondary lightmap UVs, real MeshColliders and explicit triangle-collision prebaking enabled.

Validated actual depths 0.37 / 1.745 / 3.12 m and slope 15.376 degrees. New route validation passed all 15 waypoints through the connector, around the basin, down/up the entry slope and back to the warm room. The visible safety rope's wading boundary stopped the player at z=11.60, y=-1.05; swimming remains unimplemented and the deep end is observed from the bank. Existing route/wading/HDR regression passed all 23 checks. Reports and four inspected views: artifacts/depth-hall. See DEPTH_HALL.md for layout and operation.

Completed CPU lighting bake with three active lightmaps, including the new hall floor on lightmap index 0, refreshed all four pool reflection cubemaps, and saved the scene. Added `-qa-depth-hall` to opt-in runtime QA to exercise the large hall directly.

Large-hall rendered benchmark: PASS, 180.002 seconds at 1920×1080 on RTX 4080 Laptop GPU; 21,202 frames, 23 waypoints (over two nine-waypoint loops), zero runtime errors. Average 119.79 FPS, p95 8.335 ms, p99 8.427 ms, one frame over 100 ms. This measures explicit offscreen rendering including planar reflections on this machine. Data: artifacts/depth-hall/runtime/report.json. Following the benchmark, enabled explicit collision-prebake metadata on the same four meshes; raycast depths and pillar checks still passed. No mesh geometry changed in that final metadata update.

Final Windows incremental build: Succeeded, zero errors, one remaining Pipeline configuration warning. Updated executable: Builds/Poolrooms/Poolrooms.exe. Full report: artifacts/depth-hall/final-build-report.json. The new mesh collision-prebake warning is resolved.

## 2026-09-11 — calm water and live architectural reflections

User asked to prioritize still pool water, light and reflections over interaction. Replaced the animated water normals, synthetic caustic emission and foot ripples with a flat water surface using RGB depth absorption, transmission and Fresnel blending. Added live planar reflections for all three pools through URP SingleCameraRequest, oblique clipping, Water-layer exclusion, a recursion guard, bounded half-resolution HDR textures and slight mip softness. Reflected cameras do not apply post-processing twice. Preserved land footsteps; removed the synthetic wading clip and temporarily silenced water footsteps. Updated scene materials, fallback cubemaps and the generator's water setup. Details: WATER_LOOK.md.

Validation: the predicted 25×25 reflected-marker region changed in all 625 pixels when the real marker was removed; fixed-view consecutive frames with AA disabled had mean RGB difference 0. Inspected all three rooms and a close pool-edge view. Editor console had zero errors. The final Windows build succeeded with zero errors and four existing package/internal shader warnings; report at artifacts/water/build-report.json.

The first hidden-player backbuffer captures were black and were rejected as evidence. Added opt-in `-qa-offscreen` to explicitly render the real scene and reflections each frame into a 1920×1080 sRGB target. This also avoids incorrectly saving linear HDR pixels as a dark PNG. Normal launches are unaffected. The earlier interrupted run has no complete report and is not considered a passed benchmark.

Final benchmark completed: PASS, 180.005 seconds, 21,197 rendered frames, 47 completed waypoints (over two full loops), zero errors. RTX 4080 Laptop GPU at 1920×1080: average 119.75 FPS, median 8.334 ms, p95 8.349 ms, p99 8.471 ms, one frame over 100 ms. This is an explicit offscreen rendering benchmark on this machine, not a guarantee for all hardware or an OS-input test. Verified nonblack sRGB screenshots at startup and in both other pools. Report/screenshots: artifacts/water/final-runtime; log: artifacts/water/final-player.log.

## 2026-09-11 — pool rim depth-fighting fixed

Latest user-reported defect: a strip appears/disappears at the pool edge when looking around. Read DeepSeek's handoff and independently reproduced actual coplanar depth fighting. A 41-angle, 100-world-point-per-angle material-ID test produced 1,500 ownership changes before repair and zero after, with water/post-processing/AA disabled. DeepSeek's earlier exclusion of z-fighting was incorrect for this camera sweep.

Replaced the three pools' straddling walls with inward 8 cm rims, preserving visible width. Partitioned the corners and stair entrances so no rim tops overlap decks, stairs or other rims. Existing scene repaired through Editor APIs; generator uses the same geometry helper. All 15 segments pass the top-face overlap check. Preserved TAA settings. Source scene backup and test reports are in artifacts/rim-fix; detailed explanation in RIM_FIX.md.

Completed CPU lightmap rebake (3 active maps), recaptured all three water cubemaps and saved Poolrooms. Play Mode route regression passed all 20 waypoints plus water detection, 3.187 m / 2 s wading, and HDR/volume checks: 23 passes, zero failures. Inspected the normal-material pool-edge render after baking.

Updated Builds/Poolrooms/Poolrooms.exe: Succeeded at 2026-09-11 17:02:54 local, 133,730,822 bytes, zero errors, four warnings. Warnings are disabled Player Pipeline remote control, two stripped internal occlusion debug shaders, and an upstream TMP deprecated shader pragma. Full report: artifacts/rim-fix/build-report.json. The older Builds/Windows/Poolcore.exe is still the Foundation prototype.

Built-player background startup smoke check passed: process remained running, initialized Input System and Direct3D 11 on the RTX 4080 Laptop GPU, and produced no Error/Exception/Failed log matches before the test process was stopped. Log: artifacts/rim-fix/player-smoke.log. This is a startup check, not an interactive input or sustained frame-rate test.

This verifies the reported overlapping-strip defect and route regression; it is not a claim that all texture aliasing across all graphics hardware has been eliminated. No unrelated parent-repository files were changed.

## 2026-09-11 — autonomous V0.2–V0.5 implementation checkpoint

User authorized continued development until usage limits. Added separate Poolrooms.unity: three pools, connected passages and loop gallery, slotted roofs, porcelain world-space tile shader, transparent depth-based water with captured cubemap reflections and foot ripples. Added wading, procedural footstep/water ambience, spatial emitters/reverb, Chinese TMP menu with sensitivity/FOV/volume/quality persistence, cursor-safe resume and photo/performance hotkeys.

Compiled successfully, created scene and inspected rendered first-person view and live Chinese menu. Recovered a Unity modal provenance prompt caused by the installed uGUI package's TMP Essential Resources importer; the resources were verified to originate in the project's local uGUI package. Earlier console timeouts came from that modal.

Current checkpoint: soft indirect-light bake requested through Poolcore/Bake Soft Lighting. New ExperienceValidation validates the three-room route, wading and post-processing. Final bake/route/build outcome to follow. The existing Builds/Windows executable remains the previously verified Foundation V0.1 until explicitly replaced by a successful new build. Do NOT describe the new experience as fully validated yet.

Quota check near checkpoint: 5-hour used 95%, weekly used 100%; no reset credits used. Next priorities: finish bake, run ExperienceValidation.ValidateRoute() in Play Mode, fix failures, CaptureViews(), verify UI interactions, build Windows, smoke-test and update this document.

## 2026-09-11 — V0.0 / V0.1

Implemented:
- Dedicated Foundation scene, preserving template SampleScene.
- Runtime movement settings asset, player prefab, Input System keyboard/mouse controller.
- CharacterController gravity/collision, normalized diagonal movement, faster walk, slope/step handling and fall reset.
- Click capture, Esc release, focus/pause release; movement stops while cursor is released.
- Greybox concourse, empty shallow basin, entry steps, ramp/landing, columns and corner obstacles.
- Project conventions, roadmap, playtest instructions and scoped Unity Git exclusions.

Verified in actual Editor Play Mode through MovementValidation.Run():
- 1 second forward = 2.400 m; diagonal = 2.400 m.
- Faster walking = 3.800 m/s.
- Wall collision stops at x=11.68 before boundary x=12.
- Ascended 0.2 m basin steps and returned to concourse.
- Descended into basin and grounded at y=-0.78 (skin offset).
- Ascended 15 degree ramp.
- Recovered from y=-20 to spawn.
- Rendered and visually inspected artifacts/foundation.png.
- Git check-ignore confirmed Library, Logs and Builds are excluded.

Windows build: Succeeded, StandaloneWindows64, 116,200,485 bytes, 0 errors, 3 warnings. Full report: artifacts/windows-build-report.json. Warnings: Player Pipeline remote debugging is disabled because no RuntimePipelineConfig was configured; two internal occlusion debug shaders were stripped. Editor CLI remains connected.

Built-player smoke check: launched the produced executable in batch/no-graphics mode for 8 seconds, confirmed it stayed running and initialized the Input System, with no Error/Exception matches in its log, then stopped only that smoke-test process. This checks startup, not visible rendering or frame rate. Log: artifacts/player-smoke.log.

Remaining manual acceptance: ten-minute comfort test, corner sliding feel, cursor capture/Alt-Tab transitions while holding keys, and sustained 1080p frame rate. Automated movement checks do not simulate OS keyboard/mouse interaction. Water, polished lighting, audio and settings UI are future scope.

No parent-repository changes were staged or committed; unrelated existing changes were preserved.
