using System;
using System.Collections.Generic;

namespace Dual.Container
{
    public class PrioritizedDualContainer<TKey, TValue> : DualContainer<PrioritizedContainer<TKey, TValue>,KeyValuePair<TKey,TValue>> where TKey : IComparable<TKey>
    {
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Duals.GetEnumerator();
        protected override IDual CreateAdderDualHelper(KeyValuePair<TKey, TValue> elementToBind)
        {
            return Duals.CreateAdderDual(elementToBind);
        }
    }

}