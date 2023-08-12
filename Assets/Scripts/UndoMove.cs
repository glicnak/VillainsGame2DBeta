using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UndoMove : NetworkBehaviour
{
    public PlayerManager playerManager;
    public Button undoMoveButton;
    public Button ConfirmButton;
    public GameObject Hand;
    public GameObject PlayerBase;
	public GameObject OpponentBase;
    public GameObject Street;
    public GameObject ChaosBox;

    public void OnClick(){
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        foreach (GameObject card in playerManager.cardsPlayedThisTurn){
            sendBack(card);
            card.GetComponent<CardProperties>().playedThisTurn = false;
        }
        playerManager.moveLocation = null;
        playerManager.cardsPlayedThisTurn.Clear();
        Hand.GetComponent<ManageHand>().adjustCardsInHand();
        ChaosBox.GetComponent<Chaos>().updateChaosText();
        playerManager.noMoveButtons();
    }

    public void sendBack(GameObject card){
        card.transform.SetParent(card.transform.GetComponent<CardProperties>().roundStartArea.transform, false);
            if (card.transform.GetComponent<CardProperties>().roundStartArea == Hand) {
                card.transform.GetComponent<DragDrop>().makeCardBig();
            }
		playerManager.unfreezeCard(card);
    }

    // Start is called before the first frame update
    void Start()
    {
        undoMoveButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
