using UnityEngine;
using System.Collections;

public class SendMessage : MonoBehaviour {

    public void SendMessageToUser()
    {

        Debug.Log("Dieser Button könnte zum Nachrichten verschicken sein?");
    }

    public void MessageSentToUser()
    {

        Debug.Log("Probetext: Nachricht abgeschickt ?!?");
    }

    public void MessageAborted()
    {

        Debug.Log("Probetext: Nachricht verworfen ?!?");
    }

    public void AddUserToFriendList()
    {

        Debug.Log("Dieser Button könnte den Nutzer zur Freundesliste hinzufügen");
    }
}
