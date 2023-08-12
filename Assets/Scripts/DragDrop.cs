using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DragDrop : NetworkBehaviour
{
    //Initialize variables
    public GameObject Canvas;
    public GameObject Hand;
    public GameObject PlayerBase;
    public GameObject Street;
    public GameObject ChaosBox;
    public PlayerManager playerManager;
    private bool isDragging = false;
    private bool isInDropzone;
    private GameObject startParent;
    private Vector2 startPosition;
    private Quaternion startRotation;
    private GameObject Dropzone;
    private Vector3 bigSize = new Vector3(1.3f, 1.3f, 0);
    private Vector3 regSize = new Vector3(1, 1, 0);


    // Start is called before the first frame update
    void Start()
    {
        Canvas = GameObject.Find("Main Canvas");
        Hand = GameObject.Find("Hand");
        PlayerBase = GameObject.Find("Player Base Area");
        Street = GameObject.Find("Street Area");
        ChaosBox = GameObject.Find("Player Chaos Box");
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        isInDropzone = true;
        Dropzone = collision.gameObject;
        //Debug.Log("Colliding! with " + Dropzone);
    }

    private void OnCollisionExit2D(Collision2D collision) {
        Vector2 cardPoint = transform.position;
        Collider2D[] currentDesiredTarget = Physics2D.OverlapPointAll(cardPoint);
        if (currentDesiredTarget.Length < 1){
            isInDropzone = false;
            Dropzone = null;
            //Debug.Log("Uncolliding!");
        }
    }

    public void BeginDrag(){
        if (!isOwned) return;
        if (GetComponent<CardProperties>().isDead || GetComponent<CardProperties>().isStunned) return;
		if (transform.GetChild(0).childCount >1) return;
        isDragging = true;
        startParent = transform.parent.gameObject;
        makeCardBig();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void EndDrag(){
        if (!isOwned) return;
		if (transform.GetChild(0).childCount >1) return;
        if (string.Equals(LayerMask.LayerToName(transform.parent.gameObject.layer),"Opponent Base Cards")) {
            returnCardToArea();
        }
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        isDragging = false;
        if (isInDropzone){
            if (string.Equals(LayerMask.LayerToName(Dropzone.layer),"Locations")) {
                if (Dropzone.GetComponent<CardProperties>().isDead){
                    Debug.Log("its dead");
                    returnCardToArea();
                }
                else if (playerManager.moveLocation == null){
                    if (Dropzone.transform.GetChild(2).childCount == 2){
                        setCardAtLocation();
                        playerManager.moveLocation = Dropzone;                     
                    }
                    else {
                        returnCardToArea();
                    }
                }
                else if (GameObject.ReferenceEquals(Dropzone, playerManager.moveLocation)) {
                    if (Dropzone.transform.GetChild(2).childCount < 4){    
                        setCardAtLocation();
                    }
                    else {
                        returnCardToArea();
                    }
                }
                else {
                    returnCardToArea();
                }
            }
			else if (string.Equals(LayerMask.LayerToName(Dropzone.layer),"Opponent Base Cards")) {
                if (Dropzone.GetComponent<CardProperties>().isDead){
                    returnCardToArea();
                }
                if (playerManager.moveLocation == null){
                    if (Dropzone.transform.GetChild(2).childCount <2){
                        setCardAtLocation();
                        playerManager.moveLocation = Dropzone;                        
                    }
                    else {
                        returnCardToArea();
                    }
                }
                else {
                    returnCardToArea();	
                }
            }
            else if (Dropzone == PlayerBase){
                if (GetComponent<CardProperties>().roundStartArea == Hand) {
                    returnCardToArea();
                }
                else {
                    setCardAtLocation();
				    undoMove();
                }
            }
            else if (Dropzone == Hand){
                if (GetComponent<CardProperties>().roundStartArea == Hand) {
                    setCardAtLocation();
                }
                else {
                    returnCardToArea();
                }
				undoMove();
            } 
            else {
                setCardAtLocation();
                undoMove();
            }      
        }
        else {
            //If Collision didn't work...
            Vector2 cardPoint = transform.position;
            Collider2D[] currentDesiredTarget = Physics2D.OverlapPointAll(cardPoint);
            if (currentDesiredTarget.Length >1) {
                for (int i = 0; i < currentDesiredTarget.Length; i++){
                    if (string.Equals(LayerMask.LayerToName(currentDesiredTarget[i].gameObject.layer),"Locations")) {
                        Dropzone = currentDesiredTarget[i].gameObject;
                    }
                }
            collisionBackup();
            }
            //Otherwise send back
            else {
                Dropzone = startParent;
                returnCardToArea();
            }
        }
        Hand.GetComponent<ManageHand>().adjustCardsInHand();
    }

    private void collisionBackup(){
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        if (playerManager.moveLocation == null && Dropzone!= null){
            if (Dropzone.transform.GetChild(2).childCount <2){
                if (Dropzone.GetComponent<CardProperties>().isDead){
                    returnCardToArea();
                }
                else {
                    setCardAtLocation();
                    playerManager.moveLocation = Dropzone;  
                }
            }
        }
        else {
            returnCardToArea();
        }
    }

    private void setCardAtLocation() {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        transform.rotation = Quaternion.Euler(0,0,0);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        if (string.Equals(LayerMask.LayerToName(Dropzone.layer),"Locations")) {
            transform.SetParent(Dropzone.transform.GetChild(2), false);
            playerManager.cardsPlayedThisTurn.Add(gameObject);
            gameObject.GetComponent<CardProperties>().playedThisTurn = true;
            makeCardReg();
            playerManager.isPassPressed = false;
            playerManager.moveMadeButtons();
            playerManager.removePlayerPassing();
        }
		else if (string.Equals(LayerMask.LayerToName(Dropzone.layer),"Opponent Base Cards")) {
            transform.SetParent(Dropzone.transform.GetChild(2), false);
            playerManager.cardsPlayedThisTurn.Add(gameObject);
            gameObject.GetComponent<CardProperties>().playedThisTurn = true;
            makeCardReg();
            playerManager.isPassPressed = false;
            playerManager.moveMadeButtons();
            playerManager.removePlayerPassing();
        }
        else if (Dropzone == Hand) {
            transform.SetParent(Dropzone.transform, false);
            transform.SetAsFirstSibling();
            makeCardBig();
        }
        else {
            makeCardReg();
            transform.SetParent(Dropzone.transform, false);
        }
    }

    private void returnCardToArea() {
        transform.SetParent(startParent.transform, false);
        transform.position = startPosition;
        transform.rotation = startRotation;
        if (startParent != Hand){
            makeCardReg();
        }
        else if (startParent == Hand){
            transform.SetAsFirstSibling();
        }
        //Debug.Log("Return Card to " + Dropzone);
    }

    private void undoMove() {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        if (playerManager.moveLocation != null){
			GameObject lastPlayedLocation = playerManager.moveLocation;
            if (string.Equals(LayerMask.LayerToName(lastPlayedLocation.layer),"Locations") && lastPlayedLocation.transform.GetChild(2).childCount == 2) {
                playerManager.moveLocation = null; 
                playerManager.noMoveButtons();
            }
			else if (string.Equals(LayerMask.LayerToName(lastPlayedLocation.layer),"Opponent Base Cards")) {
                playerManager.moveLocation = null; 
                playerManager.noMoveButtons();
			}
        }
        gameObject.GetComponent<CardProperties>().playedThisTurn = false;
        playerManager.cardsPlayedThisTurn.Remove(gameObject);
    }   

    public void makeCardBig() { 
        transform.GetChild(1).localScale = bigSize;
    }

    public void makeCardReg() { 
        transform.GetChild(1).localScale = regSize;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragging){
            transform.rotation = Quaternion.Euler(0,0,0);
            transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y,1);
            transform.SetParent(Canvas.transform,true);
        }        
    }
}
