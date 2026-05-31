using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpeciesPopulationSetup
{
    public StatSetup statSetup;
    [Min(0)] public int spawnCount = 5;
}

[CreateAssetMenu(menuName = "PandoraData/Ecosystem Setup", fileName = "Ecosystem Setup")]
public class EcosystemSetup : ScriptableObject
{
    [Header("Identity")]
    public string ecosystemName = "Default Ecosystem";

    [Header("Map")]
    [Min(1)] public int width = 100;
    [Min(1)] public int height = 100;
    [Range(0f, 100f)] public float grassChance = 20f;
    [Range(0f, 100f)] public float meatChance = 10f;

    [Header("Species Population")]
    public List<SpeciesPopulationSetup> speciesPopulations = new List<SpeciesPopulationSetup>();

    public List<AnimalSpawnEntry> BuildSpawnEntries()
    {
        List<AnimalSpawnEntry> spawnEntries = new List<AnimalSpawnEntry>();
        if (speciesPopulations == null)
        {
            return spawnEntries;
        }

        for (int i = 0; i < speciesPopulations.Count; i++)
        {
            SpeciesPopulationSetup population = speciesPopulations[i];
            if (population == null || population.statSetup == null || population.statSetup.animalPrefab == null || population.spawnCount <= 0)
            {
                continue;
            }

            spawnEntries.Add(new AnimalSpawnEntry
            {
                animalPrefab = population.statSetup.animalPrefab,
                statSetup = population.statSetup,
                spawnCount = population.spawnCount
            });
        }

        return spawnEntries;
    }

    public int GetTotalSpawnCount()
    {
        int total = 0;
        if (speciesPopulations == null)
        {
            return total;
        }

        for (int i = 0; i < speciesPopulations.Count; i++)
        {
            SpeciesPopulationSetup population = speciesPopulations[i];
            if (population == null)
            {
                continue;
            }

            total += Mathf.Max(0, population.spawnCount);
        }

        return total;
    }

    private void OnValidate()
    {
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        grassChance = Mathf.Clamp(grassChance, 0f, 100f);
        meatChance = Mathf.Clamp(meatChance, 0f, 100f);

        if (speciesPopulations == null)
        {
            return;
        }

        for (int i = 0; i < speciesPopulations.Count; i++)
        {
            SpeciesPopulationSetup population = speciesPopulations[i];
            if (population != null)
            {
                population.spawnCount = Mathf.Max(0, population.spawnCount);
            }
        }
    }
}
