# Changelog

All notable changes to EggHeads Lite are documented in this file.

## [1.0.2] - 2026-05-18

### Removed (dead code / clone cleanup)

- `Weapon.cs` — removed unused `WeaponCategory` and `MeleeAttackType` enums;
  removed `RocketLauncher` from `RangedAttackType`; removed `hitSound` field,
  `PlayHitSound()`, inherited `_pool` field, and `SetPool()` — pooling always
  went through `ProjectilePool.Instance`.
- `RangedWeapon.cs` — removed now-unnecessary `SetPool` override; collapsed
  `Rifle / Gun / Sniper / default` switch branches into a single `default` case.
- `ProjectilePool.cs` — removed unused single-argument overloads
  `GetProjectile(Vector3)` and `GetExplosionFromPool(Vector3)`; removed
  `SetProjectilePrefab`, `SetThrowablePrefab`, `SetExplosionPrefab` (no callers).
- `Projectile.cs` — removed public `Speed`, `Lifetime`, `IsExplosive`,
  `ExplosionRadius` properties (not read externally); removed temp `// NEW` marker.
- `WeaponController.cs` — removed dead `GetAttackDirection()` private method.
- `EggHeadPhysicsContactRelay.cs` — removed commented-out `OnCollisionStay2D` and
  `OnTriggerStay2D` blocks.
- `ShadowGroundSnapper.cs` — removed `distanceToGround` field (written but never
  read; now a local variable); corrected namespace to `...Character`; removed
  unused `using System`.
- `Events.cs` — removed five event types that have no `Publish` or `Subscribe`
  calls in this package: `VisualSelectedEvent`, `GetCharacterColorizerEvent`,
  `CreateItemEvent`, `SavePrefabEvent`, `RandomizeCharacterEvent`; removed their
  dependent `using` directives.
- `EggHeadController.cs` — removed `[SerializeField]` from `_characterState`
  (value is always overwritten in `Awake`; Inspector widget was misleading).
- Five files — removed phantom `using` directives (`Character`, `Events`, `Combat`)
  from `VisualDataSO.cs`, `VisualGroupsDataSO.cs`, `VisualType.cs`,
  `CharacterSerializer.cs`, and `NewInputProvider.cs`.

### Added

- `Audio/ProjectileImpactAudio.cs` — new shared static helper that replaces the
  identical `TryGetRandomImpactSound` + `PlayImpactClipAtPoint` logic that was
  duplicated verbatim in both `Projectile.cs` and `ThrowableProjectile.cs`.

## [1.0.1] - 2026-05-18

### Fixed

- `EggHeadDatabase.asset` — removed 12 stale/duplicate `characterPrefabs` entries
  that referenced missing prefab files; list now contains only the 3 valid
  generated characters (RangedMan, PunkMan, BomberMan).
- `Runtime/Sounds/README.md` — corrected the Create menu path for `AudioLibrarySO`
  from `PyroDuck > Audio > Audio Library` to
  `PyroDuck > EggHeadsLite > Audio Library`.

### Added

- `Runtime/Data/AudioLibrary.asset` — pre-configured `AudioLibrarySO` asset that
  maps all eight `SoundId` values to the included clips in `Runtime/Sounds/`.
- `Platformer/Scenes/Platformer.unity` — **AudioManager** GameObject added and
  wired to `AudioLibrary.asset` so the sample scene plays weapon sounds
  out-of-the-box.
- `Documentation/README.md` — new **Lite vs Full** feature comparison table and
  **Audio Setup** section with clip mapping table and code example.
- `package.json` — added `"free"` keyword; improved package description.

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
