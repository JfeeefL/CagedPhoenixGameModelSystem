using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dual.Container;
using UnityEngine;

namespace Dual.Container
{
    
    public class PrioritizedContainer<TKey, TValue> : ICollection<KeyValuePair<TKey,TValue>> where TKey : IComparable<TKey>
    {
        private readonly Dictionary<TKey, IDualContainer<TValue>> m_dualContainersDic =
            new Dictionary<TKey, IDualContainer<TValue>>();

        private IOrderedEnumerable<TKey> m_orderedKeys;

        private bool IsDirty { get; set; } = true;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (IsDirty)
            {
                m_orderedKeys = m_dualContainersDic.Keys.OrderByDescending(key => key);
                IsDirty = false;
            }
            foreach (var key in m_orderedKeys)
            {
                var container = m_dualContainersDic[key];
                foreach (var value in container.GetDuals())
                {
                    yield return new KeyValuePair<TKey, TValue>(key,value);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IDual CreateAdderDual(KeyValuePair<TKey, TValue> item)
        {
            IDual containerDual = null;

            IDualContainer<TValue> container = null;

            return new ActionDual(OnEnable,OnDisable);
            
            void OnEnable()
            {
                if (!m_dualContainersDic.TryGetValue(item.Key, out container))
                {
                    container = new AddLastLinkedListDualContainer<TValue>();
                    m_dualContainersDic[item.Key] = container;
                    IsDirty = true;
                }

                containerDual = container.CreateAdderDual(item.Value);
                containerDual.Enable();
            }

            void OnDisable()
            {
                if (containerDual != null)
                {
                    if (container == null)
                    {
                        Debug.LogError("Failed to find key matching the key to be removed.");
                        return;
                    }
                    containerDual.Disable();
                    if (container.Count == 0)
                    {
                        if (!m_dualContainersDic.Remove(item.Key))
                        {
                            Debug.LogError("Failed to remove the key");
                        }
                        IsDirty = true;
                    }
                }
                else
                {
                    Debug.LogError("Container dual is null, enabling function may not be called.");
                }
            }
        }
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new System.NotImplementedException();
        }
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            m_dualContainersDic.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }


        public int Count => m_dualContainersDic.Count;
        public bool IsReadOnly => false;
    }
}