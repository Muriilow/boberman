using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombPickable : PowerUpPickable
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.GetComponent<PlayerBomb>().AddBomb(1);

            DesappearServerRpc();
        }
    }
}
