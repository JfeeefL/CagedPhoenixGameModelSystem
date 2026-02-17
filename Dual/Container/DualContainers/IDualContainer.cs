using System.Collections.Generic;

namespace Dual.Container
{
    public interface IDualContainer<TElement>
    {

        int Count { get; }
        ICollection<TElement> GetDuals();
        DualNotSpecifiedCompositor<TElement> CreateAdderDual();
        IDual CreateAdderDual(TElement dualToBind);
    }
}