using UnityEngine;
using Parse;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System;
using UnityEngine.UI;
using System.IO;

public class UserSearch : MonoBehaviour {

    public const string FILENAME_PROFILE_PIC = "_profile_picture.png";

    private List<string> selectedCategories = new List<string>();
    private List<string> selectedTimes = new List<string>();

    public int timeSpanDaysUserNotActive = 365;

    public RectTransform categoriesPanel;
    public RectTransform timesPanel;
    public Sprite anonymous;

    public GameObject userSearchNotification;

    private string currentSortValue = UserValues.LAST_LOGIN;
    public Order sortOrder = Order.descending;

    public bool allowMultipleTimesAndCategories = false;

    private List<GameObject> currentListEntries = new List<GameObject>();
    // Prefab einer NewsEntry
    public GameObject listEntry;
    // Dies ist das Prefab in dem die children später erzeugt werden
    public Transform contentPanel;
  
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void queryUsers()
    {
        if (selectedCategories.Count == 0)
        {
            Debug.Log("SELECT CATEGORIES!");
            clearList();
            return;
        }
        if (selectedTimes.Count == 0)
        {
            Debug.Log("SELECT TIMES!");
            clearList();
            return;
        }
        userSearchNotification.SetActive(true);
        StartCoroutine(searchAsync());
    }

    IEnumerator searchAsync()
    {
       List<ParseUser> users = new List<ParseUser>();
       ParseQuery<ParseObject> query = buildTimeTableQuery(selectedTimes);
        Task task = query.FindAsync().ContinueWith(t =>
         {
         IEnumerable<ParseObject> timeTables = t.Result;
         foreach (var userTimeTable in timeTables)
         {
            ParseUser user = userTimeTable.Get<ParseUser>("user");
            bool userSetActive = (bool)user[UserValues.ACTIVE];

                 DateTime lastActiveDate = (DateTime)user[UserValues.LAST_LOGIN];
                 TimeSpan span = DateTime.Now - lastActiveDate;
                 int diffInDays = span.Days;
                 bool userIsActiveByLogin = diffInDays <= timeSpanDaysUserNotActive;

            if ((user.ObjectId == ParseUser.CurrentUser.ObjectId) || (!userSetActive)|| (!userIsActiveByLogin))
                continue;
            List<string> cats = user.Get<List<object>>(UserValues.CATEGORIES).Select(s => (string)s).ToList();
            bool userContainsAnyCategory = cats.Any(s => selectedCategories.Contains(s));
            if (userContainsAnyCategory)
                    users.Add(user);
            }
        });
        while (!task.IsCompleted) yield return null;
        clearList();
        if (users.Count > 0)
        {
            SortBy sortOrder = new SortBy(currentSortValue,Order.descending);
            users.Sort(sortOrder);
            populateList(users);
        }
        userSearchNotification.SetActive(false);
    }

    private class SortBy : IComparer<ParseUser>
    {
        private string sortValue;
        private Order ascDes;
        public SortBy(string sortValue, Order ascDes)
        {
            this.sortValue = sortValue;
            this.ascDes = ascDes;
        }
        public int Compare(ParseUser user1, ParseUser user2)
        {
            Type type = user1[sortValue].GetType();
            int comp = 0;
            if (type == typeof(string))
            {
                comp = string.Compare((string)user1[sortValue], (string)user2[sortValue]);
            }
            if (type == typeof(int))
            {
                if ((int)user1[sortValue] > (int)user2[sortValue])
                    comp = 1;
                if ((int)user1[sortValue] < (int)user2[sortValue])
                    comp = -1;
            }
            if (type == typeof(DateTime))
            {
                comp = DateTime.Compare((DateTime)user1[sortValue], (DateTime)user2[sortValue]);
            }
            if (ascDes == Order.descending)
                comp *= -1;
            return comp;
        }
    }

    private void clearList()
    {
        foreach (GameObject listEntry in currentListEntries)
        {
            Destroy(listEntry);
        }
        currentListEntries.Clear();
    }

    private void populateList(List<ParseUser> users)
    {
        foreach (ParseUser user in users)
        {
            GameObject userEntry = Instantiate(listEntry) as GameObject;
            userEntry.GetComponent<RectTransform>().SetParent(contentPanel,false);
            userEntry.GetComponent<UserEntry>().setUser(user);
            foreach (RectTransform panel in userEntry.GetComponent<RectTransform>())
            {
                if (panel.gameObject.name.Equals("UserImage"))
                {
                    foreach (RectTransform item in panel)
                    {
                        if (item.gameObject.name.Equals("Image"))
                        {
                            StartCoroutine(setUserPicture(user, user.Get<ParseFile>(UserValues.PICTURE), item.GetComponent<Image>()));
                        }
                    }
                }
                if (panel.gameObject.name.Equals("UserData"))
                {
                    foreach (RectTransform item in panel)
                    {
                        if (item.gameObject.name.Equals("Username"))
                        {
                            item.GetComponent<Text>().text = user.Get<string>(UserValues.NICK);
                        }
                        if (item.gameObject.name.Equals("SportLevel"))
                        {
                            item.GetComponent<Text>().text = user.Get<string>("climbingGrade");
                        }
                        if (item.gameObject.name.Equals("SportSince"))
                        {
                            item.GetComponent<Text>().text = user.Get<string>(UserValues.START_DATE);
                        }
                        if (item.gameObject.name.Equals("weight"))
                        {
                            item.GetComponent<Text>().text = user.Get<string>(UserValues.ABOUT);
                        }
                    }
                }
            }
            currentListEntries.Add(userEntry);
        }
    }

