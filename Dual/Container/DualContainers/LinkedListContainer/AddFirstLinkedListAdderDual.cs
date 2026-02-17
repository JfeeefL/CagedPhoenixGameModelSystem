using System.Collections.Generic;

namespace Dual.Container
{
    public class AddFirstLinkedListAdderDual<T> : LinkedListAdderDual<T>
    {
        public AddFirstLinkedListAdderDual(LinkedList<T> duals, T elementToAdd) : base(duals, elementToAdd)
        {
        }

        protected override LinkedListNode<T> AddDual(LinkedList<T> linkedList, T elementToAdd)
        {
            return linkedList.AddFirst(elementToAdd);
        }

    }
}