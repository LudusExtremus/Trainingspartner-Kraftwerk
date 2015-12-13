using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class SingleToggleChange : MonoBehaviour {

    private List<Toggle> toggles = new List<Toggle>();
    private bool lockToggle = false;
	// Use this for initialization
	void Start () {
	    foreach(RectTransform rt in GetComponent<RectTransform>())
        {
            Toggle t = rt.gameObject.GetComponent<Toggle>();
            t.onValueChanged.AddListener((active) => {
                if (!active)
                    return;
                foreach(Toggle tog in toggles)
                {
                    if(tog!=t)
                        tog.isOn = false;
                }
                }  
            );  
            toggles.Add(t);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
