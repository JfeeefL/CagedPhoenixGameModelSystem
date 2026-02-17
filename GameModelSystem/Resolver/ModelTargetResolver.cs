using System;

// 策略基类
[Serializable]
public abstract class ModelTargetResolver
{
    // 核心方法：在运行时找到具体的 GameModelOwner
    public abstract IGameModelOwner Resolve(object context = null);
    
    // 编辑器方法：获取定义，用于 Inspector 下拉列表
    public abstract IGameModelDefOwner GetDefOwnerForEditor();
}
