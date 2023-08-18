using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmptyDeckButton : MonoBehaviour
{
    public GameObject villainButtonContainer;

    public void onClick(){
		//Unclick all Villain buttons
		foreach (Transform child in villainButtonContainer.transform){
			if (child.GetComponent<Button>().interactable == true){
				child.GetComponent<CardViewButton>().onClick();
			}
		}
	}

    // Update is called once per frame
    void Update()
    {
        if(DeckManager.Instance.myDeckList.Count == 0 && gameObject.GetComponent<Button>().interactable == true){
             gameObject.GetComponent<Button>().interactable = false;
        }

        if(DeckManager.Instance.myDeckList.Count > 0 && gameObject.GetComponent<Button>().interactable == false){
             gameObject.GetComponent<Button>().interactable = true;
        }

    }
}
