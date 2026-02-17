using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class TagData 
{
    [SerializeField, ReadOnly]
    private int _count = 0;

    // 纯 C# 事件，轻量高效
    public event Action<bool> OnStatusChanged;

    public int Count => _count;
    public bool IsActive => _count > 0;

    public void Modify(int delta)
    {
        int oldVal = _count;
        int newVal = Mathf.Max(0, oldVal + delta);
        
        if (oldVal == newVal) return;

        _count = newVal;

        if (oldVal == 0 && newVal > 0) OnStatusChanged?.Invoke(true);
        else if (oldVal > 0 && newVal == 0) OnStatusChanged?.Invoke(false);
    }
}