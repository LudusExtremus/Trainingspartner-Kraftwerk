using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using Parse;

public class AppManagement : MonoBehaviour {

    public bool displayTutorial = false;
    
    [Serializable]
    public struct StateObject
    {
        public MenuState name;
        public List<GameObject> objects;
    }
    public StateObject[] stateObjects;

    public GameObject topNav;
    public GameObject otherNav;
    public GameObject searchNav;
    public GameObject createMsgNav;

    public List<MenuState> topStates;
    public List<MenuState> otherStates;
    public List<MenuState> searchStates;
    public List<MenuState> createMsgStates;

    public GameObject notificationNoInternet;

    private MenuState lastTopState = MenuState.info;
    private MenuState currentMenuState = MenuState.info;

    void Awake()
    {
        ParsePush.ParsePushNotificationReceived += (sender, args) => {
#if UNITY_ANDROID
            AndroidJavaClass parseUnityHelper = new AndroidJavaClass("com.parse.ParseUnityHelper");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            // Call default behavior.
            parseUnityHelper.CallStatic("handleParsePushNotificationReceived", currentActivity, args.StringPayload);
#endif
        };

        ParsePush.ParsePushNotificationReceived += (sender, args) => {
            var payload = args.Payload;
            object userid;
            if (payload.TryGetValue("user", out userid))
            {
                GetComponent<Messaging>().setPartner(userid as string);
            }
        };
    }

    void OnEnable()
    {
        EventManager.onMenuStateChanged += changeMenuState;
    }
    void OnDisable()
    {
        EventManager.onMenuStateChanged -= changeMenuState;
    }

    private void changeMenuState(MenuState menuState)
    {
        if (menuState == MenuState.back)
        {
            if (currentMenuState == MenuState.create_message)
            {
                menuState = MenuState.messages;
            } else
            {
                menuState = lastTopState;
            }
        }
            
        currentMenuState = menuState;

        if (!topStates.Contains(menuState))
        {
            topNav.SetActive(false);
        } else
        {
            lastTopState = menuState;
            topNav.SetActive(true);
        }

        otherNav.SetActive(otherStates.Contains(menuState));
        searchNav.SetActive(searchStates.Contains(menuState));
        createMsgNav.SetActive(createMsgStates.Contains(menuState));

        foreach(StateObject so in stateObjects)
        {
            foreach(GameObject go in so.objects)
            {
                go.SetActive(menuState == so.name);
            }
        }

    }

    // Use this for initialization
    void Start () {
        bool showTutorial = true;
        if (PlayerPrefs.HasKey("tutorial_viewed"))
        {
            showTutorial = PlayerPrefs.GetInt("tutorial_viewed") == 0;
        }
        if ((showTutorial)||(displayTutorial))
        {
            currentMenuState = MenuState.tutorial;
        } else
        {
            currentMenuState = MenuState.info;
        }
            
        changeMenuState(currentMenuState);
        StartCoroutine(checkInternetConnection((isConnected) => {
            if (!isConnected)
            {
                showConnectionError();
            }
            else
            {
                EventManager.appInitFinished = true;
                Debug.Log("connection success");
            }
        }));
        
	}

    public void restartApp()
    {
        Debug.Log("RESTART ...");
        notificationNoInternet.SetActive(false);
        Application.LoadLevel(0);
    }

    private void showConnectionError()
    {
        Debug.Log("connection failed");
        notificationNoInternet.SetActive(true);
        // TODO Show full screen error message
        // if connected click on fullscreen error message to -> Application.LoadLevel(0);
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (topStates.Contains(currentMenuState))
            {
                Application.Quit();
            } else
            {
                changeMenuState(MenuState.back);
            }
        }
    }

    IEnumerator checkInternetConnection(Action<bool> action)
    {
        WWW www = new WWW("http://google.com");
        yield return www;
        if (www.error != null)
        {
            action(false);
        }
        else
        {
            action(true);
        }
    }
}
