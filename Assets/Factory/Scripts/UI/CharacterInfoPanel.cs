using TMPro;
using UnityEngine;

public class CharacterInfoPanel : MonoBehaviour
{
    public TMP_Text characterNameText;
    public GameObject hpBar;
    public GameObject munitionsBar;

    [Tooltip("Damage bar. Ratio is based on starting damage instead of damage cap (since damage isn't capped)")]
    public GameObject damageBar;
    public GameObject speedBar;

    public float hpUpperVal = 440.0f;
    public float munitionsUpperVal = 30.0f;

    public float damageUpperVal = 25.0f;
    public float speedUpperVal = 8.0f;

    public void SetCharacter(Character character)
    {
        characterNameText.text = character.playerName;

        ScaleBar(hpBar, character.hpCap / hpUpperVal);
        ScaleBar(munitionsBar, character.munitionsCap / munitionsUpperVal);
        ScaleBar(damageBar, character.damage / damageUpperVal);
        ScaleBar(speedBar, character.movementSpeed / speedUpperVal);
    }

    private void ScaleBar(GameObject bar, float scaleX)
    {
        var s = bar.transform.localScale;
        bar.transform.localScale = new Vector3(
            Mathf.Clamp01(scaleX),
            s.y,
            s.z
        );
    }
}
