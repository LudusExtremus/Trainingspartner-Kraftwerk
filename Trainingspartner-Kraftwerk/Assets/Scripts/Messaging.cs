using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Parse;
using System.Linq;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.IO;

public class Messaging : MonoBehaviour {

    public int initialMessageLimit = 50;
    public int intervalTime = 10;

    public GameObject conversationContentPanel;
    public GameObject conversationEntry;
    public Sprite anonymous;

    public GameObject chatUsername;
    public GameObject chatContentPanel;
    public GameObject chatEntryUser;
    public GameObject chatEntryPartner;
    public GameObject chatField;

    public ScrollRect messageScroller;

    private int messageLimit = 0;
    private ParseUser partner;

    private List<ParseUser> partnerList = new List<ParseUser>();

    private bool fetchingFinished = true;

    void Start()
    {
        messageLimit = initialMessageLimit;
        //enterConversation(null);
        StartCoroutine(fetchUserPartners());
    }

    /*
   private void fetchUserPartners()
   {
       List<ParseUser> userPartners = getUserPartners(ParseUser.CurrentUser);
       foreach (ParseUser user in userPartners)
       {
           if (user != null)
           {
               Task userFetchTask = user.FetchIfNeededAsync();
           }
       }
    }
    */

    IEnumerator fetchUserPartners()
    {
        if (fetchingFinished)
        {
            fetchingFinished = false;
            partnerList = new List<ParseUser>(getPartners(ParseUser.CurrentUser));
            foreach (ParseUser partner in partnerList)
            {
                Task userFetchTask = partner.FetchIfNeededAsync();
                while (!userFetchTask.IsCompleted) yield return null;
            }
            bool partnerAdded = false;
            ParseQuery<ParseUser> query = new ParseQuery<ParseUser>().WhereEqualTo("partners", ParseUser.CurrentUser);
            List<ParseUser> newPartners = new List<ParseUser>();
            Task queryTask = query.FindAsync().ContinueWith(t =>
            {
                IEnumerable<ParseUser> users = t.Result;
                foreach (ParseUser user in users)
                {
                    if (!listContainsPartner(partnerList, user))
                    {
                        partnerAdded = true;
                        partnerList.Add(user);
                        newPartners.Add(user);
                        Debug.Log("added partner " + (string)user["nick"]);
                    }
                }
            });
            while (!queryTask.IsCompleted) yield return null;
            if (partnerAdded)
            {
                ParseUser.CurrentUser["partners"] = partnerList;
                Task updateUserTask = ParseUser.CurrentUser.SaveAsync();
                while (!updateUserTask.IsCompleted) yield return null;
            }
            fillUserPartners(newPartners);
            fetchingFinished = true;
        }
    }

    // Update is called once per frame
    void Update () {
	    
	}

    void OnGUI()
    {
        if (Event.current.keyCode == KeyCode.Return)
        {
            sendMessage();
        }
    }

    void OnEnable()
    {
        EventManager.onMenuStateChanged += menuStateChanged;
        EventManager.onConversationEntered += enterConversation;
    }

    void OnDisable()
    {
        EventManager.onMenuStateChanged -= menuStateChanged;
        EventManager.onConversationEntered -= enterConversation;
    }

    private void menuStateChanged(MenuState menuState)
    {
        if (menuState == MenuState.messages)
        {
            StartCoroutine(fetchUserPartners());
        }
        
        if (menuState == MenuState.create_message)
        {
            if(partner!=null)
                InvokeRepeating("updateMessages", 1, intervalTime);
        } else
        {
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

    public void fillUserPartners(List<ParseUser> newPartners)
    {
        List<ParseUser> partners = partnerList;
            //getUserPartners(ParseUser.CurrentUser);
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
                if(listContainsPartner(newPartners,partner))
                    conversationGO.GetComponent<Image>().color = Color.green;
                foreach (RectTransform item in conversationGO.GetComponent<RectTransform>())
                {
                    if (item.gameObject.name.Equals("UserImage"))
                    {
                        StartCoroutine(setUserPicture(partner, partner.Get<ParseFile>("picture"), item.GetComponent<Image>()));
                    }
                    if (item.gameObject.name.Equals("UserName"))
                    {
                        item.GetComponent<Text>().text = partner.Get<string>("nick");
                    }
                }
            }
        }
    }

