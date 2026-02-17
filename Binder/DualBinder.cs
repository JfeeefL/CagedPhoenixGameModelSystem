using System;
using System.Collections.Generic;
using System.Reflection;
using Dual;
using Dual.Binder;
using Event;
using Framework.Reflection; // 引用工具箱
using UnityEngine;

namespace Binder
{
    [Serializable]
    public class DualBinder : BaseLifecycleBinder<DualBinder.BindingEntry>
    {
        [Serializable]
        public class BindingEntry
        {
            public string FieldName;
            public BindType Lifecycle;
            [SerializeReference] public IDual Target;
        }

        public override void Register()
        {
            foreach (var entry in _bindings)
            {
                if (entry.Target == null) continue;
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
            // 使用 Reflect 积木获取所有标记了 [BindDual] 的字段
            var fields = Reflect.GetFieldsWithAttribute<BindDualAttribute>(target.GetType());

            foreach (var (field, attr) in fields)
            {
                var fieldType = field.FieldType;
                var fieldValue = field.GetValue(target);

                // --- 情况 1: 单个 IDual 对象 ---
                if (typeof(IDual).IsAssignableFrom(fieldType))
                {
                    // 自动实例化并获取值 (如果为 null 则 new 一个)
                    // 注意：Reflect.GetOrInstantiate 内部会 SetValue 回去
                    var dualInstance = Reflect.GetOrInstantiate<IDual>(target, field);

                    if (dualInstance != null)
                    {
                        bindings.Add(new BindingEntry
                        {
                            FieldName = field.Name,
                            Lifecycle = attr.Lifecycle,
                            Target = dualInstance
                        });
                    }
                }
                // --- 情况 2: 集合类型 (List<IDual>, IDual[], List<TagReference> 等) ---
                else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(fieldType) && fieldValue != null)
                {
                    // 检查集合的元素类型是否实现了 IDual
                    var elementType = GetElementType(fieldType);
                    if (elementType != null && typeof(IDual).IsAssignableFrom(elementType))
                    {
                        var collection = fieldValue as System.Collections.IEnumerable;
                        int index = 0;
                        foreach (var item in collection)
                        {
                            if (item is IDual dualItem)
                            {
                                bindings.Add(new BindingEntry
                                {
                                    // 使用数组索引格式命名，方便调试: "MyTags[0]"
                                    FieldName = $"{field.Name}[{index}]", 
                                    Lifecycle = attr.Lifecycle,
                                    Target = dualItem
                                });
                            }
                            index++;
                        }
                    }
                }
            }
        }

        // --- 辅助方法：获取集合元素类型 ---
        private Type GetElementType(Type collectionType)
        {
            if (collectionType.IsArray)
                return collectionType.GetElementType();

            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(List<>))
                return collectionType.GetGenericArguments()[0];

            return null; // 暂不支持非泛型 List 或其他复杂集合
        }
#endif
    }
}