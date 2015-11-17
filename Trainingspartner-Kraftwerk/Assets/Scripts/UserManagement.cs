using UnityEngine;
using UnityEngine.UI;
using Parse;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;
using System.Collections;

public class UserManagement : MonoBehaviour {

    public GameObject userNickInput;
    public GameObject userTrainingStartInput;
    public GameObject userAboutInput;

    // Use this for initialization
    void Start () {
        if (ParseUser.CurrentUser != null)
        {
            userNickInput.GetComponent<InputField>().text = (String)ParseUser.CurrentUser["nick"];
            userTrainingStartInput.GetComponent<InputField>().text = (String)ParseUser.CurrentUser["startDate"];
            userAboutInput.GetComponent<InputField>().text = (String)ParseUser.CurrentUser["about"];
        }
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    public void registerUser()
    {
        if (!validNick())
            return;
        ParseUser user = new ParseUser()
        {
            Username = SystemInfo.deviceUniqueIdentifier,
            Password = SystemInfo.deviceUniqueIdentifier
        };
        user["nick"] = userNickInput.GetComponent<InputField>().text;
        Task signUpTask = user.SignUpAsync();
        signUpTask.ContinueWith(t =>
        {
            Debug.Log("success? " + !t.IsFaulted);
        });
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

}
