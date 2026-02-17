using System;

namespace Dual
{
    public class DisableDual : BaseDual
    {
        private readonly Action m_onDisable;
        
        public DisableDual(Action onDisable)
        {
            m_onDisable = onDisable;
        }

        public override void Disable()
        {
            base.Disable();
            m_onDisable?.Invoke();
        }
    }
}