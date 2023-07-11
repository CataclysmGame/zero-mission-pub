using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Util
{
    public static Vector3 ZeroZ = new Vector3(1, 1, 0);

    private Util()
    {
    }

    public static Bounds GetAreaBounds(GameObject area)
    {
        return new Bounds(
            Vector3.Scale(area.transform.position, ZeroZ),
            Vector3.Scale(area.transform.localScale, ZeroZ)
        );
    }

    public static bool IsGameObjectInsideArea(GameObject gameObject, GameObject area)
    {
        // Ignore Z
        var bounds = GetAreaBounds(area);

        var objPos = Vector3.Scale(gameObject.transform.position, ZeroZ);
        return bounds.Contains(objPos);
    }

    public static GameObject[] FilterObjectsInsideArea(GameObject[] objects, GameObject area)
    {
        var bounds = GetAreaBounds(area);
        var filteredObjects = new List<GameObject>();

        foreach (var obj in objects)
        {
            var objPos = Vector3.Scale(obj.transform.position, ZeroZ);
            if (bounds.Contains(objPos))
            {
                filteredObjects.Add(obj);
            }
        }

        return filteredObjects.ToArray();
    }

    public static GameObject[] FindObjectsInsideBounds(string tag, Bounds bounds)
    {
        var allObjects = GameObject.FindGameObjectsWithTag(tag);
        var objectsInsideArea = new List<GameObject>();

        foreach (var obj in allObjects)
        {
            var objPos = Vector3.Scale(obj.transform.position, ZeroZ);
            if (bounds.Contains(objPos))
            {
                objectsInsideArea.Add(obj);
            }
        }

        return objectsInsideArea.ToArray();
    }

    public static GameObject[] FindObjectsInsideArea(string tag, GameObject area)
    {
        var bounds = GetAreaBounds(area);

        return FindObjectsInsideBounds(tag, bounds);
    }

    public static int CountObjectsInsideBounds(string tag, Bounds bounds)
    {
        var allObjects = GameObject.FindGameObjectsWithTag(tag);

        int count = 0;

        foreach (var obj in allObjects)
        {
            var objPos = Vector3.Scale(obj.transform.position, ZeroZ);
            if (bounds.Contains(objPos))
            {
                count++;
            }
        }

        return count;
    }

    public static int CountObjectsInsideArea(string tag, GameObject area)
    {
        var bounds = GetAreaBounds(area);
        return CountObjectsInsideBounds(tag, bounds);
    }

    public static float GetLinearVolume(AudioMixer audioMixer, string volumeParam)
    {
        const float min = 0.0001f;
        float volume01 = 0.0f;
        if (audioMixer.GetFloat(volumeParam, out volume01))
        {
            volume01 = Mathf.Pow(10, volume01 / 20.0f);
        }

        return Mathf.Max(volume01, min);
    }

    public static bool SetLinearVolume(
        AudioMixer audioMixer,
        string volumeParam,
        float volume01)
    {
        const float min = 0.0001f;

        var logVol = Mathf.Log10(volume01 <= 0 ? min : volume01) * 20.0f;

        return audioMixer.SetFloat(volumeParam, logVol);
    }

    public static bool ChangeVolume(AudioMixer audioMixer, int step = 5)
    {
        float curVol = 0.0f;
        if (!audioMixer.GetFloat("masterVolume", out curVol))
        {
            return false;
        }
        var newVol = Mathf.Clamp(curVol + step, -80, 0);
        return audioMixer.SetFloat("masterVolume", newVol);
    }

    public static async UniTask WaitClipEnd(AudioSource audioSource, AudioClip clip)
    {
        await UniTask.Delay(Mathf.CeilToInt(clip.length * 1000.0f * 0.9f));
        await UniTask.WaitWhile(() => audioSource.isPlaying);
    }

    public static async UniTask FadeAudio(AudioSource audioSource, float duration, float targetVolume)
    {
        float curTime = 0.0f;
        float start = audioSource.volume;

        while (curTime < duration)
        {
            curTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, curTime / duration);

            await UniTask.Yield();
        }

        audioSource.volume = targetVolume;
    }

    public static bool RandomBool()
    {
        return Random.Range(0, 2) == 1;
    }

    public static T PickRandom<T>(IList<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static bool AreBoundsOverlapping(Bounds b1, Bounds b2)
    {
        float b1_l = b1.center.x - b1.extents.x;
        float b1_r = b1.center.x + b1.extents.x;
        float b1_t = b1.center.y + b1.extents.y;
        float b1_b = b1.center.y - b1.extents.y;

        float b2_l = b2.center.x - b2.extents.x;
        float b2_r = b2.center.x + b2.extents.x;
        float b2_t = b2.center.y + b2.extents.y;
        float b2_b = b2.center.y - b2.extents.y;

        return (b1_l < b2_r && b1_r > b2_l && b1_t > b2_b && b1_b < b2_t);
    }

    public static int GetRandomWeightedIndex(List<float> weights)
    {
        if (weights == null || weights.Count == 0) return -1;

        float w;
        float t = 0;
        int i;
        for (i = 0; i < weights.Count; i++)
        {
            w = weights[i];

            if (float.IsPositiveInfinity(w))
            {
                return i;
            }
            else if (w >= 0f && !float.IsNaN(w))
            {
                t += weights[i];
            }
        }

        float r = UnityEngine.Random.Range(0f, 1f);
        float s = 0f;

        for (i = 0; i < weights.Count; i++)
        {
            w = weights[i];
            if (float.IsNaN(w) || w <= 0f) continue;

            s += w / t;
            if (s >= r) return i;
        }

        return -1;
    }
}
