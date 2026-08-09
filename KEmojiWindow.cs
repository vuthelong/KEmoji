#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using static Kingfisher.KEmoji.Libs.KUtils;
using Object = UnityEngine.Object;

namespace Kingfisher.KEmoji
{
    public class KEmojiWindow : EditorWindow
    {
        #region Field

        private const string MenuPath = "Tools/KEmoji";
        private const string WindowTitle = "K-Emoji";
        private const int MenuPriority = 1;

        private const string SpritesSectionTitle = "Sprites";
        private const string AtlasSectionTitle = "Atlas";
        private const string ScriptSectionTitle = "Constants script";

        private const string SavePanelTitle = "Atlas path";
        private const string SavePanelDefaultName = "Emoji";
        private const string PngExtension = "png";
        private const string ScriptPanelTitle = "Constants script path";
        private const string ScriptPanelDefaultName = "SpriteText";
        private const string CsExtension = "cs";

        private const string SummaryFormat = "{0} sprites - {1} px cells";
        private const string OutsideAssetsWarning = "K-Emoji: pick a path inside the project's Assets folder.";
        private const string DisabledMessage = "K-Emoji is turned off in Tools > KTools Setting.";
        private const string MissingTextMeshProMessage = "TextMeshPro is not installed, so no sprite asset can be written. Install com.unity.ugui, or turn the sprite asset off in Tools > KTools Setting.";
        private const string EmptyListMessage = "No sprites yet - drop some below.";
        private const string DropAreaMessage = "Drop sprites or textures here";

        private const float MinWindowWidth = 520f;
        private const float MinWindowHeight = 380f;
        private const float FooterHeight = 48f;
        private const float Padding = 10f;
        private const float DividerThickness = 1f;
        private const float ContentTopSpacing = 6f;
        private const float ContentBottomSpacing = 10f;
        private const float AtlasPanelBottomSpacing = 4f;
        private const float SectionTopSpacing = 16f;
        private const float SectionHeaderHeight = 18f;
        private const float SectionHeaderGap = 6f;
        private const float SectionRuleGap = 8f;
        private const float RowSpacing = 6f;
        private const float ColumnGap = 4f;
        private const float ListInset = 4f;

        private const float GripWidth = 12f;
        private const float GripLineWidth = 8f;
        private const float GripLineThickness = 1f;
        private const float GripLineGap = 3f;
        private const int GripLineCount = 3;

        private const float HexLabelWidth = 26f;
        private const float HexFieldWidth = 54f;
        private const float RatioLabelWidth = 36f;
        private const float RatioFieldWidth = 44f;
        private const float RemoveButtonWidth = 20f;
        private const float AddButtonWidth = 22f;
        private const float MinColumnWidth = 56f;
        private const float SpriteColumnRatio = .45f;

        private const float DropAreaHeight = 46f;
        private const float NoticeHeight = 38f;
        private const float MinListHeight = 60f;
        private const int ScriptRowCount = 3;
        private const float LabelColumnWidth = 74f;
        private const float ButtonGap = 6f;
        private const float BrowseButtonWidth = 62f;
        private const float PingButtonWidth = 44f;

        private const float FooterButtonHeight = 28f;
        private const float FooterButtonGap = 8f;
        private const float FooterButtonWidth = 112f;

        private const float SectionTextAlpha = .6f;
        private const float FooterTextAlpha = .45f;
        private const float NoticeTextAlpha = .5f;
        private const float GripAlpha = .35f;
        private const float LightSelectionBoost = 1.2f;

        private const int NoIndex = -1;

        private static readonly float RowHeight = EditorGUIUtility.singleLineHeight + RowSpacing;

        private static readonly float DropSectionHeight = ContentTopSpacing + DropAreaHeight + AtlasPanelBottomSpacing;

