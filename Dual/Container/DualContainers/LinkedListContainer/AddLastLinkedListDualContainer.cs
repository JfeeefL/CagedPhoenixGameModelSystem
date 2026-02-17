namespace Dual.Container
{
    public class AddLastLinkedListDualContainer<T> : LinkedListDualContainer<T>
    {
        protected override IDual CreateAdderDualHelper(T elementToBind)
        {
            return new AddLastLinkedListAdderDual<T>(Duals, elementToBind);
        }
    }
}