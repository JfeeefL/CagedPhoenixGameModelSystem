#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Sirenix.OdinInspector.Editor; // 核心引用：用于绘制 PropertyTree
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;

public class HierarchyDrawer
{
    private HierarchyTreeView _treeView;
    private TreeViewState _state;
    private IHierarchyOwner _owner;

    // --- 新增：用于绘制选中项的 Odin PropertyTree ---
    private PropertyTree _selectedInspector;
    private object _currentSelectedObject;
    private Vector2 _scrollPos; // 下半部分的滚动条

    public HierarchyDrawer(IHierarchyOwner owner)
    {
        _owner = owner;
        _state = new TreeViewState();
    }

    public void Draw()
    {
        if (_treeView == null)
        {
            _treeView = new HierarchyTreeView(_state, _owner);
            // 订阅选择事件
            _treeView.OnSelectionChanged += OnTreeSelectionChanged;
            // 尝试恢复选择（如果 Assembly Reload 后）
            OnTreeSelectionChanged(_treeView.GetSelection()); 
        }

        // --- 布局计算 ---
        // 我们将区域分为上下两部分：
        // 上部分：Hierarchy (固定最小高度，或可拖动)
        // 下部分：Selected Inspector (自动填充剩余空间)

        // 1. 绘制 Hierarchy (固定高度，比如 300，或者你可以做一个 Splitter)
        GUILayout.Label("Hierarchy", EditorStyles.boldLabel);
        
        // 计算 TreeView 高度 (限制最大高度，防止把 Inspector 挤没了)
        float treeHeight = Mathf.Clamp(_treeView.totalHeight + 20, 100, 300); 
        Rect treeRect = GUILayoutUtility.GetRect(0, 10000, treeHeight, treeHeight);
        GUI.Box(treeRect, GUIContent.none, EditorStyles.helpBox);
        _treeView.OnGUI(treeRect);

        // 2. 绘制选中项的详细 Inspector
        DrawSelectedInspector();
    }

    private void DrawSelectedInspector()
    {
        if (_selectedInspector == null || _currentSelectedObject == null) return;

        GUILayout.Space(10);
        SirenixEditorGUI.HorizontalLineSeparator();
        GUILayout.Label("Detailed Inspector", EditorStyles.boldLabel);

        // 开始绘制 Odin Inspector
        // 这一步会自动处理所有的 [ShowInInspector], [Range], [ListDrawer] 等特性
        
        GUILayout.BeginVertical(GUI.skin.box);
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        // 重要：Draw(false) 表示不绘制最外层的 Object 标题，只绘制内容
        _selectedInspector.Draw(false); 

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void OnTreeSelectionChanged(IList<int> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            _currentSelectedObject = null;
            _selectedInspector = null;
        }
        else
        {
            // 单选逻辑：只显示第一个选中项
            // 多选逻辑比较复杂，暂时只支持单选
            uint id = (uint)ids[0];
            var obj = _owner.GetItemForEditing(id);

            if (obj != null && obj != _currentSelectedObject)
            {
                _currentSelectedObject = obj;
                // 创建 Odin PropertyTree
                _selectedInspector = PropertyTree.Create(_currentSelectedObject);
            }
        }
    }

    public void ForceReload()
    {
        if (_treeView != null) 
        {
            _treeView.ForceReload();
            // 刷新 Inspector 以防数据在 Undo/Redo 中被替换
            OnTreeSelectionChanged(_treeView.GetSelection());
        }
    }
}
#endif