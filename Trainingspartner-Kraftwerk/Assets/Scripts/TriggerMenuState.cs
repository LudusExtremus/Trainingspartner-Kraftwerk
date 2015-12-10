using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Parse;

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
        if (GetComponent<UserEntry>() != null)
        {
            if (GetComponent<UserEntry>().getUser() != null)
            {
                if ((menuState == MenuState.messages))
                {
                    ParseUser user = GetComponent<UserEntry>().getUser();
                    GetComponent<UserEntry>().setUser(null);
                    EventManager.addConversation(user);
                    return;
                }
                if ((menuState == MenuState.create_message))
                {
                    ParseUser user = GetComponent<UserEntry>().getUser();
                    //GetComponent<UserEntry>().setUser(null);
                    EventManager.enterConversation(user);
                }
            }
        }
        EventManager.changeMenuState(menuState);
    }

    
}
