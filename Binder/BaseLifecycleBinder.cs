using System;
using System.Collections.Generic;
using Dual.Binder; // 假设你的 BindableDual 在这里
using Sirenix.OdinInspector;
using UnityEngine;

namespace Binder
{

    /// <summary>
    /// 所有 Binder 的泛型基类，处理了列表管理和生命周期分发
    /// </summary>
    [Serializable]
    public abstract class BaseLifecycleBinder<TEntry> : BaseLifecycle, ILifecycleBinder
    {
        // 统一的数据存储
        [SerializeReference]
        protected List<TEntry> _bindings = new List<TEntry>();

        public IReadOnlyList<TEntry> Bindings => _bindings;

        // 统一的运行时容器
        protected BindableDual _onEnabledDuals = new();
        protected BindableDual _onAwakenDuals = new();

        // 必须实现的注册逻辑
        public abstract void Register();

        // --- 生命周期自动分发 ---
        public override void WhenAwaken()
        {
            base.WhenAwaken();
            _onAwakenDuals.Enable();
        }

        public override void WhenEnabled()
        {
            base.WhenEnabled();
            _onEnabledDuals.Enable();
        }

        public override void WhenDisabled()
        {
            base.WhenDisabled();
            _onEnabledDuals.Disable();
        }

        public override void WhenDestroyed()
        {
            base.WhenDestroyed();
            _onAwakenDuals.Disable();
        }

#if UNITY_EDITOR
        // 模版方法：统一的 AutoBind 流程
        public bool AutoBind(object target)
        {
            if (target == null) return false;
            
            _bindings.Clear();
            
            // 调用子类具体逻辑
            OnAutoBind(target, _bindings);

            // 返回是否找到了有效绑定
            return _bindings.Count > 0;
        }

        /// <summary>
        /// 子类实现具体的扫描逻辑，将结果填入 bindings 列表
        /// </summary>
        protected abstract void OnAutoBind(object target, List<TEntry> bindings);
#endif
    }
}