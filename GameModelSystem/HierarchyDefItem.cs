using System;
using Sirenix.OdinInspector;
using UnityEngine;

// 所有想要在 Hierarchy 中显示的定义都继承这个
[Serializable]
public abstract class HierarchyDefItem : GameModelFieldDef, IHierarchyItem
{
    [HideInInspector] 
    public uint ParentGMID;

    // --- 接口实现 ---
    public uint ID => GMID;
    public uint ParentID => ParentGMID;
    
    // 给 TreeView 显示的名字 (不显示在 Detailed Inspector 中)
    [HideInInspector] 
    public string DisplayName => Name; 
    
    // 统一构造函数逻辑 (可选)
    public HierarchyDefItem()
    {
        // 默认生成一个 ID，防止初始化为 0
        if (GMID == 0) GMID = (uint)Guid.NewGuid().GetHashCode();
    }
}