using System;
using Binder;
using GameModelSystem.Setting;
using UnityEngine;
using UnityEngine.Events;

namespace Caged_Phoenix.Scripts.GameModelSystem.Setting
{
    [Serializable]
    public class FloatUniAttributeInitValueRef : UniAttributeInitValueRef<float>
    {
        [SerializeField]
        [HideInInspector]
        [InjectTarget] 
        private UnityEvent<uint, bool, float,float> onValueChange = new ();
        
        protected override UnityEvent<uint, bool, float, float> OnValueChangeEvent => onValueChange;
    }
}