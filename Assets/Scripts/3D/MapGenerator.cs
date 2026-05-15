using System.Collections.Generic;
using UnityEngine;

public class MapGenerator2D : MonoBehaviour
{
    [Header("Generate")]
    public GameObject groundPrefab;
    public GameObject waterPrefab;
    public GameObject treePrefab;
    public GameObject grassPrefab;
    public bool[,,] isTaken;
    public int worldLength, worldWidth, worldHeight;
    private List<int> waterSeed;
    public int waterAmount = 4;
    public int waterConnectChance;
    public int waterChance = 30;
    public int maxWaterTilesPerLake = 120;
    public int minWaterAmount = 2;
    public int treeChance;
    public int grassChance;

    [Header("Map Data")]
    public int treeAmount;
    public int grassAmount;
    public int newGrassGrowInterval = 10;
    public int oldGrassGrowInterval = 20;
    public int dryGrassGrowInterval = 30;
    public List<GrassEntity> grassEntities;
    private int growCounter = 1;
    private float lastGrowTimer = 0;

    private readonly Queue<Vector3Int> waterFrontier = new Queue<Vector3Int>();
    private readonly HashSet<Vector3Int> queuedWaterCells = new HashSet<Vector3Int>();

    private int groundLayer;
    private int waterLayer;
    private int treeLayer;
    private int grassLayer;

    private void Awake()
    {
        isTaken = new bool[worldLength, worldHeight, worldWidth];
        waterSeed = new List<int>(Mathf.Max(0, waterAmount));

        groundLayer = LayerMask.NameToLayer("Ground");
        waterLayer = LayerMask.NameToLayer("Water");
        treeLayer = LayerMask.NameToLayer("Tree");
        grassLayer = LayerMask.NameToLayer("Grass");

        if (worldLength <= 0 || waterAmount <= 0)
        {
            return;
        }

        int minimumLakeCount = Mathf.Clamp(minWaterAmount, 0, waterAmount);
        for (int i = 0; i < minimumLakeCount; i++)
        {
            waterSeed.Add(Random.Range(0, worldLength));
        }

        while (waterSeed.Count < waterAmount && Random.Range(0, 100) <= waterChance)
        {
            waterSeed.Add(Random.Range(0, worldLength));
        }
    }

    private void Start()
    {
        if (worldWidth <= 0 || worldHeight <= 0)
        {
            return;
        }

        int waterSeedCount = waterSeed.Count;
        for (int i = 0; i < waterSeedCount; i++)
        {
            int waterZ = Random.Range(0, worldWidth);
            GenerateWaterLake(waterSeed[i], 0, waterZ);
        }

        for (int x = 0; x < worldLength; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                for (int z = 0; z < worldWidth; z++)
                {
                    GenerateGround(x, y, z);
                }
            }
        }
    }

    private void Update()
    {
        GrassGrowManagement();


    }
    private void GrassGrowManagement()
    {
        float interval = 0f;

        if (growCounter == 1)
            interval = newGrassGrowInterval;
        else if (growCounter == 2)
            interval = oldGrassGrowInterval;
        else if (growCounter == 3)
            interval = dryGrassGrowInterval;

        if (Time.time - lastGrowTimer < interval)
            return;

        AllGrassGrow();

        growCounter++;
        if (growCounter > 3)
            growCounter = 1;

        lastGrowTimer = Time.time;
    }
    private void AllGrassGrow()
    {
        for (int i = 0; i < grassEntities.Count; i++)
            grassEntities[i].GrassGrow();
    }
    private void GenerateGround(int x, int y, int z)
    {
        if (isTaken[x, y, z] || groundPrefab == null)
        {
            return;
        }

        GameObject ground = Instantiate(groundPrefab, new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), Quaternion.identity, transform);
        ground.name = groundPrefab.name;
        ground.tag = "Ground";
        ground.layer = groundLayer;
        isTaken[x, y, z] = true;

        int tChance = Random.Range(0, 100);
        int gChance = Random.Range(0, 100);
        if (tChance < treeChance)
        {
            GenerateTree(x, y + 1, z);
        }
        else if (gChance < grassChance)
        {
            GenerateGrass(x, y + 1, z);
        }
    }

    private void GenerateGrass(int x, int y, int z)
    {
        if (grassPrefab == null)
        {
            return;
        }

        GameObject grass = Instantiate(grassPrefab, new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), Quaternion.identity, transform);
        grass.name = grassPrefab.name;
        grass.tag = "Grass";
        grass.layer = grassLayer;
        grassAmount++;
        GrassEntity grassEntity = grass.GetComponent<GrassEntity>();
        grassEntities.Add(grassEntity);
        isTaken[x, y, z] = true;
    }

    private void GenerateTree(int x, int y, int z)
    {
        if (treePrefab == null)
        {
            return;
        }

        GameObject tree = Instantiate(treePrefab, new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), Quaternion.identity, transform);
        tree.name = treePrefab.name;
        tree.tag = "Tree";
        tree.layer = treeLayer;
        treeAmount++;
        isTaken[x, y, z] = true;
    }

    private void GenerateWaterLake(int startX, int startY, int startZ)
    {
        if (!IsInBounds(startX, startY, startZ) || isTaken[startX, startY, startZ] || waterPrefab == null)
        {
            return;
        }

        int lakeTileLimit = Mathf.Max(1, maxWaterTilesPerLake);
        waterFrontier.Clear();
        queuedWaterCells.Clear();

        Vector3Int startCell = new Vector3Int(startX, startY, startZ);
        waterFrontier.Enqueue(startCell);
        queuedWaterCells.Add(startCell);

        int generatedTiles = 0;
        while (waterFrontier.Count > 0 && generatedTiles < lakeTileLimit)
        {
            Vector3Int cell = waterFrontier.Dequeue();
            queuedWaterCells.Remove(cell);

            int x = cell.x;
            int y = cell.y;
            int z = cell.z;

            if (!IsInBounds(x, y, z) || isTaken[x, y, z])
            {
                continue;
            }

            GameObject water = Instantiate(waterPrefab, new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), Quaternion.identity, transform);
            water.name = waterPrefab.name;
            water.tag = "Water";
            water.layer = waterLayer;

            isTaken[x, y, z] = true;
            generatedTiles++;

            TryQueueConnectedWater(x + 1, y, z);
            TryQueueConnectedWater(x - 1, y, z);
            TryQueueConnectedWater(x, y, z + 1);
            TryQueueConnectedWater(x, y, z - 1);
        }
    }

    private void TryQueueConnectedWater(int x, int y, int z)
    {
        Vector3Int cell = new Vector3Int(x, y, z);
        if (!IsInBounds(x, y, z) || isTaken[x, y, z] || queuedWaterCells.Contains(cell))
        {
            return;
        }

        if (Random.Range(0, 100) <= waterConnectChance)
        {
            waterFrontier.Enqueue(cell);
            queuedWaterCells.Add(cell);
        }
    }

    private bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x < worldLength
            && y >= 0 && y < worldHeight
            && z >= 0 && z < worldWidth;
    }
}
