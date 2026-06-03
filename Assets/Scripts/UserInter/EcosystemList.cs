using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EcosystemList : MonoBehaviour
{
    List<StatSetup> aniStats = new();
    List<EcosystemSetup> ecoStats = new();
    List<GameObject> aniList = new();
    List <GameObject> ecoList = new();

    public GameObject buttonPrefab;
    public RectTransform content;
    public float buttonHeight = 60f;
    public int buttonIndex = 0;
    public int maxVisibleCount = 8;

    public bool isAni = true;

    private void Start()
    {
        InitAniList();
        InitEcoList();
    }

    public void InitAniList()
    {
        aniStats.AddRange(Resources.LoadAll<StatSetup>("Stats/Ani"));
        CreateAniButtons();
    }

    public void InitEcoList()
    {
        ecoStats.AddRange(Resources.LoadAll<EcosystemSetup>("Stats/Eco"));
        CreateEcoButtons();
    }

    private void CreateAniButtons()
    {
        buttonIndex = 0;
        foreach (var s in aniStats)
        {
            GameObject b = Instantiate(buttonPrefab, content);
            StatsButton btn = b.GetComponent<StatsButton>();
            btn.Init(s);
            RectTransform rect = b.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 330 - buttonIndex * buttonHeight);
            buttonIndex++;
            aniList.Add(b);
            b.SetActive(false);
        }
    }
    private void CreateEcoButtons()
    {
        buttonIndex = 0;
        foreach (var s in ecoStats)
        {
            GameObject b = Instantiate(buttonPrefab, content);
            StatsButton btn = b.GetComponent<StatsButton>();
            btn.Init(s);
            RectTransform rect = b.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 330 - buttonIndex * buttonHeight);
            buttonIndex++;
            ecoList.Add(b);
            b.SetActive(false);
        }
    }
    public void ShowAniButton()
    {
        foreach (var ani in aniList)
            ani.SetActive(true);
        foreach(var eco in ecoList)
            eco.SetActive(false);
    }
    public void ShowEcoButton()
    {
        foreach (var ani in aniList)
            ani.SetActive(false);
        foreach(var eco in ecoList)
            eco.SetActive(true);
    }
}
