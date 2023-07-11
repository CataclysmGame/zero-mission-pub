using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character")]
public class Character : ScriptableObject
{
    public string id;

    public string playerName = "Player";

    public int maxHp = 100;
    public int maxMunitions = 5;
    public float movementSpeed = 5.0f;
    public float shootInterval = 0.5f;
    public float reloadInterval = 1.0f;
    public float lifeSteal = 0.0f;
    public int damage = 10;

    // Caps
    public int hpCap = 20 * 11;
    public int munitionsCap = 15;
    public float movementSpeedCap = 8.0f;
    public float lifeStealCap = 0.10f;
    public float shootIntervalCap = 0.10f;
    public float reloadIntervalCap = 0.10f;

    [Multiline] public string characterDescription;

    public Texture2D avatar;

    public List<Skin> skins;
}