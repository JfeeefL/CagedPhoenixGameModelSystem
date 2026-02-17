using System;
using Sirenix.OdinInspector;

namespace GameModelSystem.Editor
{
    [Serializable, InlineProperty]
    public struct AttributeId
    {
        // 移除 [CustomValueDrawer]，由外部 Drawer 接管
        [HideLabel] 
        public uint Id;
        
        public static implicit operator uint(AttributeId wrapper) => wrapper.Id;
        public static implicit operator AttributeId(uint id) => new AttributeId { Id = id };
        
        public override string ToString() => Id.ToString();
    }
}