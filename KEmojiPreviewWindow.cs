#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Kingfisher.KEmoji
{
    public class KEmojiPreviewWindow : EditorWindow
    {
        #region Field

        private const string WindowTitle = "K-Emoji Preview";

        private const float MaxDisplaySize = 512;
        private const float MinDisplaySize = 128;
        private const float OutlineThickness = 1;

        private static readonly GUIContent TitleContent = new(WindowTitle);

        private static readonly Color DarkBackgroundColor = GetGreyscale(.16f);
        private static readonly Color LightBackgroundColor = GetGreyscale(.72f);
        private static readonly Color DarkOutlineColor = GetGreyscale(.1f);
        private static readonly Color LightOutlineColor = GetGreyscale(.55f);

        private static KEmojiPreviewWindow _instance;

        private Texture2D _texture;

        #endregion

        #region Property

        private static bool IsDark => EditorGUIUtility.isProSkin;

        private static Color BackgroundColor => IsDark ? DarkBackgroundColor : LightBackgroundColor;

        private static Color OutlineColor => IsDark ? DarkOutlineColor : LightOutlineColor;

        #endregion

        #region Unity Lifecycle

        private void OnGUI()
        {
            if (!this._texture)
            {
                Close();

                return;
            }

            var rect = new Rect(0, 0, position.width, position.height);

            EditorGUI.DrawRect(rect, BackgroundColor);

            GUI.DrawTexture(rect, this._texture, ScaleMode.ScaleToFit);

            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, OutlineThickness), OutlineColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - OutlineThickness, rect.width, OutlineThickness), OutlineColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, OutlineThickness, rect.height), OutlineColor);
            EditorGUI.DrawRect(new Rect(rect.xMax - OutlineThickness, rect.y, OutlineThickness, rect.height), OutlineColor);

            CloseOnEscape();
        }

        private void OnDisable()
        {
            if (this._texture)
            {
                DestroyImmediate(this._texture);
            }

            if (_instance != this) return;

            _instance = null;
        }

        #endregion

        #region Method

        public static void Open(Texture2D texture)
        {
            if (!texture) return;

            if (_instance)
            {
                _instance.Close();
            }

            var window = CreateInstance<KEmojiPreviewWindow>();

            window._texture = texture;
            window.titleContent = TitleContent;
            window.minSize = new Vector2(MinDisplaySize, MinDisplaySize);

            _instance = window;

            var scale = Mathf.Min(1f, MaxDisplaySize / Mathf.Max(texture.width, texture.height));
            var displayWidth = texture.width * scale;
            var displayHeight = texture.height * scale;
            var mainWindowRect = EditorGUIUtility.GetMainWindowPosition();

            window.ShowUtility();

            window.position = new Rect(
                mainWindowRect.x + (mainWindowRect.width - displayWidth) * .5f,
                mainWindowRect.y + (mainWindowRect.height - displayHeight) * .5f,
                displayWidth,
                displayHeight);
        }

        private void CloseOnEscape()
        {
            if (Event.current.type != EventType.KeyDown) return;

            if (Event.current.keyCode != KeyCode.Escape) return;

            Event.current.Use();

            Close();
        }

        private static Color GetGreyscale(float value, float alpha = 1) => new(value, value, value, alpha);

        #endregion
    }
}
#endif
