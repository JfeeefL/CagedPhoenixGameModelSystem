
using System;
using Binder;
using UnityEngine;
using UnityEngine.Events;

namespace GameModelSystem.Editor
{
    [Serializable]
    public class FloatUniAttribute : UniAttributeReference<float>
    {
        [SerializeField]
        [HideInInspector]
        [InjectTarget] 
        private UnityEvent<uint, bool, float,float> onValueChange = new ();
        
        protected override UnityEvent<uint, bool, float, float> OnValueChangeEvent => onValueChange;
    }
}