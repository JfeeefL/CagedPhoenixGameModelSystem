using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;
#endif

// T 必须是继承自 HierarchyDefItem 的具体类 (如 TagDefinition)
public abstract class HierarchicalDefSO<T> : GameModelDefBaseSO, IHierarchyOwner
    where T : HierarchyDefItem, new()
{
    // --- 统一的数据存储 ---
    [SerializeField, HideInInspector]
    protected List<T> _items = new List<T>();

    // --- GameModelDefBaseSO 实现 ---
    public override GameModelFieldDef GetDef(uint gmid) => _items.FirstOrDefault(t => t.GMID == gmid);
    public override IEnumerable<GameModelFieldDef> GetAllDefs() => _items.Cast<GameModelFieldDef>();
    
    public override string GetFullPath(uint gmid)
    {
        // 1. 找到当前节点
        var current = _items.FirstOrDefault(t => t.GMID == gmid);
        if (current == null) return "Unknown";

        // 2. 如果没有父节点，直接返回名字 (性能优化)
        if (current.ParentGMID == 0) return current.Name;

        // 3. 向上回溯构建路径
        // 使用 StringBuilder 避免大量的字符串拼接 GC
        // 注意：这是一个 O(Depth * N) 的操作，但在 Editor 下完全可以接受
        var pathBuilder = new System.Collections.Generic.Stack<string>();
        pathBuilder.Push(current.Name);

        uint parentId = current.ParentGMID;
        int safetyCounter = 50; // 防止死循环

        while (parentId != 0 && safetyCounter-- > 0)
        {
            var parent = _items.FirstOrDefault(t => t.GMID == parentId);
            if (parent == null) break; // 断链保护
            
            pathBuilder.Push(parent.Name);
            parentId = parent.ParentGMID;
        }

        // 4. 拼接字符串，使用 '/' 作为分隔符
        // BaseModelReference 会做 Replace('.', '/')，所以这里直接用 '/' 也是兼容的
        return string.Join("/", pathBuilder);
    }

    // --- 供子类重写的钩子 ---
    
    // 当数据发生变动 (增删改移动) 后调用。TagDefSO 可在此调用 Bake()
    protected virtual void OnDataChanged() { }

    // 创建新项的工厂方法 (子类可以重写以设置默认值)
    protected virtual T CreateNewItem(uint parentID)
    {
        return new T
        {
            Name = "New Item",
            GMID = (uint)Guid.NewGuid().GetHashCode(),
            ParentGMID = parentID
        };
    }
    
    // 获取图标 (子类可重写)
    public virtual Texture GetHierarchyIcon(uint id)
    {
#if UNITY_EDITOR
        var def = GetDef(id);
        if (def == null) return null;

        // 复用基类的逻辑检查是否有子节点
        if (_items.Any(t => t.ParentGMID == id)) 
            return SdfIcons.CreateTransparentIconTexture(SdfIconType.Folder, Color.yellow, 16, 16, 0);

        
        return SdfIcons.CreateTransparentIconTexture(SdfIconType.Box, Color.white, 16, 16, 0);
#else
        return null;
#endif
    }

    // ========================================================================
    // IHierarchyOwner 实现 (通用逻辑)
    // ========================================================================

#if UNITY_EDITOR
    public IEnumerable<IHierarchyItem> GetHierarchyItems() => _items.Cast<IHierarchyItem>();
    
    public object GetItemForEditing(uint id) => GetDef(id);

    public void OnHierarchy_AddChild(uint parentId)
    {
        Undo.RecordObject(this, "Add Item");
        var newItem = CreateNewItem(parentId);
        _items.Add(newItem);
        
        FinishEdit();
    }

    public void OnHierarchy_Remove(uint id)
    {
        Undo.RecordObject(this, "Remove Item");
        
        // 通用递归删除
        void RemoveRecursive(uint targetId)
        {
            var children = _items.Where(t => t.ParentGMID == targetId).ToList();
            foreach (var child in children) RemoveRecursive(child.GMID);
            _items.RemoveAll(t => t.GMID == targetId);
        }
        RemoveRecursive(id);
        
        FinishEdit();
    }

    public void OnHierarchy_Rename(uint id, string newName)
    {
        var item = _items.FirstOrDefault(t => t.GMID == id);
        if (item != null && item.Name != newName)
        {
            Undo.RecordObject(this, "Rename Item");
            item.Name = newName;
            FinishEdit();
        }
    }

    public void OnHierarchy_Move(uint id, uint newParentId)
    {
        var item = _items.FirstOrDefault(t => t.GMID == id);
        if (item != null && item.ParentGMID != newParentId)
        {
            // 防死锁检测
            if (IsDescendant(newParentId, id)) 
            {
                Debug.LogWarning("Cannot move parent into child.");
                return;
            }

            Undo.RecordObject(this, "Move Item");
            item.ParentGMID = newParentId;
            FinishEdit();
        }
    }

    // 提交编辑
    protected void FinishEdit()
    {
        OnDataChanged(); // 触发钩子 (比如 Tag 的 Bake)
        EditorUtility.SetDirty(this);
    }

    // 通用防死锁
    private bool IsDescendant(uint potentialDescendantID, uint targetID)
    {
        if (potentialDescendantID == 0) return false;
        if (potentialDescendantID == targetID) return true;
        
        var current = _items.FirstOrDefault(t => t.GMID == potentialDescendantID);
        int safe = 100;
        while (current != null && safe-- > 0)
        {
            if (current.GMID == targetID) return true;
            if (current.ParentGMID == 0) return false;
            current = _items.FirstOrDefault(t => t.GMID == current.ParentGMID);
        }
        return false;
    }
#endif

    // ========================================================================
    // Editor 绘制入口 (复用 HierarchyDrawer)
    // ========================================================================
#if UNITY_EDITOR
    
    private HierarchyDrawer _drawer;

    [OnInspectorGUI]
    [PropertyOrder(10)]
    protected virtual void DrawEditor()
    {
        if (_drawer == null) _drawer = new HierarchyDrawer(this);
        _drawer.Draw();
    }
    
    protected virtual void OnEnable() => Undo.undoRedoPerformed += OnUndoRedo;
    protected virtual void OnDisable() => Undo.undoRedoPerformed -= OnUndoRedo;
    
    private void OnUndoRedo()
    {
        _drawer?.ForceReload();
        OnDataChanged(); // Undo 后也需要重新 Bake
    }
#endif
}