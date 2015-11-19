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
    public GameObject userTrainingStartInput;
    public GameObject userAboutInput;

    // Dictionary<string, bool> timeTable = new Dictionary<string, bool>();

    private List<string> categories = new List<String>{"boulder"}; // user categories

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
        
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    public void updateTimeTable(List<string> time)
    {
        ParseObject timeTable = ParseUser.CurrentUser.Get<ParseObject>("timetable");
        timeTable["times"] = time;
        timeTable.SaveAsync();
    }   

    public void queryTimeTable()
    {
        var q1 = ParseObject.GetQuery("Timetable").WhereEqualTo("times", "monday_morning");
        var q2 = ParseObject.GetQuery("Timetable").WhereEqualTo("times", "tuesday_noon");
        ParseQuery<ParseObject> query = q1.Or(q2);
        query = query.Include("user");
        query.FindAsync().ContinueWith(t =>
        {
            IEnumerable<ParseObject> timeTables = t.Result;
            TimetableLoop: foreach (var userTimeTable in timeTables)
            {
                ParseUser user = userTimeTable.Get<ParseUser>("user");
                List<string> cats = user.Get<List<object>>("categories").Select(s => (string)s).ToList();
                foreach(string cat in categories){
                    if (!cats.Contains(cat))
                        goto TimetableLoop;
                }
                Debug.Log("User: " + user["nick"] + cats.Contains("boulder"));
            }
        });
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
            updateUI(task, ParseUser.CurrentUser);
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
            updateUI(task, ParseUser.CurrentUser);
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

    private void updateUI(Task task, ParseUser currentUser)
    {
        if (currentUser != null)
        {
            userNickInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["nick"];
            if(ParseUser.CurrentUser["startDate"]!=null)
                userTrainingStartInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["startDate"];
            if(ParseUser.CurrentUser["about"] != null)
                userAboutInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["about"];
        }
        else
        {
            userNickInput.GetComponent<InputField>().text = "";
            userTrainingStartInput.GetComponent<InputField>().text = "";
            userAboutInput.GetComponent<InputField>().text = "";
        }
    }

    IEnumerator updateAsync()
    {
        if (ParseUser.CurrentUser != null)
        {
            ParseUser.CurrentUser["about"] = userAboutInput.GetComponent<InputField>().text;
            ParseUser.CurrentUser["startDate"] = userTrainingStartInput.GetComponent<InputField>().text;
            ParseUser.CurrentUser["nick"] = userNickInput.GetComponent<InputField>().text;
            ParseUser.CurrentUser["categories"] = categories;
            Task task = ParseUser.CurrentUser.SaveAsync();
            task.ContinueWith(t =>
            {
                Debug.Log("update successful? " + !t.IsFaulted);
            });
            while (!task.IsCompleted) yield return null;
            updateUI(task, ParseUser.CurrentUser);
        }
    }

    public void updateUser()
    {
        if (ParseUser.CurrentUser != null)
        {
            StartCoroutine(updateAsync());
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
        updateUI(task, null);
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
    
}
