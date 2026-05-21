using Unity.Netcode;
using UnityEngine;

public class PlayerDominoField : NetworkBehaviour
{
    public NetworkList<Vector3> dominoPositions;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

}
