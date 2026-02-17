#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Sirenix.Utilities.Editor; // 依然使用 SdfIcons，或者换原生

public class HierarchyTreeView : TreeView
{
    private IHierarchyOwner _owner;
    
    // --- 新增：选择变更事件 ---
    public Action<IList<int>> OnSelectionChanged;

    public HierarchyTreeView(TreeViewState state, IHierarchyOwner owner) : base(state)
    {
        _owner = owner;
        showAlternatingRowBackgrounds = true;
        showBorder = true;
        Reload();
    }

    protected override TreeViewItem BuildRoot()
    {
        // 显式初始化 children 防止空数据报错
        var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root", children = new List<TreeViewItem>() };
        
        var rawItems = _owner.GetHierarchyItems().ToList();
        var lookup = new Dictionary<uint, TreeViewItem>();
        var itemsList = new List<TreeViewItem>();

        // 1. 创建 Item
        foreach (var data in rawItems)
        {
            var item = new TreeViewItem 
            { 
                id = (int)data.ID, 
                displayName = data.DisplayName,
                children = new List<TreeViewItem>() // 预初始化
            };
            lookup[data.ID] = item;
            itemsList.Add(item);
        }

        // 2. 构建层级
        foreach (var data in rawItems)
        {
            if (lookup.TryGetValue(data.ID, out var item))
            {
                if (data.ParentID != 0 && lookup.TryGetValue(data.ParentID, out var parentItem))
                {
                    parentItem.AddChild(item);
                }
                else
                {
                    root.AddChild(item);
                }
            }
        }

        SetupDepthsFromParentsAndChildren(root);
        return root;
    }

    // --- 绘制 ---
    protected override void RowGUI(RowGUIArgs args)
    {
        var item = args.item;
        Rect extraRect = args.rowRect;
        extraRect.x += GetContentIndent(item);
        extraRect.width = 16;

        // 从 Owner 获取图标
        var iconTex = _owner.GetHierarchyIcon((uint)item.id);
        
        if (iconTex != null)
        {
            Rect iconRect = extraRect;
            iconRect.height = 16;
            iconRect.y += (args.rowRect.height - 16) / 2;
            GUI.DrawTexture(iconRect, iconTex);
        }

        extraRect.x += 18;
        extraRect.width = args.rowRect.width - extraRect.x;

        if (!args.isRenaming)
            EditorGUI.LabelField(extraRect, item.displayName, EditorStyles.label);
    }

    // --- 交互：右键菜单 ---
    protected override void ContextClickedItem(int id)
    {
        var menu = new GenericMenu();
        SetSelection(new List<int> { id });

        menu.AddItem(new GUIContent("Add Child"), false, () => 
        {
            _owner.OnHierarchy_AddChild((uint)id);
            ReloadAndExpand(id);
        });

        menu.AddItem(new GUIContent("Rename"), false, () => BeginRename(FindItem(id, rootItem)));
        menu.AddItem(new GUIContent("Copy ID"), false, () => GUIUtility.systemCopyBuffer = ((uint)id).ToString());
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete"), false, () => DeleteSelected());

        menu.ShowAsContext();
        UnityEngine.Event.current.Use();
    }

    protected override void ContextClicked()
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create Root Item"), false, () => 
        {
            _owner.OnHierarchy_AddChild(0); // 0 代表根
            Reload();
        });
        menu.ShowAsContext();
        UnityEngine.Event.current.Use();
    }

    protected override void KeyEvent()
    {
        if (UnityEngine.Event.current.type == EventType.KeyDown && UnityEngine.Event.current.keyCode == KeyCode.Delete)
        {
            DeleteSelected();
            UnityEngine.Event.current.Use();
        }
    }
    
    // --- 新增：重写选择变更回调 ---
    protected override void SelectionChanged(IList<int> selectedIds)
    {
        base.SelectionChanged(selectedIds);
        OnSelectionChanged?.Invoke(selectedIds);
    }

    // --- 交互：拖拽 ---
    protected override bool CanStartDrag(CanStartDragArgs args) => true;
    protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
    {
        DragAndDrop.PrepareStartDrag();
        DragAndDrop.SetGenericData("HierarchyIDs", args.draggedItemIDs);
        DragAndDrop.StartDrag("Move Items");
    }

    protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
    {
        var draggedIDs = DragAndDrop.GetGenericData("HierarchyIDs") as List<int>;
        if (draggedIDs == null || draggedIDs.Count == 0) return DragAndDropVisualMode.None;

        // 死锁检测
        foreach (var id in draggedIDs)
        {
            if (IsDescendant(args.parentItem, id)) 
                return DragAndDropVisualMode.Rejected;
        }

        if (args.performDrop)
        {
            uint newParentID = (args.parentItem == null || args.parentItem.id == -1) ? 0 : (uint)args.parentItem.id;
            foreach (var id in draggedIDs)
            {
                // 调用接口移动数据
                _owner.OnHierarchy_Move((uint)id, newParentID);
            }
            Reload();
            if (newParentID != 0) SetExpanded(args.parentItem.id, true);
        }
        return DragAndDropVisualMode.Move;
    }

    // --- 交互：重命名 ---
    protected override bool CanRename(TreeViewItem item) => true;
    protected override void RenameEnded(RenameEndedArgs args)
    {
        if (args.acceptedRename && args.newName != args.originalName)
        {
            _owner.OnHierarchy_Rename((uint)args.itemID, args.newName);
            Reload();
        }
    }

    // --- 辅助 ---
    private void DeleteSelected()
    {
        var selection = GetSelection();
        if (selection.Count == 0) return;
        if (EditorUtility.DisplayDialog("Delete", $"Delete {selection.Count} items?", "Yes", "No"))
        {
            foreach (var id in selection) _owner.OnHierarchy_Remove((uint)id);
            SetSelection(new List<int>());
            Reload();
        }
    }

    private void ReloadAndExpand(int id)
    {
        Reload();
        SetExpanded(id, true);
    }
    
    private bool IsDescendant(TreeViewItem item, int ancestorID)
    {
        while (item != null)
        {
            if (item.id == ancestorID) return true;
            item = item.parent;
        }
        return false;
    }

    public void ForceReload() => Reload();
}
#endif