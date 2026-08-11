# K-Emoji

Packs a list of sprites into one atlas and turns it into everything a
TextMeshPro `<sprite name=...>` tag needs - the atlas texture, the sprite asset
and its material, and a C# script of name constants - from a single window.

Part of [Kingfisher K-Tools](https://github.com/vuthelong/kTool). It ships
inside the K-Tools bundle together with the shared K-Setting backend, which
gives it a tab in the combined settings window.

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

K-Emoji ships on its own, with K-Setting bundled in. Package Manager > **+** >
**Install package from git URL...**, then paste:

```
https://github.com/vuthelong/kTool.git?path=/Assets/ThirdParty/.UPM/com.kingfisher.kemoji
```

Or download `KEmoji-<version>.unitypackage` from
[Releases](https://github.com/vuthelong/kTool/releases).

## License

MIT - see [LICENSE.md](../KingfisherTools/LICENSE.md).
