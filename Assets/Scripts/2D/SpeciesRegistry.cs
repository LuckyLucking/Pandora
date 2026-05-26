using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SpeciesRegistry : MonoBehaviour
{
    private static SpeciesRegistry instance;

    [SerializeField] private List<StatSetup> registeredSpecies = new List<StatSetup>();
    [SerializeField] private int nextSpeciesID = 1;

    public static SpeciesRegistry Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SpeciesRegistry>();
                if (instance == null)
                {
                    GameObject registryObject = new GameObject("SpeciesRegistry");
                    instance = registryObject.AddComponent<SpeciesRegistry>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        RebuildNextSpeciesID();
    }

    public void RegisterSpecies(StatSetup setup)
    {
        if (setup == null)
        {
            return;
        }

        if (!registeredSpecies.Contains(setup))
        {
            registeredSpecies.Add(setup);
        }

        if (setup.speciesID <= 0)
        {
            setup.speciesID = nextSpeciesID;
            nextSpeciesID++;
        }
        else
        {
            nextSpeciesID = Mathf.Max(nextSpeciesID, setup.speciesID + 1);
        }
    }

    public StatSetup ResolveSpecies(StatSetup parentSpecies, AnimalGeneSnapshot genes)
    {
        if (parentSpecies == null)
        {
            return null;
        }

        RegisterSpecies(parentSpecies);

        float parentDifference = parentSpecies.EvaluateDifferenceScore(genes);
        if (parentDifference <= parentSpecies.evolutionThreshold)
        {
            return parentSpecies;
        }

        StatSetup bestMatch = null;
        float bestMatchScore = float.MaxValue;

        for (int i = 0; i < registeredSpecies.Count; i++)
        {
            StatSetup candidate = registeredSpecies[i];
            if (candidate == null)
            {
                continue;
            }

            float score = candidate.EvaluateDifferenceScore(genes);
            if (score < bestMatchScore)
            {
                bestMatchScore = score;
                bestMatch = candidate;
            }
        }

        if (bestMatch != null && bestMatchScore <= bestMatch.evolutionThreshold)
        {
            return bestMatch;
        }

        if (bestMatch != null && bestMatch != parentSpecies && bestMatchScore < parentDifference)
        {
            return bestMatch;
        }

        return CreateNewSpecies(parentSpecies, genes);
    }

    private StatSetup CreateNewSpecies(StatSetup parentSpecies, AnimalGeneSnapshot genes)
    {
        StatSetup newSpecies = ScriptableObject.CreateInstance<StatSetup>();
        newSpecies.CopyNonGeneSettingsFrom(parentSpecies);
        newSpecies.ApplyGenes(genes);
        newSpecies.speciesID = nextSpeciesID;
        newSpecies.speciesName = string.Format("{0}_{1}", parentSpecies.speciesName, nextSpeciesID);
        nextSpeciesID++;
        RegisterSpecies(newSpecies);
        return newSpecies;
    }

    private void RebuildNextSpeciesID()
    {
        int maxSpeciesID = 0;
        for (int i = 0; i < registeredSpecies.Count; i++)
        {
            StatSetup setup = registeredSpecies[i];
            if (setup != null)
            {
                maxSpeciesID = Mathf.Max(maxSpeciesID, setup.speciesID);
            }
        }

        nextSpeciesID = Mathf.Max(1, maxSpeciesID + 1);
    }
}
