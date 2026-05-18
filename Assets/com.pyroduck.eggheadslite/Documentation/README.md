# EggHeads Lite - Modular Character Generator

EggHeads Lite is a **free** Unity package for building modular egg-style
character prefabs from ready-made parts. The supported Lite workflow is editor
driven: open the generator, choose the visual parts, save a prefab, then use the
generated prefab in your own scene or register it in the included database.

Package id: `com.pyroduck.eggheadslite`

Namespace root: `com.pyroduck.eggheadslite`

## Lite vs Full

| Feature | Lite (Free) | Full |
|---------|:-----------:|:----:|
| Editor prefab generator | ✓ | ✓ |
| Runtime platformer sample | ✓ | ✓ |
| Ready-made characters | 3 | 6+ |
| Visual parts (eyes / noses / mouths / etc.) | ~21 | 100+ |
| Weapon prefabs (melee / ranged / throwable) | 1 each | 8–10 each |
| Event-driven audio system | ✓ | ✓ |
| Pre-configured `AudioLibrary` asset | ✓ | ✓ |
| Runtime character creator UI | — | ✓ |
| Runtime randomisation & colour-wheel | — | ✓ |
| JSON save/load character config | — | ✓ |
| URP 2D Lit material upgrader | — | ✓ |
| Mobile touch-fire controls | — | ✓ |
| AI / Enemy controllers | — | ✓ |
| Color picker (wheel + hex input) | — | ✓ |

> The Lite package identity is `com.pyroduck.eggheadslite`.
> Do **not** mix it with the full `com.pyroduck.eggheads` package in the same project.

## Requirements

- Unity 2021.3 or newer
- 2D project setup
- Unity Input System, TextMesh Pro, and UGUI packages when using the included
  runtime/sample components

## Scene Setup

The following one-time project configuration is needed to use all runtime
features in your own scenes.

### Physics Layers

| Layer name | Used by | Purpose |
|------------|---------|---------|
| `Ground` | `ShadowGroundSnapper` | Raycast target for the drop-shadow effect |
| `Throwable` | `ThrowableWeapon` | Prevents throwables from colliding with the owner |

Create these layers in **Edit → Project Settings → Tags & Layers**.
`ShadowGroundSnapper` will log a warning at startup if `Ground` is missing.

### Required GameObjects per scene

| Component | Where to add | Notes |
|-----------|-------------|-------|
| `ProjectilePool` | Any persistent root GameObject | Enables object pooling for projectiles and explosions; without it the system falls back to `Instantiate`. |
| `AudioManager` | Any persistent root GameObject | Required for `PlaySoundEvent` / `PlaySoundAtEvent` to produce audio. Assign `Runtime/Data/AudioLibrary.asset` to its **Library** field. |

> The included **Platformer** sample scene already contains both of the above.

## Installation

1. Open Unity Package Manager.
2. Choose **Add package from disk**.
3. Select `Assets/com.pyroduck.eggheadslite/package.json`.
4. Unity will load the package as **EggHeads Lite - Modular Character Generator**.

For this local project, `Packages/manifest.json` already points to:

```json
"com.pyroduck.eggheadslite": "file:../Assets/com.pyroduck.eggheadslite"
```

## Generator Workflow

Open the generator from:

**Tools > PyroDuck > EggHeadsLite > Generator**

The generator creates a character prefab from the selected base and body-part
prefabs.

Default output folder:

```text
Assets/EggHeadsLite/GeneratedPrefabs
```

Recommended flow:

1. Open **Tools > PyroDuck > EggHeadsLite > Generator**.
2. Click **Load Defaults** if the fields are empty.
3. Choose or replace the body, eyes, eyebrows, nose, mouth, beard/mustache,
   hair/hat, and weapon prefabs.
4. Set **Prefab Name** and **Output Folder**.
5. Keep **Auto Register In SO** enabled to add the generated prefab to the
   selected `EggHeadDatabaseSO`.
