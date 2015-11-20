using UnityEngine;
using Parse;
using System.Collections.Generic;

public class EventManager : MonoBehaviour {

    public delegate void UserQueryFinished(List<ParseUser> users);
    public static event UserQueryFinished onUserQueryFinished;

    public static void triggerUserQueryFinished(List<ParseUser> users)
    {
        onUserQueryFinished(users);
    }

}
