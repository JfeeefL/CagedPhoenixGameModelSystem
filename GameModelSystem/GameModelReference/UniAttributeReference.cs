using System;
using Binder;
using Binder.ModelData;
using Dual;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GameModelSystem.Editor
{
    [Serializable]
    // 继承修改后的基类
    public abstract class UniAttributeReference<TAttribute> : BaseModelReference<UniAttributeData<TAttribute>>, IDual
    {
        protected abstract UnityEvent<uint, bool, TAttribute, TAttribute> OnValueChangeEvent { get; }
        
        private UniAttributeData<TAttribute> _runtimeData;
        
        public bool TryGetValue(out TAttribute value)
        {
            // 使用基类的 Resolver
            var owner = Resolver?.Resolve();
            if (owner == null)
            {
                value = default;
                return false;
            }
            
            var data = owner.RuntimeModel.GetValue<UniAttributeData<TAttribute>>(TargetGMID);
            if (data != null && data.IsInitialized)
                
                
            {
                value = data.GetValue();
                return true;
            }
            value = default;
            return false;
        }
        
        [Required]
        [SerializeField]
        [HideInInspector]
        private UniAttributeDefSO cachedDefSo; 
#if UNITY_EDITOR
        protected override void OnIDSelected(uint id, GameModelDefBaseSO source)
        {
            if (source is UniAttributeDefSO uniAttributeDefSo)
            {
                cachedDefSo = uniAttributeDefSo;
            }
        }
#endif
        public void SetValue(TAttribute value)
        {
            var owner = Resolver?.Resolve();
            if (owner == null) return;
            EnsureData(owner.RuntimeModel, TargetGMID).SetValue(value);
        }

        public UniAttributeDef Def => (cachedDefSo?.GetDef(TargetGMID) as UniAttributeDef);

        protected override UniAttributeData<TAttribute> CreateData()
        {
            // 使用基类的 Resolver
            var model = Resolver?.Resolve()?.RuntimeModel;
            
            var attributeData = new UniAttributeData<TAttribute>(
                    new UniAttributeAggregatorController(
                        Def?.aggregatorConfig,
                        model // 传入 Model
                        )
                    );
            return attributeData;
        }

        protected override bool IsDefinitionValid(GameModelFieldDef def)
        {
            return def is UniAttributeDef;
        }
        
        public IDual GetDual() => this;
        
        private void OnValueChangeHandler(bool oldAvailable, TAttribute oldValue, TAttribute newValue)
        {
            OnValueChangeEvent.Invoke(TargetGMID, oldAvailable, oldValue, newValue);
        }

        public void Enable()
        {
            var owner = Resolver?.Resolve();
            if (owner == null) 
            {
                // 仅 Debug 提示，防止刷屏
                // Debug.LogWarning($"[UniAttributeReference] Enable failed: Resolver unresolved. GMID: {TargetGMID}");
                return;
            }

            _runtimeData = EnsureData(owner.RuntimeModel, TargetGMID);
            if (_runtimeData != null)
            {
                if (OnValueChangeEvent.GetPersistentEventCount() != 0)
                {
                    if (_runtimeData.IsInitialized)
                    {
                        OnValueChangeHandler(false, default, _runtimeData.GetValue());
                    }
                    _runtimeData.OnValueChanged += OnValueChangeHandler;
                }
            }
        }

        public void Disable()
        {
            if (_runtimeData != null)
            {
                if (OnValueChangeEvent.GetPersistentEventCount() != 0)
                {
                    _runtimeData.OnValueChanged -= OnValueChangeHandler;
                }
            }
        }
    }
}