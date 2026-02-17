#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Binder
{
    public static class BinderFactory
    {
        private static Type[] _cachedBinderTypes;

        // 获取所有具体的 ILifecycleBinder 实现类
        private static Type[] GetBinderTypes()
        {
            if (_cachedBinderTypes != null) return _cachedBinderTypes;

            _cachedBinderTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ILifecycleBinder).IsAssignableFrom(p) 
                            && !p.IsInterface 
                            && !p.IsAbstract 
                            && p.GetConstructor(Type.EmptyTypes) != null) // 必须有无参构造函数
                .ToArray();

            return _cachedBinderTypes;
        }

        // 创建所有 Binder 的实例
        public static IEnumerable<ILifecycleBinder> CreateAllBinders()
        {
            return GetBinderTypes().Select(t => (ILifecycleBinder)Activator.CreateInstance(t));
        }
    }
}
#endif