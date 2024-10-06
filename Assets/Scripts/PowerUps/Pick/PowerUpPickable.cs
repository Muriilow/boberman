using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Utilities;
public abstract class PowerUpPickable : NetworkBehaviour, IPickable
{
    public float timerLife = 10;
    public CountdownTimer Timer { get; set; }

    [Rpc(SendTo.Server)]
    public void DesappearServerRpc()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }
    public override void OnNetworkSpawn()
    {
        Timer = new CountdownTimer(timerLife);
        Timer.Start();
        Timer.OnTimerStop = () =>
        {
            DesappearServerRpc();
        };

    }
    private void Update()
    {
        Timer.Tick(Time.deltaTime);
    }
}
