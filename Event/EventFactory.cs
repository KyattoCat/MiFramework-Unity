using MiFramework.Event;
using System;
using System.Collections.Generic;

public static class EventFactory
{
    private static Dictionary<Type, Queue<EventArguments>> eventPools = new Dictionary<Type, Queue<EventArguments>>();

    public static T Spawn<T>() where T : EventArguments, new()
    {
        Type type = typeof(T);
        if (!eventPools.TryGetValue(type, out Queue<EventArguments> pool))
            pool = eventPools[type] = new Queue<EventArguments>();

        if (pool.Count == 0)
            return new T();
        else
            return pool.Dequeue() as T;
    }

    public static void Release<T>(T arg) where T : EventArguments
    {
        Type type = typeof(T);

        if (!eventPools.TryGetValue(type, out Queue<EventArguments> pool))
            pool = eventPools[type] = new Queue<EventArguments>();

        arg.Clear();
        pool.Enqueue(arg);
    }
}