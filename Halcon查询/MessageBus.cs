using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Halcon查询
{
    #region 强类型的专属“信件”（事件载体定义）

    /// <summary>
    /// 导航翻页事件：主要用于通知 MainWindow 切换不同的自页面
    /// </summary>
    public class NavigationEvent
    {
        public string TargetViewName { get; set; } // 例如 "Search", "PdfView", "Detail"
    }

    /// <summary>
    /// PDF同步翻页事件：专门告诉 Pdf阅读器 跳转到指定页
    /// </summary>
    public class SyncPdfPageEvent
    {
        public int PageIndex { get; set; }
    }

    #endregion

    #region 现代化的弱引用事件总线（Pub/Sub）
    public class WeakEventBus
    {
        private static readonly WeakEventBus _default = new WeakEventBus();
        public static WeakEventBus Default => _default;

        // 核心机制：事件Type为Key，Value为弱引用包裹的委托名单
        private readonly ConcurrentDictionary<Type, List<WeakAction>> _subscribers = new ConcurrentDictionary<Type, List<WeakAction>>();

        /// <summary>
        /// 广播信件（靶向发送，绝不打扰无关人员）
        /// </summary>
        public void Publish<TEvent>(TEvent message)
        {
            var eventType = typeof(TEvent);
            if (_subscribers.TryGetValue(eventType, out var actions))
            {
                lock (actions)
                {
                    // 【核心亮点】每次发件前，自动清理掉那些其实已经被GC回收（未规范解绑）的垃圾订阅者
                    actions.RemoveAll(a => !a.IsAlive);

                    // 派发给所有活着的订阅者
                    foreach (var action in actions.ToList())
                    {
                        action.Execute(message);
                    }
                }
            }
        }

        /// <summary>
        /// 订阅信箱（注册成为特定信件的收件人）
        /// </summary>
        public void Subscribe<TEvent>(Action<TEvent> action)
        {
            var eventType = typeof(TEvent);
            var weakAction = new WeakAction<TEvent>(action);

            _subscribers.AddOrUpdate(eventType,
                new List<WeakAction> { weakAction },
                (key, existingList) =>
                {
                    lock (existingList)
                    {
                        existingList.Add(weakAction);
                    }
                    return existingList;
                });
        }

        /// <summary>
        /// 主动退订（好习惯，虽然有弱引用保底，但主动退订更优雅）
        /// </summary>
        public void Unsubscribe<TEvent>(Action<TEvent> action)
        {
            var eventType = typeof(TEvent);
            if (_subscribers.TryGetValue(eventType, out var actions))
            {
                lock (actions)
                {
                    actions.RemoveAll(a => !a.IsAlive || ((WeakAction<TEvent>)a).Matches(action));
                }
            }
        }
    }
    #endregion

    #region 弱引用执行器底层逻辑（对业务开发透明）
    internal abstract class WeakAction
    {
        public abstract bool IsAlive { get; }
        public abstract void Execute(object parameter);
    }

    internal class WeakAction<T> : WeakAction
    {
        private readonly WeakReference _target;
        private readonly System.Reflection.MethodInfo _method;

        public WeakAction(Action<T> action)
        {
            _target = new WeakReference(action.Target);
            _method = action.Method;
        }

        public override bool IsAlive => _target.IsAlive;

        public override void Execute(object parameter)
        {
            if (_target.IsAlive)
            {
                var target = _target.Target;
                if (target != null || _method.IsStatic)
                {
                    _method.Invoke(target, new[] { parameter });
                }
            }
        }

        public bool Matches(Action<T> action)
        {
            return _target.Target == action.Target && _method == action.Method;
        }
    }
    #endregion
}
