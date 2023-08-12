using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Roster : MonoBehaviour
{
    public GameObject GruntShowCaseTemplate;

    public void makeCharacter(Villain vil){
        Villain[] allGruntsArray = Resources.LoadAll<Villain>("Villains/Grunts/");
        foreach (Villain vilAll in allGruntsArray){
            if (vil.cardName == vilAll.cardName){
                GameObject card = Instantiate(GruntShowCaseTemplate, new Vector3(0, 0, 0), Quaternion.identity, GameObject.Find("Roster").transform);
                card.GetComponent<GruntViewDisplay>().setVillain(vil);
                card.transform.GetChild(0).GetChild(0).GetComponent<Image>().enabled = false;
                card.transform.GetChild(0).GetChild(1).GetComponent<Image>().enabled = false;
                card.transform.GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>().enabled = false;
                card.transform.GetChild(0).GetChild(4).GetComponent<TextMeshProUGUI>().enabled = false;
                card.transform.GetChild(0).GetChild(5).GetComponent<TextMeshProUGUI>().enabled = false;
                card.transform.GetChild(0).GetChild(2).localScale = new Vector3(2.4f,2.4f,0);
            }
        }
    }

    public void removeCharacter(Villain vil){
        foreach (Transform card in GameObject.Find("Roster").transform){
            if(vil.cardName == card.GetComponent<GruntViewDisplay>().grunt.cardName){
                Destroy(card.gameObject);
            }
        }
    }

}
