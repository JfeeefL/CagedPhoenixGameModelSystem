using System;

namespace Dual
{
    public class ActionDual : BaseDual
    {
        private readonly Action m_onEnable;
        private readonly Action m_onDisable;
        
        public ActionDual(Action onEnable, Action onDisable)
        {
            m_onEnable = onEnable;
            m_onDisable = onDisable;
        }

        public override void Enable()
        {
            base.Enable();
            m_onEnable?.Invoke();
        }

        public override void Disable()
        {
            base.Disable();

            m_onDisable?.Invoke();
        }
    }
}