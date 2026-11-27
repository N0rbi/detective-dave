using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>
    /// The listeners of one event, raised so that a listener that throws is logged and does not stop the others.
    /// The listener array is cached between subscription changes, so raising does not allocate. A listener that subscribes or
    /// unsubscribes while the event is raised takes effect from the next raise.
    /// </summary>
    internal sealed class SafeEvent<T>
    {
        static readonly Action<T>[] None = Array.Empty<Action<T>>();

        Action<T> handlers;
        Action<T>[] cache = None;

        public void Add(Action<T> handler)
        {
            if (handler == null) return;
            handlers += handler;
            cache = null;
        }

        public void Remove(Action<T> handler)
        {
            if (handler == null) return;
            handlers -= handler;
            cache = null;
        }

        public void Raise(T value, Object context)
        {
            var listeners = cache ??= Snapshot(handlers);
            foreach (var listener in listeners)
            {
                try { listener(value); }
                catch (Exception exception) { Debug.LogException(exception, context); }
            }
        }

        static Action<T>[] Snapshot(Action<T> multicast)
        {
            if (multicast == null) return None;
            var invocation = multicast.GetInvocationList();
            var listeners = new Action<T>[invocation.Length];
            for (int index = 0; index < invocation.Length; index++) listeners[index] = (Action<T>)invocation[index];
            return listeners;
        }
    }
}
