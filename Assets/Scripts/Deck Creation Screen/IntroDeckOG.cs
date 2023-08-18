using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroDeckOG : MonoBehaviour
{
    public OGIntroDeck myDeck = new OGIntroDeck();

    void Start()
    {
        string myDeckString = JsonUtility.ToJson(myDeck);
        string filePath = Application.persistentDataPath + "/Intro Deck OG.json";
        System.IO.File.WriteAllText(filePath, myDeckString);
    }

    public class OGIntroDeck {
        public string name = "Intro Deck";
        public List<string> deckList = new List<string>(){
            "Alolan Geodude"
        };
    }
}
