using System;
using System.Collections.Generic;
using Caged_Phoenix.Scripts.TrackableReference;
using Sirenix.OdinInspector;
using UnityEngine;

// GameModel 不需要引用 Editor 命名空间了，保持干净
[Serializable]
public class GameModel : ISerializationCallbackReceiver
{
    [Serializable]
    public struct Entry
    {
        // --- 核心修改 ---
        // 使用 @ 调用静态类方法，并传入 $property
        [ValueDropdown("@GameModelEditorHelper.GetAvailableIds($property)")] 
        public uint GMID;

        [SerializeReference]
        public IModelValue Value;
    }

    [SerializeField]
    [ListDrawerSettings(ShowIndexLabels = false, CustomAddFunction = "AddNewEntry")]
    private List<Entry> _entries = new List<Entry>();

    private Dictionary<uint, TrackableReference> _runtimeDict = new Dictionary<uint, TrackableReference>();

    // ... (其余 Runtime 代码保持不变) ...

    public void SetValue<T>(uint gmid, T value)
    {
        if (!_runtimeDict.ContainsKey(gmid))
        {
            _runtimeDict[gmid] = new TrackableReference();
        }
        _runtimeDict[gmid].SetValue(value);
    }

    public T GetValue<T>(uint gmid, T defaultValue = default)
    {
        if (_runtimeDict.TryGetValue(gmid, out TrackableReference trackable))
        {
            var val = trackable.GetValue();
            if (val is IModelValue wrapper) val = wrapper.GetValue();
            if (val is T tVal) return tVal;
            try { return (T)Convert.ChangeType(val, typeof(T)); } catch { }
        }
        return defaultValue;
    }
    
    public TrackableReference GetTrackable(uint gmid)
    {
        if (!_runtimeDict.TryGetValue(gmid, out TrackableReference trackable))
        {
            trackable = _runtimeDict[gmid] = new TrackableReference();
        }
        return trackable;
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        _runtimeDict.Clear();
        if (_entries == null) return;
        foreach (var entry in _entries)
        {
            if (entry.GMID == 0 || entry.Value == null) continue;
            var trackable = _runtimeDict[entry.GMID] = new TrackableReference();
            trackable.SetValue(entry.Value.GetValue());
        }
    }

    // ... (移除了原本的 #if UNITY_EDITOR 代码块，因为移动到了 Helper 中) ...
    
#if UNITY_EDITOR
    private void AddNewEntry()
    {
        _entries.Add(new Entry());
    }
#endif
}