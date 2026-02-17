using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Framework.Reflection
{
    public static class Reflect
    {
        private const BindingFlags DefaultFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // --- 查询积木 ---
        /// <summary>
        /// 获取所有带有指定特性的字段
        /// 支持 AllowMultiple = true，如果一个字段有多个特性，会返回多条 (FieldInfo, Attribute) 数据
        /// </summary>
        public static IEnumerable<(FieldInfo Field, TAttr Attr)> GetFieldsWithAttribute<TAttr>(Type type) 
            where TAttr : Attribute
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            return type.GetFields(flags)
                // 关键修改：使用 GetCustomAttributes (复数) 获取所有特性
                // 然后使用 SelectMany 将其展平
                .SelectMany(field => field.GetCustomAttributes<TAttr>(true)
                    .Select(attr => (field, attr)));
        }

        /// <summary>
        /// 获取所有带有指定特性的方法
        /// 支持 AllowMultiple = true
        /// </summary>
        public static IEnumerable<(MethodInfo Method, TAttr Attr)> GetMethodsWithAttribute<TAttr>(Type type) 
            where TAttr : Attribute
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            return type.GetMethods(flags)
                .SelectMany(method => method.GetCustomAttributes<TAttr>(true)
                    .Select(attr => (method, attr)));
        }

        // 获取泛型参数 (例如从 ObservableProperty<int> 中获取 int)
        public static Type GetGenericArgument(Type targetType, Type genericDefinition)
        {
            var current = targetType;
            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == genericDefinition)
                {
                    return current.GetGenericArguments()[0];
                }
                current = current.BaseType;
            }
            return null;
        }

        // --- 操作积木 ---

        // 递归查找字段（用于查找基类中的 private 字段，如 'unityEvent'）
        public static FieldInfo FindFieldRecursive(Type type, string fieldName)
        {
            var t = type;
            while (t != null && t != typeof(object))
            {
                var f = t.GetField(fieldName, DefaultFlags);
                if (f != null) return f;
                t = t.BaseType;
            }
            return null;
        }

        // 安全获取值，如果为 null 且 autoCreate 为 true，则自动实例化并写回
        public static T GetOrInstantiate<T>(object target, FieldInfo field) where T : class
        {
            var value = field.GetValue(target) as T;
            
            if (value == null && !field.FieldType.IsAbstract && !field.FieldType.IsInterface)
            {
                try
                {
                    value = (T)Activator.CreateInstance(field.FieldType);
                    field.SetValue(target, value);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Reflect] 无法自动实例化字段 {field.Name}: {e.Message}");
                }
            }
            return value;
        }
    }
}