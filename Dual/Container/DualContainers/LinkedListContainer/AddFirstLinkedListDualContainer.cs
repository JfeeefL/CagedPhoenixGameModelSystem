namespace Dual.Container
{
    public class AddFirstLinkedListDualContainer<T> : LinkedListDualContainer<T>
    {
        protected override IDual CreateAdderDualHelper(T elementToBind)
        {
            return new AddFirstLinkedListAdderDual<T>(Duals, elementToBind);
        }
    }
}