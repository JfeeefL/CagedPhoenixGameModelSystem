using System;
using GameModelSystem.Editor;
using UnityEngine;

namespace Binder.ModelData
{
    [Serializable]
    public class UniAttributeData<T> : IUniAttributeData
    {
        private T _value;
        
        private bool isInitialized = false;
        
        public bool IsInitialized => isInitialized;
        
        public event Action<bool,T,T> OnValueChanged;

        public event Action OnValueChangedNonParam;
        
        private UniAttributeAggregatorController _controller;
        
        public UniAttributeAggregatorController Controller => _controller;
        
        public UniAttributeData(UniAttributeAggregatorController controller, T initialValue = default)
        {
            _controller = controller;
            _value = initialValue;
            controller.RegisterOnValueChanged(obj=>SetValue((T)obj));
        }
        
        public T GetValue()
        {
            return _value;
        }
        
        public void SetValue(T newValue)
        {
            if (!isInitialized || !Equals(_value, newValue))
            {
                T oldValue = _value;
                _value = newValue;
                OnValueChanged?.Invoke(isInitialized,oldValue, newValue);
                OnValueChangedNonParam?.Invoke();
                isInitialized = true;
            }
        }
    }

    public interface IUniAttributeData
    {
        public event Action OnValueChangedNonParam;
    }
}