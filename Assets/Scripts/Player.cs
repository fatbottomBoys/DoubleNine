using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class Player : NetworkBehaviour
{
    public int playerID;
    public GameManager gameManager;

    [Header("Inputs")]
    private InputActionReference inputRef;
    public event Action touchTap;
    public event Action touchHold;
    private Vector3 mousePos;

    [Header("My dominoes")]
    public Vector3[] myDominoValues;
    public GameObject[] myDominoes;
    [SerializeField] private Vector3[] dominoPositions;
    [SerializeField] private int[] reposArray;
    [SerializeField] private bool triggerRepos;
    public GameObject baseDominoField;
    public GameObject myDominoField;
    public GameObject baseDomino;
    public Texture2D[] dominoTextures;
    public DominoField mainDominoField;
    public int[] dominosOnField;
    [SerializeField] private GameObject heldDomino;
    [SerializeField] private GameObject selectedDomino;



    [Header("My cameras")]
    public GameObject frontCam;
    public Camera frontCamCamera;
    public GameObject topCam;
    public Camera topCamCamera;
    public GameObject PlayerUI;
    public Vector3[] camPos;
    public Vector3[] camRot;
    [SerializeField] private AnimationCurve camAnimFlow;
    public float camAnimTime;
    public float animFrameRate;

    [Header("Test stuff")]
    public GameObject vis;
    

    public override void OnNetworkSpawn()
    {
        if (playerID == -1)
        {
            StartCoroutine(LoadIn());
        }

        
    }

    public void Update()
    {
        if (playerID != -1) 
        {
            //if (IsClient && Keyboard.current.qKey.wasPressedThisFrame)
            //{
            //    PingRpc(playerID);
            //}

            if (IsOwner)
            {
                //Debug.Log($"I am the owner of {this.gameObject.name}.");
                if (mainDominoField.numDominoesPlayed > 0 && dominosOnField != gameManager.playableEnds)
                {
                    dominosOnField = gameManager.playableEnds;
                    CheckPlayableTiles();
                }
                

                if (IsHost)
                {
                    mousePos = CurMousePos();
                    if (Mouse.current.leftButton.wasPressedThisFrame && ClickObj() != "0")   // need to figure out touch controls
                    {                                                   // should be "Touchscreen.current. _____________
                        GameObject go = GameObject.Find(ClickObj());
                        if (go.transform.GetComponent<DominoStats>().isPlayable && go.CompareTag($"P{playerID + 1}Dominoes"))
                        {
                            heldDomino = go;
                        }
                    }
                    else if (!Mouse.current.leftButton.isPressed && heldDomino != null)
                    {
                        //GameObject go = GameObject.Find(ClickObj());
                        
                        heldDomino = null;
                        triggerRepos = true;
                    }
                }
                else
                {

                    if (Mouse.current.leftButton.wasPressedThisFrame && ClickObj() != "0")   // need to figure out touch controls
                    {                                                   // should be "Touchscreen.current. _____________
                        SendMousePosRpc(CurMousePos());
                        GameObject go = GameObject.Find(ClickObj());
                        if (go.transform.GetComponent<DominoStats>().isPlayable && go.CompareTag($"P{playerID + 1}Dominoes"))
                        {
                            heldDomino = go;
                            SendHeldDomNameRpc(ClickObj(), false);
                        }
                    }
                    else if(!Mouse.current.leftButton.isPressed && heldDomino != null)
                    {
                        heldDomino = null;
                        SendHeldDomNameRpc("0", true);
                    }
                    else if (Mouse.current.leftButton.isPressed)
                    {
                        SendMousePosRpc(CurMousePos());
                    }
                }

                
            }

            ReposDominos();
            if (heldDomino != null)
            {
                heldDomino.transform.position = mousePos;
            }

        }
    }

    public IEnumerator LoadIn()
    {
        yield return new WaitForSeconds(2);
        if (IsOwner)
        {
            frontCam.SetActive(true);
            frontCam.transform.GetComponent<Camera>().enabled = true;
            frontCam.transform.GetComponent<AudioListener>().enabled = true;
        }

        AskForValuesRpc();



        GameObject gM = GameObject.Find("GameManager");
        gameManager = gM.transform.GetComponent<GameManager>();

        GameObject dF = GameObject.Find("DominoField");
        mainDominoField = dF.transform.GetComponent<DominoField>();

        List<GameObject> tempPlayers = new List<GameObject>();
        if (gameManager.players.Length > 0) 
        { 
            for(int i = 0;i < gameManager.players.Length; i++)
            {
                tempPlayers.Add(gameManager.players[i]);
            }

            playerID = gameManager.players.Length;

            tempPlayers.Add(this.transform.gameObject);
            gameManager.players = tempPlayers.ToArray();
        }
        else
        {
            playerID = 0;
            tempPlayers.Add(this.transform.gameObject);
            gameManager.players = tempPlayers.ToArray();
        }

        gameManager.SendServerRequests();
        while(gameManager.playerDominoes.Count < 40)
        {
            yield return new WaitForFixedUpdate();
            //Debug.Log("waiting for server to fill up GM's dominoes list");
        }

        List<Vector3> tempValues = new List<Vector3>();
        for (int i = 0; i < 10; i++)
        {
            tempValues.Add(gameManager.playerDominoes[(playerID * 10) + i]);
        }
        myDominoValues = tempValues.ToArray();

        this.gameObject.transform.rotation = Quaternion.Euler(0, 90 * playerID, 0);
        myDominoField = GameObject.Find("Player" + (playerID + 1) + "_DomnioField");
        dominoPositions = myDominoField.GetComponent<PlayerDominoField>().dominoPositions;
        this.gameObject.name = "Player" + (playerID + 1);

        

        PopLocalDominoes();


        StartCoroutine(CamAnim());




    }
    public void PopLocalDominoes()
    {
        List<GameObject> tempDoms = new List<GameObject>();
        myDominoes = GameObject.FindGameObjectsWithTag($"P{playerID + 1}Dominoes");

        for (int i = 0; i < 10; i++) 
        {
            
            myDominoes[i].transform.position = myDominoField.transform.position + dominoPositions[i];
            DominoStats domStats = myDominoes[i].transform.GetComponent<DominoStats>();
            myDominoValues[i] = domStats.myValue;
            Material[] myMat = myDominoes[i].transform.GetChild(0).transform.GetComponent<MeshRenderer>().materials;

            myMat[1].mainTexture = dominoTextures[Mathf.RoundToInt(domStats.myValue.x)];
            myMat[2].mainTexture = dominoTextures[Mathf.RoundToInt(domStats.myValue.y)];
        }

    }
    public void CheckPlayableTiles()
    {
        int left = dominosOnField[0];
        int right = dominosOnField[1];

        for(int i = 0; i < myDominoValues.Length; i++)
        {
            if (myDominoValues[i].x != left && myDominoValues[i].y != left && myDominoValues[i].x != right && myDominoValues[i].y != right)
            {
                Debug.Log($"{myDominoes[i].name} was deemed playable by values l:{left} and r:{right}");
                DominoStats ds = myDominoes[i].transform.GetComponent<DominoStats>();
                ds.isPlayable = false;
                Material[] dominoe = myDominoes[i].transform.GetChild(0).transform.GetComponent<MeshRenderer>().materials;
                for (int j = 0; j < dominoe.Length; j++)
                {
                    dominoe[j].color = new Color(1,1,1,0.50f);
                }
            }
            else
            {
                Debug.Log($"{myDominoes[i].name} was deemed UNplayable by values l:{left} and r:{right}");
                DominoStats ds = myDominoes[i].transform.GetComponent<DominoStats>();
                ds.isPlayable = true;
                Material[] dominoe = myDominoes[i].transform.GetChild(0).transform.GetComponent<MeshRenderer>().materials;
                for (int j = 0; j < dominoe.Length; j++)
                {
                    dominoe[j].color = new Color(1, 1, 1, 1f);
                }
            }
        }
    }
    public void GrabDomino()
    {
        //I didn't know the reason for this function but I figured it was a selecting which domino to play kind of thing
        // but I am currently not calling the play function just yet.
        //PlayDominoRpc(heldDomino);
    }
    public void PlayLeft()                                                                      //Ui trigger function for playing held domino on left side
    {
        mainDominoField.PlayDomino(heldDomino.transform.GetComponent<DominoStats>().myValue, 0);
    }
    public void PlayRight()                                                                      //Ui trigger function for playing held domino on right side
    {
        mainDominoField.PlayDomino(heldDomino.transform.GetComponent<DominoStats>().myValue, 1);
    }
    public void PlayDomino(GameObject domino)
    {
        
        dominosOnField = mainDominoField.getCurrentValuesOnField();
        int left = dominosOnField[0];
        int right = dominosOnField[1];

        //take input domino object and check for playability
        DominoStats dominoObject = domino.transform.GetComponent<DominoStats>();

        if (dominoObject.myValue.x == left || dominoObject.myValue.x == right || dominoObject.myValue.y == right || dominoObject.myValue.y == left)
        {
            //unhide ui for deciding play
            PlayerUI.SetActive(true);
        }
        else if(dominoObject.myValue.x == left || dominoObject.myValue.y == left)
        {
            // may need to make an RPC here???
            mainDominoField.PlayDomino(dominoObject.myValue, 0);
        }
        else
        {
            // may need to make an RPC here as well???
            mainDominoField.PlayDomino(dominoObject.myValue, 1);
        }

    }
    public IEnumerator CamAnim()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < camAnimTime * animFrameRate; i++)
        {
            float t = i / (camAnimTime * animFrameRate);

            frontCam.transform.localPosition = Vector3.Lerp(camPos[0], camPos[1], camAnimFlow.Evaluate(t));
            frontCam.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(camRot[0]), Quaternion.Euler(camRot[1]), camAnimFlow.Evaluate(t));

            yield return new WaitForFixedUpdate();
        }

        yield break;
    }
    public void ReposDominos()
    {
        if(heldDomino != null)
        {
            int domID = heldDomino.transform.GetComponent<DominoStats>().myID;
            //Debug.Log("Got Domino Stats");
            int totalDominoes = myDominoes.Length;
            int[] closestDominoes = new int[2];
            closestDominoes[0] = 0;
            closestDominoes[1] = 0;

            List<GameObject> nonHeldDoms = new List<GameObject>();      // put all the non-held dominoes in a list
            for (int i = 0; i < myDominoes.Length; i++)
            {
                if (reposArray[i] != domID)
                {
                    nonHeldDoms.Add(myDominoes[reposArray[i]]);
                    //Debug.Log($"Added domino at ID {i}, current count = {nonHeldDoms.Count}");
                }
                
            }

            for (int i = 0; i < myDominoes.Length; i++)     // check which two domino positions in the sequence are closest to the held one.
            {
                if (Vector3.Distance(myDominoes[domID].transform.position, dominoPositions[i] + myDominoField.transform.position) <
                    Vector3.Distance(myDominoes[domID].transform.position, dominoPositions[closestDominoes[1]] + myDominoField.transform.position))
                {
                    closestDominoes[0] = closestDominoes[1];
                    closestDominoes[1] = i;
                    
                }
            }

            int idToSkip = 0;                                   //figure out where the gap should be

            float absPos = 0f;
            //Debug.Log($"{myDominoField.transform.right.x}, {myDominoField.transform.right.y}, {myDominoField.transform.right.z}");
            // sometimes these positions are going to be on the Z axis as well.
            if (Mathf.Abs(myDominoField.transform.right.x) >= 0.00001f)
            {
                absPos = Mathf.Abs(myDominoes[domID].transform.position.x - dominoPositions[closestDominoes[1]].x);
            }
            else
            {
                absPos = Mathf.Abs(myDominoes[domID].transform.position.z - dominoPositions[closestDominoes[1]].z);
            }

            if(closestDominoes[0] == 0 && absPos > .35)                         // at the very beginning?
            {
                idToSkip = 0;
            }
            else if(closestDominoes[1] == nonHeldDoms.Count && absPos > .35)   //at the very end?
            {
                //Debug.Log($"heldDom = farthest right, calculating from id {closestDominoes[1]}, nonHeldDoms.count is {nonHeldDoms.Count}");
                idToSkip = myDominoes.Length - 1;
            }
            else if (closestDominoes[0] > closestDominoes[1])   // at 0?
            {
                idToSkip = closestDominoes[0];
            }
            else
            {
                idToSkip = closestDominoes[1];                  // or at 1?
            }

            //Debug.Log($"ID to skip is {idToSkip}");

            int temp = 0;
            for (int i = 0; i < myDominoes.Length; i++)   //make a new list containing all the values, INCLUDING the gap, 
            {
                if (i != idToSkip)
                {
                    reposArray[i] = Array.IndexOf(myDominoes, nonHeldDoms[temp]);
                    nonHeldDoms[temp].transform.position = dominoPositions[i] + myDominoField.transform.position;
                    temp++;
                }
                else
                {
                    reposArray[i] = domID;
                }
            }

        }
        else if(triggerRepos == true)
        {
            for(int i = 0; i < myDominoes.Length; i++)
            {
                myDominoes[reposArray[i]].transform.position = dominoPositions[i] + myDominoField.transform.position;
            }

            triggerRepos = false;
        }
    }
    private Vector3 CurMousePos()
    {
        LayerMask lMask = LayerMask.GetMask("UI");
        
        Ray ray = frontCamCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if(Physics.Raycast(ray, out RaycastHit raycastHit, 100, lMask))
        {
            return raycastHit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }
    private string ClickObj()
    {
        LayerMask lMask = LayerMask.GetMask("Domino");
        Ray ray = frontCamCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 100, lMask))
        {
            return raycastHit.collider.gameObject.name;
        }
        else
        {
            return "0";
        }
    }
    private Vector3 TouchPoint()
    {
        Ray ray = frontCamCamera.ScreenPointToRay(Touchscreen.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            return raycastHit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }

    //[Rpc(SendTo.Server)]
    //public void PingRpc(int player)
    //{
    //    // Server -> Clients because PongRpc sends to NotServer
    //    // Note: This will send to all clients.
    //    // Sending to the specific client that requested the pong will be discussed in the next section.
    //    PongRpc(player, "PONG!");
    //    Debug.Log($"Received ping from player {player}");
    //}

    //[Rpc(SendTo.NotServer)]
    //void PongRpc(int player, string message)
    //{
    //    Debug.Log($"Received pong from server originating from {player}");
    //}

    [Rpc(SendTo.Server)]
    public void AskForValuesRpc()
    {
        SendValuesBackRpc(this.name);
    }

    [Rpc(SendTo.NotServer)]
    public void SendValuesBackRpc(string name)
    {
        this.name = name;
    }

    [Rpc(SendTo.Server)]
    public void SendHeldDomNameRpc(string name, bool trigRepos)
    {
        if(name != "0")
        {
            heldDomino = GameObject.Find(name);
        }
        else
        {
            heldDomino = null;
        }

        this.triggerRepos = trigRepos;
    }

    [Rpc(SendTo.Server)]
    public void SendMousePosRpc(Vector3 mPos)
    {
        mousePos = mPos;
    }







    // test functions ///////////////////////////////////////////


}
