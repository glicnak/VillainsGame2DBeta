using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Random = System.Random;

public class GameManager : NetworkBehaviour
{
    public GameObject LocationTemplate;
    public GameObject SuperHeroTemplate;
    public GameObject CivilianTemplate;
    public List<GameObject> deadCardsThisRound;
    public Dictionary<GameObject,GameObject> cardsPlayedByAll = new Dictionary<GameObject,GameObject>();

    public GameObject newLocation;
    public List<Location> initialDeck;
    public List<Location> locationDeck;
    public List<Location> ruinsDeck;

    public int locationsRuined = 0;

    private int gameStartTriggers;

    private static Random rng = new Random(); 

    public static GameManager Instance;

    private void Awake(){
        Instance = this;
    }

    [Server]
    public override void OnStartServer(){
        base.OnStartServer();
        clearDecks();
        makeLocationDeck();
        makeRuinsDeck();
        gameStartTriggers = 0;
    }

    public void clearDecks(){
        locationDeck.Clear();
        ruinsDeck.Clear();
    }    
    
    public void makeLocationDeck(){
        Location[] allLocations = Resources.LoadAll<Location>("Locations/Locations/");
        Location[] allSuperHeros = Resources.LoadAll<Location>("Locations/SuperHeros/");
        Location[] allCivilians = Resources.LoadAll<Location>("Locations/Civilians/");
        Location harbinger = Resources.Load<Location>("Spawnables/S Harbinger");
        shuffleArray(allLocations);
        shuffleArray(allSuperHeros);
        shuffleArray(allCivilians);
        //Prep Deck
        int r0 = rng.Next(4, 6);
        int r1min = r0 - (r0/3) + 2;
        int r1max = r0 + 2;
        int r1 = rng.Next(r1min, r1max);
        int r2 = rng.Next(r1-1, r1+1);
        for (int i=0; i<r0; i++){
            locationDeck.Add(allLocations[i]);
        }
        for (int i=0; i<r1; i++){
            locationDeck.Add(allSuperHeros[i]);
        }
        for (int i=0; i<r2; i++){
            locationDeck.Add(allCivilians[i]);
        }
        //Prep Top 5 cards
        initialDeck.Add(allLocations[r0]);
        initialDeck.Add(allSuperHeros[r1]);
        initialDeck.Add(allSuperHeros[r1+1]);
        initialDeck.Add(allCivilians[r2]);
        initialDeck.Add(allCivilians[r2+1]);
        shuffleDeck(initialDeck);
        shuffleDeck(locationDeck);
        //Add the top 3 cards
        for (int i=0; i<initialDeck.Count;i++){
            locationDeck.Insert(0, initialDeck[i]);
        }
        hasTriples(locationDeck);
        locationDeck.Add(harbinger);
    }

    public static void shuffleArray(Location[] array) {
        for (int i = 0; i < array.Length - 1; i++){
            int j = rng.Next(i, array.Length);
            Location temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    public void makeRuinsDeck(){
        Location[] allSHR = Resources.LoadAll<Location>("Locations/SuperHeroRuins/");
        foreach (Location hero in allSHR){
            ruinsDeck.Add(hero);
        }
        shuffleDeck(ruinsDeck);
    }   

    public void shuffleDeck(List<Location> deck){
        int n = deck.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            Location value = deck[k];  
            deck[k] = deck[n];  
            deck[n] = value;  
        }  
    }

    public void hasTriples(List<Location> deck){
        for (int i=2; i<deck.Count; i++){
            if (deck[i] == deck[i-1] && deck[i]==deck[i-2]){
                int swapIndex = i;
                while (deck[swapIndex] == deck[i]){
                    swapIndex = rng.Next(3, deck.Count);
                }
                Location temp = deck[i];
                deck [i] = deck[swapIndex];
                deck[swapIndex] = temp;
            }
        }
    }

    public void addToCardsPlayedByAll(GameObject card, GameObject target){
        cardsPlayedByAll.Add(card, target);
    }

    public void removeFromCardsPlayedByAll(GameObject card){
        cardsPlayedByAll.Remove(card);
    }

    public void removeFromDeadCards(GameObject card){
        deadCardsThisRound.Remove(card);
    }

    public void clearCardsPlayedByAll(){
        cardsPlayedByAll.Clear();
    }

    public bool startGame(){
        gameStartTriggers++;
        if (gameStartTriggers == 2){
            return true;
        }
        else {
            return false;
        }
    }
}

