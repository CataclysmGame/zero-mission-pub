using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIncreaseFunction
{
    public float GetNewValue(float currentValue);
    public int GetNewIntValue(float currentValue);
}
