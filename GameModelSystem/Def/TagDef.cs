using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameModelSystem.Editor
{
    [Serializable]
    public class TagDef : HierarchyDefItem
    {

        [Title("@Name", "Tag Configuration", TitleAlignments.Split)]
        [PropertyOrder(-1)]
        [ShowInInspector, ReadOnly]
        private string _idDisplay => $"ID: {GMID}";

        [BoxGroup("Settings")]
        [LabelWidth(120)]
        public Color DebugColor = Color.white;
    }
}