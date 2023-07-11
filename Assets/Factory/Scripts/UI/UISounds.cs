using UnityEngine;
using UnityEngine.EventSystems;

public class UISounds : MonoBehaviour
{
    public bool createEventTrigger = true;

    public EventTrigger eventTrigger;
    public AudioSource audioSource;

    public AudioClip selectClip;
    public AudioClip submitClip;
    public AudioClip cancelClip;

    private void Awake()
    {
        audioSource = audioSource ?? GetComponent<AudioSource>();

        if (createEventTrigger)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }
        else if (eventTrigger == null)
        {
            eventTrigger = GetComponent<EventTrigger>();
        }

        var selectEntry = new EventTrigger.Entry() { eventID = EventTriggerType.Select };
        selectEntry.callback.AddListener(OnSelect);

        var submitEntry = new EventTrigger.Entry() { eventID = EventTriggerType.Submit };
        submitEntry.callback.AddListener(OnSubmit);

        var cancelEntry = new EventTrigger.Entry() { eventID = EventTriggerType.Cancel };
        cancelEntry.callback.AddListener(OnCancel);

        eventTrigger.triggers.Add(selectEntry);
        eventTrigger.triggers.Add(submitEntry);
        eventTrigger.triggers.Add(cancelEntry);
    }

    public void OnSelect(BaseEventData data)
    {
        audioSource.PlayOneShot(selectClip);
    }

    public void OnSubmit(BaseEventData data)
    {
        audioSource.PlayOneShot(submitClip);
    }

    public void OnCancel(BaseEventData data)
    {
        audioSource.PlayOneShot(cancelClip);
    }
}
