using Binder;

namespace Dual.Binder
{
    public class LifetimeAggregationContainer : BaseLifecycle, ILifetimeContainer
    {
        private ILifetimeContainer[] containers;
        
        public LifetimeAggregationContainer(params ILifetimeContainer[] containers)
        {
            this.containers = containers;
        }
        
        public void BindLifetime(ILifecycle lifecycleBinder)
        {
            foreach (var container in containers)
            {
                container.BindLifetime(lifecycleBinder);
            }
        }

        public IDual CreateLifetimeBinder(ILifecycle lifecycleBinder)
        {
            var binder = new BindableDual();
            foreach (var container in containers)
            {
                binder.Bind(container.CreateLifetimeBinder(lifecycleBinder));
            }

            return binder;
        }
    }
}