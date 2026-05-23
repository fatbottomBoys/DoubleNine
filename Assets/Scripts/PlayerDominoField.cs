using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using System.Collections;
using UnityEngine;

public class PlayerDominoField : NetworkBehaviour
{
    public Vector3[] dominoPositions;
    public string myName;
    public bool receivedResponse;
    //public override void OnNetworkSpawn()
    //{
    //    base.OnNetworkSpawn();
    //}

    public void Awake()
    {
        StartCoroutine(LoadIn());
    }


    public IEnumerator LoadIn()
    {
        yield return new WaitForFixedUpdate(); 
        yield return new WaitForFixedUpdate();

        if (!IsHost)
        {
            receivedResponse = false;
            WhatsMyNameRpc();
            AskForDomPosRpc();
            while (!receivedResponse)
            {
                yield return new WaitForFixedUpdate();
                
            }

            this.gameObject.name = myName;
        }

        

    }


    [Rpc(SendTo.Server)]                    // #1: Send a request for all the domino positions, so the server will execute a function...
    public void AskForDomPosRpc()
    {
        HostToClientDominoFill();
    }

    public void HostToClientDominoFill()    // ...to go through all of its dominos and send them 1 by 1 as RPC's
    {
        foreach (Vector3 pos in dominoPositions)
        {
            SendDomPosRpc(pos);
        }
    }

    [Rpc(SendTo.NotServer)]                 // #2: Then, the client will receive that position and execute a function....
    public void SendDomPosRpc(Vector3 pos)
    {
        AddToDomPos(pos);
    }

    public void AddToDomPos(Vector3 pos)    // ... to add those values to its local positions list.
    {
        List<Vector3> tempDoms = dominoPositions.ToList();
        tempDoms.Add(pos);
        dominoPositions = tempDoms.ToArray();
    }

    [Rpc(SendTo.Server)]
    public void WhatsMyNameRpc()
    {
        ThatsYourNameRpc(myName);
    }

    [Rpc(SendTo.NotServer)]
    public void ThatsYourNameRpc(string name)
    {
        myName = name;
        receivedResponse = true;
    }







}
