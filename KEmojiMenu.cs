#if UNITY_EDITOR
using static Kingfisher.KEmoji.Libs.KUtils;

namespace Kingfisher.KEmoji
{
    internal static class KEmojiMenu
    {
        #region Field

        private const string KeyPrefix = "KEmoji-kingfisher-";

        private const string SpriteAssetEnabledKey = KeyPrefix + "spriteAssetEnabled";
        private const string ConstantsScriptEnabledKey = KeyPrefix + "constantsScriptEnabled";
        private const string TexturePackerJsonEnabledKey = KeyPrefix + "texturePackerJsonEnabled";
        private const string PreserveMetricsEnabledKey = KeyPrefix + "preserveMetricsEnabled";
        private const string PointFilterEnabledKey = KeyPrefix + "pointFilterEnabled";
        private const string UncompressedEnabledKey = KeyPrefix + "uncompressedEnabled";
        private const string CellSizeKey = KeyPrefix + "cellSize";
        private const string PluginDisabledKey = KeyPrefix + "pluginDisabled";

        private const float DefaultCellSize = 128;

        public static readonly string[] SettingsLayout =
        {
            "# Output",
            "SpriteAssetEnabled|Create a TextMeshPro sprite asset",
            "ConstantsScriptEnabled|Generate a script of sprite name constants",
            "TexturePackerJsonEnabled|Write a TexturePacker JSON beside the atlas",

            "# Atlas",
            "~CellSize|Cell size|32|512",
            "PointFilterEnabled|Import with point filtering",
            "UncompressedEnabled|Import uncompressed",

            "# Sprite asset",
            "PreserveMetricsEnabled|Keep the metrics of sprites already in the asset",
        };

        #endregion

        #region Property

        public static bool SpriteAssetEnabled { get => EditorPrefsCached.GetBool(SpriteAssetEnabledKey, true); set => EditorPrefsCached.SetBool(SpriteAssetEnabledKey, value); }

        public static bool ConstantsScriptEnabled { get => EditorPrefsCached.GetBool(ConstantsScriptEnabledKey, true); set => EditorPrefsCached.SetBool(ConstantsScriptEnabledKey, value); }

        public static bool TexturePackerJsonEnabled { get => EditorPrefsCached.GetBool(TexturePackerJsonEnabledKey, false); set => EditorPrefsCached.SetBool(TexturePackerJsonEnabledKey, value); }

        public static bool PreserveMetricsEnabled { get => EditorPrefsCached.GetBool(PreserveMetricsEnabledKey, true); set => EditorPrefsCached.SetBool(PreserveMetricsEnabledKey, value); }

        public static bool PointFilterEnabled { get => EditorPrefsCached.GetBool(PointFilterEnabledKey, true); set => EditorPrefsCached.SetBool(PointFilterEnabledKey, value); }

        public static bool UncompressedEnabled { get => EditorPrefsCached.GetBool(UncompressedEnabledKey, true); set => EditorPrefsCached.SetBool(UncompressedEnabledKey, value); }

        public static float CellSize { get => EditorPrefsCached.GetFloat(CellSizeKey, DefaultCellSize); set => EditorPrefsCached.SetFloat(CellSizeKey, value); }

        public static bool PluginDisabled { get => EditorPrefsCached.GetBool(PluginDisabledKey, false); set => EditorPrefsCached.SetBool(PluginDisabledKey, value); }

        public static string DataPath => KEmoji.DataAssetPath;

        #endregion

        #region Method

        public static void DeleteData() => KEmoji.DeleteData();

        public static void OpenTool() => KEmojiWindow.Open();

        #endregion
    }
}
#endif
