using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using UnityEngine.UI;

public class CompareCameraTrackPriority : IComparer<CameraTrack>
{
    public int Compare(CameraTrack x, CameraTrack y)
    {
        return (y.m_priority - x.m_priority);
    }
}


public class CameraTrack : MonoBehaviour
{
    static bool debug_messages = false;

    public static CameraTrack s_currentCamera;
    public static List<CameraTrack> s_occupiedCameraZones = new List<CameraTrack>();

    public static CameraTrack SortCameraZones()
    {
        CameraTrack oldCamera = s_currentCamera;
        s_occupiedCameraZones.Sort(new CompareCameraTrackPriority());

        if (debug_messages)
        {
            print("list cameras");
            for (int i = 0; i < s_occupiedCameraZones.Count; i++)
            {
                print(s_occupiedCameraZones[i].m_priority);
            }
            print("list over");
        }
        if (s_occupiedCameraZones.Count == 0)
            s_currentCamera = null;
        else if (oldCamera != s_occupiedCameraZones[0])
            s_occupiedCameraZones[0].ActivateCamera();

        if (oldCamera != null && oldCamera != s_currentCamera)
            oldCamera.m_activated = false;

        return s_currentCamera;
    }

    internal static Vector3 m_positionTrackingRef;
    internal static Vector3 m_lookTrackingRef;
    internal static Vector3 m_targetTracking;
 
    ////█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█
    ////█ CameraTrack
    public Camera m_cam;
    public GameObject m_target;
    public BoxCollider m_cameraTrackingBox;
    public int m_priority = 0;
    public bool m_useTargetDynamicOffset = true;
    public bool m_forceLookAtTarget = false;
    public float m_smoothTime = 0.3f;

    [Header("Optional")]
    public float m_leadMultiplier = 1;
    public BoxCollider m_targetTrackingBox;
    public Camera cameraLerpTarget;
    internal bool m_activated;
    internal Vector3 m_dynamicOffset;
    internal int triggerClicks;
    [Header("Control Properties")]
    public bool constrainZ;
    public BoxCollider constraintTarget;


    void Start()
    {
    }
    //█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█
    void FixedUpdate()
    {
        if (m_activated)
        {
            Vector3 lastCameraPos = m_cam.transform.position;
            Vector3 campos;
            campos = MapBoxPosition(m_target.transform.position, m_targetTrackingBox, m_cameraTrackingBox);
            if (cameraLerpTarget)
            {
                m_cam.Lerp(cameraLerpTarget, Time.deltaTime * 3f);
            }
            if (!float.IsNaN(campos.x))
            {
                m_cam.transform.position = Vector3.SmoothDamp(lastCameraPos, campos + (m_useTargetDynamicOffset ? m_dynamicOffset : Vector3.zero) * m_leadMultiplier, ref m_positionTrackingRef, m_smoothTime);
            }
            if (m_forceLookAtTarget)
                m_cam.transform.LookAt(m_target.transform.position);
        }
    }
    //█▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄█

    public void ActivateCamera()
    {
        s_currentCamera = this;
        m_activated = true;
    }

    //███████████████████████████████████████████████

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == m_target)
        {
            triggerClicks++;
            if (triggerClicks > 0)
            {
                if (!s_occupiedCameraZones.Contains(this))
                {
                    s_occupiedCameraZones.Add(this);
                    SortCameraZones();
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == m_target)
        {
            --triggerClicks;
            if (triggerClicks <= 0)
            {
                s_occupiedCameraZones.Remove(this);
                SortCameraZones();
            }
        }
    }

    Vector3 MapBoxPosition(Vector3 position, BoxCollider box, BoxCollider mapBox)
    {
        Vector3 displacement = position - (box.transform.position + box.center);
        displacement = Quaternion.Inverse(box.transform.rotation) * displacement;
        displacement.x = Mathf.Clamp(displacement.x, -box.size.x / 2, box.size.x / 2) / box.size.x;
        displacement.y = Mathf.Clamp(displacement.y, -box.size.y / 2, box.size.y / 2) / box.size.y;
        displacement.z = Mathf.Clamp(displacement.z, -box.size.z / 2, box.size.z / 2) / box.size.z;

        displacement.x *= mapBox.size.x;
        displacement.y *= mapBox.size.y;
        displacement.z *= mapBox.size.z;

        displacement = mapBox.transform.rotation * displacement;
        return displacement + mapBox.transform.position + mapBox.center;
    }
}