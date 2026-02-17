using System;

namespace Binder
{
    // [ModelProvider("ContextName", "SubID")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ModelProviderAttribute : Attribute
    {
        public string Name { get; private set; }
        public string SubID { get; private set; } // 新增：第二关键字

        public ModelProviderAttribute(string name = null, string subId = null)
        {
            Name = name;
            SubID = subId;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class ModelInjectAttribute : Attribute
    {
        public string Name { get; private set; }
        public ModelInjectAttribute(string name = null) => Name = name;
    }

    // [InjectTarget("SubID")]
    // 标记在字段上，表示接受注入，且要求 Provider 的 SubID 必须匹配
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectTargetAttribute : Attribute 
    { 
        public string SubID { get; private set; } // 新增：第二关键字要求

        public InjectTargetAttribute(string subId = null)
        {
            SubID = subId;
        }
    }
}