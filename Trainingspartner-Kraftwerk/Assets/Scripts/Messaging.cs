using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Parse;
using System.Linq;
using System;
using System.Collections;
using System.Threading.Tasks;

public class Messaging : MonoBehaviour {

    public int initialMessageLimit = 50;
    public int intervalTime = 5;

    private int messageLimit = 0;
    private ParseUser partner;
	// Use this for initialization
	void Start () {
        
        messageLimit = initialMessageLimit;
        //enterConversation(null);
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    public void enterConversation(ParseUser partner)
    {
        this.partner = partner;
        InvokeRepeating("updateMessages", 0, intervalTime);
    }
    public void leaveConversation()
    {
        messageLimit = initialMessageLimit;
        CancelInvoke("updateMessages");
    }

    public void increaseMessageLimit()
    {
        messageLimit *= 2;
        updateMessages();
    }

    private void deleteMessage(ParseObject message)
    {
        StartCoroutine(deleteMessageAsync(message));
    }

    private IEnumerator deleteMessageAsync(ParseObject message)
    {
        Task task = message.DeleteAsync();
        task.ContinueWith(t =>
        {
            Debug.Log("message delete successful? " + !t.IsFaulted);
        });
        while (!task.IsCompleted) yield return null;
    }

    public void updateMessages()
    {
        StartCoroutine(queryMessages());
    }

    private IEnumerator queryMessages()
    {
        
        Task partnerSearch = ParseUser.Query
        .WhereEqualTo("nick", "Thor")
        .FirstAsync().ContinueWith(t =>
        {
            partner = t.Result;
        });
        while (!partnerSearch.IsCompleted) yield return null;
        

        IEnumerable<ParseObject> messageList = new List<ParseObject>();
        ParseQuery<ParseObject> queryPartner = new ParseQuery<ParseObject>("Message").WhereEqualTo("sender", partner);
        ParseQuery<ParseObject> querySender = new ParseQuery<ParseObject>("Message").WhereEqualTo("sender", ParseUser.CurrentUser);
        ParseQuery<ParseObject> query = querySender.Or(queryPartner).OrderByDescending("timestamp").Limit(messageLimit);
        Task task = query.FindAsync().ContinueWith(t =>
        {
            messageList = t.Result;
        });
        while (!task.IsCompleted) yield return null;
            messageList = (from m in messageList
                       orderby (m.Get<long>("timestamp")) ascending
                    select m).ToList();
        foreach(ParseObject message in messageList)
        {
            DateTime dateTime = new DateTime(message.Get<long>("timestamp"));
            Debug.Log( dateTime.ToString() + " " + message.Get<string>("message_text"));
        }
    }

    public void addMessage(string message,ParseUser receiver)
    {
        ParseObject parseMessage = new ParseObject("Message");
        parseMessage["message_text"] = message;
        parseMessage["receiver"] = receiver;
        parseMessage["sender"] = ParseUser.CurrentUser;
        parseMessage["timestamp"] = DateTime.Now.Ticks;
        StartCoroutine(saveMessage(parseMessage));
    }

    private IEnumerator saveMessage(ParseObject parseMessage)
    {
        /*
        Task partnerSearch = ParseUser.Query
        .WhereEqualTo("nick", "Thor")
        .FirstAsync().ContinueWith(t =>
        {
            receiver = t.Result;
        });
        while (!partnerSearch.IsCompleted) yield return null;
        parseMessage["receiver"] = receiver;
        */

        Task saveMessageTask = parseMessage.SaveAsync();
        saveMessageTask.ContinueWith(t =>
        {
            Debug.Log("Message saved? " + !t.IsFaulted);
        });
        while (!saveMessageTask.IsCompleted) yield return null;
        updateMessages();
    }
}