        private static readonly Color DarkWindowColor = GetGreyscale(.22f);
        private static readonly Color LightWindowColor = GetGreyscale(.78f);
        private static readonly Color DarkPanelColor = GetGreyscale(.19f);
        private static readonly Color LightPanelColor = GetGreyscale(.735f);
        private static readonly Color DarkSelectionColor = new(.17f, .365f, .535f);
        private static readonly Color LightSelectionColor = new Color(.2f, .375f, .555f) * LightSelectionBoost;
        private static readonly Color DarkDividerColor = GetGreyscale(.13f);
        private static readonly Color LightDividerColor = GetGreyscale(.6f);
        private static readonly Color DarkRowHoverColor = GetGreyscale(1f, .05f);
        private static readonly Color LightRowHoverColor = GetGreyscale(0f, .05f);
        private static readonly Color DarkDangerTintColor = new(1.35f, .62f, .58f);
        private static readonly Color LightDangerTintColor = new(1.3f, .72f, .68f);
        private static readonly Color DarkDangerTextColor = new(1f, .78f, .75f);
        private static readonly Color LightDangerTextColor = new(.42f, .06f, .04f);

        private static readonly GUIContent AddButtonContent = new("+", "Add an empty row");
        private static readonly GUIContent RemoveButtonContent = new("x", "Remove this sprite");
        private static readonly GUIContent BrowseButtonContent = new("Browse");
        private static readonly GUIContent PingButtonContent = new("Ping", "Show the atlas in the Project window");
        private static readonly GUIContent PreviewButtonContent = new("Preview", "Build the atlas in memory and show it");
        private static readonly GUIContent GenerateButtonContent = new("Generate");
        private static readonly GUIContent HexLabelContent = new("Hex", "Unicode code point the sprite answers to, in hexadecimal");
        private static readonly GUIContent RatioLabelContent = new("Scale", "How much of the cell the sprite fills");
        private static readonly GUIContent NamespaceLabelContent = new("Namespace");
        private static readonly GUIContent ClassLabelContent = new("Class");
        private static readonly GUIContent EmptyListContent = new(EmptyListMessage);
        private static readonly GUIContent DropAreaContent = new(DropAreaMessage);
        private static readonly GUIContent SpritesSectionContent = new(SpritesSectionTitle);
        private static readonly GUIContent AtlasSectionContent = new(AtlasSectionTitle);
        private static readonly GUIContent ScriptSectionContent = new(ScriptSectionTitle);
        private static readonly GUIContent SummaryContent = new();

        private static GUIStyle _sectionStyle;
        private static GUIStyle _footerStyle;
        private static GUIStyle _noticeStyle;
        private static GUIStyle _dropAreaStyle;
        private static GUIStyle _footerButtonStyle;
        private static GUIStyle _dangerButtonStyle;
        private static bool _hasBuiltStyles;
        private static bool _isStyleDark;
        private static int _summarySpriteCount = NoIndex;
        private static int _summaryCellSize = NoIndex;
        private static string _resolvedAtlasPath;
        private static Object _resolvedAtlas;
        private static bool _hasResolvedAtlas;

        private Vector2 _scroll;
        private float _listTop;
        private int _dragIndex = NoIndex;
        private int _pendingRemoveIndex = NoIndex;
        private bool _pendingAdd;

        #endregion

        #region Property

        private static bool IsDark => EditorGUIUtility.isProSkin;

        private static Color WindowBackground => IsDark ? DarkWindowColor : LightWindowColor;

        private static Color PanelBackground => IsDark ? DarkPanelColor : LightPanelColor;

        private static Color SelectedBackground => IsDark ? DarkSelectionColor : LightSelectionColor;

        private static Color DividerColor => IsDark ? DarkDividerColor : LightDividerColor;

        private static Color RowHover => IsDark ? DarkRowHoverColor : LightRowHoverColor;

        private static Color DangerTint => IsDark ? DarkDangerTintColor : LightDangerTintColor;

        private static KEmojiData Data => KEmoji.Data;

        private static bool IsDisabled => KEmojiMenu.PluginDisabled;

        private static bool HasSpriteAssetWarning => KEmojiMenu.SpriteAssetEnabled && !KEmojiSpriteAsset.IsAvailable;

