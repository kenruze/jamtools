using UnityEngine;
using System.Collections;

public class PlaySpeed : MonoBehaviour {

    [Range(0,3)]
    public float timeScale=1;

	// Use this for initialization
	void Start () {
        Time.timeScale = timeScale;
	}
	
	// Update is called once per frame
	void Update () {
        Time.timeScale = timeScale;
	}
}
