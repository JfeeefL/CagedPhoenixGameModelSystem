using UnityEngine;

namespace Binder
{
    // 所有 Binder 必须实现的接口
    public interface ILifecycleBinder : ILifecycle
    {
        void Register();
        // 编辑器：自动绑定
        // 返回 bool 表示是否有绑定内容 (用于按需添加)
#if UNITY_EDITOR
        bool AutoBind(object target);
#endif
    }

    public interface ILifecycle
    {
        bool IsAlive { get; }
        bool IsEnabled { get; }

        // 运行时生命周期
        void WhenAwaken();
        void WhenEnabled();
        void WhenDisabled();
        void WhenDestroyed();
    }

    public class BaseLifecycle : ILifecycle
    {
        public bool IsAlive { get; private set; }
        public bool IsEnabled { get; private set; }
        public virtual void WhenAwaken()
        {
            if (IsAlive == true)
            {
                Debug.LogError("Can not awake an alive component.");
            }
            IsAlive = true;
        }

        public virtual void WhenEnabled()
        {
            if (IsEnabled)
            {
                Debug.LogError("Can not enable a enabled component.");
            }
            IsEnabled = true;
        }

        public virtual void WhenDisabled()
        {
            if (!IsEnabled)
            {
                Debug.LogError("Can not disable a disabled component.");
            }
            IsEnabled = false;
        }

        public virtual void WhenDestroyed()
        {
            if (IsAlive == false)
            {
                Debug.LogError("Can not destroy a destroyed component.");
            }
            IsEnabled = false;
            IsAlive = false;
        }
    }
}