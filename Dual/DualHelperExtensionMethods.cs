using System;
using Dual.Binder;
using UnityEngine;

namespace Dual
{
    public static class DualHelperExtensionMethods 
    {
        public static BindableDual Bind(this BindableDual bindableDual, IDual dualToBind)
        {
            bindableDual.CreateBinderDual(dualToBind).Enable();
            return bindableDual;
        }

        public static BindableDual AddDual(this BindableDual bindableDual, Action onEnable, Action onDisable)
        {
            return bindableDual.Bind(new ActionDual(onEnable, onDisable));
        }

        public static BindableDual AddOnEnable(this BindableDual bindableDual, Action onEnable)
        {
            return bindableDual.Bind(new EnableDual(onEnable));
        }
        
        public static BindableDual AddOnDisable(this BindableDual bindableDual, Action onDisable)
        {
            return bindableDual.Bind(new DisableDual(onDisable));
        }
    }
}