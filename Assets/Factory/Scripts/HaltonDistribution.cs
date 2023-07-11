using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaltonDistribution
{
    private int _counter = 0;

    private int _base1 = 2;
    private int _base2 = 3;

    public HaltonDistribution(int startingCounter)
    {
        _counter = startingCounter;
    }

    public Vector3 GetPoint(float xMin, float xMax, float yMin, float yMax)
    {
        float x = ComputeHaltonPoint(_counter, _base1);
        float y = ComputeHaltonPoint(_counter, _base2);
        _counter++;

        return new Vector3(
            Mathf.Lerp(xMin, xMax, x),
            Mathf.Lerp(yMin, yMax, y),
            0);
    }

    float ComputeHaltonPoint(int i, int b)
    {
        float result = 0;
        float f = 1f / b;
        while (i > 0)
        {
            result += f * (i % b);
            i = Mathf.FloorToInt(i / (float)b);
            f = f / b;
        }
        return result;
    }
}
