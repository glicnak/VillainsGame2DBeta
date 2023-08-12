using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardViewZoom : MonoBehaviour
{
    public GameObject ZoomCanvas;
    public GameObject zoomedSuperVillainTemplate;
    public GameObject zoomedGruntTemplate;
    public bool isHovered = false;
    private GameObject zoomedSuperVillain;
    private GameObject zoomedSpawnable;
    private GameObject ZoomCard;
    private GameObject zoomedCard;
    private Villain vil = null;
    private bool superVillainZoomed = false;
    private Villain[] allSuperVillainsArray;
    private Villain[] allSpawnablesArray;

    void Awake() {
        ZoomCanvas = GameObject.Find("Zoomed Cards");
        ZoomCard = transform.GetChild(0).gameObject;
        allSuperVillainsArray = Resources.LoadAll<Villain>("Villains/SuperVillains/");
        allSpawnablesArray = Resources.LoadAll<Villain>("Spawnables/");
    }

    public void onHoverEnter(){
        if (GameObject.Find("ZoomCard(Clone)")){
            GameObject oldOne = GameObject.Find("ZoomCard(Clone)");
            Destroy(oldOne);
        }
        makeZoomedExtraCards();
        makeZoomedGrunt();
        isHovered=true;
    }

    public void onHoverExit(){
        foreach (Transform child in ZoomCanvas.transform){
            Destroy(child.gameObject);
        }
        superVillainZoomed = false;
        isHovered = false;
    }

    private void makeZoomedGrunt(){
        float xCoord = transform.position.x + (960 - transform.position.x)/13;
        float yCoord = transform.position.y + (540 - transform.position.y)/5;
        zoomedCard = Instantiate(ZoomCard, new Vector2(xCoord,yCoord), transform.rotation);
        foreach (Transform child in zoomedCard.transform){
            if (child.GetComponent<Image>() != null){
                child.GetComponent<Image>().raycastTarget = false;
            }
        }
        zoomedCard.transform.SetParent(ZoomCanvas.transform, true);
        zoomedCard.transform.localScale = new Vector3(2,2,0);
    }

    private void makeZoomedExtraCards(){
        makeSuperVillainCards();
        makeSpawnableCards();
        zoomedSuperVillain.transform.SetAsLastSibling();
    }

    private void makeSpawnableCards(){
        for (int i=0; i<GetComponent<GruntDisplay>().grunt.spawnables.Count ; i++){
            for (int j=0; j<allSpawnablesArray.Length; j++){
                if (GetComponent<GruntDisplay>().grunt.spawnables[i] == allSpawnablesArray[j].cardName){
                    vil = allSpawnablesArray[j];
                }
            }
            if (vil == null){
                break;
            }
            GameObject template = zoomedSuperVillainTemplate;
            if(vil.cardTag == "Grunt"){
                template = zoomedGruntTemplate;
            }
            int deviation = (4*i);
            if (superVillainZoomed){
                deviation += 4;
            }
            zoomedSpawnable = Instantiate(template, new Vector2(1540+deviation,500+4*deviation), Quaternion.Euler(0, 0, -2*deviation));
            foreach (Transform child in zoomedSpawnable.transform){
                if (child.GetComponent<Image>() != null){
                    child.GetComponent<Image>().raycastTarget = false;
                }
            }
            zoomedSpawnable.transform.SetParent(ZoomCanvas.transform, true);
            zoomedSpawnable.transform.localScale = new Vector3(2.6f,2.6f,0);
            if(vil.cardTag == "SuperVillain"){
                zoomedSpawnable.GetComponent<SuperVillainDisplay>().setVillain(vil);
            }
            else if (vil.cardTag == "Grunt"){
                zoomedSpawnable.GetComponent<GruntDisplay>().setVillain(vil);
            }
            zoomedSpawnable.transform.SetAsFirstSibling();
        }
    }

    private void makeSuperVillainCards(){
        for (int i=0; i<allSuperVillainsArray.Length; i++){
            if (GetComponent<GruntDisplay>().grunt.returnSuperVillain == allSuperVillainsArray[i].cardName){
                vil = allSuperVillainsArray[i];
            }
        }
        if(vil == null){
            for (int i=0; i<allSpawnablesArray.Length; i++){
                if (GetComponent<GruntDisplay>().grunt.returnSuperVillain == allSpawnablesArray[i].cardName){
                    vil = allSpawnablesArray[i];
                }
            }
        }
        if (vil == null){
            return;
        }
        zoomedSuperVillain = Instantiate(zoomedSuperVillainTemplate, new Vector2(1540,500), transform.rotation);
        foreach (Transform child in zoomedSuperVillain.transform){
            if (child.GetComponent<Image>() != null){
                child.GetComponent<Image>().raycastTarget = false;
            }
        }
        zoomedSuperVillain.transform.SetParent(ZoomCanvas.transform, true);
        zoomedSuperVillain.transform.localScale = new Vector3(2.6f,2.6f,0);
        zoomedSuperVillain.GetComponent<SuperVillainDisplay>().setVillain(vil);
        superVillainZoomed = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space") && isHovered){
            //Cycle
            ZoomCanvas.transform.GetChild(ZoomCanvas.transform.childCount -2).SetAsFirstSibling();
        }
    }
}
