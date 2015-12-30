using UnityEngine;
using System.Collections;
using System;
using Parse;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

public class NewsManagement : MonoBehaviour {

    public GameObject listEntry;
    public RectTransform contentPanel;
    public Sprite template;

    public const string FILENAME_NEWS_PIC = "_news_picture.jpg";
    // Use this for initialization
    void Start () {
        //clearList();
        StartCoroutine(updateNews());
	}

    void OnEnable()
    {
        EventManager.onMenuStateChanged += menuStateChanged;
    }

    void OnDisable()
    {
        EventManager.onMenuStateChanged -= menuStateChanged;
    }

    private void menuStateChanged(MenuState menuState)
    {
        if (menuState == MenuState.news)
        {
            StartCoroutine(updateNews());
        }
    }

    private void clearList()
    {
        foreach (RectTransform listEntry in contentPanel)
        {
            Destroy(listEntry.gameObject);
        }
    }

    IEnumerator updateNews()
    {
        IEnumerable<ParseObject> news = null;
        ParseQuery<ParseObject> query = new ParseQuery<ParseObject>("News").OrderByDescending("release").WhereLessThanOrEqualTo("release",DateTime.Now);
        Task task = query.FindAsync().ContinueWith(t =>
        {
            news = t.Result;
        });
        while (!task.IsCompleted) yield return null;
        int count = 0;
        foreach (ParseObject topic in news)
        {
            count++;
        }
        if (count != contentPanel.childCount)
        {
            clearList();
            foreach (ParseObject topic in news)
            {
                GameObject newsEntry = Instantiate(listEntry) as GameObject;
                newsEntry.GetComponent<RectTransform>().SetParent(contentPanel, false);
                foreach (RectTransform item in newsEntry.GetComponent<RectTransform>())
                {
                    if (item.gameObject.name.Equals("Image"))
                    {
                        foreach (RectTransform subItem in item.GetComponent<RectTransform>())
                        {
                            if (subItem.gameObject.name.Equals("Image"))
                            {
                                if (topic.ContainsKey("image"))
                                    StartCoroutine(loadImage(subItem.GetComponent<Image>(), topic));
                            }
                        }
                    }
                    if (item.gameObject.name.Equals("Text"))
                    {
                        foreach (RectTransform subItem in item.GetComponent<RectTransform>())
                        {
                            if (subItem.gameObject.name.Equals("Headline"))
                            {
                                subItem.GetComponent<Text>().text = topic.Get<string>("headline");
                            }
                            if (subItem.gameObject.name.Equals("Text"))
                            {
                                subItem.GetComponent<Text>().text = topic.Get<string>("text");
                            }
                        }

                    }
                }
            }
        }
    }

    IEnumerator loadImage(Image imageComponent, ParseObject news)
    {
        Sprite image = template;
        ParseFile imageObject = news.Get<ParseFile>("image");
        string path = Application.persistentDataPath + "/" + news.ObjectId + FILENAME_NEWS_PIC;
        if (imageObject != null)
        {
            if (File.Exists(path))
            {
                var fileData = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                image = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                var pictureRequest = new WWW(imageObject.Url.AbsoluteUri);
                yield return pictureRequest;
                byte[] fileBytes = pictureRequest.texture.EncodeToJPG(25);
                File.WriteAllBytes(path, fileBytes);
                image = Sprite.Create(pictureRequest.texture, new Rect(0, 0, pictureRequest.texture.width, pictureRequest.texture.height), new Vector2(0.5f, 0.5f));
            }
        }
        imageComponent.overrideSprite = image;
    }

    // Update is called once per frame
    void Update () {
	    
	}
}
