using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public DataBoard dataBoard;
    public ScrollRect listScrollRect;
    public float buttonStartY = 330f;
    public float buttonStartX = 0f;
    public float contentPaddingBottom = 30f;

    public bool isAni = true;

    public Transform SystemEditionParent;
    public Transform AnimalEditionParent;

    private void Start()
    {
        InitAniList();
        InitEcoList();

        SystemEditionParent.gameObject.SetActive(false);
        AnimalEditionParent.gameObject.SetActive(false);
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
            btn.SetDataBoard(dataBoard);
            RectTransform rect = b.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(buttonStartX, buttonStartY - buttonIndex * buttonHeight);
            buttonIndex++;
            aniList.Add(b);
            b.SetActive(false);
        }

        ResizeContent(aniList.Count);
    }
    private void CreateEcoButtons()
    {
        buttonIndex = 0;
        foreach (var s in ecoStats)
        {
            GameObject b = Instantiate(buttonPrefab, content);
            StatsButton btn = b.GetComponent<StatsButton>();
            btn.Init(s);
            btn.SetDataBoard(dataBoard);
            RectTransform rect = b.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(buttonStartX, buttonStartY - buttonIndex * buttonHeight);
            buttonIndex++;
            ecoList.Add(b);
            b.SetActive(false);
        }

        ResizeContent(ecoList.Count);
    }
    public void ShowAniButton()
    {
        isAni = true;
        SystemEditionParent.gameObject.SetActive(false);
        AnimalEditionParent.gameObject.SetActive(true);

        foreach (var ani in aniList)
            ani.SetActive(true);
        foreach(var eco in ecoList)
            eco.SetActive(false);

        ResizeContent(aniList.Count);
        ResetScrollToTop();
    }
    public void ShowEcoButton()
    {
        isAni = false;
        SystemEditionParent.gameObject.SetActive(true);
        AnimalEditionParent.gameObject.SetActive(false);
        foreach (var ani in aniList)
            ani.SetActive(false);
        foreach(var eco in ecoList)
            eco.SetActive(true);

        ResizeContent(ecoList.Count);
        ResetScrollToTop();
    }

    private void ResizeContent(int itemCount)
    {
        if (content == null)
        {
            return;
        }

        float viewportHeight = GetViewportHeight();
        float visibleHeight = maxVisibleCount * buttonHeight;
        float listHeight = Mathf.Max(0, itemCount - 1) * buttonHeight + buttonHeight + contentPaddingBottom;
        float contentHeight = Mathf.Max(viewportHeight, visibleHeight, buttonStartY + listHeight);

        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);

        if (listScrollRect != null && listScrollRect.content == null)
        {
            listScrollRect.content = content;
        }
    }

    private float GetViewportHeight()
    {
        if (listScrollRect != null && listScrollRect.viewport != null)
        {
            return listScrollRect.viewport.rect.height;
        }

        return 0f;
    }

    private void ResetScrollToTop()
    {
        if (listScrollRect == null)
        {
            return;
        }

        listScrollRect.horizontal = false;
        listScrollRect.vertical = true;
        listScrollRect.verticalNormalizedPosition = 1f;
    }

    public void RefreshLists()
    {
        ClearButtons(aniList);
        ClearButtons(ecoList);
        aniStats.Clear();
        ecoStats.Clear();

        InitAniList();
        InitEcoList();

        if (isAni)
        {
            ShowAniButton();
        }
        else
        {
            ShowEcoButton();
        }
    }

    private void ClearButtons(List<GameObject> buttons)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] != null)
            {
                Destroy(buttons[i]);
            }
        }

        buttons.Clear();
    }
}
