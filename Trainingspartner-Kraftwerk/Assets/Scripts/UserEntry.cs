using UnityEngine;
using Parse;
using System.Collections;

public class UserEntry : MonoBehaviour {

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
