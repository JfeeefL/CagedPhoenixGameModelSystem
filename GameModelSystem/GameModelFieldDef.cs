using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class GameModelFieldDef
{
    [HorizontalGroup("H"), LabelWidth(60)]
    public string Name;

    [HorizontalGroup("H", Width = 80), LabelWidth(30), ReadOnly]
    public uint GMID;

    // 虚方法：供子类生成完整路径（如 A.B.C）
    public virtual string GetFullPath() => Name;
}