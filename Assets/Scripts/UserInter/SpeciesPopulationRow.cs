using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeciesPopulationRow : MonoBehaviour
{
    public TMP_Dropdown speciesDropdown;
    public TMP_InputField spawnCountInput;
    public Button removeButton;

    private readonly List<StatSetup> speciesOptions = new List<StatSetup>();
    private Action<SpeciesPopulationRow> onRemove;

    private void OnDestroy()
    {
        if (removeButton != null)
        {
            removeButton.onClick.RemoveListener(HandleRemoveClicked);
        }
    }

    public void Init(List<StatSetup> availableSpecies, Action<SpeciesPopulationRow> removeCallback)
    {
        onRemove = removeCallback;
        SetSpeciesOptions(availableSpecies);

        if (spawnCountInput != null && string.IsNullOrWhiteSpace(spawnCountInput.text))
        {
            spawnCountInput.text = "5";
        }

        if (removeButton != null)
        {
            removeButton.onClick.RemoveListener(HandleRemoveClicked);
            removeButton.onClick.AddListener(HandleRemoveClicked);
        }
    }

    public bool TryRead(out StatSetup statSetup, out int spawnCount)
    {
        statSetup = GetSelectedSpecies();
        spawnCount = ReadSpawnCount();

        return statSetup != null && spawnCount > 0;
    }

    private void SetSpeciesOptions(List<StatSetup> availableSpecies)
    {
        speciesOptions.Clear();

        if (availableSpecies != null)
        {
            for (int i = 0; i < availableSpecies.Count; i++)
            {
                if (availableSpecies[i] != null)
                {
                    speciesOptions.Add(availableSpecies[i]);
                }
            }
        }

        if (speciesDropdown == null)
        {
            return;
        }

        speciesDropdown.ClearOptions();

        List<string> optionLabels = new List<string> { "Select Species" };
        for (int i = 0; i < speciesOptions.Count; i++)
        {
            optionLabels.Add(GetSpeciesDisplayName(speciesOptions[i]));
        }

        speciesDropdown.AddOptions(optionLabels);
        speciesDropdown.value = 0;
        speciesDropdown.RefreshShownValue();
    }

    private StatSetup GetSelectedSpecies()
    {
        if (speciesDropdown == null)
        {
            return null;
        }

        int speciesIndex = speciesDropdown.value - 1;
        if (speciesIndex < 0 || speciesIndex >= speciesOptions.Count)
        {
            return null;
        }

        return speciesOptions[speciesIndex];
    }

    private int ReadSpawnCount()
    {
        if (spawnCountInput == null || string.IsNullOrWhiteSpace(spawnCountInput.text))
        {
            return 0;
        }

        if (int.TryParse(spawnCountInput.text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ||
            int.TryParse(spawnCountInput.text.Trim(), out parsed))
        {
            return Mathf.Max(0, parsed);
        }

        return 0;
    }

    private void HandleRemoveClicked()
    {
        onRemove?.Invoke(this);
    }

    private static string GetSpeciesDisplayName(StatSetup setup)
    {
        if (setup == null)
        {
            return string.Empty;
        }

        return string.IsNullOrEmpty(setup.speciesName) ? setup.name : setup.speciesName;
    }
}
