using System.Collections.Generic;
using Dual.Container;

namespace Dual.Binder
{
    public class BinderDual : BaseDual
    {
        private readonly LinkedList<IDual> m_dualsLinkedList;
        private readonly List<IDual> m_attachedWhenEnumerating;
        private readonly List<IDual> m_detachedWhenEnumerating;
        private readonly BindableDual m_bindable;
        private readonly IDual m_dualToBind;
        
        private LinkedListNode<IDual> m_boundNode;
        
        public BinderDual(LinkedList<IDual> dualsLinkedList, BindableDual bindable, IDual dualToBind)
        {
            m_dualsLinkedList = dualsLinkedList;
            m_bindable = bindable;
            m_dualToBind = dualToBind;
        }

        public override void Enable()
        {
            base.Enable();
            if (m_bindable.IsEnabled)
            {
                m_dualToBind.Enable();
            }

            m_boundNode = m_dualsLinkedList.AddLast(m_dualToBind);
        }

        public override void Disable()
        {
            base.Disable();
            if (m_bindable.IsEnabled)
            {
                m_dualToBind.Disable();
            }

            m_dualsLinkedList.Remove(m_boundNode);
            m_boundNode = null;
        }
    }
}