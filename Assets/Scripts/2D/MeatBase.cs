using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeatState
{
    Masive,
    Medium,
    Small
}
public class MeatBase : MonoBehaviour
{
    public MeatState currentState;

    public float meatCurrentAmount;

    private void Awake()
    {
        InitMeatState();
    }

    private void Update()
    {
        
    }

    public void InitMeatState()
    {
        if (meatCurrentAmount > 0 && meatCurrentAmount <= 10f)
            ChangeSize(MeatState.Small);
        else if (meatCurrentAmount > 10 && meatCurrentAmount <= 20f)
            ChangeSize(MeatState.Medium);
        else
            ChangeSize(MeatState.Masive);
    }
    public void ChangeSize(MeatState changeState)
    {
        if (changeState == MeatState.Small)
        {
            currentState = MeatState.Small;
            transform.localScale *= 0.5f;
        }
        if (changeState == MeatState.Medium)
        {
            currentState = MeatState.Medium;
            transform.localScale *= 0.7f;
        }
        if (currentState == MeatState.Masive)
        {
            currentState = MeatState.Masive;
        }
    }
}