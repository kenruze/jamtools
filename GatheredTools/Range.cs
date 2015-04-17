using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.UI;

[System.Serializable]
public struct Range
{
    public float minimum;
    public float maximum;
    public float value;
    public float Value
    {
        get { return value; }
        set
        {
            this.value = value;
            this.value = Mathf.Clamp(this.value, minimum, maximum);
        }
    }

    public float NormalizedValue
    {
        get { return (value - minimum) / (maximum - minimum); }
    }

    public Range(float min, float max, float val)
    {
        minimum = min;
        maximum = max;
        value = val;
    }
}