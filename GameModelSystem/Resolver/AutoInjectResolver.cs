using System;
using Binder;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
[InlineProperty] // 必须：移除折叠外壳，让内容直接嵌入
public class AutoInjectResolver : ModelTargetResolver
{
    // 放在一个默认折叠的组里，标题为 "Debug Info"
    // 这样在默认情况下，你只会看到 Header（我们在基类画的）和一个闭合的 Foldout
    //[FoldoutGroup("Debug Info", expanded: false)]
    [ReadOnly, HideInInspector] 
    [InjectTarget] 
    [SerializeField]
    [HideLabel] // 在折叠组内省空间
    private UnityEngine.Object _injectedTarget;

    public override IGameModelOwner Resolve(object context = null)
    {
        if (_injectedTarget == null)
        {
            return null;
        }
        if (_injectedTarget is GameObject go) return go.GetComponent<IGameModelOwner>();
        return _injectedTarget as IGameModelOwner;
    }

    public override IGameModelDefOwner GetDefOwnerForEditor()
    {
        if(_injectedTarget == null) return null;
        if (_injectedTarget is GameObject go) return go.GetComponent<IGameModelDefOwner>();
        return _injectedTarget as IGameModelDefOwner;
    }
}