using UnityEngine;
using Parse;
using System.Collections;

public class UserEntry : MonoBehaviour {
    /*
    public Image userImage;
    public Text username;
    public Text sportLevel;
    public Text sportSince;
    public Text about;
    */
    private ParseUser user;
    
    public void setUser(ParseUser user)
    {
        this.user = user;
    }

    public ParseUser getUser()
    {
        return this.user;
    }

    void Start()
    {

    }

    void Update()
    {

    }

}
