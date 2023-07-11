using UnityEngine;
using UnityEngine.UI;

public enum AutoSelectCondition
{
    OnAwake,
    OnStart,
    OnEnable,
}

public class AutoSelect : MonoBehaviour
{
    public Selectable selectable;
    public AutoSelectCondition condition = AutoSelectCondition.OnEnable;

    private void Awake()
    {
        if (condition == AutoSelectCondition.OnAwake)
        {
            selectable.Select();
        }
    }

    private void Start()
    {
        if (condition == AutoSelectCondition.OnStart)
        {
            selectable.Select();
        }
    }

    private void OnEnable()
    {
        if (condition == AutoSelectCondition.OnEnable)
        {
            selectable.Select();
        }
    }
}
