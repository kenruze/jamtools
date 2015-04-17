using UnityEngine;
using System.Collections;

public class Circle : MonoBehaviour
{
    [ContextMenuItem("Refresh", "Refresh")]
    public int segments;
    public float xradius;
    public float yradius;
    public float height;
    public bool onXZPlane;
    LineRenderer line;

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        line = gameObject.GetComponent<LineRenderer>();
        line.SetVertexCount(segments + 1);
        line.useWorldSpace = false;
        CreatePoints();
    }

    void CreatePoints()
    {
        float x;
        float y = onXZPlane ? height : 0;
        float z = onXZPlane ? 0 : height;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            if (onXZPlane)
            {
                z = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;
            }
            else
            {
                y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;
            }

            line.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / segments);
        }
    }
}