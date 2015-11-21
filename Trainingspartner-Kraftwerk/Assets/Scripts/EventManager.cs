using UnityEngine;
using Parse;
using System.Collections.Generic;

public class EventManager : MonoBehaviour {

    public delegate void UserQueryFinished(List<ParseUser> users);
    public static event UserQueryFinished onUserQueryFinished;

    public delegate void MenuStateChanged(MenuState state);
    public static event MenuStateChanged onMenuStateChanged;

    public static void finishUserQuery(List<ParseUser> users)
    {
        onUserQueryFinished(users);
    }

    public static void changeMenuState(MenuState state)
    {
        onMenuStateChanged(state);
    }

}
