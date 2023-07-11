using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(ConstantFunction), menuName = "IncreaseFunctions/" + nameof(ConstantFunction))]
public class ConstantFunction : IncreaseFunction
{
    [Tooltip("The constant value returned by this function")]
    [SerializeField]
    private float _value = 0;
    public float Value => _value;

    public override float GetNewValue(float currentValue)
    {
        return _value;
    }

    public override int GetNewIntValue(float currentValue)
    {
        return Mathf.RoundToInt(GetNewValue(currentValue));
    }
}
