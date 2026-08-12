# K-Emoji

Packs a list of sprites into one atlas and turns it into everything a
TextMeshPro `<sprite name=...>` tag needs - the atlas texture, the sprite asset
and its material, and a C# script of name constants - from a single window.

One of the Kingfisher K-Tools, built on the shared
[K-Setting](https://github.com/vuthelong/KSetting) backend. This package ships
with its own copy, so installing K-Emoji on its own still gives you a tab in
the combined settings window.

## Features

- Drop sprites or a sliced texture onto the window and they become rows -
  reorder them by the grip, rename them, remove them
- Per-sprite scale, so a sprite can fill less than its cell, and a per-sprite
  hexadecimal code point for tags that address a sprite by unicode
- One click writes the atlas PNG, slices it, writes a `TMP_SpriteAsset` beside
  it with a `TextMeshPro/Sprite` material, and regenerates the constants script
- Regenerating keeps what you tuned by hand: glyph metrics, glyph rects and
  scales already in the sprite asset are carried over by sprite name, and slice
  ids are reused so existing references survive
- **Preview** builds the atlas in memory and shows it without writing anything
- Optional TexturePacker JSON beside the atlas, for tooling that expects it

Which outputs to write - the sprite asset, the constants script and the
TexturePacker JSON - are toggled in the K-Emoji window itself, beside the paths
they write to. Cell size and how the atlas imports are settings, and live in
**Tools > KTools Setting > KEmoji**. The sprite list, the atlas path and the
constants script path are per project and live in `.KData`.

Everything is editor-only - the assembly is `Editor`-platform only, so nothing
here is compiled into player builds.

## What it needs

- **TextMeshPro** (`com.unity.ugui`) for the sprite asset step. It is reached by
  reflection, so K-Emoji compiles and runs without it; the window says so and
  the atlas is still written.
- **2D Sprite** (`com.unity.2d.sprite`) to slice the atlas into `Sprite`
  sub-assets. Without it K-Emoji falls back to the importer's own sprite sheet
  and warns if nothing was sliced. A TextMeshPro sprite asset does not need the
  slices - it addresses the texture by glyph rect.

## Use it

1. **Tools > KEmoji**
2. Drop sprites onto the drop area, or add empty rows with **+**
3. Set the atlas path - anywhere inside `Assets/`, ending in `.png`
4. Set the constants script path, namespace and class name, if you want the
   constants
5. **Generate**

The generated script holds one constant per sprite, named from the sprite in
`UPPER_SNAKE_CASE`, so a tag reads
`$"<sprite name={SpriteText.ICON_CP}>"` instead of a bare string.

## Install

Two channels, both on this repo's
[Releases](https://github.com/vuthelong/KEmoji/releases) page. Pick one - see
[Install one way only](#install-one-way-only).

### Package Manager (git URL)

Package Manager > **+** > **Install package from git URL...**, then paste:

```
https://github.com/vuthelong/KEmoji.git#1.0.0
```

Drop the `#1.0.0` suffix to track the default branch instead of a release tag.
Unity keeps the package read-only in `Library/PackageCache`.

### .unitypackage

Download `KEmoji-1.0.0.unitypackage` from the release and drop it into your
project, or **Assets > Import Package > Custom Package...**. It unpacks to
`Assets/ThirdParty/KingfisherTools/`, where you can read and edit the source.

## Install one way only

Do not combine the two channels, and do not install two single-tool packages
side by side. Each one ships its own copy of K-Setting, and Unity rejects
duplicate assembly names with
`Assembly with name 'Kingfisher.KSetting' already exists`.

Only one Kingfisher package can be installed at a time. Pick the tool you want -
each one brings the same combined settings window with it.

## License

MIT - see [LICENSE.md](../KingfisherTools/LICENSE.md).
