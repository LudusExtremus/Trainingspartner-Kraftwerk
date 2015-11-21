using UnityEngine;
using UnityEngine.UI;
using Parse;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class UserManagement : MonoBehaviour {

    public GameObject userNickInput;
    public GameObject userClimbGradeInput;
    public GameObject userTrainingStartInput;
    public GameObject userAboutInput;

    public GameObject categoryBoulderToggle;
    public GameObject categoryRopeClimbToggle;

    public GameObject timeTogglePanel;

    // Dictionary<string, bool> timeTable = new Dictionary<string, bool>();
    //List<string> times = new List<string>() { "Mon_Mor", "Mon_Eve", "Mon_Noon", "Tue_Mor", "Tue_Eve", "Tue_Noon", "Wed_Mor", "Wed_Eve", "Wed_Noon", "Thu_Mor", "Thu_Eve", "Thu_Noon", "Fri_Mor", "Fri_Eve", "Fri_Noon", "Sat_Mor", "Sat_Eve", "Sat_Noon", "Sun_Mor", "Sun_Eve", "Sun_Noon" };
    private List<string> categories = new List<String>{"boulder"}; // selected user categories

    public Image userImage;
    // Use this for initialization
    void Start () {
        //queryTimeTable();
        /*
        Dictionary<string, object> time = ParseUser.CurrentUser.Get<Dictionary<string, object>>("timetable");
        foreach (var key in time.Keys)
        {
            Debug.Log("Key: " + key + " Value: " + time[key].ToString());
        }
        
        if (ParseUser.CurrentUser != null)
        {
            userNickInput.GetComponent<InputField>().text = (String)ParseUser.CurrentUser["nick"];
            userTrainingStartInput.GetComponent<InputField>().text = (String)ParseUser.CurrentUser["startDate"];
            userAboutInput.GetComponent<InputField>().text = (String)ParseUser.CurrentUser["about"];
        };
        
        timeTable.Add("Monday_Morning",true);
        timeTable.Add("Monday_Noon", false);
        timeTable.Add("Monday_Evening", true);
        */

        //updateTimeTable(times);
        //queryTimeTable();
        updateProfileUI(ParseUser.CurrentUser);
    }
	
	// Update is called once per frame
	void Update () {
        
    } 

    public void queryTimeTable()
    {
        ParseQuery<ParseObject> query = buildTimeTableQuery(new List<string>() { "Mon_Mor", "Tue_Eve" });
        query.FindAsync().ContinueWith(t =>
        {
            IEnumerable<ParseObject> timeTables = t.Result;
            foreach (var userTimeTable in timeTables)
            {
                ParseUser user = userTimeTable.Get<ParseUser>("user");
                List<string> cats = user.Get<List<object>>("categories").Select(s => (string)s).ToList();
                bool userContainsAnyCategory = cats.Any(s => categories.Contains(s)); 
                if(userContainsAnyCategory)
                    Debug.Log(user["nick"] + " contains any of: " + categories);
            }
        });
    }

    private ParseQuery<ParseObject> buildTimeTableQuery(List<string> times)
    {
        ParseQuery<ParseObject> query = new ParseQuery<ParseObject>("Timetable").WhereEqualTo(times[0], true);
        for(int i = 1; i < times.Count; i++)
        {
            query.Or(new ParseQuery<ParseObject>("Timetable").WhereEqualTo(times[i], true));
        }
        query = query.Include("user");
        return query;
    }

    public void registerUser()
    {
        if (!validNick())
            return;
        StartCoroutine(registerUserAsync());
    }

    IEnumerator registerUserAsync()
    {
        ParseUser user = new ParseUser()
        {
            Username = SystemInfo.deviceUniqueIdentifier,
            Password = SystemInfo.deviceUniqueIdentifier
        };
        user["nick"] = userNickInput.GetComponent<InputField>().text;
        Task signUpTask = user.SignUpAsync();
        signUpTask.ContinueWith(t =>
        {
            Debug.Log("success register user? " + !t.IsFaulted);
        });
        while (!signUpTask.IsCompleted) yield return null;

        ParseObject timeTable = new ParseObject("Timetable");
        timeTable["user"] = user;
        timeTable["nick"] = userNickInput.GetComponent<InputField>().text;
        Task saveTimetableTask = timeTable.SaveAsync();
        saveTimetableTask.ContinueWith(t =>
        {
            Debug.Log("success save user in time table? " + !t.IsFaulted);
        });
        while (!saveTimetableTask.IsCompleted) yield return null;

        ParseUser.CurrentUser["timetable"] = timeTable;
        Task saveUserInTimeTable = ParseUser.CurrentUser.SaveAsync();
        saveUserInTimeTable.ContinueWith(t =>
        {
            Debug.Log("success save time table in user? " + !t.IsFaulted);
        });
        while (!saveUserInTimeTable.IsCompleted) yield return null;
    }

    private bool validNick()
    {
        string pattern = @"^[a-zA-Z0-9\_]+$";
        Regex regex = new Regex(pattern);
        if (!regex.IsMatch(userNickInput.GetComponent<InputField>().text))
        {
            Debug.Log("Nickname can only contain letters, numbers and underscore");
            return false;
        }
        if (userNickInput.GetComponent<InputField>().text.Length <3)
        {
            Debug.Log("Nickname must contain at least 3 characters");
            return false;
        }
        return true;
    }

    IEnumerator loginAsync()
    {
        if (ParseUser.CurrentUser == null)
        {
            Task task = ParseUser.LogInAsync(SystemInfo.deviceUniqueIdentifier, SystemInfo.deviceUniqueIdentifier);
            task.ContinueWith(t =>
            {
                Debug.Log("logged in? " + !t.IsFaulted);
            });
            while (!task.IsCompleted) yield return null;
            updateProfileUI( ParseUser.CurrentUser);
            ParseFile pictureFile = (ParseFile)ParseUser.CurrentUser["picture"];
            var pictureRequest = new WWW(pictureFile.Url.AbsoluteUri);
            yield return pictureRequest;
            byte[] bytes = pictureRequest.texture.EncodeToJPG();
            File.WriteAllBytes(Application.dataPath + "/Resources/SavedFoto.jpg", bytes);
            Sprite image = Sprite.Create(pictureRequest.texture, new Rect(0, 0, pictureRequest.texture.width, pictureRequest.texture.height), new Vector2(0.5f, 0.5f));
            userImage.overrideSprite = image;
        }
    }

    IEnumerator logoutAsync()
    {
        if (ParseUser.CurrentUser != null)
        {
            Task task = ParseUser.LogOutAsync();
            task.ContinueWith(t =>
            {
                Debug.Log("logged out? " + !t.IsFaulted);
            });
            while (!task.IsCompleted) yield return null;
            updateProfileUI( ParseUser.CurrentUser);
        }
    }

    public void logout()
    {
        StartCoroutine(logoutAsync());
    }
    public void login()
    {
        StartCoroutine(loginAsync());
    }

    private void updateProfileUI(ParseUser currentUser)
    {
        if (currentUser != null)
        {
            userNickInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["nick"];
            if(ParseUser.CurrentUser["startDate"]!=null)
                userTrainingStartInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["startDate"];
            if(ParseUser.CurrentUser["about"] != null)
                userAboutInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["about"];
            if (ParseUser.CurrentUser["climbingGrade"] != null)
                userClimbGradeInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["climbingGrade"];

            List<string> categories = null;
            if (ParseUser.CurrentUser["categories"].GetType() == typeof(List<object>))
                categories = ParseUser.CurrentUser.Get<List<object>>("categories").Select(s => (string)s).ToList();
            else
                categories = ParseUser.CurrentUser.Get<List<string>>("categories").Select(s => (string)s).ToList();

            categoryRopeClimbToggle.GetComponent<Toggle>().isOn = categories.Contains("ropeClimb");
            categoryBoulderToggle.GetComponent<Toggle>().isOn = categories.Contains("boulder");

            StartCoroutine(updateTimeTable());   
        }
        else
        {
            userNickInput.GetComponent<InputField>().text = "";
            userTrainingStartInput.GetComponent<InputField>().text = "";
            userAboutInput.GetComponent<InputField>().text = "";
        }
    }

    private IEnumerator updateTimeTable()
    {
        ParseObject timeTable = (ParseObject)ParseUser.CurrentUser["timetable"];
        Task task = timeTable.FetchIfNeededAsync();
        while (!task.IsCompleted) yield return null;
        foreach (Transform timeToggle in timeTogglePanel.transform)
        {
            bool isActive = (bool)timeTable[timeToggle.name];
            timeToggle.gameObject.GetComponent<Toggle>().isOn = isActive;
        }
    }

    public void deleteUser()
    {
        StartCoroutine(deleteAsync());
    }

    IEnumerator deleteAsync()
    {
        Task task = ParseUser.CurrentUser.DeleteAsync();
        task.ContinueWith(t =>
        {
            Debug.Log("delete successful? " + !t.IsFaulted);
        });
        while (!task.IsCompleted) yield return null;
        updateProfileUI( null);
    }

    public void uploadImage()
    {
        StartCoroutine(UploadPlayerFile((response) => {
            if (response == 1)
                Debug.Log("Upload complete");
            else
                Debug.LogError("The file could not be uploaded");
        }));
    }

    IEnumerator UploadPlayerFile(Action<int> callback)
    {
        byte[] fileBytes = System.IO.File.ReadAllBytes(Application.dataPath + "/Resources/Foto.jpg");
        ParseFile file = new ParseFile("Foto.jpg", fileBytes, "image/jpeg");

        var saveTask = file.SaveAsync();

        while (!saveTask.IsCompleted)
            yield return null;

        if (saveTask.IsFaulted)
        {
            Debug.LogError("An error occurred while uploading the player file : " + saveTask.Exception.Message);
            callback(-1);
        }
        else
        {
            ParseUser.CurrentUser["picture"] = file;
            ParseUser.CurrentUser.SaveAsync();
            Debug.Log("picture save success ");
            callback(1);
        }
    }

    IEnumerator updateUserAsync()
    {
        if (ParseUser.CurrentUser != null)
        {
            Task task = ParseUser.CurrentUser.SaveAsync();
            task.ContinueWith(t =>
            {
                Debug.Log("update successful? " + !t.IsFaulted);
            });
            while (!task.IsCompleted) yield return null;
            updateProfileUI( ParseUser.CurrentUser);
        }
    }

    public void updateUserAbout(string about)
    {
        ParseUser.CurrentUser["about"] = about;
        StartCoroutine(updateUserAsync());
    }
    public void updateUserClimbGrade(string climbingGrade)
    {
        ParseUser.CurrentUser["climbingGrade"] = climbingGrade;
        StartCoroutine(updateUserAsync());
    }
    public void updateUserNick(string nick)
    {
        ParseUser.CurrentUser["nick"] = nick;
        StartCoroutine(updateUserAsync());
    }
    public void updateUserSportSince(string startDate)
    {
        ParseUser.CurrentUser["startDate"] = startDate;
        StartCoroutine(updateUserAsync());
    }

    public void updateUserRopeClimb(bool active)
    {
        updateUserCategories("ropeClimb",active);
    }
    public void updateUserBoulder(bool active)
    {
        updateUserCategories("boulder", active);
    }
    private void updateUserCategories(string value, bool active)
    {
        List<string> categories = null;
        if (ParseUser.CurrentUser["categories"].GetType() == typeof(List<object>))
            categories = ParseUser.CurrentUser.Get<List<object>>("categories").Select(s => (string)s).ToList();
        else
            categories = ParseUser.CurrentUser.Get<List<string>>("categories").Select(s => (string)s).ToList();
        if (active)
        {
            if (!categories.Contains(value))
            {
                categories.Add(value);
            }
            else
                return;
        }
        else
        {
            if (categories.Contains(value))
            {
                categories.Remove(value);
            }
            else
                return;
        }
        ParseUser.CurrentUser["categories"] = categories;
        StartCoroutine(updateUserAsync());
    }

    private void updateUserTimeTable(string time, bool active)
    {
        ParseObject timeTable = ParseUser.CurrentUser.Get<ParseObject>("timetable");
        timeTable[time] = active;
        timeTable.SaveAsync();
    }
    public void updateUserTimeMon_Mor(bool active)
    {
        updateUserTimeTable("Mon_Mor", active);
    }
    public void updateUserTimeMon_Eve(bool active)
    {
        updateUserTimeTable("Mon_Eve", active);
    }
    public void updateUserTimeMon_Noon(bool active)
    {
        updateUserTimeTable("Mon_Noon", active);
    }
    public void updateUserTimeTue_Mor(bool active)
    {
        updateUserTimeTable("Tue_Mor", active);
    }
    public void updateUserTimeTue_Eve(bool active)
    {
        updateUserTimeTable("Tue_Eve", active);
    }
    public void updateUserTimeTue_Noon(bool active)
    {
        updateUserTimeTable("Tue_Noon", active);
    }
    public void updateUserTimeWed_Mor(bool active)
    {
        updateUserTimeTable("Wed_Mor", active);
    }
    public void updateUserTimeWed_Eve(bool active)
    {
        updateUserTimeTable("Wed_Eve", active);
    }
    public void updateUserTimeWed_Noon(bool active)
    {
        updateUserTimeTable("Wed_Noon", active);
    }
    public void updateUserTimeThu_Mor(bool active)
    {
        updateUserTimeTable("Thu_Mor", active);
    }
    public void updateUserTimeThu_Eve(bool active)
    {
        updateUserTimeTable("Thu_Eve", active);
    }
    public void updateUserTimeThu_Noon(bool active)
    {
        updateUserTimeTable("Thu_Noon", active);
    }
    public void updateUserTimeFri_Mor(bool active)
    {
        updateUserTimeTable("Fri_Mor", active);
    }
    public void updateUserTimeFri_Eve(bool active)
    {
        updateUserTimeTable("Fri_Eve", active);
    }
    public void updateUserTimeFri_Noon(bool active)
    {
        updateUserTimeTable("Fri_Noon", active);
    }
    public void updateUserTimeSat_Mor(bool active)
    {
        updateUserTimeTable("Sat_Mor", active);
    }
    public void updateUserTimeSat_Eve(bool active)
    {
        updateUserTimeTable("Sat_Eve", active);
    }
    public void updateUserTimeSat_Noon(bool active)
    {
        updateUserTimeTable("Sat_Noon", active);
    }
    public void updateUserTimeSun_Mor(bool active)
    {
        updateUserTimeTable("Sun_Mor", active);
    }
    public void updateUserTimeSun_Eve(bool active)
    {
        updateUserTimeTable("Sun_Eve", active);
    }
    public void updateUserTimeSun_Noon(bool active)
    {
        updateUserTimeTable("Sun_Noon", active);
    }

}
