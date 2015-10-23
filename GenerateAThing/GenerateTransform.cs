using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerateTransform:GenerateComponent
{

    public Vector3 positionRanges;
    public Vector3 rotationRanges;
    public bool useWorldForScale;
    public Vector3 scaleMin = Vector3.one;
    public Vector3 scaleMax = Vector3.one;

    protected Vector3 initialLocalPosition;
    protected Quaternion initialLocalRotation;
    protected Vector3 initialLocalScale;
    protected Vector3 initialWorldScale;

    void Awake()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        initialLocalScale = transform.localScale;
        initialWorldScale = transform.lossyScale;
    }

    public override void Generate(GenerateAThing owner = null)
    {
        if (functionActive == ActiveSetting.On)
        {
            print("generate transform");
            transform.localPosition = initialLocalPosition + RandomInsideBox(positionRanges);
            if (useWorldForScale)
            {
                var parent = transform.parent;
                transform.SetParent(null);
                transform.localScale = MultiplyPerAxis(initialWorldScale, RandomVectorBetween(scaleMin, scaleMax));
                transform.SetParent(parent);
            }
            else
                transform.localScale = MultiplyPerAxis(initialLocalScale,  RandomVectorBetween(scaleMin, scaleMax));
            transform.localRotation = Quaternion.Euler(RandomInsideBox(rotationRanges)) * initialLocalRotation;
        }
        else if (functionActive == ActiveSetting.Disable)
        {
            transform.localPosition = initialLocalPosition;
            transform.localScale = initialLocalScale;
            transform.localRotation = initialLocalRotation;
        }
    }

    public static Vector3 RandomInsideBox(Vector3 magnitudes, bool bidirectional = true)
    {
        if (bidirectional)
            return new Vector3(Random.Range(-magnitudes.x, magnitudes.x), Random.Range(-magnitudes.y, magnitudes.y), Random.Range(-magnitudes.z, magnitudes.z));
        else
            return new Vector3(Random.Range(0, magnitudes.x), Random.Range(0, magnitudes.y), Random.Range(0, magnitudes.z));
    }

    public static Vector3 RandomVectorBetween(Vector3 min, Vector3 max)
    {
        return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
    }

    public static Vector3 MultiplyPerAxis(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
}