        private static float ScrollbarWidth => GUI.skin.verticalScrollbar.fixedWidth + GUI.skin.verticalScrollbar.margin.left;

        private static float ScriptRowsHeight => KEmojiMenu.ConstantsScriptEnabled ? SectionTopSpacing + SectionHeaderHeight + SectionHeaderGap + RowHeight * ScriptRowCount : 0f;

        private static float AtlasPanelHeight => ContentTopSpacing + SectionHeaderHeight + SectionHeaderGap + RowHeight + ScriptRowsHeight + AtlasPanelBottomSpacing;

        private static float NoticesHeight
        {
            get
            {
                var height = 0f;

                if (IsDisabled)
                    height += ContentTopSpacing + NoticeHeight;

                if (HasSpriteAssetWarning)
                    height += ContentTopSpacing + NoticeHeight;

                return height;
            }
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable() => wantsMouseMove = true;

        private void OnDestroy() => KEmoji.SaveData();

        private void OnGUI()
        {
            BuildStyles();

            EditorGUI.DrawRect(new Rect(0f, 0f, position.width, position.height), WindowBackground);

            var noticesHeight = NoticesHeight;
            var atlasPanelHeight = AtlasPanelHeight;
            var listHeight = Mathf.Max(MinListHeight, position.height - noticesHeight - DropSectionHeight - atlasPanelHeight - FooterHeight);

            DrawNotices(new Rect(0f, 0f, position.width, noticesHeight));

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(IsDisabled);

            DrawSpritesSection(new Rect(0f, noticesHeight, position.width, listHeight));
            DrawDropSection(new Rect(0f, noticesHeight + listHeight, position.width, DropSectionHeight));
            DrawAtlasPanel(new Rect(0f, noticesHeight + listHeight + DropSectionHeight, position.width, atlasPanelHeight));

            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                KEmoji.MarkDataDirty();
            }

            DrawFooter(new Rect(0f, position.height - FooterHeight, position.width, FooterHeight));

            UpdateDrag();
            ApplyPending();

            if (Event.current.type != EventType.MouseMove) return;

            Repaint();
        }

        private void OnFocus() => _hasResolvedAtlas = false;

        private void OnLostFocus() => KEmoji.SaveData();

        #endregion

        #region Drawing

        private static void DrawNotices(Rect rect)
        {
            var y = rect.y + ContentTopSpacing;

            if (IsDisabled)
            {
                EditorGUI.HelpBox(new Rect(rect.x + Padding, y, rect.width - Padding * 2f, NoticeHeight), DisabledMessage, MessageType.Info);

                y += NoticeHeight + ContentTopSpacing;
            }

            if (!HasSpriteAssetWarning) return;

            EditorGUI.HelpBox(new Rect(rect.x + Padding, y, rect.width - Padding * 2f, NoticeHeight), MissingTextMeshProMessage, MessageType.Warning);
        }

        private void DrawSpritesSection(Rect rect)
        {
            var headerRect = DrawSectionHeader(rect, rect.y + ContentTopSpacing, SpritesSectionContent, AddButtonWidth + SectionRuleGap);

            DrawAddButton(headerRect);

            var listTop = headerRect.yMax + SectionHeaderGap;
            var listRect = new Rect(rect.x + Padding, listTop, rect.width - Padding * 2f, rect.yMax - listTop - ContentBottomSpacing);

            EditorGUI.DrawRect(listRect, PanelBackground);

            if (Data.entries.Count == 0)
            {
                GUI.Label(listRect, EmptyListContent, _noticeStyle);

                return;
            }

            DrawRows(listRect);
        }

        private void DrawAddButton(Rect headerRect)
        {
            var buttonRect = new Rect(headerRect.xMax - AddButtonWidth, headerRect.y, AddButtonWidth, headerRect.height);

            if (!GUI.Button(buttonRect, AddButtonContent, EditorStyles.miniButton)) return;

            this._pendingAdd = true;
        }

