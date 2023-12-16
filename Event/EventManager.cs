using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiFramework.Event
{
    /// <summary>
    /// 基础来自于: https://blog.csdn.net/abcdtty/article/details/13021237
    /// </summary>
    public class EventManager
    {
        public static EventManager Instance => Singleton<EventManager>.Instance;

        public delegate void EventHandler<T>(T e) where T : EventArguments;

        private readonly Dictionary<Type, Delegate> eventDict = new Dictionary<Type, Delegate>();

        public void Register<T>(EventHandler<T> handler) where T : EventArguments
        {
            Type eventType = typeof(T);

            // 反注册handler避免重复注册同一回调
            UnRegister(handler);

            if (eventDict.TryGetValue(eventType, out var @delegate))
            {
                eventDict[eventType] = Delegate.Combine(@delegate, handler);
            }
            else
            {
                eventDict[eventType] = handler;
            }
        }

        public void UnRegister<T>(EventHandler<T> handler) where T : EventArguments
        {
            Type eventType = typeof(T);

            if (eventDict.TryGetValue(eventType, out var @delegate) && @delegate != null)
            {
                eventDict[eventType] = Delegate.Remove(@delegate, handler);
            }
        }

        public void Invoke<T>() where T : EventArguments, new()
        {
            Invoke(EventFactory.Spawn<T>());
        }

        public void Invoke<T>(T e) where T : EventArguments
        {
            Type eventType = typeof(T);

            if (!eventDict.TryGetValue(eventType, out var handler) || handler == null)
                return;

            var invocationList = handler.GetInvocationList();

            for (int i = 0; i < invocationList.Length; i++)
            {
                var eventHandler = invocationList[i] as EventHandler<T>;
                // 捕获异常
                try
                {
                    if (e.bSupportAsync)
                        eventHandler?.BeginInvoke(e, null, null);
                    else
                        eventHandler?.Invoke(e);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }
}
