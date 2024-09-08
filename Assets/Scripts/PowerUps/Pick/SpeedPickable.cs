using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPickable : PowerUpPickable
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.GetComponent<PlayerMovement>().AddMoveSpeed(1);

            DesappearServerRpc();
        }
    }
}
