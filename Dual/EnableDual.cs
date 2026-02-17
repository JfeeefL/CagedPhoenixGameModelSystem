using System;

namespace Dual
{
    public class EnableDual : BaseDual
    {
        private readonly Action m_onEnable;
        
        public EnableDual(Action onEnable)
        {
            m_onEnable = onEnable;
        }

        public override void Enable()
        {
            base.Enable();
            m_onEnable?.Invoke();
        }
    }
}