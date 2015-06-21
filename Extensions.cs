using UnityEngine;
using System.Collections;

public static class JamExtensions
{
    public static Camera Lerp(this Camera source, Camera target, float t)
    {
        source.transform.rotation = Quaternion.Lerp(source.transform.rotation, target.transform.rotation, t);
        source.transform.position = Vector3.Lerp(source.transform.position, target.transform.position, t);
        source.fieldOfView = Mathf.Lerp(source.fieldOfView, target.fieldOfView, t);
        return source;
    }

    public static T InstantiateChild<T>(this Transform transform, GameObject source, Vector3 localPosition = default(Vector3), Quaternion rotation = default(Quaternion))where T : MonoBehaviour
    {
        GameObject clone = GameObject.Instantiate(source) as GameObject;
        clone.transform.SetParent(transform);
        clone.transform.localPosition = localPosition;
        clone.transform.localRotation = rotation;
        return clone.GetComponent<T>();
    }
}
