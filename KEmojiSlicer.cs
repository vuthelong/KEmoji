#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kingfisher.KEmoji
{
    // Sprite rects are written through the 2D Sprite package when it is installed, and
    // through the importer's own sprite sheet when it is not.
    internal static class KEmojiSlicer
    {
        #region Field

        private const string FactoriesTypeName = "UnityEditor.U2D.Sprites.SpriteDataProviderFactories";
        private const string ProviderTypeName = "UnityEditor.U2D.Sprites.ISpriteEditorDataProvider";
        private const string SpriteRectTypeName = "UnityEditor.U2D.Sprites.SpriteRect";

        private const string InitMethodName = "Init";
        private const string GetProviderMethodName = "GetSpriteEditorDataProviderFromObject";
        private const string InitProviderMethodName = "InitSpriteEditorDataProvider";
        private const string GetSpriteRectsMethodName = "GetSpriteRects";
        private const string SetSpriteRectsMethodName = "SetSpriteRects";
        private const string ApplyMethodName = "Apply";
        private const string TargetObjectPropertyName = "targetObject";

        private const string NamePropertyName = "name";
        private const string RectPropertyName = "rect";
        private const string AlignmentPropertyName = "alignment";
        private const string PivotPropertyName = "pivot";
        private const string SpriteIdPropertyName = "spriteID";

        private static readonly Vector2 CenterPivot = new(.5f, .5f);

        private static readonly Type FactoriesType = FindType(FactoriesTypeName);
        private static readonly Type ProviderType = FindType(ProviderTypeName);
        private static readonly Type SpriteRectType = FindType(SpriteRectTypeName);

        private static readonly MethodInfo InitMethod = FactoriesType?.GetMethod(InitMethodName, Type.EmptyTypes);
        private static readonly MethodInfo GetProviderMethod = FactoriesType?.GetMethod(GetProviderMethodName, new[] { typeof(Object) });

        private static readonly MethodInfo InitProviderMethod = ProviderType?.GetMethod(InitProviderMethodName, Type.EmptyTypes);
        private static readonly MethodInfo GetSpriteRectsMethod = ProviderType?.GetMethod(GetSpriteRectsMethodName, Type.EmptyTypes);
        private static readonly MethodInfo SetSpriteRectsMethod = ProviderType?.GetMethod(SetSpriteRectsMethodName);
        private static readonly MethodInfo ApplyMethod = ProviderType?.GetMethod(ApplyMethodName, Type.EmptyTypes);
        private static readonly PropertyInfo TargetObjectProperty = ProviderType?.GetProperty(TargetObjectPropertyName);

        private static readonly PropertyInfo NameProperty = SpriteRectType?.GetProperty(NamePropertyName);
        private static readonly PropertyInfo RectProperty = SpriteRectType?.GetProperty(RectPropertyName);
        private static readonly PropertyInfo AlignmentProperty = SpriteRectType?.GetProperty(AlignmentPropertyName);
        private static readonly PropertyInfo PivotProperty = SpriteRectType?.GetProperty(PivotPropertyName);
        private static readonly PropertyInfo SpriteIdProperty = SpriteRectType?.GetProperty(SpriteIdPropertyName);

        #endregion

        #region Property

        public static bool IsProviderAvailable => InitMethod != null && GetProviderMethod != null && InitProviderMethod != null && SetSpriteRectsMethod != null && ApplyMethod != null && SpriteRectType != null;

        #endregion

        #region Slicing

        public static void Slice(TextureImporter importer, KEmojiAtlas.Result atlas)
        {
            if (SliceWithProvider(importer, atlas)) return;

            SliceWithImporter(importer, atlas);
        }

        private static bool SliceWithProvider(TextureImporter importer, KEmojiAtlas.Result atlas)
        {
            if (!IsProviderAvailable) return false;

            var factories = Activator.CreateInstance(FactoriesType);

            InitMethod.Invoke(factories, null);

            var provider = GetProviderMethod.Invoke(factories, new object[] { importer });

            if (provider == null) return false;

            InitProviderMethod.Invoke(provider, null);
            SetSpriteRectsMethod.Invoke(provider, new object[] { BuildRects(provider, atlas) });
            ApplyMethod.Invoke(provider, null);

            if (TargetObjectProperty?.GetValue(provider) is not AssetImporter target) return false;

            target.SaveAndReimport();

            return true;
        }

        private static void SliceWithImporter(TextureImporter importer, KEmojiAtlas.Result atlas)
        {
            var cellSize = KEmoji.CellSize;
            var sheet = new SpriteMetaData[atlas.Placements.Count];

            for (var i = 0; i < atlas.Placements.Count; i++)
            {
                var placement = atlas.Placements[i];

                sheet[i] = new SpriteMetaData
                {
                    name = placement.Name,
                    rect = new Rect(placement.X, placement.Y, cellSize, cellSize),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = CenterPivot,
                };
            }

#pragma warning disable 618
            importer.spritesheet = sheet;
#pragma warning restore 618

            importer.SaveAndReimport();
        }

        #endregion

        #region Sprite Rect

        // Ids are carried over by name, so references to an already sliced atlas survive a regeneration.
        private static Array BuildRects(object provider, KEmojiAtlas.Result atlas)
        {
            var idsByName = CollectIds(provider);
            var cellSize = KEmoji.CellSize;
            var rects = Array.CreateInstance(SpriteRectType, atlas.Placements.Count);

            for (var i = 0; i < atlas.Placements.Count; i++)
            {
                var placement = atlas.Placements[i];
                var rect = Activator.CreateInstance(SpriteRectType);

                NameProperty.SetValue(rect, placement.Name);
                RectProperty.SetValue(rect, new Rect(placement.X, placement.Y, cellSize, cellSize));
                AlignmentProperty.SetValue(rect, SpriteAlignment.Center);
                PivotProperty.SetValue(rect, CenterPivot);
                SpriteIdProperty.SetValue(rect, idsByName.TryGetValue(placement.Name, out var id) ? id : GUID.Generate());

                rects.SetValue(rect, i);
            }

            return rects;
        }

        private static Dictionary<string, GUID> CollectIds(object provider)
        {
            var idsByName = new Dictionary<string, GUID>();

            if (GetSpriteRectsMethod?.Invoke(provider, null) is not Array existing) return idsByName;

            for (var i = 0; i < existing.Length; i++)
            {
                var rect = existing.GetValue(i);

                if (rect == null) continue;

                if (NameProperty.GetValue(rect) is not string name || string.IsNullOrEmpty(name)) continue;

                idsByName[name] = (GUID)SpriteIdProperty.GetValue(rect);
            }

            return idsByName;
        }

        #endregion

        #region Method

        private static Type FindType(string fullName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (var i = 0; i < assemblies.Length; i++)
            {
                if (assemblies[i].GetType(fullName) is { } type) return type;
            }

            return null;
        }

        #endregion
    }
}
#endif
