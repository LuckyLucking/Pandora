using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatsButton : MonoBehaviour
{
    public TMP_Text text;
    public StatSetup aniData;
    public EcosystemSetup ecoData;

    public DataBoard dataBoard;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
        }
    }

    public void Init(StatSetup stats)
    {
        aniData = stats;
        ecoData = null;
        text.text = string.IsNullOrEmpty(stats.speciesName) ? stats.name : stats.speciesName;
    }

    public void Init(EcosystemSetup stats)
    {
        ecoData = stats;
        aniData = null;
        text.text = string.IsNullOrEmpty(stats.ecosystemName) ? stats.name : stats.ecosystemName;
    }

    public void SetDataBoard(DataBoard targetDataBoard)
    {
        dataBoard = targetDataBoard;
    }

    public void OnClick()
    {
        if (dataBoard == null)
        {
            Debug.LogWarning("StatsButton needs a DataBoard assigned before it can show data.", this);
            return;
        }

        if (aniData != null)
        {
            dataBoard.ShowData(aniData);
        }
        else if (ecoData != null)
        {
            dataBoard.ShowData(ecoData);
        }
    }
}
