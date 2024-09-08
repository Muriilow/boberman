using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastPickable : PowerUpPickable
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.GetComponent<PlayerBomb>().AddRadius(1);

            DesappearServerRpc();
        }
    }
}
