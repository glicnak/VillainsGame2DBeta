using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillainScrollView : MonoBehaviour
{
    public GameObject GruntTemplate;
    public GameObject contentBox;

    void Start()
    {
        if(gameObject.name == "Grunt Scroll View"){
            Villain[] allGruntsArray = Resources.LoadAll<Villain>("Villains/Grunts/");
            foreach (Villain vil in allGruntsArray){
                GameObject card = Instantiate(GruntTemplate, new Vector3(0, 0, 0), Quaternion.identity);
                card.transform.SetParent(contentBox.transform);
                card.GetComponent<GruntDisplay>().setVillain(vil);
            }
        }
    }
}
