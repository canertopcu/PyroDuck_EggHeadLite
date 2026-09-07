# Editor campaigns

- Add the UI Toolkit About PyroDuck window and optional remote Featured carousel.
- Include a configurable feed, bundled fallback, scheduling, cache and local opt-out.
- Keep promotion code in Editor assemblies; no runtime changes.

# Compatibility update

- Use Assets/PyroDuck/EggHeadsLite for .unitypackage distribution; remove local UPM registration.
- Bridge object identifiers across Unity 2022.3, 6000.3 and 6000.5.
- Backport Full input cleanup, jump responsiveness, and TMP resource installation.
- Preserve Lite content limits, damage, weapons, and editor generator workflow.

# Changelog

All notable changes to EggHeads Lite are documented in this file.

## [1.0.0] - 2026-05-18

### Added

- Initial release.
- Editor prefab generator at **Tools > PyroDuck > EggHeadsLite > Generator**.
- 3 ready-to-play characters: RangedMan, PunkMan, BomberMan.
- Three weapon categories: ranged, melee, throwable â€” one prefab each.
- Event-driven audio system (`AudioManager`, `AudioLibrarySO`, `EventManager`).
- Pre-configured `AudioLibrary.asset` mapping all `SoundId` values to included clips.
- Optional platformer sample scene with AudioManager wired out-of-the-box.
- `EggHeadDatabaseSO` with **Validate Database** inspector tool.
- Scene hierarchy organizer (`SceneOrganizer`) grouping VFX and audio under named roots.
- MIT license.
