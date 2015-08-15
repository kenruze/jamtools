using UnityEngine;
using System.Collections;

public static class JamExtensions
{
    public static Camera Lerp(this Camera source, Camera target, float t)
    {
		if (t == 1)
		{
			source.transform.rotation = target.transform.rotation;
			source.transform.position = target.transform.position;
		}
		else
		{
			source.transform.rotation = Quaternion.Lerp (source.transform.rotation, target.transform.rotation, t);
			source.transform.position = Vector3.Lerp (source.transform.position, target.transform.position, t);
		}
        source.LerpNoTransform(target, t);
        return source;
    }

    public static Camera LerpNoTransform(this Camera source, Camera target, float t)
    {
        source.fieldOfView = Mathf.Lerp(source.fieldOfView, target.fieldOfView, t);
        //clipping planes and other stuff
        return source;
    }

    public static Camera SmoothDamp(this Camera source, Camera target, float smoothTime, ref Vector3 lookRotationRef, ref Vector3 positionRef, ref float fovRef)
    {
        source.transform.rotation = Quaternion.LookRotation(Vector3.SmoothDamp(source.transform.forward, target.transform.forward, ref lookRotationRef, smoothTime));
        source.transform.position = Vector3.SmoothDamp(source.transform.position, target.transform.position, ref positionRef, smoothTime);
        source.fieldOfView = Mathf.SmoothDamp(source.fieldOfView, target.fieldOfView, ref fovRef, smoothTime);
        return source;
    }

    public static GameObject InstantiateChild(this Transform transform, GameObject source, Vector3 localPosition = default(Vector3), Quaternion rotation = default(Quaternion))
    {
        GameObject clone;
        if (source == null)
            clone = new GameObject();
        else
            clone = GameObject.Instantiate(source) as GameObject;
        clone.transform.SetParent(transform);
        clone.transform.localPosition = localPosition;
        clone.transform.localRotation = rotation;
        return clone;
    }

    public static T InstantiateChild<T>(this Transform transform, GameObject source, Vector3 localPosition = default(Vector3), Quaternion rotation = default(Quaternion))
    {
        GameObject clone = GameObject.Instantiate(source) as GameObject;
        clone.transform.SetParent(transform);
        clone.transform.localPosition = localPosition;
        clone.transform.localRotation = rotation;
        return clone.GetComponent<T>();
    }
}
