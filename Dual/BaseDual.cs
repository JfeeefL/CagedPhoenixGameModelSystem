using System.Collections.Generic;
using UnityEngine;

namespace Dual
{
    public abstract class BaseDual : IDual
    {
        public bool IsEnabled { get; private set; }

        public virtual void Enable()
        {
            if (IsEnabled)
            {
                Debug.LogError("Dual is already enabled. You should not enable a dual twice without disabling it");
                return;
            }
            else
            {
                IsEnabled = true;
            }
        }

        public virtual void Disable()
        {
            if (!IsEnabled)
            {
                Debug.LogError("Dual is already disabled. You should not disable a dual twice without enabling it");
                return;
            }
            else
            {
                IsEnabled = false;
            }
        }

        public IDual GetDual()
        {
            return this;
        }
    }
}