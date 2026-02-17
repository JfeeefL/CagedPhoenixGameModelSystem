using System;
using UnityEngine;

// 标记接口，用于 SerializeReference
public interface IModelValue 
{
    object GetValue();
}

[Serializable]
public class MInt : IModelValue
{
    public int Value;
    public object GetValue() => Value;
    public override string ToString() => Value.ToString();
}

[Serializable]
public class MFloat : IModelValue
{
    public float Value;
    public object GetValue() => Value;
    public override string ToString() => Value.ToString();
}

[Serializable]
public class MString : IModelValue
{
    public string Value;
    public object GetValue() => Value;
    public override string ToString() => Value;
}

// 如果你需要直接存 UnityObject
[Serializable]
public class MObject : IModelValue
{
    public UnityEngine.Object Value;
    public object GetValue() => Value;
}