using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatformCheck : MonoBehaviour
{
   public PlayerController pc;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "InGroundPlayer")
        {

            pc.isPlafromCheck = false;

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "InGroundPlayer")
        {

            pc.AbleCollision();
            pc.isPlafromCheck = true;

        }
    }


}
