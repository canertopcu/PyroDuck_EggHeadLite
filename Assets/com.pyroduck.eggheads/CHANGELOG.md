# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2026-04-17

### Added
- `Compat.Rigidbody2DCompat` extension methods route `linearVelocity` through `velocity`
  on Unity versions older than 6000.0, restoring compile compatibility with the declared
  `2021.3` minimum in `package.json`.
- `InputProviderService` static locator — a single `IInputProvider` is now shared across
  `BaseCharacterController` and `WeaponController` instead of each component allocating
  its own provider (prevents duplicate subscriptions in the New Input System).
- `WeaponPickupFactory` static helper extracted from `EggHeadController.DropWeapon`, so
  any system (AI drops, chest loot, debug spawners) can reuse the same pickup setup.
- `HealthComponent` — reusable `IDamageable` component usable on NPCs and destructibles.
  `EggHeadController` delegates to it when present, keeping backward compatibility with
  existing prefabs that rely on its built-in `maxHealth` field.
- `VisualInstanceTag` runtime marker + new
  `CharacterVisualController.CreateItem(VisualDataSO, VisualType)` overload. Lets
  `CharacterSerializer` round-trip visuals through a direct asset reference instead of
  fragile `go.name.Contains(prefab.name)` matching. Legacy name lookup is kept as a
  fallback so previously saved JSONs still load.
- `CreateItemEvent.VisualData` field carrying the source `VisualDataSO` so event
  consumers can tag spawned instances for serialization.
- Custom inspector for `EggHeadDatabaseSO` with a one-click **Validate Database** button
  (checks for null entries, missing prefabs/icons, duplicate names, empty groups).
- `com.pyroduck.eggheads.Tests.Editor` assembly with 14 EditMode tests covering
  `EventManager`, `CharacterSerializer`, and `HealthComponent`.
- `MeleeWeapon` per-swing multi-hit guard: each target can be hit at most once per
  attack press, plus a configurable `damageLayers` filter and `swingWindow`.
- `AudioManager.ResetStaticState` runtime hook so the singleton cleanly re-initialises
  when domain reload is disabled.
- `Runtime/Sounds/README.md` with a step-by-step CC0 audio setup guide.

### Fixed
- `CharacterSerializer.SaveToJson` / `LoadFromJson` now short-circuit on null
  controllers / groups and swallow malformed JSON instead of throwing.
- `OnDisable` on the character and weapon controllers no longer calls `DisableInput` on
  the shared `IInputProvider`; the service owns its lifetime and clears on domain reload.

### Changed
- Editor assembly renamed from `EggHeadBuilder` to `com.pyroduck.eggheads.Editor` for
  UPM consistency (root namespace unchanged).
- Package version bumped to **1.3.0**.
- Mele / WeaponMele → **Melee / WeaponMelee** rename across prefabs, `VisualDataSO`
  assets, and the VisualGroup asset file (`WeaponsMeleeVisuals.asset`). GUIDs preserved
  so scene/prefab references are intact.
- README updated: removed the non-existent `PlaygroundManager`, replaced the fictional
  `visualPanelController.RandomizeCharacter()` sample with the real
  `RandomizeCharacterEvent` dispatch, and added a linked **Audio Setup** section.

## [1.2.0] - 2026-04-17

### Added
- `IMovementInput`, `IPointerInput`, `ICombatInput` segregated input interfaces. `IInputProvider` now composes the three (backwards compatible).
- `WeaponController.RefreshMainCamera()` public method to re-resolve the main camera after runtime camera swaps.
- XML documentation on public/core API surface (`EventManager`, `Projectile`, `MeleeWeapon`, `ThrowableProjectile`, `WeaponController`, `CharacterColorizer`).
- Platformer sample registered in `package.json`'s `samples` array.

