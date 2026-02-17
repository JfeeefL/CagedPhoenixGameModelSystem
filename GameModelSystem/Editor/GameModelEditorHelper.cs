#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor; // 必须引用
using UnityEngine;

public static class GameModelEditorHelper 
{
    // 静态方法，接收 InspectorProperty 作为上下文
    public static IEnumerable GetAvailableIds(InspectorProperty property)
    {
        var list = new ValueDropdownList<uint>();
        
        // 1. 向上查找 IGameModelDefOwner
        // property 是 GMID 字段
        var parent = property.Parent;
        IGameModelDefOwner defOwner = null;

        // 向上遍历直到找到 Owner (通常是 Hero/MonoBehaviour)
        // 增加最大深度限制防止死循环，虽然一般不会
        int maxDepth = 10;
        while (parent != null && maxDepth-- > 0)
        {
            // 尝试从当前层级的值中获取
            if (parent.ValueEntry != null && parent.ValueEntry.WeakSmartValue is IGameModelDefOwner owner)
            {
                defOwner = owner;
                break;
            }
            
            // 尝试从序列化根对象获取 (应对 ScriptableObject 或直接在组件上的情况)
            if (parent.SerializationRoot != null && parent.SerializationRoot.ValueEntry.WeakSmartValue is IGameModelDefOwner rootOwner)
            {
                defOwner = rootOwner;
                break;
            }
            
            parent = parent.Parent;
        }

        if (defOwner == null)
        {
            list.Add("Err: No DefOwner Found", 0);
            return list;
        }

        // 2. 生成列表
        var defs = defOwner.GetModelDefs();
        if (defs != null)
        {
            HashSet<uint> addedIds = new HashSet<uint>();
            foreach (var defSO in defs)
            {
                if (defSO == null) continue;
                foreach (var def in defSO.GetAllDefs())
                {
                    if (addedIds.Contains(def.GMID)) continue;
                    addedIds.Add(def.GMID);

                    // 处理名字和层级
                    string rawName = string.IsNullOrEmpty(def.Name) ? $"Field_{def.GMID}" : def.Name;
                    // 将 . 替换为 / 以支持 Odin 折叠层级
                    string displayPath = rawName.Replace('.', '/');
                    
                    
                    list.Add(displayPath, def.GMID);
                }
            }
        }
        
        return list;
    }
}
#endif