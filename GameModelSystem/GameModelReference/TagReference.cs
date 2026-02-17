using System;
using Binder;
using Dual;
using GameModelSystem.Editor;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[Serializable]
public class TagReference : BaseModelReference<TagData>, IDual, ISerializationCallbackReceiver
{
    
    [SerializeField]
    [HideInInspector]
    private TagDefSO _cachedDefSO; 
    
    // 重写筛选逻辑：只显示 TagDefinition
    protected override bool IsDefinitionValid(GameModelFieldDef def)
    {
        return def is TagDef;
    }
#if UNITY_EDITOR
    protected override void OnIDSelected(uint id, GameModelDefBaseSO source)
    {
        if (source is TagDefSO tagSO)
        {
            _cachedDefSO = tagSO;
        }
    }
#endif
    protected override TagData CreateData()
    {
        return new TagData();
    }

    // --- 运行时逻辑 ---
    [InjectTarget]
    [SerializeField]
    [HideInInspector]
    public UnityEvent<uint,bool> OnTagStatusChange = new UnityEvent<uint,bool>();

    private TagData _runtimeData;
    private bool _initialized;

    public void AddTag(int amount = 1) => ModifyRecursively(amount);
    public void RemoveTag(int amount = 1) => ModifyRecursively(-amount);

    private void ModifyRecursively(int delta)
    {
        var owner = Resolver?.Resolve();
        if (owner == null) return;

        EnsureData(owner.RuntimeModel, TargetGMID)?.Modify(delta);

        if (_cachedDefSO != null)
        {
            foreach (var id in _cachedDefSO.GetAncestors(TargetGMID))
            {
                if(id == TargetGMID) continue; 
                EnsureData(owner.RuntimeModel, id)?.Modify(delta);
            }
        }
    }
    
    

    public IDual GetDual()
    {
        return this;
    }
    
    private void OnTagStatusChangedHandler(bool isEnabled)
    {
        OnTagStatusChange.Invoke(TargetGMID, isEnabled);
    }

    public void Enable()
    {
        var owner = Resolver?.Resolve();
        if (owner == null) return;

        _runtimeData = EnsureData(owner.RuntimeModel, TargetGMID);
        if (_runtimeData != null)
        {
            if (OnTagStatusChange.GetPersistentEventCount() != 0)
            {
                _runtimeData.OnStatusChanged += OnTagStatusChangedHandler;
            }
        }
    }

    public void Disable()
    {
        if (_runtimeData != null)
        {
            if (OnTagStatusChange.GetPersistentEventCount() != 0)
            {
                _runtimeData.OnStatusChanged -= OnTagStatusChangedHandler;
            }
        }
    }

    // 2. Self-Healing (Fallback)
    // If the user copy-pastes this reference, or changes the Resolver without re-selecting ID,
    // we try to re-find the SO before serialization to ensure data consistency.
    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        // Only run this logic if we have an ID but no SO, or to validate correctness
        // Note: Doing deep lookup here might be heavy if you have thousands of objects saving at once.
        // Optimization: Only lookup if _cachedDefSO is null or doesn't contain the ID.
        
        if (TargetGMID != 0 && (_cachedDefSO == null || _cachedDefSO.GetDef(TargetGMID) == null))
        {
            // Try to find it from current Resolver
            foreach (var source in GetDefinitionSources())
            {
                if (source is TagDefSO tagSO && tagSO.GetDef(TargetGMID) != null)
                {
                    _cachedDefSO = tagSO;
                    break;
                }
            }
        }
#endif
    }

    public void OnAfterDeserialize() { } // Not needed
}