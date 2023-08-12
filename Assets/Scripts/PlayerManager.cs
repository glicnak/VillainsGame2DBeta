using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System.Threading;
using System.Linq;
using Random = System.Random;

public class PlayerManager : NetworkBehaviour
{
    public GameObject SuperVillainTemplate;
    public GameObject GruntTemplate;
    public GameObject LocationTemplate;
    public GameObject SuperHeroTemplate;
    public GameObject CivilianTemplate;
    public GameObject PlayerBase;
    public GameObject Hand;
    public GameObject OpponentBase;
    public GameObject Street;
    public GameObject PassButton;
    public GameObject ConfirmButton;
    public GameObject UndoMoveButton;
    public GameObject ChaosBox;
    public GameObject OpponentInfoBox;
    public GameObject Timer;
    public int cardsInHand;
    public string chaosText;
    public int oppTotalChaos;
    public int oppRoundChaos;
    public int chaosThreshholdToPromote = 6;
    public List<GameObject> cardsPlayedThisTurn;
    public GameObject moveLocation;
    public bool isPassPressed;
    public bool vPressed = false;
    public NetworkIdentity myId;
    public bool deckMade = false;
    public bool gameStarted = false;
    public bool gameEnded = false;
    public int rngPercent = 0;
    public GameObject priorityPlayer = null;
    public bool currentCardHasPriority = false;
    public bool confirmPressed = false;
    public List<Villain> deck;
    public List<int> percentRolls;
    public List<GameObject> baseCardOrder;
    public List<int> baseCardOrderInt;
    private Vector3 bigSize = new Vector3(1.3f, 1.3f, 0);
    private Vector3 regSize = new Vector3(1, 1, 0);

    private static Random rng = new Random();

    public static PlayerManager Instance;

    void Awake(){
        Instance = this;
    }

    public override void OnStartClient(){
        base.OnStartClient();
        PlayerBase = GameObject.Find("Player Base Area");
        Hand = GameObject.Find("Hand");
        OpponentBase = GameObject.Find("Opponent Base");
        Street = GameObject.Find("Street Area");
        PassButton = GameObject.Find("Pass Button");
        ConfirmButton = GameObject.Find("Confirm Button");
        UndoMoveButton = GameObject.Find("Undo Button");
        ChaosBox = GameObject.Find("Player Chaos Box");
        OpponentInfoBox = GameObject.Find("Opponent Info Box");
        Timer = GameObject.Find("Timer");
        allButtonsOff();
        DoTheStartyStuff();
    }
    
    public void DoTheStartyStuff(){
        makePlayerDeck();
        ChaosBox.GetComponent<Chaos>().totalChaos = 0;
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (gm.startGame()){
            TargetDrawStartingCards(getMyId().connectionToClient);
            CmdStartGame();
        }
    }
    
    [Command(requiresAuthority=false)]
    public void CmdStartGame(){
        generateRngPercent();
        generateRandomListForStreetChildren();
        StartCoroutine(gmStartGame());
    }

    [Server]
    IEnumerator gmStartGame(){
        yield return new WaitForSeconds(1.3f);
        CmdMakeLocation();
        yield return new WaitForSeconds(1.0f);
        CmdMakeLocation();
        yield return new WaitForSeconds(1.0f);
        CmdMakeLocation();
        yield return new WaitForSeconds(1.0f);
        CmdMakeLocation();
        yield return new WaitForSeconds(1.0f);
        CmdMakeLocation();
        yield return new WaitForSeconds(1.3f);
        RpcSetGameStarted();
        CmdResetTurnButtons();
        CmdUnfreezeAllCards();
        RpcResetTurnButtons();
    }

    [ClientRpc]
    void RpcSetGameStarted(){
        gameStarted = true;
    }

    private void makePlayerDeck(){
        if (!deckMade){
            DeckManager.Instance.makeDeckList();
            deck = DeckManager.Instance.myDeck;
            deckMade = true;
        }
    }

    public List<Villain> getDeck(){
        return DeckManager.Instance.myDeck;
    }

    public Villain getTopCard(){
        if(DeckManager.Instance.myDeck[0] != null){
            return DeckManager.Instance.myDeck[0];
        }
        else return null;
    }

    public NetworkIdentity getMyId(){
        return GetComponent<NetworkIdentity>();
    }

    [Command(requiresAuthority=false)]
    public void CmdTargetDrawStartingCards(NetworkIdentity hostId){
        TargetDrawStartingCards(hostId.connectionToClient);
    }

    [TargetRpc]
    public void TargetDrawStartingCards(NetworkConnection hostConnection){
        bothDrawStartingCards();
    }

    public void bothDrawStartingCards(){
        NetworkIdentity oppId = getOpponentId();
        CmdGetOppStartingCardInfo(oppId);
        drawStartingCards();
    }

    public void drawStartingCards(){
        List<Villain> myDeck = getDeck();
        NetworkIdentity myId = getMyId();
        for (int i=0; i<5; i++){
            CmdDrawAStartingCard(myDeck[0], i+2, myId);
            popTop();
        }
    }

    public void popTop(){
        DeckManager.Instance.removeDeckTop();
    }

    [Command(requiresAuthority=false)]
    void CmdGetOppStartingCardInfo(NetworkIdentity oppId){
        TargetGetOppStartingCardInfo(oppId.connectionToClient, oppId);
    }

    [TargetRpc]
    void TargetGetOppStartingCardInfo(NetworkConnection oppConnection, NetworkIdentity oppId){
        List<Villain> oppDeck = oppId.GetComponent<PlayerManager>().deck;
        for (int i=0; i<5; i++){
            CmdDrawAStartingCard(oppDeck[0], i, oppId);
            oppId.GetComponent<PlayerManager>().popTop();
        }
    }

    [Command(requiresAuthority=false)]
    void CmdDrawAStartingCard(Villain topCard, int cardCount, NetworkIdentity myId) {
        if (cardCount < 7 && topCard != null){
            if (topCard.cardTag == "SuperVillain"){
                GameObject card = Instantiate(SuperVillainTemplate, new Vector2(0,0), Quaternion.identity);
                NetworkServer.Spawn(card, myId.connectionToClient); 
                TargetShowCardInHand(myId.connectionToClient, card);
                RpcSetVillainInfo(card, topCard);
                RpcSetRoundStartHand(card); 
                RpcFreezeCard(card);
            }
            if (topCard.cardTag == "Grunt"){
                GameObject card = Instantiate(GruntTemplate, new Vector2(0,0), Quaternion.identity);
                NetworkServer.Spawn(card, myId.connectionToClient); 
                TargetShowCardInHand(myId.connectionToClient, card);
                RpcSetVillainInfo(card, topCard);
                RpcSetRoundStartHand(card); 
                RpcFreezeCard(card);
            }
        }
        UpdateChaosInfo();
    }

    public void noMoveButtons(){
        PassButton.GetComponent<Button>().interactable = true;
        ConfirmButton.GetComponent<Button>().interactable = false;
        UndoMoveButton.GetComponent<Button>().interactable = true;
    }

    public void moveMadeButtons(){
        PassButton.GetComponent<Button>().interactable = false;
        ConfirmButton.GetComponent<Button>().interactable = true;
        UndoMoveButton.GetComponent<Button>().interactable = true;
    }

    public void passMadeButtons(){
        PassButton.GetComponent<Button>().interactable = false;
        ConfirmButton.GetComponent<Button>().interactable = true;
        UndoMoveButton.GetComponent<Button>().interactable = false;
    }

    [ClientRpc]
    public void RpcAllButtonsOff(){
        allButtonsOff();
    }

    public void allButtonsOff(){
        PassButton.GetComponent<Button>().interactable = false;
        ConfirmButton.GetComponent<Button>().interactable = false;
        UndoMoveButton.GetComponent<Button>().interactable = false;
    }

    [ClientRpc]
    public void RpcBaseCardOrderClear(){
        baseCardOrder.Clear();
        baseCardOrderInt.Clear();
        foreach (Transform card in PlayerBase.transform){
            baseCardOrder.Add(card.gameObject);
            baseCardOrderInt.Add(card.GetSiblingIndex());
        }
    }

    public void fixBaseCardOrder(){
        for (int i = 0; i<baseCardOrder.Count; i++){
            if(baseCardOrder[i].transform.parent.tag == "Player Base Area"){
                baseCardOrder[i].transform.SetSiblingIndex(baseCardOrderInt[i]);
            }
            else {
                baseCardOrderInt[i] += -1;
            }
        }
    }

    [ClientRpc]
    public void RpcMakeCardBig(GameObject card){
        makeCardBig(card);
    }

    [ClientRpc]
    public void RpcMakeCardReg(GameObject card){
        makeCardReg(card);
    }
    
    public void makeCardBig(GameObject card) { 
        card.transform.GetChild(1).localScale = bigSize;
    }

    public void makeCardReg(GameObject card) { 
        card.transform.GetChild(1).localScale = regSize;
    }

    public int countMyCards(){
        int handSize = 0;
        foreach (Transform child in Street.transform){
            for (int i=2; i<child.GetChild(2).childCount;i++){
                if (child.GetChild(2).GetChild(i) != null && !child.GetChild(2).GetChild(i).GetComponent<CardProperties>().isDead){
                    handSize++;
                }
            }
        }
        handSize = handSize + PlayerBase.transform.childCount + Hand.transform.childCount;
        return handSize;
    }

    public void forceUndoMove(){
        UndoMoveButton.GetComponent<UndoMove>().OnClick();
    }

    public void hostBothDrawACard(){
        if (isServer){
            TargetBothDrawACard(GetComponent<NetworkIdentity>().connectionToClient);
        }
    }

    [TargetRpc]
    public void TargetBothDrawACard(NetworkConnection hostConnection){
        bothDrawACard();
    }

    public void bothDrawACard(){
        NetworkIdentity oppId = getOpponentId();
        CmdGetOppCardInfo(oppId);
        drawACard();
    }

    public void drawACard(){
        List<Villain> myDeck = getDeck();
        Villain topCard = null;
        int cardCount = countMyCards();
        if (myDeck.Count > 0 && cardCount < 7){
            topCard = getTopCard();
            popTop();
        }
        NetworkIdentity myId = getMyId();
        CmdDrawACard(topCard, cardCount, myId);   
    }

