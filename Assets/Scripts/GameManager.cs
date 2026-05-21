using NUnit.Framework;
using Unity.Netcode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    public NetworkList<Vector3> playerDominoes;

    [Header("Game Start stuff")]
    [SerializeField]private MultiplayerUI m_multiplayerUI;

    [Header("Test Stuff")]
    public GameObject serverDomino;
    public Vector3 dominoPos;

    public void Awake()
    {
        playerDominoes = new NetworkList<Vector3>(new List<Vector3>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        if (!IsClient)
        {
            PopulateDominoes();
            DistributeDominoes();

            
            GameObject dom = GameObject.Instantiate(serverDomino);
            dom.name = "TEST";
            dom.transform.position = dominoPos;
            dom.GetComponent<NetworkObject>().Spawn();
        }


        

        //initialize debug multiplayer connectivity
        if(m_multiplayerUI != null)
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
    }

    void Update()
    {
        
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
            playerDominoes.Add(shuffledDominoes[i]);
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

 
}
