using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameModelSystem.Editor
{
    // 1. TagDefinition 现在继承 HierarchyDefItem

    // 2. TagDefSO 继承泛型基类
    [CreateAssetMenu(fileName = "NewTagDef", menuName = "Framework/Tag Definition")]
    public partial class TagDefSO : HierarchicalDefSO<TagDef>, ISerializationCallbackReceiver
    {
        // --- 运行时特有逻辑 (Bake & Ancestors) ---
        
        [SerializeField, HideInInspector]
        private List<AncestorEntry> _serializedAncestors = new List<AncestorEntry>();
        private Dictionary<uint, uint[]> _runtimeAncestorDict = new Dictionary<uint, uint[]>();

        [Serializable]
        private struct AncestorEntry
        {
            public uint TagID;
            public uint[] Ancestors;
        }

        public uint[] GetAncestors(uint gmid)
        {
            if (_runtimeAncestorDict.TryGetValue(gmid, out var ancestors)) return ancestors;
            return new uint[] { gmid };
        }

        // --- 重写钩子 ---

        // 当编辑器数据发生变化时，自动重新 Bake
        protected override void OnDataChanged()
        {
            Bake();
        }

        // 定制创建逻辑 (比如前缀)
        protected override TagDef CreateNewItem(uint parentID)
        {
            var item = base.CreateNewItem(parentID);
            item.Name = "New Tag";
            return item;
        }

        // 保持原有的 Bake 逻辑不变，只需把 _tags 换成 _items
        private void Bake()
        {
            if (_items == null) return;
            var lookup = _items.ToDictionary(t => t.GMID);
            var newData = new List<AncestorEntry>();

            foreach (var tag in _items)
            {
                var chain = new List<uint>();
                var current = tag;
                int safe = 100;
                while (current != null && safe-- > 0)
                {
                    chain.Add(current.GMID);
                    if (current.ParentGMID == 0 || !lookup.TryGetValue(current.ParentGMID, out var parent)) break;
                    current = parent;
                }
                newData.Add(new AncestorEntry { TagID = tag.GMID, Ancestors = chain.ToArray() });
            }
            _serializedAncestors = newData;
        }
        
        // 序列化回调
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) Bake();
#endif
        }

        public void OnAfterDeserialize()
        {
            _runtimeAncestorDict.Clear();
            if (_serializedAncestors != null)
            {
                foreach (var entry in _serializedAncestors)
                    _runtimeAncestorDict[entry.TagID] = entry.Ancestors;
            }
        }
    }
}