    [Command(requiresAuthority=false)]
    void CmdDrawACard(Villain topCard, int cardCount, NetworkIdentity myId) {
        if (cardCount < 7 && topCard != null){
            if (topCard.cardTag == "SuperVillain"){
                GameObject card = Instantiate(SuperVillainTemplate, new Vector2(0,0), Quaternion.identity);
                NetworkServer.Spawn(card, myId.connectionToClient); 
                TargetShowCardInHand(myId.connectionToClient, card);
                RpcSetVillainInfo(card, topCard);
                RpcSetRoundStartHand(card); 
            }
            if (topCard.cardTag == "Grunt"){
                GameObject card = Instantiate(GruntTemplate, new Vector2(0,0), Quaternion.identity);
                NetworkServer.Spawn(card, myId.connectionToClient); 
                TargetShowCardInHand(myId.connectionToClient, card);
                RpcSetVillainInfo(card, topCard);
                RpcSetRoundStartHand(card); 
            }
        }
        UpdateChaosInfo();
    }

    [Command(requiresAuthority=false)]
    void CmdGetOppCardInfo(NetworkIdentity oppId){
        TargetGetOppCardInfo(oppId.connectionToClient, oppId);
    }

    [TargetRpc]
    void TargetGetOppCardInfo(NetworkConnection oppConnection, NetworkIdentity oppId){
        List<Villain> oppDeck = oppId.GetComponent<PlayerManager>().deck;
        Villain oppTopCard = null;
        int oppTotalCards = countMyCards();
        if (oppDeck.Count > 0 && oppTotalCards < 7){
            oppTopCard = oppDeck[0];
            oppId.GetComponent<PlayerManager>().popTop();
        }
        CmdDrawACard(oppTopCard, oppTotalCards, oppId);
    }

    [ClientRpc]
    void RpcSetVillainInfo(GameObject card, Villain topCard){
        if (card.tag == "SuperVillain"){
            card.GetComponent<SuperVillainDisplay>().setVillain(topCard);
        }
        if (card.tag == "Grunt"){
            card.GetComponent<GruntDisplay>().setVillain(topCard);
        }
    }

    public void UpdateChaosInfo(){
        cardsInHand = getCardsInHand();
        chaosText = getChaosText();
        int totesChaos = ChaosBox.GetComponent<Chaos>().totalChaos;
        int roundsChaos = ChaosBox.GetComponent<Chaos>().roundChaos;
        NetworkIdentity opponentId = getOpponentId();
        if (isOwned){
            ChaosBox.GetComponent<Chaos>().updateChaosText();
            CmdUpdateOpponentInfo(opponentId, cardsInHand, chaosText, totesChaos, roundsChaos);
            CmdGetOppChaosInfo();
        }
    }

    [Command]
    public void CmdUpdateOpponentInfo(NetworkIdentity opponentId, int cardsInHand, string chaosText, int totesChaos, int roundsChaos){
        TargetUpdateOpponentInfo(opponentId.connectionToClient, opponentId, cardsInHand, chaosText, totesChaos, roundsChaos);   
    }

    [TargetRpc]
    void TargetUpdateOpponentInfo(NetworkConnection opponentConnection, NetworkIdentity opponentId, int cardsInHand, string chaosText, int totesChaos, int roundsChaos){
        OpponentInfoBox.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Cards: " + cardsInHand;
        OpponentInfoBox.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Chaos: " + chaosText;
        oppTotalChaos = totesChaos;
        oppRoundChaos = roundsChaos;
    }

    [Command(requiresAuthority=false)]
    void CmdGetOppChaosInfo(){
        NetworkIdentity oppId = getOpponentId();
        TargetGetOppChaosInfo(oppId.connectionToClient, GetComponent<NetworkIdentity>());
    }

    [TargetRpc]
    void TargetGetOppChaosInfo(NetworkConnection oppConnection, NetworkIdentity myId){
        int oppCardsInHand = getCardsInHand();
        string oppChaosText = getChaosText();
        int oppTotesChaos = ChaosBox.GetComponent<Chaos>().totalChaos;
        int oppRoundsChaos = ChaosBox.GetComponent<Chaos>().roundChaos;
        CmdReturnOppChaosInfo(myId, oppCardsInHand, oppChaosText, oppTotesChaos, oppRoundsChaos);
    }

    [Command(requiresAuthority=false)]
    void CmdReturnOppChaosInfo(NetworkIdentity myId, int oppCardsInHand, string oppChaosText, int oppTotesChaos, int oppRoundsChaos){
        TargetReturnOppChaosInfo(myId.connectionToClient, oppCardsInHand, oppChaosText, oppTotesChaos, oppRoundsChaos);
    }

    [TargetRpc]
    void TargetReturnOppChaosInfo(NetworkConnection myConnection, int oppCardsInHand, string oppChaosText, int oppTotesChaos, int oppRoundsChaos){
        OpponentInfoBox.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Cards: " + oppCardsInHand;
        OpponentInfoBox.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Chaos: " + oppChaosText;  
        oppTotalChaos = oppTotesChaos;
        oppRoundChaos = oppRoundsChaos;
    }
    
    [TargetRpc]
    void TargetShowCardInHand(NetworkConnection oppConnection, GameObject card){
        card.transform.rotation = Quaternion.identity;
        makeCardBig(card);
        card.transform.SetParent(Hand.transform, false);
        card.transform.SetAsLastSibling();
        Hand.GetComponent<ManageHand>().adjustCardsInHand();
    }

    [ClientRpc]
    void RpcShowRuinsCard(GameObject card, int index){
        card.GetComponent<CardProperties>().ruinsIndex = index;
        card.transform.rotation = Quaternion.identity;
        card.transform.SetParent(Street.transform,false);
        card.transform.SetSiblingIndex(index);
    }

    [ClientRpc]
    void RpcShowCard(GameObject card, string area, GameObject location){
        card.transform.rotation = Quaternion.identity;
        if (area == "Locations"){
            if(card.GetComponent<NetworkIdentity>().isOwned){
                card.transform.SetParent(location.transform.GetChild(2), false);
			    card.layer = 6;
            }
            else{
                card.transform.SetParent(location.transform.GetChild(0), false);
			    card.layer = 6;
            }
            makeCardReg(card);
            if (card.transform.GetChild(2).childCount > 1){
                callBackToBase(card.transform.GetChild(2).GetChild(1).gameObject, "Player Base", null);
            }
            if (card.transform.GetChild(0).childCount > 1){
                callBackToBase(card.transform.GetChild(0).GetChild(1).gameObject, "Player Base", null);
            }
            tiltCard(card);
        }
        else if (area == "Opponent Base Cards"){
            if(card.GetComponent<NetworkIdentity>().isOwned){
                card.transform.SetParent(location.transform.GetChild(2), false);
			    card.layer = 6;
            }
            else{
                card.transform.SetParent(location.transform.GetChild(0), false);
			    card.layer = 6;
            }
            makeCardReg(card);
            tiltCard(card);
        }
        else if (area == "Player Base Siege"){
            if(card.GetComponent<NetworkIdentity>().isOwned){
                card.transform.SetParent(location.transform.GetChild(2), false);
			    card.layer = 6;
            }
            else{
                card.transform.SetParent(location.transform.GetChild(0), false);
			    card.layer = 6;
            }
            makeCardReg(card);
            tiltCard(card);
        }
        else if (area == "Opponent Base"){
            if(card.GetComponent<NetworkIdentity>().isOwned){
                card.transform.SetParent(location.transform.GetChild(2), false);
			    card.layer = 6;
            }
            else{
                card.transform.SetParent(location.transform.GetChild(0), false);
			    card.layer = 6;
            }
            makeCardReg(card);
            tiltCard(card);
        }
        else if (area == "Street"){
            card.transform.SetParent(Street.transform,false);
            card.transform.SetAsLastSibling();
            makeCardReg(card);
            checkDestroyLastLocation();
        }
        else if (area == "Player Base"){
            if (card.GetComponent<NetworkIdentity>().isOwned){
                card.transform.SetParent(PlayerBase.transform, false);
                card.transform.GetComponent<DragDrop>().enabled=true;
                checkDestroyLastLocation();
            }
            else {
                card.transform.SetParent(OpponentBase.transform, false);
                card.transform.GetComponent<DragDrop>().enabled=true;
				card.layer = 10;
                checkDestroyLastLocation();
            }
            makeCardReg(card);
            tiltCard(card);
        }
        UpdateChaosInfo();
    }

    public void tiltCard(GameObject card){
        int rand = rng.Next(-3, 4);
        if(rand == 0){
            rand= -2;
        }
        float inc = rand*0.5f;
        card.transform.rotation = Quaternion.Euler(0, 0, inc);
    }

    public void confirmButtonClicks(string type){
        isPassPressed = false;
        if (type == "move"){
            CmdAddPlayersMoving();
        }
        if (type == "pass"){
            CmdAddPlayersPassing();
        }
        if (!Timer.GetComponent<Timer>().timerIsRunning){
            Timer.GetComponent<Timer>().startGreenTimer();
            CmdStartOppTimer();
        }
        fixBaseCardOrder();
        allButtonsOff();
        freezeAllCards();
        moveLocation = null;
        cardsPlayedThisTurn.Clear();
    }

    [Command]
    void CmdStartOppTimer(){
        NetworkIdentity oppId = getOpponentId();
        TargetRedTimer(oppId.connectionToClient);
    }

    [TargetRpc]
    void TargetRedTimer(NetworkConnection oppConnection){
        Timer.GetComponent<Timer>().startRedTimer();
    }
    
    [Command]
    void CmdConfirmTurn(){
        RpcStopTimers();
        if (ConfirmButton.GetComponent<Confirm>().playersMoving.Any()){
            RpcSetPriorityPlayer(ConfirmButton.GetComponent<Confirm>().playersMoving[0]);
            NetworkIdentity secondId = null;
            if (ConfirmButton.GetComponent<Confirm>().playersMoving.Count >= 2) {
                secondId = ConfirmButton.GetComponent<Confirm>().playersMoving[1].GetComponent<NetworkIdentity>();
            }
            TargetConfirmMove1st(ConfirmButton.GetComponent<Confirm>().playersMoving[0].GetComponent<NetworkIdentity>().connectionToClient, secondId);
        }
        else if (ConfirmButton.GetComponent<Confirm>().playersPassing.Any()){
            CmdConfirmPass();
            CmdEmptyPlayersMoving();
        }
    }

    [ClientRpc]
    public void RpcSetPriorityPlayer(GameObject player){
        priorityPlayer = player;
    }

    [ClientRpc]
    void RpcStopTimers(){
        Timer.GetComponent<Timer>().stopTimer();
    }

    [Command(requiresAuthority=false)]
    void CmdAddPlayersMoving(){
        RpcAddPlayersMoving(gameObject);
    }

