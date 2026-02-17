using System;
using System.Collections.Generic;
using Binder;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Dual.Binder
{
    [Serializable]
    public class LifetimeDictionary : BaseLifecycle, ILifetimeContainer
    {
        [ShowInInspector]
        private Dictionary<Type, ILifecycle> _lifecycles = new Dictionary<Type, ILifecycle>();

        public IReadOnlyDictionary<Type, ILifecycle> Dictionary => _lifecycles;
        
        
        public void BindLifetime(ILifecycle lifecycleBinder)
        {
            var type = lifecycleBinder.GetType();
            _lifecycles[type] = lifecycleBinder;
        }

        public IDual CreateLifetimeBinder(ILifecycle lifecycleBinder)
        {
            var type = lifecycleBinder.GetType();
            return new ActionDual(() =>
            {
                if (!_lifecycles.TryAdd(type, lifecycleBinder))
                {
                    Debug.LogError($"Fail to add type of {type.Name}, such type already in dictionary.");
                }
            }, () =>
            {
                if (!_lifecycles.Remove(type))
                {
                    Debug.LogError($"Fail to remove type of {type.Name}, no such type in dictionary.");
                }
            });
        }

    }
}