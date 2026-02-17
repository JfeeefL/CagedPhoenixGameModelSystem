using System;
using GameModelSystem.Editor;

namespace GameModelSystem.Setting
{
    [Serializable]
    public abstract class UniAttributeInitValueRef<TAttributeContent> : UniAttributeReference<TAttributeContent>
    {
        protected override bool IsDefinitionValid(GameModelFieldDef def)
        {
            return def is UniAttributeDef attributeDef && !attributeDef.aggregatorConfig.IsValid();
        }
    }
}