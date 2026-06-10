using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StatsAssetCreator : MonoBehaviour
{
    private const string AnimalResourcesPath = "Stats/Ani";
    private const string AnimalAssetFolder = "Assets/Resources/Stats/Ani";
    private const string EcosystemAssetFolder = "Assets/Resources/Stats/Eco";

    public Transform animalCreatingPanel;
    public Transform ecosystemCreatingPanel;

    [Header("Refresh")]
    public EcosystemList ecosystemList;
    public DataBoard aniDataBoard;
    public DataBoard ecoDataBoard;
    public bool refreshListAfterSave = true;
    public bool showSavedData = true;

    [Header("Animal References")]
    public StatSetup animalTemplate;
    public AnimalBase animalPrefab;
    public GameObject eggPrefab;

    [Header("Animal Inputs")]
    public TMP_InputField animalFileNameInput;
    public TMP_InputField speciesNameInput;
    public TMP_InputField speciesIDInput;
    public TMP_InputField dietHabitInput;
    public TMP_InputField bodySizeInput;
    public TMP_InputField moveSpeedInput;
    public TMP_InputField turnSpeedInput;
    public TMP_InputField visionRangeInput;
    public TMP_InputField maxHealthInput;
    public TMP_InputField maxEnergyInput;
    public TMP_InputField reproductionThresholdInput;
    public TMP_InputField reproductionCooldownInput;
    public TMP_InputField hatchDurationInput;
    public TMP_InputField attackDamageInput;
    public TMP_InputField attackEnergyCostNormalizedInput;
    public TMP_InputField mutationChanceInput;
    public TMP_InputField mutationPercentInput;

    [Header("Ecosystem References")]
    public EcosystemSetup ecosystemTemplate;
    public List<SpeciesPopulationSetup> speciesPopulations = new List<SpeciesPopulationSetup>();

    [Header("Ecosystem Inputs")]
    public TMP_InputField ecosystemFileNameInput;
    public TMP_InputField ecosystemNameInput;
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public TMP_InputField grassChanceInput;
    public TMP_InputField meatChanceInput;

    public void SaveAnimalFromInputs()
    {
        animalCreatingPanel.gameObject.SetActive(false);
        StatSetup setup = CreateAnimalFromInputs();
        StatSetup savedSetup = SaveAsset(setup, AnimalAssetFolder, GetText(animalFileNameInput, setup.speciesName));

        if (savedSetup == null)
        {
            return;
        }

        SpeciesRegistry.Instance.RegisterSpecies(savedSetup);
        AfterSaveAnimal(savedSetup);
    }

    public void SaveEcosystemFromInputs()
    {
        ecosystemCreatingPanel.gameObject.SetActive(false);
        EcosystemSetup setup = CreateEcosystemFromInputs();
        EcosystemSetup savedSetup = SaveAsset(setup, EcosystemAssetFolder, GetText(ecosystemFileNameInput, setup.ecosystemName));

        if (savedSetup == null)
        {
            return;
        }

        AfterSaveEcosystem(savedSetup);
    }

    public StatSetup CreateAnimalFromInputs()
    {
        StatSetup setup = animalTemplate != null
            ? Instantiate(animalTemplate)
            : ScriptableObject.CreateInstance<StatSetup>();

        setup.name = GetText(animalFileNameInput, GetText(speciesNameInput, "New Species"));
        setup.speciesName = GetText(speciesNameInput, setup.speciesName);
        setup.speciesID = Mathf.Max(1, ReadInt(speciesIDInput, GetNextSpeciesID()));
        setup.animalPrefab = animalPrefab != null ? animalPrefab : setup.animalPrefab;
        setup.eggPrefab = eggPrefab != null ? eggPrefab : setup.eggPrefab;

        setup.dietHabit = Mathf.Clamp01(ReadFloat(dietHabitInput, setup.dietHabit));
        setup.bodySize = Mathf.Max(0.01f, ReadFloat(bodySizeInput, setup.bodySize));
        setup.moveSpeed = Mathf.Max(0f, ReadFloat(moveSpeedInput, setup.moveSpeed));
        setup.turnSpeed = Mathf.Max(0f, ReadFloat(turnSpeedInput, setup.turnSpeed));
        setup.visionRange = Mathf.Max(0.1f, ReadFloat(visionRangeInput, setup.visionRange));
        setup.maxHealth = Mathf.Max(0.1f, ReadFloat(maxHealthInput, setup.maxHealth));
        setup.maxEnergy = Mathf.Max(0.1f, ReadFloat(maxEnergyInput, setup.maxEnergy));
        setup.reproductionThreshold = Mathf.Clamp(ReadFloat(reproductionThresholdInput, setup.reproductionThreshold), 0f, setup.maxEnergy);
        setup.reproductionCooldown = Mathf.Max(0f, ReadFloat(reproductionCooldownInput, setup.reproductionCooldown));
        setup.hatchDuration = Mathf.Max(0f, ReadFloat(hatchDurationInput, setup.hatchDuration));
        setup.attackDamage = Mathf.Max(0f, ReadFloat(attackDamageInput, setup.attackDamage));
        setup.attackEnergyCostNormalized = Mathf.Clamp01(ReadFloat(attackEnergyCostNormalizedInput, setup.attackEnergyCostNormalized));
        setup.mutationChance = Mathf.Clamp01(ReadFloat(mutationChanceInput, setup.mutationChance));
        setup.mutationPercent = Mathf.Clamp01(ReadFloat(mutationPercentInput, setup.mutationPercent));

        return setup;
    }

    public EcosystemSetup CreateEcosystemFromInputs()
    {
        EcosystemSetup setup = ecosystemTemplate != null
            ? Instantiate(ecosystemTemplate)
            : ScriptableObject.CreateInstance<EcosystemSetup>();

        setup.name = GetText(ecosystemFileNameInput, GetText(ecosystemNameInput, "New Ecosystem"));
        setup.ecosystemName = GetText(ecosystemNameInput, setup.ecosystemName);
        setup.width = Mathf.Max(1, ReadInt(widthInput, setup.width));
        setup.height = Mathf.Max(1, ReadInt(heightInput, setup.height));
        setup.grassChance = Mathf.Clamp(ReadFloat(grassChanceInput, setup.grassChance), 0f, 100f);
        setup.meatChance = Mathf.Clamp(ReadFloat(meatChanceInput, setup.meatChance), 0f, 100f);

        if (speciesPopulations != null && speciesPopulations.Count > 0)
        {
            setup.speciesPopulations = CloneSpeciesPopulations(speciesPopulations);
        }

        return setup;
    }

    private void AfterSaveAnimal(StatSetup setup)
    {
        if (refreshListAfterSave && ecosystemList != null)
        {
            ecosystemList.RefreshLists();
            ecosystemList.ShowAniButton();
        }

        if (showSavedData && aniDataBoard != null)
        {
            aniDataBoard.ShowData(setup);
        }
    }

    private void AfterSaveEcosystem(EcosystemSetup setup)
    {
        if (refreshListAfterSave && ecosystemList != null)
        {
            ecosystemList.RefreshLists();
            ecosystemList.ShowEcoButton();
        }

        if (showSavedData && ecoDataBoard != null)
        {
            ecoDataBoard.ShowData(setup);
        }
    }

    private int GetNextSpeciesID()
    {
        int maxSpeciesID = 0;
        StatSetup[] setups = Resources.LoadAll<StatSetup>(AnimalResourcesPath);
        for (int i = 0; i < setups.Length; i++)
        {
            if (setups[i] != null)
            {
                maxSpeciesID = Mathf.Max(maxSpeciesID, setups[i].speciesID);
            }
        }

        return maxSpeciesID + 1;
    }

    private static List<SpeciesPopulationSetup> CloneSpeciesPopulations(List<SpeciesPopulationSetup> source)
    {
        List<SpeciesPopulationSetup> result = new List<SpeciesPopulationSetup>();
        for (int i = 0; i < source.Count; i++)
        {
            SpeciesPopulationSetup population = source[i];
            if (population == null || population.statSetup == null)
            {
                continue;
            }

            result.Add(new SpeciesPopulationSetup
            {
                statSetup = population.statSetup,
                spawnCount = Mathf.Max(0, population.spawnCount)
            });
        }

        return result;
    }

    private static string GetText(TMP_InputField input, string fallback)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.text))
        {
            return fallback;
        }

        return input.text.Trim();
    }

    private static int ReadInt(TMP_InputField input, int fallback)
    {
        string value = GetText(input, string.Empty);
        if (string.IsNullOrEmpty(value))
        {
            return fallback;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ||
            int.TryParse(value, out parsed))
        {
            return parsed;
        }

        Debug.LogWarning($"Cannot parse int value: {value}");
        return fallback;
    }

    private static float ReadFloat(TMP_InputField input, float fallback)
    {
        string value = GetText(input, string.Empty);
        if (string.IsNullOrEmpty(value))
        {
            return fallback;
        }

        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ||
            float.TryParse(value, out parsed))
        {
            return parsed;
        }

        Debug.LogWarning($"Cannot parse float value: {value}");
        return fallback;
    }

    private static string SanitizeFileName(string fileName)
    {
        string result = string.IsNullOrWhiteSpace(fileName) ? "New Data" : fileName.Trim();
        char[] invalidChars = Path.GetInvalidFileNameChars();

        for (int i = 0; i < invalidChars.Length; i++)
        {
            result = result.Replace(invalidChars[i], '_');
        }

        return result;
    }

#if UNITY_EDITOR
    private static T SaveAsset<T>(T setup, string assetFolder, string fileName) where T : ScriptableObject
    {
        Directory.CreateDirectory(assetFolder);
        AssetDatabase.Refresh();

        string safeFileName = SanitizeFileName(fileName);
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{assetFolder}/{safeFileName}.asset");

        AssetDatabase.CreateAsset(setup, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Saved data asset: {assetPath}");
        return setup;
    }
#else
    private static T SaveAsset<T>(T setup, string assetFolder, string fileName) where T : ScriptableObject
    {
        Debug.LogError("Saving ScriptableObject assets into Resources requires UnityEditor.AssetDatabase, so it only works in the Unity Editor. Use JSON/Application.persistentDataPath for runtime builds.");
        Destroy(setup);
        return null;
    }
#endif
}
