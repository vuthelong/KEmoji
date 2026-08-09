#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Kingfisher.KEmoji.Libs
{
    public static class KUtils
    {
        #region Field

        private const char PathSeparator = '/';
        private const char WindowsPathSeparator = '\\';
        private const string AssetsFolderName = "Assets";
        private const string AssetsFolderPrefix = AssetsFolderName + "/";

        private static string _projectRootPath;

        #endregion

        #region Property

        public static string ProjectRootPath
        {
            get
            {
                if (_projectRootPath != null) return _projectRootPath;

                var dataPath = Application.dataPath.NormalizeSlashes();

                return _projectRootPath = dataPath.Substring(0, dataPath.Length - AssetsFolderName.Length);
            }
        }

        #endregion

        #region Path

        public static string NormalizeSlashes(this string path) => string.IsNullOrEmpty(path) ? path : path.Replace(WindowsPathSeparator, PathSeparator);

        public static string CombinePath(this string path, string name) => path + PathSeparator + name;

        public static string GetParentPath(this string path) => path.LastIndexOf(PathSeparator) is var i && i > 0 ? path.Substring(0, i) : path;

        public static bool IsInAssetsFolder(this string path) => !string.IsNullOrEmpty(path) && path.NormalizeSlashes().StartsWith(AssetsFolderPrefix, StringComparison.OrdinalIgnoreCase);

        public static string ToProjectRelativePath(this string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            var normalized = path.NormalizeSlashes();

            if (normalized.StartsWith(AssetsFolderPrefix, StringComparison.OrdinalIgnoreCase)) return normalized;

            if (!normalized.StartsWith(ProjectRootPath, StringComparison.OrdinalIgnoreCase)) return null;

            var relative = normalized.Substring(ProjectRootPath.Length);

            return relative.StartsWith(AssetsFolderPrefix, StringComparison.OrdinalIgnoreCase) ? relative : null;
        }

        public static string ToAbsolutePath(this string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            var normalized = path.NormalizeSlashes();

            return Path.IsPathRooted(normalized) ? normalized : ProjectRootPath + normalized;
        }

        public static void EnsureAssetFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return;

            if (AssetDatabase.IsValidFolder(folderPath)) return;

            var parts = folderPath.NormalizeSlashes().Split(PathSeparator);
            var currentPath = parts[0];

            for (var i = 1; i < parts.Length; i++)
            {
                var nextPath = currentPath.CombinePath(parts[i]);

                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }

                currentPath = nextPath;
            }
        }

        #endregion

        #region Nested Type

        public static class EditorPrefsCached
        {
            private static readonly Dictionary<string, bool> Bools = new();
            private static readonly Dictionary<string, float> Floats = new();

            public static bool GetBool(string key, bool defaultValue = false)
            {
                if (Bools.TryGetValue(key, out var cached)) return cached;

                if (KSettingsBridge.TryGetBool(key, out var stored)) return Bools[key] = stored;

                return Bools[key] = EditorPrefs.GetBool(key, defaultValue);
            }

            public static void SetBool(string key, bool value)
            {
                Bools[key] = value;

                KSettingsBridge.SetBool(key, value);
                EditorPrefs.SetBool(key, value);
            }

            public static float GetFloat(string key, float defaultValue = 0)
            {
                if (Floats.TryGetValue(key, out var cached)) return cached;

                if (KSettingsBridge.TryGetFloat(key, out var stored)) return Floats[key] = stored;

                return Floats[key] = EditorPrefs.GetFloat(key, defaultValue);
            }

            public static void SetFloat(string key, float value)
            {
                Floats[key] = value;

                KSettingsBridge.SetFloat(key, value);
                EditorPrefs.SetFloat(key, value);
            }
        }

        private static class KSettingsBridge
        {
            private const string SettingsTypeName = "Kingfisher.KSetting.KSettings, Kingfisher.KSetting";
            private const string HasBoolMethodName = "HasBool";
            private const string HasFloatMethodName = "HasFloat";
            private const string GetBoolMethodName = "GetBool";
            private const string GetFloatMethodName = "GetFloat";
            private const string SetBoolMethodName = "SetBool";
            private const string SetFloatMethodName = "SetFloat";

            private static readonly object[] OneArg = new object[1];
            private static readonly object[] TwoArgs = new object[2];

            private static readonly Type SettingsType = Type.GetType(SettingsTypeName);

            private static readonly MethodInfo HasBool = SettingsType?.GetMethod(HasBoolMethodName, new[] { typeof(string) });
            private static readonly MethodInfo HasFloat = SettingsType?.GetMethod(HasFloatMethodName, new[] { typeof(string) });

            private static readonly MethodInfo GetBoolMethod = SettingsType?.GetMethod(GetBoolMethodName, new[] { typeof(string), typeof(bool) });
            private static readonly MethodInfo GetFloatMethod = SettingsType?.GetMethod(GetFloatMethodName, new[] { typeof(string), typeof(float) });

            private static readonly MethodInfo SetBoolMethod = SettingsType?.GetMethod(SetBoolMethodName, new[] { typeof(string), typeof(bool) });
            private static readonly MethodInfo SetFloatMethod = SettingsType?.GetMethod(SetFloatMethodName, new[] { typeof(string), typeof(float) });

            public static bool TryGetBool(string key, out bool value)
            {
                value = default;

                if (!Has(HasBool, key)) return false;

                value = (bool)Invoke(GetBoolMethod, key, false);

                return true;
            }

            public static bool TryGetFloat(string key, out float value)
            {
                value = default;

                if (!Has(HasFloat, key)) return false;

                value = (float)Invoke(GetFloatMethod, key, 0f);

                return true;
            }

            public static void SetBool(string key, bool value) => Invoke(SetBoolMethod, key, value);

            public static void SetFloat(string key, float value) => Invoke(SetFloatMethod, key, value);

            private static bool Has(MethodInfo has, string key)
            {
                if (has == null) return false;

                OneArg[0] = key;

                return (bool)has.Invoke(null, OneArg);
            }

            private static object Invoke(MethodInfo method, string key, object value)
            {
                if (method == null) return value;

                TwoArgs[0] = key;
                TwoArgs[1] = value;

                return method.Invoke(null, TwoArgs);
            }
        }

        #endregion
    }
}
#endif
