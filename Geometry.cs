using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using UnityEngine.UI;

public class Geometry
{
    // The angle between dirA and dirB around axis
    public static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
    {
        // Project A and B onto the plane orthogonal target axis
        dirA = dirA - Vector3.Project(dirA, axis);
        dirB = dirB - Vector3.Project(dirB, axis);

        // Find (positive) angle between A and B
        float angle = Vector3.Angle(dirA, dirB);

        // Return angle multiplied with 1 or -1
        return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
    }
}

public class InterpolatedValue
{
    public float value;
    public float velocityRef;
    public float target;
    public float smoothTime;

    public InterpolatedValue()
    {
        value = 0;
        velocityRef = 0;
        target = 0;
        smoothTime = 1;
    }

    public float InterpolateStep()
    {
        value = Mathf.SmoothDamp(value, target, ref velocityRef, smoothTime);
        return value;
    }

    public float InterpolateStep(float target, float smoothTime = -1)
    {
        if (smoothTime < 0)
            smoothTime = this.smoothTime;
        value = Mathf.SmoothDamp(value, target, ref velocityRef, smoothTime);
        return value;
    }
}

public class InterpolatedVector
{
    public Vector3 value;
    public Vector3 velocityRef;
    public Vector3 target;
    public float smoothTime = 1;

    public InterpolatedVector()
    {
        value = Vector3.zero;
        velocityRef = Vector3.zero;
        target = Vector3.zero;
        smoothTime = 1;
    }

    public Vector3 InterpolateStep()
    {
        value = Vector3.SmoothDamp(value, target, ref velocityRef, smoothTime);
        return value;
    }

    public Vector3 InterpolateStep(Vector3 target, float smoothTime = -1)
    {
        if (smoothTime < 0)
            smoothTime = this.smoothTime;
        value = Vector3.SmoothDamp(value, target, ref velocityRef, smoothTime);
        return value;
    }
}

public class TimerValue
{
    public float value;
    public float max;
    public bool autoReset;
    public bool carryTickRemainder = true;

    public TimerValue(float initMax, bool autoResetting = true)
    {
        value = initMax;
        max = initMax;
        autoReset = autoResetting;
    }

    public float progressToZero
    {
        get { return 1 - value / max; }
    }

    public float progressOfMax
    {
        get { return value / max; }
    }

    public bool Tick(float deltaT)
    {
        value -= deltaT;
        if (value <= 0)
        {
            if (autoReset)
            {
                value = max + (carryTickRemainder ? value : 0);
            }
            return true;
        }
        return false;
    }

    public void Reset()
    {
        value = max;
    }
}