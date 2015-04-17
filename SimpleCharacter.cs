using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleCharacter : MonoBehaviour
{
    [Header("Init")]
    [ContextMenuItem("init", "Init")]
    public Camera m_cam;
    Vector3 m_camSmoothRef;
    Vector3 m_camSmoothLookRef;

    public Animator m_animator;
    public Rigidbody m_body;
    public CapsuleCollider m_collider;
    public LayerMask m_groundCollisionMask = 16 - 1;
    public string walkingAnimatorPropName = "walking";

    [Header("Parameters")]
    public float m_movementSpeed = 10;
    public float m_rotationSpeed = 720;
    public enum CharacterInputMode
    {
        gamepadKeyboard,
        providedDirectionInput,
        walkToTarget,
        walkToTargetMouse,
        gamepadKeyboardMouse,
    }
    public CharacterInputMode m_inputMode;
    bool m_grounded;
    Vector3 m_velocity;
    internal bool m_animationLocked;
    internal Vector3 m_movementInput;
    internal Vector3 m_walkTarget;
    internal bool m_walkingToTarget;
    internal bool m_receivingMoveInput;
    internal float m_movementThrottle = 1;

    internal bool _initDone;

    public void Init()
    {
        if (m_cam == null)
        {
            m_cam = Camera.main;
        }
        if (m_animator == null)
        {
            m_animator = GetComponentInChildren<Animator>();
        }
        if (m_body == null)
        {
            m_body = GetComponentInChildren<Rigidbody>();
        }
        if (m_collider == null)
        {
            m_collider = GetComponentInChildren<CapsuleCollider>();
        }
        _initDone = true;
    }

    void Start()
    {
        if (!_initDone)
        {
            Init();
        }
    }

    void Update()
    {
        if (m_inputMode == CharacterInputMode.gamepadKeyboard || m_inputMode == CharacterInputMode.gamepadKeyboardMouse)
        {
            Vector3 displacement = Vector3.zero;
            displacement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            //update motion
            float mag = displacement.magnitude;
            if (mag >= 0.01f)
            {
                if (mag > 1)
                {
                    displacement.Normalize();
                    mag = 1;
                }
                //rotate stick input to camera orientation
                Vector3 flatcamforward = m_cam.transform.forward;
                flatcamforward.y = 0; flatcamforward.Normalize();
                Quaternion camToCharacterSpace = Quaternion.FromToRotation(Vector3.forward, flatcamforward);
                displacement = (camToCharacterSpace * displacement); displacement.y = 0;
                displacement.Normalize();
                displacement = displacement * mag;

                m_movementInput = displacement;
                m_walkingToTarget = false;
                m_receivingMoveInput = true;
            }
            else
            {
                m_movementInput = Vector3.zero;
                m_receivingMoveInput = false;
            }
        }
        if (m_inputMode == CharacterInputMode.walkToTarget || m_inputMode == CharacterInputMode.walkToTargetMouse || m_inputMode == CharacterInputMode.gamepadKeyboardMouse)
        {
            if (Input.GetMouseButton(0) && (m_inputMode == CharacterInputMode.walkToTargetMouse || m_inputMode == CharacterInputMode.gamepadKeyboardMouse))
            {
                RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 10000, m_groundCollisionMask);
                float distance = float.MaxValue;
                int closestHit = -1;

                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].distance < distance)
                    {
                        closestHit = i;
                        distance = hits[i].distance;
                    }
                }
                if (closestHit >= 0)
                {
                    m_walkTarget = hits[closestHit].point;
                    m_walkingToTarget = true;
                }
            }
            if (!m_receivingMoveInput)
            {
                if (m_walkingToTarget)
                {
                    Vector3 displacement = -(transform.position - m_walkTarget);
                    displacement.y = 0;
                    m_movementInput = displacement;
                    m_movementInput.Normalize();
                    if (displacement.magnitude <= 0.2f)
                    {
                        m_walkingToTarget = false;
                    }
                }
                else
                {
                    m_movementInput = Vector3.zero;
                }
            }
        }
    }

    void FixedUpdate()
    {
        //update motion
        float mag = m_movementInput.magnitude;
        if (mag >= 0.01f)
        {
            if (mag > 1)
            {
                m_movementInput.Normalize();
                mag = 1;
            }
            m_grounded = false;

            Vector3 moveStep = m_movementInput * m_movementSpeed * m_movementThrottle;
            moveStep.y = m_velocity.y;
            m_velocity = moveStep;
            m_body.transform.rotation = Quaternion.RotateTowards(m_body.transform.rotation, Quaternion.LookRotation(m_movementInput), m_rotationSpeed * Time.deltaTime);
        }
        else if (m_grounded)
        {
            m_velocity *= Mathf.Clamp01(1 - 16 * Time.deltaTime);
        }

        if (m_animator != null && walkingAnimatorPropName!="")
        {
            m_animator.SetFloat( walkingAnimatorPropName, mag);
        }

        const float stepHeight = 0.02f;
        RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.up * m_collider.height / 2, m_collider.radius - 0.1f, Vector3.down, m_collider.height / 2 + 0.2f, m_groundCollisionMask);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider == m_collider)
                continue;

            m_grounded = true;
            float disparity = m_collider.height / 2 - m_collider.radius - hits[i].distance;
            if (disparity > 0 - stepHeight)
            {
                m_velocity.y += (disparity + stepHeight);
                transform.position += Vector3.up * (disparity + stepHeight);
                break;
            }
        }
        m_body.velocity = m_velocity;

        //█physics
        if (!m_grounded)
        {
            m_velocity.y -= 9.8f * Time.deltaTime;
        }
        else
        {
            m_velocity.y = 0;
        }
    }

    public void SetWalkTarget(Vector3 position)
    {
        m_walkTarget = position;
        m_walkingToTarget = true;
    }

    public void StopWalking()
    {
        m_walkTarget = transform.position;
        m_walkingToTarget = false;
    }

    public void PlayAnimation(string animationName)
    {
        float crossFadeTime = 0.25f;
        m_animator.CrossFade(animationName, crossFadeTime, 0, 0);
    }

   
}
