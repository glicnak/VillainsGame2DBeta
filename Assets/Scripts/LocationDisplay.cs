using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Mirror;

public class LocationDisplay : NetworkBehaviour
{
    public Location location;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI health1Text;
    public TextMeshProUGUI health2Text;
    public Image artworkImage;
    public TextMeshProUGUI abilityText;
    public bool inRuins;
    public Action dmgFunction;
    public Action destroyFunction;

    [SyncVar]
    public int attackInt;
    [SyncVar]
    public int health2Int;
    [SyncVar]
    public int healthRemaining;


    public void setLocation(Location loc){
        location = loc;
        gameObject.tag = location.cardTag;
        nameText.text = location.cardName;
        attackInt = 0;
        health2Int = location.cardHealth2;
        healthRemaining = health2Int;
        inRuins = location.inRuins;
        abilityText.text = location.cardAbility;;
        string destination = "Card Images/Artwork/" + nameText.text;
        artworkImage.sprite = Resources.Load(destination, typeof(Sprite)) as Sprite;
        updateCardInfo();
    }
    
    public void updateCardInfo(){
        health2Text.text = health2Int.ToString();
    }

    public void dealDamage(int dmg){
        if(GetComponent<CardProperties>().isImmune){
            return;
        }
        if(dmg < 0){
           dmg = 0;
        }
        if (healthRemaining > 0){
            healthRemaining = healthRemaining - dmg;
            addDmgMarker(dmg);
        }
        if (healthRemaining <= 0){
            health2Text.text = "x";
            gameObject.GetComponent<CardProperties>().isDead = true;
            gameObject.GetComponent<CardProperties>().becomeBloodied();
        }    
        else {
            health2Text.text = healthRemaining.ToString();
        }
    }

    public void heal(int life){
        if (healthRemaining != 0){
            healthRemaining += life;
            health2Text.text = healthRemaining.ToString();
        }
    }

    public void setHealth(int life){
        if (life == -1){
            healthRemaining = health2Int;
        }
        else {
            healthRemaining = life;
        }
        health2Text.text = healthRemaining.ToString();
    }

    public void addDmgMarker(int dmg){
        if(dmg>0 && GetComponent<CardProperties>().isFaceUp){
            string boom = "-" + dmg.ToString();
            transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 2).GetComponent<Image>().enabled = true;
            transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 2).GetChild(0).GetComponent<TextMeshProUGUI>().text = boom;
            transform.GetChild(1).GetChild(transform.GetChild(1).childCount - 2).GetChild(0).GetComponent<TextMeshProUGUI>().enabled = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}