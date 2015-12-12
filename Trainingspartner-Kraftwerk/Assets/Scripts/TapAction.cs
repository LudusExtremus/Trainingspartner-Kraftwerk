using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class TapAction : MonoBehaviour {

    [System.Serializable]
    public class tapEvent : UnityEvent { };
    [SerializeField]
    public tapEvent onChangeEvent;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            onChangeEvent.Invoke();
        }
    }



}
