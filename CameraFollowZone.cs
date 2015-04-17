using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.UI;

public class CompareCameraPriority : IComparer<CameraFollowZone>
{
    public int Compare(CameraFollowZone x, CameraFollowZone y)
    {
        return (y.m_priority - x.m_priority);
    }
}


public class CameraFollowZone : MonoBehaviour
{
    static bool debug_messages = false;

    public static CameraFollowZone s_currentCamera;
    public static List<CameraFollowZone> s_occupiedCameraZones = new List<CameraFollowZone>();
    public static AudioSource s_zoneMusic;
    public static bool m_transitioningMusic;

    public static IEnumerator TransitionMusic(AudioSource music)
    {
        if (music == s_zoneMusic)
            yield break;

        while (m_transitioningMusic)
        {
            yield return new WaitForEndOfFrame();
        }
        m_transitioningMusic = true;
        float destinationVolume = music.volume;
        float currentMusicVolume = s_zoneMusic.volume;
        music.volume = 0;
        music.gameObject.SetActive(true);
        float exclusiveFadeoutTime = 0.4f;
        float transitionTime = 3;
        for (float timer = 0; timer < transitionTime; timer += Time.deltaTime)
        {
            if (timer > exclusiveFadeoutTime)
            {
                music.volume += Time.deltaTime * destinationVolume / transitionTime;
            }
            s_zoneMusic.volume -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        s_zoneMusic.gameObject.SetActive(false);
        s_zoneMusic.volume = currentMusicVolume;
        s_zoneMusic = music;
        music.volume = destinationVolume;
        m_transitioningMusic = false;
    }

    public static CameraFollowZone SortCameraZones()
    {
        CameraFollowZone oldCamera = s_currentCamera;
        s_occupiedCameraZones.Sort(new CompareCameraPriority());

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

    public List<GameObject> m_fadeOut = new List<GameObject>();
    public List<GameObject> m_fadeIn = new List<GameObject>();
    public int m_priority = 0;
    public bool m_useTargetDynamicOffset = true;
    public bool m_forceLookAtTarget = false;

    public AudioSource m_zoneMusic;

    internal bool m_initComplete;
    void Init()
    {
        if (m_cam == null) m_cam = Camera.main;
        m_initComplete = true;
    }

    ////█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█
    ////█ CameraFollowZone
    [Header("Init")]
    [ContextMenuItem("init", "Init")]
    public Camera m_cam;
    public GameObject m_target;
    public CapsuleCollider m_cameraTrackingBox;
    [Header("Optional")]
    public float m_smoothTime = 0.3f;
    public float m_leadMultiplier = 1;
    public CapsuleCollider m_targetTrackingBox;
    public bool m_matchCapsuleVerticalWithTarget;
    public float m_matchHeightOffset = 3;
    public bool m_matchProportionally;
    internal bool m_activated;
    internal Vector3 m_dynamicOffset;

    void Start()
    {
    }
    //█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█
    void FixedUpdate()
    {
        if (m_activated)
        {
            Vector3 trackingPosition = m_target.transform.position;
            float matchProportion = 0;
            if (m_targetTrackingBox != null)
            {
                trackingPosition = ClosestPointOnCapsule(m_targetTrackingBox, trackingPosition) + (m_useTargetDynamicOffset ? m_dynamicOffset : Vector3.zero);
                if (m_matchProportionally)
                {
                    Vector3 start = (m_targetTrackingBox.transform.position + m_targetTrackingBox.transform.right * -m_targetTrackingBox.height / 2);
                    matchProportion = (trackingPosition - start).magnitude / m_targetTrackingBox.height;
                }
            }
            if (m_matchCapsuleVerticalWithTarget)
            {
                Vector3 capsulePos = m_cameraTrackingBox.transform.position;
                capsulePos.y = m_target.transform.position.y + m_matchHeightOffset;
                m_cameraTrackingBox.transform.position = capsulePos;
            }
            Vector3 campos;
            if (m_matchProportionally && m_targetTrackingBox != null)
            {
                Vector3 start = (m_cameraTrackingBox.transform.position + m_cameraTrackingBox.transform.right * -m_cameraTrackingBox.height / 2);
                campos = start + m_cameraTrackingBox.transform.right * matchProportion * m_cameraTrackingBox.height;
            }
            else
            {
                campos = ClosestPointOnCapsule(m_cameraTrackingBox, trackingPosition);
            }
            m_cam.transform.position = Vector3.SmoothDamp(m_cam.transform.position, campos + (m_useTargetDynamicOffset ? m_dynamicOffset : Vector3.zero) * m_leadMultiplier, ref m_positionTrackingRef, m_smoothTime);
            if (m_forceLookAtTarget)
                trackingPosition = m_target.transform.position;

            m_targetTracking = Vector3.SmoothDamp(m_targetTracking, trackingPosition + (m_useTargetDynamicOffset ? m_dynamicOffset : Vector3.zero) * m_leadMultiplier, ref m_lookTrackingRef, m_smoothTime);
            m_cam.transform.LookAt(m_targetTracking);
        }
    }
    //█▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄█

    public void ActivateCamera()
    {
        s_currentCamera = this;
        m_activated = true;

        if (m_zoneMusic != null)
        {
            if (s_zoneMusic == null)
                s_zoneMusic = m_zoneMusic;
            else
                StartCoroutine(TransitionMusic(m_zoneMusic));
        }
    }

    //███████████████████████████████████████████████

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == m_target)
        {
            if (!s_occupiedCameraZones.Contains(this))
            {
                s_occupiedCameraZones.Add(this);
                SortCameraZones();
            }
        }
    }
    void OnTriggerExit(Collider other)
    {

        if (other.gameObject == m_target)
        {
            s_occupiedCameraZones.Remove(this);
            SortCameraZones();
        }

    }

    //does not use radius correctly
    Vector3 ClosestPointOnCapsule(CapsuleCollider capsule, Vector3 point)
    {
        Vector3 arm;
        if (capsule.direction == 0)//x
            arm = Vector3.left * Mathf.Max(capsule.radius, capsule.height / 2);
        else if (capsule.direction == 1)//y
            arm = Vector3.up * Mathf.Max(capsule.radius, capsule.height / 2);
        else //(m_cameraBox.direction == 2)//z
            arm = Vector3.forward * Mathf.Max(capsule.radius, capsule.height / 2);

        Vector3 end1 = capsule.transform.position + (capsule.transform.rotation * (capsule.center + arm));
        Vector3 end2 = capsule.transform.position + (capsule.transform.rotation * (capsule.center - arm));
        return ClosestPointOnLine(end1, end2, point);

    }

    Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
    {
        var vVector1 = vPoint - vA;
        var vVector2 = (vB - vA).normalized;

        var d = Vector3.Distance(vA, vB);
        var t = Vector3.Dot(vVector2, vVector1);

        if (t <= 0)
            return vA;

        if (t >= d)
            return vB;

        var vVector3 = vVector2 * t;

        var vClosestPoint = vA + vVector3;

        return vClosestPoint;
    }
}