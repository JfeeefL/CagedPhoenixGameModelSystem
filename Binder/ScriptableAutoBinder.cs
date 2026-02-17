using System;
using Behaviour; // 假设 BinderHolder 在这里
using Binder;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

// 1. 新增 ScriptableAutoBinder 基类
// 让 ScriptableObject 也能享受依赖注入和生命周期管理
public abstract class ScriptableAutoBinder : ScriptableObject, IAutoBindable
{
    [SerializeField, HideInInspector]
    private BinderHolder _binderManager = new BinderHolder();

    [ShowInInspector, HideLabel, ShowIf("@isBoundOnValidate==false")]
    private BinderHolder InspectorView => _binderManager;

    [LabelText("是否自动绑定", icon: SdfIconType.Bluetooth)]
    [SerializeField, ShowInInspector, OnValueChanged("OnValidate")]
    private bool isBoundOnValidate = true;

    protected virtual void OnEnable()
    {
        _binderManager.WhenAwaken();
        _binderManager.WhenEnabled();
    }

    protected virtual void OnDisable()
    {
        _binderManager.WhenDisabled();
        _binderManager.WhenDestroyed();
    }
    protected virtual void OnValidate()
    {
#if UNITY_EDITOR
        if (isBoundOnValidate)
        {
            // 注意：SO 的 Context 就是它自己
            _binderManager.AutoBind(this, this);
        }
#endif
    }
    
#if UNITY_EDITOR
    [Button("【全自动绑定】", ButtonSizes.Large), GUIColor(0.2f, 1f, 0.2f), HideInPlayMode, ShowIf("@isBoundOnValidate==false")]
    public void AutoBind(object target, Object unityContext = null)
    {
        OnValidate();
    }
#endif
}