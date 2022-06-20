using UnityEngine.Events;

namespace LLE.Rx
{
    public class MFEvent<T0> : UnityEvent<T0> { }

    public class MFEvent<T0, T1> : UnityEvent<T0, T1> { }

    public class MFEvent<T0, T1, T2> : UnityEvent<T0, T1, T2> { }

    public class MFEvent<T0, T1, T2, T3> : UnityEvent<T0, T1, T2, T3> { }

    public class DispatchedEvent : UnityEvent
    {
        public new void Invoke()
        {
            Dispatcher.Process(() => base.Invoke());
        }
    }

    public class DispatchedEvent<T0> : UnityEvent<T0>
    {
        public new void Invoke(T0 t0)
        {
            Dispatcher.Process(() => base.Invoke(t0));
        }
    }

    public class DispatchedEvent<T0, T1> : UnityEvent<T0, T1>
    {
        public new void Invoke(T0 t0, T1 t1)
        {
            Dispatcher.Process(() => base.Invoke(t0, t1));
        }
    }

    public class DispatchedEvent<T0, T1, T2> : UnityEvent<T0, T1, T2>
    {
        public new void Invoke(T0 t0, T1 t1, T2 t2)
        {
            Dispatcher.Process(() => base.Invoke(t0, t1, t2));
        }
    }

    public class DispatchedEvent<T0, T1, T2, T3> : UnityEvent<T0, T1, T2, T3>
    {
        public new void Invoke(T0 t0, T1 t1, T2 t2, T3 t3)
        {
            Dispatcher.Process(() => base.Invoke(t0, t1, t2, t3));
        }
    }
}