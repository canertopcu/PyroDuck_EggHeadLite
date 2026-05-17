# EggHeads — Modular 2D Character Creator

## Overview
EggHeads is a premium modular 2D egg-style character creation system for Unity. Designed for flexibility and performance, it provides customizable facial parts, three weapon categories, color customization, Zero-GC object pooling, and a fully playable platformer demo scene out-of-the-box.

---

## Features

- **Modular Architecture** — Add new body parts via ScriptableObjects without touching code.
- **Three Weapon Categories** — Ranged, Melee, and Throwable, each with unique mechanics.
- **Weapon Drop & Pickup** — Drop your current weapon with `G`, cycle weapons with `Q`/`E`, walk over a dropped weapon to pick it up automatically.
- **Explosive Projectiles** — Set `isExplosive = true` on any Projectile prefab for radius-based damage with distance falloff and a pooled explosion effect.
- **Zero-GC Weapon Pooling** — Production-ready projectile, throwable, and explosion effect pools.
- **Universal Input** — Supports both the New Input System and the Legacy Input Manager (PC & Mobile).
- **Playground Demo** — Built-in platformer test scene to showcase weapons, pickups, hazards, and pooling in action.
- **URP 2D Support** — One-click utility to upgrade all character materials to `URP 2D Lit`.
- **JSON Serialization** — Save and load character configurations at runtime.

---

## Requirements

| Dependency | Version | Notes |
|---|---|---|
| Unity | 2021.3+ | Minimum supported |
| TextMeshPro | Any | Bundled with Unity |
| Unity Input System | Any | **Optional** — legacy fallback included |
| Universal Render Pipeline | Any | **Optional** — needed for URP 2D Lit materials |

---

## Installation

### Via Unity Package Manager (UPM)
1. Open **Window > Package Manager**
2. Click **+** > **Add package from disk…**
3. Navigate to the `package.json` inside the `com.pyroduck.eggheads` folder
4. Click **Open**

### Import Sample Scene
After installation, open **Package Manager**, select **EggHeads**, expand **Samples**, and import **Character Creator Scene**.

---

## Quick Start

1. Open the imported **Character Creator Scene**.
2. Press **Play** — the full customization UI is ready.
3. Import the **Platformer** sample from Package Manager to try the gameplay scene.

---

## Weapon Controls

| Key | Action |
|---|---|
| `E` | Cycle to next weapon |
| `Q` | Cycle to previous weapon |
| `G` | Drop current weapon |
| Walk over weapon | Pick it up automatically |

Assign `WeaponsMeleeVisuals`, `WeaponsRangedVisuals`, and `WeaponsThrowablesVisuals` ScriptableObjects to the `Weapon Groups` list on `WeaponLoadoutController` in the Inspector.

---

## Editor Tools

Available under the **Tools** menu:

- **Tools > EggHeads URP 2D Upgrader** — One-click shader upgrade for all item prefabs to `Universal Render Pipeline/2D/Sprite-Lit-Default`, with a one-click revert to the default unlit sprite material.

---

## Architecture & Data Management

No runtime `Resources` path strings are used — visual groups and selectable character prefabs are assigned through ScriptableObjects or explicit Inspector references.

| Asset | Purpose |
|---|---|
| `VisualDataSO` | Defines a single visual part: name, prefab, icon sprite, colorable flag |
| `VisualGroupsDataSO` | Groups multiple `VisualDataSO` entries into a category |
| `EggHeadDatabaseSO` | Main registry read by the UI to populate selection panels and character selection |

For the Platformer sample, `CharacterSelectUI` reads character prefabs from
`EggHeadDatabaseSO.characterPrefabs`. This keeps the sample working after Unity
imports it under `Assets/Samples/...`, without depending on `Resources.LoadAll`
path strings.

### Runtime Composition

