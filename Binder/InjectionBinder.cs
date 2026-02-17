using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Binder;
using Framework.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace Binder
{
    [Serializable]
    public class InjectionBinder : BaseLifecycleBinder<object>
    {
        [Serializable]
        public class InjectionLog
        {
            public string Type;
            public string ProviderName;
            public string SubID; // 新增日志字段
            public string InjectField;
            public string TargetObject;
        }

        [SerializeField]
        [ListDrawerSettings(IsReadOnly = true, ShowIndexLabels = false)]
        private List<InjectionLog> _logs = new List<InjectionLog>();

        public override void Register() { }

#if UNITY_EDITOR
        
        private struct MethodProviderInfo
        {
            public string ProviderName;
            public string SubID; // 新增：方法 Provider 的第二关键字
            public UnityEngine.Object TargetObject;
            public MethodInfo Method;
        }

        protected override void OnAutoBind(object target, List<object> bindings)
        {
            _logs.Clear();
            if (target == null) return;
            
            var unityRoot = target as UnityEngine.Object;
            var targetType = target.GetType();

            // ---------------------------------------------------------
            // 1. Field Providers (Key 增加 SubID)
            // Key: (Type, Name, SubID) -> Value: Instance
            // ---------------------------------------------------------
            var fieldProviders = new Dictionary<(Type, string, string), object>();
            
            var providerFields = Reflect.GetFieldsWithAttribute<ModelProviderAttribute>(targetType);
            foreach (var (field, attr) in providerFields)
            {
                var value = field.GetValue(target);
                if (value == null) continue;
                
                // 假设 ModelProviderAttribute 有 SubID 字段/属性
                string subId = attr.SubID; 

                var key = (field.FieldType, attr.Name, subId);
                if (!fieldProviders.ContainsKey(key)) fieldProviders.Add(key, value);
            }

            // ---------------------------------------------------------
            // 2. Method Providers (Info 增加 SubID)
            // ---------------------------------------------------------
            var methodProviders = new List<MethodProviderInfo>();
            var methods = Reflect.GetMethodsWithAttribute<ModelProviderAttribute>(targetType);
            if (target is UnityEngine.Object unityTarget)
            {
                foreach (var (method, attr) in methods)
                {
                    methodProviders.Add(new MethodProviderInfo 
                    { 
                        ProviderName = attr.Name, 
                        SubID = attr.SubID, // 获取 SubID
                        TargetObject = unityTarget, 
                        Method = method 
                    });
                }
            }

            // ---------------------------------------------------------
            // 3. Inject (Root)
            // ---------------------------------------------------------
            var injectFields = Reflect.GetFieldsWithAttribute<ModelInjectAttribute>(targetType);
            foreach (var (field, attr) in injectFields)
            {
                var visited = new HashSet<object>(); 
                
                // Root Inject: 检查入口字段本身是否有 [InjectTarget("SubID")]
                var targetAttr = field.GetCustomAttribute<InjectTargetAttribute>();
                string targetSubId = targetAttr?.SubID; // 获取 SubID，如果没有则为 null

                InjectField(target, field, fieldProviders, attr.Name, targetSubId, field.Name, unityRoot);

                var injectRoot = field.GetValue(target);
                if (injectRoot == null) continue;

                InjectRecursive(injectRoot, fieldProviders, methodProviders, attr.Name, field.Name, visited, unityRoot);
            }
            
            if (_logs.Count > 0)
            {
                bindings.Add(new object());
                if (unityRoot != null) EditorUtility.SetDirty(unityRoot);
            }
        }

        private void InjectRecursive(
            object currentObj, 
            Dictionary<(Type, string, string), object> fieldProviders, 
            List<MethodProviderInfo> methodProviders,
            string contextName, 
            string path,
            HashSet<object> visited,
            UnityEngine.Object rootContext)
        {
            if (currentObj == null) return;

            if (visited.Contains(currentObj)) return;
            visited.Add(currentObj);

            if (currentObj is System.Collections.IEnumerable collection && !(currentObj is string))
            {
                int index = 0;
                foreach (var item in collection)
                {
                    if (CanRecurseInto(item))
                    {
                        InjectRecursive(item, fieldProviders, methodProviders, contextName, $"{path}[{index}]", visited, rootContext);
                    }
                    index++;
                }
                return;
            }

            var type = currentObj.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                // 获取 Target 的 SubID
                var targetAttr = field.GetCustomAttribute<InjectTargetAttribute>();
                string targetSubId = targetAttr?.SubID; // 默认为 null

                if (typeof(UnityEventBase).IsAssignableFrom(field.FieldType))
                {
                    // 传入 targetSubId
                    InjectMethodToEvent(currentObj, field, methodProviders, contextName, targetSubId, path);
                }
                else
                {
                    // 传入 targetSubId
                    InjectField(currentObj, field, fieldProviders, contextName, targetSubId, path, rootContext);
                }
                
                if (field.FieldType.IsClass && 
                    !field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) && 
                    !field.FieldType.IsPrimitive && 
                    field.FieldType != typeof(string))
                {
                    var childValue = field.GetValue(currentObj);
                    if (CanRecurseInto(childValue))
                    {
                        InjectRecursive(childValue, fieldProviders, methodProviders, contextName, $"{path}.{field.Name}", visited, rootContext);
                    }
                }
            }
        }

        private void InjectField(
            object currentObj, 
            FieldInfo field, 
            Dictionary<(Type, string, string), object> providers, 
            string contextName, 
            string targetSubId, // 新增参数
            string path,
            UnityEngine.Object rootContext)
        {
            if (IsCollectionType(field.FieldType))
            {
                InjectCollectionAggregation(currentObj, field, providers, contextName, targetSubId, path, rootContext);
                return; 
            }

            foreach (var provider in providers)
            {
                var (providerType, providerName, providerSubId) = provider.Key;
                var providerValue = provider.Value;

                // 1. 名字匹配
                if (!string.Equals(contextName, providerName, StringComparison.Ordinal)) continue;

                // 2. SubID 匹配 (关键：必须严格相等，null == null)
                if (!string.Equals(targetSubId, providerSubId, StringComparison.Ordinal)) continue;
                
                if (field.FieldType.IsAssignableFrom(providerType))
                {
                    field.SetValue(currentObj, providerValue);
                    if (rootContext != null) EditorUtility.SetDirty(rootContext);
                    
                    LogInjection("Field", providerType, $"{providerName}:{providerSubId}", path, field.Name, providerValue);
                    return; 
                }

                if (providerValue is GameObject go && typeof(IGameModelOwner).IsAssignableFrom(field.FieldType))
                {
                    var component = go.GetComponent<IGameModelOwner>();
                    if (component != null)
                    {
                        field.SetValue(currentObj, component);
                        if (rootContext != null) EditorUtility.SetDirty(rootContext);

                        LogInjection("Field (Comp)", typeof(IGameModelOwner), $"{providerName}:{providerSubId}", path, field.Name, component);
                        return;
                    }
                }
            }
        }

        private void InjectCollectionAggregation(
            object currentObj,
            FieldInfo field,
            Dictionary<(Type, string, string), object> providers, 
            string contextName,
            string targetSubId, // 新增参数
            string path,
            UnityEngine.Object rootContext)
        {
            var targetElementType = GetElementType(field.FieldType);
            if (targetElementType == null) return;

            var accumulatedItems = new List<object>();
            bool foundAny = false;

            foreach (var provider in providers)
            {
                var (providerType, providerName, providerSubId) = provider.Key;
                var providerValue = provider.Value;
                if (providerValue == null) continue;

                // 1. 名字匹配
                if (!string.Equals(contextName, providerName, StringComparison.Ordinal)) continue;
                // 2. SubID 匹配
                if (!string.Equals(targetSubId, providerSubId, StringComparison.Ordinal)) continue;

                if (IsCollectionType(providerType))
                {
                    var providerElementType = GetElementType(providerType);
                    if (providerElementType != null && targetElementType.IsAssignableFrom(providerElementType))
                    {
                        foreach (var item in (System.Collections.IEnumerable)providerValue)
                        {
                            accumulatedItems.Add(item);
                        }
                        foundAny = true;
                    }
                }
                else
                {
                    if (targetElementType.IsAssignableFrom(providerType))
                    {
                        accumulatedItems.Add(providerValue);
                        foundAny = true;
                    }
                    else if (providerValue is GameObject go && targetElementType.IsInterface)
                    {
                        var comp = go.GetComponent(targetElementType);
                        if (comp != null)
                        {
                            accumulatedItems.Add(comp);
                            foundAny = true;
                        }
                    }
                }
            }

            if (foundAny)
            {
                object finalCollection = null;
                if (field.FieldType.IsArray)
                {
                    var newArray = Array.CreateInstance(targetElementType, accumulatedItems.Count);
                    for (int i = 0; i < accumulatedItems.Count; i++) newArray.SetValue(accumulatedItems[i], i);
                    finalCollection = newArray;
                }
                else
                {
                    var listType = typeof(List<>).MakeGenericType(targetElementType);
                    var newList = (System.Collections.IList)Activator.CreateInstance(listType);
                    foreach (var item in accumulatedItems) newList.Add(item);
                    finalCollection = newList;
                }

                field.SetValue(currentObj, finalCollection);
                if (rootContext != null) EditorUtility.SetDirty(rootContext);

                LogInjection("Field (Agg)", typeof(object[]), $"{contextName}:{targetSubId}", path, field.Name, $"Merged {accumulatedItems.Count}");
            }
        }

        private void InjectMethodToEvent(
            object currentObj, 
            FieldInfo field, 
            List<MethodProviderInfo> methods, 
            string contextName, 
            string targetSubId, // 新增参数
            string path)
        {
            var eventInstance = field.GetValue(currentObj) as UnityEventBase;
            if (eventInstance == null) return;

            foreach (var provider in methods)
            {
                // 1. 名字匹配
                bool nameMatch = string.Equals(contextName, provider.ProviderName, StringComparison.Ordinal);
                if (!nameMatch) continue;

                // 2. SubID 匹配 (严格匹配)
                if (!string.Equals(targetSubId, provider.SubID, StringComparison.Ordinal)) continue;

                if (IsSignatureCompatible(eventInstance, provider.Method))
                {
                    BindPersistentListener(eventInstance, provider.TargetObject, provider.Method);
                    LogInjection("Event", typeof(UnityEventBase), $"{provider.ProviderName}:{provider.SubID}", path, field.Name, provider.TargetObject.name);
                    break; 
                }
            }
        }

        // --- Helpers ---
        private void LogInjection(string type, Type providerType, string providerName, string path, string fieldName, object value)
        {
            _logs.Add(new InjectionLog 
            { 
                Type = type,
                ProviderName = $"{providerType.Name} ({providerName})", 
                InjectField = $"{path}.{fieldName}",
                TargetObject = value?.ToString() ?? "null"
            });
        }
        
        // ... (其余辅助方法保持不变: IsCollectionType, GetElementType, IsSignatureCompatible, BindPersistentListener, GetDelegate, CanRecurseInto) ...
        
        private bool IsCollectionType(Type type) => typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string);
        
        private Type GetElementType(Type collectionType) 
        {
            if (collectionType.IsArray) return collectionType.GetElementType();
            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(List<>)) return collectionType.GetGenericArguments()[0];
            var i = collectionType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            return i != null ? i.GetGenericArguments()[0] : typeof(object);
        }

        private bool IsSignatureCompatible(UnityEventBase unityEvent, MethodInfo method)
        {
            var methodParams = method.GetParameters();
            var invokeMethod = unityEvent.GetType().GetMethod("Invoke");
            if (invokeMethod == null) return false;
            var eventParams = invokeMethod.GetParameters();
            if (methodParams.Length != eventParams.Length) return false;
            for (int i = 0; i < methodParams.Length; i++)
            {
                if (!methodParams[i].ParameterType.IsAssignableFrom(eventParams[i].ParameterType)) return false;
            }
            return true;
        }

        private void BindPersistentListener(UnityEventBase unityEvent, UnityEngine.Object target, MethodInfo method)
        {
            Delegate actionDelegate = GetDelegate(method, target);
            if (actionDelegate == null) return;
            int count = unityEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (unityEvent.GetPersistentTarget(i) == target && unityEvent.GetPersistentMethodName(i) == method.Name) return;
            }

            if (unityEvent is UnityEvent nonGenericEvent)
            {
                UnityEventTools.AddPersistentListener(nonGenericEvent, (UnityAction)actionDelegate);
            }
            else
            {
                var eventType = unityEvent.GetType();
                if (eventType.IsGenericType)
                {
                    var genericArgs = eventType.GetGenericArguments();
                    var addMethod = typeof(UnityEventTools).GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .FirstOrDefault(m => m.Name == "AddPersistentListener" && m.IsGenericMethod && m.GetGenericArguments().Length == genericArgs.Length);
                    if (addMethod != null)
                    {
                        var genericMethod = addMethod.MakeGenericMethod(genericArgs);
                        genericMethod.Invoke(null, new object[] { unityEvent, actionDelegate });
                    }
                }
            }
            if (target != null) EditorUtility.SetDirty(target);
        }

        private Delegate GetDelegate(MethodInfo method, UnityEngine.Object target)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 0) return Delegate.CreateDelegate(typeof(UnityAction), target, method, false);
            if (parameters.Length == 1)
            {
                var type = typeof(UnityAction<>).MakeGenericType(parameters[0].ParameterType);
                return Delegate.CreateDelegate(type, target, method, false);
            }
            if (parameters.Length == 2)
            {
                var type = typeof(UnityAction<,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType);
                return Delegate.CreateDelegate(type, target, method, false);
            }
            return null;
        }

        private bool CanRecurseInto(object obj)
        {
             if (obj == null) return false;
             var t = obj.GetType();
             if (t.IsValueType || t == typeof(string) || t.IsPrimitive) return false;
             if (typeof(UnityEngine.Object).IsAssignableFrom(t))
                 return t.IsSubclassOf(typeof(MonoBehaviour)) || t.IsSubclassOf(typeof(ScriptableObject));
             return true;
        }
#endif
    }
}