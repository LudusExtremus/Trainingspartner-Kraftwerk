using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

// System.serializable bedeutet, dass diese Items im Editor angezeigt werden 
[System.Serializable]
public class Item {
    
    public Sprite icon;
    public Sprite buttonImageTop;
    public Sprite buttonImageMid;
    public Sprite buttonImageBot;
    public string textTop;
    public string textMid;
    public string textBot;
    public bool isChampion;
    public Button.ButtonClickedEvent thingToDo;
}
public class CreateScrollList : MonoBehaviour {

    // Prefab einer NewsEntry

    public GameObject newsEntry;

    // Liste vom Typ Item (siehe Klasse oben)

    public List<Item> itemList;

    // Dies ist das Prefab in dem die children später erzeugt werden

    public Transform contentPanel;

   

	void Start () {
        
        //Methodenaufruf

        PopulateList();
	}

    void PopulateList() {
        foreach (var item in itemList)
        {
           // Instantiierung des Prefabs "newsEntry"  

            GameObject newNewsEntry = Instantiate(newsEntry) as GameObject;

            // Link zu den Variablen des Prefabs
             
            NewsEntryButton newsEntryButton = newNewsEntry.GetComponent<NewsEntryButton>();
            
            // Übergabe aller Links vom Prefab an jedes einzelne Listenitem. 

            newsEntryButton.newsIcon.sprite = item.icon;
            newsEntryButton.buttonImageTop.sprite = item.buttonImageTop;
            newsEntryButton.buttonImageMid.sprite = item.buttonImageMid;
            newsEntryButton.buttonImageBot.sprite = item.buttonImageBot;
            newsEntryButton.textTop.text = item.textTop;
            newsEntryButton.textMid.text = item.textMid;
            newsEntryButton.textBot.text = item.textBot;

            // isChampion kann rausgeschmissen werden - Außer wir benötigen einen bool für irgendetwas?
            newsEntryButton.championIcon.SetActive(item.isChampion);
            
            // Prefab wird an das contentPanel geparented.
            newNewsEntry.transform.SetParent(contentPanel);
        }

    }

 
}
