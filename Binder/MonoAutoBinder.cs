using System;
using Behaviour;
using Binder;
using Dual.Binder;
using KBCore.Refs;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class MonoAutoBinder : MonoBindable
{
    [SerializeField, HideInInspector]
    private BinderHolder _binderManager = new BinderHolder();
    
    // 这个属性是为了让 Odin 画出 Manager 的 Debug 列表
    [ShowInInspector, HideLabel, ShowIf("@isBoundOnValidate==false")]
    private BinderHolder InspectorView => _binderManager;

    [LabelText("是否自动绑定",icon:SdfIconType.Bluetooth), HideInPlayMode]
    [SerializeField, ShowInInspector, OnValueChanged("WhenIsBoundOnValidateValueChanged")]
    private bool isBoundOnValidate = true;
    
    [LabelText("是否绑定场景引用",icon:SdfIconType.Search), HideInPlayMode]
    [SerializeField, ShowInInspector, OnValueChanged("WhenIsRefBoundOnValidateValueChanged")]
    private bool isRefBoundOnValidate = false;
    private void WhenIsBoundOnValidateValueChanged()
    {
        if(isBoundOnValidate)
            OnValidate();
    }
    
    
    private void WhenIsRefBoundOnValidateValueChanged()
    {
        if(isRefBoundOnValidate)
            OnValidate();
    }

    protected override void Init()
    {
        binder.BindLifetime(_binderManager);
    }

    protected virtual void OnValidate()
    {
        if (isRefBoundOnValidate)
        {
            this.ValidateRefs();
        }
        
#if UNITY_EDITOR
        if (isBoundOnValidate)
        {
            AutoBindAll();
        }
#endif
    }

#if UNITY_EDITOR
    [Button("【全自动绑定】", ButtonSizes.Large), GUIColor(0.2f, 1f, 0.2f), HideInPlayMode, ShowIf("@isBoundOnValidate==false")]
    protected void AutoBindAll()
    {
        _binderManager.AutoBind(this, this);
    }
#endif
}