```
┌──────────────────────────────────────────────────────────────────┐
│                     EggHeadController                            │
│  ┌────────────────┐  ┌─────────────────┐  ┌──────────────────┐   │
│  │ Movement +     │  │ Health state    │  │ Hazard contact   │   │
│  │ jump (from     │  │ & drop (uses    │  │ relay (publishes │   │
│  │ BaseCharCtrl)  │  │ HealthComponent)│  │ TakeDamage)      │   │
│  └────────────────┘  └─────────────────┘  └──────────────────┘   │
│           │                    │                    │            │
│           ▼                    ▼                    ▼            │
│  IInputProvider      CharacterVisualCtrl   HealthComponent       │
│  (via InputProviderService — single shared instance)             │
└──────────────────────────────────────────────────────────────────┘
         │                      │                     │
         ▼                      ▼                     ▼
   Unity Input System    WeaponLoadoutCtrl      EventManager
   / Legacy Input        + VisualInstanceTag    (static typed bus)
                         on spawned visuals
```

### EventManager

`EventManager` is the package's lightweight, type-safe event bus. It is used for
cross-feature messages where a direct reference would make systems harder to reuse:
UI talks to the creator, pickups talk to the weapon loadout, health talks to
animation/UI, and gameplay systems trigger audio without referencing `AudioManager`.

Use normal C# references for tight parent/child relationships or data you need every
frame. Use `EventManager` for short-lived intent messages and notifications.

```csharp
using com.pyroduck.eggheads.Runtime.Scripts.Events;

private void OnEnable()
{
    EventManager.Subscribe<PlaySoundEvent>(OnPlaySound);
}

private void OnDisable()
{
    EventManager.Unsubscribe<PlaySoundEvent>(OnPlaySound);
}

private void OnPlaySound(PlaySoundEvent evt)
{
    // React immediately. Event delivery is synchronous.
}
```

Publish with a strongly typed payload:

```csharp
EventManager.Publish(new PlaySoundEvent { Id = SoundId.RangedFire });
```

For request/response style class events, use the pooled API and do not keep the event
object after publishing:

```csharp
var evt = EventManager.GetEvent<GetCharacterColorizerEvent>();
EventManager.PublishPooled(evt);
CharacterColorizer colorizer = evt.Result;
```

Rules of thumb:

- Subscribe in `OnEnable`, unsubscribe in `OnDisable`.
- Payloads are delivered synchronously on the same thread.
- Publishing with no listeners is safe.
- Struct events are preferred for fire-and-forget messages.
- Pooled class events are for mutable request/response payloads.

Core event flows:

| Event | Direction | Purpose |
|---|---|---|
| `CreateItemEvent` | `VisualButton` → `EggHeadCreator` | Equip, remove, or swap a visual slot |
| `VisualSelectedEvent` | `VisualButton` → color UI | Show/hide color picker and refresh active color |
| `RandomizeCharacterEvent` | UI → `EggHeadCreator` | Randomize all visual slots from `EggHeadDatabaseSO` |
| `GetCharacterColorizerEvent` | color UI → `EggHeadCreator` → color UI | Resolve active `CharacterColorizer` without a direct reference |
| `SavePrefabEvent` | save UI → `EggHeadCreator` | Save the current creator character as a prefab in the Editor |
| `SetMovementEvent` | controller → animation | Keep walk/crouch/run animator parameters in sync |
| `TriggerJumpStartEvent` / `TriggerJumpEndEvent` | controller → animation | Start/end jump animation flow |
| `TriggerJumpEvent` | animation/UI → controller | Apply physical jump impulse |
| `TakeDamage` | `HealthComponent` → listeners | Notify HP change, damage source, and target |
| `CharacterDiedEvent` / `CharacterRevivedEvent` | `HealthComponent` / controller → listeners | Toggle death visuals and gameplay state |
| `WeaponPickupEvent` | `WeaponPickup` → `WeaponLoadoutController` | Auto-equip a touched dropped weapon |
| `FireButtonPressedEvent` / `FireButtonReleaseEvent` | mobile UI → `WeaponController` | Bridge touch fire controls to weapon input |
| `PlaySoundEvent` / `PlaySoundAtEvent` | anywhere → `AudioManager` | Play 2D or positional SFX |
| `SetAudioMasterVolumeEvent` | UI/settings → `AudioManager` | Change package-wide audio volume |