        private void DrawRows(Rect listRect)
        {
            var entries = Data.entries;
            var contentHeight = entries.Count * RowHeight + ListInset * 2f;
            var contentWidth = contentHeight > listRect.height ? listRect.width - ScrollbarWidth : listRect.width;

            this._listTop = listRect.y + ListInset;
            this._scroll = GUI.BeginScrollView(listRect, this._scroll, new Rect(0f, 0f, contentWidth, contentHeight));

            for (var i = 0; i < entries.Count; i++)
            {
                DrawRow(new Rect(ListInset, ListInset + i * RowHeight, contentWidth - ListInset * 2f, RowHeight), i);
            }

            GUI.EndScrollView();
        }

        private void DrawRow(Rect rect, int index)
        {
            var entry = Data.entries[index];

            if (index == this._dragIndex)
            {
                EditorGUI.DrawRect(rect, SelectedBackground);
            }
            else if (rect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rect, RowHover);
            }

            var fieldY = rect.y + (rect.height - EditorGUIUtility.singleLineHeight) * .5f;
            var fieldHeight = EditorGUIUtility.singleLineHeight;
            var fixedWidth = HexLabelWidth + HexFieldWidth + RatioLabelWidth + RatioFieldWidth + RemoveButtonWidth + ColumnGap * 5f;
            var flexibleWidth = Mathf.Max(MinColumnWidth * 2f, rect.width - GripWidth - ColumnGap - fixedWidth);
            var spriteWidth = flexibleWidth * SpriteColumnRatio;
            var nameWidth = flexibleWidth - spriteWidth;

            var x = rect.x;

            DrawGrip(new Rect(x, rect.y, GripWidth, rect.height), index);

            x += GripWidth + ColumnGap;

            var picked = EditorGUI.ObjectField(new Rect(x, fieldY, spriteWidth, fieldHeight), entry.sprite, typeof(Sprite), false) as Sprite;

            if (picked != entry.sprite)
            {
                entry.sprite = picked;
                entry.spriteName = picked ? picked.name : string.Empty;
            }

            x += spriteWidth + ColumnGap;

            entry.spriteName = EditorGUI.TextField(new Rect(x, fieldY, nameWidth, fieldHeight), entry.spriteName);

            x += nameWidth + ColumnGap;

            GUI.Label(new Rect(x, fieldY, HexLabelWidth, fieldHeight), HexLabelContent, _footerStyle);

            x += HexLabelWidth;

            entry.unicode = EditorGUI.TextField(new Rect(x, fieldY, HexFieldWidth, fieldHeight), entry.unicode);

            x += HexFieldWidth + ColumnGap;

            GUI.Label(new Rect(x, fieldY, RatioLabelWidth, fieldHeight), RatioLabelContent, _footerStyle);

            x += RatioLabelWidth;

            entry.sizeRatio = EditorGUI.FloatField(new Rect(x, fieldY, RatioFieldWidth, fieldHeight), entry.sizeRatio);

            x += RatioFieldWidth + ColumnGap;

            var previousBackground = GUI.backgroundColor;

            GUI.backgroundColor = DangerTint;

            if (GUI.Button(new Rect(x, fieldY, RemoveButtonWidth, fieldHeight), RemoveButtonContent, _dangerButtonStyle))
            {
                this._pendingRemoveIndex = index;
            }

            GUI.backgroundColor = previousBackground;
        }

        private void DrawGrip(Rect rect, int index)
        {
            var color = GetSkinTextColor(GripAlpha);
            var x = rect.x + (rect.width - GripLineWidth) * .5f;
            var y = rect.y + (rect.height - (GripLineCount - 1) * GripLineGap) * .5f;

            for (var i = 0; i < GripLineCount; i++)
            {
                EditorGUI.DrawRect(new Rect(x, y + i * GripLineGap, GripLineWidth, GripLineThickness), color);
            }

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.MoveArrow);

            if (Event.current.type != EventType.MouseDown) return;

            if (!rect.Contains(Event.current.mousePosition)) return;

            this._dragIndex = index;

            GUI.FocusControl(null);

