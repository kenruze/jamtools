using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JamTools;

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
 
    ////█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█
    ////█ CameraTrack
    public Camera m_cam;
    public GameObject m_target;
    [UnityEngine.Serialization.FormerlySerializedAs("m_cameraTrackingBox")]
    public BoxCollider m_cameraMappingingBox;
    public int m_priority = 0;
    public float m_smoothTime = 0.3f;
    public float m_lookSmoothTime = 0.3f;
    public float m_camLerpSmoothTime = 2f;

    public enum LookMode
    {
        TargetWithOffset,
        TargetOnTrackWithOffset,
        MapBoxZ,
        TrackBoxZ,
        LerpCamera,
    }

    public LookMode lookMode;
    public Vector3 lookOffset;

    public Rigidbody targetBody;
    public float leadForwards = 0;
    public float leadVelocityXZ = 0;
    public float leadVelocityY = 0;
    public float leadVelocityXZMax = 10;
    public float leadVelocityYMax = 10;

    [Header("Optional")]
    public float m_leadMultiplier = 1;
    public BoxCollider m_targetTrackingBox;
    public Camera cameraLerpTarget;
    internal bool m_activated;
    internal Vector3 leadOffset;
    internal int triggerClicks;
    [Header("Control Properties")]
    public PuppetControlCameraRelative.CameraTrackConstrainZ constrainZ;

    public bool mapUseFixedUpdate = true;
    public bool lookUseFixedUpdate = true;

    internal Vector3 targetOnTrackPos;
    static Vector3 lookDirectionRef;

    static Vector3 lookDirectionRef2;
    static Vector3 positionRef;
    static float fovRef;

    void Start()
    {
    }
    //█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█
    void Update()
    {
        if (!mapUseFixedUpdate && m_activated)
            MapUpdate();
        if (!lookUseFixedUpdate && m_activated)
            LookUpdate();
    }

    void FixedUpdate()
    {
        if (mapUseFixedUpdate && m_activated)
            MapUpdate();    
        if (lookUseFixedUpdate && m_activated)
            LookUpdate();
    }

    void MapUpdate()
    {
        Vector3 lastCameraPos = m_cam.transform.position;
        Vector3 trackPosition = m_target.transform.position;
        if (targetBody != null)
        {
            if (constrainZ!= PuppetControlCameraRelative.CameraTrackConstrainZ.Off)
            {
                trackPosition += Vector3.ProjectOnPlane(targetBody.transform.forward, m_targetTrackingBox.transform.forward) * leadForwards;
            }
            else
            {
                trackPosition += targetBody.transform.forward * leadForwards;
            }

            Vector3 velocityComponent = targetBody.velocity;
            float velocityY = Mathf.Min(velocityComponent.y * leadVelocityY, leadVelocityYMax);
            velocityComponent.y = 0;
            velocityComponent = Mathf.Min(velocityComponent.magnitude * leadVelocityXZ, leadVelocityXZMax) * velocityComponent.normalized;
            velocityComponent.y = velocityY;
            trackPosition += velocityComponent;

        }
        Vector3 
        campos = MapBoxPosition(trackPosition, m_targetTrackingBox, m_cameraMappingingBox, out targetOnTrackPos);
        if (cameraLerpTarget)
        {
            m_cam.SmoothDamp(cameraLerpTarget, m_camLerpSmoothTime, ref lookDirectionRef2, ref positionRef, ref fovRef);
        }
        if(m_leadMultiplier !=0)
            m_cam.transform.position = Vector3.SmoothDamp(lastCameraPos, campos + leadOffset * m_leadMultiplier, ref m_positionTrackingRef, m_smoothTime);
        else
            m_cam.transform.position = Vector3.SmoothDamp(lastCameraPos, campos, ref m_positionTrackingRef, m_smoothTime);
        
    }

    void LookUpdate()
    {
        switch (lookMode)
        {
            case LookMode.TargetWithOffset:
                m_cam.transform.rotation = Quaternion.LookRotation(Vector3.SmoothDamp(m_cam.transform.forward, m_target.transform.position - m_cam.transform.position + lookOffset, ref lookDirectionRef, m_lookSmoothTime));
                break;
            case LookMode.TargetOnTrackWithOffset:
                m_cam.transform.rotation = Quaternion.LookRotation(Vector3.SmoothDamp(m_cam.transform.forward, targetOnTrackPos - m_cam.transform.position + lookOffset, ref lookDirectionRef, m_lookSmoothTime));
                break;
            case LookMode.MapBoxZ:
                m_cam.transform.rotation = Quaternion.LookRotation(Vector3.SmoothDamp(m_cam.transform.forward, m_cameraMappingingBox.transform.forward, ref lookDirectionRef, m_lookSmoothTime));
                break;
            case LookMode.TrackBoxZ:
                m_cam.transform.rotation = Quaternion.LookRotation(Vector3.SmoothDamp(m_cam.transform.forward, m_targetTrackingBox.transform.forward, ref lookDirectionRef, m_lookSmoothTime));
                break;
            case LookMode.LerpCamera:
                m_cam.transform.rotation = Quaternion.LookRotation(Vector3.SmoothDamp(m_cam.transform.forward, cameraLerpTarget.transform.forward, ref lookDirectionRef, m_lookSmoothTime));
                break;
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

    Vector3 MapBoxPosition(Vector3 position, BoxCollider box, BoxCollider mapBox, out Vector3 targetOnTrackPos)
    {
        Vector3 displacement = position - (box.transform.position + box.center);
        displacement = Quaternion.Inverse(box.transform.rotation) * displacement;
        targetOnTrackPos = Vector3.zero;
        if (box.size.x != 0 && box.transform.lossyScale.x != 0)
        {
            displacement.x = Mathf.Clamp(displacement.x, -box.size.x / 2, box.size.x / 2);
            targetOnTrackPos.x = displacement.x;
            displacement.x /= box.size.x * box.transform.lossyScale.x;
            displacement.x *= mapBox.size.x * mapBox.transform.lossyScale.x;        
        }
        else
        {
            displacement.x = 0;
        }
        if (box.size.y != 0 && box.transform.lossyScale.y != 0)
        {
            displacement.y = Mathf.Clamp(displacement.y, -box.size.y / 2, box.size.y / 2);
            targetOnTrackPos.y = displacement.y;
            displacement.y /= box.size.y * box.transform.lossyScale.y;
            displacement.y *= mapBox.size.y * mapBox.transform.lossyScale.y;
        }
        else
        {
            displacement.y = 0;
        }
        if (box.size.z != 0 && box.transform.lossyScale.z != 0)
        {
            displacement.z = Mathf.Clamp(displacement.z, -box.size.z / 2, box.size.z / 2);
            targetOnTrackPos.z = displacement.z;
            displacement.z /= box.size.z * box.transform.lossyScale.z;
            displacement.z *= mapBox.size.z * mapBox.transform.lossyScale.z;
        }
        else
        {
            displacement.z = 0;
        }
        targetOnTrackPos = box.transform.rotation * targetOnTrackPos + box.transform.position + box.center;
        displacement = mapBox.transform.rotation * displacement;
        return displacement + mapBox.transform.position + mapBox.center;
    }


    [ContextMenu("default zone setup")]
    public void DefaultZoneSetup()
    {
        ZoneSetup(new Vector3(40, 20, 20), new Vector3(0, 4, -30), false, Camera.main);
    }

    [ContextMenu("platformer setup 20 x 2")]
    public void ZoneSetupPlatformer20x2()
    {
        ZoneSetup(new Vector3(20, 20, 2), new Vector3(0, 4, -30), true, Camera.main);
    }

    public void ZoneSetup(Vector3 zoneDimensions, Vector3 cameraMapBoxOffset, bool sideScroller = false, Camera targetCam = null)
    {
        BoxCollider triggerArea = GetComponent<BoxCollider>();
        if (triggerArea == null)
        {
            triggerArea = gameObject.AddComponent<BoxCollider>();
            triggerArea.size = zoneDimensions;
        }
        triggerArea.isTrigger = true;
        if (m_targetTrackingBox == null)
        {
            m_targetTrackingBox = transform.InstantiateChild(null).AddComponent<BoxCollider>();
            m_targetTrackingBox.name = "TargetTrackingBox";
            m_targetTrackingBox.size = zoneDimensions;
        }
        m_targetTrackingBox.isTrigger = true;
        m_targetTrackingBox.enabled = false;
        if (m_cameraMappingingBox == null)
        {
            m_cameraMappingingBox = transform.InstantiateChild(null, cameraMapBoxOffset).AddComponent<BoxCollider>();
            m_cameraMappingingBox.name = "CameraMappingBox";
            m_cameraMappingingBox.size = zoneDimensions;
        }
        m_cameraMappingingBox.isTrigger = true;
        m_cameraMappingingBox.enabled = false;
        m_cam = Camera.main;

        if (sideScroller)
        {
            constrainZ = PuppetControlCameraRelative.CameraTrackConstrainZ.Map;
            lookMode = LookMode.MapBoxZ;
        }
        else
        {
            constrainZ =  PuppetControlCameraRelative.CameraTrackConstrainZ.Off;
        }

        if (targetCam != null)
        {
            cameraLerpTarget = m_cameraMappingingBox.gameObject.AddComponent<Camera>();
            cameraLerpTarget.enabled = false;
            cameraLerpTarget.LerpNoTransform(targetCam, 1);
        }
        if (m_target == null)
            m_target = GameObject.Find("CamTrackingTarget");
    }
}