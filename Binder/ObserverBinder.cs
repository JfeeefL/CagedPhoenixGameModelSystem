using System;
using System.Collections.Generic;
using System.Reflection;
using Binder; // 引用 BaseLifecycleBinder
using Dual.Binder;
using Event;
using Framework.Reflection; // 引用 Reflect 工具箱
using Observable; // 引用 ObservableImplementationAttribute
using UnityEngine;

namespace Binder
{
    [Serializable]
    public class ObserverBinder : BaseLifecycleBinder<ObserverBinder.BindingEntry>
    {
        [Serializable]
        public class BindingEntry
        {
            public BindType Lifecycle;
            [SerializeReference] public ICanRegisterObserver Observer;
        }

        // --- 运行时逻辑 ---
        public override void Register()
        {
            foreach (var entry in _bindings)
            {
                if (entry.Observer == null) continue;
                
                // 具体的注册逻辑
                var dual = entry.Observer.CreateRegisterDual();
                
                switch (entry.Lifecycle)
                {
                    case BindType.WhenAwake: 
                        _onAwakenDuals.CreateBinderDual(dual).Enable(); 
                        break;
                    case BindType.WhenEnabled: 
                        _onEnabledDuals.CreateBinderDual(dual).Enable(); 
                        break;
                }
            }
        }

        // --- 编辑器自动绑定逻辑 ---
#if UNITY_EDITOR
        protected override void OnAutoBind(object target, List<BindingEntry> bindings)
        {
            // 1. 使用积木查找带 [BindObservable] 的方法
            var methods = Reflect.GetMethodsWithAttribute<BindObservableAttribute>(target.GetType());

            foreach (var (method, attr) in methods)
            {
                // 参数检查：必须只有1个参数
                if (method.GetParameters().Length != 1) continue;
                
                var paramType = method.GetParameters()[0].ParameterType;
                var observableType = attr.TargetObservableType;

                // 2. 使用积木提取泛型参数 T (从 ObservableProperty<T> 中)
                var valueType = Reflect.GetGenericArgument(observableType, typeof(ObservableProperty<>));
                
                // 类型匹配检查
                if (valueType == null || !valueType.IsAssignableFrom(paramType))
                {
                    Debug.LogWarning($"[ObserverBinder] 方法 {method.Name} 参数类型不匹配，跳过。");
                    continue;
                }

                // 3. 获取 Observer 实现类型 (已修复 dynamic 问题)
                var observerImplType = GetObserverTypeFromObservable(observableType);
                if (observerImplType == null)
                {
                    Debug.LogError($"[ObserverBinder] 无法找到 {observableType.Name} 对应的 Observer 实现。请检查 [ObservableImplementation] Attribute。");
                    continue;
                }

                // 4. 实例化 Observer
                var observerInstance = (ICanRegisterObserver)Activator.CreateInstance(observerImplType);

                // 5. 使用积木：找到并实例化内部 'unityEvent' 字段
                // (支持递归查找基类 private 字段)
                var eventField = Reflect.FindFieldRecursive(observerImplType, "unityEvent");
                var unityEvent = Reflect.GetOrInstantiate<object>(observerInstance, eventField);

                // 6. 使用积木：一键绑定 Method 到 UnityEvent
                UnityEventReflector.BindPersistentListener(unityEvent, target, method, paramType);

                // 7. 添加到列表
                bindings.Add(new BindingEntry 
                { 
                    Lifecycle = attr.Type, 
                    Observer = observerInstance 
                });
            }
        }

        private Type GetObserverTypeFromObservable(Type observableType)
        {
            // 【修复】移除 dynamic，使用显式转型
            // 确保文件头部有 using Observable; 
            var attr = observableType.GetCustomAttribute(typeof(ObservableImplementationAttribute)) 
                       as ObservableImplementationAttribute;
            return attr?.ObserverType;
        }
#endif
    }
}