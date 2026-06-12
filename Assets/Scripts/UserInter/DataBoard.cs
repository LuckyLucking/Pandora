using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DataBoard : MonoBehaviour, IScrollHandler
{
    private StatSetup animData;
    public EcosystemSetup currentData;
    private EcosystemSetup ecoData;

    [Header("Display")]
    public TMP_Text dataText;
    public GameObject boardRoot;
    public ScrollRect dataScrollRect;
    public RectTransform dataContent;
    public float minScrollContentHeight = 0f;

    [Header("Scene Load")]
    [SerializeField] private string ecosystemSceneName = "DefaultEco";

    [Header("Scroll")]
    public float scrollWheelSensitivity = 0.2f;

    [Header("Animal")]
    public float dietHabit;
    public float bodySize;
    public float moveSpeed;
    public float turnSpeed;
    public float visionRange;
    public float maxHealth;
    public float maxEnergy;
    public float hatchDuration;
    public float attackDamage;


    [Header("Ecosystem")]
    public int width;
    public int height;
    public float grassChance;
    public float meatChance;

    public void ShowData(StatSetup anim)
    {
        if (anim == null)
        {
            return;
        }

        animData = anim;
        ecoData = null;
        currentData = null;

        dietHabit = anim.dietHabit;
        bodySize = anim.bodySize;
        moveSpeed = anim.moveSpeed;
        turnSpeed = anim.turnSpeed;
        visionRange = anim.visionRange;
        maxHealth = anim.maxHealth;
        maxEnergy = anim.maxEnergy;
        hatchDuration = anim.hatchDuration;
        attackDamage = anim.attackDamage;

        SetBoardVisible(true);
        RenderAnimalData();
    }

    public void ShowData(EcosystemSetup eco)
    {
        if (eco == null)
        {
            return;
        }

        ecoData = eco;
        currentData = eco;
        animData = null;

        width = eco.width;
        height = eco.height;
        grassChance = eco.grassChance;
        meatChance = eco.meatChance;

        SetBoardVisible(true);
        RenderEcosystemData();
    }

    public void LoadEcosystem()
    {
        if (currentData == null)
        {
            Debug.LogWarning("DataBoard cannot load an ecosystem because currentData is null.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(ecosystemSceneName))
        {
            Debug.LogWarning("DataBoard needs an ecosystem scene name before it can load an ecosystem.", this);
            return;
        }

        EcosystemLoadContext.Select(currentData);
        SceneManager.LoadScene(ecosystemSceneName);
    }

    private void RenderAnimalData()
    {
        if (!HasDataText())
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Species Name: {GetSpeciesName(animData)}");
        builder.AppendLine($"Species ID: {animData.speciesID}");
        builder.AppendLine($"Diet Type: {animData.GetDietType()}");
        builder.AppendLine($"Diet Habit: {Format(dietHabit)}");
        builder.AppendLine($"Body Size: {Format(bodySize)}");
        builder.AppendLine($"Move Speed: {Format(moveSpeed)}");
        builder.AppendLine($"Turn Speed: {Format(turnSpeed)}");
        builder.AppendLine($"Vision Range: {Format(visionRange)}");
        builder.AppendLine($"Max Health: {Format(maxHealth)}");
        builder.AppendLine($"Max Energy: {Format(maxEnergy)}");
        builder.AppendLine($"Hatch Duration: {Format(hatchDuration)}");
        builder.AppendLine($"Attack Damage: {Format(attackDamage)}");

        SetText(builder.ToString());
    }

    private void RenderEcosystemData()
    {
        if (!HasDataText())
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Ecosystem Name: {GetEcosystemName(ecoData)}");
        builder.AppendLine($"Width: {width}");
        builder.AppendLine($"Height: {height}");
        builder.AppendLine($"Grass Chance: {Format(grassChance)}");
        builder.AppendLine($"Meat Chance: {Format(meatChance)}");
        builder.AppendLine($"Total Spawn Count: {ecoData.GetTotalSpawnCount()}");

        if (ecoData.speciesPopulations != null && ecoData.speciesPopulations.Count > 0)
        {
            builder.AppendLine("Species Populations:");
            for (int i = 0; i < ecoData.speciesPopulations.Count; i++)
            {
                SpeciesPopulationSetup population = ecoData.speciesPopulations[i];
                if (population == null || population.statSetup == null)
                {
                    continue;
                }

                builder.AppendLine($"- {GetSpeciesName(population.statSetup)}: {population.spawnCount}");
            }
        }

        SetText(builder.ToString());
    }

    private void SetText(string value)
    {
        dataText.text = value;
        RefreshScrollContent();
    }

    private void RefreshScrollContent()
    {
        if (dataText == null)
        {
            return;
        }

        dataText.enableWordWrapping = true;
        dataText.overflowMode = TextOverflowModes.Overflow;

        Canvas.ForceUpdateCanvases();
        dataText.ForceMeshUpdate();

        RectTransform textRect = dataText.rectTransform;
        RectTransform contentRect = dataContent != null ? dataContent : textRect;
        float viewportHeight = GetDataViewportHeight(contentRect);
        float contentHeight = Mathf.Max(minScrollContentHeight, viewportHeight, dataText.preferredHeight);

        contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);

        if (dataScrollRect != null)
        {
            if (dataScrollRect.content == null)
            {
                dataScrollRect.content = contentRect;
            }

            dataScrollRect.horizontal = false;
            dataScrollRect.vertical = true;
            dataScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private float GetDataViewportHeight(RectTransform contentRect)
    {
        if (dataScrollRect != null && dataScrollRect.viewport != null)
        {
            return dataScrollRect.viewport.rect.height;
        }

        return contentRect.rect.height;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (dataScrollRect == null)
        {
            return;
        }

        dataScrollRect.verticalNormalizedPosition = Mathf.Clamp01(
            dataScrollRect.verticalNormalizedPosition + eventData.scrollDelta.y * scrollWheelSensitivity
        );
    }

    private void SetBoardVisible(bool isVisible)
    {
        if (boardRoot != null)
        {
            boardRoot.SetActive(isVisible);
            return;
        }

        gameObject.SetActive(isVisible);
    }

    private bool HasDataText()
    {
        if (dataText != null)
        {
            return true;
        }

        Debug.LogWarning("DataBoard needs a TMP_Text assigned before it can display data.", this);
        return false;
    }

    private static string GetSpeciesName(StatSetup stats)
    {
        if (stats == null)
        {
            return string.Empty;
        }

        return string.IsNullOrEmpty(stats.speciesName) ? stats.name : stats.speciesName;
    }

    private static string GetEcosystemName(EcosystemSetup eco)
    {
        if (eco == null)
        {
            return string.Empty;
        }

        return string.IsNullOrEmpty(eco.ecosystemName) ? eco.name : eco.ecosystemName;
    }

    private static string Format(float value)
    {
        return value.ToString("0.###");
    }
}
