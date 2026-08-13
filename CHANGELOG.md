# Changelog

All notable changes to K-Emoji are documented here.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.3] - 2026-08-13

### Changed

- Dropped the `Clone or submodule` install channel from the README - Package
  Manager (git URL) and the `.unitypackage` on each release cover it, and
  installing K-Setting alongside K-Emoji is now called out as a tip instead
  of its own subheading.
- Moved the main window's menu item from the top-level `Tools/KEmoji` into
  `Tools/Kingfisher/KEmoji`, and flattened its Settings entry from a nested
  submenu to `Tools/Kingfisher/KEmoji Setting` - both now match every other
  tool's menu path.

## [1.0.2] - 2026-08-13

### Changed

- Install instructions now default to tracking the branch, so Package
  Manager's own Update button works after install; pinning to a specific
  tag moved to its own optional step.

## [1.0.1] - 2026-08-13

### Fixed

- The sprite asset toggle no longer warns that TextMeshPro is missing on
  installs where `TMP_Asset.material` is a property instead of a field (the
  shape shipped with `com.unity.ugui`-bundled TextMeshPro) - both shapes are
  recognized now.
- Sped up regenerating sprite assets with many glyphs when "preserve metrics"
  is on.
- Fixed a native texture leak if atlas generation fails partway through a
  batch.
- Accept sprite or texture drops anywhere on the sprites list, not just the
  dedicated drop area.

## [1.0.0] - 2026-08-13

### Added

- Drop sprites or a sliced texture onto the window and they become rows -
  reorder them by the grip, rename them, remove them.
- Per-sprite scale, so a sprite can fill less than its cell, and a per-sprite
  hexadecimal code point for tags that address a sprite by unicode.
- One click writes the atlas PNG, slices it, writes a `TMP_SpriteAsset` beside
  it with a `TextMeshPro/Sprite` material, and regenerates the constants
  script.
- Regenerating keeps what you tuned by hand: glyph metrics, glyph rects and
  scales already in the sprite asset are carried over by sprite name, and slice
  ids are reused so existing references survive.
- **Preview** builds the atlas in memory and shows it without writing anything.
- Optional TexturePacker JSON beside the atlas, for tooling that expects it.
- The constants script is optional; which outputs to write are toggled in the
  K-Emoji window beside the paths they write to.
- Per-tool settings window for when K-Setting is not installed; with it, the
  settings fold into **Tools > KTools Setting** instead.
- Installs as a UPM package from its git URL, or as a plain folder under
  `Assets/`.
- Editor-only: the assembly is `Editor`-platform only, so nothing is compiled
  into player builds.
