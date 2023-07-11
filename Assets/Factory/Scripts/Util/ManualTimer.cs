using UnityEngine;
using UnityEngine.Events;

public class ManualTimer
{
    public float Period { get; set; } = 1.0f;

    public bool Paused { get; set; } = false;

    private float _counter = 0.0f;

    public UnityAction Callback { get; private set; }

    public int NumTicks { get; private set; } = 0;

    public bool OneShot { get; private set; } = false;

    public ManualTimer(UnityAction callback, float period)
    {
        Callback = callback;
        Period = period;
    }

    public void Pause()
    {
        Paused = true;
    }

    public void Resume()
    {
        Paused = false;
    }

    public void Reset()
    {
        _counter = 0;
        Paused = false;
    }

    public void ResetTicks()
    {
        NumTicks = 0;
    }

    public void Update()
    {
        if (Paused)
        {
            return;
        }

        if (OneShot && NumTicks == 1)
        {
            return;
        }

        _counter += Time.deltaTime;

        if (_counter >= Period)
        {
            _counter = 0.0f;
            NumTicks++;
            Callback();
        }
    }
}
