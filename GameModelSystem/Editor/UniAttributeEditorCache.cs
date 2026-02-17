#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Sirenix.OdinInspector; // 确保引用你的命名空间

namespace GameModelSystem.Editor.Editor
{
    public static class UniAttributeEditorCache
    {
        private static List<ValueDropdownItem<uint>> _cachedItems;
        // 反向查找表：ID -> 显示名称
        private static Dictionary<uint, string> _idToNameMap;
        private static bool _isDirty = true;

        public static void MarkDirty() => _isDirty = true;

        public static IEnumerable<ValueDropdownItem<uint>> GetAllAttributes()
        {
            if (_isDirty || _cachedItems == null) RebuildCache();
            return _cachedItems;
        }

        public static string GetName(uint id)
        {
            if (id == 0) return "None";
            if (_isDirty || _idToNameMap == null) RebuildCache();
            
            return _idToNameMap.TryGetValue(id, out var name) ? name : $"<Missing:{id}>";
        }

        private static void RebuildCache()
        {
            _cachedItems = new List<ValueDropdownItem<uint>>();
            _idToNameMap = new Dictionary<uint, string>();
            
            var guids = AssetDatabase.FindAssets("t:UniAttributeDefSO");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<UniAttributeDefSO>(path);
                if (so == null) continue;

                // 1. 构建当前 SO 的查找表 (ID -> Def)
                var allDefs = so.GetAllDefs().ToList();
                // 假设 Def 继承自 HierarchyDefItem，有 ParentGMID 字段
                // 这里我们需要反射或者转换类型来访问 ParentGMID，
                // 或者假设 GetFullPath 已经由 SO 正确实现。
                // 为了保险，我们在 Cache 层自己算一遍路径，满足“不包含SO名”的需求。
                
                var defMap = allDefs.ToDictionary(d => d.GMID);

                foreach (var def in allDefs)
                {
                    // 2. 递归构建路径
                    string fullPath = def.Name;
                    
                    // 尝试获取 ParentID (需要 Def 是 HierarchyDefItem 或类似结构)
                    // 这里假设 GameModelFieldDef 可能没有 ParentGMID，需要转型
                    uint parentId = 0;
                    if (def is HierarchyDefItem hDef) 
                    {
                        parentId = hDef.ParentGMID;
                    }
                    // 如果你的 Def 类型定义不同，请在此处调整获取 ParentID 的逻辑

                    var currentParentId = parentId;
                    int safety = 20;
                    while (currentParentId != 0 && safety-- > 0)
                    {
                        if (defMap.TryGetValue(currentParentId, out var parentDef))
                        {
                            fullPath = $"{parentDef.Name}/{fullPath}";
                            
                            if (parentDef is HierarchyDefItem pDef)
                                currentParentId = pDef.ParentGMID;
                            else
                                currentParentId = 0;
                        }
                        else
                        {
                            break; // 找不到父节点（可能跨 SO 了，或者断链）
                        }
                    }

                    // 3. 存入缓存
                    _cachedItems.Add(new ValueDropdownItem<uint>(fullPath, def.GMID));
                    
                    if (!_idToNameMap.ContainsKey(def.GMID))
                    {
                        _idToNameMap[def.GMID] = fullPath;
                    }
                }
            }
            
            // 排序，让下拉列表更好找
            _cachedItems.Sort((a, b) => string.CompareOrdinal(a.Text, b.Text));
            
            _isDirty = false;
        }
    }
    
    public class UniAttributeCacheRefresher : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            UniAttributeEditorCache.MarkDirty();
        }
    }
}
#endif