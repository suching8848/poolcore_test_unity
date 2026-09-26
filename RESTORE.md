# Restore Poolcore

The Git repository root is the Unity project root. Clone https://github.com/suching8848/poolcore_test_unity.git and add the cloned folder to Unity Hub. Use Unity 6000.6.0f1 (see ProjectSettings/ProjectVersion.txt) and allow Unity to restore packages and regenerate Library.

Open Assets/Poolcore/Scenes/Poolrooms.unity, then press Play and choose the entry button. The spawn is the small window room: forward through the narrow passage reaches the milk-white hall; behind it is the route back to the original depth hall. Foundation remains a separate regression scene.

Assets, their .meta GUIDs, baked lightmaps, reflection cubemaps, Packages, ProjectSettings, editor builders, runtime code, audio credits and documents are versioned. Archive/Development preserves original audio, historical snapshots and selected diagnostics. PROGRESS.md records implementation and previous test results.

Library, Temp, Logs, UserSettings and Builds are generated/local data and are not in Git. Build Windows from the Poolrooms scene to regenerate Builds/Poolrooms/Poolrooms.exe. Do not run the scene builders just to open the saved map: the saved scene already contains the finished geometry and lighting. The optional Unity Pipeline editor automation package is declared in Packages/manifest.json.

At archival time no new gameplay changes or runtime tests were performed; this operation preserved the previously validated implementation and checked remote Git recovery.
