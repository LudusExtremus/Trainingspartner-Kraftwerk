using UnityEngine;
using Parse;
using System.Collections.Generic;

public class EventManager : MonoBehaviour {

    public static bool appInitFinished = false;

    public delegate void UserQueryFinished(List<ParseUser> users);
    public static event UserQueryFinished onUserQueryFinished;

    public delegate void MenuStateChanged(MenuState state);
    public static event MenuStateChanged onMenuStateChanged;

    public delegate void EnterConversation(ParseUser user);
    public static event EnterConversation onConversationEntered;

    public static void finishUserQuery(List<ParseUser> users)
    {
        onUserQueryFinished(users);
    }

    public static void changeMenuState(MenuState state)
    {
        onMenuStateChanged(state);
    }

    public static void enterConversation(ParseUser user)
    {
        onConversationEntered(user);
    }
}
