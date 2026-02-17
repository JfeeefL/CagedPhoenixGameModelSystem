#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using GameModelSystem;

namespace GameModelSystem.Editor
{
    // A custom Odin Drawer for NameHash
    // Supports modification of Dictionary Keys
    public class NameHashDrawer : OdinValueDrawer<NameHash>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            // 1. Get the current value
            var current = this.ValueEntry.SmartValue;
            string oldName = current.Name;

            // 2. Sanitize the label
            // FIX: EditorGUILayout.DelayedTextField throws if label is null.
            // When drawing a Dictionary Key, Odin often passes null as the label.
            GUIContent safeLabel = label ?? GUIContent.none;

            // 3. Draw the text field
            // Use DelayedTextField to avoid re-hashing on every character typed
            string newName = EditorGUILayout.DelayedTextField(safeLabel, oldName);

            // 4. Detect changes
            if (newName != oldName)
            {
                // Core Fix:
                // Construct a new NameHash and assign it to SmartValue.
                // Odin captures this assignment:
                // - If it's a normal field, it just assigns.
                // - If it's a Dictionary Key, Odin automatically handles Dictionary.Remove(old) -> Dictionary.Add(new).
                this.ValueEntry.SmartValue = new NameHash(newName);
                
                // Ensure changes are applied and marked as dirty
                this.ValueEntry.ApplyChanges();
            }
        }
    }
}
#endif