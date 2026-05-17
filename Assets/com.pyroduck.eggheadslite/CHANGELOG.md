# Changelog

All notable changes to EggHeads Lite are documented in this file.

## [1.0.0] - 2026-05-18

### Added

- Package identity updated to `com.pyroduck.eggheadslite`.
- Editor generator exposed at **Tools > PyroDuck > EggHeadsLite > Generator**.
- Generated prefabs save to `Assets/EggHeadsLite/GeneratedPrefabs` by default.
- Generator can automatically register generated prefabs in
  `EggHeadDatabaseSO.characterPrefabs`.
- Lite README focused on the generator and database registration workflow.

### Changed

- Runtime, editor, and test assembly names use the `com.pyroduck.eggheadslite`
  namespace.
- ScriptableObject creation menus now live under **Create > PyroDuck > EggHeadsLite**.

### Removed

- Old full-package URP material upgrader editor menu.
