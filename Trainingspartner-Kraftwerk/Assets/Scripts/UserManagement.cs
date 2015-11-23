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

public class UserManagement : MonoBehaviour
{

    public GameObject userNickInput;
    public GameObject userClimbGradeInput;
    public GameObject userTrainingStartInput;
    public GameObject userAboutInput;

    public GameObject categoryBoulderToggle;
    public GameObject categoryRopeClimbToggle;

    public GameObject timeTogglePanel;

    // Dictionary<string, bool> timeTable = new Dictionary<string, bool>();
    List<string> times = new List<string>() { "Mon_Mor", "Mon_Eve", "Mon_Noon", "Tue_Mor", "Tue_Eve", "Tue_Noon", "Wed_Mor", "Wed_Eve", "Wed_Noon", "Thu_Mor", "Thu_Eve", "Thu_Noon", "Fri_Mor", "Fri_Eve", "Fri_Noon", "Sat_Mor", "Sat_Eve", "Sat_Noon", "Sun_Mor", "Sun_Eve", "Sun_Noon" };
    //private List<string> categories = new List<String>{"boulder"}; // selected user categories

    public Image userImage;
    public Sprite anonymous;
    // Use this for initialization
    void Start()
    {
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
        firstLogin();
        //deleteUser();
    }

    private void firstLogin()
    {
        if (ParseUser.CurrentUser == null)
        {
            login();
        } else
        {
            updateProfileUI(ParseUser.CurrentUser);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void registerNewUser()
    {
        StartCoroutine(registerNewUserAsync());
    }

    IEnumerator registerNewUserAsync()
    {
        bool idExistst = false;
        string userID = getRandomString(40);
        while (!idExistst)
        {
            var query = ParseUser.Query.WhereEqualTo("username", userID);
            Task task = query.FindAsync().ContinueWith(t =>
            {
                IEnumerable<ParseUser> users = t.Result;
                foreach (ParseUser user in users)
                {
                    idExistst = true;
                    userID = getRandomString(40);
                    break;
                }
                if (!idExistst)
                {
                    Debug.Log("creating new user: " + userID);
                    idExistst = true;
                }
            });
            while (!task.IsCompleted) yield return null;
        }
        if (ParseUser.CurrentUser != null)
        {
            Debug.Log("register ");
            registerUser(userID);
        }
    }
    private string getRandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var random = new System.Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public void registerUser(string username)
    {
        //if (!validNick())
        //    return;
        StartCoroutine(registerUserAsync(username));
    }

    IEnumerator registerUserAsync(string username)
    {
        ParseUser user = new ParseUser()
        {
            Username = username,
            Password = username
        };
        user["nick"] = userNickInput.GetComponent<InputField>().text;
        user["startDate"] = userTrainingStartInput.GetComponent<InputField>().text;
        user["about"] = userAboutInput.GetComponent<InputField>().text;
        user["climbingGrade"] = userClimbGradeInput.GetComponent<InputField>().text;
        user["categories"] = new List<string>();
        user["picture"] = null;
        Task signUpTask = user.SignUpAsync();
        signUpTask.ContinueWith(t =>
        {
            Debug.Log("success register user? " + !t.IsFaulted);
        });
        while (!signUpTask.IsCompleted) yield return null;

        ParseObject timeTable = new ParseObject("Timetable");
        timeTable["user"] = user;
        timeTable["nick"] = userNickInput.GetComponent<InputField>().text;
        foreach (string time in times)
        {
            timeTable[time] = false;
        }

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
        if (userNickInput.GetComponent<InputField>().text.Length < 3)
        {
            Debug.Log("Nickname must contain at least 3 characters");
            return false;
        }
        return true;
    }

    IEnumerator loginAsync()
    {
        bool idExistst = false;
        string userID = SystemInfo.deviceUniqueIdentifier;

        var query = ParseUser.Query.WhereEqualTo("username", userID);
        Task findUserTask = query.FindAsync().ContinueWith(t =>
        {
            IEnumerable<ParseUser> users = t.Result;
            foreach (ParseUser user in users)
            {
                idExistst = true;
                userID = getRandomString(40);
                break;
            }
        });
        while (!findUserTask.IsCompleted) yield return null;

        if (!idExistst)
        {
            registerUser(SystemInfo.deviceUniqueIdentifier);
        }

        if ((ParseUser.CurrentUser == null) && (idExistst))
        {
            Task task = ParseUser.LogInAsync(SystemInfo.deviceUniqueIdentifier, SystemInfo.deviceUniqueIdentifier);
            task.ContinueWith(t =>
            {
                Debug.Log("logged in? " + !t.IsFaulted);
            });
            while (!task.IsCompleted) yield return null;
            updateProfileUI(ParseUser.CurrentUser);
        }
    }

    IEnumerator setProfilePicture()
    {
        ParseFile pictureFile = null;
        Sprite image = anonymous;

        if (ParseUser.CurrentUser["picture"] != null)
        {
            pictureFile = (ParseFile)ParseUser.CurrentUser["picture"];
        }
        if (pictureFile != null)
        {
            var pictureRequest = new WWW(pictureFile.Url.AbsoluteUri);
            yield return pictureRequest;
            byte[] bytes = pictureRequest.texture.EncodeToJPG();
            Debug.Log("" + Application.persistentDataPath);
            string path = Application.persistentDataPath + "/Resources";
            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllBytes(path + "/SavedFoto.jpg", bytes);
            image = Sprite.Create(pictureRequest.texture, new Rect(0, 0, pictureRequest.texture.width, pictureRequest.texture.height), new Vector2(0.5f, 0.5f));
        }
        userImage.overrideSprite = image;
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
            updateProfileUI(ParseUser.CurrentUser);
        }
    }

    public void logout()
    {
        StartCoroutine(logoutAsync());
    }
    // Login or register if user does not exist 
    public void login()
    {
        StartCoroutine(loginAsync());
    }

    private void updateProfileUI(ParseUser currentUser)
    {
        if (currentUser != null)
        {
            userNickInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["nick"];
            userTrainingStartInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["startDate"];
            userAboutInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["about"];
            userClimbGradeInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["climbingGrade"];

            List<string> categories = getUserCategories();

            categoryRopeClimbToggle.GetComponent<Toggle>().isOn = categories.Contains("ropeClimb");
            categoryBoulderToggle.GetComponent<Toggle>().isOn = categories.Contains("boulder");

            StartCoroutine(updateTimeTable());
            StartCoroutine(setProfilePicture());
        }
        else
        {
            userNickInput.GetComponent<InputField>().text = "";
            userTrainingStartInput.GetComponent<InputField>().text = "";
            userAboutInput.GetComponent<InputField>().text = "";
            userClimbGradeInput.GetComponent<InputField>().text = "";

            userImage.overrideSprite = anonymous;
        }
    }

    private static List<string> getUserCategories()
    {
        List<string> categories = null;
        if (ParseUser.CurrentUser["categories"].GetType() == typeof(List<object>))
            categories = ParseUser.CurrentUser.Get<List<object>>("categories").Select(s => (string)s).ToList();
        else
            categories = ParseUser.CurrentUser.Get<List<string>>("categories").Select(s => (string)s).ToList();
        return categories;
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
        ParseUser currentUser = ParseUser.CurrentUser;

        ParseObject timeTable = currentUser.Get<ParseObject>("timetable");
        Task deleteTask = timeTable.DeleteAsync();
        deleteTask.ContinueWith(t =>
        {
            Debug.Log("delete timetable successful? " + !t.IsFaulted);
        });
        while (!deleteTask.IsCompleted) yield return null;

        Task task = currentUser.DeleteAsync();
        task.ContinueWith(t =>
        {
            Debug.Log("delete successful? " + !t.IsFaulted);
        });
        while (!task.IsCompleted) yield return null;

        updateProfileUI(null);
    }

    public void uploadImage()
    {
        StartCoroutine(UploadPlayerFile((response) =>
        {
            if (response == 1)
            {
                Debug.Log("Upload complete");
            }
            else
                Debug.LogError("The file could not be uploaded");
        }));
    }

    IEnumerator UploadPlayerFile(Action<int> callback)
    {
        string path = Application.dataPath + "/Resources";
        byte[] fileBytes = System.IO.File.ReadAllBytes(path + "/Foto.jpg");
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
            updateProfileUI(ParseUser.CurrentUser);
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
        updateUserCategories("ropeClimb", active);
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