    IEnumerator setUserPicture(ParseUser user, ParseFile pictureFile, Image userImage)
    {
        Sprite image = anonymous;
        string path = Application.persistentDataPath + "/" + user.ObjectId + FILENAME_PROFILE_PIC;
        bool updateExistingProfilePic = false;
        if (File.Exists(path))
        {
            updateExistingProfilePic = DateTime.Compare(File.GetLastWriteTime(path), user.UpdatedAt.Value) < 0;
        }
        if (pictureFile != null)
        {
            if ((File.Exists(path))&&(!updateExistingProfilePic))
            {
                var fileData = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                image = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                Debug.Log("User Open file");
            }
            else
            {
                var pictureRequest = new WWW(pictureFile.Url.AbsoluteUri);
                yield return pictureRequest;
                byte[] fileBytes = pictureRequest.texture.EncodeToJPG(25);
                File.WriteAllBytes(path, fileBytes);
                image = Sprite.Create(pictureRequest.texture, new Rect(0, 0, pictureRequest.texture.width, pictureRequest.texture.height), new Vector2(0.5f, 0.5f));
                Debug.Log("User Download file");
            }
        }
        if(!userImage.IsDestroyed())
            userImage.overrideSprite = image;
    }


    private ParseQuery<ParseObject> buildTimeTableQuery(List<string> times)
    {
        ParseQuery<ParseObject> query = new ParseQuery<ParseObject>("Timetable").WhereEqualTo(times[0], true);
        for (int i = 1; i < times.Count; i++)
        {
            query = query.Or(new ParseQuery<ParseObject>("Timetable").WhereEqualTo(times[i], true));
        }
        query = query.Include("user");
        return query;
    }

    private void updateSearchOptions(List<string> list, string value, bool active)
    {
        if (!allowMultipleTimesAndCategories)
        {
            if (!active)
                return;
            list.Clear();
            if (list == selectedCategories)
            {
                foreach(RectTransform cat in categoriesPanel)
                {
                    if(!cat.gameObject.name.Equals(value))
                        cat.GetComponent<Toggle>().isOn = false;
                }
            }
            if (list == selectedTimes)
            {
                foreach (RectTransform time in timesPanel)
                {
                    if (!time.gameObject.name.Equals(value))
                        time.GetComponent<Toggle>().isOn = false;
                }
            }
        }

        if (active)
        {
            if (!list.Contains(value))
            {
                list.Add(value);
            }
            else
                return;
        }
        else
        {
            if (list.Contains(value))
            {
                list.Remove(value);
            }
            else
                return;
        }
        queryUsers();
    }

    public void selectUserRopeClimb(bool active)
    {
        updateSearchOptions(selectedCategories, "ropeClimb", active);
    }
    public void selectUserBoulder(bool active)
    {
        updateSearchOptions(selectedCategories, "boulder", active);
    }

    public void updateUserTimeMon_Mor(bool active)
    {
        updateSearchOptions(selectedTimes,"Mon_Mor", active);
    }
    public void updateUserTimeMon_Eve(bool active)
    {
        updateSearchOptions(selectedTimes, "Mon_Eve", active);
    }
    public void updateUserTimeMon_Noon(bool active)
    {
        updateSearchOptions(selectedTimes, "Mon_Noon", active);
    }
    public void updateUserTimeTue_Mor(bool active)
    {
        updateSearchOptions(selectedTimes, "Tue_Mor", active);
    }
    public void updateUserTimeTue_Eve(bool active)
    {
        updateSearchOptions(selectedTimes, "Tue_Eve", active);
    }
    public void updateUserTimeTue_Noon(bool active)
    {
        updateSearchOptions(selectedTimes, "Tue_Noon", active);
    }
    public void updateUserTimeWed_Mor(bool active)
    {
        updateSearchOptions(selectedTimes, "Wed_Mor", active);
    }
    public void updateUserTimeWed_Eve(bool active)
    {
        updateSearchOptions(selectedTimes, "Wed_Eve", active);
    }
    public void updateUserTimeWed_Noon(bool active)
    {
        updateSearchOptions(selectedTimes, "Wed_Noon", active);
    }
    public void updateUserTimeThu_Mor(bool active)
    {
        updateSearchOptions(selectedTimes, "Thu_Mor", active);
    }
    public void updateUserTimeThu_Eve(bool active)
    {
        updateSearchOptions(selectedTimes, "Thu_Eve", active);
    }
    public void updateUserTimeThu_Noon(bool active)
    {
        updateSearchOptions(selectedTimes, "Thu_Noon", active);
    }
    public void updateUserTimeFri_Mor(bool active)
    {
        updateSearchOptions(selectedTimes, "Fri_Mor", active);
    }
    public void updateUserTimeFri_Eve(bool active)
    {
        updateSearchOptions(selectedTimes, "Fri_Eve", active);
    }
    public void updateUserTimeFri_Noon(bool active)
    {
        updateSearchOptions(selectedTimes, "Fri_Noon", active);
    }
    public void updateUserTimeSat_Mor(bool active)
    {
        updateSearchOptions(selectedTimes, "Sat_Mor", active);
    }
    public void updateUserTimeSat_Eve(bool active)
    {
        updateSearchOptions(selectedTimes, "Sat_Eve", active);
    }
    public void updateUserTimeSat_Noon(bool active)
    {
        updateSearchOptions(selectedTimes, "Sat_Noon", active);
    }
    public void updateUserTimeSun_Mor(bool active)
    {
        updateSearchOptions(selectedTimes, "Sun_Mor", active);
    }
    public void updateUserTimeSun_Eve(bool active)
    {
        updateSearchOptions(selectedTimes, "Sun_Eve", active);
    }
    public void updateUserTimeSun_Noon(bool active)
    {
        updateSearchOptions(selectedTimes, "Sun_Noon", active);
    }

}
