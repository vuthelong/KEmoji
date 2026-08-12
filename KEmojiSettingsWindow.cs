#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Kingfisher.KEmoji
{
    internal class KEmojiSettingsWindow : EditorWindow
    {
        #region Field

        private const string MenuPath = "Tools/Kingfisher/KEmoji/Settings";
        private const string WindowTitle = "KEmoji";

        private const string KSettingsWindowTypeName = "Kingfisher.KSetting.KSettingsWindow, Kingfisher.KSetting";
        private const string OpenMethodName = "Open";
        private const string DisabledPropertyName = "PluginDisabled";

        private const string EnabledLabel = "Enabled";
        private const string EnabledSuffix = "Enabled";
        private const char WordSeparator = ' ';

        private const float WindowWidth = 320f;
        private const float WindowHeight = 360f;

        private const BindingFlags PropertyBindingFlags = BindingFlags.Public | BindingFlags.Static;

        private static PropertyInfo[] _toggleProperties;
        private static PropertyInfo _disabledProperty;

        #endregion

        #region Unity Lifecycle

        private void OnEnable() => CacheProperties();

        private void OnGUI()
        {
            if (_disabledProperty != null)
            {
                DrawToggle(_disabledProperty, EnabledLabel, invert: true);

                EditorGUILayout.Space();
            }

            using var _ = new EditorGUI.DisabledScope(_disabledProperty != null && (bool)_disabledProperty.GetValue(null));

            for (var i = 0; i < _toggleProperties.Length; i++)
            {
                DrawToggle(_toggleProperties[i], Prettify(_toggleProperties[i].Name));
            }
        }

        #endregion

        #region Method

        [MenuItem(MenuPath)]
        public static void Open()
        {
            if (Type.GetType(KSettingsWindowTypeName)?.GetMethod(OpenMethodName, PropertyBindingFlags) is { } openMethod)
            {
                openMethod.Invoke(null, null);

                return;
            }

            CacheProperties();

            var window = GetWindow<KEmojiSettingsWindow>(utility: false, title: WindowTitle, focus: true);

            window.minSize = new Vector2(WindowWidth, WindowHeight);
        }

        private static void CacheProperties()
        {
            if (_toggleProperties != null) return;

            var all = typeof(KEmojiMenu).GetProperties(PropertyBindingFlags);
            var toggles = new List<PropertyInfo>();

            for (var i = 0; i < all.Length; i++)
            {
                var property = all[i];

                if (property.PropertyType != typeof(bool) || !property.CanRead || !property.CanWrite) continue;

                if (property.Name == DisabledPropertyName)
                {
                    _disabledProperty = property;

                    continue;
                }

                toggles.Add(property);
            }

            _toggleProperties = toggles.ToArray();
        }

        private static void DrawToggle(PropertyInfo property, string label, bool invert = false)
        {
            var value = (bool)property.GetValue(null);
            var displayValue = invert ? !value : value;
            var newDisplayValue = EditorGUILayout.ToggleLeft(label, displayValue);

            if (newDisplayValue == displayValue) return;

            property.SetValue(null, invert ? !newDisplayValue : newDisplayValue);
        }

        private static string Prettify(string propertyName)
        {
            if (propertyName.EndsWith(EnabledSuffix, StringComparison.Ordinal))
                propertyName = propertyName.Substring(0, propertyName.Length - EnabledSuffix.Length);

            var builder = new StringBuilder();

            for (var i = 0; i < propertyName.Length; i++)
            {
                var character = propertyName[i];

                if (i == 0)
                {
                    builder.Append(char.ToUpperInvariant(character));

                    continue;
                }

                if (char.IsUpper(character) && !char.IsUpper(propertyName[i - 1]))
                    builder.Append(WordSeparator).Append(char.ToLowerInvariant(character));
                else
                    builder.Append(character);
            }

            return builder.ToString();
        }

        #endregion
    }
}
#endif
