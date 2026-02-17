using System.Collections.Generic;
using Dual.Container;
using UnityEngine;

namespace Dual.Binder
{
    public class BindableDual : BaseDual, IBindable
    {
        private readonly LinkedList<IDual> m_boundDuals = new LinkedList<IDual>();
        
        
        public IEnumerable<IDual> GetDuals()
        {
            return m_boundDuals;
        }

        public int Count => m_boundDuals.Count;
        
        public bool Initialized { get; private set; } = false;

        protected virtual void Init() {}

        public override void Enable()
        {
            if(Initialized == false)
            {
                Init();
                Initialized = true;
            }
            
            base.Enable();

            foreach (var dual in m_boundDuals)
            {
                dual.Enable();
            }

        }
        
        public override void Disable()
        {
            base.Disable();

            foreach (var dual in m_boundDuals)
            {
                dual.Disable();
            }

        }

        public DualNotSpecifiedCompositor<IDual> CreateBinderDual()
        {
            return new DualNotSpecifiedCompositor<IDual>(CreateBinderDual);
        }

        public BinderDual CreateBinderDual(IDual dualToBind)
        {
            var binderDual = new BinderDual(m_boundDuals,this, dualToBind);
            return binderDual;
        }
    }
}