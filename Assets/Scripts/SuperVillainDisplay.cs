using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class SuperVillainDisplay : NetworkBehaviour
{
    public Villain superVillain;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI healthText;
    public Image artworkImage;
    public TextMeshProUGUI abilityText;
    public Action abilityFunction;
    
    [SyncVar]
    public int attackInt;
    [SyncVar]
    public int healthInt;
    [SyncVar]
    public int healthRemaining;

    public void setVillain(Villain vill){
        superVillain = vill;
        gameObject.tag = superVillain.cardTag;
        nameText.text = superVillain.cardName;
        attackInt = superVillain.cardAttack;
        healthInt = superVillain.cardHealth;
        healthRemaining = healthInt;
        abilityText.text = superVillain.cardAbility;
        string destination = "Card Images/Artwork/" + nameText.text;
        artworkImage.sprite = Resources.Load(destination, typeof(Sprite)) as Sprite;
        updateCardInfo();
    }

    public void updateCardInfo(){
        attackText.text = attackInt.ToString();
        healthText.text = healthInt.ToString();
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
            healthText.text = healthRemaining.ToString();
            addDmgMarker(dmg);
        }
        if (healthRemaining <=0) {
            healthText.text = "x";
            gameObject.GetComponent<CardProperties>().isDead = true;
            gameObject.GetComponent<CardProperties>().becomeBloodied();
        }
    }

    public void heal(int life){
        if (healthRemaining != 0){
            healthRemaining += life;
            healthText.text = healthRemaining.ToString();
        }
    }

    public void setHealth(int life){
        if (life == -1){
            healthRemaining = superVillain.cardHealth;
        }
        else {
            healthRemaining = life;
        }
        healthText.text = healthRemaining.ToString();
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