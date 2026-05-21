using UnityEngine;
using Unity.Netcode;

public class DominoStats : MonoBehaviour
{
    public bool isActive;
    public Vector3 myValue;
    public int myID;
    public bool isDouble;
    public bool isPlayable;

    public void Start()
    {
        isPlayable = true;
    }


}
