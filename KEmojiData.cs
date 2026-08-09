#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingfisher.KEmoji
{
    public class KEmojiData : ScriptableObject
    {
        #region Field

        private const float DefaultSizeRatio = 1;

        public List<Entry> entries = new();

        public string atlasPath = "";

        public string scriptPath = "";
        public string scriptNamespace = "";
        public string scriptClassName = "";

        #endregion

        #region Nested Type

        [Serializable]
        public class Entry
        {
            public Sprite sprite;

            public string spriteName = "";
            public string unicode = "";

            public float sizeRatio = DefaultSizeRatio;

            public Entry()
            {
            }

            public Entry(Sprite sprite)
            {
                this.sprite = sprite;
                this.spriteName = sprite ? sprite.name : "";
            }
        }

        #endregion
    }
}
#endif
