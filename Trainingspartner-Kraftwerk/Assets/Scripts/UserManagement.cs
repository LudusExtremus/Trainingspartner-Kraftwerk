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
using ImageVideoContactPicker;

public class UserManagement : MonoBehaviour
{
    public GameObject userNickInput;
    public GameObject userClimbGradeInput;
    public GameObject userTrainingStartInput;
    public GameObject userAboutInput;

    public GameObject categoryBoulderToggle;
    public GameObject categoryRopeClimbToggle;

    public GameObject timeTogglePanel;

    public GameObject profileUpdateNotification;
    public int notificationTime = 2;

    List<string> times = new List<string>() { "Mon_Mor", "Mon_Eve", "Mon_Noon", "Tue_Mor", "Tue_Eve", "Tue_Noon", "Wed_Mor", "Wed_Eve", "Wed_Noon", "Thu_Mor", "Thu_Eve", "Thu_Noon", "Fri_Mor", "Fri_Eve", "Fri_Noon", "Sat_Mor", "Sat_Eve", "Sat_Noon", "Sun_Mor", "Sun_Eve", "Sun_Noon" };

    public Image userImage;
    public Sprite anonymous;

    public int profilePictureSize = 400;

    private const string FILENAME_PROFILE_PIC = "profile_picture.png";
    private Texture2D profilePicTexture;
    private string errorMessage = "";
    private bool userUpdateAllowed = true;
    //private string path = Application.persistentDataPath + "/Resources/profile.jpg";

    void OnEnable()
    {
        PickerEventListener.onImageSelect += OnImageSelect;
        PickerEventListener.onImageLoad += OnImageLoad;
        PickerEventListener.onError += OnError;
        PickerEventListener.onCancel += OnCancel;
    }

    void OnDisable()
    {
        PickerEventListener.onImageSelect -= OnImageSelect;
        PickerEventListener.onImageLoad -= OnImageLoad;
        PickerEventListener.onError -= OnError;
        PickerEventListener.onCancel -= OnCancel;
    }

    void Start()
    {
        //updateTimeTable(times);
        //queryTimeTable();
        firstLogin();
        //deleteUser();
    }

    void OnGUI()
    {
        if(errorMessage.Count() > 0)
        {
            GUI.TextField(new Rect(0, 0, Screen.width , Screen.height / 3), errorMessage);
        }
    }

