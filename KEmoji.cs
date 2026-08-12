#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Kingfisher.KEmoji.Libs.KUtils;
using Object = UnityEngine.Object;

namespace Kingfisher.KEmoji
{
    public static class KEmoji
    {
        #region Field

        public const string DataAssetPath = "Assets/ThirdParty/KingfisherTools/KEmoji/KEmojiData.asset";

        private const string Version = "1.0.0";

        private const string LogFormat = "K-Emoji: {0}";
        private const string GeneratedFormat = "generated {0} - {1} sprites in {2} px cells.";
        private const string SpriteAssetFormat = "wrote the sprite asset {0}.";
        private const string ScriptFormat = "updated {0}.";
        private const string DisabledWarning = "the tool is turned off in Tools > KTools Setting.";
        private const string NoEntriesWarning = "nothing to generate - add at least one sprite first.";
        private const string NoAtlasPathWarning = "choose an atlas path inside the project's Assets folder before generating.";
        private const string WrongExtensionWarning = "the atlas path has to end in .png.";
        private const string NoSpriteAssetWarning = "no sprite asset was written - TextMeshPro has to be installed and the atlas has to import as a texture.";
        private const string NoSlicesWarning = "the atlas was not sliced into sprites - install the 2D Sprite package (com.unity.2d.sprite) if you need them. A TextMeshPro sprite asset does not.";

        private const string DefaultScriptClassName = "SpriteText";
        private const string AtlasExtension = ".png";

        private const int MinCellSize = 1;

        private static KEmojiData _data;

        #endregion

        #region Property

        public static KEmojiData Data => _data ? _data : _data = LoadOrCreateData();

        public static int CellSize => Mathf.Max(MinCellSize, Mathf.RoundToInt(KEmojiMenu.CellSize));

        public static bool HasSprites
        {
            get
            {
                var entries = Data.entries;

                for (var i = 0; i < entries.Count; i++)
                {
                    if (entries[i].sprite) return true;
                }

                return false;
            }
        }

        #endregion

        #region Generation

        public static Texture2D BuildPreview()
        {
            var entries = CollectSprites();

            return entries.Count == 0 ? null : KEmojiAtlas.Build(entries, CellSize).Texture;
        }

        public static void Generate()
        {
            if (KEmojiMenu.PluginDisabled)
            {
                LogWarning(DisabledWarning);

                return;
            }

            var entries = CollectSprites();

            if (entries.Count == 0)
            {
                LogWarning(NoEntriesWarning);

                return;
            }

            var atlasPath = Data.atlasPath.ToProjectRelativePath();

            if (string.IsNullOrEmpty(atlasPath))
            {
                LogWarning(NoAtlasPathWarning);

                return;
            }

            if (!atlasPath.EndsWith(AtlasExtension, StringComparison.OrdinalIgnoreCase))
            {
                LogWarning(WrongExtensionWarning);

                return;
            }

            SaveData();

            var atlas = KEmojiAtlas.Build(entries, CellSize);

            WritePng(atlasPath, atlas);

            Object.DestroyImmediate(atlas.Texture);

            atlas.Texture = null;

            Slice(atlasPath, atlas);

            if (KEmojiMenu.TexturePackerJsonEnabled)
            {
                KEmojiCodeGen.WriteTexturePackerJson(atlas, atlasPath);
            }

            Log(string.Format(GeneratedFormat, atlasPath, entries.Count, CellSize));

            WriteSpriteAsset(atlasPath, atlas, entries);
            WriteConstants(atlas);
        }

        private static void WritePng(string atlasPath, KEmojiAtlas.Result atlas)
        {
            var absolutePath = atlasPath.ToAbsolutePath();
            var directory = Path.GetDirectoryName(absolutePath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(absolutePath, atlas.Texture.EncodeToPNG());

            AssetDatabase.ImportAsset(atlasPath);
        }

        private static void Slice(string atlasPath, KEmojiAtlas.Result atlas)
        {
            if (AssetImporter.GetAtPath(atlasPath) is not TextureImporter importer) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.mipmapEnabled = false;

            if (KEmojiMenu.PointFilterEnabled)
            {
                importer.filterMode = FilterMode.Point;
            }

            if (KEmojiMenu.UncompressedEnabled)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
            }

            importer.SaveAndReimport();

            KEmojiSlicer.Slice(importer, atlas);

            if (CountSprites(atlasPath) >= atlas.Placements.Count) return;

            LogWarning(NoSlicesWarning);
        }

        private static int CountSprites(string atlasPath)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(atlasPath);
            var count = 0;

            for (var i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite) count++;
            }

            return count;
        }

        private static void WriteSpriteAsset(string atlasPath, KEmojiAtlas.Result atlas, List<KEmojiData.Entry> entries)
        {
            if (!KEmojiMenu.SpriteAssetEnabled) return;

            if (KEmojiSpriteAsset.Generate(atlasPath, atlas, entries) is not { } spriteAssetPath)
            {
                LogWarning(NoSpriteAssetWarning);

                return;
            }

            Log(string.Format(SpriteAssetFormat, spriteAssetPath));
        }

        private static void WriteConstants(KEmojiAtlas.Result atlas)
        {
            if (!KEmojiMenu.ConstantsScriptEnabled) return;

            if (!KEmojiCodeGen.WriteConstants(atlas, Data)) return;

            Log(string.Format(ScriptFormat, Data.scriptPath));
        }

        #endregion

        #region Data Lifetime

        public static List<KEmojiData.Entry> CollectSprites()
        {
            var entries = Data.entries;
            var sprites = new List<KEmojiData.Entry>(entries.Count);

            for (var i = 0; i < entries.Count; i++)
            {
                if (!entries[i].sprite) continue;

                sprites.Add(entries[i]);
            }

            return sprites;
        }

        public static void MarkDataDirty() => EditorUtility.SetDirty(Data);

        public static void SaveData()
        {
            if (!_data) return;

            AssetDatabase.SaveAssetIfDirty(_data);
        }

        // Back to the state of a fresh install: no asset, nothing held in memory.
        public static void DeleteData()
        {
            AssetDatabase.DeleteAsset(DataAssetPath);

            _data = null;
        }

        private static KEmojiData LoadOrCreateData()
        {
            if (AssetDatabase.LoadAssetAtPath<KEmojiData>(DataAssetPath) is { } loaded) return loaded;

            var created = ScriptableObject.CreateInstance<KEmojiData>();

            created.scriptClassName = DefaultScriptClassName;

            return CreateDataAsset(created);
        }

        private static KEmojiData CreateDataAsset(KEmojiData data)
        {
            EnsureDataFolder();

            AssetDatabase.CreateAsset(data, DataAssetPath);
            AssetDatabase.SaveAssets();

            return data;
        }

        private static void EnsureDataFolder()
        {
            var folderPath = DataAssetPath.GetParentPath();

            if (AssetDatabase.IsValidFolder(folderPath)) return;

            Directory.CreateDirectory(folderPath);

            AssetDatabase.Refresh();
        }

        #endregion

        #region Logging

        private static void Log(string message) => Debug.Log(string.Format(LogFormat, message));

        private static void LogWarning(string message) => Debug.LogWarning(string.Format(LogFormat, message));

        #endregion
    }
}
#endif
