using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using Random = System.Random;

public class CardAbilities : NetworkBehaviour
{
    private static Random rng = new Random();  

    public int onAttack(GameObject card, bool priority){
        string name = getName(card);
        GameObject Defender = card.transform.parent.parent.gameObject; // Gets the card being struck
        GameObject Area = card.transform.parent.parent.parent.gameObject; //Street Area, Player Base Area, Opp Base Area
        bool mine = false;
        if(card.transform.parent.GetSiblingIndex() == 2 || card.transform.parent.tag == "Player Base Area"){
            mine = true;
        }
        if (!Defender.GetComponent<CardProperties>().isDead){
            card.GetComponent<CardProperties>().OppAliveBeforeAttack = true;
        }
        if(Defender.tag == "Location" && Defender.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Beach" || Defender.tag == "SuperHero" && Defender.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Beach" && !Defender.GetComponent<CardProperties>().isDead){
            priority = false;
        }
        switch (name){
        //SuperVillains
            case "Arbok":
                if(priority && mine){
                   if(Area.tag == "Street"){
                        foreach (Transform child in Area.transform){
                            foreach (Transform kid in child.GetChild(0)){
                                if(kid.tag == "SuperVillain" && kid.GetComponent<CardProperties>().isFaceUp){
                                    kid.GetComponent<CardProperties>().dealDmg(2,card);
                                }
                            }
                        }
                    }
                    if(Area.tag == "Opp Base Area"){
                        foreach (Transform child in Area.transform){
                            if(child.tag == "SuperVillain" && child.GetComponent<CardProperties>().isFaceUp){
                                child.GetComponent<CardProperties>().dealDmg(2,card);
                            }
                        }
                    }
                }
                else if (priority && !mine){
                    if(Area.tag == "Street"){
                        foreach (Transform child in Area.transform){
                            foreach (Transform kid in child.GetChild(2)){
                                if(kid.tag == "SuperVillain" && kid.GetComponent<CardProperties>().isFaceUp){
                                    kid.GetComponent<CardProperties>().dealDmg(2,card);
                                }
                            }
                        }
                    }
                    if(Area.tag == "Player Base Area"){
                        foreach (Transform child in Area.transform){
                            if(child.tag == "SuperVillain" && child.GetComponent<CardProperties>().isFaceUp){
                                child.GetComponent<CardProperties>().dealDmg(2,card);
                            }
                        }
                    }
                }
                return 0;
            case "Arcanine":
                if(priority){
                    if(card.transform.parent.GetSiblingIndex() == 2){
                        foreach(Transform child in card.transform.parent.parent.GetChild(0)){
                            if (child.tag == "Grunt"){
                                child.GetComponent<CardProperties>().returnedEarly = true;
                                child.GetComponent<CardProperties>().onRevealAbility = true;
                                PlayerManager.Instance.moveCardAbility(child.gameObject, "Player Base", null);
                                PlayerManager.Instance.CmdFlipCardUp(child.gameObject);
                                child.GetComponent<CardProperties>().instaKill();
                            }
                        }
                    }    
                    else{
                        foreach(Transform child in card.transform.parent.parent.GetChild(2)){
                            if (child.tag == "Grunt"){
                                child.GetComponent<CardProperties>().returnedEarly = true;
                                child.GetComponent<CardProperties>().onRevealAbility = true;
                                PlayerManager.Instance.moveCardAbility(child.gameObject, "Player Base", null);
                                PlayerManager.Instance.CmdFlipCardUp(child.gameObject);
                                child.GetComponent<CardProperties>().instaKill();
                            }
                        }
                    }
                }
                return 0;
            case "Charizard":
                if(card.transform.parent.GetSiblingIndex()==2 && Defender.transform.GetChild(2).childCount <= 3){
                    return 3;
                }
                if(card.transform.parent.GetSiblingIndex()==0 && Defender.transform.GetChild(0).childCount <= 3){
                    return 3;
                }
                else return 0;
            case "Dodrio":
                if(priority){
                    if(mine && Area.tag=="Street"){
                        if (Defender.transform.GetSiblingIndex() > 0){
                            for (int i = Defender.transform.GetSiblingIndex()-1; i>=0; i--){
                                if(Area.transform.GetChild(i).GetChild(2).childCount > 2){
                                    for (int j=Area.transform.GetChild(i).GetChild(2).childCount-1; j>1; j--){
                                        GameObject target = Area.transform.GetChild(i).GetChild(2).GetChild(j).gameObject;
                                        GetComponent<PlayerManager>().moveCardAbility(target, "Player Base", null);
                                    }
                                break;
                                }
                            }
                        }
                    }
                    else if (mine && Area.tag=="Opp Base Area"){
                        if (Defender.transform.GetSiblingIndex()+1 < Area.transform.childCount){
                            for (int i = Defender.transform.GetSiblingIndex()+1; i<Area.transform.childCount; i++){
                                if(Area.transform.GetChild(i).GetChild(2).childCount > 1){
                                    for (int j=Area.transform.GetChild(i).GetChild(2).childCount-1; j>0; j--){
                                        GameObject target = Area.transform.GetChild(i).GetChild(2).GetChild(j).gameObject;
                                        GetComponent<PlayerManager>().moveCardAbility(target, "Player Base", null);
                                    }
                                break;
                                }
                            }
                        }
                    }
                }
                return 0;
            case "Lapras":
                if(card.transform.parent.GetSiblingIndex() == 2 && card.transform.GetSiblingIndex() == 2 && card.transform.parent.childCount == 4){
                    card.transform.parent.GetChild(3).GetComponent<CardProperties>().heal(3);
                }
                return 0;
            case "Onix":
                if(priority){
                    if(mine && Area.tag=="Street"){
                        if (Defender.transform.GetSiblingIndex() > 0 && Defender.transform.GetChild(2).childCount < 4){
                            for (int i = Defender.transform.GetSiblingIndex()-1; i>=0; i--){
                                if(Area.transform.GetChild(i).GetChild(2).childCount > 2){
                                    for (int j = 2; j<Area.transform.GetChild(i).GetChild(2).childCount; j++){
                                        GameObject target = Area.transform.GetChild(i).GetChild(2).GetChild(j).gameObject;
                                        if (!target.GetComponent<CardProperties>().isDead){
                                            GetComponent<PlayerManager>().moveCardAbility(target, "Locations", Defender);
                                            GetComponent<PlayerManager>().resolveAttack(target, Defender);
                                            return 0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return 0;
            case "Pinsir":
                int chance = GetComponent<PlayerManager>().rngPercent;
                card.GetComponent<CardProperties>().onRevealAbility = true;
                if (0<chance && chance <=80){
                    return (chance-1)/20;
                }
                else{
                    Defender.GetComponent<CardProperties>().instaKill();
                    return 0;
                }
            case "Sandslash":
                if(card.transform.parent.GetSiblingIndex() == 2 && card.transform.GetSiblingIndex() == 2 && card.transform.parent.childCount == 4){
                    if (card.transform.parent.GetChild(3).tag == "SuperVillain"){
                        card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().attackInt += 1;
                        card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().superVillain.cardAttack += 1;
                        card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().updateCardInfo();
                    }
                    if (card.transform.parent.GetChild(3).tag == "Grunt"){
                        card.transform.parent.GetChild(3).GetComponent<GruntDisplay>().attackInt += 1;
                        card.transform.parent.GetChild(3).GetComponent<GruntDisplay>().grunt.cardAttack += 1;
                        card.transform.parent.GetChild(3).GetComponent<GruntDisplay>().updateCardInfo();

                    }
                }
                else if(card.transform.parent.GetSiblingIndex() == 0 && card.transform.GetSiblingIndex() == 2 && card.transform.parent.childCount == 4){
                    if (card.transform.parent.GetChild(3).tag == "SuperVillain"){
                        card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().attackInt += 1;
                        card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().superVillain.cardAttack += 1;
                        card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().updateCardInfo();
                    }
                    if (card.transform.parent.GetChild(3).tag == "Grunt"){
                        card.transform.parent.GetChild(3).GetComponent<GruntDisplay>().attackInt += 1;
                        card.transform.parent.GetChild(3).GetComponent<GruntDisplay>().grunt.cardAttack += 1;
                        card.transform.parent.GetChild(3).GetComponent<GruntDisplay>().updateCardInfo();
                    }
                }
                return 0;
            case "Starmie":
                GetComponent<PlayerManager>().recruitVillain(name, !mine);
                return 0;
            case "Tangela":
                List<GameObject> deadOpps = new List<GameObject>();
                if(priority && mine && Area.tag == "Street"){
                    foreach(Transform child in Area.transform){
                        for (int i=2; i<child.GetChild(0).childCount; i++){
                            if (Defender != child.gameObject && child.GetChild(0).GetChild(i).GetComponent<CardProperties>().isDead){
                                deadOpps.Add(child.GetChild(0).GetChild(i).gameObject);
                            }
                        }
                    }
                    shuffleDeck(deadOpps);
                    for (int i = 0; i<4-Defender.transform.GetChild(0).childCount; i++){
                        if (i<deadOpps.Count){
                            GetComponent<PlayerManager>().moveCardAbility(deadOpps[i], "Locations", Defender);
                        }
                    }
                }
                return 0;
            case "Venusaur":
                if(Area.tag == "Street"){
                    if (0 <= Defender.transform.GetSiblingIndex()-1 && Defender.transform.GetSiblingIndex()-1 < Area.transform.childCount){
                        Area.transform.GetChild(Defender.transform.GetSiblingIndex()-1).GetComponent<CardProperties>().dealDmg(2,card);
                    }
                    if (0 <= Defender.transform.GetSiblingIndex()+1 && Defender.transform.GetSiblingIndex()+1 < Area.transform.childCount){
                        Area.transform.GetChild(Defender.transform.GetSiblingIndex()+1).GetComponent<CardProperties>().dealDmg(2,card);
                    }
                }
                return 0;
            //Grunts
            case "Bulbasaur":
                if(Area.tag == "Street"){
                    if (0 <= Defender.transform.GetSiblingIndex()-1 && Defender.transform.GetSiblingIndex()-1 < Area.transform.childCount){
                        if (Area.transform.GetChild(Defender.transform.GetSiblingIndex()-1).tag == "Civilian" || Area.transform.GetChild(Defender.transform.GetSiblingIndex()-1).tag == "SuperHero"){
                            Area.transform.GetChild(Defender.transform.GetSiblingIndex()-1).GetComponent<CardProperties>().dealDmg(1,card);
                        }
                    }
                    if (0 <= Defender.transform.GetSiblingIndex()+1 && Defender.transform.GetSiblingIndex()+1 < Area.transform.childCount){
                        if (Area.transform.GetChild(Defender.transform.GetSiblingIndex()+1).tag == "Civilian" || Area.transform.GetChild(Defender.transform.GetSiblingIndex()+1).tag == "SuperHero"){
                            Area.transform.GetChild(Defender.transform.GetSiblingIndex()+1).GetComponent<CardProperties>().dealDmg(1,card);
                        }
                    }
                }
                return 0;
            case "Diglett":
                if (priority){
                    if (Defender.tag == "Location"){
                        return 3;
                    }
                }
                return 0;
            case "Exeggcute":
                if(!card.GetComponent<CardProperties>().onRevealAbility){
                    card.GetComponent<CardProperties>().onRevealAbility = true;  
                    if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
                        return 0;
                    }
                    List<GameObject> myCards = new List<GameObject>();
                    GameObject myBase = GameObject.Find("Player Base Area");
                    if(mine && Area.tag == "Street" && Defender.transform.GetSiblingIndex() > 0 && Area.transform.GetChild(Defender.transform.GetSiblingIndex()-1).GetChild(2).childCount < 4){
                        foreach(Transform child in myBase.transform){
                            if (!child.GetComponent<CardProperties>().isDead && !child.GetComponent<CardProperties>().isStunned){
                                myCards.Add(child.gameObject);
                            }
                        }
                        if(myCards.Count>0){
                            shuffleDeck(myCards);
                            GameObject target = myCards[0];
                            GameObject newDefender = Area.transform.GetChild(Defender.transform.GetSiblingIndex()-1).gameObject;
                            GetComponent<PlayerManager>().moveCardAbility(target, "Locations", newDefender);
                            GetComponent<PlayerManager>().resolveAttack(target, newDefender);
                        }
                    }
                }
                return 0;
            case "Goldeen":
                if(!card.GetComponent<CardProperties>().onRevealAbility){
                    card.GetComponent<CardProperties>().onRevealAbility = true;
                    if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
                    return 0;
                }
                    if(card.transform.parent.GetSiblingIndex() == 2 && card.transform.GetSiblingIndex() == 2 && card.transform.parent.childCount == 4){
                        if (card.transform.parent.GetChild(3).tag == "SuperVillain"){
                            card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().attackInt += 1;
                            card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().superVillain.cardAttack += 1;
                            card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().updateCardInfo();
                        }
                        if (card.transform.parent.GetChild(3).tag == "Grunt"){
                            card.transform.parent.GetChild(3).GetComponent<GruntDisplay>().attackInt += 1;
                            card.transform.parent.GetChild(3).GetComponent<GruntDisplay>().grunt.cardAttack += 1;
                            card.transform.parent.GetChild(3).GetComponent<GruntDisplay>().updateCardInfo();

                        }
                    }
                    else if(card.transform.parent.GetSiblingIndex() == 0 && card.transform.GetSiblingIndex() == 2 && card.transform.parent.childCount == 4){
                        if (card.transform.parent.GetChild(3).tag == "SuperVillain"){
                            card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().attackInt += 1;
                            card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().superVillain.cardAttack += 1;
                            card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().updateCardInfo();
                        }
                        if (card.transform.parent.GetChild(3).tag == "Grunt"){
                            card.transform.parent.GetChild(3).GetComponent<GruntDisplay>().attackInt += 1;
                            card.transform.parent.GetChild(3).GetComponent<GruntDisplay>().grunt.cardAttack += 1;
                            card.transform.parent.GetChild(3).GetComponent<GruntDisplay>().updateCardInfo();
                        }
                    }
                }
                return 0;
            case "Horsea":
                if(!card.GetComponent<CardProperties>().onRevealAbility){
                    card.GetComponent<CardProperties>().onRevealAbility = true;
                    if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
                    return 0;
                }
                    if(mine && Area.tag=="Street"){
                        if (Defender.transform.GetSiblingIndex() > 0){
                            for (int i = Defender.transform.GetSiblingIndex()-1; i>=0; i--){
                                if(Area.transform.GetChild(i).GetChild(2).childCount > 2){
                                    GameObject target = Area.transform.GetChild(i).GetChild(2).GetChild(2).gameObject;
                                    GetComponent<PlayerManager>().moveCardAbility(target, "Player Base", null);
                                    break;
                                }
                            }
                        }
                    }
                    else if (mine && Area.tag=="Opp Base Area"){
                        if (Defender.transform.GetSiblingIndex()+1 < Area.transform.childCount){
                            for (int i = Defender.transform.GetSiblingIndex()+1; i<Area.transform.childCount; i++){
                                if(Area.transform.GetChild(i).GetChild(2).childCount > 1){
                                    for (int j=Area.transform.GetChild(i).GetChild(2).childCount-1; j>0; j--){
                                        GameObject target = Area.transform.GetChild(i).GetChild(2).GetChild(j).gameObject;
                                        GetComponent<PlayerManager>().moveCardAbility(target, "Player Base", null);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                return 0;
            case "Krabby":
                if(card.transform.parent.GetSiblingIndex()==2 && Defender.transform.GetChild(2).childCount <= 3){
                    return 2;
                }
                if(card.transform.parent.GetSiblingIndex()==0 && Defender.transform.GetChild(0).childCount <= 3){
                    return 2;
                }
                else return 0;
            case "Magnemite":
                if(!card.GetComponent<CardProperties>().onRevealAbility){
                    card.GetComponent<CardProperties>().onRevealAbility = true;
                    if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
                    return 0;
                }
                    return 2;
                }
                return 0;
            case "Paras":
                if(card.transform.parent.GetSiblingIndex() == 2 && card.transform.GetSiblingIndex() == 2 && card.transform.parent.childCount == 4 && priority){
                    card.transform.parent.GetChild(3).GetComponent<CardProperties>().heal(2);
                }
                return 0;
            case "Ponyta":
                if(!card.GetComponent<CardProperties>().onRevealAbility){
                    card.GetComponent<CardProperties>().onRevealAbility = true;
                    if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
                        return 0;
                    }
                    if(priority){
                        if(mine && Area.tag=="Street"){
                            if (Defender.transform.GetSiblingIndex() > 0 && Defender.transform.GetChild(2).childCount < 4){
                                for (int i = Defender.transform.GetSiblingIndex()-1; i>=0; i--){
                                    if(Area.transform.GetChild(i).GetChild(2).childCount > 2){
                                        GameObject target = Area.transform.GetChild(i).GetChild(2).GetChild(2).gameObject;
                                        GetComponent<PlayerManager>().moveCardAbility(target, "Locations", Defender);
                                        return 0;
                                    }
                                }
                            }
                        }
                    }
               }
               return 0;
            case "Psyduck":
                if(!card.GetComponent<CardProperties>().onRevealAbility){
                    card.GetComponent<CardProperties>().onRevealAbility = true;
                    if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
                        return 0;
                    }
                    List<GameObject> oppCards = new List<GameObject>();
                    GameObject oppBase = GameObject.Find("Opponent Base");
                    if(mine && Area.tag == "Street" && Defender.transform.GetChild(0).childCount < 4){
                        foreach(Transform child in oppBase.transform){
                            if (!child.GetComponent<CardProperties>().isDead && !child.GetComponent<CardProperties>().isStunned){
                                oppCards.Add(child.gameObject);
                            }
                        }
                        if(oppCards.Count > 0){
                            shuffleDeck(oppCards);
                            GameObject target = oppCards[0];
                            GetComponent<PlayerManager>().moveCardAbility(target, "Locations", Defender);
                            GetComponent<PlayerManager>().resolveAttack(target, Defender);
                        }
                    }
                }
                return 0;
            case "Slowpoke":
                int counter =0;
                if(Area.tag == "Street"){
                    for (int i=2; i<Defender.transform.GetChild(0).childCount; i++){
                        if (!Defender.transform.GetChild(0).GetChild(i).GetComponent<CardProperties>().isDead && Defender.transform.GetChild(0).GetChild(i).gameObject != card && Defender.transform.GetChild(0).GetChild(i).GetComponent<CardProperties>().isFaceUp){
                            counter++;
                        }
                    }
                    for (int i=2; i<Defender.transform.GetChild(2).childCount; i++){
                        if (!Defender.transform.GetChild(2).GetChild(i).GetComponent<CardProperties>().isDead && Defender.transform.GetChild(2).GetChild(i).gameObject != card && Defender.transform.GetChild(2).GetChild(i).GetComponent<CardProperties>().isFaceUp){
                            counter++;
                        }
                    }
                }
                else {
                    counter = 1;
                }
                return counter;
            case "Spearow":
                if(!card.GetComponent<CardProperties>().onRevealAbility){
                    card.GetComponent<CardProperties>().onRevealAbility = true;
                    if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
                        return 0;
                    }
                    if(mine & Area.tag == "Street"){
                        foreach(Transform child in card.transform.parent.parent.GetChild(0)){
                            if(child.tag == "Grunt" || child.tag=="SuperVillain"){
                                child.GetComponent<CardProperties>().returnedEarly = true;
                                PlayerManager.Instance.moveCardAbility(child.gameObject, "Player Base", null);
                                PlayerManager.Instance.CmdFlipCardUp(child.gameObject);
                                child.GetComponent<CardProperties>().onRevealAbility = true;
                                child.GetComponent<CardProperties>().isStunned = true;
                            }
                        }
                    }
                }
                return 0;
            case "Tentacool":
                if(!card.GetComponent<CardProperties>().onRevealAbility){
                    card.GetComponent<CardProperties>().onRevealAbility = true;
                    if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
                        return 0;
                    }
                    if(Area.tag == "Street" && priority){
                        if (0 <= Defender.transform.GetSiblingIndex()-1 && Defender.transform.GetSiblingIndex()-1 < Area.transform.childCount){
                            if (Area.transform.GetChild(Defender.transform.GetSiblingIndex()-1).tag == "Location"){
                                Area.transform.GetChild(Defender.transform.GetSiblingIndex()-1).GetComponent<CardProperties>().dealDmg(2,card);
                            }
                        }
                        if (0 <= Defender.transform.GetSiblingIndex()+1 && Defender.transform.GetSiblingIndex()+1 < Area.transform.childCount){
                            if (Area.transform.GetChild(Defender.transform.GetSiblingIndex()+1).tag == "Location"){
                                Area.transform.GetChild(Defender.transform.GetSiblingIndex()+1).GetComponent<CardProperties>().dealDmg(2,card);
                            }
                        }
                    }
                }
                return 0;
            case "Venonat":
                if(mine && priority){
                    GameObject ChaosBox = GameObject.Find("Player Chaos Box");
                    ChaosBox.GetComponent<Chaos>().roundChaos += 2;
                    ChaosBox.GetComponent<Chaos>().totalChaos += 2;
                }
                return 0;
            case "Vulpix":
                if(priority){
                    if(card.transform.parent.GetSiblingIndex() == 2 && card.transform.GetSiblingIndex() == 2 && card.transform.parent.childCount == 4){
                        if(card.transform.parent.GetChild(3).tag == "Grunt"){
                            card.transform.parent.GetChild(3).GetComponent<CardProperties>().isImmune = true;
                        }
                    }
                    if(card.transform.parent.GetSiblingIndex() == 0 && card.transform.GetSiblingIndex() == 2 && card.transform.parent.childCount == 4){
                        if(card.transform.parent.GetChild(3).tag == "Grunt"){
                            card.transform.parent.GetChild(3).GetComponent<CardProperties>().isImmune = true;
                        }
                    }
                }
                return 0;
            case "Zubat":
                if(priority){
                    Defender.GetComponent<CardProperties>().isStunned = true;
                }
                return 0;
            //Spawnables
            case "Flareon":
                int percent = GetComponent<PlayerManager>().rngPercent;
                Defender.GetComponent<CardProperties>().dealDmg((percent-1)/25, card);
                List<int> percentRolls = GetComponent<PlayerManager>().percentRolls;
                if(Area.tag == "Street"){
                    if (card.transform.parent.GetSiblingIndex() == 2){
                        for (int i=2; i < Defender.transform.GetChild(0).childCount; i++){
                            if (Defender.transform.GetChild(0).GetChild(i).GetComponent<CardProperties>().isFaceUp && Defender.transform.GetChild(0).GetChild(i).gameObject != card){
                                if(Defender.transform.GetChild(0).GetChild(i).tag != "Grunt" || percentRolls[i-2] > 40){
                                    Defender.transform.GetChild(0).GetChild(i).GetComponent<CardProperties>().dealDmg((percentRolls[i-2]-1)/25, card);
                                }
                            }
                        }
                        for (int i=2; i < Defender.transform.GetChild(2).childCount; i++){
                            if (Defender.transform.GetChild(2).GetChild(i).GetComponent<CardProperties>().isFaceUp && Defender.transform.GetChild(2).GetChild(i).gameObject != card){
                                if(Defender.transform.GetChild(2).GetChild(i).tag != "Grunt" || percentRolls[i+Defender.transform.GetChild(0).childCount-4] > 40){
                                    Defender.transform.GetChild(2).GetChild(i).GetComponent<CardProperties>().dealDmg((percentRolls[i+Defender.transform.GetChild(0).childCount-4]-1)/25, card);
                                }
                            }
                        }
                    }
                    else{
                        for (int i=2; i < Defender.transform.GetChild(2).childCount; i++){
                            if (Defender.transform.GetChild(2).GetChild(i).GetComponent<CardProperties>().isFaceUp && Defender.transform.GetChild(2).GetChild(i).gameObject != card){
                                if(Defender.transform.GetChild(2).GetChild(i).tag != "Grunt" || percentRolls[i-2] > 40){
                                    Defender.transform.GetChild(2).GetChild(i).GetComponent<CardProperties>().dealDmg((percentRolls[i-2]-1)/25, card);
                                }
                            }
                        }
                        for (int i=2; i < Defender.transform.GetChild(0).childCount; i++){
                            if (Defender.transform.GetChild(0).GetChild(i).GetComponent<CardProperties>().isFaceUp && Defender.transform.GetChild(0).GetChild(i).gameObject != card){
                                if(Defender.transform.GetChild(0).GetChild(i).tag != "Grunt" || percentRolls[i+Defender.transform.GetChild(2).childCount-4] > 40){
                                   Defender.transform.GetChild(0).GetChild(i).GetComponent<CardProperties>().dealDmg((percentRolls[i+Defender.transform.GetChild(2).childCount-4]-1)/25, card);
                                }
                            }
                        }
                    }
                }
                return 0;
            case "Jolteon":
                if(priority){
                    Defender.GetComponent<CardProperties>().isStunned=true;
                    if (0 <= Defender.transform.GetSiblingIndex()-1 && Defender.transform.GetSiblingIndex()-1 < Area.transform.childCount){
                        Area.transform.GetChild(Defender.transform.GetSiblingIndex()-1).GetComponent<CardProperties>().isStunned=true;
                    }
                    if (0 <= Defender.transform.GetSiblingIndex()+1 && Defender.transform.GetSiblingIndex()+1 < Area.transform.childCount){
                        Area.transform.GetChild(Defender.transform.GetSiblingIndex()+1).GetComponent<CardProperties>().isStunned=true;
                    }
                }
                return 0;
            case "Umbreon":
                if(card.transform.parent.GetSiblingIndex() == 2){
                   if(Area.tag == "Street"){
                        foreach (Transform child in Area.transform){
                            for (int i=2; i<child.GetChild(0).childCount; i++){
                                if (!child.GetChild(0).GetChild(i).GetComponent<CardProperties>().isDead && child.GetChild(0).GetChild(i).GetComponent<CardProperties>().isFaceUp){
                                    child.GetChild(0).GetChild(i).GetComponent<CardProperties>().dealDmg(1, card);
                                }
                            }
                        }
                        foreach (Transform child in Area.transform){
                            if(!child.GetComponent<CardProperties>().isDead && child.GetComponent<CardProperties>().isFaceUp){
                                child.GetComponent<CardProperties>().dealDmg(1, card);
                            }
                        }
                    }
                    if(Area.tag == "Opp Base Area"){
                        foreach (Transform child in Area.transform){
                            if(!child.GetComponent<CardProperties>().isDead && child.GetComponent<CardProperties>().isFaceUp){
                                child.GetComponent<CardProperties>().dealDmg(1, card);
                            }
                        }
                    }
                }
                else{
                    if(Area.tag == "Street"){
                        foreach (Transform child in Area.transform){
                            for (int i=2; i<child.GetChild(2).childCount; i++){
                                if (!child.GetChild(2).GetChild(i).GetComponent<CardProperties>().isDead && child.GetChild(2).GetChild(i).GetComponent<CardProperties>().isFaceUp){
                                    child.GetChild(2).GetChild(i).GetComponent<CardProperties>().dealDmg(1, card);
                                }
                            }
                        }
                        foreach (Transform child in Area.transform){
                            if(!child.GetComponent<CardProperties>().isDead && child.GetComponent<CardProperties>().isFaceUp){
                                child.GetComponent<CardProperties>().dealDmg(1, card);
                            }
                        }
                    }
                    if(Area.tag == "Player Base Area"){
                        foreach (Transform child in Area.transform){
                            if(!child.GetComponent<CardProperties>().isDead && child.GetComponent<CardProperties>().isFaceUp){
                                child.GetComponent<CardProperties>().dealDmg(1, card);
                            }
                        }
                    }
                }
                return 0;
            case "Vaporeon":
                if(card.transform.parent.GetSiblingIndex() == 2 && card.transform.GetSiblingIndex() == 2 && card.transform.parent.childCount == 4){
                    if (card.transform.parent.GetChild(3).tag == "SuperVillain"){
                        card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().healthInt += 4;
                        card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().superVillain.cardHealth += 4;
                        card.transform.parent.GetChild(3).GetComponent<CardProperties>().heal(4);
                    }
                    if (card.transform.parent.GetChild(3).tag == "Grunt"){
                        card.transform.parent.GetChild(3).GetComponent<CardProperties>().heal(4);
                    }
                }
                else if(card.transform.parent.GetSiblingIndex() == 0 && card.transform.GetSiblingIndex() == 2 && card.transform.parent.childCount == 4){
                    if (card.transform.parent.GetChild(3).tag == "SuperVillain"){
                        card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().healthInt += 4;
                        card.transform.parent.GetChild(3).GetComponent<SuperVillainDisplay>().superVillain.cardHealth += 4;
                    }
                }
                return 0;
            default:
                return 0;
        }
    }

    public void onAttackEnd(GameObject card, bool priority){
        string name = getName(card);
        GameObject Defender = card.transform.parent.parent.gameObject; // Gets the card being struck
        GameObject Area = card.transform.parent.parent.parent.gameObject; //Street Area, Player Base Area, Opp Base Area
        bool mine = false;
        if(card.transform.parent.GetSiblingIndex() == 2 || card.transform.parent.tag == "Player Base Area"){
            mine = true;
        }
        if(Defender.tag == "Location" && Defender.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Beach" || Defender.tag == "SuperHero" && Defender.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Beach" && !Defender.GetComponent<CardProperties>().isDead){
            priority = false;
        }
        switch (name){
        //SuperVillains
            case "Beedrill":
                if(!card.GetComponent<CardProperties>().onRevealAbility){
                    card.GetComponent<CardProperties>().onRevealAbility = true;
                    if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
                        break;
                    }
                    card.GetComponent<SuperVillainDisplay>().attackInt += -2;
                    card.GetComponent<SuperVillainDisplay>().updateCardInfo();
                }
                break;
            case "Eevee":
                if(!card.GetComponent<CardProperties>().eeveeAbility){
                    card.GetComponent<CardProperties>().eeveeAbility = true;
                    if (Area.tag == "Street" && Defender.transform.GetChild(0).childCount == 4 && Defender.transform.GetChild(2).childCount == 4 && Defender.GetComponent<CardProperties>().isDead && card.GetComponent<CardProperties>().OppAliveBeforeAttack){
                        card.GetComponent<GruntDisplay>().grunt.returnSuperVillain = "Umbreon";
                    }
                    else if(Area.tag == "Street" && Defender.transform.GetChild(card.transform.parent.GetSiblingIndex()).childCount == 4){
                        card.GetComponent<GruntDisplay>().grunt.returnSuperVillain = "Vaporeon";
                    }
                    else if (Area.tag == "Street" && Defender.transform.GetChild(card.transform.parent.GetSiblingIndex()).childCount == 3){
                        card.GetComponent<GruntDisplay>().grunt.returnSuperVillain = "Flareon";
                    }
                    else {
                        card.GetComponent<GruntDisplay>().grunt.returnSuperVillain = "Jolteon";
                    }
                }
                break;
            case "Golem":
                if(!Defender.GetComponent<CardProperties>().isDead){
                    if(Area.tag == "Street"){
                        for (int i=2; i < Defender.transform.GetChild(0).childCount; i++){
                            if (Defender.transform.GetChild(0).GetChild(i).GetComponent<CardProperties>().isFaceUp){
                                Defender.transform.GetChild(0).GetChild(i).GetComponent<CardProperties>().instaKill();
                            }
                        }   
                        for (int i=2; i < Defender.transform.GetChild(2).childCount; i++){
                            if (Defender.transform.GetChild(2).GetChild(i).GetComponent<CardProperties>().isFaceUp){
                                Defender.transform.GetChild(2).GetChild(i).GetComponent<CardProperties>().instaKill();
                            }
                        }
                    }
                    Defender.GetComponent<CardProperties>().instaKill();
                    card.GetComponent<CardProperties>().instaKill();
                }
                break;
            case "Pinsir":
                card.GetComponent<CardProperties>().onRevealAbility = false;
                break;
            case "Scyther":
                if(!card.GetComponent<CardProperties>().onRevealAbility){
                    card.GetComponent<CardProperties>().onRevealAbility = true;
                    if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
                        break;
                    }
                    if(priority){
                        GetComponent<PlayerManager>().moveCardAbility(card, "Player Base", null);
                    }
                }
                break;
            case "Tauros":
                if (mine && Defender.GetComponent<CardProperties>().isDead && card.GetComponent<CardProperties>().OppAliveBeforeAttack){
                    GameObject ChaosBox = GameObject.Find("Player Chaos Box");
                    ChaosBox.GetComponent<Chaos>().roundChaos += 2;
                    ChaosBox.GetComponent<Chaos>().totalChaos += 2;
                }
                break;
        //Grunts
            case "Pikachu":
                card.GetComponent<CardProperties>().singleUseCard = true;
                break;
        //Spawnables
            case "Staryu":
                GetComponent<PlayerManager>().moveCardAbility(card, "Player Base", null);
                break;
            case "Stinger":
                card.GetComponent<CardProperties>().singleUseCard = true;
                break;
            default:
                break;
        }
        attackEndAllChecks(card, name, Defender, Area, priority, mine);
    }

    public int onDefend(GameObject card, GameObject opp, bool priority){
        string name = getName(card);
        int baseDef = checkForChansey(card);
        if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Beach" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Beach" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
            priority = false;
        }
        if(card.transform.parent.tag == "Street" && card.tag == "SuperHero" || card.transform.parent.tag == "Street" && card.tag == "Civilian"){
            if (0 <= card.transform.GetSiblingIndex()-1 && card.transform.GetSiblingIndex()-1 < card.transform.parent.childCount){
                if(card.transform.parent.GetChild(card.transform.GetSiblingIndex()-1).GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Psychic Plains"){
                    baseDef = baseDef - 1;
                }
            }
            if (0 <= card.transform.GetSiblingIndex()+1 && card.transform.GetSiblingIndex()+1 < card.transform.parent.childCount){
                if(card.transform.parent.GetChild(card.transform.GetSiblingIndex()+1).GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Psychic Plains"){
                    baseDef = baseDef - 1;
                }
            }
        }
        switch (name){
        //SuperVillains
            case "Alakazam":
                if(card.transform.GetChild(0).childCount == 1 && card.transform.GetChild(2).childCount == 1 && card.transform.parent.parent.gameObject == opp){
                    return -2147483647;
                }
                return baseDef;
            case "Blastoise":
                return baseDef-1;
            case "Cloyster":
                if (opp != null){
                    opp.GetComponent<CardProperties>().dealDmg(1, card);
                    if (opp.tag == "SuperVillain"){
                        opp.GetComponent<SuperVillainDisplay>().healthInt += -2;
                        opp.GetComponent<SuperVillainDisplay>().superVillain.cardHealth += -2;
                    }
                    if (opp.tag == "Grunt"){
                        opp.GetComponent<GruntDisplay>().healthInt += -2;
                        opp.GetComponent<GruntDisplay>().grunt.cardHealth += -2;
                    }
                    if (opp.tag == "SuperHero"){
                        opp.GetComponent<SuperHeroDisplay>().health2Int += -2;
                        opp.GetComponent<SuperHeroDisplay>().superHero.cardHealth2 += -2;
                    }
                    if (opp.tag == "Civilian"){
                        opp.GetComponent<CivilianDisplay>().health2Int += -2;
                        opp.GetComponent<CivilianDisplay>().civilian.cardHealth2 += -2;
                    }
                    if (opp.tag == "Location"){
                        opp.GetComponent<LocationDisplay>().health2Int += -2;
                        opp.GetComponent<LocationDisplay>().location.cardHealth2 += -2;
                    }
                }
                return baseDef;
            case "Gengar":
                if(card.transform.GetChild(0).childCount == 1 && card.transform.GetChild(2).childCount == 1 && card.transform.parent.parent.gameObject != opp){
                    return -2147483647;
                }
                else return baseDef;
            case "Kabutops":
                if(priority && opp != null){
                    if(card.transform.GetChild(0).childCount == 1 && card.transform.GetChild(2).childCount == 1 && card.transform.parent.parent.gameObject != opp && opp.tag == "SuperHero"){
                        opp.GetComponent<CardProperties>().dealDmg(card.GetComponent<SuperVillainDisplay>().attackInt, card);
                    }
                }
                return baseDef;
            case "Rhydon":
                if(!card.GetComponent<CardProperties>().isDead && opp!=null){
                    card.GetComponent<SuperVillainDisplay>().superVillain.cardAttack = card.GetComponent<SuperVillainDisplay>().superVillain.cardAttack+2;
                    card.GetComponent<SuperVillainDisplay>().attackInt = card.GetComponent<SuperVillainDisplay>().attackInt+2;
                    card.GetComponent<SuperVillainDisplay>().updateCardInfo();
                }
                return baseDef;
            //Grunt
            case "Drowzee":
                if(card.transform.GetChild(0).childCount == 1 && card.transform.GetChild(2).childCount == 1 && card.transform.parent.parent.gameObject == opp && opp.tag == "SuperHero"){
                    return -2147483647;
                }
                return baseDef;
            case "Ghastly":
                if(card.transform.GetChild(0).childCount == 1 && card.transform.GetChild(2).childCount == 1 && card.transform.parent.parent.gameObject != opp && priority){
                    return -2147483647;
                }
                else return baseDef;
            //location
            case "Forest":
                return baseDef-1;
            case "Lightning Lands":
                if(opp.tag == "SuperVillain"){
                    if(card.tag == "Location" || card.tag == "SuperHero" && !card.GetComponent<CardProperties>().isDead){
                        opp.GetComponent<CardProperties>().dealDmg(1, null);
                    }
                }
                return baseDef;
            case "Volcano":
                if(priority){
                    return baseDef + 1;
                }
                return baseDef;
            default:
                return baseDef;
        }
    }

    public void onDeath(GameObject card, bool priority){
        string name = getName(card);
        bool mine = false;
        if(card.transform.parent.GetSiblingIndex() == 2 || card.transform.parent.tag == "Player Base Area"){
            mine = true;
        }
        if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Beach" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Beach" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
            priority = false;
        }
        switch (name){
        //SuperVillains
            case "Victreebel":
                GetComponent<PlayerManager>().recruitVillain(name, mine);
                break;
        //Grunts
            case "Magikarp": 
                Villain Gyarados = Resources.Load<Villain>("Spawnables/S Gyarados");
                GetComponent<PlayerManager>().gruntToVillain(card, Gyarados);
                break;
            case "Voltorb":
                if(card.transform.parent.parent.gameObject.layer == 6 || card.transform.parent.parent.gameObject.layer == 7 || card.transform.parent.parent.gameObject.layer == 10){
                    GameObject Defender = card.transform.parent.parent.gameObject;
                    GameObject Area = card.transform.parent.parent.parent.gameObject;
                    if(card.transform.parent.GetSiblingIndex() == 2){
                       if(Area.tag == "Street"){
                            foreach (Transform child in Defender.transform.GetChild(0)){
                                if(child.GetComponent<CardProperties>().isFaceUp){
                                    child.GetComponent<CardProperties>().dealDmg(2, card);
                                }
                            }
                        }
                        if(Area.tag == "Opp Base Area"){
                            if(Defender.GetComponent<CardProperties>().isFaceUp){
                                Defender.GetComponent<CardProperties>().dealDmg(2, card);
                            }
                        }
                    }
                    else{
                        if(Area.tag == "Street"){
                            foreach (Transform child in Defender.transform.GetChild(2)){
                                if(child.GetComponent<CardProperties>().isFaceUp){
                                    child.GetComponent<CardProperties>().dealDmg(2, card);
                                }
                            }
                        }
                        if(Area.tag == "Player Base Area"){
                            if(Defender.GetComponent<CardProperties>().isFaceUp){
                                Defender.GetComponent<CardProperties>().dealDmg(2, card);
                            }
                        }
                    }
                }
                break;
            default:
                break;
        }
    }

    public void onReveal(GameObject card, bool priority){
        if(!card.GetComponent<CardProperties>().hasRevealed){
            card.GetComponent<CardProperties>().hasRevealed = true;
            if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Creepy Mines" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
                return;
            }
            bool mine = false;
            if(card.transform.parent.GetSiblingIndex() == 2 || card.transform.parent.tag == "Player Base Area"){
                mine = true;
            }
            string name = getName(card);
        if(card.transform.parent.parent.tag == "Location" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Beach" || card.transform.parent.parent.tag == "SuperHero" && card.transform.parent.parent.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Beach" && !card.transform.parent.parent.GetComponent<CardProperties>().isDead){
                priority = false;
            }
            switch (name){
            //SuperVillains
                case "Beedrill":
                    GetComponent<PlayerManager>().recruitVillain(name, mine);
                    break;
                case "Persian":
                    if(priority){
                        GetComponent<PlayerManager>().recruitVillain(name, mine);
                    }
                    break;
            //Grunts
                case "Koffing":
                    if(mine){
                        GameObject ChaosBox = GameObject.Find("Player Chaos Box");
                        ChaosBox.GetComponent<Chaos>().roundChaos += 2;
                        ChaosBox.GetComponent<Chaos>().totalChaos += 2;
                    }
                    break;
                case "Nidorina":
                    GetComponent<PlayerManager>().recruitVillain(name, mine);
                    break;
                case "Nidorino":
                    GetComponent<PlayerManager>().recruitVillain(name, mine);
                    break;
                case "Seel":
                    if(priority){
                        GetComponent<PlayerManager>().hostBothDrawACard();
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void attackEndAllChecks(GameObject card, string name, GameObject Defender, GameObject Area, bool priority, bool mine){
    //Muk
        if(mine){
            for (int i=2; i<Defender.transform.GetChild(0).childCount; i++){
                if (Defender.transform.GetChild(0).GetChild(i).GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Muk" && Defender.transform.GetChild(0).GetChild(i).GetComponent<CardProperties>().isFaceUp){
                    card.GetComponent<CardProperties>().dealDmg(2, card);
                }
            }
        }
        else {
            for (int i=2; i<Defender.transform.GetChild(2).childCount; i++){
                if (Defender.transform.GetChild(2).GetChild(i).GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Muk" && Defender.transform.GetChild(0).GetChild(i).GetComponent<CardProperties>().isFaceUp){
                    card.GetComponent<CardProperties>().dealDmg(2, card);
                }
            }
        }
        //Check Player Base
        foreach (Transform vil in PlayerManager.Instance.PlayerBase.transform){
            if (mine && vil.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Alolan Golem"){
                PlayerManager.Instance.ChaosBox.GetComponent<Chaos>().roundChaos += 1;
                PlayerManager.Instance.ChaosBox.GetComponent<Chaos>().totalChaos += 1;
            }
        }
        //Check My side of Street
        foreach (Transform streetCard in PlayerManager.Instance.Street.transform){
            for (int i = 2; i<streetCard.GetChild(2).childCount; i++){
                if (mine && card.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text != "Alolan Golem" && streetCard.GetChild(2).GetChild(i).GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Alolan Golem"){
                    PlayerManager.Instance.ChaosBox.GetComponent<Chaos>().roundChaos += 1;
                    PlayerManager.Instance.ChaosBox.GetComponent<Chaos>().totalChaos += 1;
                }
            }
        }
        if (card.GetComponent<CardProperties>().OppAliveBeforeAttack && Defender.GetComponent<CardProperties>().isDead){
            //Check Player Base
            foreach (Transform vil in PlayerManager.Instance.PlayerBase.transform){
                if (mine && vil.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Alolan Geodude"){
                    PlayerManager.Instance.ChaosBox.GetComponent<Chaos>().roundChaos += 1;
                    PlayerManager.Instance.ChaosBox.GetComponent<Chaos>().totalChaos += 1;
                }
            }
            //Check My side of Street
            foreach (Transform streetCard in PlayerManager.Instance.Street.transform){
                for (int i = 2; i<streetCard.GetChild(2).childCount; i++){
                    if (mine && card.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text != "Alolan Geodude" && streetCard.GetChild(2).GetChild(i).GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Alolan Geodude"){
                        PlayerManager.Instance.ChaosBox.GetComponent<Chaos>().roundChaos += 1;
                        PlayerManager.Instance.ChaosBox.GetComponent<Chaos>().totalChaos += 1;
                    }
                }
            }
        }
        card.GetComponent<CardProperties>().OppAliveBeforeAttack = false;
    }

    public string getName(GameObject card){
        switch (card.tag){
            case "SuperVillain":
                return card.GetComponent<SuperVillainDisplay>().nameText.text;
            case "Grunt":
                return card.GetComponent<GruntDisplay>().nameText.text;
            case "SuperHero":
                return card.GetComponent<SuperHeroDisplay>().nameText.text;
            case "Civilian":
                return card.GetComponent<CivilianDisplay>().nameText.text;
            case "Location":
                return card.GetComponent<LocationDisplay>().nameText.text;
            default:
                return null;
        }
    }

    public int checkForChansey(GameObject card){
        int baseDef = 0;
        if(card.transform.parent.tag=="Player Base Area" || card.transform.parent.tag=="Opp Base Area"){
            foreach (Transform vil in card.transform.parent){
                if (vil.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Chansey"){
                    baseDef = -1;
                }
            }
        }
        return baseDef;
    }

    public void shuffleDeck(List<GameObject> deck){
        int n = deck.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            GameObject value = deck[k];  
            deck[k] = deck[n];  
            deck[n] = value;  
        }  
    }

}
