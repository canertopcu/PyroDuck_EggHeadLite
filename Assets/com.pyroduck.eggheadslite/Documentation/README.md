# EggHeads Lite - Modular Character Generator

EggHeads Lite is a lightweight Unity package for building modular egg-style
character prefabs from ready-made parts. The supported Lite workflow is editor
driven: open the generator, choose the visual parts, save a prefab, then use the
generated prefab in your own scene or register it in the included database.

Package id: `com.pyroduck.eggheadslite`

Namespace root: `com.pyroduck.eggheadslite`

## Requirements

- Unity 2021.3 or newer
- 2D project setup
- Unity Input System, TextMesh Pro, and UGUI packages when using the included
  runtime/sample components

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
  Runtime/Data/           EggHeadDatabaseSO, VisualDataSO, VisualGroupsDataSO assets
  Runtime/Prefabs/        Character base and modular body-part prefabs
  Runtime/Scripts/        Runtime controllers and data scripts
  Runtime/Sprites/        Included sprites
  Platformer/             Optional sample assets
```

## Notes

- Generated prefabs are saved outside the package by default so they can be
  edited safely in the consuming project.
- Existing prefab and material references are preserved through Unity `.meta`
  GUIDs.
- The Lite package identity is `com.pyroduck.eggheadslite`; avoid mixing it with
  the full EggHeads package in the same project.
