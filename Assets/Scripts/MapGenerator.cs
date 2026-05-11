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
    public int maxWaterTilesPerLake = 120;
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
        waterSeed = new List<int>();

        if (worldLength <= 0 || waterAmount <= 0)
        {
            return;
        }

        int r = Random.Range(0, 100);
        int s = 1;
        int seed = Random.Range(0, worldLength);
        waterSeed.Add(seed);
        while (r <= waterChance && s <= waterAmount - 1)
        {
            seed = Random.Range(0, worldLength);
            waterSeed.Add(seed);
            s++;
            r = Random.Range(0, 100);
        }
    }

    private void Start()
    {
        if (worldWidth <= 0 || worldHeight <= 0)
        {
            return;
        }  

        for (int s = 0; s < waterSeed.Count; s++)
        {
            int w = Random.Range(0, worldWidth);
            GenerateWaterLake(waterSeed[s], 0, w);
        }
    }

    //水平面是x轴和z轴
    private void GenerateWaterLake(int startX, int startY, int startZ)
    {
        if (!IsInBounds(startX, startY, startZ) || isTaken[startX, startY, startZ] || waterPrefab == null)
        {
            return;
        }

        int lakeTileLimit = Mathf.Max(1, maxWaterTilesPerLake);
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        HashSet<Vector3Int> queuedCells = new HashSet<Vector3Int>();
        Vector3Int startCell = new Vector3Int(startX, startY, startZ);
        frontier.Enqueue(startCell);
        queuedCells.Add(startCell);

        int generatedTiles = 0;
        while (frontier.Count > 0 && generatedTiles < lakeTileLimit)
        {
            Vector3Int cell = frontier.Dequeue();
            queuedCells.Remove(cell);
            int x = cell.x;
            int y = cell.y;
            int z = cell.z;

            if (!IsInBounds(x, y, z) || isTaken[x, y, z])
            {
                continue;
            }

            GameObject water = Instantiate(waterPrefab, new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), Quaternion.identity, transform);
            water.name = waterPrefab.name;
            isTaken[x, y, z] = true;
            generatedTiles++;

            TryQueueConnectedWater(frontier, queuedCells, x + 1, y, z);
            TryQueueConnectedWater(frontier, queuedCells, x - 1, y, z);
            TryQueueConnectedWater(frontier, queuedCells, x, y, z + 1);
            TryQueueConnectedWater(frontier, queuedCells, x, y, z - 1);
        }
    }

    private void TryQueueConnectedWater(Queue<Vector3Int> frontier, HashSet<Vector3Int> queuedCells, int x, int y, int z)
    {
        Vector3Int cell = new Vector3Int(x, y, z);
        if (!IsInBounds(x, y, z) || isTaken[x, y, z] || queuedCells.Contains(cell))
        {
            return;
        }

        int t = Random.Range(0, 100);
        if (t <= waterConnectChance)
        {
            frontier.Enqueue(cell);
            queuedCells.Add(cell);
        }
    }

    private bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x < worldLength
            && y >= 0 && y < worldHeight
            && z >= 0 && z < worldWidth;
    }
}

