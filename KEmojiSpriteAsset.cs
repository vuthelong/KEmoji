#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
using static Kingfisher.KEmoji.Libs.KUtils;

namespace Kingfisher.KEmoji
{
    internal static class KEmojiSpriteAsset
    {
        #region Field

        private const string SpriteAssetTypeName = "TMPro.TMP_SpriteAsset, Unity.TextMeshPro";
        private const string SpriteGlyphTypeName = "TMPro.TMP_SpriteGlyph, Unity.TextMeshPro";
        private const string SpriteCharacterTypeName = "TMPro.TMP_SpriteCharacter, Unity.TextMeshPro";

        private const string SpriteSheetFieldName = "spriteSheet";
        private const string MaterialFieldName = "material";
        private const string CharacterTablePropertyName = "spriteCharacterTable";
        private const string GlyphTablePropertyName = "spriteGlyphTable";
        private const string VersionPropertyName = "version";
        private const string UpdateLookupTablesMethodName = "UpdateLookupTables";

        private const string UnicodePropertyName = "unicode";
        private const string GlyphIndexPropertyName = "glyphIndex";
        private const string ScalePropertyName = "scale";
        private const string NamePropertyName = "name";

        private const string ShaderName = "TextMeshPro/Sprite";
        private const string MainTexPropertyName = "_MainTex";
        private const string MaterialNameSuffix = " Material";
        private const string AssetExtension = ".asset";
        private const string SpriteAssetVersion = "1.0.0";

        private const float DefaultScale = 1;
        private const float DefaultBearingRatio = .9f;
        private const float NoBearingX = 0;
        private const int FirstAtlasIndex = 0;
        private const uint NoUnicode = 0;

        private static readonly int MainTexPropertyId = Shader.PropertyToID(MainTexPropertyName);

        private static readonly Type SpriteAssetType = Type.GetType(SpriteAssetTypeName);
        private static readonly Type SpriteGlyphType = Type.GetType(SpriteGlyphTypeName);
        private static readonly Type SpriteCharacterType = Type.GetType(SpriteCharacterTypeName);

        private static readonly FieldInfo SpriteSheetField = SpriteAssetType?.GetField(SpriteSheetFieldName);
        private static readonly FieldInfo MaterialField = SpriteAssetType?.GetField(MaterialFieldName);

        private static readonly PropertyInfo CharacterTableProperty = SpriteAssetType?.GetProperty(CharacterTablePropertyName);
        private static readonly PropertyInfo GlyphTableProperty = SpriteAssetType?.GetProperty(GlyphTablePropertyName);
        private static readonly MethodInfo VersionSetMethod = SpriteAssetType?.GetProperty(VersionPropertyName)?.GetSetMethod(true);
        private static readonly MethodInfo UpdateLookupTablesMethod = SpriteAssetType?.GetMethod(UpdateLookupTablesMethodName, Type.EmptyTypes);

        private static readonly PropertyInfo UnicodeProperty = SpriteCharacterType?.GetProperty(UnicodePropertyName);
        private static readonly PropertyInfo GlyphIndexProperty = SpriteCharacterType?.GetProperty(GlyphIndexPropertyName);
        private static readonly PropertyInfo CharacterScaleProperty = SpriteCharacterType?.GetProperty(ScalePropertyName);
        private static readonly PropertyInfo CharacterNameProperty = SpriteCharacterType?.GetProperty(NamePropertyName);

        #endregion

        #region Property

        public static bool IsAvailable => HasAssetMembers && HasCharacterMembers;

        private static bool HasAssetMembers => SpriteAssetType != null && SpriteGlyphType != null && SpriteCharacterType != null && SpriteSheetField != null && MaterialField != null && CharacterTableProperty != null && GlyphTableProperty != null;

        private static bool HasCharacterMembers => UnicodeProperty != null && GlyphIndexProperty != null && CharacterScaleProperty != null && CharacterNameProperty != null;

        #endregion

        #region Generation

        public static string Generate(string atlasPath, KEmojiAtlas.Result atlas, List<KEmojiData.Entry> entries)
        {
            if (!IsAvailable) return null;

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);

            if (!texture) return null;

            var shader = Shader.Find(ShaderName);

            if (!shader) return null;

            var assetPath = Path.ChangeExtension(atlasPath, AssetExtension).NormalizeSlashes();
            var assetName = Path.GetFileNameWithoutExtension(assetPath);
            var spriteAsset = LoadOrCreate(assetPath, assetName);

            if (!spriteAsset) return null;

            SpriteSheetField.SetValue(spriteAsset, texture);

            var material = LoadOrCreateMaterial(spriteAsset, assetPath, shader);

            material.name = assetName + MaterialNameSuffix;

            material.SetTexture(MainTexPropertyId, texture);

            MaterialField.SetValue(spriteAsset, material);

            if (!Fill(spriteAsset, atlas, entries)) return null;

            VersionSetMethod?.Invoke(spriteAsset, new object[] { SpriteAssetVersion });
            UpdateLookupTablesMethod?.Invoke(spriteAsset, null);

            EditorUtility.SetDirty(spriteAsset);
            EditorUtility.SetDirty(material);

            AssetDatabase.SaveAssets();

            return assetPath;
        }

