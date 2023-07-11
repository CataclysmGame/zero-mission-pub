using System.Collections.Generic;
using UnityEngine;

public class PersistentMusicPlayer : MonoBehaviour
{
    public AudioSource musicAudioSource;
    public AudioSource soundAudioSource;

    public AudioClip[] startingQueue;

    public static PersistentMusicPlayer Instance { get; private set; }

    struct QueueElement
    {
        public AudioClip[] clips;
        public bool loop;
        public int maxPlays;
        public int playsCount;
    }

    private QueueElement? curElement;

    private Queue<QueueElement> musicQueue;

    private void Awake()
    {
        musicQueue = new Queue<QueueElement>();

        if (startingQueue != null)
        {
            foreach (var clip in startingQueue)
            {
                var el = new QueueElement();
                el.clips = new AudioClip[] { clip };
                musicQueue.Enqueue(el);
            }
        }

        DontDestroyOnLoad(transform.gameObject);

        Instance = this;
    }
    
    public bool MusicPlaying { get => musicAudioSource.isPlaying; }

    public void PlaySoundOneShot(AudioClip clip, float volumeScale = 1.0f)
    {
        soundAudioSource.PlayOneShot(clip, volumeScale);
    }

    public void Play()
    {
        if (!musicAudioSource.isPlaying)
        {
            musicAudioSource.Play();
        }
    }

    public void Stop()
    {
        if (musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
        }
    }

    public void ClearQueue()
    {
        curElement = null;
        musicQueue.Clear();
    }

    public void EnqueMusic(AudioClip clip, bool loop = false)
    {
        var el = new QueueElement()
        {
            clips = new AudioClip[] { clip },
            loop = loop,
            maxPlays = -1,
            playsCount = 0,
        };

        musicQueue.Enqueue(el);
    }

    public void EnqueMusic(AudioClip[] clips, bool loop = false)
    {
        var el = new QueueElement()
        {
            clips = clips,
            loop = loop,
            maxPlays = -1,
            playsCount = 0,
        };

        musicQueue.Enqueue(el);
    }

    private void Update()
    {
        if (!MusicPlaying && (musicQueue.Count > 0 || curElement.HasValue))
        {
            // Play next item in queue
            if (curElement.HasValue && curElement.Value.loop)
            {
                var el = curElement.Value;

                if (el.maxPlays > 0 && el.playsCount >= el.maxPlays)
                {
                    curElement = null;
                    return;
                }

                musicAudioSource.clip = el.clips[Random.Range(0, el.clips.Length)];
                musicAudioSource.Play();

                el.playsCount++;

                curElement = el;
            }
            else
            {
                var nextEl = musicQueue.Dequeue();
                musicAudioSource.clip = nextEl.clips[0];
                musicAudioSource.Play();

                curElement = nextEl;
            }
        }
    }
}