    IEnumerator setUserPicture(ParseUser user, ParseFile pictureFile, Image userImage)
    {
        Sprite image = anonymous;
        string path = Application.persistentDataPath + "/" + user.ObjectId + UserSearch.FILENAME_PROFILE_PIC;
        if (pictureFile != null)
        {
            if (File.Exists(path))
            {
                var fileData = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                image = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                Debug.Log("User Open file");
            }
            else
            {
                var pictureRequest = new WWW(pictureFile.Url.AbsoluteUri);
                yield return pictureRequest;
                byte[] fileBytes = pictureRequest.texture.EncodeToJPG(25);
                File.WriteAllBytes(path, fileBytes);
                image = Sprite.Create(pictureRequest.texture, new Rect(0, 0, pictureRequest.texture.width, pictureRequest.texture.height), new Vector2(0.5f, 0.5f));
                Debug.Log("User Download file");
            }
        }
        userImage.overrideSprite = image;
    }

    private Task conversationAdded(ParseUser user, ParseUser partner)
    {
        Debug.Log("add conversation  ");
        List<ParseUser> partners = partnerList;
            //getUserPartners(user);
        if (!listContainsPartner(partners,partner))
        {
            Debug.Log("added partner");
            partners.Add(partner);
            user["partners"] = partners;
            return user.SaveAsync();
        }
        return null;
    }

    private bool listContainsPartner(List<ParseUser> partners, ParseUser partner)
    {
        foreach (ParseUser user in partners)
        {
            if (user.ObjectId.Equals(partner.ObjectId))
            {
                return true;
            }
        }
        return false;
    }

    private static List<ParseUser> getPartners(ParseUser user)
    {
        List<ParseUser> partners = null;
        if (user["partners"].GetType() == typeof(List<object>))
            partners = user.Get<List<object>>("partners").Select(u => (ParseUser)u).ToList();
        else
            partners = user.Get<List<ParseUser>>("partners").Select(u => (ParseUser)u).ToList();
        return partners;
    }

    private void enterConversation(ParseUser partner)
    {
        leaveConversation();
        chatUsername.GetComponent<Text>().text = (string)partner["nick"];
        //getUserPartners(ParseUser.CurrentUser)
        if (!partnerList.Contains(partner))
        {
            Task saveCurrentUserTask = conversationAdded(ParseUser.CurrentUser, partner);
            StartCoroutine(saveCurrentUserInvokeUpdates(partner, saveCurrentUserTask));
        } else
        {
            this.partner = partner;
        }
        Debug.Log("enter conversation");
    }

    IEnumerator saveCurrentUserInvokeUpdates(ParseUser partner, Task saveCurrentUserTask)
    {
        if (saveCurrentUserTask != null)
            while (!saveCurrentUserTask.IsCompleted) yield return null;
        else
            Debug.Log("ERROR SAVING PARTNER TO CURRENT USER");

        //ParseCloud.CallFunctionAsync<ParseUser>("updatePartners", new Dictionary<string, object>());

        this.partner = partner;
        InvokeRepeating("updateMessages", 0, intervalTime);
    }
    /*
    IEnumerator AddLocalUserToPartnersPartnerList(ParseUser partner, Task saveCurrentUserTask)
    {
        if(saveCurrentUserTask!=null)
            while (!saveCurrentUserTask.IsCompleted) yield return null;
        Task fetchUserTask = partner.FetchAsync();
        Debug.Log("fetch async " );
        while (!fetchUserTask.IsCompleted) yield return null;
        Debug.Log("fetch completed " + fetchUserTask.IsFaulted);
        if (!fetchUserTask.IsFaulted)
        {
            List<ParseUser> partners = getUserPartners(partner);
            if (!partners.Contains(ParseUser.CurrentUser))
            {
                Debug.Log("partners not contains current user");
                partners.Add(partner);
                partner["partners"] = partners;
                Task partnerSaveTask = partner.SaveAsync();
                while(!partnerSaveTask.IsCompleted) yield return null;
                Debug.Log("partner partners saved " + partnerSaveTask.IsFaulted);
            }
        }
        this.partner = partner;
        InvokeRepeating("updateMessages", 0, intervalTime);
    }
    */
    public void leaveConversation()
    {
        messageLimit = initialMessageLimit;
        CancelInvoke("updateMessages");
    }

    public void increaseMessageLimit()
    {
        messageLimit *= 2;
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
        Debug.Log("updateMessages ");
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
        messageScroller.verticalNormalizedPosition = 0;
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
        string hour = (dateTime.Hour < 10) ? "0" + dateTime.Hour : "" + dateTime.Hour;
        string min = (dateTime.Minute < 10) ? "0" + dateTime.Minute : "" + dateTime.Minute;
        string sec = (dateTime.Second < 10) ? "0" + dateTime.Second : "" + dateTime.Second;
        string msg =  message.Get<string>("message_text") + " - " + hour + ":" + min + ":" + sec;
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
        string text = chatField.GetComponent<InputField>().text;
        if (text.Count() > 0)
        {
            chatField.GetComponent<InputField>().text = "";
            addMessage(text, partner);
        }
            
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
