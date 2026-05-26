using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimalSpawnEntry
{
    public AnimalBase animalPrefab;
    public StatSetup statSetup;
    public int spawnCount = 5;
}

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
    public float grassGenerateInterval = 10f;
    public int grassGenerateAmount = 5;
    public List<MeatBase> meatAmount;
    public float meatResolveTimer = 10f;

    [Header("Animals")]
    public List<AnimalSpawnEntry> animalSpawnEntries;
    public List<AnimalBase> animalAmount;

    private void Start()
    {
        if (grassAmount == null)
            grassAmount = new List<GrassBase>();
        if (meatAmount == null)
            meatAmount = new List<MeatBase>();
        if (animalAmount == null)
            animalAmount = new List<AnimalBase>();

        isTaken = new bool[width, height];

        GenerateTerrain();
        GenerateAnimals();
        grassGenerateTimer = grassGenerateInterval;
    }
    private void Update()
    {
        grassGenerateTimer -= Time.deltaTime;
        if (grassGenerateTimer <= 0)
        {
            int i = 0;
            while (i < grassGenerateAmount)
            {
                int x = Random.Range(0, width - 1);
                int y = Random.Range(0, height - 1);
                if (isTaken[x, y] == false)
                {
                    GenerateGrass(x,y);
                    i++;
                }
            }
            grassGenerateTimer = grassGenerateInterval;
        }
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

        //if (tChance <= treeChance && isTaken[x, y] == false)
        //    GenerateTree(x, y);

        //if (isTaken[x, y] == false)
        //    GenerateGround(x, y);

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
            case 1: m.meatMaxAmount = 10; break;
            case 2: m.meatMaxAmount = 20; break;
            case 3: m.meatMaxAmount = 30; break;
        }
        m.InitMeatState();

        meatAmount.Add(m);
    }

    private void GenerateAnimals()
    {
        if (animalSpawnEntries == null)
        {
            return;
        }

        for (int i = 0; i < animalSpawnEntries.Count; i++)
        {
            AnimalSpawnEntry entry = animalSpawnEntries[i];
            if (entry == null || entry.animalPrefab == null || entry.statSetup == null || entry.spawnCount <= 0)
            {
                continue;
            }

            for (int count = 0; count < entry.spawnCount; count++)
            {
                TrySpawnAnimal(entry);
            }
        }
    }

    private bool TrySpawnAnimal(AnimalSpawnEntry entry)
    {
        const int maxAttempts = 200;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            if (isTaken[x, y])
            {
                continue;
            }

            Vector3 spawnPosition = new Vector3(x + 0.5f, y + 0.5f, 0f);
            AnimalBase animal = Instantiate(entry.animalPrefab, spawnPosition, entry.animalPrefab.transform.rotation, transform);
            animal.InitializeFromSetup(entry.statSetup);
            animalAmount.Add(animal);
            isTaken[x, y] = true;
            return true;
        }

        return false;
    }
}
