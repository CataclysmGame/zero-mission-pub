using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(SteppedLogistic), menuName = "IncreaseFunctions/" + nameof(SteppedLogistic))]
public class SteppedLogistic : IncreaseFunction
{
    [Tooltip("The starting value")]
    [SerializeField]
    private int _startingValue = 0;
    public int StartingValue => _startingValue;

    [Tooltip("The maximum value this function can return")]
    [SerializeField]
    private int _maxValueCap = 5;

    [SerializeField]
    private int _alpha = 1;

    [SerializeField]
    private int _beta = 1;

    [SerializeField]
    private float _gamma = 0.1f;

    public override float GetNewValue(float currentValue)
    {
        return Mathf.Min(_alpha / (1 + Mathf.Exp(-_gamma * (currentValue - _beta))) + _startingValue, _maxValueCap);
    }

    public override int GetNewIntValue(float currentValue)
    {
        return Mathf.RoundToInt(GetNewValue(currentValue));
    }
}
