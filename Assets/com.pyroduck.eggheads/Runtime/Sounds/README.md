# Audio Assets

This folder contains the runtime audio clips used by `AudioManager`,
`AudioLibrarySO`, weapon prefabs, projectile prefabs, and sample scenes.

## Recommended CC0 / royalty-free starter set

| File name (suggested) | Suggested `SoundId` | Purpose |
|-----------------------|---------------------|---------|
| `Jump.wav`            | `CharacterJump`     | Short upward hop |
| `Land.wav`            | `CharacterLand`     | Ground impact |
| `Footstep.wav`        | `CharacterFootstep` | Looped footsteps |
| `PickupWeapon.wav`    | `PickupWeapon`      | Weapon pickup |
| `DropWeapon.wav`      | `DropWeapon`        | Weapon drop |
| `Shoot.wav`           | `RangedFire`        | Projectile launch |
| `MeleeSwing.wav`      | `MeleeSwing`        | Melee weapon swing |
| `MeleeImpact.wav`     | `MeleeImpact`       | Melee weapon hit |
| `ThrowableThrow.wav`  | `ThrowableThrow`    | Thrown weapon |
| `ThrowableImpact.wav` | `ThrowableImpact`   | Thrown weapon hit |
| `Explosion.wav`       | `ThrowableExplosion`| Explosive detonation |

Source suggestions (all CC0 / public domain):

- https://freesound.org (filter by "Creative Commons 0")
- https://kenney.nl/assets (impact, UI, digital packs)
- https://opengameart.org (licensing per asset)

## Setup

1. Drop `.wav` / `.ogg` clips into `Runtime/Sounds`.
2. In Unity: **Create > PyroDuck > Audio > Audio Library** - an `AudioLibrary.asset` will appear.
3. Open the asset and add one `SoundEntry` per clip, assigning the matching `SoundId`.
4. Drop an **AudioManager** component into your scene and assign the `AudioLibrary` asset to its `library` field.

Playback from anywhere:

```csharp
using com.pyroduck.eggheads.Runtime.Scripts.Audio;
using com.pyroduck.eggheads.Runtime.Scripts.Events;

EventManager.Publish(new PlaySoundEvent   { Id = SoundId.RangedFire });
EventManager.Publish(new PlaySoundAtEvent { Id = SoundId.MeleeImpact, Position = hitPos });
```
