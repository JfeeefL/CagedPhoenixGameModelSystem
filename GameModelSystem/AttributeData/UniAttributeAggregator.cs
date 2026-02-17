using System;
using System.Collections.Generic;
using System.Linq;
using Binder.ModelData;
using Caged_Phoenix.Scripts.TrackableReference;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace GameModelSystem.Editor
{
    public class UniAttributeAggregatorController
    {
        private readonly UniAttributeAggregatorConfig _config;
        
        private Dictionary<int, object> _cachedAttributes;
        
        private event Action<object> OnValueChanged; 
        
        private int _callbackCount = 0;
        
        private object _cachedValue;
        
        public UniAttributeAggregatorController(UniAttributeAggregatorConfig config, GameModel gameModel)
        {
            _config = config;
            SetupAttributeCache(gameModel);
        }
        
        
        public void RegisterOnValueChanged(Action<object> callback)
        {
            _callbackCount++;
            OnValueChanged += callback;
            if(!isAggregationInitialized)
                RunAggregation();
            else if(_cachedValue != null)
            {
                callback(_cachedValue);
            }
        }
        
        public void UnregisterOnValueChanged(Action<object> callback)
        {
            _callbackCount--;
            OnValueChanged -= callback;
        }
        
        private int uninitializedCount = 0;
            
        public void SetupAttributeCache(GameModel model)
        {
            if(_cachedAttributes == null)
            {
                _cachedAttributes = new Dictionary<int, object>();
                foreach(var pair in _config.Map)
                {
                    var trackable = model.GetTrackable(pair.Value);
                    if(trackable.GetValue() == null)
                    {
                        uninitializedCount++;
                        trackable.OnInitialized += (attribute) =>
                        {
                            _cachedAttributes[pair.Key.Hash] = attribute;
                            if (attribute is IUniAttributeData data) {
                                data.OnValueChangedNonParam += RunAggregation;
                            }
                            uninitializedCount--;
                            if (uninitializedCount == 0)
                            {
                                RunAggregation();
                            }
                        };
                    }
                    else
                    {
                        _cachedAttributes[pair.Key.Hash] = trackable.GetValue();
                        if (trackable.GetValue() is IUniAttributeData data) {
                            data.OnValueChangedNonParam += RunAggregation;
                        }
                    }
                }
            }
        }
        
        private bool isAggregationInitialized = false;
        
        private void RunAggregation()
        {
            if(_callbackCount == 0) return;
            //Check cached attributes
            if(uninitializedCount > 0)
            {
                return ;
            }
            
            if(_config._attributeAggregator != null)
            { 
                _cachedValue = _config._attributeAggregator.Aggregate(_cachedAttributes);
                isAggregationInitialized = true;
                OnValueChanged?.Invoke(_cachedValue);
            }
        }
    }

    public interface IAttributeAggregator
    {
        public object Aggregate(Dictionary<int, object> attributes);
    }
}