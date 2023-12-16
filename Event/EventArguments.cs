namespace MiFramework.Event
{
    public class EventArguments
    {
        public object sender;
        public virtual bool bSupportAsync => false;
        public virtual void Clear() { sender = null; }
    }
}
