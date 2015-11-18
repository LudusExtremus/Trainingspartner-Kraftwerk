using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

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

}
public class CreateScrollList : MonoBehaviour {

    public GameObject newsEntry;
    public List<Item> itemList;

    public Transform contentPanel;

	void Start () {
        PopulateList();
	}

    void PopulateList() {
        foreach (var item in itemList)
        {
            GameObject newNewsEntry = Instantiate(newsEntry) as GameObject;
            NewsEntryButton newsEntryButton = newNewsEntry.GetComponent<NewsEntryButton>();
            newsEntryButton.newsIcon.sprite = item.icon;
            newsEntryButton.buttonImageTop.sprite = item.buttonImageTop;
            newsEntryButton.buttonImageMid.sprite = item.buttonImageMid;
            newsEntryButton.buttonImageBot.sprite = item.buttonImageBot;
            newsEntryButton.textTop.text = item.textTop;
            newsEntryButton.textMid.text = item.textMid;
            newsEntryButton.textBot.text = item.textBot;
            newsEntryButton.championIcon.SetActive(item.isChampion);
            newNewsEntry.transform.SetParent(contentPanel);
        }

    }
}
