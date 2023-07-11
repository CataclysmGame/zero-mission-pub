using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IncreaseFunction : ScriptableObject, IIncreaseFunction
{
    public abstract float GetNewValue(float currentValue);
    public abstract int GetNewIntValue(float currentValue);
}
