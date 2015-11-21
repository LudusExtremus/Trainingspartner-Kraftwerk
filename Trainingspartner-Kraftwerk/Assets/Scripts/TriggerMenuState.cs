using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TriggerMenuState : MonoBehaviour {

    public MenuState myMenuState;

	// Use this for initialization
	void Start () {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            triggerState(myMenuState);
        });
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void triggerState(MenuState menuState)
    {
        EventManager.changeMenuState(menuState);
    }
}
