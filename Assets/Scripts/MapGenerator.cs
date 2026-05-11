using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Generate")]
    public GameObject groundPrefab;
    public GameObject waterPrefab;
    public GameObject treePrefab;
    public bool[,,] isTaken;
    public int worldLength, worldWidth,worldHeight;
    private List<int> waterSeed;
    public int waterAmount=4;
    public int waterConnectChance;
    public int waterChance = 30;
    public int treeChance; 
    public int grassChance;

    [Header("Map Data")]
    public int treeAmount;
    public int grassAmount;

    private void Awake()
    {
        isTaken = new bool[worldLength, worldHeight, worldWidth];
        for (int x = 0; x < worldLength; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                for (int z = 0; z < worldWidth; z++)
                {
                    isTaken[x, y, z] = false;
                }
            }
        }
        int r = Random.Range(0, 100);
        waterSeed = new List<int>();
        int s = 1;
        waterSeed[0] = Random.Range(0, worldLength);
        while (r <= waterChance && s<= waterAmount-1)
        {
            waterSeed[s] = Random.Range(0, worldLength);
            s++;
            r = Random.Range(0, 100);
        }
    }

    private void Start()
    {
        for (int x = 0; x < worldLength; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                for (int z = 0; z < worldWidth; z++)
                {
                    //water
                    for (int s = 0; s < waterAmount; s++)
                    {
                        //if (waterSeed[s] == null)
                        //    break;
                        if (isTaken[x, y, z] == false)
                        {

                        }
                    }
                    
                }
            }

        }
    }

    //水平面是x轴和z轴
    private void GenerateWater(int x, int y, int z)
    {
        if (!IsInBounds(x, y, z) || isTaken[x, y, z] || waterPrefab == null)
        {
            return;
        }

        GameObject water = Instantiate(waterPrefab, new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), Quaternion.identity, transform);
        water.name = waterPrefab.name;
        isTaken[x, y, z] = true;
        int t = Random.Range(0, 99);
        if(t <= waterConnectChance && isTaken[x+1,y,z]==false)
            GenerateWater(x+1, y, z);
        t = Random.Range(0, 99);
        if (t <= waterConnectChance && isTaken[x - 1, y, z] == false)
            GenerateWater(x - 1, y, z);
        t = Random.Range(0, 99);
        if (t <= waterConnectChance && isTaken[x, y, z + 1] == false)
            GenerateWater(x, y, z+1);
        t = Random.Range(0, 99);
        if (t <= waterConnectChance && isTaken[x, y, z - 1] == false)
            GenerateWater(x, y, z-1);
    }

    private bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x < worldLength
            && y >= 0 && y < worldHeight
            && z >= 0 && z < worldWidth;
    }
}

