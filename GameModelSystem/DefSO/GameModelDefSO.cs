using UnityEngine;

[CreateAssetMenu(menuName = "Framework/Game Model Definition")]
public partial class GameModelDefSO : HierarchicalDefSO<HierarchicalFieldDef>
{
    // 定制创建逻辑
    protected override HierarchicalFieldDef CreateNewItem(uint parentID)
    {
        var item = base.CreateNewItem(parentID);
        item.Name = "New Field";
        return item;
    }

}