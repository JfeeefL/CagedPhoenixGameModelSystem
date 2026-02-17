using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class DynamicFieldReference : BaseModelReference<float>
{
    // Resolver 和 GetDefinitionSources 逻辑已全部移至基类
    // 此类仅需负责业务逻辑
    
    public virtual T GetValue<T>()
    {
        var owner = Resolver?.Resolve();
        if (owner == null) return default;
        return owner.RuntimeModel.GetValue<T>(TargetGMID);
    }

    public virtual void SetValue<T>(T value)
    {
        Resolver?.Resolve()?.RuntimeModel.SetValue(TargetGMID, value);
    }

    protected override float CreateData()
    {
        return 0;
    }
}