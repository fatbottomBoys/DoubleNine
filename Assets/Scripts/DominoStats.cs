using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections;
using JetBrains.Annotations;

public class DominoStats : NetworkBehaviour
{
    public bool isActive;
    public Vector3 myValue;
    public string myName;
    public string myTag;
    public int myID;
    public bool receivedResponse;
    public Texture2D[] domTextures;

    public bool isDouble;
    public bool isPlayable;
    //public NetworkVariable<Vector3> myVirtualParentPos;

    public void Awake()
    {
        isPlayable = true;
        //Debug.Log(this.gameObject.name);
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
            while (!receivedResponse)
            {
                yield return new WaitForFixedUpdate();
                //Debug.Log($"Domino{myID} waiting for response from server");
            }
        }
        
        
        this.gameObject.name = myName;
        this.gameObject.tag = myTag;

        Material[] myMat = this.transform.GetChild(0).transform.GetComponent<MeshRenderer>().materials;
        if (myID == -10)
        {
            myMat[1].mainTexture = domTextures[Mathf.RoundToInt(myValue.x)];
            myMat[2].mainTexture = domTextures[Mathf.RoundToInt(myValue.y)];
        }
    }

    [Rpc(SendTo.Server)]
    public void WhatsMyNameRpc()
    {
        ThatsYourNameRpc(myName, myTag, myValue, myID);
    }

    [Rpc(SendTo.NotServer)]
    public void ThatsYourNameRpc(string name, string tag, Vector3 value, int id)
    {
        myName = name;
        myTag = tag;
        receivedResponse = true;
        myID = id;
        myValue = value;
    }


}
