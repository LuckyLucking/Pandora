using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PandoraGenerator : MonoBehaviour
{
    public int[,] pandoraMap;
    public bool[,] isTaken;

    [Header("Blocks")]
    public int width = 100;
    public int height = 100;
    public GameObject groundSp;
    public GameObject grassSp;
    public GameObject meatSp;
    public GameObject treeSp;

}
