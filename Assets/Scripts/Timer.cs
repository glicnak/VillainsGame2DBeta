using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Timer : NetworkBehaviour
{
    public PlayerManager playerManager;
    public Image greenTimeBar;
    public Image redTimeBar;
    public float timeToAct = 8;
    public float timeRemaining = 0;
    public bool timerIsRunning = false;
    public string timeoutMsg = null;
    private float totalTime;
    private Image currentTimeBar;

    // Start is called before the first frame update
    void Start()
    {
        greenTimeBar.GetComponent<Image>().enabled = false;
        redTimeBar.GetComponent<Image>().enabled = false;
    }

    public void startGreenTimer(){
        currentTimeBar = greenTimeBar;
        timeoutMsg = "Your opponent's time is up!";
        runTimer(timeToAct);
    }

    public void startRedTimer(){
        currentTimeBar = redTimeBar;
        timeoutMsg = "Your time is up!";
        runTimer(timeToAct);
    }

    public void stopTimer(){
        timeRemaining = 0;
        timerIsRunning = false;
        currentTimeBar.GetComponent<Image>().enabled = false;
        timeoutMsg = null;
    }

    private void runTimer(float time){
        currentTimeBar.GetComponent<Image>().enabled = true;
        timeRemaining = time;
        totalTime = time;
        timerIsRunning = true;
        currentTimeBar.fillAmount = 1f;
    }

    private void autoConfirm(){
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.forceUndoMove();
        playerManager.confirmButtonClicks("pass");
    }

    void Update(){
        if (timerIsRunning){
            if (timeRemaining > 6){
                timeRemaining -= 1.3f*Time.deltaTime;
                currentTimeBar.fillAmount = timeRemaining / totalTime;
            }
            else if (timeRemaining > 5 && timeRemaining <= 6){
                timeRemaining -= 1.2f*Time.deltaTime;
                currentTimeBar.fillAmount = timeRemaining / totalTime;
            }
            else if (timeRemaining > 4 && timeRemaining <= 5){
                timeRemaining -= 1.1f*Time.deltaTime;
                currentTimeBar.fillAmount = timeRemaining / totalTime;
            }
            else if (timeRemaining > 3 && timeRemaining <= 4){
                timeRemaining -= 1f*Time.deltaTime;
                currentTimeBar.fillAmount = timeRemaining / totalTime;
            }
            else if (timeRemaining > 2 && timeRemaining <= 3){
                timeRemaining -= 0.9f*Time.deltaTime;
                currentTimeBar.fillAmount = timeRemaining / totalTime;
            }
            else if (timeRemaining > 1.5 && timeRemaining <= 2){
                timeRemaining -= 0.8f*Time.deltaTime;
                currentTimeBar.fillAmount = timeRemaining / totalTime;
            }
            else if (timeRemaining > 1 && timeRemaining <= 1.5){
                timeRemaining -= 0.7f*Time.deltaTime;
                currentTimeBar.fillAmount = timeRemaining / totalTime;
            }
            else if (timeRemaining > 0.5 && timeRemaining <= 1){
                timeRemaining -= 0.6f*Time.deltaTime;
                currentTimeBar.fillAmount = timeRemaining / totalTime;
            }
            else if (timeRemaining > 0 && timeRemaining <= 0.5){
                timeRemaining -= 0.5f*Time.deltaTime;
                currentTimeBar.fillAmount = timeRemaining / totalTime;
            }
            else {
                if (currentTimeBar == redTimeBar){
                    autoConfirm();
                }
                Debug.Log(timeoutMsg);
                stopTimer();
            }
        }
    }

}