### Fixed
- `MeleeWeapon.TryAttack` no longer bypasses the base-class cooldown/CanAttack pipeline (LSP violation) and the swing-speed calculation now uses `Mathf.DeltaAngle` on local rotation so damage no longer spikes on 0° / 360° wraparound.
- `MeleeWeapon` now skips self-damage by comparing hierarchy roots.
- `ThrowableProjectile.Explode` now skips the owner's entire hierarchy (matches `Projectile.IsOwnerHierarchy`) instead of only the owner root, preventing child colliders from taking self-damage.
- `PuppetActionChecker` caches the `Throwable` layer index instead of resolving it per hit.
- `WeaponController` recovers when `Camera.main` becomes null (e.g. camera rig swap) instead of silently disabling aim.
- `EventManager` static state is now cleared on `SubsystemRegistration`, preventing delegate leaks when Enter Play Mode Options disables domain reload.
- `CharacterColorizer.OnValidate` no longer mutates the serialized `bodyParts` list during asset import; colour push is deferred to the next editor tick.
- `AnimationsController` replaces the string-based `Invoke("TriggerJumpAction", ...)` with a coroutine (refactor-safe).

### Changed
- `Projectile.speed`, `lifetime`, `isExplosive`, `explosionRadius` fields converted from `public` to `[SerializeField] private` with read-only properties. Existing serialized values migrate via `[FormerlySerializedAs]` — no action required on prefabs.
- `EggHeadController.DropWeapon` split into a dedicated `SpawnWeaponPickup` helper to clarify responsibilities and ease future customisation.
- Non-English inline comments translated to English across the runtime scripts.
- Dead/commented-out code blocks removed from `ProjectilePool` and `MeleeWeapon`.
- README tool list now reflects the actual shipping editor tools only.

## [1.1.0] - 2026-04-14

### Added
- **Weapon Drop / Pickup / Cycle system** on `EggHeadController`
  - `G` to drop current weapon (spawns a physics object with icon sprite)
  - `Q` / `E` to cycle through all weapons from assigned ScriptableObject groups
  - Walking over a dropped weapon auto-equips it via `WeaponPickup` trigger component
- **Explosive Projectile** support on `Projectile`
  - `isExplosive` flag: on collision, triggers `Physics2D.OverlapCircleAll` radius damage
  - Linear distance falloff: max damage at center, 0 at radius edge
  - Pooled `ExplosionEffect` played at impact point
- **`WeaponPickup`** new MonoBehaviour for dropped-weapon ground objects
- `IInputProvider` extended: `GetDropWeaponDown()`, `GetNextWeaponDown()`, `GetPrevWeaponDown()`
- `PuppetActionChecker`: explosion force direction now correctly pushes away from impact origin
- `ProjectilePool`: added `GetExplosionFromPool(Vector3)` public method

### Fixed
- `Projectile` now handles both `OnCollisionEnter2D` and `OnTriggerEnter2D` — works regardless of whether the collider is set as trigger or solid
- `WeaponController.OnDisable` now calls `_input?.DisableInput()` to prevent input leaks
- `ProjectilePool` prefab fields changed from `public` to `[SerializeField] private`
- `RangedWeapon` muzzle flash creation moved to `OnEnable` to eliminate per-shot GC allocations
- Unused `using` directives removed from `EggHeadsURPUpgrader`

### Changed
- All source code comments and tooltips are now in English
- `BaseCharacterController.OnDisable` changed to `protected virtual` for correct subclass cleanup
- `EggHeadCreator` and `EggHeadController` `OnDisable` now call `base.OnDisable()`

## [1.0.0] - 2026-04-06

### Added
- Modular character visual system with swappable parts (Eyes, Mouth, Nose, Eyebrows, Hair/Hats, Beard/Mustache)
- 19 eye styles, 16 mouth styles, 20 nose styles, 11 eyebrow styles
- 12 hair styles, 13 hat styles, 9 beard/mustache styles
- Weapon system with 3 categories: Ranged (10), Melee (9), Throwable (8)
- Weapon controller with recoil animation and muzzle flash support
- Color customization per visual part via Color Wheel and Hex Input
- ScriptableObject-based data architecture (VisualDataSO, VisualGroupsDataSO)
- Runtime UI panels for visual selection and animation preview
- Character animation system: Idle, Walk Forward/Backward, Jump
- Input abstraction layer supporting both New Input System and Legacy Input
- Mobile touch input and PC mouse input support
- Editor tool: UI Generator for quick customization canvas setup
- Prefab save/export functionality (Editor only)
- 6 ready-to-use character prefabs
- Sample scene: PrefabCreatorScene
