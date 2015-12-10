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

    public GameObject conversationContentPanel;
    public GameObject conversationEntry;
    public Sprite anonymous;

    public GameObject chatUsername;
    public GameObject chatContentPanel;
    public GameObject chatEntryUser;
    public GameObject chatEntryPartner;
    public GameObject chatField;

    private int messageLimit = 0;
    private ParseUser partner;
    // Use this for initialization
    void Start()
    {
        messageLimit = initialMessageLimit;
        //enterConversation(null);
        fetchUserPartners();
    }

    private static void fetchUserPartners()
    {
        List<ParseUser> userPartners = getUserPartners();
        foreach (ParseUser user in userPartners)
        {
            if (user != null)
            {
                user.FetchIfNeededAsync();
                Debug.Log("fetch ");
            }
        }
    }

    // Update is called once per frame
    void Update () {
	    
	}

    void OnEnable()
    {
        EventManager.onConversationAdded += conversationAdded;
        EventManager.onMenuStateChanged += menuStateChanged;
        EventManager.onConversationEntered += enterConversation;
    }

    void OnDisable()
    {
        EventManager.onConversationAdded -= conversationAdded;
        EventManager.onMenuStateChanged -= menuStateChanged;
        EventManager.onConversationEntered -= enterConversation;
    }

    private void menuStateChanged(MenuState menuState)
    {
        if (menuState == MenuState.messages)
        {
            fetchUserPartners();
            fillUserPartners();
        }
        if (menuState != MenuState.create_message)
        {
            removeChatMessages();
            leaveConversation();
        }
    }

    void OnApplicationPause(bool paused)
    {
        if (!paused)
        {
            if(partner!=null)
                enterConversation(partner);

        } else
        {
            leaveConversation();
        }
    }

    public void fillUserPartners()
    {
        List<ParseUser> partners = getUserPartners();
        if (conversationContentPanel.GetComponent<RectTransform>().childCount!=partners.Count)
        {
            foreach (RectTransform partnerRect in conversationContentPanel.GetComponent<RectTransform>())
            {
                Destroy(partnerRect.gameObject);
            }
            foreach (ParseUser partner in partners)
            {
                if (partner == null)
                    continue;
                GameObject conversationGO = Instantiate(conversationEntry) as GameObject;
                conversationGO.GetComponent<RectTransform>().SetParent(conversationContentPanel.GetComponent<RectTransform>(),false);
                conversationGO.GetComponent<UserEntry>().setUser(partner);
                foreach (RectTransform item in conversationGO.GetComponent<RectTransform>())
                {
                    if (item.gameObject.name.Equals("UserImage"))
                    {
                        StartCoroutine(setUserPicture(partner.Get<ParseFile>("picture"), item.GetComponent<Image>()));
                    }
                    if (item.gameObject.name.Equals("UserName"))
                    {
                        item.GetComponent<Text>().text = partner.Get<string>("nick");
                    }
                }
            }
        }
    }

    IEnumerator setUserPicture(ParseFile pictureFile, Image userImage)
    {
        Sprite image = anonymous;
        if (pictureFile != null)
        {
            var pictureRequest = new WWW(pictureFile.Url.AbsoluteUri);
            yield return pictureRequest;
            image = Sprite.Create(pictureRequest.texture, new Rect(0, 0, pictureRequest.texture.width, pictureRequest.texture.height), new Vector2(0.5f, 0.5f));
        }
        userImage.overrideSprite = image;
    }

    private void conversationAdded(ParseUser partner)
    {
        Debug.Log("add conversation  ");
        this.partner = partner;
        List<ParseUser> partners = getUserPartners();
        Debug.Log("size  " + partners.Count);
        foreach (ParseUser p in partners)
        {
            Debug.Log("partner " + (string)p["nick"]);
        }
        if (!listContainsPartner(partners,partner))
        {
            Debug.Log("added partner");
            partners.Add(partner);
            ParseUser.CurrentUser["partners"] = partners;
            ParseUser.CurrentUser.SaveAsync();
        }
    }

    private bool listContainsPartner(List<ParseUser> partners, ParseUser partner)
    {
        foreach (ParseUser user in partners)
        {
            Debug.Log("user id " + user.ObjectId);
            if (user.ObjectId.Equals(partner.ObjectId))
            {
                return true;
            }
        }
        return false;
    }

    private static List<ParseUser> getUserPartners()
    {
        List<ParseUser> partners = null;
        if (ParseUser.CurrentUser["partners"].GetType() == typeof(List<object>))
            partners = ParseUser.CurrentUser.Get<List<object>>("partners").Select(u => (ParseUser)u).ToList();
        else
            partners = ParseUser.CurrentUser.Get<List<ParseUser>>("partners").Select(u => (ParseUser)u).ToList();
        return partners;
    }

    private void enterConversation(ParseUser partner)
    {
        leaveConversation();
        if (!getUserPartners().Contains(partner))
        {
            conversationAdded(partner);
        }
        this.partner = partner;
        chatUsername.GetComponent<Text>().text = (string)partner["nick"];
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
        ParseQuery<ParseObject> queryPartner = new ParseQuery<ParseObject>("Message").WhereEqualTo("sender", partner).WhereEqualTo("receiver", ParseUser.CurrentUser);
        ParseQuery<ParseObject> querySender = new ParseQuery<ParseObject>("Message").WhereEqualTo("sender", ParseUser.CurrentUser).WhereEqualTo("receiver", partner);
        ParseQuery<ParseObject> query = querySender.Or(queryPartner).OrderByDescending("timestamp").Limit(messageLimit);
        query.Include("sender");
        Task task = query.FindAsync().ContinueWith(t =>
        {
            messageList = t.Result;
        });
        while (!task.IsCompleted) yield return null;
            messageList = (from m in messageList
                       orderby (m.Get<long>("timestamp")) ascending
                    select m).ToList();
        removeChatMessages();
        foreach(ParseObject message in messageList)
        {
            createMessageObject(message);
        }
    }

    private void removeChatMessages()
    {
        foreach (RectTransform item in chatContentPanel.GetComponent<RectTransform>())
        {
            Destroy(item.gameObject);
        }
    }

    private void createMessageObject(ParseObject message)
    {
        DateTime dateTime = new DateTime(message.Get<long>("timestamp"));
        string msg =  message.Get<string>("message_text") + " : " + dateTime.ToString();
        Debug.Log(msg);
        bool isUserMessage = message.Get<ParseUser>("sender").ObjectId.Equals(ParseUser.CurrentUser.ObjectId);
        GameObject messageEntryObject = isUserMessage ? chatEntryUser : chatEntryPartner;
        GameObject mGo = Instantiate(messageEntryObject) as GameObject;
        mGo.GetComponent<RectTransform>().SetParent(chatContentPanel.GetComponent<RectTransform>(),false);
        foreach (RectTransform item in mGo.GetComponent<RectTransform>())
        {
            if (item.gameObject.name.Equals("Text"))
            {
                item.GetComponent<Text>().text = msg;
            }
        }
    }

    public void sendMessage()
    {
        addMessage(chatField.GetComponent<InputField>().text,partner);
    }

    public void addMessage(string message,ParseUser receiver)
    {
        if(receiver==null)
            return;
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
