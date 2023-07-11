using System;
using System.Collections.Generic;

public sealed class SceneParameters
{
    public static readonly string PARAM_PLAYER_ATLAS = "playerAtlas";
    public static readonly string PARAM_PLAYER_AVATAR = "playerAvatar";
    public static readonly string PARAM_PLAYER_CHR_IDX = "playerCharIndex";
    public static readonly string PARAM_PLAYER_SKIN = "playerSkin";
    public static readonly string PARAM_PLAYER_SKIN_ID = "playerSkinId";
    public static readonly string PARAM_PLAYER_SKIN_FIRST_HAND = "playerSkinFirstHand";

    public IDictionary<string, object> Parameters { get; private set; }

    private static SceneParameters instance = null;

    private SceneParameters()
    {
        Parameters = new Dictionary<string, object>();
    }

    public static SceneParameters Instance
    {
        get => instance ?? (instance = new SceneParameters());
    }

    public void SetParameter(string name, object value)
    {
        Parameters[name] = value;
    }

    public T GetParameter<T>(string name) => (T)Parameters[name];

    public T GetParameter<T>(string name, Func<T> onEmpty)
    {
        if (Parameters.ContainsKey(name))
        {
            return (T)Parameters[name];
        }

        return onEmpty();
    }

    public void ClearParameters()
    {
        Parameters.Clear();
    }

    public bool ContainsParameter(string name) =>
        Parameters.ContainsKey(name);
}
