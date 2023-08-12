using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class Chaos : NetworkBehaviour
{
    public TextMeshProUGUI chaosText;
    public GameObject PlayerBase;
    public GameObject OpponentBase;
    public GameObject Street;
    public int totalChaos = 0;
    public int roundChaos = 0;

    public string updateChaosText(){
        chaosText.text = totalChaos + " (" + roundChaos + ")";
        return chaosText.text;
    }
   
}
