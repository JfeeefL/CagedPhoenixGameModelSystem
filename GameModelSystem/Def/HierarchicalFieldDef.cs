using System;
using Sirenix.OdinInspector;

[Serializable]
public class HierarchicalFieldDef : HierarchyDefItem
{
    [Title("Configuration")]
    [InfoBox("This is a detailed inspector for the selected node.")]
    public bool IsActive = true;

    [BoxGroup("Attribute Settings")]
    [LabelWidth(100)]
    public float MinValue = 0;

    [BoxGroup("Attribute Settings")]
    [LabelWidth(100)]
    public float MaxValue = 100;

    [BoxGroup("Attribute Settings")]
    [ProgressBar("MinValue", "MaxValue")]
    public float DefaultValue = 50;
}