using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using Parse;

public class AppManagement : MonoBehaviour {

    public List<GameObject> searchObjects;
    public List<GameObject> profileObjects;
    public List<GameObject> infoObjects;
    public List<GameObject> messagesObjects;
    public List<GameObject> createMessageObjects;
    public List<GameObject> imageGalleryObjects;
    public List<GameObject> feedbackObjects;
    public List<GameObject> newsObjects;
    public List<GameObject> tutorialObjects;

    public GameObject topNav;
    public GameObject messagesNav;
    public GameObject searchNav;

    public List<MenuState> topStates;
    public List<MenuState> messagesStates;
    public List<MenuState> searchStates;

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
            menuState = lastTopState;

        currentMenuState = menuState;

        if (!topStates.Contains(menuState))
        {
            topNav.SetActive(false);
        } else
        {
            lastTopState = menuState;
            topNav.SetActive(true);
        }

        messagesNav.SetActive(messagesStates.Contains(menuState));
        searchNav.SetActive(searchStates.Contains(menuState));

        foreach (GameObject go in profileObjects)
            go.SetActive(menuState == MenuState.profile);
        foreach (GameObject go in searchObjects)
            go.SetActive(menuState == MenuState.search);
        foreach (GameObject go in infoObjects)
            go.SetActive(menuState == MenuState.info);
        foreach (GameObject go in messagesObjects)
            go.SetActive(menuState == MenuState.messages);
        foreach (GameObject go in createMessageObjects)
            go.SetActive(menuState == MenuState.create_message);
        foreach (GameObject go in imageGalleryObjects)
            go.SetActive(menuState == MenuState.image_galery);
        foreach (GameObject go in feedbackObjects)
            go.SetActive(menuState == MenuState.feedback);
        foreach (GameObject go in newsObjects)
            go.SetActive(menuState == MenuState.news);
        foreach (GameObject go in tutorialObjects)
            go.SetActive(menuState == MenuState.tutorial);
    }

    // Use this for initialization
    void Start () {
        bool showTutorial = true;
        if (PlayerPrefs.HasKey("tutorial_viewed"))
        {
            showTutorial = PlayerPrefs.GetInt("tutorial_viewed") == 0;
        }
        if (showTutorial)
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
