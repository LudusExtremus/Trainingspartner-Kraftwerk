using UnityEngine;
using System.Collections;

public class ClearCache : MonoBehaviour {

	// Use this for initialization
	void Start () {

        Caching.CleanCache();
        Debug.Log("Cleared cache most likely");

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