            Event.current.Use();
        }

        private static void DrawDropSection(Rect rect)
        {
            var dropRect = new Rect(rect.x + Padding, rect.y + ContentTopSpacing, rect.width - Padding * 2f, DropAreaHeight);

            GUI.Box(dropRect, DropAreaContent, _dropAreaStyle);

            HandleDrop(dropRect);
        }

        private static void HandleDrop(Rect rect)
        {
            if (!rect.Contains(Event.current.mousePosition)) return;

            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                Event.current.Use();

                return;
            }

            if (Event.current.type != EventType.DragPerform) return;

            DragAndDrop.AcceptDrag();

            AddDragged(DragAndDrop.objectReferences);

            Event.current.Use();
        }

        private static void DrawAtlasPanel(Rect panelRect)
        {
            EditorGUI.DrawRect(panelRect, PanelBackground);
            EditorGUI.DrawRect(new Rect(panelRect.x, panelRect.y, panelRect.width, DividerThickness), DividerColor);

            var y = panelRect.y + ContentTopSpacing;

            DrawSectionHeader(panelRect, y, AtlasSectionContent, 0f);

            y += SectionHeaderHeight + SectionHeaderGap;

            DrawAtlasRow(GetRowRect(panelRect, y));

            if (!KEmojiMenu.ConstantsScriptEnabled) return;

            y += RowHeight + SectionTopSpacing;

            DrawSectionHeader(panelRect, y, ScriptSectionContent, 0f);

            DrawScriptRows(panelRect, y + SectionHeaderHeight + SectionHeaderGap);
        }

        private static void DrawAtlasRow(Rect rect)
        {
            var pingRect = new Rect(rect.xMax - PingButtonWidth, rect.y, PingButtonWidth, rect.height);
            var browseRect = new Rect(pingRect.x - ButtonGap - BrowseButtonWidth, rect.y, BrowseButtonWidth, rect.height);

            Data.atlasPath = EditorGUI.TextField(new Rect(rect.x, rect.y, browseRect.x - ButtonGap - rect.x, rect.height), Data.atlasPath);

            if (GUI.Button(browseRect, BrowseButtonContent, EditorStyles.miniButton))
            {
                Browse(SavePanelTitle, SavePanelDefaultName, PngExtension, path => Data.atlasPath = path);
            }

            var atlas = LoadAtlas();

            EditorGUI.BeginDisabledGroup(!atlas);

            if (GUI.Button(pingRect, PingButtonContent, EditorStyles.miniButton))
            {
                EditorGUIUtility.PingObject(atlas);

                Selection.activeObject = atlas;
            }

            EditorGUI.EndDisabledGroup();
        }

        private static void DrawScriptRows(Rect panelRect, float y)
        {
            var pathRect = GetRowRect(panelRect, y);
            var browseRect = new Rect(pathRect.xMax - BrowseButtonWidth, pathRect.y, BrowseButtonWidth, pathRect.height);

            Data.scriptPath = EditorGUI.TextField(new Rect(pathRect.x, pathRect.y, browseRect.x - ButtonGap - pathRect.x, pathRect.height), Data.scriptPath);

            if (GUI.Button(browseRect, BrowseButtonContent, EditorStyles.miniButton))
            {
                Browse(ScriptPanelTitle, ScriptPanelDefaultName, CsExtension, path => Data.scriptPath = path);
            }

            var namespaceRect = GetRowRect(panelRect, y + RowHeight);

            GUI.Label(new Rect(namespaceRect.x, namespaceRect.y, LabelColumnWidth, namespaceRect.height), NamespaceLabelContent);

            Data.scriptNamespace = EditorGUI.TextField(new Rect(namespaceRect.x + LabelColumnWidth, namespaceRect.y, namespaceRect.width - LabelColumnWidth, namespaceRect.height), Data.scriptNamespace);

            var classRect = GetRowRect(panelRect, y + RowHeight * 2f);

            GUI.Label(new Rect(classRect.x, classRect.y, LabelColumnWidth, classRect.height), ClassLabelContent);

            Data.scriptClassName = EditorGUI.TextField(new Rect(classRect.x + LabelColumnWidth, classRect.y, classRect.width - LabelColumnWidth, classRect.height), Data.scriptClassName);
        }

        private static void DrawFooter(Rect rect)
        {
            EditorGUI.DrawRect(rect, PanelBackground);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, DividerThickness), DividerColor);

            var buttonY = rect.y + (rect.height - FooterButtonHeight) * .5f;
            var generateRect = new Rect(rect.xMax - Padding - FooterButtonWidth, buttonY, FooterButtonWidth, FooterButtonHeight);
            var previewRect = new Rect(generateRect.x - FooterButtonGap - FooterButtonWidth, buttonY, FooterButtonWidth, FooterButtonHeight);
            var hasSprites = KEmoji.HasSprites;

            GUI.Label(new Rect(rect.x + Padding, rect.y, previewRect.x - FooterButtonGap - rect.x, rect.height), GetSummaryContent(), _footerStyle);

            EditorGUI.BeginDisabledGroup(IsDisabled || !hasSprites);

            if (GUI.Button(previewRect, PreviewButtonContent, _footerButtonStyle))
            {
                KEmojiPreviewWindow.Open(KEmoji.BuildPreview());
            }

            EditorGUI.BeginDisabledGroup(!Data.atlasPath.IsInAssetsFolder());

            if (GUI.Button(generateRect, GenerateButtonContent, _footerButtonStyle))
            {
                KEmoji.Generate();

                _hasResolvedAtlas = false;
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
        }

        private static Rect DrawSectionHeader(Rect sectionRect, float y, GUIContent title, float reservedRight)
        {
            var rect = new Rect(sectionRect.x + Padding, y, sectionRect.width - Padding * 2f, SectionHeaderHeight);

            var labelWidth = _sectionStyle.CalcSize(title).x;

            GUI.Label(new Rect(rect.x, rect.y, labelWidth, rect.height), title, _sectionStyle);

            var ruleX = rect.x + labelWidth + SectionRuleGap;
            var ruleWidth = rect.xMax - reservedRight - ruleX;

            if (ruleWidth > 0f)
            {
                EditorGUI.DrawRect(new Rect(ruleX, rect.y + (rect.height - DividerThickness) * .5f, ruleWidth, DividerThickness), DividerColor);
            }

            return rect;
        }

        #endregion

        #region Sprite Source

        private static void AddDragged(Object[] draggedObjects)
        {
            for (var i = 0; i < draggedObjects.Length; i++)
            {
                if (draggedObjects[i] is Sprite sprite)
                {
                    Data.entries.Add(new KEmojiData.Entry(sprite));

                    continue;
                }

                if (draggedObjects[i] is not Texture2D texture) continue;

                AddSlices(texture);
            }

            KEmoji.MarkDataDirty();
        }

        private static void AddSlices(Texture2D texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);

            if (string.IsNullOrEmpty(path)) return;

            var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);

            for (var i = 0; i < subAssets.Length; i++)
            {
                if (subAssets[i] is not Sprite sprite) continue;

                Data.entries.Add(new KEmojiData.Entry(sprite));
            }
        }

        private void ApplyPending()
        {
            if (this._pendingAdd)
            {
                this._pendingAdd = false;

                Data.entries.Add(new KEmojiData.Entry());

                KEmoji.MarkDataDirty();
            }

            if (this._pendingRemoveIndex == NoIndex) return;

            if (this._pendingRemoveIndex < Data.entries.Count)
            {
                Data.entries.RemoveAt(this._pendingRemoveIndex);

                KEmoji.MarkDataDirty();
            }

            this._pendingRemoveIndex = NoIndex;
            this._dragIndex = NoIndex;
        }

        #endregion

        #region Reorder

        private void UpdateDrag()
        {
            if (this._dragIndex == NoIndex) return;

            if (Data.entries.Count == 0)
            {
                this._dragIndex = NoIndex;

                return;
            }

            if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore)
            {
                this._dragIndex = NoIndex;

                Repaint();

                return;
            }

            if (Event.current.type != EventType.MouseDrag) return;

            var target = Mathf.Clamp(Mathf.FloorToInt((Event.current.mousePosition.y + this._scroll.y - this._listTop) / RowHeight), 0, Data.entries.Count - 1);

            if (target == this._dragIndex) return;

            Move(this._dragIndex, target);

            this._dragIndex = target;

            Repaint();
        }

        private static void Move(int from, int to)
        {
            var entries = Data.entries;
            var moved = entries[from];

            entries.RemoveAt(from);
            entries.Insert(to, moved);

            KEmoji.MarkDataDirty();
        }

        #endregion

        #region Style

        private static void BuildStyles()
        {
            if (_hasBuiltStyles && _isStyleDark == IsDark) return;

            _hasBuiltStyles = true;
            _isStyleDark = IsDark;

            _sectionStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
            _sectionStyle.normal.textColor = GetSkinTextColor(SectionTextAlpha);

            _footerStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
            _footerStyle.normal.textColor = GetSkinTextColor(FooterTextAlpha);

            _noticeStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            _noticeStyle.normal.textColor = GetSkinTextColor(NoticeTextAlpha);

            _dropAreaStyle = new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleCenter };
            _dropAreaStyle.normal.textColor = GetSkinTextColor(NoticeTextAlpha);

            _footerButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };

            var dangerText = IsDark ? DarkDangerTextColor : LightDangerTextColor;

            _dangerButtonStyle = new GUIStyle(EditorStyles.miniButton);
            _dangerButtonStyle.normal.textColor = dangerText;
            _dangerButtonStyle.hover.textColor = dangerText;
            _dangerButtonStyle.active.textColor = dangerText;
        }

        private static Color GetSkinTextColor(float alpha) => GetGreyscale(IsDark ? 1f : 0f, alpha);

        private static Color GetGreyscale(float value, float alpha = 1) => new(value, value, value, alpha);

        #endregion

        #region Method

        [MenuItem(MenuPath, false, MenuPriority)]
        public static void Open()
        {
            var window = GetWindow<KEmojiWindow>(utility: false, title: WindowTitle, focus: true);

            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
        }

        private static void Browse(string title, string defaultName, string extension, Action<string> apply)
        {
            var chosen = EditorUtility.SaveFilePanel(title, Application.dataPath, defaultName, extension);

            if (string.IsNullOrEmpty(chosen)) return;

            var relativePath = chosen.ToProjectRelativePath();

            if (string.IsNullOrEmpty(relativePath))
            {
                Debug.LogWarning(OutsideAssetsWarning);

                return;
            }

            apply(relativePath);

            KEmoji.MarkDataDirty();

            GUIUtility.ExitGUI();
        }

        private static GUIContent GetSummaryContent()
        {
            var spriteCount = Data.entries.Count;
            var cellSize = KEmoji.CellSize;

            if (spriteCount == _summarySpriteCount && cellSize == _summaryCellSize) return SummaryContent;

            _summarySpriteCount = spriteCount;
            _summaryCellSize = cellSize;

            SummaryContent.text = string.Format(SummaryFormat, spriteCount, cellSize);

            return SummaryContent;
        }

        // The asset database is off limits during a repaint, so the lookup only runs when the
        // path changes, when the window is focused again, and right after a generate.
        private static Object LoadAtlas()
        {
            if (_hasResolvedAtlas && _resolvedAtlasPath == Data.atlasPath) return _resolvedAtlas;

            _hasResolvedAtlas = true;
            _resolvedAtlasPath = Data.atlasPath;

            var relativePath = _resolvedAtlasPath.ToProjectRelativePath();

            return _resolvedAtlas = string.IsNullOrEmpty(relativePath) ? null : AssetDatabase.LoadAssetAtPath<Object>(relativePath);
        }

        private static Rect GetRowRect(Rect sectionRect, float y) =>
            new(sectionRect.x + Padding, y + RowSpacing * .5f, sectionRect.width - Padding * 2f, EditorGUIUtility.singleLineHeight);

        #endregion
    }
}
#endif
