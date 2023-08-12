using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Pass : NetworkBehaviour
{
    public PlayerManager playerManager;
    public Button PassButton;

    public void OnClick(){
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.isPassPressed = true;
        playerManager.passMadeButtons();
    }

    // Start is called before the first frame update
    void Start()
    {
        PassButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
