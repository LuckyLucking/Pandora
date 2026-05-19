using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassBase : MonoBehaviour
{
    public float grassFullfillAmount;
    public float grassCurrentAmount;

    private void Awake()
    {
        grassCurrentAmount = grassFullfillAmount;
    }
}
