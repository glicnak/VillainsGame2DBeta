using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NoDeckButton : MonoBehaviour
{
	public GameObject villainButtonContainer;
	public GameObject textInput; 
	public GameObject saveButton;

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
		Debug.Log("Click the new Villain buttons");
		
		//Make the button "on""
		gameObject.GetComponent<Button>().interactable = true;

		//Set the name in the input field
		saveButton.GetComponent<Button>().interactable = true;
		textInput.GetComponent<TMP_InputField>().interactable = true;
		textInput.GetComponent<TMP_InputField>().Select();
		textInput.GetComponent<TMP_InputField>().text = transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
	}

}
