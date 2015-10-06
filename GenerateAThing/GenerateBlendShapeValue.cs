using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerateBlendShapeValue:GenerateComponent
{
    public string shapeName;

    public float min;
    public float max = 1;

    bool useValueFromParent;

    [Header("exposed")]
    [Tooltip("you could assign this to a target on another object")]
    SkinnedMeshRenderer skin;

    void Awake()
    {
        if(skin == null)
            skin = GetComponent<SkinnedMeshRenderer>();
    }

    public override void Generate(GenerateAThing owner = null)
    {
        if (skin != null)
        {
            if (functionActive == ActiveSetting.On)
            {
                Mesh blendMesh = skin.sharedMesh;
                int blendShapeCount = blendMesh.blendShapeCount;
                float value = Random.Range(min, max);
                if (useValueFromParent)
                {
                    print("implement value from parent");
                }

                bool found = false;
                for (int i = 0; i < blendShapeCount; i++)
                {
                    if (blendMesh.GetBlendShapeName(i) == shapeName)
                    {
                        skin.SetBlendShapeWeight(i, value);
                        found = true;
                    }
                }
                if (!found)
                {
                    print("blend shape not found in model");
                }
            }
        }
        else
        {
            print("no skin to set blend shape on");
        }
    }
}
