using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventsManager : MonoBehaviour
{
    private Dictionary<Type, UnityEvent<IEvent>> subscribers;
    private Dictionary<object, UnityAction<IEvent>> castedActions;

    private static EventsManager instance;

    public static EventsManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(EventsManager)) as EventsManager;

                if (!instance)
                {
                    Logger.LogError("EventsManager instance not found");
                }
                else
                {
                    instance.Init();
                }
            }

            return instance;
        }
    }

    void Init()
    {
        if (subscribers == null)
        {
            subscribers = new Dictionary<Type, UnityEvent<IEvent>>();
        }

        if (castedActions == null)
        {
            castedActions = new Dictionary<object, UnityAction<IEvent>>();
        }
    }

    public static void Subscribe<T>(UnityAction<T> call)
        where T : IEvent
    {
        UnityEvent<IEvent> unityEvent = null;
        if (Instance.subscribers.TryGetValue(typeof(T), out unityEvent))
        {
            UnityAction<IEvent> casted = (evt) => call((T)evt);
            Instance.castedActions.Add(call, casted);
            unityEvent.AddListener(casted);
        }
        else
        {
            unityEvent = new UnityEvent<IEvent>();
            UnityAction<IEvent> casted = (evt) => call((T)evt);
            Instance.castedActions.Add(call, casted);
            unityEvent.AddListener(casted);
            Instance.subscribers.Add(typeof(T), unityEvent);
        }
    }

    public static void Unsubscribe<T>(UnityAction<T> call)
        where T : IEvent
    {
        if (instance == null)
        {
            return;
        }
        UnityAction<IEvent> casted = null;
        if (Instance.castedActions.TryGetValue(call, out casted))
        {
            UnityEvent<IEvent> unityEvent = null;
            if (Instance.subscribers.TryGetValue(typeof(T), out unityEvent))
            {
                unityEvent.RemoveListener(casted);
                Instance.castedActions.Remove(call);
            }
        }
    }

    public static void Publish<T>(T evt)
        where T: IEvent
    {
        UnityEvent<IEvent> unityEvent = null;
        if (Instance.subscribers.TryGetValue(typeof(T), out unityEvent))
        {
            if (unityEvent == null)
            {
                Logger.LogError("UnityEvent is null");
                return;
            }
            unityEvent.Invoke(evt);
        }
    }
}
