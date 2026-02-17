using System.Collections.Generic;

namespace Dual.Container
{
    public abstract class LinkedListAdderDual<TElement> : AdderDual<LinkedList<TElement>,TElement>
    {
        private LinkedListNode<TElement> m_boundNode;

        protected LinkedListAdderDual(LinkedList<TElement> duals, TElement elementToAdd) : base(duals,elementToAdd)
        {
        }

        protected abstract LinkedListNode<TElement> AddDual(LinkedList<TElement> linkedList, TElement elementToAdd);
        public override void Enable()
        {
            base.Enable();
            m_boundNode = AddDual(m_duals,ElementToAdd);
        }

        public override void Disable()
        {
            base.Disable();
            m_duals.Remove(m_boundNode);
            m_boundNode = null;
        }
    }
}