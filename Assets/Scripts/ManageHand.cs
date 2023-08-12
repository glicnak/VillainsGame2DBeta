using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManageHand : MonoBehaviour
{
    public GameObject Hand;
    public GameObject PlayerBase;
    public GameObject Street;

    public void adjustCardsInHand(){
        int HS = Hand.transform.childCount;
        adjustSpacing(HS);
        tiltHandCards(HS);
        LayoutRebuilder.ForceRebuildLayoutImmediate(Hand.GetComponent<RectTransform>());        
    }

    private void adjustSpacing(int HS){
        if (HS <2) {
            Hand.GetComponent<RadialLayout>().MinAngle = 0;
            Hand.GetComponent<RadialLayout>().StartAngle = 84;
        }
        else if(HS == 2){
            Hand.GetComponent<RadialLayout>().MinAngle = 12;
            Hand.GetComponent<RadialLayout>().StartAngle = 90;
        }
        else if(HS == 3){
            Hand.GetComponent<RadialLayout>().MinAngle = 18;
            Hand.GetComponent<RadialLayout>().StartAngle = 94;
        }
        else if(HS == 4){
            Hand.GetComponent<RadialLayout>().MinAngle = 22;
            Hand.GetComponent<RadialLayout>().StartAngle = 97;
        }
        else {
            Hand.GetComponent<RadialLayout>().MinAngle = HS*2+14;
            Hand.GetComponent<RadialLayout>().StartAngle = 94+HS;
        }
    }

    private void tiltHandCards(int HS){
        if(HS == 1){
            Hand.transform.GetChild(0).transform.rotation = Quaternion.Euler(0,0,6);
        }
        else if(HS == 2){
            Hand.transform.GetChild(0).transform.rotation = Quaternion.Euler(0,0,10);
            Hand.transform.GetChild(1).transform.rotation = Quaternion.Euler(0,0,3);
        }
        else {
            for (int i=0; i<HS; i++) {
                Hand.transform.GetChild(i).transform.rotation = Quaternion.Euler(0,0,7*HS-4*i-12);
            }
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
