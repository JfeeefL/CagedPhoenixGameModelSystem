using System;
using System.Collections.Generic;
using System.Linq;
using Dual.Binder;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Binder
{
    [Serializable]
    public class BinderHolder : BaseLifecycle, IAutoBindable
    {
        // 多态容器
        [SerializeReference, HideInInspector]
        private List<ILifecycleBinder> _activeBinders = new List<ILifecycleBinder>();

        // 调试视图
        [ShowInInspector, LabelText("Active Binders")]
        [ListDrawerSettings(IsReadOnly = true, ShowFoldout = true)]
        private List<ILifecycleBinder> DebugView => _activeBinders;

        // ----------------- 生命周期代理 -----------------

        public override void WhenAwaken() // 对应 Awake
        {
            base.WhenAwaken();
            foreach (var binder in _activeBinders) binder.Register();
            foreach (var binder in _activeBinders) binder.WhenAwaken();
        }

        public override void WhenEnabled() // 对应 OnEnable
        {
            base.WhenEnabled();
            foreach (var binder in _activeBinders) binder.WhenEnabled();
        }

        public override void WhenDisabled() // 对应 OnDisable
        {
            base.WhenDisabled();
            foreach (var binder in _activeBinders) binder.WhenDisabled();
        }

        public override void WhenDestroyed() // 对应 OnDestroy
        {
            base.WhenDestroyed();
            foreach (var binder in _activeBinders) binder.WhenDestroyed();
        }

        // ----------------- 编辑器逻辑 -----------------

#if UNITY_EDITOR
        // 传入 owner 是为了设置 Dirty 和 Undo
        public void AutoBind(object target, UnityEngine.Object unityContext = null)
        {
            if (unityContext) UnityEditor.Undo.RecordObject(unityContext, "Auto Bind Manager");
            
            _activeBinders.Clear();
            var allBinders = BinderFactory.CreateAllBinders();
            

            foreach (var binder in allBinders)
            {
                // 现在的 binder.AutoBind 接受 object target
                if (binder.AutoBind(target))
                {
                    _activeBinders.Add(binder);
                }
            }

            if (unityContext) UnityEditor.EditorUtility.SetDirty(unityContext);
        }
#endif
    }

    public interface IAutoBindable
    {
        
#if UNITY_EDITOR
        void AutoBind(object target, UnityEngine.Object unityContext = null);
#endif
    }
}