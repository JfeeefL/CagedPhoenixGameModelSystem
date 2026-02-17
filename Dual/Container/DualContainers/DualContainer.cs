using System.Collections.Generic;

namespace Dual.Container
{
    public abstract class DualContainer<TContainer, TElement> : IDualContainer<TElement> where TContainer : ICollection<TElement>,new()
    {
        protected readonly TContainer Duals = new TContainer();
        protected abstract IDual CreateAdderDualHelper(TElement elementToBind);
        public int Count => Duals.Count;
        
        public ICollection<TElement> GetDuals()
        {
            return Duals;
        }

        public DualNotSpecifiedCompositor<TElement> CreateAdderDual()
        {
            return new DualNotSpecifiedCompositor<TElement>(CreateAdderDualHelper);
        }

        public IDual CreateAdderDual(TElement dualToBind)
        {
            return CreateAdderDualHelper(dualToBind);
        }
    }
}