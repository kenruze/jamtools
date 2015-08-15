using UnityEngine;
using System.Collections;

public class fixPaperDoll : MonoBehaviour {

    public SkinnedMeshRenderer skin;
    public bool allowDestorySkins = false;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    [ContextMenu("DoPaperDoll")]
    public void DoPaperDoll()
    {
        DoPaperDoll(skin);
    }
    public void DoPaperDoll(SkinnedMeshRenderer skin = null, bool destructive = false)
    {
        string suffix = "_PD";
        string pieceName = skin.gameObject.name + suffix;
        Debug.Log(skin.sharedMesh.boneWeights.Length);
        bool doPaperdoll = true;
        int firstWeightIndex = skin.sharedMesh.boneWeights[0].boneIndex0;
        foreach(var weight in skin.sharedMesh.boneWeights)
        {
            if(weight.boneIndex0 != firstWeightIndex || weight.weight1 != 0)
            {
                doPaperdoll = false;
                break;
            }
        }
        print(skin.name + " will do paper doll: "+doPaperdoll);
        if(doPaperdoll)
        {
            var bone = skin.bones[firstWeightIndex];

            GameObject clone = new GameObject(pieceName);

            clone.transform.SetParent(bone);
            clone.transform.localPosition = skin.transform.localPosition;
            clone.transform.localRotation = skin.transform.localRotation;
			SetParentMaintainLocals(clone.transform, skin.rootBone.parent);
            clone.transform.SetParent(skin.bones[firstWeightIndex]);

            clone.AddComponent<MeshFilter>().mesh = skin.sharedMesh;
            clone.AddComponent<MeshRenderer>().material = skin.sharedMaterial;

            if(destructive)
            {
                if(allowDestorySkins)
                    DestroyImmediate(skin.gameObject);
                else
                    print("was not allowed to destroy skin");
            }
        }
    }

    [ContextMenu("Paper Doll All Children")]
    public void PaperDollAllChildren()
    {  
        PaperDollAllChildren(gameObject);
    }
    public void PaperDollAllChildren(GameObject root = null)
        {

        var skins = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach(var skin in skins)
        {
            DoPaperDoll(skin, true);
        }

    }

	public static void SetParentMaintainLocals(Transform t, Transform parent)
	{
		var p = t.localPosition;
		var r = t.localRotation;
		var s = t.localScale;

		t.parent = parent;

		t.localPosition = p;
		t.localRotation = r;
		t.localScale = s;
	}
}
