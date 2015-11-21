using UnityEngine;
using System.Collections.Generic;

public class AppManagement : MonoBehaviour {

    public List<GameObject> searchObjects;
    public List<GameObject> profileObjects;

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
        foreach (GameObject go in profileObjects)
        {
            go.SetActive(menuState == MenuState.profile);
        }
        foreach (GameObject go in searchObjects)
        {
            go.SetActive(menuState == MenuState.search);
        }
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
