using UnityEngine;
using System.Collections.Generic;
using Parse;
using System.Linq;
using System;
using System.Collections;
using System.Threading.Tasks;

public class Messaging : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
	    
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

    public void updateMessages(ParseUser partner)
    {
        StartCoroutine(queryMessages(partner));
    }

    private IEnumerator queryMessages(ParseUser partner)
    {
        /*
        Task partnerSearch = ParseUser.Query
        .WhereEqualTo("nick", "Thor")
        .FirstAsync().ContinueWith(t =>
        {
            partner = t.Result;
        });
        while (!partnerSearch.IsCompleted) yield return null;
        */

        IEnumerable<ParseObject> messageList = new List<ParseObject>();
        ParseQuery<ParseObject> queryPartner = new ParseQuery<ParseObject>("Message").WhereEqualTo("sender", partner);
        ParseQuery<ParseObject> querySender = new ParseQuery<ParseObject>("Message").WhereEqualTo("sender", ParseUser.CurrentUser);
        ParseQuery<ParseObject> query = querySender.Or(queryPartner);
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
        StartCoroutine(saveMessage(parseMessage, receiver));
    }

    private IEnumerator saveMessage(ParseObject parseMessage, ParseUser receiver)
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
        updateMessages(receiver);
    }
}
