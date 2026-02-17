#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor; // 用于 GUIHelper
using UnityEditor;
using UnityEngine;
using System.Linq;
using GameModelSystem.Editor.Editor;
using Sirenix.OdinInspector; // 确保引用了 Cache 所在的命名空间

namespace GameModelSystem.Editor.Editor
{
    // 这个 Drawer 会自动接管项目中所有 AttributeId 类型的绘制
    public class AttributeIdDrawer : OdinValueDrawer<AttributeId>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            // 1. 获取当前值 (SmartValue 是 struct 的属性，修改它会触发 Odin 更新)
            uint currentId = this.ValueEntry.SmartValue.Id;
            
            // 2. 获取显示名称 (从缓存中查)
            string displayName = UniAttributeEditorCache.GetName(currentId);
            
            // 3. 处理 Missing 状态颜色
            bool isMissing = displayName.StartsWith("<Missing");
            if (isMissing) GUIHelper.PushColor(new Color(1f, 0.6f, 0.6f));

            // 4. 准备绘制区域
            Rect rect = EditorGUILayout.GetControlRect(label != null);
            if (label != null) rect = EditorGUI.PrefixLabel(rect, label);

            // 5. 绘制按钮 (看起来像下拉框)
            if (GUI.Button(rect, new GUIContent(displayName), EditorStyles.popup))
            {
                // 显示搜索选择器
                ShowSelector(rect, currentId);
            }

            if (isMissing) GUIHelper.PopColor();
        }

        private void ShowSelector(Rect rect, uint currentId)
        {
            // 从缓存获取所有选项
            var items = UniAttributeEditorCache.GetAllAttributes();
            
            var selector = new GenericSelector<ValueDropdownItem<uint>>(
                "Select Attribute", 
                false, 
                x => x.Text, 
                items
            );

            selector.EnableSingleClickToSelect();
            
            // 设置当前选中项
            var currentItem = items.FirstOrDefault(x => x.Value == currentId);
            if (currentItem.Value != 0) selector.SetSelection(currentItem);

            // 回调处理
            selector.SelectionConfirmed += (selection) =>
            {
                var newItem = selection.FirstOrDefault();
                
                // 【核心修复】
                // 通过 ValueEntry.SmartValue 设置新值
                // 这会自动处理 Struct 的赋值、序列化脏标记、Undo 记录等所有脏活
                this.ValueEntry.SmartValue = new AttributeId { Id = newItem.Value };
                
                // 强制应用修改并刷新界面
                this.ValueEntry.ApplyChanges();
                GUIHelper.RequestRepaint();
            };

            selector.ShowInPopup(rect);
        }
    }
}
#endif