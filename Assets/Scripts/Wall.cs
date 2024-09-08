using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Wall : NetworkBehaviour
{
    private Grid<BackgroundTile> walls;
    public int XIndex { get; set; }
    public int YIndex { get; set; }

    [Rpc(SendTo.Server)]
    public void DestroyAnimationServerRpc()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }
}
