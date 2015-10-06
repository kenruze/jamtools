using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MirrorTransform:MonoBehaviour
{
    public Transform mirror;


    public void Mirror(GenerateAThing owner = null)
    {
        if (mirror == null)
            print("mirror not set");
        else
        {

            transform.localScale = mirror.localScale;
            Vector3 take = mirror.localPosition;
            take.x *= -1;      
            transform.localPosition = take;
            take = mirror.localRotation.eulerAngles;
            take *= -1;
            transform.localRotation = Quaternion.Euler(take);
        }
    }
}
