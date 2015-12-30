using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TutorialHandler : MonoBehaviour {

    public List<Sprite> tutorialImages;

    private Image currentImage;
    private int count = 0;
    // Use this for initialization
    void Start () {
        foreach(Transform t in GetComponent<RectTransform>())
        {
            if (t.name.Equals("Image"))
            {
                currentImage = t.GetComponent<Image>();
                currentImage.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, 1720);
                currentImage.overrideSprite = tutorialImages[0];
            }
        }
        //GetComponent<RectTransform>().position = new Vector2(540,960);
    }
	
    public void setNextImage()
    {
        if (++count >= tutorialImages.Count)
        {
            PlayerPrefs.SetInt("tutorial_viewed", 1);
            EventManager.changeMenuState(MenuState.info);
        } else
        {
            currentImage.overrideSprite = tutorialImages[count];
        }
    }
    public void setPrevImage()
    {
        if (count - 1 >= 0)
        {
            currentImage.overrideSprite = tutorialImages[--count];
        }
    }
    // Update is called once per frame
    void Update () {
	    
	}

    private Texture2D resizeTexture(Texture2D tex, int size)
    {
        int width = tex.width;
        int height = tex.height;
        if ((width == 0) || (height == 0))
        {
            return null;
        }
        if (width > size)
        {
            width = size;
            height = (width * tex.height) / tex.width;
        }
        if (height > size)
        {
            height = size;
            width = (height * tex.width) / tex.height;
        }
        return (ScaleTexture(tex, width, height));
    }
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
        float incX = (1.0f / (float)targetWidth);
        float incY = (1.0f / (float)targetHeight);
        for (int i = 0; i < result.height; ++i)
        {
            for (int j = 0; j < result.width; ++j)
            {
                Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                result.SetPixel(j, i, newColor);
            }
        }
        result.Apply();
        return result;
    }
}