    [ClientRpc]
    void RpcAddPlayersMoving(GameObject player){
        ConfirmButton.GetComponent<Confirm>().playersMoving.Add(player);
        if (ConfirmButton.GetComponent<Confirm>().playersMoving.Count + ConfirmButton.GetComponent<Confirm>().numberPassClicks >= 2 && isOwned) {
            CmdConfirmTurn();
        }
    }

    [Command(requiresAuthority=false)]
    void CmdEmptyPlayersMoving(){
        RpcEmptyPlayersMoving();
    }

    [ClientRpc]
    void RpcEmptyPlayersMoving(){
        ConfirmButton.GetComponent<Confirm>().playersMoving.Clear();
    }

    [Command(requiresAuthority = false)]
    void CmdAddPlayersPassing(){
        RpcAddPlayersPassing(gameObject);
    }

    [ClientRpc]
    void RpcAddPlayersPassing(GameObject player){
        ConfirmButton.GetComponent<Confirm>().playersPassing.Add(player);
        ConfirmButton.GetComponent<Confirm>().numberPassClicks++;  
        if (ConfirmButton.GetComponent<Confirm>().playersMoving.Count + ConfirmButton.GetComponent<Confirm>().numberPassClicks >= 2 && isOwned) {
            CmdConfirmTurn();
        }
    }

    public void removePlayerPassing(){
        CmdRemovePlayerPassing();
    }

    [Command(requiresAuthority = false)]
    void CmdRemovePlayerPassing(){
        RpcRemovePlayerPassing(gameObject);
    }

    [ClientRpc]
    void RpcRemovePlayerPassing(GameObject player){
        if(ConfirmButton.GetComponent<Confirm>().playersPassing.Contains(player)){
            ConfirmButton.GetComponent<Confirm>().playersPassing.Remove(player);
            ConfirmButton.GetComponent<Confirm>().numberPassClicks--; 
        }
    }

    [Command(requiresAuthority=false)]
    void CmdEmptyPlayersPassing(){
        RpcEmptyPlayersPassing();
    }

    [ClientRpc]
    void RpcEmptyPlayersPassing(){
        ConfirmButton.GetComponent<Confirm>().playersPassing.Clear();
        ConfirmButton.GetComponent<Confirm>().numberPassClicks = 0;
    }

    [TargetRpc]
    void TargetConfirmMove1st(NetworkConnection playerConnection, NetworkIdentity secondId){
        foreach (Transform child in Street.transform){
            for (int i = 2; i<child.GetChild(2).childCount; i++){
                GameObject card = child.GetChild(2).GetChild(i).gameObject;
                if (card.GetComponent<CardProperties>().playedThisTurn){
                    playCard(card, "Locations", card.transform.parent.parent.gameObject);
                    freezeCard(card);
                    card.GetComponent<CardProperties>().playedThisTurn = false;
                }
            }
        }
		foreach (Transform child in OpponentBase.transform){
            if (child.GetChild(2).childCount > 1){
			    GameObject card = child.GetChild(2).GetChild(1).gameObject;
                if (card.GetComponent<CardProperties>().playedThisTurn){
                    playCard(card, "Opponent Base Cards", card.transform.parent.parent.gameObject);
                    freezeCard(card);
                    card.GetComponent<CardProperties>().playedThisTurn = false;
                }
            }
		}
        UpdateChaosInfo();
        if (secondId != null){
            CmdConfirmMove2nd(secondId);
        }
        else {
            CmdReturnInvalidAttackers();
            CmdResolveCards();
            CmdConfirmPass();
            CmdEmptyPlayersMoving();
        }
    }

    [Command(requiresAuthority=false)]
    void CmdConfirmMove2nd(NetworkIdentity secondId){
        TargetConfirmMove2nd(secondId.connectionToClient);
    }

    [TargetRpc]
    void TargetConfirmMove2nd(NetworkConnection playerConnection){
        foreach (Transform child in Street.transform){
            for (int i = 2; i<child.GetChild(2).childCount; i++){
                GameObject card = child.GetChild(2).GetChild(i).gameObject;
                if (card.GetComponent<CardProperties>().playedThisTurn){
                    playCard(card, "Locations", card.transform.parent.parent.gameObject);
                    freezeCard(card);
                    card.GetComponent<CardProperties>().playedThisTurn = false;
                }
            }
        }
		foreach (Transform child in OpponentBase.transform){
            if (child.GetChild(2).childCount > 1){
			    GameObject card = child.GetChild(2).GetChild(1).gameObject;
                if (card.GetComponent<CardProperties>().playedThisTurn){
                    playCard(card, "Opponent Base Cards", card.transform.parent.parent.gameObject);
                    freezeCard(card);
                    card.GetComponent<CardProperties>().playedThisTurn = false;
                }
            }
		}
        UpdateChaosInfo();
        CmdReturnInvalidAttackers();
        CmdResolveCards();
        CmdEmptyPlayersMoving();
        CmdEmptyPlayersPassing();
    }
    
    [Command(requiresAuthority=false)]
    void CmdReturnInvalidAttackers(){
        Dictionary<GameObject,GameObject> cardsFromGM = gmGetCardsPlayedByAll();
        List<GameObject> cardsToRemove = new List<GameObject>();
        foreach (GameObject k in cardsFromGM.Keys){
            if (cardsFromGM.ContainsKey(cardsFromGM[k])){
                cardsToRemove.Add(k);
            }
        }
        foreach (GameObject card in cardsToRemove){
            gmRemoveFromCardsPlayedByAll(card);
        }
        RpcReturnInvalidAttackers(cardsToRemove);
    }

    [ClientRpc]
    void RpcReturnInvalidAttackers(List<GameObject> cardsToRemoveFromGM){
        foreach(GameObject card in cardsToRemoveFromGM){
            card.GetComponent<CardProperties>().cardFlipFront();
            callBackToBase(card, "Player Base", null);
            card.GetComponent<CardProperties>().playedThisTurn = false;
        }
        foreach (Transform child in PlayerBase.transform){
            if (child.GetChild(0).childCount > 1 && child.GetChild(0).GetChild(1).GetChild(2).childCount >1){
                child.GetChild(0).GetChild(1).gameObject.GetComponent<CardProperties>().cardFlipFront();
                callBackToBase(child.GetChild(0).GetChild(1).gameObject, "Player Base", null);
                child.GetChild(0).GetChild(1).gameObject.GetComponent<CardProperties>().playedThisTurn = false;
                child.GetChild(0).GetChild(1).GetChild(2).GetChild(1).gameObject.GetComponent<CardProperties>().cardFlipFront();
                callBackToBase(child.GetChild(0).GetChild(1).GetChild(2).GetChild(1).gameObject, "Player Base", null);
                child.GetChild(0).GetChild(1).GetChild(2).GetChild(1).gameObject.GetComponent<CardProperties>().playedThisTurn = false;
            }
        }
        foreach (Transform child in OpponentBase.transform){
            if (child.GetChild(2).childCount > 1 && child.GetChild(2).GetChild(1).GetChild(0).childCount >1){
                child.GetChild(2).GetChild(1).gameObject.GetComponent<CardProperties>().cardFlipFront();
                callBackToBase(child.GetChild(2).GetChild(1).gameObject, "Player Base", null);
                child.GetChild(2).GetChild(1).gameObject.GetComponent<CardProperties>().playedThisTurn = false;
                child.GetChild(2).GetChild(1).GetChild(0).GetChild(1).gameObject.GetComponent<CardProperties>().cardFlipFront();
                callBackToBase(child.GetChild(2).GetChild(1).GetChild(0).GetChild(1).gameObject, "Player Base", null);
                child.GetChild(2).GetChild(1).GetChild(0).GetChild(1).gameObject.GetComponent<CardProperties>().playedThisTurn = false;
            }
        }
    }

    [Command(requiresAuthority=false)]
    public void CmdRemoveFromCardsPlayedByAll(GameObject card){
        gmRemoveFromCardsPlayedByAll(card);
    }    

    [Server]
    void gmRemoveFromCardsPlayedByAll(GameObject card){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.removeFromCardsPlayedByAll(card);
    }

    [Server]
    Dictionary<GameObject,GameObject> gmGetCardsPlayedByAll(){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        return gm.cardsPlayedByAll;        
    }

    [Command(requiresAuthority=false)]
    void CmdResolveCards(){
            RpcSetGameStarted();
            StartCoroutine(gmResolveCards("Played"));
    }

    [Server]
    IEnumerator gmResolveCards(string what){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (what == "Played"){
            foreach (GameObject k in gm.cardsPlayedByAll.Keys){
                if(!k.GetComponent<CardProperties>().returnedEarly){
                    CmdCheckForPriority(k);
                    yield return new WaitForSeconds(0.3f);
                    RpcFlipCardUp(k);
                    RpcResolveAttack(k, k.transform.parent.parent.gameObject, gm.deadCardsThisRound);
                    yield return new WaitForSeconds(1.3f);
                    removeDmgMarkers();
                }
            }
            gm.clearCardsPlayedByAll();        
        }
        else if (what == "Location"){
            if (gm.newLocation != null){
                RpcFlipCardDown(gm.newLocation);
                yield return new WaitForSeconds(0.3f);
                RpcFlipCardUp(gm.newLocation);
            }
            gm.newLocation = null;
        }
        yield return new WaitForSeconds(0.1f);
        if (gm.locationsRuined > 2){
            CmdEndGame();
        }
        else if (gm.locationsRuined < 3 && !gameEnded && gameStarted){
            RpcBaseCardOrderClear();
            CmdResetGruntsTo1();
            RpcDoubleUpOnClearingMoveCardStuff();
            CmdResetTurnButtons();
            CmdUnfreezeAllCards();
        }
    }

    [ClientRpc]
    void RpcDoubleUpOnClearingMoveCardStuff(){
        moveLocation = null;
        cardsPlayedThisTurn.Clear();
    }

    public void playCard(GameObject card, string area, GameObject location){
        CmdPlayCards(card, area, location);
    }

    [Command(requiresAuthority=false)]
    void CmdPlayCards(GameObject card, string area, GameObject location){
        RpcShowCard(card, area, location);
        if (card.GetComponent<CardProperties>().roundStartArea == Hand){
            RpcFlipCardDown(card);
        }
        if (isServer){
            gmAddToCardsPlayedByAll(card, location);
            RpcFlipCardDown(card);
        }
    }

    [Server]
    void gmAddToCardsPlayedByAll(GameObject card, GameObject target){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.addToCardsPlayedByAll(card, target);
    }

    [Command(requiresAuthority=false)]
    void CmdConfirmPass(){
        RpcFillPlayersPassing();
        CmdEmptyPlayersMoving();
        StartCoroutine(gmConfirmPass());
    }

