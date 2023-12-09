namespace MiFramework.Event
{
    public class EventArguments
    {
        public object sender;
        public virtual void Clear() { sender = null; }
    }
}
