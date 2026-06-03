using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserEcosystemSwitchButton : MonoBehaviour
{
    public Transform EcosystemCanvas;
    public Transform UserInterCanvas;

    private void Start()
    {
        EcosystemCanvas.gameObject.SetActive(false);
        UserInterCanvas.gameObject.SetActive(true);
    }

    public void User2Eco()
    {
        EcosystemCanvas.gameObject.SetActive(true);
        UserInterCanvas.gameObject.SetActive(false);
    }
    public void Eco2User()
    {
        EcosystemCanvas.gameObject.SetActive(false);
        UserInterCanvas.gameObject.SetActive(true);
    }
}
