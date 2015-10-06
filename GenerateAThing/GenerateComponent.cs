using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerateComponent:MonoBehaviour
{
    public ActiveSetting functionActive;

    public virtual void Generate(GenerateAThing owner = null)
    {
        print("empty generate");
    }

    public enum ActiveSetting
    {
        On,
        Hold,
        Disable,
    }
}
