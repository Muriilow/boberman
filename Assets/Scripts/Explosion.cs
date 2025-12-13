using Unity.Netcode;
using UnityEngine;

public class Explosion : NetworkBehaviour
{
    [SerializeField] private Animator _anim;

    // Start is called before the first frame update
    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetDirectionClientRpc(Vector2 direction)
    {
        var angle = Mathf.Atan2 (direction.y, direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }
    [Rpc(SendTo.Server)]
    public void DestroyAnimationServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
            collision.GetComponent<PlayerManager>().Damage(1);
    }
}
