using Binder;

namespace Dual.Binder
{
    public interface ILifetimeContainer : ILifecycle
    {
        void BindLifetime(ILifecycle lifecycleBinder);
        IDual CreateLifetimeBinder(ILifecycle lifecycleBinder);
    }
}