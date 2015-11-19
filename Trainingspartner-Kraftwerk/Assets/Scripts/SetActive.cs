using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SetActive : MonoBehaviour {

    public Button.ButtonClickedEvent thingToDo;

	public void Activate() {

        this.gameObject.SetActive(false);
        Debug.Log("Did something");
	}
	
}
