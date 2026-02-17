using System;
using System.Collections.Generic;
using System.Reflection;
using Binder;
using Dual;
using Dual.Binder;
using Event; // 引用 EventListenerAttribute 和 IEvent
using Event.Container;
using Framework.Reflection; // 引用 Reflect 工具箱
using UnityEngine;

namespace Binder
{
    [Serializable]
    public class EventBinder : BaseLifecycleBinder<EventBinder.BindingEntry>
    {
        [Serializable]
        public class BindingEntry
        {
            public BindType Lifecycle;
            public EventListenerConfig ListenerConfig;
            
            [SerializeReference] public EventBindParam ExtraParam;
            [SerializeReference] public ISerializableListener Listener;
        }

        // --- 运行时逻辑 (保持原有业务逻辑) ---
        public override void Register()
        {
            foreach (var entry in _bindings)
            {
                // 这里调用你项目原本的工厂方法创建 Dual
                switch (entry.Lifecycle)
                {
                    case BindType.WhenAwake:
                        CreateWhenAwakeEventBind(entry.Listener, entry.ListenerConfig).Enable();
                        break;
                    case BindType.WhenEnabled:
                        CreateWhenEnabledEventBind(entry.Listener, entry.ListenerConfig).Enable();
                        break;
                }
            }
        }

        // (这些辅助方法保持原样，依赖你的 GlobalEvent 系统)
        private IDual CreateWhenEnabledEventBind(ISerializableListener listener, EventListenerConfig config)
        {
             return _onEnabledDuals.CreateBinderDual(
                GlobalEvent.CreateEventListenerDual(listener.ParameterType(), listener.GetListener, config));
        }
        
        private IDual CreateWhenAwakeEventBind(ISerializableListener listener, EventListenerConfig config)
        {
             return _onAwakenDuals.CreateBinderDual(
                GlobalEvent.CreateEventListenerDual(listener.ParameterType(), listener.GetListener, config));
        }

        // --- 编辑器自动绑定逻辑 ---
#if UNITY_EDITOR
        protected override void OnAutoBind(object target, List<BindingEntry> bindings)
        {
            // 1. 使用积木查找带 [EventBind] 的方法
            var methods = Reflect.GetMethodsWithAttribute<EventBindAttribute>(target.GetType());

            foreach (var (method, attr) in methods)
            {
                // 参数检查：必须只有1个参数，且是 IEvent
                var parameters = method.GetParameters();
                if (parameters.Length != 1) continue;
                
                Type eventType = parameters[0].ParameterType;
                if (!typeof(IEvent).IsAssignableFrom(eventType)) continue;

                // 2. 获取 Listener 类型 (已修复 dynamic 问题)
                var listenerType = GetListenerTypeFromEvent(eventType);
                if (listenerType == null)
                {
                    Debug.LogError($"[EventBinder] 无法找到事件 {eventType.Name} 对应的 Listener。请检查 [EventListener] Attribute。");
                    continue;
                }

                // 3. 实例化 Listener
                var listenerInstance = (ISerializableListener)Activator.CreateInstance(listenerType);
                
                // 4. 使用积木：找到并实例化内部 'listener' 字段 (通常是 UnityEvent<T>)
                var fieldInfo = Reflect.FindFieldRecursive(listenerType, "listener");
                var unityEvent = Reflect.GetOrInstantiate<object>(listenerInstance, fieldInfo);

                // 5. 使用积木：一键绑定
                UnityEventReflector.BindPersistentListener(unityEvent, target, method, eventType);

                // 6. 添加绑定
                bindings.Add(new BindingEntry
                {
                    Lifecycle = attr.Type,
                    ListenerConfig = attr.ListenerConfig,
                    ExtraParam = new EventBindParam(), // 默认参数
                    Listener = listenerInstance
                });
            }
        }

        private Type GetListenerTypeFromEvent(Type eventType)
        {
            // 【修复】移除 dynamic，使用显式转型
            // 确保文件头部有 using Event;
            var attr = eventType.GetCustomAttribute(typeof(EventListenerAttribute)) 
                       as EventListenerAttribute;
            return attr?.ListenerType;
        }
#endif
    }
}