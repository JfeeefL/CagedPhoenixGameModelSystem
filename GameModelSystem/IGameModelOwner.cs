using System.Collections.Generic;

public interface IGameModelDefOwner
{
    // 修改：支持返回多个定义（例如：基础属性定义 + 角色特有定义）
    IEnumerable<GameModelDefBaseSO> GetModelDefs();
}

public interface IGameModelOwner : IGameModelDefOwner
{
    GameModel RuntimeModel { get; }
}