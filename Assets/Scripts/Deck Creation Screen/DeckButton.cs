using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckButton : MonoBehaviour
{
	public GameObject villainButtonContainer;
	public GameObject textInput; 
	public GameObject saveButton;

	public Deck thisButtonsDeck = new Deck();

	public void Start(){
		//Set the name
		loadTheVillainsFromJson();
		if (thisButtonsDeck.name != null && thisButtonsDeck.name != ""){
			transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = thisButtonsDeck.name;
		}
	}

	public void onClick(){
		//Unclick the buttons
		foreach (Transform button in transform.parent){
			button.gameObject.GetComponent<Button>().interactable = false;
		}

		//Unclick all Villain buttons
		foreach (Transform child in villainButtonContainer.transform){
			if (child.GetComponent<Button>().interactable == true){
				child.GetComponent<CardViewButton>().onClick();
			}
		}

		//Click the new Villain buttons
		loadTheVillainsFromJson();
		List<string> villainList = thisButtonsDeck.deckList;
		if(villainList !=null){
			foreach (Transform child in villainButtonContainer.transform){
				if (villainList.Contains(child.GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>().text)){
					child.GetComponent<CardViewButton>().onClick();
				}
			}
		}

		//Set the name in the input field
		textInput.GetComponent<TMP_InputField>().interactable = true;
		textInput.GetComponent<TMP_InputField>().Select();
		textInput.GetComponent<TMP_InputField>().text = transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;

		//Enable the save button
		saveButton.GetComponent<Button>().interactable = true;
		
		//Make the button "on""
		gameObject.GetComponent<Button>().interactable = true;
	}

	public void loadTheVillainsFromJson(){
		//Set the file name
		string filePath = Application.persistentDataPath + "/" + gameObject.name + ".json";

		//If there is none, look for an OG Version
		if (!System.IO.File.Exists(filePath)){
			filePath = Application.persistentDataPath + "/" + gameObject.name + " OG.json";
		}

		//load the villainList
		if (System.IO.File.Exists(filePath)){
			string thisButtonDeckData = System.IO.File.ReadAllText(filePath);
			thisButtonsDeck = JsonUtility.FromJson<Deck>(thisButtonDeckData);
		}
	}

    public class Deck {
        public string name;
        public List<string> deckList = new List<string>();
    }

}