    [ClientRpc]
    void RpcFillPlayersPassing(){
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players){
            if (!ConfirmButton.GetComponent<Confirm>().playersPassing.Contains(p)){
                ConfirmButton.GetComponent<Confirm>().playersPassing.Add(p);
            }
        }
    }

    [Server]
    IEnumerator gmConfirmPass(){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        float time = gm.cardsPlayedByAll.Count * 1.5f + 0.3f;
        yield return new WaitForSeconds(time);
        RpcAllButtonsOff();
        removeDmgMarkers();
        TargetCallBackCards(ConfirmButton.GetComponent<Confirm>().playersPassing[0].GetComponent<NetworkIdentity>().connectionToClient);
        foreach (GameObject deadCard in gm.deadCardsThisRound){
            if (deadCard != null){
                int siblingIndex = deadCard.transform.GetSiblingIndex();
                string deadCardName = deadCard.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text;
                string deadCardAbility = deadCard.transform.GetChild(1).GetChild(5).GetComponent<TextMeshProUGUI>().text;
                bool wasInStreet = false;
                if (deadCard.transform.parent.gameObject == Street && deadCard.tag == "Location"){
                    wasInStreet = true;
                }
                StartCoroutine(gmDestroyCard(deadCard));
                if (wasInStreet){
                    StartCoroutine(gmReplaceRuins(siblingIndex, deadCardName, deadCardAbility));
                }
            }
            yield return new WaitForSeconds(0.4f);
            RpcAllButtonsOff();
        }
        if (gm.locationsRuined > 2){
            CmdEndGame();
        }
        else if (gm.locationDeck.Count == 0){
            CmdEarlyEndGame();
        }
        else {
            if (gm.locationDeck.Count > 0){
                CmdMakeLocation();
            }
            RpcAddSVCardsToDecks();
            yield return new WaitForSeconds(0.1f);
            TargetBothDrawACard(getMyId().connectionToClient);
            yield return new WaitForSeconds(0.2f);
            RpcResetTurnChaos();
            CmdUnstunAllCards();
            CmdRemoveImmuneAllCards();
            CmdEmptyPlayersPassing();
            gm.clearCardsPlayedByAll();  
            gm.deadCardsThisRound.Clear();
            CmdResetTurnButtons();
            CmdUnfreezeAllCards();
        }
    }   

    [ClientRpc]
    void RpcAddSVCardsToDecks(){
        addSVCardsToDeck();
    }

    void addSVCardsToDeck(){
        if(ChaosBox.GetComponent<Chaos>().roundChaos >= chaosThreshholdToPromote){
            DeckManager.Instance.addSVToDeck();
        }
        DeckManager.Instance.clearSVList();
    }

    [TargetRpc]
    void TargetCallBackCards(NetworkConnection playerConnection){
        foreach (Transform child in Street.transform){
            if (child.GetChild(2).childCount == 3){
                GameObject card = child.GetChild(2).GetChild(2).gameObject;
                callBackToBase(card, "Player Base", null);
            }
            else if (child.GetChild(2).childCount == 4){
                GameObject card1 = child.GetChild(2).GetChild(2).gameObject;
                GameObject card2 = child.GetChild(2).GetChild(3).gameObject;
                callBackToBase(card1, "Player Base", null);
                callBackToBase(card2, "Player Base", null);
            }
        }
		foreach (Transform child in OpponentBase.transform){
            if (child.GetChild(2).childCount > 1){
			    callBackToBase(child.GetChild(2).GetChild(1).gameObject, "Player Base", null);
            }
		}
        CmdCallBackCards2nd(ConfirmButton.GetComponent<Confirm>().playersPassing[1].GetComponent<NetworkIdentity>());
        UpdateChaosInfo();
    }

    [Command(requiresAuthority=false)]
    void CmdCallBackCards2nd(NetworkIdentity secondId){
        TargetCallBackCards2nd(secondId.connectionToClient);
    }

    [TargetRpc]
    void TargetCallBackCards2nd(NetworkConnection secondConnection){
        foreach (Transform child in Street.transform){
            if (child.GetChild(2).childCount == 3){
                GameObject card = child.GetChild(2).GetChild(2).gameObject;
                callBackToBase(card, "Player Base", null);
            }
            else if (child.GetChild(2).childCount == 4){
                GameObject card1 = child.GetChild(2).GetChild(2).gameObject;
                GameObject card2 = child.GetChild(2).GetChild(3).gameObject;
                callBackToBase(card1, "Player Base", null);
                callBackToBase(card2, "Player Base", null);
            }
        }
		foreach (Transform child in OpponentBase.transform){
            if (child.GetChild(2).childCount > 1){
			    callBackToBase(child.GetChild(2).GetChild(1).gameObject, "Player Base", null);
            }
		}
        UpdateChaosInfo();
        CmdEmptyPlayersPassing();
    }

    public void callBackToBase(GameObject card, string area, GameObject location){
        CmdcallBackToBase(card, area, location);
    }

    [Command(requiresAuthority = false)]
    void CmdcallBackToBase(GameObject card, string area, GameObject location){
        if(card != null){
            if (card.GetComponent<CardProperties>().isDead){
                gmDestroyCard(card);
            }
            else if(card.GetComponent<CardProperties>().singleUseCard){
                RpcDestroy(card);
            }
            else {
                RpcShowCard(card, area, location); 
                RpcSetRoundStartBase(card);
            }
        }
    }

    [ClientRpc]
    public void RpcDestroy(GameObject card){
        if (card!= null){
            Object.Destroy(card);
        }
    }

    public void moveCardAbility(GameObject card, string area, GameObject location){
        CmdMoveCardAbility(card, area, location);
    }
    
    [Command(requiresAuthority = false)]
    void CmdMoveCardAbility(GameObject card, string area, GameObject location){
        StartCoroutine(gmMoveCardAbility(card, area, location));
    }    

    [Server]
    IEnumerator gmMoveCardAbility(GameObject card, string area, GameObject location){
        if(card != null){
            if(card.GetComponent<CardProperties>().singleUseCard){
                RpcDestroy(card);
            }
            else {
                RpcFlipCardUp(card);
                yield return new WaitForSeconds(0.4f); 
                RpcShowCard(card, area, location); 
                RpcOnMoveAbility(card,area,location);
            }
        }
    }

    [ClientRpc]
    public void RpcOnMoveAbility(GameObject card, string area, GameObject location){
        //Check My Base
        foreach (Transform vil in PlayerBase.transform){
            if (vil.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Ice Sandshrew"){
                ChaosBox.GetComponent<Chaos>().roundChaos += 1;
                ChaosBox.GetComponent<Chaos>().totalChaos += 1;
            }
            if (vil.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Ice Sandslash"){
                ChaosBox.GetComponent<Chaos>().roundChaos += 2;
                ChaosBox.GetComponent<Chaos>().totalChaos += 2;
            }
            if (vil.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Aerodactyl"){
                vil.GetComponent<SuperVillainDisplay>().attackInt += 1;
                vil.GetComponent<SuperVillainDisplay>().superVillain.cardAttack += 1;
                vil.GetComponent<SuperVillainDisplay>().updateCardInfo();
            }
        }
        //Check my side of the Street
        foreach (Transform streetCard in Street.transform){
            for (int i = 2; i<streetCard.GetChild(2).childCount; i++){
                if (streetCard.GetChild(2).GetChild(i).GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Ice Sandshrew"){
                    ChaosBox.GetComponent<Chaos>().totalChaos += 1;
                }
                if (streetCard.GetChild(2).GetChild(i).GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Ice Sandslash"){
                    ChaosBox.GetComponent<Chaos>().totalChaos += 2;
                }
                if (streetCard.GetChild(2).GetChild(i).GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text == "Aerodactyl"){
                    streetCard.GetChild(2).GetChild(i).GetComponent<SuperVillainDisplay>().attackInt += 1;
                    streetCard.GetChild(2).GetChild(i).GetComponent<SuperVillainDisplay>().superVillain.cardAttack += 1;
                    streetCard.GetChild(2).GetChild(i).GetComponent<SuperVillainDisplay>().updateCardInfo();
                }
            }
        }
    }

    public void resolveAttack(GameObject Attacker, GameObject Defender){
        CmdResolveAttack(Attacker, Defender);
    }

    [Command(requiresAuthority=false)]
    void CmdResolveAttack(GameObject Attacker, GameObject Defender){
        StartCoroutine(gmResolveAttack(Attacker, Defender));
    }

    [Server]
    IEnumerator gmResolveAttack(GameObject Attacker, GameObject Defender){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        CmdCheckForPriority(Attacker);
        yield return new WaitForSeconds(0.4f);
        RpcResolveAttack(Attacker, Defender, gm.deadCardsThisRound);
        yield return new WaitForSeconds(0.6f);
        removeDmgMarkers();
    }

    [ClientRpc]
    void RpcResolveAttack(GameObject Attacker, GameObject Defender, List<GameObject> deadCardsThisRound){
        if(Attacker.GetComponent<CardProperties>().returnedEarly){
            UpdateChaosInfo();
            return;
        }
        if(Defender.layer != 6 && Defender.layer != 7 && Defender.layer != 10){
            UpdateChaosInfo();
            return;
        }
        GameObject Helper = null;
        int attackerPower = 0 + onAttack(Attacker) + onDefend(Defender, Attacker);
        if(!Attacker.GetComponent<CardProperties>().isDead){
            if (Attacker.tag == "SuperVillain"){
                attackerPower += Attacker.GetComponent<SuperVillainDisplay>().attackInt;
            }
            if (Attacker.tag == "Grunt"){
                attackerPower += Attacker.GetComponent<GruntDisplay>().attackInt;
            }
        }
        if (Defender.tag == "SuperHero"){
            if (Attacker.tag == "SuperVillain" && !Defender.GetComponent<CardProperties>().isDead){
                Defender.GetComponent<CardProperties>().dealDmg(attackerPower, Attacker);
                Attacker.GetComponent<CardProperties>().dealDmg(Defender.GetComponent<SuperHeroDisplay>().attackInt + onDefend(Attacker, Defender), null);
            }
            if (Attacker.tag == "Grunt" && !Defender.GetComponent<CardProperties>().isDead){
                Defender.GetComponent<CardProperties>().dealDmg(attackerPower, Attacker);
                Attacker.GetComponent<CardProperties>().dealDmg(Defender.GetComponent<SuperHeroDisplay>().attackInt + onDefend(Attacker, Defender), null);
            }
        }
        else if (Defender.tag == "Civilian"){
            Defender.GetComponent<CardProperties>().dealDmg(attackerPower, Attacker);
            int helperPower = 0;
            if (0 <= Defender.transform.GetSiblingIndex()-1 && Defender.transform.GetSiblingIndex()-1 < Street.transform.childCount && Street.transform.GetChild(Defender.transform.GetSiblingIndex()-1).gameObject.tag == "SuperHero"){
                if (Street.transform.GetChild(Defender.transform.GetSiblingIndex()-1).GetComponent<SuperHeroDisplay>().healthRemaining > 0 && !Street.transform.GetChild(Defender.transform.GetSiblingIndex()-1).GetComponent<CardProperties>().isStunned){
                    helperPower = Street.transform.GetChild(Defender.transform.GetSiblingIndex()-1).GetComponent<SuperHeroDisplay>().attackInt;
                    Helper = Street.transform.GetChild(Defender.transform.GetSiblingIndex()-1).gameObject;
                }
            }
            if (0 <= Defender.transform.GetSiblingIndex()+1 && Defender.transform.GetSiblingIndex()+1 < Street.transform.childCount && Street.transform.GetChild(Defender.transform.GetSiblingIndex()+1).gameObject.tag == "SuperHero"){
                if (Street.transform.GetChild(Defender.transform.GetSiblingIndex()+1).GetComponent<SuperHeroDisplay>().healthRemaining > 0 && !Street.transform.GetChild(Defender.transform.GetSiblingIndex()+1).GetComponent<CardProperties>().isStunned){
                    if (helperPower < Street.transform.GetChild(Defender.transform.GetSiblingIndex()+1).GetComponent<SuperHeroDisplay>().attackInt){
                        helperPower = Street.transform.GetChild(Defender.transform.GetSiblingIndex()+1).GetComponent<SuperHeroDisplay>().attackInt;
                        Helper = Street.transform.GetChild(Defender.transform.GetSiblingIndex()+1).gameObject;
                    }
                }
            }
            if (Attacker.tag == "SuperVillain"){
                Attacker.GetComponent<CardProperties>().dealDmg(Defender.GetComponent<CivilianDisplay>().attackInt + helperPower + onDefend(Attacker, Helper), null);
            }
            if (Attacker.tag == "Grunt"){
                Attacker.GetComponent<CardProperties>().dealDmg(Defender.GetComponent<CivilianDisplay>().attackInt + helperPower + onDefend(Attacker, Helper), null);
            }
        }
        else if (Defender.tag == "Location"){
            Defender.GetComponent<CardProperties>().dealDmg(attackerPower, Attacker);
            int helperPower = 0;
            if (0 <= Defender.transform.GetSiblingIndex()-1 && Defender.transform.GetSiblingIndex()-1 < Street.transform.childCount && Street.transform.GetChild(Defender.transform.GetSiblingIndex()-1).gameObject.tag == "SuperHero"){
                if (Street.transform.GetChild(Defender.transform.GetSiblingIndex()-1).GetComponent<SuperHeroDisplay>().healthRemaining > 0 && !Street.transform.GetChild(Defender.transform.GetSiblingIndex()-1).GetComponent<CardProperties>().isStunned){
                    helperPower = Street.transform.GetChild(Defender.transform.GetSiblingIndex()-1).GetComponent<SuperHeroDisplay>().attackInt;
                    Helper = Street.transform.GetChild(Defender.transform.GetSiblingIndex()-1).gameObject;
                }
            }
            if (0 <= Defender.transform.GetSiblingIndex()+1 && Defender.transform.GetSiblingIndex()+1 < Street.transform.childCount && Street.transform.GetChild(Defender.transform.GetSiblingIndex()+1).gameObject.tag == "SuperHero"){
                if (Street.transform.GetChild(Defender.transform.GetSiblingIndex()+1).GetComponent<SuperHeroDisplay>().healthRemaining > 0 && !Street.transform.GetChild(Defender.transform.GetSiblingIndex()+1).GetComponent<CardProperties>().isStunned){
                    if(helperPower <= Street.transform.GetChild(Defender.transform.GetSiblingIndex()+1).GetComponent<SuperHeroDisplay>().attackInt){
                        helperPower = Street.transform.GetChild(Defender.transform.GetSiblingIndex()+1).GetComponent<SuperHeroDisplay>().attackInt;
                        Helper = Street.transform.GetChild(Defender.transform.GetSiblingIndex()+1).gameObject;
                    }
                }
            }
            if (Attacker.tag == "SuperVillain"){
                Attacker.GetComponent<CardProperties>().dealDmg(Defender.GetComponent<LocationDisplay>().attackInt + helperPower + onDefend(Attacker, Helper), null);
            }
            if (Attacker.tag == "Grunt"){
                Attacker.GetComponent<CardProperties>().dealDmg(Defender.GetComponent<LocationDisplay>().attackInt + helperPower + onDefend(Attacker, Helper), null);
            }
        }
        else if (Defender.tag == "SuperVillain"){
            Defender.GetComponent<CardProperties>().dealDmg(attackerPower, Attacker);
            if (Attacker.tag == "SuperVillain"){
                Attacker.GetComponent<CardProperties>().dealDmg(Defender.GetComponent<SuperVillainDisplay>().attackInt + onDefend(Attacker, Defender), null);
            }
            if (Attacker.tag == "Grunt"){
                Attacker.GetComponent<CardProperties>().dealDmg(Defender.GetComponent<SuperVillainDisplay>().attackInt + onDefend(Attacker, Defender), null);
            }
        }
        else if (Defender.tag == "Grunt"){
            Defender.GetComponent<CardProperties>().dealDmg(attackerPower, Attacker);
            if (Attacker.tag == "SuperVillain"){
                Attacker.GetComponent<CardProperties>().dealDmg(Defender.GetComponent<GruntDisplay>().attackInt + onDefend(Attacker, Defender), null);
            }
            if (Attacker.tag == "Grunt"){
                Attacker.GetComponent<CardProperties>().dealDmg(Defender.GetComponent<GruntDisplay>().attackInt + onDefend(Attacker, Defender), null);
            }
        }
        if(Helper != null){
            CmdMakeHelperPop(Helper);
        }
        onAttackEnd(Attacker);
        if (Attacker !=null && Attacker.GetComponent<CardProperties>().isDead && !deadCardsThisRound.Contains(Attacker) && !Attacker.GetComponent<CardProperties>().hasUnflipped()){
            CmdAddDeadCard(Attacker);
            Attacker.GetComponent<CardProperties>().becomeBloodied();
            onDeath(Attacker);
            if(Defender.tag == "SuperHero" || Helper != null && Helper.tag == "SuperHero"){
                if(Attacker.GetComponent<NetworkIdentity>().isOwned && Attacker.tag == "Grunt"){
                    addVillainFromGruntCard(Attacker);
                }
                if(Attacker.GetComponent<NetworkIdentity>().isOwned && Attacker.tag == "SuperVillain"){
                    addVillainFromSVCard(Attacker);
                }
            }
        }
        if (Defender != null && Defender.GetComponent<CardProperties>().isDead && !deadCardsThisRound.Contains(Defender) && !Defender.GetComponent<CardProperties>().hasUnflipped()){
            CmdAddDeadCard(Defender);
            Defender.GetComponent<CardProperties>().becomeBloodied();
            onDeath(Defender);
        }
        if (Defender != null && !Defender.GetComponent<CardProperties>().dittoChecked  && !Defender.GetComponent<CardProperties>().isDead){
            if(Defender.tag == "SuperHero"){
                if (!Defender.GetComponent<SuperHeroDisplay>().inRuins){
                    checkForDitto(Defender);
                }
            }
            if (Defender.tag == "Civilian"){
                checkForDitto(Defender);
            }
        }
        UpdateChaosInfo();
    }

    [Command(requiresAuthority=false)]
    public void CmdMakeHelperPop(GameObject card){
        StartCoroutine(gmMakeHelperPop(card));
    }

    [Server]
    IEnumerator gmMakeHelperPop(GameObject card){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        RpcMakeCardBig(card);
        yield return new WaitForSeconds(0.2f);
        RpcMakeCardReg(card);
    }
    
    [Command(requiresAuthority=false)]
    public void CmdCheckForDeath(){
        StartCoroutine(gmCheckForDeath());
    }

    [Server]
    IEnumerator gmCheckForDeath(){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        yield return new WaitForSeconds(0);
        foreach(Transform card in OpponentBase.transform){
            for(int i=1; i<card.GetChild(2).childCount;i++){
                Transform card2 = card.GetChild(2).GetChild(i);
                if (card2.GetComponent<CardProperties>().isDead && !gm.deadCardsThisRound.Contains(card2.gameObject) && !card2.GetComponent<CardProperties>().hasUnflipped()){
                    CmdAddDeadCard(card2.gameObject);
                    card2.GetComponent<CardProperties>().becomeBloodied();
                    onDeath(card2.gameObject);
                }
            }
            if (card.GetComponent<CardProperties>().isDead && !gm.deadCardsThisRound.Contains(card.gameObject) && !card.GetComponent<CardProperties>().hasUnflipped()){
                CmdAddDeadCard(card.gameObject);
                card.GetComponent<CardProperties>().becomeBloodied();
                onDeath(card.gameObject);
            }   
        }
        foreach(Transform card in PlayerBase.transform){
            for(int i=1; i<card.GetChild(0).childCount;i++){
                Transform card2 = card.GetChild(0).GetChild(i);
                if (card2.GetComponent<CardProperties>().isDead && !gm.deadCardsThisRound.Contains(card2.gameObject) && !card2.GetComponent<CardProperties>().hasUnflipped()){
                    CmdAddDeadCard(card2.gameObject);
                    card2.GetComponent<CardProperties>().becomeBloodied();
                    onDeath(card2.gameObject);
                }
            }
            if (card.GetComponent<CardProperties>().isDead && !gm.deadCardsThisRound.Contains(card.gameObject) && !card.GetComponent<CardProperties>().hasUnflipped()){
                CmdAddDeadCard(card.gameObject);
                card.GetComponent<CardProperties>().becomeBloodied();
                onDeath(card.gameObject);
            }
        }
        foreach(Transform card in Street.transform){
            for(int i=2; i<card.GetChild(0).childCount;i++){
                Transform card2 = card.GetChild(0).GetChild(i);
                if (card2.GetComponent<CardProperties>().isDead && !gm.deadCardsThisRound.Contains(card2.gameObject) && !card2.GetComponent<CardProperties>().hasUnflipped()){
                    CmdAddDeadCard(card2.gameObject);
                    card2.GetComponent<CardProperties>().becomeBloodied();
                    onDeath(card2.gameObject);
                }
            }
            for(int i=2; i<card.GetChild(2).childCount;i++){
                Transform card2 = card.GetChild(2).GetChild(i);
                if (card2.GetComponent<CardProperties>().isDead && !gm.deadCardsThisRound.Contains(card2.gameObject) && !card2.GetComponent<CardProperties>().hasUnflipped()){
                    CmdAddDeadCard(card2.gameObject);
                    card2.GetComponent<CardProperties>().becomeBloodied();
                    onDeath(card2.gameObject);
                }
            }
            if (card.GetComponent<CardProperties>().isDead && !gm.deadCardsThisRound.Contains(card.gameObject) && !card.GetComponent<CardProperties>().hasUnflipped()){
                CmdAddDeadCard(card.gameObject);
                card.GetComponent<CardProperties>().becomeBloodied();
                onDeath(card.gameObject);
            }
        }
    }

    public void addVillainFromGruntCard(GameObject card){
        Villain[] allSuperVillainsArray = Resources.LoadAll<Villain>("Villains/SuperVillains/");
        for (int i=0; i<allSuperVillainsArray.Length; i++){
            if (card.GetComponent<GruntDisplay>().grunt.returnSuperVillain == allSuperVillainsArray[i].cardName){
                DeckManager.Instance.addSVToList(allSuperVillainsArray[i]);
                card.GetComponent<CardProperties>().killedByHero = true;
            }
        }
    }

    public void addVillainFromSVCard(GameObject card){
        DeckManager.Instance.addSVToList(card.GetComponent<SuperVillainDisplay>().superVillain);
        card.GetComponent<CardProperties>().killedByHero = true;
    }

    [Command(requiresAuthority=false)]
    public void CmdAddDeadCard(GameObject card){
        gmAddDeadCard(card);
    }

    [Server]
    void gmAddDeadCard(GameObject card){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (card.GetComponent<CardProperties>().isDead && !gm.deadCardsThisRound.Contains(card)){
            gm.deadCardsThisRound.Add(card);
        }
    }

    [Command(requiresAuthority=false)]
    public void CmdMakeLocation(){
        Location loc = gmGetNewLocationInfo();
        if (loc.cardTag == "Location"){
            GameObject card = Instantiate(LocationTemplate, new Vector2(0,0), Quaternion.identity);
            NetworkServer.Spawn(card);
            RpcShowCard(card, "Street", null);
            RpcSetNewLocationInfo(loc, card);
            gmSetNewLocation(card);
            RpcFlipCardDown(card);
            gmRemoveTopLocationCard();
            StartCoroutine(gmResolveCards("Location"));
        }
        if (loc.cardTag == "SuperHero"){
            GameObject card = Instantiate(SuperHeroTemplate, new Vector2(0,0), Quaternion.identity);
            NetworkServer.Spawn(card);
            RpcShowCard(card, "Street", null);
            RpcSetNewLocationInfo(loc,card);
            gmSetNewLocation(card);
            RpcFlipCardDown(card);
            gmRemoveTopLocationCard();
            StartCoroutine(gmResolveCards("Location"));
        }
        if (loc.cardTag == "Civilian"){
            GameObject card = Instantiate(CivilianTemplate, new Vector2(0,0), Quaternion.identity);
            NetworkServer.Spawn(card);
            RpcShowCard(card, "Street", null);
            RpcSetNewLocationInfo(loc,card);
            gmSetNewLocation(card);
            RpcFlipCardDown(card);
            gmRemoveTopLocationCard();
            StartCoroutine(gmResolveCards("Location"));
        }
        RpckeepRuinsStable();
    }

    [ClientRpc]
    public void RpckeepRuinsStable(){
        for (int i = 0; i<Street.transform.childCount; i++){
            GameObject Scard = Street.transform.GetChild(i).gameObject;
            int sibInd = Scard.transform.GetSiblingIndex();
            if (Scard.tag == "SuperHero"){
                if (Scard.GetComponent<SuperHeroDisplay>().inRuins){
                    if(i <= Scard.GetComponent<CardProperties>().ruinsIndex){
                        if(Scard.GetComponent<CardProperties>().ruinsIndex > Street.transform.childCount-1){
                            Scard.transform.SetSiblingIndex(Street.transform.childCount-1);
                            Scard.GetComponent<CardProperties>().ruinsIndex=Street.transform.childCount-1;
                        }
                        else {
                            if(Street.transform.childCount > 5){
                                Scard.transform.SetSiblingIndex(Scard.GetComponent<CardProperties>().ruinsIndex+1);
                            }
                            else {
                                Scard.transform.SetSiblingIndex(Scard.GetComponent<CardProperties>().ruinsIndex);
                            }
                        }
                    }
                }
            }
        }
    }

    [Server]
    Location gmGetNewLocationInfo(){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (gm.locationDeck.Count != 0){
            return gm.locationDeck[0];
        }
        return null;
    }

    [ClientRpc]
    void RpcSetNewLocationInfo(Location loc, GameObject card){
        if (loc.cardTag == "Location"){
            card.transform.GetComponent<LocationDisplay>().setLocation(loc);
        }
        if (loc.cardTag == "SuperHero"){
            card.transform.GetComponent<SuperHeroDisplay>().setLocation(loc);
        }
        if (loc.cardTag == "Civilian"){
            card.transform.GetComponent<CivilianDisplay>().setLocation(loc);
        }
    }

    [ClientRpc]
    void RpcFixRuinInfo(GameObject card, string locationName, string locationAbilityText){
        card.transform.GetChild(1).GetChild(6).GetComponent<TextMeshProUGUI>().text = "<b>" +  card.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text + "</b><br><br>" + locationAbilityText;
        card.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text = locationName;
    }

    [ClientRpc]
    void RpcSetNewVillainInfo(Villain vil, GameObject card){
        if (vil.cardTag == "SuperVillain"){
            card.transform.GetComponent<SuperVillainDisplay>().setVillain(vil);
        }
        if (vil.cardTag == "Grunt"){
            card.transform.GetComponent<GruntDisplay>().setVillain(vil);
        }
    }

    [Server]
    void gmSetNewLocation(GameObject card){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.newLocation = card;
    }

    [Server]
    void gmRemoveTopLocationCard(){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.locationDeck.RemoveAt(0);
    }

    [Server]
    void gmRemoveTopRuinsCard(){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.ruinsDeck.RemoveAt(0);
    }
   
    private void checkDestroyLastLocation(){
        if (Street.transform.childCount > 5){
            GameObject card = Street.transform.GetChild(0).gameObject;
            if (Street.transform.GetChild(0).tag == "SuperHero"){
                if (Street.transform.GetChild(0).GetComponent<SuperHeroDisplay>().inRuins){
                    card = Street.transform.GetChild(1).gameObject;
                    if (Street.transform.GetChild(1).tag == "SuperHero"){
                        if (Street.transform.GetChild(1).GetComponent<SuperHeroDisplay>().inRuins){
                            card = Street.transform.GetChild(2).gameObject;
                        }
                    }
                }
            }
            if (card.transform.GetChild(2).childCount ==2 && card.transform.GetChild(0).childCount == 2) {
                CmdDestroy5thLoc(card);
            }
        }
    }

    [Command(requiresAuthority=false)]
    public void CmdDestroy5thLoc(GameObject card){
        Object.Destroy(card);
    }
    
    [Command(requiresAuthority=false)]
    public void CmdDestroyCard(GameObject card){
        gmDestroyCard(card);
    }
    
    [Server]
    IEnumerator gmDestroyCard(GameObject card){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (card.tag == "Grunt" || card.tag == "SuperVillain"){
            for (int i = card.transform.GetChild(0).childCount -1; i > 1; i--){
                if (i>1){
                    callBackToBase(card.transform.GetChild(0).GetChild(i).gameObject, "Player Base", null);
                    yield return new WaitForSeconds(0.1f);
                }
            }
            for (int i = card.transform.GetChild(2).childCount -1; i > 1; i--){
                if (i > 1){
                    callBackToBase(card.transform.GetChild(2).GetChild(i).gameObject, "Player Base", null);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
        else {
            for (int i = card.transform.GetChild(0).childCount -1; i > 1; i--){
                if (i>1){
                    callBackToBase(card.transform.GetChild(0).GetChild(i).gameObject, "Player Base", null);
                    //yield return new WaitForSeconds(0.1f);
                }
            }
            for (int i = card.transform.GetChild(2).childCount -1; i > 1; i--){
                if (i > 1){
                    callBackToBase(card.transform.GetChild(2).GetChild(i).gameObject, "Player Base", null);
                    //yield return new WaitForSeconds(0.1f);
                }
            }
        }
        if (card.tag == "SuperHero"){
            if (!card.GetComponent<SuperHeroDisplay>().inRuins){
                yield return new WaitForSeconds(0.1f);
                Object.Destroy(card);
            }
        }
        else {
            yield return new WaitForSeconds(0.1f);
            Object.Destroy(card);
        }
    }

    public void incrementLocationsRuined(){
        if (isServer){
            CmdIncrementLocationsRuined();
        }
    }

    [Command(requiresAuthority=false)]
    public void CmdIncrementLocationsRuined(){
        RpcDebug("inc");
        gmIncrementLocationsRuined();
    }

    [ClientRpc]
    void RpcDebug(string a){
        Debug.Log(a);
    }

    [Server]
    public void gmIncrementLocationsRuined(){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.locationsRuined++;
    }

    [Server]
    IEnumerator gmReplaceRuins(int siblingIndex, string locationName, string locationAbilityText){
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        Location loc = gm.ruinsDeck[0];
        GameObject card = Instantiate(SuperHeroTemplate, new Vector2(0,0), Quaternion.identity);
        NetworkServer.Spawn(card);
        RpcShowRuinsCard(card, siblingIndex);
        RpcSetNewLocationInfo(loc, card);
        RpcFixRuinInfo(card, locationName, locationAbilityText);
        gmSetNewLocation(card);
        RpcFlipCardDown(card);
        gmRemoveTopRuinsCard();
        StartCoroutine(gmResolveCards("Location"));
        yield return new WaitForSeconds(0.3f);
    }

    public void chaosSpoils(int chaosAdded, GameObject card){
        if(card.GetComponent<NetworkIdentity>().isOwned && chaosAdded > 0){
            ChaosBox.GetComponent<Chaos>().roundChaos += chaosAdded;
            ChaosBox.GetComponent<Chaos>().totalChaos += chaosAdded;
        }
        UpdateChaosInfo();
    }

    public void recruitVillain(string name, bool mine){
        targetRecruitVillain(getMyId().connectionToClient, name, mine);
    }

    [TargetRpc]
    public void targetRecruitVillain(NetworkConnection oppConec, string name, bool mine){
        CmdRecruitVillain(name, mine);
    }

    [Command(requiresAuthority=false)]
    void CmdRecruitVillain(string name, bool mine){
        NetworkIdentity oppId = getOpponentId();
        targetRecruitVillain2(oppId.connectionToClient, GetComponent<NetworkIdentity>(), name, mine);
    }

    [TargetRpc]
    void targetRecruitVillain2(NetworkConnection oppConnection, NetworkIdentity myId,  string name, bool mine){
        int oppCardsInHand = countMyCards();
        CmdRecruitVillain2(myId, oppCardsInHand, name, mine);
    }

    [Command(requiresAuthority=false)]
    void CmdRecruitVillain2(NetworkIdentity myId, int oppCardsInHand, string name, bool mine){
        TargetRecruitVillain3(myId.connectionToClient, oppCardsInHand, name, mine);
    }

    [TargetRpc]
    void TargetRecruitVillain3(NetworkConnection myConnection, int cardsInHand, string name, bool mine){
        int quantity = 0;
        Villain vil = null;
        string location = null;
        switch (name){
            case "Beedrill":
                vil = Resources.Load<Villain>("Spawnables/S Stinger");
                quantity = 2;
                location = "Player Base";
                break;
            case "Persian":
                for (int i=0; i<DeckManager.Instance.myDeck.Count;i++){
                    if (DeckManager.Instance.myDeck[i].cardTag == "Grunt"){
                        vil = DeckManager.Instance.myDeck[i];
                        DeckManager.Instance.removeDeckTop();
                        break;
                    }
                }
                if(!isServer){
                    if(mine){
                        quantity = 1;
                    }
                }
                else {
                    if (!mine){
                        quantity = 1;
                    }
                }
                quantity = 1;
                location = "Player Base";
                break;
            case "Starmie":
                vil = Resources.Load<Villain>("Spawnables/S Staryu");
                quantity = 1;
                location = "Hand";
                break;
            case "Nidorina":
                vil = Resources.Load<Villain>("Spawnables/S Stinger");
                quantity = 1;
                location = "Player Base";
                break;
            case "Nidorino":
                vil = Resources.Load<Villain>("Spawnables/S Stinger");
                quantity = 1;
                location = "Player Base";
                break;
            case "Victreebel":
                vil = Resources.Load<Villain>("Spawnables/S Bellsprout");
                quantity = 1;
                location = "Player Base";
                break;
            default:
                break;
        }
        if(vil != null && location != null && quantity != 0){
            if(isServer){
                if(!mine){
                    for(int i=0; i<7-cardsInHand && i<quantity; i++){
                        CmdSpawnVillain(vil, location, !mine);
                    }
                }
                else {
                    for(int i=0; i<7-countMyCards() && i<quantity; i++){
                        CmdSpawnVillain(vil, location, !mine);
                    }
                }
            }
            else {
                if(!mine){
                    for(int i=0; i<7-countMyCards() && i<quantity; i++){
                        CmdSpawnVillain(vil, location, mine);
                    }
                }
                else {
                    for(int i=0; i<7-cardsInHand && i<quantity; i++){
                        CmdSpawnVillain(vil, location, mine);
                    }
                }
            }
        }
    }

    [Command(requiresAuthority=false)]
    public void CmdSpawnVillain(Villain vil, string location, bool mine){
        GameObject template = null;
        NetworkConnection connec = getOpponentId().connectionToClient;
        switch (vil.cardTag){
            case "SuperVillain":
                template = SuperVillainTemplate;
                break;
            case "Grunt":
                template = GruntTemplate;
                break;
            default:
                template = SuperVillainTemplate;
                break;
        }
        GameObject card = Instantiate(template, new Vector2(0,0), Quaternion.identity);
        if(mine){
            NetworkServer.Spawn(card, connec);
        }
        else {
            connec = getMyId().connectionToClient;
            NetworkServer.Spawn(card, connec);
        }
        if(location == "Hand"){
            TargetShowCardInHand(connec, card);
            RpcSetRoundStartHand(card);
            RpcSetNewVillainInfo(vil, card);
        }
        else {
            RpcShowCard(card, location, null);
            RpcSetRoundStartBase(card);
            RpcSetNewVillainInfo(vil, card);
            RpcSetAsRevealed(card);
            StartCoroutine(gmFaceDownFaceUp(card));
        }
    }

    [ClientRpc]
    public void RpcSetAsRevealed(GameObject card){
        card.GetComponent<CardProperties>().onRevealAbility = true;
    } 

    public void gruntToVillain(GameObject card, Villain vil){
        if(isServer){
            CmdGruntToVillain(card, vil);
        }
    }

    [Command(requiresAuthority=false)]
    public void CmdGruntToVillain(GameObject card, Villain vil){
        StartCoroutine(gmGruntToVillain(card, vil));
    }

    [Server]
    IEnumerator gmGruntToVillain(GameObject card, Villain vil){
        string area = "Hand";
        GameObject location = null;
        int sibIndex = card.transform.GetSiblingIndex();
        if(card.transform.parent.gameObject == PlayerBase){
            area = "Player Base";
        }
        else if(card.transform.parent.gameObject == OpponentBase){
            area = "Opponent Base Cards";
        }
        else if(card.transform.parent.parent.parent.gameObject == Street){
            area = "Locations";
            location = card.transform.parent.parent.gameObject;
        }
        else if (card.transform.parent.parent.parent.gameObject == PlayerBase){
            area = "Player Base Siege";
            location = card.transform.parent.parent.gameObject;
        }
        else if (card.transform.parent.parent.parent.gameObject == OpponentBase){
            area = "Opponent Base";
            location = card.transform.parent.parent.gameObject;
        }
        NetworkIdentity myId = card.GetComponent<NetworkIdentity>();
        yield return new WaitForSeconds(0.2f);
        if(area == "Player Base" && card.transform.GetChild(0).childCount > 1){
            callBackToBase(card.transform.GetChild(0).GetChild(1).gameObject, "Player Base", null);
        }
        if(GameManager.Instance.deadCardsThisRound.Contains(card)){
            GameManager.Instance.deadCardsThisRound.Remove(card);
        }
        yield return new WaitForSeconds(0.1f);
        Object.Destroy(card);
        yield return new WaitForSeconds(0.1f);
        GameObject newCard = Instantiate(SuperVillainTemplate, new Vector2(0,0), Quaternion.identity);
        NetworkServer.Spawn(newCard, myId.connectionToClient); 
        RpcSetVillainInfo(newCard, vil);
        if (area == "Hand") {
            TargetShowCardInHand(myId.connectionToClient, newCard);
            RpcSetRoundStartHand(newCard);
        }
        else {
            RpcShowCard(newCard, area, location);
            RpcSetRoundStartBase(newCard);
        }
        newCard.transform.SetSiblingIndex(sibIndex);
        if(area == "Locations"){
            TargetFreezeCard(myId.connectionToClient, newCard);   
        }
    }

    public void transformVillain(GameObject card, Villain vil){
        if (isServer){
            CmdTransformVillain(card, vil);
        }
    }

    [Command(requiresAuthority=false)]
    public void CmdTransformVillain(GameObject card, Villain vil){
        StartCoroutine(gmTransformVillain(card, vil));
    }

    [Server]
    IEnumerator gmTransformVillain(GameObject card, Villain vil){
        yield return new WaitForSeconds(0.4f);
        RpcSetNewVillainInfo(vil, card);
    }

    public void checkForDitto(GameObject card){
        CmdDittoChecked(card);
        int chance = rngPercent;
        generateRngPercent();
        if (0<chance && chance <= 2){
            Location ditto = Resources.Load<Location>("Spawnables/S Ditto");
            transformLocation(card, ditto);
        }
    }

    [Command(requiresAuthority=false)]
    public void CmdDittoChecked(GameObject card){
        RpcDittoChecked(card);
    }

    [ClientRpc]
    public void RpcDittoChecked(GameObject card){
        card.GetComponent<CardProperties>().dittoChecked = true;
    }

    public void transformLocation(GameObject card, Location loc){
        if (isServer){
            CmdTransformLocation(card, loc);
        }
    }

    [Command(requiresAuthority=false)]
    public void CmdTransformLocation(GameObject card, Location loc){
        StartCoroutine(gmTransformLocation(card, loc));
    }

    [Server]
    IEnumerator gmTransformLocation(GameObject card, Location loc){
        yield return new WaitForSeconds(0.4f);
        RpcSetNewLocationInfo(loc, card);
    }

    [Server]
    IEnumerator gmFaceDownFaceUp(GameObject card){
        RpcFlipCardDown(card);
        yield return new WaitForSeconds(0.2f);
        RpcFlipCardUp(card);
    }

    [Command(requiresAuthority=false)]
    void CmdResetTurnButtons(){
        RpcResetTurnButtons();
    }

    [ClientRpc]
    void RpcResetTurnButtons(){
        noMoveButtons();
        removeDmgMarkers();
        confirmPressed = false;
    }

    [ClientRpc]
    void RpcResetTurnChaos(){
        ChaosBox.GetComponent<Chaos>().roundChaos = 0;
        UpdateChaosInfo();
    }

    [ClientRpc]
    void RpcSetRoundStartBase(GameObject card){
        card.GetComponent<CardProperties>().roundStartArea = PlayerBase;
    }
    
    [ClientRpc]
    void RpcSetRoundStartHand(GameObject card){
        card.GetComponent<CardProperties>().roundStartArea = Hand;
    }

    [Command(requiresAuthority=false)]
    public void CmdCheckForPriority(GameObject card){
        bool hasPriority = false;
        if (priorityPlayer != null){
            if(card.GetComponent<NetworkIdentity>().connectionToClient == priorityPlayer.GetComponent<NetworkIdentity>().connectionToClient){
                hasPriority = true;
            }
        }
        RpcCheckForPriority(hasPriority);
    }

    [ClientRpc]
    public void RpcCheckForPriority(bool hasPriority){
        currentCardHasPriority = hasPriority;
    }

    public int onAttack(GameObject card){
        int a = GetComponent<CardAbilities>().onAttack(card, currentCardHasPriority);
        CmdCheckForDeath();
        return a;
    }

    public void onAttackEnd(GameObject card){
        GetComponent<CardAbilities>().onAttackEnd(card, currentCardHasPriority);
        CmdCheckForDeath();
    }

    public int onDefend(GameObject card, GameObject opp){
        int a = GetComponent<CardAbilities>().onDefend(card, opp, currentCardHasPriority);
        CmdCheckForDeath();
        return a;
    }

    public void onReveal(GameObject card){
        GetComponent<CardAbilities>().onReveal(card, currentCardHasPriority);
        CmdCheckForDeath();
    }

    public void onDeath(GameObject card){
        GetComponent<CardAbilities>().onDeath(card, currentCardHasPriority);
        CmdCheckForDeath();
    }

    [Command(requiresAuthority=false)]
    public void CmdFlipCardUp(GameObject card){
        RpcFlipCardUp(card);
    }

    [ClientRpc]
    void RpcFlipCardUp(GameObject card){
        card.GetComponent<CardProperties>().cardFlipFront();
        onReveal(card);
    }
    
    [Command(requiresAuthority=false)]
    void CmdFlipCardDown(GameObject card){
        RpcFlipCardDown(card);
    }

    [ClientRpc]
    void RpcFlipCardDown(GameObject card){
        card.GetComponent<CardProperties>().cardFlipBack();
    }

    [TargetRpc]
    void TargetFreezeCard(NetworkConnection myConnection, GameObject card){
        freezeCard(card);
    }

    [ClientRpc]
    void RpcFreezeCard(GameObject card){
        freezeCard(card);
    }

    public void freezeCard(GameObject card){
        card.transform.GetComponent<DragDrop>().enabled = false; 
    }

    public void unfreezeCard(GameObject card){
        card.transform.GetComponent<DragDrop>().enabled = true;
    }

    public void removeImmune(GameObject card){
        card.GetComponent<CardProperties>().isImmune = false;
    }

    public void unstunCard(GameObject card){
        card.transform.GetComponent<CardProperties>().isStunned = false;
    }

    [Command(requiresAuthority=false)]
    void CmdUnfreezeAllCards(){
        RpcUnfreezeAllCards();
    }

    [Command(requiresAuthority=false)]
    void CmdRemoveImmuneAllCards(){
        RpcRemoveImmuneAllCards();
    }

    [Command(requiresAuthority=false)]
    void CmdResetGruntsTo1(){
        RpcResetGruntsTo1();
    }

    [Command(requiresAuthority=false)]
    void CmdUnstunAllCards(){
        RpcUnstunAllCards();
    }

    [ClientRpc]
    void RpcUnfreezeAllCards(){
        unfreezeAllCards();
    }

    [ClientRpc]
    void RpcRemoveImmuneAllCards(){
        removeImmuneAllCards();
    }

    [ClientRpc]
    void RpcResetGruntsTo1(){
        resetGruntsTo1();
    }

    [ClientRpc]
    void RpcUnstunAllCards(){
        unstunAllCards();
    }

    public void freezeAllCards(){
        foreach (Transform child in Hand.transform){
            freezeCard(child.gameObject);
        }
        foreach (Transform child in PlayerBase.transform){
            freezeCard(child.gameObject);
        }
    }

    public void unfreezeAllCards(){
        foreach (Transform child in Hand.transform){
            unfreezeCard(child.gameObject);
        }
        foreach (Transform child in PlayerBase.transform){
            unfreezeCard(child.gameObject);
        }
    }

    public void removeImmuneAllCards(){
        foreach (Transform child in Hand.transform){
            removeImmune(child.gameObject);
        }
        foreach (Transform child in OpponentBase.transform){
            removeImmune(child.gameObject);
        }
        foreach (Transform child in PlayerBase.transform){
            removeImmune(child.gameObject);
        }
    }

    public void resetGruntsTo1(){
        foreach (Transform child in Hand.transform){
            if(child.tag == "Grunt"){
                child.GetComponent<GruntDisplay>().healthRemaining = 1;
            }
        }
        foreach (Transform child in OpponentBase.transform){
            if(child.tag == "Grunt"){
                child.GetComponent<GruntDisplay>().healthRemaining = 1;
            }
        }
        foreach (Transform child in PlayerBase.transform){
            if(child.tag == "Grunt"){
                child.GetComponent<GruntDisplay>().healthRemaining = 1;
            }
        }
    }

    public void unstunAllCards(){
        foreach (Transform child in Hand.transform){
            unstunCard(child.gameObject);
        }
        foreach (Transform child in OpponentBase.transform){
            unstunCard(child.gameObject);
        }
        foreach (Transform child in PlayerBase.transform){
            unstunCard(child.gameObject);
        }
        foreach (Transform child in Street.transform){
            unstunCard(child.gameObject);
        }
    }

    public void removeDmgMarkers(){
        CmdRemoveDmgMarkers();
    }

    [Command(requiresAuthority=false)]
    public void CmdRemoveDmgMarkers(){
        RpcRemoveDmgMarkers();
    }

    [ClientRpc]
    public void RpcRemoveDmgMarkers(){
        foreach(Transform card in OpponentBase.transform){
            for(int i=1; i<card.GetChild(2).childCount;i++){
                card.GetChild(2).GetChild(i).GetComponent<CardProperties>().removeDmgMarker();
            }
            card.GetComponent<CardProperties>().removeDmgMarker(); 
        }
        foreach(Transform card in PlayerBase.transform){
            for(int i=1; i<card.GetChild(0).childCount;i++){
                card.GetChild(0).GetChild(i).GetComponent<CardProperties>().removeDmgMarker();
            }
            card.GetComponent<CardProperties>().removeDmgMarker(); 
        }
        foreach(Transform card in Street.transform){
            for(int i=2; i<card.GetChild(0).childCount;i++){
                card.GetChild(0).GetChild(i).GetComponent<CardProperties>().removeDmgMarker();
            }
            for(int i=2; i<card.GetChild(2).childCount;i++){
                card.GetChild(2).GetChild(i).GetComponent<CardProperties>().removeDmgMarker();
            }
            card.GetComponent<CardProperties>().removeDmgMarker(); 
        }
    }

    public NetworkIdentity getOpponentId(){
        NetworkIdentity opponentId = null;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players){
            if (p.GetComponent<NetworkIdentity>() != GetComponent<NetworkIdentity>()){
                opponentId = p.GetComponent<NetworkIdentity>();
            }
        }
        return opponentId;
    }

    public void generateRngPercent(){
        if(isServer){
            int randomPercentage = rng.Next(1,100);
            CmdSetRandomPercent(randomPercentage);
        }
    }
    
    [Command(requiresAuthority=false)]
    public void CmdSetRandomPercent(int randomPercentage){
        RpcSetRandomPercent(randomPercentage);
    }

    [ClientRpc]
    public void RpcSetRandomPercent(int randomPercentage){
        rngPercent = randomPercentage;
    }

    public void generateRandomListForStreetChildren(){
        if(isServer){
            List<int> percentInts = new List<int>();
            for (int i = 0; i<6; i++){
                percentInts.Add(rng.Next(1,100));
            }
            CmdGenerateRandomListForStreetChildren(percentInts);
        }
    }

    [Command(requiresAuthority=false)]
    public void CmdGenerateRandomListForStreetChildren(List<int> percentInts){
        RpcGenerateRandomListForStreetChildren(percentInts);
    }

    [ClientRpc]
    public void RpcGenerateRandomListForStreetChildren(List<int> percentInts){
        percentRolls = percentInts;
    }

    private int getCardsInHand(){
        return Hand.transform.childCount;
    }

    private string getChaosText(){
        return ChaosBox.GetComponent<Chaos>().updateChaosText();
    }

    [Command(requiresAuthority=false)]
    public void CmdEndGame (){
        RpcEndGame();
    }

    [Command(requiresAuthority=false)]
    public void CmdEarlyEndGame(){
        RpcEarlyEndGame();
    }

    [ClientRpc]
    public void RpcEndGame(){
        gameEnded = true;
        endGameScreen();
        allButtonsOff();
        freezeAllCards();
        Debug.Log("Game Over");
    }

    [ClientRpc]
    public void RpcEarlyEndGame(){
        gameEnded = true;
        earlyEndGameScreen();
        allButtonsOff();
        freezeAllCards();
        Debug.Log("Game Over");
    }

    public void endGameScreen(){
        PassButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Chaos!";
        if (ChaosBox.GetComponent<Chaos>().totalChaos == oppTotalChaos){
            if(ChaosBox.GetComponent<Chaos>().roundChaos > oppRoundChaos){
                ConfirmButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "You Win!";
                UndoMoveButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "They Lose";

            }
            else if(ChaosBox.GetComponent<Chaos>().roundChaos < oppRoundChaos){
                ConfirmButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "You Lose!";
                UndoMoveButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "They Win";
            }
            else {
                ConfirmButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Whoopsie!";
                UndoMoveButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Tie game!";
            }
        }
        else if (ChaosBox.GetComponent<Chaos>().totalChaos > oppTotalChaos){
            ConfirmButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "You Win!";
            UndoMoveButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "They Lose";
        }
        else {
            ConfirmButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "You Lose!";
            UndoMoveButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "They Win";
        }
    }

    public void earlyEndGameScreen(){
        PassButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Oh no!";
        ConfirmButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "The Town";
        UndoMoveButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Is Safe!";
    }

    public int chaosTextToInt(string chaosText){
        string a = string.Empty;
        char breakpoint = '/';
        int val = 0;

        for (int i=0; i< chaosText.Length; i++)
        {
            if (System.Char.IsDigit(chaosText[i])){
                a += chaosText[i];
            }
            if (chaosText[i] == breakpoint){
                break;
            }
        }
        if (a.Length>0){
            val = int.Parse(a);
        }
        return val;
    }

    public void shuffleDeck(List<Villain> deck){
        int n = deck.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            Villain value = deck[k];  
            deck[k] = deck[n];  
            deck[n] = value;  
        }  
    }

    void Update()
    {
        generateRngPercent();
        generateRandomListForStreetChildren();
        if (!confirmPressed && ConfirmButton.GetComponent<Button>().interactable && Input.GetKeyDown("return") || !confirmPressed && ConfirmButton.GetComponent<Button>().interactable && Input.GetKeyDown("c")){
            confirmPressed = true;
            ConfirmButton.GetComponent<Confirm>().OnClick();
        }
        if (UndoMoveButton.GetComponent<Button>().interactable && Input.GetKeyDown("backspace") || UndoMoveButton.GetComponent<Button>().interactable && Input.GetKeyDown("u")){
            UndoMoveButton.GetComponent<UndoMove>().OnClick();
        }
        if (PassButton.GetComponent<Button>().interactable && Input.GetKeyDown("p")){
            PassButton.GetComponent<Pass>().OnClick();
        }
    }
}
