# Poolcore development plan

Goal: a quiet first-person poolcore exploration experience for Windows, built on reliable movement, lighting and reusable rooms.

## Approved: V0.0–V0.1
- Create authored directory layout, documentation and Unity Git exclusions.
- Build a dedicated Foundation scene, preserving the template SampleScene.
- Input System keyboard/mouse control; CharacterController collision, gravity, slopes, steps, normalized diagonals, faster walking.
- Click to capture cursor, Esc/focus loss to release, no movement while released, fall recovery.
- Configurable movement settings and reusable player prefab.
- Greybox testing: walls/corners, columns, ramp, steps, shallow empty basin.
- Validate locomotion with actual PhysX scene geometry and inspect a rendered camera view.
- Verify Windows build; document manual comfort/focus checks still requiring user playtesting.

## Later
V0.2: one pool room with tile materials, soft light, baked indirect light and reflection probes.
V0.3: shallow water, caustics, footsteps and spatial ambience.
V0.4: pause/settings UI, optimization, reusable architecture and Windows playtest release.
V0.5: three connected spaces and a looped exploration route.

Initial performance target: 1080p 60 FPS on the user's PC, to be measured in the built player. No paid assets, combat, jumping or swimming assumed.
