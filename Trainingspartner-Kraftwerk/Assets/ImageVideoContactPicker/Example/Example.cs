using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ImageVideoContactPicker;

public class Example : MonoBehaviour {

	string log = "";
	Texture2D texture;

	List<string> _names = new List<string>();
	Dictionary<string,List<string>> _phoneNumbers = new Dictionary<string, List<string>>();
	Dictionary<string,List<string>> _emails = new Dictionary<string, List<string>>();

	void OnEnable()
	{
		PickerEventListener.onImageSelect += OnImageSelect;
		PickerEventListener.onImageLoad += OnImageLoad;
		PickerEventListener.onVideoSelect += OnVideoSelect;
		PickerEventListener.onContactSelect += OnContactSelect;
		PickerEventListener.onError += OnError;
		PickerEventListener.onCancel += OnCancel;
	}
	
	void OnDisable()
	{
		PickerEventListener.onImageSelect -= OnImageSelect;
		PickerEventListener.onImageLoad -= OnImageLoad;
		PickerEventListener.onVideoSelect -= OnVideoSelect;
		PickerEventListener.onContactSelect -= OnContactSelect;
		PickerEventListener.onError -= OnError;
		PickerEventListener.onCancel -= OnCancel;
	}

	
	void OnImageSelect(string imgPath)
	{
		Debug.Log ("Image Location : "+imgPath);
		log += "\nImage Path : " + imgPath;
	}


	void OnImageLoad(string imgPath, Texture2D tex)
	{
		Debug.Log ("Image Location : "+imgPath);
		texture = tex;
	}
	void OnVideoSelect(string vidPath)
	{
		Debug.Log ("Video Location : "+vidPath);
		log += "\nVideo Path : " + vidPath;
	}
	void OnContactSelect(string name, List<string> numbers, List<string> emails)
	{
		Debug.Log("Name : "+name);
		log += "\nName : "+name;
		for(int i=0;i<numbers.Count;i++){
			log += "\nContact "+(1+i)+" : " + numbers[i];
			Debug.Log(numbers[i]);
		}
		for(int i=0;i<emails.Count;i++){
			log += "\nEmail "+(1+i)+" : " + emails[i];
			Debug.Log(emails[i]);
		}
	}
	
	void OnError(string errorMsg)
	{
		Debug.Log ("Error : "+errorMsg);
		log += "\nError :" +errorMsg;
	}

	void OnCancel()
	{
		Debug.Log ("Cancel by user");
		log += "\nCancel by user";
	}

	Vector2 sp1 = Vector2.zero;
	Vector2 sp2 = Vector2.zero;
	void OnGUI()
	{
		GUILayout.Label (log);

		if(GUI.Button(new Rect(10,10,150,35),"Browse Image"))
		 {
			#if UNITY_ANDROID
			AndroidPicker.BrowseImage();
			#elif UNITY_IPHONE
			IOSPicker.BrowseImage();
			#endif
		}
		if(GUI.Button(new Rect(180,10,150,35),"Browse Video"))
		{
			#if UNITY_ANDROID
			AndroidPicker.BrowseVideo();
			#elif UNITY_IPHONE
			IOSPicker.BrowseVideo();
			#endif
		}
		if(GUI.Button(new Rect(350,10 ,150,35),"Browse Contact"))
		{
			#if UNITY_ANDROID
			AndroidPicker.BrowseContact();
			#elif UNITY_IPHONE
			IOSPicker.BrowseContact();
			#endif
		}


		sp1 = GUI.BeginScrollView (new Rect (10, 50, Screen.width/2 - 20, Screen.height - 20), sp1, new Rect (0, 0, Screen.width/2 - 50, _names.Count * 30));
		for (int i=0; i<_names.Count; i++) {
			if(GUI.Button(new Rect(0,5*i,Screen.width/2-25,25),_names[i]))
			{
				_selectedName = _names[i];
			}
		}
		GUI.EndScrollView ();

		if (texture != null){
			
			GUI.DrawTexture(new Rect(20,50,Screen.width - 40,Screen.height - 60), texture, ScaleMode.ScaleToFit, true);
		}

		if (string.IsNullOrEmpty (_selectedName))
			return;

		List<string> phNos = _phoneNumbers [_selectedName];
		List<string> phEmails = _emails[_selectedName];
		sp2 = GUI.BeginScrollView (new Rect (Screen.width/2+10, 50, Screen.width/2 - 20, Screen.height - 20), sp2, new Rect (0, 0, Screen.width/2 - 50, phNos.Count * 30));

		int counter = 1;
		for (int j=0; j<phNos.Count; j++) {
			GUI.Label(new Rect(0,5*counter,Screen.width/2-25,25),phNos[j]);
			counter++;
		}

		for (int k=0; k<phEmails.Count; k++) {
			GUI.Label(new Rect(0,5*counter,Screen.width/2-25,25),phEmails[k]);
			counter++;
		}
		GUI.EndScrollView ();
	}

	string _selectedName = "";
	// Use this for initialization
	void Start () {

	}


}
