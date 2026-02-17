using System;
using System.Collections.Generic;
using System.Linq;
using Binder; // 引用 InjectTarget 所在的命名空间
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using System.Reflection;
#endif

[Serializable]
public abstract class BaseModelReference<TData>
{
    [InjectTarget]
    [SerializeReference]
    [HideInInspector, ReadOnly]
    public ModelTargetResolver Resolver;

    // --- 2. 核心数据 ---
    
    [HideLabel]
    [CustomValueDrawer("DrawGMIDSelector")] 
    public uint TargetGMID;
#if UNITY_EDITOR
    protected virtual void OnIDSelected(uint id, GameModelDefBaseSO sourceSo)
    {
    }
#endif
    // --- 3. 运行时逻辑 ---
    
    protected virtual IEnumerable<GameModelDefBaseSO> GetDefinitionSources()
    {
        var resolverToUse = Resolver;

#if UNITY_EDITOR
        if (resolverToUse == null)
        {
            resolverToUse = FindEditorResolver();
        }
#endif

        if (resolverToUse == null) yield break;
        
#if UNITY_EDITOR
        var owner = resolverToUse.GetDefOwnerForEditor();
        if (owner != null)
        {
            foreach (var def in owner.GetModelDefs()) yield return def;
        }
#else
        // 运行时逻辑（如果有）
#endif
    }

    protected virtual bool IsDefinitionValid(GameModelFieldDef def) => true;

    // --- 4. 编辑器辅助逻辑 ---

#if UNITY_EDITOR
    
    // 【核心黑魔法】在编辑器下智能查找 Provider
    private ModelTargetResolver FindEditorResolver()
    {
        // 1. 尝试获取当前选中的对象 (Context)
        var targetObj = Selection.activeGameObject;
        if (targetObj == null) return null;

        // 2. 获取该物体上所有的 MonoBehaviour
        var components = targetObj.GetComponents<MonoBehaviour>();
        
        foreach (var comp in components)
        {
            if (comp == null) continue;
            var type = comp.GetType();

            // 3. 扫描字段，寻找 [ModelProvider] 且类型为 ModelTargetResolver 的字段
            // 这里我们复用 Reflect 工具或者手动反射
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (typeof(ModelTargetResolver).IsAssignableFrom(field.FieldType))
                {
                    if (field.GetCustomAttribute<ModelProviderAttribute>() != null)
                    {
                        // 找到第一个符合条件的 Resolver Provider，借用它
                        return field.GetValue(comp) as ModelTargetResolver;
                    }
                }
            }
        }
        return null;
    }

    protected uint DrawGMIDSelector(uint value, GUIContent label)
    {
        string displayName = null;
        bool isMissing = false;
        
        // 尝试获取用于预览的 Resolver
        var tempResolver = Resolver ?? FindEditorResolver();

        // 如果连预览的 Resolver 都找不到，提示用户配置
        if (tempResolver == null)
        {
             if (label != null) EditorGUILayout.PrefixLabel(label);
             GUILayout.Label("<No Resolver Provider Found>", EditorStyles.centeredGreyMiniLabel);
             return value;
        }

        // A. 尝试获取当前 ID 的显示名称
        if (value != 0)
        {
            var found = false;
            foreach (var source in GetDefinitionSources())
            {
                if (source == null) continue;
                var def = source.GetDef(value);
                if (def != null)
                {
                    displayName = source.GetFullPath(value).Replace('.', '/');
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                isMissing = true;
                displayName = $"<Missing: {value}>";
            }
        }
        else
        {
            displayName = "None";
        }

        if (isMissing) GUIHelper.PushColor(new Color(1f, 0.6f, 0.6f));

        Rect rect = EditorGUILayout.GetControlRect();
        if (label != null) rect = EditorGUI.PrefixLabel(rect, label);

        if (GUI.Button(rect, new GUIContent(displayName), EditorStyles.popup))
        {
            ShowSelector(rect, value);
        }

        if (isMissing) GUIHelper.PopColor();

        return value;
    }

    private void ShowSelector(Rect rect, uint currentValue)
    {
        var dropdownList = new ValueDropdownList<uint>();
        var sourceMap = new Dictionary<uint, GameModelDefBaseSO>();
        
        foreach (var source in GetDefinitionSources())
        {
            if (source == null) continue;
            foreach (var def in source.GetAllDefs())
            {
                if (!IsDefinitionValid(def)) continue;
                string name = source.GetFullPath(def.GMID).Replace('.', '/');
                dropdownList.Add(name, def.GMID);
                if(!sourceMap.ContainsKey(def.GMID)) sourceMap[def.GMID] = source;
            }
        }

        var selector = new GenericSelector<ValueDropdownItem<uint>>(
            "Select Item", false, item => item.Text, dropdownList
        );

        selector.EnableSingleClickToSelect();
        
        var currentItem = dropdownList.FirstOrDefault(x => x.Value == currentValue);
        if (currentValue != 0 && !EqualityComparer<ValueDropdownItem<uint>>.Default.Equals(currentItem, default))
        {
            selector.SetSelection(currentItem);
        }

        selector.ShowInPopup(rect);
        selector.SelectionConfirmed += (selection) =>
        {
            if (selection != null && selection.Any())
            {
                var newValue = selection.First().Value;
                this.TargetGMID = newValue;
                if (sourceMap.TryGetValue(newValue, out var sourceSO))
                {
                    OnIDSelected(newValue, sourceSO);
                }
                GUIHelper.RequestRepaint();
            }
        };
    }

    
#endif
    // --- 运行时数据获取 (基本保持不变，确保使用 Resolver 即可) ---
    protected abstract TData CreateData();
    
    protected TData EnsureData(GameModel model, uint gmid)
    {
        // 运行时使用 Resolver
        if (Resolver == null)
        {
            Debug.LogError($"[BaseModelReference] Resolver is null! Did you forget to [ModelInject] it? TargetGMID: {TargetGMID}");
            return default;
        }

        var dataRef = model.GetTrackable(gmid);
        if (dataRef.GetValue() == null)
        {
            dataRef.SetValue(CreateData());
        }
        return (TData)dataRef.GetValue();
    }
}