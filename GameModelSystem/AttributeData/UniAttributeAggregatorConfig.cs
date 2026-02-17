using System;
using UnityEngine;

namespace GameModelSystem.Editor
{
    [Serializable]
    public class UniAttributeAggregatorConfig
    {
        [SerializeField]
        public UniAttributeMap Map = new();
        
        [SerializeReference]
        public IAttributeAggregator _attributeAggregator;
        
        public bool IsValid()
        {
            return _attributeAggregator != null;
        }
    }
}