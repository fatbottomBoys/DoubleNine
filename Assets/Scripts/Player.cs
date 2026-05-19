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


    [Header("My cameras")]
    public GameObject frontCam;
    public Camera frontCamCamera;
    public GameObject topCam;
    public Camera topCamCamera;
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
            LoadIn();
        }

        
    }

    public void Update()
    {
        if (IsClient && Keyboard.current.qKey.wasPressedThisFrame)
        {
            PingRpc(playerID);
        }

        if (IsOwner)
        {
            if (mainDominoField.numDominoesPlayed != 0)
            {
                CheckPlayableTiles();
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)   // need to figure out touch controls
            {                                                   // should be "Touchscreen.current. _____________
                heldDomino = ClickObj();
            }
            else if (!Mouse.current.leftButton.isPressed && heldDomino != null)
            {
                heldDomino = null;
                triggerRepos = true;
            }

            if(heldDomino != null)
            {
                heldDomino.transform.position = CurMousePos();
            }
            ReposDominos();
        }
    }

    public void LoadIn()
    {
        if (IsOwner)
        {
            frontCam.SetActive(true);
            frontCam.transform.GetComponent<Camera>().enabled = true;
            frontCam.transform.GetComponent<AudioListener>().enabled = true;
        }

        



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

        List<Vector3> tempValues = new List<Vector3>();
        for (int i = 0; i < 10; i++)
        {
            tempValues.Add(gameManager.playerDominoes[(playerID * 10) + i]);
        }
        myDominoValues = tempValues.ToArray();

        this.gameObject.transform.rotation = Quaternion.Euler(0, 90 * playerID, 0); 
        GameObject DFParent = GameObject.Instantiate(baseDominoField, this.gameObject.transform.position, this.gameObject.transform.rotation);
        DFParent.name = "Player" + (playerID + 1) + "_DomnioField";
        DFParent.GetComponent<NetworkObject>().Spawn();
        myDominoField = DFParent.transform.GetChild(0).transform.gameObject;

        //myDominoField.transform.parent = null;
        myDominoField.name = "Player" + (playerID + 1) + "_DomnioField_Offset";
        this.gameObject.name = "Player" + (playerID + 1);

        

        PopLocalDominoes();
        StartCoroutine(CamAnim());


    }

    public void PopLocalDominoes()
    {
        dominoPositions = new Vector3[10];
        List<GameObject> tempDoms = new List<GameObject>();
        for (int i = 0; i < myDominoValues.Length; i++) 
        {
            
            int myX = Mathf.RoundToInt(myDominoValues[i].x);
            int myY = Mathf.RoundToInt(myDominoValues[i].y);
            GameObject go = GameObject.Instantiate(baseDomino, myDominoField.transform);
            dominoPositions[i] = new Vector3(-1.35f + (i * 0.3f), 0, 0);
            go.transform.localPosition = dominoPositions[i];
            go.name = "Domino_" + myX + "_" + myY;
            DominoStats goStats = go.transform.GetComponent<DominoStats>();
            goStats.myValue = myDominoValues[i];
            goStats.myID = i;

            Material[] myMat = go.transform.GetChild(0).transform.GetComponent<MeshRenderer>().materials;
            myMat[1].mainTexture = dominoTextures[myX];
            myMat[2].mainTexture = dominoTextures[myY];
            go.transform.GetChild(0).transform.GetComponent<MeshRenderer>().materials = myMat;

            tempDoms.Add(go);
        }
        myDominoes = tempDoms.ToArray();

    }
    public void CheckPlayableTiles()
    {
        dominosOnField = mainDominoField.getCurrentValuesOnField();
        int left = dominosOnField[0];
        int right = dominosOnField[1];

        for(int i = 0; i < myDominoValues.Length; i++)
        {
            if (myDominoValues[i].x != left && myDominoValues[i].y != left && myDominoValues[i].x != right && myDominoValues[i].y != right)
            {
                Material[] dominoe = myDominoes[i].transform.GetChild(0).transform.GetComponent<MeshRenderer>().materials;
                for (int j = 0; j < dominoe.Length; j++)
                {
                    dominoe[j].color = new Color(1,1,1,0.50f);
                }
            }
            else
            {
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
        if(heldDomino != null && heldDomino.transform.localPosition.z <= 0.7f)
        {
            int domID = heldDomino.transform.GetComponent<DominoStats>().myID;
            int[] closestDominoes = new int[2];
            closestDominoes[0] = 0;
            closestDominoes[1] = 0;

            List<GameObject> nonHeldDoms = new List<GameObject>();      // put all the non-held dominoes in a list
            for (int i = 0; i < myDominoes.Length; i++)
            {
                if (reposArray[i] != domID)
                {
                    nonHeldDoms.Add(myDominoes[reposArray[i]]);
                    print("Added domino at ID " + i);
                }
                
            }

            for (int i = 1; i < nonHeldDoms.Count; i++)     // check which two dominos in the sequence are closest to the held one.
            {
                if (Vector3.Distance(myDominoes[domID].transform.localPosition, nonHeldDoms[i].transform.localPosition) <
                    Vector3.Distance(myDominoes[domID].transform.localPosition, nonHeldDoms[closestDominoes[0]].transform.localPosition))
                {
                    closestDominoes[1] = closestDominoes[0];
                    closestDominoes[0] = Array.IndexOf(myDominoes, nonHeldDoms[i]);
                }
            }

            int idToSkip = 0;                                   //figure out where the gap should be

            if(closestDominoes[0] == 0)                         // at the very beginning?
            {
                idToSkip = 0;
            }
            else if(closestDominoes[0] == nonHeldDoms.Count)    //at the very end?
            {
                idToSkip = nonHeldDoms.Count;
            }
            else if (closestDominoes[0] > closestDominoes[1])   // at 0?
            {
                idToSkip = closestDominoes[0];
            }
            else
            {
                idToSkip = closestDominoes[1];                  // or at 1?
            }

            int temp = 0;
            for (int i = 0; i < myDominoes.Length; i++)   //make a new list containing all the values, INCLUDING the gap, 
            {
                if (i != idToSkip)
                {
                    reposArray[i] = Array.IndexOf(myDominoes, nonHeldDoms[temp]);
                    nonHeldDoms[temp].transform.localPosition = dominoPositions[i];
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
                myDominoes[reposArray[i]].transform.localPosition = dominoPositions[i];
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
    private GameObject ClickObj()
    {
        LayerMask lMask = LayerMask.GetMask("Domino");
        Ray ray = frontCamCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 100, lMask))
        {
            return raycastHit.collider.gameObject;
        }
        else
        {
            return null;
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

    [Rpc(SendTo.Server)]
    public void PingRpc(int player)
    {
        // Server -> Clients because PongRpc sends to NotServer
        // Note: This will send to all clients.
        // Sending to the specific client that requested the pong will be discussed in the next section.
        PongRpc(player, "PONG!");
        Debug.Log($"Received ping from player {player}");
    }

    [Rpc(SendTo.NotServer)]
    void PongRpc(int player, string message)
    {
        Debug.Log($"Received pong from server originating from {player}");
    }

    //[Rpc(SendTo.Server)]
    //public void C2SDominoRpc(GameObject obj, GameObject virtualparent, string name)
    //{
    //    Debug.Log($"Received Domino spawn request from player {playerID}, sending request back");
    //    S2CDominoRpc(obj);
    //}

    //[Rpc(SendTo.NotServer)]
    //public void S2CDominoRpc(GameObject newObj)
    //{
    //    Debug.Log($"Received Domino spawn request back from player {playerID}");
    //}

    



    // test functions ///////////////////////////////////////////


}
