using Dual.Binder;

namespace Dual.Container
{
    public abstract class AdderDual<TContainer,TElement> : BaseDual
    {
        protected readonly TContainer m_duals;
        
        protected readonly TElement ElementToAdd;
        protected AdderDual(TContainer duals, TElement elementToAdd)
        {
            m_duals = duals;
            ElementToAdd = elementToAdd;
        }
    }
}