using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    [Header("Players")]
    public GameObject[] players;

    [Header("Who's turn?")]
    public int firstPlayer;
    public int curPlayerTurn;

    [Header("Dominoes on-field")]
    public GameObject dominoField;
    public int[] playableEnds;

    [Header("Dominoes")]
    [Tooltip("x and y = values of the domino, z = whether its been played or not")]
    public List<Vector3> allDominoes;
    public List<Vector3> shuffledDominoes;
    public List<Vector3> playerDominoes;
    public GameObject playerDominoFieldBase;
    public GameObject[] playerDominoFields;
    public GameObject[][] serverPlayerDominoes;
    public Texture2D[] dominoTextures;

    [Header("Game Start stuff")]
    [SerializeField]private MultiplayerUI m_multiplayerUI;

    [Header("Test Stuff")]
    public GameObject serverDomino;
    public Vector3 dominoPos;

    public void Awake()
    {
        playerDominoes = new List<Vector3>();


        




        //initialize debug multiplayer connectivity
        if (m_multiplayerUI != null)
        {
            m_multiplayerUI.OnStartHost += StartHost;
            m_multiplayerUI.OnStartClient += StartClient;
            m_multiplayerUI.OnDisconnectClient += DisconnectClient;
                
        }
        
    }

    private void DisconnectClient()
    {
        m_multiplayerUI.DisableButtons();
        NetworkManager.Shutdown();
    }

    private void StartClient()
    {
        m_multiplayerUI.DisableButtons();
        NetworkManager.StartClient();

    }

    private void StartHost()
    {
        m_multiplayerUI.DisableButtons();
        NetworkManager.StartHost();

        playerDominoes = new List<Vector3>();
        PopulateDominoes();
        DistributeDominoes();
        StartCoroutine(InstanceDominoes());
    }

    private void PopulateDominoes()
    {
        List<Vector3> unshuffledDoms = new List<Vector3>();
        for (int i = 0; i < 10; i++)                                                                                    // generate all the base dominoes
        {                                                                                                               // because I'm too lazy to write 
            for (int j = 0; j < 10; j++)                                                                                // them all out myself.
            {                                                                                                           //
                if (!unshuffledDoms.Contains(new Vector3(i, j, 0)) && !unshuffledDoms.Contains(new Vector3(j, i, 0)))   //
                {                                                                                                       //
                    unshuffledDoms.Add(new Vector3(i, j, 0));                                                           //
                }                                                                                                       //
            }                                                                                                           //
        }                                                                                                               //
                                                                                                                        //
        allDominoes = unshuffledDoms;                                                                         //
    }
    public void DistributeDominoes()
    {
        shuffledDominoes = Shuffle(allDominoes); //first, shuffle the dominoes

        bool verified = false;                                                                  // Next, check if its gonna
        while (!verified)                                                                       // populate anyone with 5 
        {                                                                                       // doubles or more.
            bool fiveDoubles = false;                                                           //
                                                                                                //
            for (int i = 0; i < 4; i++)                                                         //
            {                                                                                   //
                int doubleCounter = 0;                                                          //
                for (int j = 0; j < 10; j++)                                                    //
                {                                                                               //
                    if (shuffledDominoes[(10 * i) + j].x == shuffledDominoes[(10 * i) + j].y)   //
                    {                                                                           //
                        doubleCounter++;                                                        //
                    }                                                                           //
                }                                                                               //
                                                                                                //
                if(doubleCounter >= 5)                                                          //
                {                                                                               //
                    fiveDoubles = true;                                                         //
                }                                                                               //
            }                                                                                   //

            if(fiveDoubles == true)                         // if it will, then reshuffle.
            {                                               //
                shuffledDominoes = Shuffle(allDominoes);    //
            }                                               //
            else
            {                                               // if not, then mark it verified.
                verified = true;                            //
                break;                                      //
            }                                               //
        }


        for (int i = 0; i < 40; i++)                                              // Finally, fill the player dominos list
        {                                                                         //
            this.playerDominoes.Add(shuffledDominoes[i]);
        }

    }
    public IEnumerator InstanceDominoes()
    {
        playerDominoFields = new GameObject[4];
        serverPlayerDominoes = new GameObject[4][];
        for(int i = 0;i < 4; i++)
        {
            GameObject temp = GameObject.Instantiate(playerDominoFieldBase, new Vector3(Mathf.Sin(1.5708f * i) * -4.328f, 0.2f, Mathf.Cos(1.5708f * i) * -4.328f), Quaternion.Euler(-64.191f, i * 90, 0));
            temp.GetComponent<NetworkObject>().Spawn();
            yield return new WaitForFixedUpdate();
            playerDominoFields[i] = temp;
            playerDominoFields[i].name = "Player" + (i + 1) + "_DomnioField";
            PlayerDominoField pDomField = playerDominoFields[i].GetComponent<PlayerDominoField>();
            pDomField.dominoPositions = new NetworkList<Vector3>(new List<Vector3>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


            List<GameObject> tempDoms = new List<GameObject>();
            for (int j = 0;j < 10; j++)
            {
                int myX = Mathf.RoundToInt(playerDominoes[(i*10) + j].x);
                int myY = Mathf.RoundToInt(playerDominoes[(i * 10) + j].y);
                GameObject go = GameObject.Instantiate(serverDomino);
                go.GetComponent<NetworkObject>().Spawn();
                yield return new WaitForFixedUpdate();
                DominoStats goStats = go.transform.GetComponent<DominoStats>();
                goStats.myVirtualParentPos = new NetworkVariable<Vector3>(playerDominoFields[i].transform.position, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
                pDomField.dominoPositions.Add(playerDominoFields[i].transform.right * (-1.35f + (j * 0.3f)));
                go.transform.rotation = pDomField.transform.rotation;
                go.transform.localPosition = pDomField.transform.position + pDomField.dominoPositions[j];

                goStats.myValue = playerDominoes[(i * 10) + j];
                goStats.myName = $"P{i}Domino_{myX}_{myY}";
                goStats.myTag = $"P{i + 1}Dominoes";
                goStats.myID = (i * 10) + j;

                Material[] myMat = go.transform.GetChild(0).transform.GetComponent<MeshRenderer>().materials;
                myMat[1].mainTexture = dominoTextures[myX];
                myMat[2].mainTexture = dominoTextures[myY];
                go.transform.GetChild(0).transform.GetComponent<MeshRenderer>().materials = myMat;

                tempDoms.Add(go);
            }
            serverPlayerDominoes[i] = tempDoms.ToArray();
        }
    }
    private List<Vector3> Shuffle(List<Vector3> array)      // shuffles a Vector3 array
    {                                               // if you know how to make this work for any type of array, be my guest.
        List<Vector3> shuffledDoms = new List<Vector3>();
        for(int i = 0; i <  array.Count; i++)
        {
            shuffledDoms.Add(array[i]);
        }


        int n = array.Count;                                               // Fisher-Yates Shuffle I found on the internet
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider(); // this shit is way above my paygrade
        while (n > 1)                                                       //
        {                                                                   //
            byte[] box = new byte[1];                                       //
            do provider.GetBytes(box);                                      //
            while (!(box[0] < n * (Byte.MaxValue / n)));                    //
            int k = (box[0] % n);                                           //
            n--;                                                            //
            Vector2 value = shuffledDoms[k];                                //
            shuffledDoms[k] = shuffledDoms[n];                              //
            shuffledDoms[n] = value;                                        //
        }                                                                   //
        return shuffledDoms;
    }

    public void SendServerRequests()
    {
        for (int i = 0; i < 55; i++)
        {
            AskForValuesRpc(i);


        }
    }

    [Rpc(SendTo.Server)]
    public void AskForValuesRpc(int val)
    {
        if(val < 40)
        {
            ReturnValuesRpc(allDominoes[val], shuffledDominoes[val], playerDominoes[val]);
        }
        else
        {
            ReturnValuesRpc(allDominoes[val], shuffledDominoes[val], playerDominoes[0]);
        }
        
        Debug.Log("GameManager sent value request to server");
    }

    [Rpc(SendTo.NotServer)]
    public void ReturnValuesRpc(Vector3 allDom, Vector3 shuffledDom, Vector3 playerDom)
    {
        
        this.allDominoes.Add(allDom);
        this.shuffledDominoes.Add(shuffledDom);
        this.playerDominoes.Add(playerDom);
        Debug.Log("GameManager received values back from server");
    }




}
