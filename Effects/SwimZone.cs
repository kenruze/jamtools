using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JamTools;

public partial class SwimZone : MonoBehaviour
{
    public string stateBool = "Swim";

    void OnTriggerEnter(Collider other)
    {
        var birb = other.gameObject.GetComponent<BirbCharacterAbilities>();

        if (birb != null)
        {
            birb.GetComponent<CharacterPuppet>().animator.SetBool(stateBool, true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var birb = other.gameObject.GetComponent<BirbCharacterAbilities>();

        if (birb != null)
        {
            birb.GetComponent<CharacterPuppet>().animator.SetBool(stateBool, false);
        }
    }
}

