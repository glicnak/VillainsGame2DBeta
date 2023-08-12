using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Threading;

public class Confirm : NetworkBehaviour
{
    //Initialize variables
    public PlayerManager playerManager;
    public GameObject Card1;
    public GameObject Card2;
    public GameObject Card8;
    public Button ConfirmButton;
    public GameObject Hand;
    public GameObject PlayerBase;
	public GameObject OpponentBase;
    public GameObject Street;

    [SyncVar]
    public List<GameObject> playersMoving;
    public List<GameObject> playersPassing;
    public int numberPassClicks = 0;

    public void OnClick(){
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        if (playerManager.isPassPressed == true) {
            playerManager.confirmButtonClicks("pass");
        }
        else {
            playerManager.confirmButtonClicks("move");
        }
    }
   
    // Start is called before the first frame update
    void Start()
    {
        ConfirmButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
