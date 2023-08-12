using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardViewButton : MonoBehaviour
{
	public GameObject Roster;

	public void Start(){
		transform.GetChild(0).GetChild(1).GetComponent<Image>().color = new Vector4(0.777f, 0.777f, 0.777f, 1);
		transform.GetChild(0).GetChild(1).GetComponent<Image>().color = new Vector4(1, 0.777f, 0.42f, 1);
	}

	public void onClick(){
		if (gameObject.GetComponent<Button>().interactable == false){
			if(checkTooMany()){
				gameObject.GetComponent<Button>().interactable = true;
				transform.GetChild(0).GetChild(0).GetComponent<Image>().color = new Vector4(1,1,1,1);
				transform.GetChild(0).GetChild(1).GetComponent<Image>().color = new Vector4(1,1,1,1);
				DeckManager.Instance.myDeckList.Add(transform.GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>().text);
				Roster.GetComponent<Roster>().makeCharacter(GetComponent<GruntDisplay>().grunt);
			}
		}
		else if (gameObject.GetComponent<Button>().interactable == true){
			gameObject.GetComponent<Button>().interactable = false;
			transform.GetChild(0).GetChild(1).GetComponent<Image>().color = new Vector4(0.777f, 0.777f, 0.777f, 1);
			transform.GetChild(0).GetChild(1).GetComponent<Image>().color = new Vector4(1, 0.777f, 0.42f, 1);
			DeckManager.Instance.myDeckList.Remove(transform.GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>().text);
			Roster.GetComponent<Roster>().removeCharacter(GetComponent<GruntDisplay>().grunt);
		}
	}

	public bool checkTooMany(){
		if(gameObject.tag=="Grunt"){
			if (DeckManager.Instance.countGrunts() < DeckManager.Instance.deckLimit){
				return true;
			}
			else return false;
		}
		else return false;
	}
}
