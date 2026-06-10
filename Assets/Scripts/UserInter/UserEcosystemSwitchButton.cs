using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserEcosystemSwitchButton : MonoBehaviour
{
    public Transform EcosystemCanvas;
    public Transform UserInterCanvas;
    public Transform ContinueCanvas;
    public Transform AnimalCreateCanvas;
    public Transform EcosystemCreateCanvas;
    private void Start()
    {
        EcosystemCanvas.gameObject.SetActive(false);
        UserInterCanvas.gameObject.SetActive(true);
        ContinueCanvas.gameObject.SetActive(false);
        AnimalCreateCanvas.gameObject.SetActive(false);
        EcosystemCreateCanvas.gameObject.SetActive(false);
    }

    public void User2Eco()
    {
        EcosystemCanvas.gameObject.SetActive(true);
        UserInterCanvas.gameObject.SetActive(false);
    }
    public void User2Conti()
    {
        ContinueCanvas.gameObject.SetActive(true);
        UserInterCanvas.gameObject.SetActive(false);
    }
    public void Eco2User()
    {
        EcosystemCanvas.gameObject.SetActive(false);
        UserInterCanvas.gameObject.SetActive(true);
    }
    public void ContiUser()
    {
        ContinueCanvas.gameObject.SetActive(false);
        UserInterCanvas.gameObject.SetActive(true);
    }
    public void ShowAnimalCreate()
    {
        AnimalCreateCanvas.gameObject.SetActive(true);
    }
    public void ShowEcosystemCreate()
    {
        EcosystemCreateCanvas.gameObject.SetActive(true);
    }
    public void CloseAnimalCreate()
    {
        AnimalCreateCanvas.gameObject.SetActive(false);
        EcosystemCreateCanvas.gameObject.SetActive(false);
    }
    public void CloseEcosystemCreate()
    {
        AnimalCreateCanvas.gameObject.SetActive(false);
        EcosystemCreateCanvas.gameObject.SetActive(false);
    }
}
