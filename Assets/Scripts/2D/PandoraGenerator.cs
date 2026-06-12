using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimalSpawnEntry
{
    public AnimalBase animalPrefab;
    public StatSetup statSetup;
    public int spawnCount = 5;
    public int currentCount;
}

public class PandoraGenerator : MonoBehaviour
{
    private static PandoraGenerator instance;

    public bool[,] isTaken;

    [Header("Ecosystem")]
    public EcosystemSetup ecosystemSetup;

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
    [SerializeField] private List<AnimalSpawnEntry> runtimeAnimalSpawnEntries;

    public static PandoraGenerator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PandoraGenerator>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;

        EcosystemSetup selectedEcosystem = EcosystemLoadContext.ConsumeSelectedEcosystem();
        if (selectedEcosystem != null)
        {
            ecosystemSetup = selectedEcosystem;
        }

        if (animalAmount == null)
        {
            animalAmount = new List<AnimalBase>();
        }

        if (runtimeAnimalSpawnEntries == null)
        {
            runtimeAnimalSpawnEntries = new List<AnimalSpawnEntry>();
        }
    }

    private void Start()
    {
        if (grassAmount == null)
            grassAmount = new List<GrassBase>();
        if (meatAmount == null)
            meatAmount = new List<MeatBase>();

        ApplyEcosystemSetup(ecosystemSetup);
        if (ecosystemSetup == null)
        {
            runtimeAnimalSpawnEntries = CloneSpawnEntries(animalSpawnEntries);
        }

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

        int gChance = Random.Range(0, 1000);

        if (gChance <= grassChance*10 && isTaken[x, y] == false)
            GenerateGrass(x, y);

        int mChance = Random.Range(0, 1000);
        if(mChance <= meatChance*10 && isTaken[x, y] == false)
            GenerateMeat(x, y);
    }
    private void GenerateBorder(int x, int y)
    {
        if (x == 0)
        {
            Instantiate(treeSp, new Vector3(x -0.5f,y+0.5f ), treeSp.transform.rotation, transform);
            Instantiate(treeSp, new Vector3(x -1.5f,y+0.5f ), treeSp.transform.rotation, transform);
        }
        if (x == width - 1)
        {
            Instantiate(treeSp, new Vector3(x + 1.5f,y+0.5f), treeSp.transform.rotation, transform);
            Instantiate(treeSp, new Vector3(x + 2.5f,y+0.5f), treeSp.transform.rotation, transform);
        }
        if (y == 0)
        {
            Instantiate(treeSp, new Vector3(x+0.5f,y -0.5f), treeSp.transform.rotation, transform);
            Instantiate(treeSp, new Vector3(x+0.5f,y -1.5f), treeSp.transform.rotation, transform);
        }
        if (y == height - 1)
        {
            Instantiate(treeSp, new Vector3(x +0.5f,y + 1.5f), treeSp.transform.rotation, transform);
            Instantiate(treeSp, new Vector3(x +0.5f,y + 2.5f), treeSp.transform.rotation, transform);
        }

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
        List<AnimalSpawnEntry> activeSpawnEntries = GetActiveSpawnEntries();
        if (activeSpawnEntries == null)
        {
            return;
        }

        for (int i = 0; i < activeSpawnEntries.Count; i++)
        {
            AnimalSpawnEntry entry = activeSpawnEntries[i];
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
            animal.InitializeFromSetup(entry.statSetup, entry, entry.animalPrefab);
            isTaken[x, y] = true;
            return true;
        }

        return false;
    }

    private void RebuildAnimalTracking()
    {
        if (animalAmount == null)
        {
            animalAmount = new List<AnimalBase>();
        }

        animalAmount.Clear();

        if (runtimeAnimalSpawnEntries != null)
        {
            for (int i = 0; i < runtimeAnimalSpawnEntries.Count; i++)
            {
                AnimalSpawnEntry entry = runtimeAnimalSpawnEntries[i];
                if (entry != null)
                {
                    entry.currentCount = 0;
                }
            }
        }

        AnimalBase[] animals = FindObjectsOfType<AnimalBase>();
        for (int i = 0; i < animals.Length; i++)
        {
            AnimalBase animal = animals[i];
            if (animal == null || animal.IsDead || !animal.gameObject.activeInHierarchy)
            {
                continue;
            }

            RegisterAnimal(animal);
        }
    }

    public void ApplyEcosystemSetup(EcosystemSetup setup)
    {
        ecosystemSetup = setup;
        if (setup == null)
        {
            runtimeAnimalSpawnEntries = new List<AnimalSpawnEntry>();
            return;
        }

        width = setup.width;
        height = setup.height;
        grassChance = setup.grassChance;
        meatChance = setup.meatChance;
        runtimeAnimalSpawnEntries = setup.BuildSpawnEntries();
    }

    private List<AnimalSpawnEntry> GetActiveSpawnEntries()
    {
        if (ecosystemSetup != null)
        {
            return runtimeAnimalSpawnEntries;
        }

        return animalSpawnEntries;
    }

    private List<AnimalSpawnEntry> CloneSpawnEntries(List<AnimalSpawnEntry> source)
    {
        List<AnimalSpawnEntry> clonedEntries = new List<AnimalSpawnEntry>();
        if (source == null)
        {
            return clonedEntries;
        }

        for (int i = 0; i < source.Count; i++)
        {
            AnimalSpawnEntry entry = source[i];
            if (entry == null)
            {
                continue;
            }

            clonedEntries.Add(new AnimalSpawnEntry
            {
                animalPrefab = entry.animalPrefab,
                statSetup = entry.statSetup,
                spawnCount = entry.spawnCount
            });
        }

        return clonedEntries;
    }

    public AnimalSpawnEntry RegisterAnimal(AnimalBase animal, AnimalSpawnEntry preferredEntry = null)
    {
        if (animal == null || animal.Setup == null)
        {
            return null;
        }

        if (animalAmount == null)
        {
            animalAmount = new List<AnimalBase>();
        }

        bool added = false;
        if (!animalAmount.Contains(animal))
        {
            animalAmount.Add(animal);
            added = true;
        }

        if (!added)
        {
            return FindRuntimeSpawnEntry(animal, preferredEntry);
        }

        AnimalSpawnEntry runtimeEntry = GetOrCreateRuntimeSpawnEntry(animal, preferredEntry);
        if (runtimeEntry != null)
        {
            runtimeEntry.currentCount = Mathf.Max(0, runtimeEntry.currentCount + 1);
        }

        return runtimeEntry;
    } 

    public void UnregisterAnimal(AnimalBase animal, AnimalSpawnEntry preferredEntry = null)
    {
        if (animalAmount == null || animal == null)
        {
            return;
        }

        bool removed = animalAmount.Remove(animal);
        if (!removed)
        {
            return;
        }

        AnimalSpawnEntry runtimeEntry = FindRuntimeSpawnEntry(animal, preferredEntry);
        if (runtimeEntry != null)
        {
            runtimeEntry.currentCount = Mathf.Max(0, runtimeEntry.currentCount - 1);
        }
    }

    private AnimalSpawnEntry GetOrCreateRuntimeSpawnEntry(AnimalBase animal, AnimalSpawnEntry preferredEntry = null)
    {
        AnimalSpawnEntry entry = FindRuntimeSpawnEntry(animal, preferredEntry);
        if (entry != null)
        {
            return entry;
        }

        StatSetup setup = animal.Setup;
        if (setup == null)
        {
            return null;
        }

        entry = new AnimalSpawnEntry
        {
            animalPrefab = setup.animalPrefab,
            statSetup = setup,
            spawnCount = 0,
            currentCount = 0
        };

        if (runtimeAnimalSpawnEntries == null)
        {
            runtimeAnimalSpawnEntries = new List<AnimalSpawnEntry>();
        }

        runtimeAnimalSpawnEntries.Add(entry);

        return entry;
    }

    private AnimalSpawnEntry FindRuntimeSpawnEntry(AnimalBase animal, AnimalSpawnEntry preferredEntry = null)
    {
        if (animal == null || runtimeAnimalSpawnEntries == null)
        {
            return null;
        }

        if (preferredEntry != null)
        {
            for (int i = 0; i < runtimeAnimalSpawnEntries.Count; i++)
            {
                if (ReferenceEquals(runtimeAnimalSpawnEntries[i], preferredEntry))
                {
                    return runtimeAnimalSpawnEntries[i];
                }
            }

            if (preferredEntry.statSetup != null)
            {
                for (int i = 0; i < runtimeAnimalSpawnEntries.Count; i++)
                {
                    AnimalSpawnEntry entry = runtimeAnimalSpawnEntries[i];
                    if (entry != null && ReferenceEquals(entry.statSetup, preferredEntry.statSetup))
                    {
                        return entry;
                    }
                }
            }
        }

        StatSetup setup = animal.Setup;
        if (setup != null)
        {
            for (int i = 0; i < runtimeAnimalSpawnEntries.Count; i++)
            {
                AnimalSpawnEntry entry = runtimeAnimalSpawnEntries[i];
                if (entry != null && ReferenceEquals(entry.statSetup, setup))
                {
                    return entry;
                }
            }
        }

        int speciesID = animal.SpeciesID;
        for (int i = 0; i < runtimeAnimalSpawnEntries.Count; i++)
        {
            AnimalSpawnEntry entry = runtimeAnimalSpawnEntries[i];
            if (entry == null || entry.statSetup == null)
            {
                continue;
            }

            if (entry.statSetup.speciesID == speciesID)
            {
                return entry;
            }
        }

        return null;
    }
}
