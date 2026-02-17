using System;

namespace Caged_Phoenix.Scripts.TrackableReference
{
    public class TrackableReference
    {
        private object reference;

        private bool isInitialized = false;

        public event Action<object> OnInitialized = delegate { };

        public event Action<object,object> OnValueChanged = delegate { };

        public void SetValue(object newReference)
        {
            var oldReference = reference;
            if(newReference == oldReference) return;
            reference = newReference;
            if (!isInitialized && newReference != null)
            {
                OnInitialized.Invoke(newReference);
                isInitialized = true;
            } 
            OnValueChanged.Invoke(oldReference,newReference);
        }

        public object GetValue()
        {
            return reference;
        }
    }
}