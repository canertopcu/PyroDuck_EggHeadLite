# EggHeads Lite compatibility verification

Existing work was saved in commit `4976842` before this migration.

## Results

| Unity editor | Tests passed | Platformer Play Mode smoke test |
| --- | --- | --- |
| 6000.5.8f1 | 19/19 | Passed |
| 6000.3.22f1 | 19/19 | Passed |
| 2022.3.62f3 | 19/19 | Passed |

Tests cover the existing event, serialization and health behavior, stable object
identifiers, and 120 frames of the Platformer scene with a live character visual.
The 2022.3 and 6000.3 runs used separate projects containing only the distributed
Lite assets, the required packages, and the documented project layers. TMP
Essential Resources were installed automatically in both clean projects.

The exact requested 2022.3.23 editor was not installed. 2022.3.62f3 is the tested
2022.3 LTS version; this is not an exact-patch verification of 2022.3.23.
These are headless tests; rendering and interactive controls were not visually inspected.

## Distribution

Import `Builds/EggHeadsLite.unitypackage`. The package lives entirely under
`Assets/PyroDuck/EggHeadsLite`; no local UPM registration is required.
The three existing ready-made characters were moved from
`Assets/EggHeadsLite/GeneratedHeads` into `Runtime/Prefabs/Characters` with their
GUIDs preserved. Otherwise they were absent from clean imports.
Existing asset GUIDs and Lite content limits were preserved.

Full's object-ID compatibility, input cleanup, jump handling, TMP installation,
and HP-bar cleanup were ported. Lite's damage cooldown, weapon/pool APIs and
character-selection workflow remain in place. The stale Full demo entry in
EditorBuildSettings now points to Lite's Platformer scene.

ProjectSettings and Package Manager packages are not bundled. See the shipped
Documentation/README.md for dependencies, input handling and layer indices.
The only asset reference outside Lite is TMP's standard LiberationSans SDF,
provided by TMP Essential Resources.

Test results: `Logs/final-<Unity version>.xml`.
Export log: `Logs/export-lite-final.log`.