    void OnImageSelect(string imgPath)
    {
        Debug.Log("Image Location : " + imgPath);
    }
    void OnImageLoad(string imgPath, Texture2D tex)
    {
        tex = resizeTexture(tex, profilePictureSize);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        userImage.overrideSprite = sprite;
        uploadImage(tex);
        Debug.Log("Image Location : " + imgPath);
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

    void OnError(string errorMsg)
    {
        Debug.Log("Error : " + errorMsg);
        StartCoroutine(showError("Error loading image: Please load images from internal storage only!"));
    }
    void OnCancel()
    {
        Debug.Log("Cancel by user");
    }

    IEnumerator showError(string message)
    {
        this.errorMessage = message;
        yield return new WaitForSeconds(5);
        this.errorMessage = "";
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
        user["partners"] = new List<ParseUser>();
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

        ParseInstallation.CurrentInstallation["user"] = ParseUser.CurrentUser;
        Task saveUserInInstallation = ParseInstallation.CurrentInstallation.SaveAsync();
        saveUserInInstallation.ContinueWith(t =>
        {
            Debug.Log("success save user in installation? " + !t.IsFaulted);
        });
        while (!saveUserInInstallation.IsCompleted) yield return null;

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
        Debug.Log("set picture ");
        if (ParseUser.CurrentUser["picture"] != null)
        {
            pictureFile = (ParseFile)ParseUser.CurrentUser["picture"];
        }
        Debug.Log("pictureFile " + pictureFile);
        string path = Application.persistentDataPath + "/" + FILENAME_PROFILE_PIC;
        if (pictureFile != null)
        {
            
            if (File.Exists(path))
            {
                var fileData = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                image = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                Debug.Log("Open file");
            }
            else
            {
                //var pictureRequest = WWW.LoadFromCacheOrDownload(pictureFile.Url.AbsoluteUri,1);
                var pictureRequest = new WWW(pictureFile.Url.AbsoluteUri);
                yield return pictureRequest;
                byte[] fileBytes = pictureRequest.texture.EncodeToJPG(25);
                File.WriteAllBytes(path, fileBytes);
                image = Sprite.Create(pictureRequest.texture, new Rect(0, 0, pictureRequest.texture.width, pictureRequest.texture.height), new Vector2(0.5f, 0.5f));
                Debug.Log("Download file");
            }
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
            updateProfileUI(null);
            Caching.CleanCache();
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
            userTrainingStartInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["startDate"];
            userAboutInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["about"];
            userClimbGradeInput.GetComponent<InputField>().text = (string)ParseUser.CurrentUser["climbingGrade"];

            List<string> categories = getUserCategories();
            userUpdateAllowed = false;
            categoryRopeClimbToggle.GetComponent<Toggle>().isOn = categories.Contains("ropeClimb");
            categoryBoulderToggle.GetComponent<Toggle>().isOn = categories.Contains("boulder");
            userUpdateAllowed = true;
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
        userUpdateAllowed = false;
        foreach (Transform timeToggle in timeTogglePanel.transform)
        {
            bool isActive = (bool)timeTable[timeToggle.name];
            timeToggle.gameObject.GetComponent<Toggle>().isOn = isActive;
        }
        userUpdateAllowed = true;
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
        Caching.CleanCache();
    }

    public void browseGalleryProfileImage()
    {
        #if UNITY_ANDROID
                AndroidPicker.BrowseImage();
        #elif UNITY_IPHONE
			        IOSPicker.BrowseImage();
        #endif
    }

    public void uploadImage(Texture2D tex)
    {
        profilePicTexture = tex;
        StartCoroutine(UploadPlayerFile((response) =>
        {
            if (response == 1)
            {
                Debug.Log("Upload and save complete");
            }
            else
                Debug.LogError("The file could not be uploaded");
        }));
    }

    IEnumerator UploadPlayerFile(Action<int> callback)
    {
        string path = Application.persistentDataPath + "/" + FILENAME_PROFILE_PIC;

        byte[] fileBytes = profilePicTexture.EncodeToJPG(25);
        ParseFile file = new ParseFile("Profile.jpg", fileBytes, "image/jpeg");
        
        var saveTask = file.SaveAsync();

        while (!saveTask.IsCompleted)
            yield return null;

        showProfileUpdatedMessage(saveTask);

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
            File.WriteAllBytes(path, fileBytes);
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
            StartCoroutine(showProfileUpdatedMessage(task));
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
        if(userUpdateAllowed)
            updateUserCategories("ropeClimb", active);
    }
    public void updateUserBoulder(bool active)
    {
        if (userUpdateAllowed)
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
        Task saveTask = timeTable.SaveAsync();
        StartCoroutine(showProfileUpdatedMessage(saveTask));
    }

    IEnumerator showProfileUpdatedMessage(Task saveTask)
    {
        while (!saveTask.IsCompleted) yield return null;
        if (!saveTask.IsFaulted)
        {
            profileUpdateNotification.SetActive(true);
            yield return new WaitForSeconds(notificationTime);
            profileUpdateNotification.SetActive(false);
        }
    }

    public void updateUserTimeMon_Mor(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Mon_Mor", active);
    }
    public void updateUserTimeMon_Eve(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Mon_Eve", active);
    }
    public void updateUserTimeMon_Noon(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Mon_Noon", active);
    }
    public void updateUserTimeTue_Mor(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Tue_Mor", active);
    }
    public void updateUserTimeTue_Eve(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Tue_Eve", active);
    }
    public void updateUserTimeTue_Noon(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Tue_Noon", active);
    }
    public void updateUserTimeWed_Mor(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Wed_Mor", active);
    }
    public void updateUserTimeWed_Eve(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Wed_Eve", active);
    }
    public void updateUserTimeWed_Noon(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Wed_Noon", active);
    }
    public void updateUserTimeThu_Mor(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Thu_Mor", active);
    }
    public void updateUserTimeThu_Eve(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Thu_Eve", active);
    }
    public void updateUserTimeThu_Noon(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Thu_Noon", active);
    }
    public void updateUserTimeFri_Mor(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Fri_Mor", active);
    }
    public void updateUserTimeFri_Eve(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Fri_Eve", active);
    }
    public void updateUserTimeFri_Noon(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Fri_Noon", active);
    }
    public void updateUserTimeSat_Mor(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Sat_Mor", active);
    }
    public void updateUserTimeSat_Eve(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Sat_Eve", active);
    }
    public void updateUserTimeSat_Noon(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Sat_Noon", active);
    }
    public void updateUserTimeSun_Mor(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Sun_Mor", active);
    }
    public void updateUserTimeSun_Eve(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Sun_Eve", active);
    }
    public void updateUserTimeSun_Noon(bool active)
    {
        if (userUpdateAllowed)
            updateUserTimeTable("Sun_Noon", active);
    }

}
