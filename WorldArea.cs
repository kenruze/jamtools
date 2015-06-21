using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.UI;

public class WorldArea : MonoBehaviour
{
    //internal bool m_initComplete;
    //void Init()
    //{
    //    //if (m_cam == null) m_cam = Camera.main;
    //    m_initComplete = true;
    //}

    ////█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█
    ////█ WorldArea
    //[Header("Init")]
    //[ContextMenuItem("init", "Init")]
    //public Camera m_cam;
    public List<GameObject> contents = new List<GameObject>();
    public List<GameObject> triggerTargets = new List<GameObject>();
    public bool startingArea;
    int countInside;

    void Start()
    {
        //if (!m_initComplete) Init();
        if (!startingArea)
            HideContents();
    }

    void HideContents()
    {
        for (int i = 0; i < contents.Count; i++)
        {
            contents[i].SetActive(false);
        }
    }
    void ShowContents()
    {
        for (int i = 0; i < contents.Count; i++)
        {
            contents[i].SetActive(true);
        }
    }

    //█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█
    void Update()
    {
    }
    //█▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄█

    //███████████████████████████████████████████████

    void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < triggerTargets.Count; i++)
        {
            if (other.gameObject == triggerTargets[i])
            {
                if (countInside <= 0)
                    ShowContents();
                countInside++;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        for (int i = 0; i < triggerTargets.Count; i++)
        {
            if (other.gameObject == triggerTargets[i])
            {
                --countInside;
                if (countInside <= 0)
                    HideContents();
            }
        }
    }
}