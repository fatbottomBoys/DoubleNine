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

    [Header("My dominoes")]
    public Vector3[] myDominoValues;
    public GameObject[] myDominoes;
    public GameObject myDominoField;
    public GameObject baseDomino;
    public Texture2D[] dominoTextures;

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

            Material[] myMat = go.transform.GetChild(0).transform.GetComponent<MeshRenderer>().materials;
            myMat[1].mainTexture = dominoTextures[myX];
            myMat[2].mainTexture = dominoTextures[myY];
            go.transform.GetChild(0).transform.GetComponent<MeshRenderer>().materials = myMat;

            tempDoms.Add(go);
        }
        myDominoes = tempDoms.ToArray();
    }
}
