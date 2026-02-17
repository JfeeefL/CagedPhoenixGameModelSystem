using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
[InlineProperty] // <--- 关键修改：让子类属性直接内联显示，去除折叠页外壳
[LabelWidth(100)]
public class DirectReferenceResolver : ModelTargetResolver
{
    [Required]
    public UnityEngine.Object TargetObject; // 可以是 GameObject 或 ScriptableObject

    public override IGameModelOwner Resolve(object context = null)
    {
        if (TargetObject is GameObject go) return go.GetComponent<IGameModelOwner>();
        return TargetObject as IGameModelOwner;
    }

    public override IGameModelDefOwner GetDefOwnerForEditor()
    {
        if (TargetObject is GameObject go) return go.GetComponent<IGameModelDefOwner>();
        return TargetObject as IGameModelDefOwner;
    }
}