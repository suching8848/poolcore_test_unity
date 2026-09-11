# Progress

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
