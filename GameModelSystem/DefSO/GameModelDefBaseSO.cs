using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

// 抽象基类：只定义接口，不定义存储方式
public abstract class GameModelDefBaseSO : ScriptableObject
{
    // 供外部获取定义的统一接口
    public abstract GameModelFieldDef GetDef(uint gmid);
    public abstract IEnumerable<GameModelFieldDef> GetAllDefs();
    
    // 供 DynamicFieldReference 生成下拉列表时的辅助（可选）
    public virtual string GetFullPath(uint gmid)
    {
        var def = GetDef(gmid);
        return def != null ? def.Name : "Unknown";
    }
}