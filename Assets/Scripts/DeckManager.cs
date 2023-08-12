using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class DeckManager : MonoBehaviour
{
 void Awake(){
        DontDestroyOnLoad(gameObject);
        Instance = this;
        allGruntsArray = Resources.LoadAll<Villain>("Villains/Grunts/");
    }

    public static DeckManager Instance;
    public List<string> myDeckList;
    public List<Villain> allGrunts;
    public List<Villain> myDeck;
    public List<Villain> svToAdd;
    Villain[] allGruntsArray;
    private static Random rng = new Random();  
    public int deckLimit = 13;
    

    public void shuffleDeck(List<Villain> deck){
        int n = deck.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            Villain value = deck[k];  
            deck[k] = deck[n];  
            deck[n] = value;  
        }  
    }

    public void addSVToList(Villain vil){
        svToAdd.Add(vil);
    }

    public void addSVToDeck(){
        foreach(Villain vil in svToAdd){
            int r = rng.Next(0,myDeck.Count-1);
            myDeck.Insert(r, vil);
        }
    }

    public void clearSVList(){
        svToAdd.Clear();
    }

    public void removeDeckTop(){
        myDeck.RemoveAt(0);
    }

    public void makeDeckList(){
        //Make the lists of all
        foreach (Villain vil in allGruntsArray){
            allGrunts.Add(vil);
        }
        //Make the player lists
        foreach (Villain vil in allGrunts){
            if (myDeckList.Contains(vil.cardName) && !myDeck.Contains(vil)){
                myDeck.Add(vil);
            }
        }
        fillDeckList();
        shuffleDeck(myDeck);
    }

    public void fillDeckList(){
        shuffleDeck(allGrunts);
        for (int i=0; i<allGrunts.Count;i++){
            if(myDeck.Count >= deckLimit){
                break;
            }
            if(!myDeck.Contains(allGrunts[i])){
                myDeck.Add(allGrunts[i]);
            }
        }
    }

    public int countGrunts(){
        int count = 0;
        foreach (Villain vil in allGruntsArray){
            if (myDeckList.Contains(vil.cardName)){
                count++;
            }
        }
        return count;
    }

}
