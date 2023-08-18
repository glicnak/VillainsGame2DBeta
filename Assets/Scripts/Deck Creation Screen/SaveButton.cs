using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveButton : MonoBehaviour
{
    public Deck myDeck = new Deck();
    public GameObject deckButtons;

    void Start()
    {
        
    }

    public void OnClick(){
        //Set the name & deck
        myDeck.name = GameObject.Find("Rename Deck Input").GetComponent<TMP_InputField>().text;
        myDeck.deckList = DeckManager.Instance.myDeckList;

        //Get The Clicked Deck Button & rename the button on screen
        GameObject currentDeckButton = renameDeckOnScreen();

        //Save the File
        string myDeckString = JsonUtility.ToJson(myDeck);
        string filePath = Application.persistentDataPath + "/" + currentDeckButton.name + ".json";
        Debug.Log("Deck saved to: " + filePath);
        System.IO.File.WriteAllText(filePath, myDeckString);
    }

    public class Deck {
        public string name;
        public List<string> deckList = new List<string>();
    }

    public GameObject renameDeckOnScreen(){
        foreach(Transform child in deckButtons.transform){
            if (child.GetComponent<Button>().interactable == true){
                child.GetChild(0).GetComponent<TextMeshProUGUI>().text = myDeck.name;
                return child.gameObject;
            }
        }
        return null;
    }
}
