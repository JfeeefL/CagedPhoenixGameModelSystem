using System;
using Sirenix.OdinInspector;

[Serializable]
[InlineProperty] // <--- 关键修改：让子类属性直接内联显示，去除折叠页外壳
public class GlobalServiceResolver : ModelTargetResolver
{
    public string ServiceName;

    public override IGameModelOwner Resolve(object context = null)
    {
        // 伪代码：从服务定位器获取
        // return ServiceLocator.Get(ServiceName) as IGameModelOwner;
        return null; 
    }

    public override IGameModelDefOwner GetDefOwnerForEditor()
    {
        // 编辑器下可能无法预览服务，返回 null 或模拟数据
        return null; 
    }
}