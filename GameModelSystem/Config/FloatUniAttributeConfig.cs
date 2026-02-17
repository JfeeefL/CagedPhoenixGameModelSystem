using System;
using Binder.Setting;
using GameModelSystem.Setting;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Caged_Phoenix.Scripts.GameModelSystem.Setting
{
    [Serializable]
    public class FloatUniAttributeConfig : UniAttributeConfig<float>
    {
        protected override UniAttributeInitValueRef<float> Reference => reference;
        
        [SerializeField]
        [InlineProperty]
        [HideLabel]
        [PropertyOrder(-1)]
        private FloatUniAttributeInitValueRef reference = new FloatUniAttributeInitValueRef();
    }
}