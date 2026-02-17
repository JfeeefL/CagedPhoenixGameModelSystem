using System;
using Event; // 复用 BindType

namespace Dual
{
    // 用于标记任何实现了 IDual 接口的字段
    [AttributeUsage(AttributeTargets.Field)]
    public class BindDualAttribute : Attribute
    {
        public BindType Lifecycle { get; private set; }
        public BindDualAttribute(BindType lifecycle = BindType.WhenEnabled)
        {
            Lifecycle = lifecycle;
        }
    }
}