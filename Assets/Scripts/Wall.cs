using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Wall : NetworkBehaviour, IDamageable
{
    private Grid<BackgroundTile> walls;
    public int XIndex { get; set; }
    public int YIndex { get; set; }

    public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }

    public void Damage(float damageAmount)
    {
        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
