using System.Collections.Generic;
using Binder.ModelData;
using UnityEngine;

namespace GameModelSystem.Editor.Aggregators
{
    public class RatioAggregator : IAttributeAggregator
    {
        [SerializeField]
        private NameHash a = NameHash.FromName("a");
        
        [SerializeField]
        private NameHash b  = NameHash.FromName("b");
        
        public object Aggregate(Dictionary<int, object> attributes)
        {
            float x = UniAttribute.Resolve<float>(attributes[a.Hash]);
            float y = UniAttribute.Resolve<float>(attributes[b.Hash]);
            
            if (y == 0) return 0f;
            return x/y;
        }
    }
}