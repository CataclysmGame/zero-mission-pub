using UnityEngine;

public enum MusicAlternativePickType
{
    PickFirst,
    PickLast,
    PickRandom,
}

public static class MusicAlternativePickTypeExtensions
{
    public static AudioClip Pick(this MusicAlternativePickType self, params AudioClip[] clips)
    {
        switch (self)
        {
            case MusicAlternativePickType.PickFirst:
                return clips.Length > 0 ? clips[0] : null;
            case MusicAlternativePickType.PickLast:
                return clips.Length > 0 ? clips[clips.Length - 1] : null;
            case MusicAlternativePickType.PickRandom:
                return clips.Length > 0 ? clips[Random.Range(0, clips.Length)] : null;
            default:
                return null;
        }
    }
}
