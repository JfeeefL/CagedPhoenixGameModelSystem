using System;

namespace Dual
{
    public class DualNotSpecifiedCompositor<T>
    {
        private readonly Func<T,IDual> m_binder;
        public DualNotSpecifiedCompositor(Func<T,IDual> binder)
        {
            m_binder = binder;
        }
        public IDual SetDual(T dual)
        {
            return m_binder.Invoke(dual);
        }
    }
}