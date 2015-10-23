using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerateAThing : MonoBehaviour
{
    
    public bool generateOnAwake = true;
    public bool generateOnPressR = true;

    [Header("Exposed")]
    public List<GenerateComponent> generatorComponents;
    public List<MirrorTransform> mirrors;
    // Use this for initialization
    void Awake()
    {
        if (generatorComponents == null || generatorComponents.Count == 0)
        {
            CollectComponents();
        }

        if (generateOnAwake)
            Generate();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Generate();
        }
    }

    public void Generate()
    {
        for (int i = 0; i < generatorComponents.Count; i++)
        {
            generatorComponents[i].Generate(this);
        }
        for (int i = 0; i < mirrors.Count; i++)
        {
            mirrors[i].Mirror(this);
        }
    }

    [ContextMenu("Collect Generative Components")]
    public void CollectComponents()
    {
        if (generatorComponents == null)
            generatorComponents = new List<GenerateComponent>();
        var collect = GetComponentsInChildren<GenerateComponent>();
        for (int i = 0; i < collect.Length; i++)
        {
            if (!generatorComponents.Contains(collect[i]))
            {
                generatorComponents.Add(collect[i]);
            }
        }
        if (mirrors == null)
            mirrors = new List<MirrorTransform>();
        var collectMirrors = GetComponentsInChildren<MirrorTransform>();
        for (int i = 0; i < collectMirrors.Length; i++)
        {
            if (!mirrors.Contains(collectMirrors[i]))
            {
                mirrors.Add(collectMirrors[i]);
            }
        }

    }
}
