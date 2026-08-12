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

Two channels. Take whichever suits - the package is the same either way.

### Package Manager (git URL)

Package Manager > **+** > **Install package from git URL...**, then paste
K-Emoji, and K-Setting with it if you want the combined settings window:

```
https://github.com/vuthelong/KSetting.git#1.0.0
https://github.com/vuthelong/KEmoji.git#1.0.0
```

Drop the `#1.0.0` suffix to track the default branch instead of a tag. Unity
keeps packages read-only in `Library/PackageCache`.

### Clone or submodule

To keep the source editable instead, put the repository into
`Assets/ThirdParty/KingfisherTools/`:

```
cd Assets/ThirdParty/KingfisherTools
git clone https://github.com/vuthelong/KSetting.git
git clone https://github.com/vuthelong/KEmoji.git
```

Or add them as submodules, which is what the
[kTool](https://github.com/vuthelong/kTool) development project does:

```
git submodule add https://github.com/vuthelong/KEmoji.git Assets/ThirdParty/KingfisherTools/KEmoji
```

Check out a tag to pin a version:

```
git -C Assets/ThirdParty/KingfisherTools/KEmoji checkout 1.0.0
```

Keep one copy per project, whichever channel you use - Unity rejects a second
with `Assembly with name 'Kingfisher.KEmoji' already exists`.

## Settings

**Tools > Kingfisher > KEmoji > Settings** opens K-Emoji's own settings window.

Install [K-Setting](https://github.com/vuthelong/KSetting) beside it and you get
**Tools > KTools Setting** instead - one window that every installed Kingfisher
tool folds its settings into. It finds the installed tools by reflection at load
time, so there is nothing to wire up.

K-Setting is optional. Without it, each tool keeps its own window.

## License

Proprietary - see [LICENSE.md](LICENSE.md). Licensed per purchase (Unity Asset
Store or a direct agreement with Kingfisher); it is not open source.
