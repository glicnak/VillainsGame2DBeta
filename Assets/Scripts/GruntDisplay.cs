using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Mirror;

public class GruntDisplay : NetworkBehaviour
{
    public Villain grunt;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI attackText;
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
        grunt = vill;
        gameObject.tag = grunt.cardTag;
        nameText.text = grunt.cardName;
        attackInt = grunt.cardAttack;
        healthInt = 1;
        healthRemaining = 1;
        abilityText.text = grunt.cardAbility;
        string destination = "Card Images/Artwork/" + nameText.text;
        artworkImage.sprite = Resources.Load(destination, typeof(Sprite)) as Sprite;
        updateCardInfo();
    }
    
    public void updateCardInfo(){
        attackText.text = attackInt.ToString();
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
        if (healthRemaining <=0){
            gameObject.GetComponent<CardProperties>().isDead = true;
            gameObject.GetComponent<CardProperties>().becomeBloodied();
        }
    }

    public void heal(int life){
        if (healthRemaining != 0){
            healthRemaining += life;  
        }
    }

    public void setHealth(int life){
        if (life == -1){
            healthRemaining = grunt.cardHealth;
        }
        else {
            healthRemaining = life;
        }
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