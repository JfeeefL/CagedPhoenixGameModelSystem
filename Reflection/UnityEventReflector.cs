#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor.Events;
using UnityEngine.Events;

namespace Framework.Reflection
{
    public static class UnityEventReflector
    {
        /// <summary>
        /// 将目标方法绑定到 UnityEvent 对象上（自动处理泛型）
        /// </summary>
        public static void BindPersistentListener(object unityEventInstance, object targetObject, MethodInfo targetMethod, Type eventType)
        {
            if (unityEventInstance == null || targetMethod == null) return;

            // 1. 创建 Delegate
            Delegate actionDelegate;
            if (eventType == null)
            {
                actionDelegate = Delegate.CreateDelegate(typeof(UnityAction), targetObject, targetMethod);
            }
            else
            {
                var actionType = typeof(UnityAction<>).MakeGenericType(eventType);
                actionDelegate = Delegate.CreateDelegate(actionType, targetObject, targetMethod);
            }

            // 2. 调用 UnityEventTools
            var toolsType = typeof(UnityEventTools);
            MethodInfo addMethod = null;
            var addMethodName = "AddPersistentListener";

            if (eventType == null)
            {
                // 无参版本
                addMethod = toolsType.GetMethod(addMethodName, 
                    BindingFlags.Static | BindingFlags.Public, 
                    null, 
                    new[] { typeof(UnityEvent), typeof(UnityAction) }, 
                    null);
                    
                if (addMethod != null)
                    addMethod.Invoke(null, new object[] { unityEventInstance, actionDelegate });
            }
            else
            {
                // 泛型版本
                var methods = toolsType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(m => m.Name == addMethodName && m.IsGenericMethod);

                foreach (var m in methods)
                {
                    var parameters = m.GetParameters();
                    // 简单的特征匹配：参数必须是 (UnityEvent<T>, UnityAction<T>)
                    if (parameters.Length == 2 && parameters[0].ParameterType.Name.StartsWith("UnityEvent"))
                    {
                        addMethod = m.MakeGenericMethod(eventType);
                        break;
                    }
                }

                if (addMethod != null)
                    addMethod.Invoke(null, new object[] { unityEventInstance, actionDelegate });
            }
        }
    }
}
#endif