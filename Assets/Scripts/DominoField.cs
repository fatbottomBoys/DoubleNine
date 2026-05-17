using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class DominoField : MonoBehaviour
{

    public GameManager gM;

    [Header("Domnio prefab")]
    public GameObject dominoPrefab;
    public Texture2D[] dominoTextures;
    [Tooltip("0 is the full domino length, 1 is the double domino length")]
    public float[] dominoHalfSize;

    [Header("Current Field Stats")]
    [SerializeField] private int numDominoesPlayed;
    [SerializeField] private Vector3[] dominosPlayed;
    [SerializeField] private float anchorPoint;
    [SerializeField] private int[] domsPlayedPerSide;       // 0 is pos z, 1 is neg z
    [SerializeField] private float[] spaceOnTable;          // "
    [SerializeField] private Vector3[] nextStandardPos;     // "
    [SerializeField] private Vector3[] nextStandardRot;     // "
    [SerializeField] private Vector3[] nextDoublePos;       // "
    [SerializeField] private Vector3[] nextDoubleRot;       // "
    [SerializeField] private int[] currentValuesOnField;    // "
    [SerializeField] private int[] multiplier;              // "
    [SerializeField] private int[] curSection;              // "
    [SerializeField] private bool[] prevDomWasDouble;       // "

    [SerializeField] private bool[] specCirc1;       //whether the second to last domino in a LONG section was a standard domino or not.
    [SerializeField] private bool[] specCirc2;       //whether the second to last domino in a SHORT section was a standard domino or not.


    [Tooltip("how far along can the dominoes go before they are forced to turn. 0 is the longer sections, 1 is the shorter sections")]
    [SerializeField] private float[] sizeLimiter;
    [SerializeField] private Vector3[][] cornerMove;

    [Header("Test Stuff")]
    [SerializeField] private Vector3 doubleDom;
    [SerializeField] private Vector3 standardDom;

    public void Start()
    {
        cornerMove = new Vector3[2][];
        cornerMove[0] = new Vector3[2]; // 0 is for standard dominos, 1 is for doubles
        cornerMove[1] = new Vector3[2]; // "
    }

    public void Update()
    {
        if(numDominoesPlayed <= 40)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                StartCoroutine(PlayDomino(standardDom, 0));
            }
            else if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                StartCoroutine(PlayDomino(standardDom, 1));
            }
            else if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                StartCoroutine(PlayDomino(doubleDom, 1));
            }
            else if (Keyboard.current.dKey.wasPressedThisFrame)
            {
                StartCoroutine(PlayDomino(doubleDom, 0));
            }
        }
    }

    public IEnumerator PlayDomino(Vector3 domino, int side)
    {
        List<Vector3> tempDomsPlayed = new List<Vector3>();
        for (int i = 0; i < dominosPlayed.Length; i++) 
        {
            tempDomsPlayed.Add(dominosPlayed[i]);
        }
        numDominoesPlayed++;
        domsPlayedPerSide[side]++;
        bool firstDom = false;
        if(numDominoesPlayed == 1)
        {
            firstDom = true;
        }
        bool isDouble = false;
        int newValue = 0;
        if (firstDom)
        {
            currentValuesOnField[0] = Mathf.RoundToInt(domino.x);
            currentValuesOnField[1] = Mathf.RoundToInt(domino.y);
        }
        int prevValueOnField = currentValuesOnField[side];

        // generate the domino & update stats /////////////////////////////////////////////////////////


        if(domino.x == domino.y)
        {
            isDouble = true;
            newValue = Mathf.RoundToInt(domino.x);
            currentValuesOnField[side] = newValue;
        }
        else if(!firstDom)
        {
            if(domino.x == currentValuesOnField[side])
            {
                newValue = Mathf.RoundToInt(domino.y);
            }
            else
            {
                newValue = Mathf.RoundToInt(domino.x);
            }
            currentValuesOnField[side] = newValue;
        }

        GameObject newDomino = GameObject.Instantiate(dominoPrefab, this.transform);

        Material[] myMat = newDomino.transform.GetChild(0).transform.GetComponent<MeshRenderer>().materials;
        myMat[1].mainTexture = dominoTextures[prevValueOnField];

        DominoStats goStats = newDomino.transform.GetComponent<DominoStats>();
        Vector3 newDomValue = new Vector3(0, 0, 1);
        goStats.isActive = true;

        if (!firstDom)
        {
            if (prevValueOnField <= newValue)
            {
                newDomValue.x = prevValueOnField;
                newDomValue.y = newValue;
            }
            else
            {
                newDomValue.x = newValue;
                newDomValue.y = prevValueOnField;
            }
        }
        else
        {
            newDomValue.x = domino.x;
            newDomValue.y = domino.y;
        }
        
        goStats.myValue = newDomValue;

        if (isDouble)
        {
            goStats.isDouble = true;
            myMat[1].mainTexture = dominoTextures[prevValueOnField];
            myMat[2].mainTexture = dominoTextures[prevValueOnField];
        }
        else if(!firstDom)
        {
            myMat[1].mainTexture = dominoTextures[newValue];
            myMat[2].mainTexture = dominoTextures[prevValueOnField];
        }
        else
        {
            myMat[1].mainTexture = dominoTextures[Mathf.RoundToInt(domino.x)];
            myMat[2].mainTexture = dominoTextures[Mathf.RoundToInt(domino.y)];
        }

        gM.playableEnds = currentValuesOnField;

        // place the domino down //////////////////////////////////////////////

        if(!isDouble)
        {
            newDomino.transform.localRotation = Quaternion.Euler(nextStandardRot[side] + new Vector3(0, 180f * side, 0));
            newDomino.transform.localPosition = nextStandardPos[side];
            
        }
        else
        {
            newDomino.transform.localRotation = Quaternion.Euler(nextDoubleRot[side]);
            newDomino.transform.localPosition = nextDoublePos[side];
            
        }

        tempDomsPlayed.Add(newDomino.transform.localPosition);
        Vector3 tempAvg = Vector3.zero;
        for(int i = 0; i < tempDomsPlayed.Count; i++)
        {
            tempAvg += tempDomsPlayed[i];
        }
        tempAvg = new Vector3((tempAvg.x / tempDomsPlayed.Count) * -1, 0, 0);
        if (Mathf.Abs(tempAvg.x) >= anchorPoint)
        {
            this.transform.position = tempAvg;
        }
        dominosPlayed = tempDomsPlayed.ToArray();

        // figure out where the next domino is gonna go ////////////////////////

        if (isDouble)
        {
            prevDomWasDouble[side] = true;
        }
        else
        {
            prevDomWasDouble[side] = false;
        }



        if (!firstDom)
        {
            if (curSection[side] % 2 == 0 && spaceOnTable[side] + (2 * dominoHalfSize[0]) + dominoHalfSize[1] >= sizeLimiter[0]) // if the current section is LONG and its OVER the long limit
            {
                print("Long section " + curSection[side] + " on side " + side + "'s second to last tile...");
                curSection[side]++;
                spaceOnTable[side] = 0;

                if (side ==  0)
                {
                    multiplier[side] = 1;
                }
                else
                {
                    multiplier[side] = -1;
                }

                int tempMult = 0;
                if(newDomino.transform.localPosition.z > 0)
                {
                    tempMult = 1;
                }
                else
                {
                    tempMult = -1;
                }

                if (isDouble)
                {
                    print("was a double");
                    nextStandardRot[side] += new Vector3(0, 90 * tempMult, 0);
                    nextStandardPos[side] = newDomino.transform.localPosition + new Vector3(dominoHalfSize[0] * 2 * multiplier[side], 0, 0);
                    nextDoubleRot[side] += new Vector3(0, 90, 0);
                }
                else
                {
                    print("wasn't a double");
                    nextStandardRot[side] += new Vector3(0, 90 * tempMult, 0);
                    nextStandardPos[side] = newDomino.transform.localPosition + new Vector3(dominoHalfSize[1] * multiplier[side], 0, (dominoHalfSize[0] + dominoHalfSize[1]) * tempMult);
                    nextDoublePos[side] = newDomino.transform.localPosition + new Vector3(0, 0, (dominoHalfSize[0] + dominoHalfSize[1]) * tempMult);
                    specCirc1[side] = true;
                }
                

            }
            else if (curSection[side] % 2 == 1 && spaceOnTable[side] + (2 * dominoHalfSize[0]) + dominoHalfSize[1] >= sizeLimiter[1]) // or if the current section is SHORT and its OVER the short limit
            {
                print("Short section " + curSection[side] + " on side " + side + "'s second to last tile...");
                curSection[side]++;
                spaceOnTable[side] = -1 * sizeLimiter[0];

                if(newDomino.transform.position.z > 0)
                {
                    multiplier[side] = -1;
                }
                else
                {
                    multiplier[side] = 1;
                }

                int tempMult = 0;
                if (newDomino.transform.localPosition.x > 0)
                {
                    tempMult = 1;
                }
                else
                {
                    tempMult = -1;
                }


                if (isDouble)
                {
                    print("was a double");
                    nextStandardPos[side] = newDomino.transform.localPosition + new Vector3(0, 0, dominoHalfSize[0] * 2 * multiplier[side]);
                    nextStandardRot[side] += new Vector3(0, (90 * multiplier[side]) + 180, 0);
                    nextDoubleRot[side] += new Vector3(0, 90, 0);
                }
                else
                {
                    print("wasn't a double");
                    nextStandardRot[side] += new Vector3(0, (90 * multiplier[side]) + 180, 0);
                    nextStandardPos[side] = newDomino.transform.localPosition + new Vector3(dominoHalfSize[1] * tempMult, 0, 0.415f * multiplier[side]);
                    nextDoublePos[side] = newDomino.transform.localPosition + new Vector3((dominoHalfSize[0] + dominoHalfSize[1]) * tempMult, 0, 0);
                    specCirc2[side] = true;
                }
                

            }
            else
            {
                nextStandardPos[side] = newDomino.transform.localPosition;
                nextDoublePos[side] = newDomino.transform.localPosition;

                if (specCirc1[side] && prevDomWasDouble[side])
                {
                    nextDoubleRot[side] += new Vector3(0, 90, 0);
                    nextStandardPos[side].x += multiplier[side] * 0.15f;
                    specCirc1[side] = false;
                }
                else if (specCirc2[side] && prevDomWasDouble[side])
                {
                    nextDoubleRot[side] += new Vector3(0, 90, 0);
                    nextStandardPos[side].z += multiplier[side] * 0.15f;
                    specCirc2[side] = false;
                }
                else if (specCirc1[side])
                {
                    if (isDouble)
                    {
                        nextDoubleRot[side] += new Vector3(0, 90, 0);
                        nextStandardPos[side].x += multiplier[side] * 0.15f;
                        specCirc1[side] = false;
                    }
                    else
                    {
                        nextDoubleRot[side] += new Vector3(0, 90, 0);
                        specCirc1[side] = false;
                    }
                    
                }
                else if (specCirc2[side])
                {
                    if (isDouble)
                    {
                        nextDoubleRot[side] += new Vector3(0, 90, 0);
                        nextStandardPos[side].z += multiplier[side] * 0.15f;
                        specCirc2[side] = false;
                    }
                    else
                    {
                        nextDoubleRot[side] += new Vector3(0, 90, 0);
                        specCirc2[side] = false;
                    }
                    
                }


                if (!isDouble)
                {
                    if (curSection[side] % 2 == 0)
                    {
                        nextStandardPos[side] += (new Vector3(0, 0, dominoHalfSize[0]) * 2) * multiplier[side];
                        nextDoublePos[side] += new Vector3(0, 0, dominoHalfSize[0] + dominoHalfSize[1]) * multiplier[side];
                    }
                    else
                    {
                        nextStandardPos[side] += (new Vector3(dominoHalfSize[0], 0, 0) * 2) * multiplier[side];
                        nextDoublePos[side] += new Vector3(dominoHalfSize[0] + dominoHalfSize[1], 0, 0) * multiplier[side];
                    }

                    spaceOnTable[side] += (dominoHalfSize[0] * 2);

                }
                else
                {
                    if (curSection[side] % 2 == 0)
                    {
                        nextStandardPos[side] += new Vector3(0, 0, dominoHalfSize[0] + dominoHalfSize[1]) * multiplier[side];
                        spaceOnTable[side] += (dominoHalfSize[1] * 2);
                    }
                    else
                    {
                        nextStandardPos[side] += new Vector3(dominoHalfSize[0] + dominoHalfSize[1], 0, 0) * multiplier[side];
                        spaceOnTable[side] += (dominoHalfSize[1] * 2);
                    }

                }
            }



            
        }
        else
        {
            if (!isDouble)
            {
                nextStandardPos[0] = newDomino.transform.localPosition + (new Vector3(0, 0, dominoHalfSize[0]) * 2);
                nextStandardPos[1] = newDomino.transform.localPosition - (new Vector3(0, 0, dominoHalfSize[0]) * 2);
                nextDoublePos[0] = newDomino.transform.localPosition + (new Vector3(0, 0, dominoHalfSize[0]) + new Vector3(0, 0, dominoHalfSize[1]));
                nextDoublePos[1] = newDomino.transform.localPosition - (new Vector3(0, 0, dominoHalfSize[0]) + new Vector3(0, 0, dominoHalfSize[1]));
                spaceOnTable[1] += dominoHalfSize[0];
                spaceOnTable[0] += dominoHalfSize[0];
            }
            else
            {
                nextStandardPos[0] = newDomino.transform.localPosition + (new Vector3(0, 0, dominoHalfSize[0]) + new Vector3(0, 0, dominoHalfSize[1]));
                nextStandardPos[1] = newDomino.transform.localPosition - (new Vector3(0, 0, dominoHalfSize[0]) + new Vector3(0, 0, dominoHalfSize[1]));
                spaceOnTable[1] += dominoHalfSize[1];
                spaceOnTable[0] += dominoHalfSize[1];
            }
        }

        yield return new WaitForFixedUpdate();
    }

    // Tester Functions




}
