using System;
using System.Collections.Generic;
using Dual.Binder;
using Event;
using Observable;
using Framework.Reflection;
using UnityEngine;

namespace Binder
{
    [Serializable]
    public class PublisherBinder : BaseLifecycleBinder<PublisherBinder.BindingEntry>
    {
        [Serializable]
        public class BindingEntry
        {
            public BindType Lifecycle;
            [SerializeReference] public IObservableProperty Target;
        }

        public override void Register()
        {
            foreach (var entry in _bindings)
            {
                if (entry.Target == null) continue;
                // IObservableProperty 本身就是 IDual
                switch (entry.Lifecycle)
                {
                    case BindType.WhenAwake: _onAwakenDuals.CreateBinderDual(entry.Target).Enable(); break;
                    case BindType.WhenEnabled: _onEnabledDuals.CreateBinderDual(entry.Target).Enable(); break;
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnAutoBind(object target, List<BindingEntry> bindings)
        {
            var fields = Reflect.GetFieldsWithAttribute<BindPublisherAttribute>(target.GetType());

            foreach (var (field, attr) in fields)
            {
                if (!typeof(IObservableProperty).IsAssignableFrom(field.FieldType)) continue;

                var value = Reflect.GetOrInstantiate<IObservableProperty>(target, field);

                if (value != null)
                {
                    bindings.Add(new BindingEntry
                    {
                        Lifecycle = attr.Lifecycle,
                        Target = value
                    });
                }
            }
        }
#endif
    }
}