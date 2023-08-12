using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class CardProperties : MonoBehaviour
{
    public Sprite cardBack;
    public GameObject roundStartArea;
    public int ruinsIndex = 0;
    public bool playedThisTurn;
    public bool isDead;
    public bool isFaceUp = false;
    public bool hasRevealed = false;
    public bool onRevealAbility = false;
    public bool eeveeAbility = false;
    public bool dittoChecked = false;
    public bool isStunned = false;
    public bool returnedEarly = false;
    public bool isImmune = false;
    public bool singleUseCard = false;
    public bool OppAliveBeforeAttack = true;
    public bool killedByHero = false;
    public bool svGlow = false;
    public bool isBloodied = false;
    private Vector3 bigSize = new Vector3(1.3f, 1.3f, 0);
    private Vector3 regSize = new Vector3(1, 1, 0);
    
    // Start is called before the first frame update
    void Start()
    {
    
    }

    public void dealDmg(int dmg, GameObject dealer){
        if (!isImmune){
            if(transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Cozy Path"){
                dmg = dmg-1;
            }
            if(dmg < 0){
                dmg = 0;
            }
            int realDmg = dmg;
            switch (gameObject.tag){
                case "SuperVillain":
                    if(gameObject.GetComponent<SuperVillainDisplay>().healthRemaining < dmg){
                        realDmg = gameObject.GetComponent<SuperVillainDisplay>().healthRemaining;
                    }
                    gameObject.GetComponent<SuperVillainDisplay>().dealDamage(realDmg);
                    break;
                case "Grunt":
                    if(gameObject.GetComponent<GruntDisplay>().healthRemaining < dmg){
                        realDmg = gameObject.GetComponent<GruntDisplay>().healthRemaining;
                    }
                    gameObject.GetComponent<GruntDisplay>().dealDamage(realDmg);
                    break;
                case "SuperHero":
                    if(gameObject.GetComponent<SuperHeroDisplay>().healthRemaining < dmg){
                        realDmg = gameObject.GetComponent<SuperHeroDisplay>().healthRemaining;
                    }
                    gameObject.GetComponent<SuperHeroDisplay>().dealDamage(realDmg);
                    break;
                case "Civilian":
                    if(gameObject.GetComponent<CivilianDisplay>().healthRemaining < dmg){
                        realDmg = gameObject.GetComponent<CivilianDisplay>().healthRemaining;
                    }
                    gameObject.GetComponent<CivilianDisplay>().dealDamage(realDmg);
                    break;
                case "Location":
                    if(gameObject.GetComponent<LocationDisplay>().healthRemaining < dmg){
                        realDmg = gameObject.GetComponent<LocationDisplay>().healthRemaining;
                    }
                    gameObject.GetComponent<LocationDisplay>().dealDamage(realDmg);
                    break;
            }
            if(dealer!= null){
                if (dealer.tag == "Grunt" || dealer.tag == "SuperVillain"){
                    getSpoils(realDmg, dealer);
                }
            }
        }
    }

    public void heal(int dmg){
        switch (gameObject.tag){
            case "SuperVillain":
                gameObject.GetComponent<SuperVillainDisplay>().heal(dmg);
                break;
            case "Grunt":
                gameObject.GetComponent<GruntDisplay>().heal(dmg);
                break;
            case "SuperHero":
                gameObject.GetComponent<SuperHeroDisplay>().heal(dmg);
                break;
            case "Civilian":
                gameObject.GetComponent<CivilianDisplay>().heal(dmg);
                break;
            case "Location":
                gameObject.GetComponent<LocationDisplay>().heal(dmg);
                break;
        }
    }

    public void setHealth(int a){
        switch (gameObject.tag){
            case "SuperVillain":
                gameObject.GetComponent<SuperVillainDisplay>().setHealth(a);
                break;
            case "Grunt":
                gameObject.GetComponent<GruntDisplay>().setHealth(a);
                break;
            case "SuperHero":
                gameObject.GetComponent<SuperHeroDisplay>().setHealth(a);
                break;
            case "Civilian":
                gameObject.GetComponent<CivilianDisplay>().setHealth(a);
                break;
            case "Location":
                gameObject.GetComponent<LocationDisplay>().setHealth(a);
                break;
        }
    }

    public void instaKill(){
        switch (gameObject.tag){
            case "SuperVillain":
                gameObject.GetComponent<SuperVillainDisplay>().dealDamage(gameObject.GetComponent<SuperVillainDisplay>().healthRemaining);
                break;
            case "Grunt":
                gameObject.GetComponent<GruntDisplay>().dealDamage(gameObject.GetComponent<GruntDisplay>().healthRemaining);
                break;
            case "SuperHero":
                if(!gameObject.GetComponent<SuperHeroDisplay>().inRuins){
                    gameObject.GetComponent<SuperHeroDisplay>().dealDamage(gameObject.GetComponent<SuperHeroDisplay>().healthRemaining);
                }
                break;
            case "Civilian":
                gameObject.GetComponent<CivilianDisplay>().dealDamage(gameObject.GetComponent<CivilianDisplay>().healthRemaining);
                break;
            case "Location":
                break;
            default:
                break;
        }
    }

    public void getSpoils(int chaos, GameObject dealer){
        if(gameObject.tag == "Grunt" && gameObject.GetComponent<CardProperties>().isDead){
            chaos = 1;
            if(dealer.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Ice Vulpix" || dealer.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Ice Ninetales"){
                chaos += chaos;
            }
            PlayerManager.Instance.chaosSpoils(1, dealer);
        }
        else {
            if(dealer.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Ice Vulpix" && gameObject.tag == "Civilian" || dealer.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Ice Ninetales"){
                chaos += chaos;
            }
            PlayerManager.Instance.chaosSpoils(chaos, dealer);
        }
    }

    public void removeDmgMarker(){
        transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 2).GetComponent<Image>().enabled = false;
        transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 2).GetChild(0).GetComponent<TextMeshProUGUI>().enabled = false;
    }

    public void cardFlipFront(){
        transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 1).GetComponent<Image>().enabled = false;
        isFaceUp = true;
    }

    public void cardFlipBack(){
        transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 1).GetComponent<Image>().enabled = true;
        isFaceUp = false;
    }

    public void makeCardBig() { 
        transform.GetChild(1).localScale = bigSize;
    }

    public void makeCardReg() { 
        transform.GetChild(1).localScale = regSize;
    }

    public void becomeBloodied(){
        if (isDead && !isBloodied && !transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 3).GetComponent<Image>().enabled){
            transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 3).GetComponent<Image>().enabled = true;
            isBloodied = true;
            if(gameObject.tag == "Location"){
                PlayerManager.Instance.incrementLocationsRuined();
            }
        }
    }

    public bool hasUnflipped(){
        bool hasUnflipped = false;
        for (int i = 2; i<transform.GetChild(0).childCount; i++){
            if(transform.GetChild(0).GetChild(i) != null){
                if(!transform.GetChild(0).GetChild(i).GetComponent<CardProperties>().isFaceUp){
                    hasUnflipped = true;
                }
            }
        }
        for (int i = 2; i<transform.GetChild(2).childCount; i++){
            if(transform.GetChild(2).GetChild(i) != null){
                if(!transform.GetChild(2).GetChild(i).GetComponent<CardProperties>().isFaceUp){
                    hasUnflipped = true;
                }
            }
        }
        return hasUnflipped;
    }

    // Update is called once per frame
    void Update()
    {
        if(!svGlow && killedByHero && GetComponent<NetworkIdentity>().isOwned && PlayerManager.Instance.ChaosBox.GetComponent<Chaos>().roundChaos >= PlayerManager.Instance.chaosThreshholdToPromote){
          svGlow = true;
          transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 4).GetComponent<Image>().enabled = true;
        }       

    }
}