        private static bool Fill(ScriptableObject spriteAsset, KEmojiAtlas.Result atlas, List<KEmojiData.Entry> entries)
        {
            if (CharacterTableProperty.GetValue(spriteAsset) is not IList characters) return false;
            if (GlyphTableProperty.GetValue(spriteAsset) is not IList glyphs) return false;

            var preserved = CollectPreserved(characters, glyphs);

            characters.Clear();
            glyphs.Clear();

            var cellSize = KEmoji.CellSize;

            for (var i = 0; i < atlas.Placements.Count; i++)
            {
                var placement = atlas.Placements[i];
                var cellRect = new GlyphRect(placement.X, placement.Y, cellSize, cellSize);
                var glyphIndex = (uint)i;

                preserved.TryGetValue(placement.Name, out var existing);

                var glyph = (Glyph)Activator.CreateInstance(SpriteGlyphType);

                glyph.index = glyphIndex;
                glyph.metrics = existing?.Metrics ?? CreateDefaultMetrics(cellSize);
                glyph.glyphRect = existing == null ? cellRect : Remap(existing.GlyphRect, cellRect, cellSize);
                glyph.scale = existing?.GlyphScale ?? DefaultScale;
                glyph.atlasIndex = FirstAtlasIndex;

                glyphs.Add(glyph);

                var character = Activator.CreateInstance(SpriteCharacterType);

                UnicodeProperty.SetValue(character, ResolveUnicode(entries[i].unicode, existing));
                GlyphIndexProperty.SetValue(character, glyphIndex);
                CharacterScaleProperty.SetValue(character, existing?.CharacterScale ?? DefaultScale);
                CharacterNameProperty.SetValue(character, placement.Name);

                characters.Add(character);
            }

            return true;
        }

        private static ScriptableObject LoadOrCreate(string assetPath, string assetName)
        {
            if (AssetDatabase.LoadAssetAtPath(assetPath, SpriteAssetType) is ScriptableObject existing) return existing;

            EnsureAssetFolder(assetPath.GetParentPath());

            var created = ScriptableObject.CreateInstance(SpriteAssetType);

            created.name = assetName;

            AssetDatabase.CreateAsset(created, assetPath);

            return created;
        }

        private static Material LoadOrCreateMaterial(ScriptableObject spriteAsset, string assetPath, Shader shader)
        {
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            for (var i = 0; i < subAssets.Length; i++)
            {
                if (subAssets[i] is not Material material) continue;

                if (material.shader != shader)
                {
                    material.shader = shader;
                }

                return material;
            }

            var created = new Material(shader);

            AssetDatabase.AddObjectToAsset(created, spriteAsset);

            return created;
        }

        #endregion

        #region Preserved Sprite

        private static Dictionary<string, PreservedSprite> CollectPreserved(IList characters, IList glyphs)
        {
            var preserved = new Dictionary<string, PreservedSprite>();

            if (!KEmojiMenu.PreserveMetricsEnabled) return preserved;

            for (var i = 0; i < characters.Count; i++)
            {
                var character = characters[i];

                if (character == null) continue;

                if (CharacterNameProperty.GetValue(character) is not string name || string.IsNullOrEmpty(name)) continue;

                if (preserved.ContainsKey(name)) continue;

                if (FindGlyph(glyphs, (uint)GlyphIndexProperty.GetValue(character)) is not { } glyph) continue;

                preserved.Add(name, new PreservedSprite
                {
                    Metrics = glyph.metrics,
                    GlyphRect = glyph.glyphRect,
                    GlyphScale = glyph.scale,
                    CharacterScale = (float)CharacterScaleProperty.GetValue(character),
                    Unicode = (uint)UnicodeProperty.GetValue(character),
                });
            }

            return preserved;
        }

        private static Glyph FindGlyph(IList glyphs, uint glyphIndex)
        {
            for (var i = 0; i < glyphs.Count; i++)
            {
                if (glyphs[i] is not Glyph glyph) continue;

                if (glyph.index != glyphIndex) continue;

                return glyph;
            }

            return null;
        }

        private static GlyphRect Remap(GlyphRect preservedRect, GlyphRect cellRect, int cellSize)
        {
            if (preservedRect.width <= 0 || preservedRect.height <= 0) return cellRect;

            var width = Mathf.Clamp(preservedRect.width, 1, cellSize);
            var height = Mathf.Clamp(preservedRect.height, 1, cellSize);

            var offsetX = Mathf.Clamp(preservedRect.x % cellSize, 0, cellSize - width);
            var offsetY = Mathf.Clamp(preservedRect.y % cellSize, 0, cellSize - height);

            return new GlyphRect(cellRect.x + offsetX, cellRect.y + offsetY, width, height);
        }

        private static GlyphMetrics CreateDefaultMetrics(int cellSize) => new(cellSize, cellSize, NoBearingX, cellSize * DefaultBearingRatio, cellSize);

        private static uint ResolveUnicode(string hex, PreservedSprite existing)
        {
            if (string.IsNullOrEmpty(hex)) return existing?.Unicode ?? NoUnicode;

            return uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var unicode) ? unicode : NoUnicode;
        }

        #endregion

        #region Nested Type

        private class PreservedSprite
        {
            public GlyphMetrics Metrics;
            public GlyphRect GlyphRect;

            public float GlyphScale;
            public float CharacterScale;

            public uint Unicode;
        }

        #endregion
    }
}
#endif