6. Click **Generate**.

After generation, the new prefab is selected and pinged in the Project window.

## Database Registration

The generator uses this database by default:

```text
Assets/com.pyroduck.eggheadslite/Runtime/Data/EggHeadDatabase.asset
```

When **Auto Register In SO** is enabled, the generated prefab is automatically
added to:

```csharp
EggHeadDatabaseSO.characterPrefabs
```

If automatic registration is disabled, drag the generated prefab manually into
the `characterPrefabs` list on the relevant `EggHeadDatabaseSO`.

The database inspector includes **Validate Database** for checking common setup
issues such as null entries, missing prefabs, missing icons, and duplicate names.

## Audio Setup

A pre-configured `AudioLibrary.asset` is provided at:

```text
Runtime/Data/AudioLibrary.asset
```

It maps every `SoundId` to a clip from `Runtime/Sounds/`:

| `SoundId` | Value | Default clip |
|-----------|------:|--------------|
| `ProjectileFire` | 100 | `Pistol.wav` |
| `ProjectileImpact` | 101 | `Explosion.wav` |
| `ProjectileExplosion` | 102 | `Explosion1.wav` |
| `ThrowableThrow` | 200 | `ShurikenFly.wav` |
| `ThrowableImpact` | 201 | `Bumerang.wav` |
| `ThrowableExplosion` | 202 | `Explosion1.wav` |
| `MeleeSwing` | 300 | `Swing.wav` |
| `MeleeImpact` | 301 | `KnifeStab2.wav` |

The Platformer sample scene already has an **AudioManager** GameObject that
references this asset. To use audio in your own scene:

1. Add an **AudioManager** component (or GameObject) to your scene.
2. Assign `Runtime/Data/AudioLibrary.asset` to its **Library** field.
3. Play sounds from any script via the event bus:

```csharp
using com.pyroduck.eggheadslite.Runtime.Scripts.Audio;
using com.pyroduck.eggheadslite.Runtime.Scripts.Events;

EventManager.Publish(new PlaySoundEvent   { Id = SoundId.ProjectileFire });
EventManager.Publish(new PlaySoundAtEvent { Id = SoundId.MeleeImpact, Position = hitPos });
```

To override a clip, duplicate `AudioLibrary.asset`, open it, and replace the
`Clips` array entry for the relevant `SoundId`.

## Adding New Visual Parts

1. Create or import a body-part prefab.
2. Create a visual data asset from **Create > PyroDuck > EggHeadsLite > Visual Data**.
3. Assign the prefab to `BodyPartPrefab`.
4. Assign `IconSprite` if the part should appear in UI lists.
5. Enable `isColorable` if the generator should expose a color field.
6. Add the visual data asset to the correct **Visual Group**.
7. Select `EggHeadDatabase.asset` and run **Validate Database**.

Visual groups can be created from:

**Create > PyroDuck > EggHeadsLite > Visual Group**

The main database can be created from:

**Create > PyroDuck > EggHeadsLite > EggHead Database**

## Package Layout

```text
Assets/com.pyroduck.eggheadslite
  Editor/                 Generator and database inspector
  Runtime/Data/           EggHeadDatabaseSO, VisualDataSO, VisualGroupsDataSO,
                          AudioLibrary assets
  Runtime/Prefabs/        Character base and modular body-part prefabs
  Runtime/Scripts/        Runtime controllers and data scripts
  Runtime/Sounds/         Included audio clips
  Runtime/Sprites/        Included sprites
  Platformer/             Optional sample scene (with AudioManager wired up)
```

## Notes

- Generated prefabs are saved outside the package by default so they can be
  edited safely in the consuming project.
- Existing prefab and material references are preserved through Unity `.meta`
  GUIDs.
- The Lite package identity is `com.pyroduck.eggheadslite`; avoid mixing it with
  the full EggHeads package in the same project.
