using Binder;

namespace Dual.Binder
{
    public class LifetimeContainer : BaseLifecycle, ILifetimeContainer
    {
        private BindableDual whenAlive = new();
        private BindableDual whenEnabled = new();

        private IDual enablingBinder;

        private bool IsInitialized { get; set; } = false;

        protected virtual void Init()
        {
            
        }

        public void BindLifetime(ILifecycle lifecycleBinder)
        {
            whenAlive.Bind(new ActionDual(lifecycleBinder.WhenAwaken, lifecycleBinder.WhenDestroyed));
            whenEnabled.Bind(new ActionDual(lifecycleBinder.WhenEnabled, lifecycleBinder.WhenDisabled));
        }

        public IDual CreateLifetimeBinder(ILifecycle lifecycleBinder)
        {
            return new BindableDual()
                .Bind(whenAlive.CreateBinderDual(new ActionDual(lifecycleBinder.WhenAwaken,
                    lifecycleBinder.WhenDestroyed)))
                .Bind(whenEnabled.CreateBinderDual(new ActionDual(lifecycleBinder.WhenEnabled,
                    lifecycleBinder.WhenDisabled)));
        }

        
        public override void WhenAwaken()
        {
            base.WhenAwaken();
            if (!IsInitialized)
            {
                Init();
                IsInitialized = true;
            }
            
            whenAlive.Enable();

            if (enablingBinder == null)
            {
                enablingBinder = whenAlive.CreateBinderDual(whenEnabled);
            }
        }

        public override void WhenEnabled()
        {
            base.WhenEnabled();
            if (enablingBinder == null)
            {
                enablingBinder = whenAlive.CreateBinderDual(whenEnabled);
            }
            enablingBinder.Enable();
        }

        public override void WhenDisabled()
        {
            base.WhenDisabled();
            if(enablingBinder == null) return;
            enablingBinder.Disable();
        }

        public override void WhenDestroyed()
        {
            base.WhenDestroyed();
            whenAlive.Disable();
        }
    }
}