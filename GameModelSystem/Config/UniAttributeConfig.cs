using System;
using Dual;
using GameModelSystem.Editor;
using GameModelSystem.Setting;
using UnityEngine;

namespace Binder.Setting
{
    
    [Serializable]
    public abstract class UniAttributeConfig<TAttributeContent> : IUniAttributeConfig
    {
        protected abstract UniAttributeInitValueRef<TAttributeContent> Reference { get; }

        [SerializeField] private TAttributeContent Value;
        
        public bool TryGetValue(out TAttributeContent result)
        {
            return Reference.TryGetValue(out result);
        }
        
        public void SetValue(TAttributeContent value)
        {
            Reference.SetValue(value);
        }

        public IDual GetDual()
        {
            return this;
        }

        public void Enable()
        {
            Reference.Enable();
            Reference.SetValue(Value);
        }

        public void Disable()
        {
            Reference.Disable();
        }
    }
    
    public interface IUniAttributeConfig : IDual {}
}