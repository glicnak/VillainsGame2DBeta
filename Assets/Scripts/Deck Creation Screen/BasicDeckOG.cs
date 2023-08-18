using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDeckOG : MonoBehaviour
{
    public OGBasicDeck myDeck = new OGBasicDeck();

    void Start()
    {
        string myDeckString = JsonUtility.ToJson(myDeck);
        string filePath = Application.persistentDataPath + "/Basic Deck OG.json";
        System.IO.File.WriteAllText(filePath, myDeckString);
    }

    public class OGBasicDeck {
        public string name = "Basic Deck";
        public List<string> deckList = new List<string>(){
            "Alolan Geodude", "Ice Sandshrew", "Ice Vulpix", "Cubone", "Exeggcute", "Goldeen", "Horsea", "Paras", "Ponyta", "Psyduck", "Spearow", "Venonat", "Vulpix" 
        };
    }
}
