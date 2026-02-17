using System.Collections.Generic;

namespace Dual.Container
{
    public class AddLastLinkedListAdderDual<T> : LinkedListAdderDual<T>
    {
        public AddLastLinkedListAdderDual(LinkedList<T> duals, T elementToAdd) : base(duals, elementToAdd)
        {
        }

        protected override LinkedListNode<T> AddDual(LinkedList<T> linkedList, T elementToAdd)
        {
            return linkedList.AddLast(elementToAdd);
        }

    }
}