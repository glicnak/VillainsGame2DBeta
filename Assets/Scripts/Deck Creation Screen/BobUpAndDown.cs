using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobUpAndDown : MonoBehaviour
{   
    public bool up = false;
    public bool initialJump = false;
    public bool startNoMove = false;

    void Update() {

        if(Time.frameCount%18 == 0){
            if(!initialJump){
                initialJump = true;
                up = true;
                transform.position = new Vector3(transform.position.x, transform.position.y + 8, transform.position.z);
            }
            else {
                if (up){
                    up = false;
                    transform.position = new Vector3(transform.position.x, transform.position.y - 13, transform.position.z);
                }
                else {
                    up = true;
                    transform.position = new Vector3(transform.position.x, transform.position.y + 13, transform.position.z);
                }
            }
        }
    }
}
