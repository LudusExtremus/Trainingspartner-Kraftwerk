using UnityEngine;
using Parse;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System;
using UnityEngine.UI;

public class UserSearch : MonoBehaviour {

    private List<string> selectedCategories = new List<string>();
    private List<string> selectedTimes = new List<string>();

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
                List<string> cats = user.Get<List<object>>("categories").Select(s => (string)s).ToList();
                bool userContainsAnyCategory = cats.Any(s => selectedCategories.Contains(s));
                if (userContainsAnyCategory)
                    users.Add(user);
            }
        });
        while (!task.IsCompleted) yield return null;
        clearList();
        if (users.Count > 0)
        {
            populateList(users);
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
            GameObject newListEntry = Instantiate(listEntry) as GameObject;
            NewsEntryButton newsEntryButton = newListEntry.GetComponent<NewsEntryButton>();
            newsEntryButton.textTop.text = (string)user["nick"];
            newsEntryButton.textMid.text = (string)user["about"];
            newsEntryButton.textBot.text = (string)user["startDate"];
            newListEntry.transform.SetParent(contentPanel);
            currentListEntries.Add(newListEntry);
        }
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
        foreach(string s in list)
            Debug.Log("" + s);
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
