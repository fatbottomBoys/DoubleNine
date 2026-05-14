using JetBrains.Annotations;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DominoField : MonoBehaviour
{

    public GameManager gM;

    [Header("Domnio prefab")]
    public GameObject dominoPrefab;
    public Texture2D[] dominoTextures;

    [Header("Current Field Stats")]
    [SerializeField] private int numDominoesPlayed;
    [SerializeField] private Vector3[] spaceOnTable;        // 0 is pos z, 1 is neg z
    [SerializeField] private Vector3[] nextStandardPos;     // "
    [SerializeField] private Vector3[] nextStandardRot;     // "
    [SerializeField] private Vector3[] nextDoublePos;       // "
    [SerializeField] private Vector3[] nextDoubleRot;       // "
    public int[] currentValuesOnField;                      // "

    [SerializeField] private Vector2 tableSize;

    [Header("Test Stuff")]
    [SerializeField] private Vector3 startDom;
    [SerializeField] private Vector3 conDom;

    public IEnumerator PlayDomino(Vector3 domino, int side)
    {
        numDominoesPlayed++;

        bool isDouble = false;
        int newValue = 0;
        int prevValueOnField = 0;

        if (numDominoesPlayed == 1)
        {
            prevValueOnField = Mathf.RoundToInt(domino.x);
        }
        else
        {
            prevValueOnField = currentValuesOnField[side];
        }

        if(domino.x == domino.y)
        {
            isDouble = true;
        }
        else
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
        if(prevValueOnField <= newValue)
        {
            newDomValue.x = prevValueOnField;
            newDomValue.y = newValue;
        }
        else
        {
            newDomValue.x = newValue;
            newDomValue.y = prevValueOnField;
        }
        goStats.myValue = newDomValue;

        if (isDouble)
        {
            myMat[1].mainTexture = dominoTextures[prevValueOnField];
            myMat[2].mainTexture = dominoTextures[prevValueOnField];
        }
        else
        {
            myMat[1].mainTexture = dominoTextures[prevValueOnField];
            myMat[2].mainTexture = dominoTextures[newValue];
        }




        yield return new WaitForFixedUpdate();
    }

    // Tester Functions




}
