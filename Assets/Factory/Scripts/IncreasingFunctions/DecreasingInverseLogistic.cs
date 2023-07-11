using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(DecreasingInverseLogistic), menuName = "IncreaseFunctions/" + nameof(DecreasingInverseLogistic))]
public class DecreasingInverseLogistic : IncreaseFunction
{
    [Tooltip("The starting value")]
    [SerializeField]
    private float _startingValue = 0;
    public float StartingValue => _startingValue;

    [Tooltip("The minimum value this function can return")]
    [SerializeField]
    private float _minimumValue = 5;

    [SerializeField]
    private int _alpha = 1;

    [SerializeField]
    private int _beta = 1;

    [SerializeField]
    private float _gamma = 0.1f;

    public override float GetNewValue(float currentValue)
    {
        return Mathf.Max(Mathf.Min(1 / (Mathf.Min(_alpha / (1 + Mathf.Exp(-_gamma * (currentValue - _beta))), 200)), _startingValue), _minimumValue);
    }

    public override int GetNewIntValue(float currentValue)
    {
        return Mathf.RoundToInt(GetNewValue(currentValue));
    }
}
