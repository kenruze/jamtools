using UnityEngine;
using System.Collections;

public class GizmoComponent : MonoBehaviour
{
    [Header ("Set alpha to use colour")]
    public Color colour;
    public Mesh mesh;
    public bool wire;
    [Header ("The image file should be placed in the Assets/Gizmos folder")]
    public string iconName;

    void OnDrawGizmos ()
    {
        if (colour.a > 0)
            Gizmos.color = colour;
        
        if (iconName != "")
        {
            Gizmos.DrawIcon (transform.position, iconName, true);
        }
        if (mesh != null)
        {
            if (wire)
                Gizmos.DrawWireMesh (mesh, transform.position, transform.rotation, transform.transform.lossyScale);
            else
                Gizmos.DrawMesh (mesh, transform.position, transform.rotation, transform.transform.lossyScale);
        }
    }
}
