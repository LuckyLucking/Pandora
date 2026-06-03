using UnityEngine;
using TMPro;

public class StatsButton : MonoBehaviour
{
    public TMP_Text text;
    public StatSetup aniData;
    public EcosystemSetup ecoData;

    public DataBoard dataBoard;
    public void Init(StatSetup stats)
    {
        aniData = stats;
        text.text = stats.name;
    }
    public void Init(EcosystemSetup stats)
    {
        ecoData = stats;
        text.text = stats.name;
    }

    public void OnClick()
    {
        if (aniData != null)
            Debug.Log($"Clicked: {aniData.name}");
        else if (ecoData != null)
            Debug.Log($"Clicked: {ecoData.name}");
    }
}