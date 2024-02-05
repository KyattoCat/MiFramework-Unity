using System;

namespace MiFramework.Event
{
    public abstract class EventArguments : IDisposable
    {
        public virtual bool bSupportAsync => false;
        public abstract void Clear();

        public void Dispose()
        {
            EventFactory.Release(this);
        }
    }
}
