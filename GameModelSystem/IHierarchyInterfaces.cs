
using System.Collections.Generic;
using UnityEngine;

// 1. 数据项接口：任何想在树里显示的东西，必须实现这个
public interface IHierarchyItem
{
    
#if UNITY_EDITOR
    uint ID { get; }
    string DisplayName { get; }
    uint ParentID { get; }
#endif
}

// 2. 数据持有者接口：SO 必须实现这个来告诉 TreeView 怎么操作数据
public interface IHierarchyOwner
{
#if UNITY_EDITOR
    // 获取所有数据
    IEnumerable<IHierarchyItem> GetHierarchyItems();
// --- 新增：获取用于编辑的具体数据对象 ---
    object GetItemForEditing(uint id);
    // 操作
    void OnHierarchy_AddChild(uint parentId);
    void OnHierarchy_Remove(uint id);
    void OnHierarchy_Rename(uint id, string newName);
    void OnHierarchy_Move(uint id, uint newParentId); // 拖拽逻辑
    
    // 视觉配置 (可选)
    Texture GetHierarchyIcon(uint id);
#endif
}