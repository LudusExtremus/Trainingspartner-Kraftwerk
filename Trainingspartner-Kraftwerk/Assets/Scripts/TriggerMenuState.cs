using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Parse;

public class TriggerMenuState : MonoBehaviour {

    public MenuState myMenuState;

    public GameObject appManager;
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
                if ((menuState == MenuState.create_message))
                {
                    ParseUser user = GetComponent<UserEntry>().getUser();
                    //GetComponent<UserEntry>().setUser(null);
                    EventManager.enterConversation(user);
                }
            }
        }
        if (menuState == MenuState.messages)
        {
            if (!appManager.GetComponent<Messaging>().hasPartners())
            {
                appManager.GetComponent<Messaging>().showNoPartnersNotification();
                return;
            }
        }
        if(menuState == MenuState.search)
        {
            if (!appManager.GetComponent<UserManagement>().userHasSufficientProfileInformation())
            {
                appManager.GetComponent<UserManagement>().showProfileInformationNotification();
                menuState = MenuState.profile;
            }
        }
        EventManager.changeMenuState(menuState);
    }

    
}
