using UnityEngine;

namespace GameModelSystem.Editor
{
    
    [CreateAssetMenu(menuName = "Framework/Uni-Attribute Def SO", fileName = "UniAttributeDefSO")]
    public class UniAttributeDefSO : HierarchicalDefSO<UniAttributeDef>
    {
        protected override UniAttributeDef CreateNewItem(uint parentID)
        {
            var item = base.CreateNewItem(parentID);
            item.Name = "New Attribute";
            return item;
        }
    }
}