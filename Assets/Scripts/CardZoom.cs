using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardZoom : MonoBehaviour
{
    public GameObject ZoomCanvas;
    public bool isHovered = false;
    public bool extraCardsMade = false;
    private GameObject ZoomCard;
    private GameObject zoomedCard;
    public GameObject zoomedSuperVillainTemplate;
    public GameObject zoomedGruntTemplate;
    private GameObject zoomedSuperVillain;
    private GameObject zoomedSpawnable;
    private Villain vil = null;
    private bool vPressed = false;
    private bool superVillainZoomed = false;
    private Villain[] allSuperVillainsArray;
    private Villain[] allSpawnablesArray;



    void Awake() {
        ZoomCanvas = GameObject.Find("Zoomed Cards");
        ZoomCard = transform.GetChild(1).gameObject;
        allSuperVillainsArray = Resources.LoadAll<Villain>("Villains/SuperVillains/");
        allSpawnablesArray = Resources.LoadAll<Villain>("Spawnables/");
        if (PlayerManager.Instance != null && PlayerManager.Instance.vPressed){
            vPressed = true;
        }
    }

    public void onHoverEnter(){
        isHovered = true;
        if (transform.parent.gameObject.name == "Street Area" || transform.parent.gameObject.name == "Player Base Area" || transform.parent.gameObject.name == "Opponent Base"){
            if (!GameObject.Find("ZoomCard(Clone)")){
                makeZoomedCard();
            }
        }
        else {
            if (GameObject.Find("ZoomCard(Clone)")){
                GameObject oldOne = GameObject.Find("ZoomCard(Clone)");
                Destroy(oldOne);
            }
            foreach (Transform child in ZoomCanvas.transform){
                Destroy(child.gameObject);
            }
            if(vPressed){
                makeZoomedExtraCards();
            }
            makeZoomedCard();
        }
    }

    public void onHoverExit(){
        isHovered = false;
        extraCardsMade = false;
        superVillainZoomed = false;
        Destroy(zoomedCard);
        foreach (Transform child in ZoomCanvas.transform){
            Destroy(child.gameObject);
        }
    }

    private void makeZoomedCard(){
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
        if(isHovered){
            extraCardsMade = true;
            if(gameObject.tag == "Grunt"){
                makeSuperVillainCards();
            }
            makeSpawnableCards();
        }
    }

    private void makeSpawnableCards(){
        Villain currentVillain = null;
        if(gameObject.tag == "Grunt"){
            currentVillain = GetComponent<GruntDisplay>().grunt;
        }
        else if (gameObject.tag == "SuperVillain"){
            currentVillain = GetComponent<SuperVillainDisplay>().superVillain;
        }
        for (int i=0; i<currentVillain.spawnables.Count ; i++){
            for (int j=0; j<allSpawnablesArray.Length; j++){
                if (currentVillain.spawnables[i] == allSpawnablesArray[j].cardName){
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
            zoomedSpawnable = Instantiate(template, new Vector2(1755+deviation, 675 + 4*deviation), Quaternion.Euler(0, 0, -2*deviation));
            foreach (Transform child in zoomedSpawnable.transform){
                if (child.GetComponent<Image>() != null){
                    child.GetComponent<Image>().raycastTarget = false;
                }
            }
            zoomedSpawnable.transform.SetParent(ZoomCanvas.transform, true);
            zoomedSpawnable.transform.localScale = new Vector3(1.777f,1.777f,0);
            if(vil.cardTag == "SuperVillain"){
                zoomedSpawnable.GetComponent<SuperVillainDisplay>().setVillain(vil);
            }
            else if (vil.cardTag == "Grunt"){
                zoomedSpawnable.GetComponent<GruntViewDisplay>().setVillain(vil);
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
        zoomedSuperVillain = Instantiate(zoomedSuperVillainTemplate, new Vector2(1755,675), Quaternion.identity);
        foreach (Transform child in zoomedSuperVillain.transform){
            if (child.GetComponent<Image>() != null){
                child.GetComponent<Image>().raycastTarget = false;
            }
        }
        zoomedSuperVillain.transform.SetParent(ZoomCanvas.transform, true);
        zoomedSuperVillain.transform.SetAsFirstSibling();
        zoomedSuperVillain.transform.localScale = new Vector3(1.777f,1.777f,0);
        zoomedSuperVillain.GetComponent<SuperVillainDisplay>().setVillain(vil);
        superVillainZoomed = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(vPressed){
            if(Input.GetKeyDown("v")){
                vPressed = false;
                PlayerManager.Instance.vPressed = false;
                foreach(Transform child in ZoomCanvas.transform){
                    if (child.gameObject.name != "Card(Clone)"){
                        Destroy(child.gameObject);
                    }
                }
            }
        }
        else {
            if(Input.GetKeyDown("v")){
                vPressed = true;
                PlayerManager.Instance.vPressed = true;
                makeZoomedExtraCards();
            }
        }
        if (Input.GetKeyDown("space")){
            if(vPressed && isHovered && transform.parent.tag != "Street"){
                //Cycle
                List<GameObject> sideZooms = new List<GameObject>();
                foreach(Transform child in ZoomCanvas.transform){
                    if (child.gameObject.name != "Card(Clone)"){
                        sideZooms.Add(child.gameObject);
                    }
                }
                sideZooms[sideZooms.Count-1].transform.SetAsFirstSibling();
            }
        }
    }
}
