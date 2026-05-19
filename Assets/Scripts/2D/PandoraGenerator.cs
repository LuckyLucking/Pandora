using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PandoraGenerator : MonoBehaviour
{
    public bool[,] isTaken;

    [Header("Blocks")]
    public int width = 100;
    public int height = 100;
    public GameObject groundSp;
    public GameObject grassSp;
    public GameObject meatSp;
    public GameObject treeSp;

    [Header("GenerateChance")]
    public float treeChance = 10;
    public float grassChance = 20;
    public float meatChance = 10;

    [Header("Resources")]
    public List<GrassBase> grassAmount;
    public float grassGenerateTimer = 10f;
    public List<MeatBase> meatAmount;
    public float meatResolveTimer = 10f;

    private void Start()
    {
        isTaken = new bool[width, height];

        GenerateTerrain();
    }
    private void GenerateTerrain()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                InitMap(x, y);
            }
        }
    }
    private void InitMap(int x,int y)
    {
        GenerateBorder(x, y);
        int tChance = Random.Range(0, 100);

        if (tChance <= treeChance && isTaken[x, y] == false)
            GenerateTree(x, y);

        if (isTaken[x, y] == false)
            GenerateGround(x, y);

        int gChance = Random.Range(0, 100);

        if (gChance <= grassChance && isTaken[x, y] == false)
            GenerateGrass(x, y);

        int mChance = Random.Range(0, 100);
        if(mChance <= meatChance && isTaken[x, y] == false)
            GenerateMeat(x, y);
    }
    private void GenerateBorder(int x, int y)
    {
        if(x == 0)
            Instantiate(treeSp, new Vector3(x -0.5f,y+0.5f ), treeSp.transform.rotation, transform);
        if (x==width -1)
            Instantiate(treeSp, new Vector3(x + 1.5f,y+0.5f), treeSp.transform.rotation, transform);
        if (y == 0)
            Instantiate(treeSp, new Vector3(x+0.5f,y -0.5f), treeSp.transform.rotation, transform);
        if (y==height -1)
            Instantiate(treeSp, new Vector3(x +0.5f,y + 1.5f), treeSp.transform.rotation, transform);

    }
    private void GenerateGround(int x, int y)
    {
        GameObject ground = Instantiate(groundSp, new Vector3(x + 0.5f,  y + 0.5f), groundSp.transform.rotation, transform);

    }
    private void GenerateTree(int x, int y)
    {
        GameObject tree = Instantiate(treeSp,new Vector3(x +0.5f,y+0.5f),treeSp.transform.rotation,transform);
        isTaken[x, y] = true;
    }
    private void GenerateGrass(int x, int y)
    {
        GameObject grass = Instantiate(grassSp, new Vector3(x + 0.5f, y + 0.5f), grassSp.transform.rotation, transform);
        isTaken[x, y] = true;
        GrassBase g = grass.GetComponent<GrassBase>();
        grassAmount.Add(g);
    }   
    private void GenerateMeat(int x, int y)
    {
        GameObject meat = Instantiate(meatSp, new Vector3(x + 0.5f, y + 0.5f), meatSp.transform.rotation, transform);
        isTaken[x, y] = true;
        MeatBase m = meat.GetComponent<MeatBase>();
        int size = Random.Range(1, 4);
        switch (size)
        {
            case 1: m.meatCurrentAmount = 10; m.ChangeSize(MeatState.Small); break;
            case 2: m.meatCurrentAmount = 20; m.ChangeSize(MeatState.Medium); break;
            case 3: m.meatCurrentAmount = 30; m.ChangeSize(MeatState.Masive); break;
        }

        meatAmount.Add(m);
    }
}
