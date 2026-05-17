using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int playerID;
    public GameManager gameManager;

    [Header("My dominoes")]
    public Vector3[] myDominoValues;
    public GameObject[] myDominoes;
    public GameObject myDominoField;
    public GameObject baseDomino;
    public Texture2D[] dominoTextures;
    public DominoField mainDominoField;
    public int[] dominosOnField;

    [Header("My cameras")]
    public GameObject frontCam;
    public GameObject topCam;

    public void PopLocalDominoes()
    {
        List<GameObject> tempDoms = new List<GameObject>();
        for (int i = 0; i < myDominoValues.Length; i++) 
        {
            int myX = Mathf.RoundToInt(myDominoValues[i].x);
            int myY = Mathf.RoundToInt(myDominoValues[i].y);
            GameObject go = GameObject.Instantiate(baseDomino, myDominoField.transform);
            go.transform.localPosition = new Vector3(-1.35f + (i * 0.3f), 0, 0);
            go.name = "Domino_" + myX + "_" + myY;
            DominoStats goStats = go.transform.GetComponent<DominoStats>();
            goStats.myValue = myDominoValues[i];

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
            if (myDominoValues[i].x != left || myDominoValues[i].y != left || myDominoValues[i].x != right || myDominoValues[i].y != right)
            {
                Material[] dominoe = myDominoes[i].transform.GetChild(0).transform.GetComponent<MeshRenderer>().materials;
                for (int j = 0; j < dominoe.Length; j++)
                {
                    dominoe[j].color = new Color(1,1,1,0.25f);
                }
            }

        }
    }

    public void Update()
    {
        CheckPlayableTiles();
    }


    public void PlayTile()
    {
        

    }
}