### How to Add a New Weapon

1. **Create the prefab.** Duplicate `WeaponRanged_0` / `WeaponMelee_0` / `WeaponThrowable_0`
   from `Runtime/Prefabs/Weapons/...`. Edit the sprite + physics collider.
2. **Create a VisualDataSO.** Right-click → *Create > Scriptable Objects > BodyPartSO*.
   Set `Name`, assign the prefab to `BodyPartPrefab`, and pick an `IconSprite` for the UI.
3. **Register it.** Drag the asset into the matching `VisualGroupsDataSO`
   (`WeaponsRangedVisuals`, `WeaponsMeleeVisuals`, or `WeaponsThrowablesVisuals`).
4. **Validate.** Select `EggHeadDatabase`, hit **Validate Database** — the console
   reports missing prefabs/icons or duplicated names.
5. **Done.** The new weapon shows up in the UI immediately and is cyclable at runtime
   via `Q` / `E`.

### How to Add a New Body Part Category

1. Add a new enum value to `Enums/VisualType.cs`.
2. Create a new `VisualGroupsDataSO`, select your new `VisualType`.
3. Register it in the `EggHeadDatabaseSO`.
4. Add a matching `VisualMapping` entry (with the parent transform) on the base
   character prefab's `CharacterVisualController` so the new part has a spawn point.

---

## Runtime API

### JSON Serialization
```csharp
using com.pyroduck.eggheads.Runtime.Scripts.Data;

// Save
string json = CharacterSerializer.SaveToJson(visualController, colorizer, database.visualGroups);

// Load
CharacterSerializer.LoadFromJson(json, visualController, colorizer, database.visualGroups);
```

### Randomize via Script
```csharp
using com.pyroduck.eggheads.Runtime.Scripts.Events;

EventManager.Publish(new RandomizeCharacterEvent { Database = yourDatabaseSO });
```

### Audio Setup
See [`Runtime/Sounds/README.md`](../Runtime/Sounds/README.md) for a step-by-step guide on filling the
`AudioLibrarySO` asset and wiring up `AudioManager`. Any system can trigger a sound via
the strongly typed event bus:

```csharp
using com.pyroduck.eggheads.Runtime.Scripts.Audio;
using com.pyroduck.eggheads.Runtime.Scripts.Events;

EventManager.Publish(new PlaySoundEvent   { Id = SoundId.RangedFire });
EventManager.Publish(new PlaySoundAtEvent { Id = SoundId.MeleeImpact, Position = hitPos });
```

### Explosive Projectile Setup
```
Projectile prefab:
  ├── isExplosive    = true
  ├── explosionRadius = 3
  └── Damage          = 50  (max damage at center; 0 at edge)

ProjectilePool:
  └── Explosion Prefab = <your ExplosionEffect prefab>
```

---

## Core Components

| Component | Description |
|---|---|
| `EggHeadController` | Platformer character controller — movement, jump, health state, hazard damage |
| `WeaponLoadoutController` | Runtime weapon loadout — weapon list, cycle, equip, drop, pickup |
| `EggHeadCreator` | Creator-mode character controller — manages visual assembly at design time |
| `CharacterVisualController` | Manages body part slots and runtime swapping |
| `CharacterColorizer` | Per-part color application and caching |
| `WeaponController` | Aims weapon at pointer, handles fire input (KB/Mouse + touch UI) |
| `ProjectilePool` | Zero-GC pool for ranged projectiles, throwables, and explosion effects |
| `WeaponPickup` | Dropped-weapon trigger — auto-picked up when a character walks over it |
| `PuppetActionChecker` | `IDamageable` implementation; applies physics impulse on hit or explosion |
| `EventManager` | Static event bus — subscribe/publish strongly typed events anywhere |
| `AudioManager` | Event-driven 2D/3D audio with pooled `AudioSource` instances |

---

## License
MIT License — see `LICENSE.md`.
