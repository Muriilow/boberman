using Unity.Netcode;

public class Wall : NetworkBehaviour
{
    private Grid<BackgroundTile> _walls;
    public int XIndex { get; set; }
    public int YIndex { get; set; }

    [Rpc(SendTo.Server)]
    public void DestroyAnimationServerRpc()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }
}
