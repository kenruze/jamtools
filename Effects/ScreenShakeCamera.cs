using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//USAGE: place as a child of another camera, disable the other camera's camera component.
//this camera will match the disabled one and shake as a localposition change
//you can move and rotate (and modify field of view) the parent camera
public class ScreenShakeCamera : MonoBehaviour
{

	Camera cam;
	public Camera targetParentCamera;
	public float cameraShake;

	List<CameraShake> shakeStack = new List<CameraShake> ();

	void Awake ()
	{
		if (targetParentCamera == null)
			targetParentCamera = transform.parent.GetComponent<Camera> ();
		cam = GetComponent<Camera> ();
	}

	void Update ()
	{
		cam.Lerp (targetParentCamera, 1);

		if (shakeStack.Count > 0)
		{
			float intensity = 0;
			for (int i = 0; i < shakeStack.Count; i++)
			{
				if (Time.time > shakeStack [i].EndTime)
				{
					shakeStack.RemoveAt (i);
					--i;
				}
				else
				{
					intensity += shakeStack [i].Evaluate ();
				}
			}
			cameraShake = intensity;
		}

		if (cameraShake > 0)
		{
			transform.localPosition = Random.onUnitSphere * 0.1f * cameraShake;
		}
		else
		{
			transform.localPosition = Vector3.zero;
		}
	}

	public void AddShake(AnimationCurve intensity, float duration = 1)
	{
		shakeStack.Add(new CameraShake(){intensityCurve = intensity, StartTime = Time.time, EndTime = Time.time+duration});
	}

	public class CameraShake
	{
		public AnimationCurve intensityCurve;
		public float StartTime;
		public float EndTime;

		public float Evaluate ()
		{
			return intensityCurve.Evaluate (Time.time - StartTime);
		}
	}
}
