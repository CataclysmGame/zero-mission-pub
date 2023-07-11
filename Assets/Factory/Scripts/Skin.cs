using UnityEngine;

[CreateAssetMenu(menuName = "Skin")]
public class Skin : ScriptableObject
{
    public string id;
    public string displayName;

    [Multiline] public string description;

    public Texture2D splashArt;
    public Texture2D avatar;
    public Texture2D inGameAvatar;
    public Texture2D atlas;

    [Tooltip("Optional skin glow")] public Texture2D glow;
    [Tooltip("Optional run skin glow")] public Texture2D runGlow;
}