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

    public float meatMaxAmount;
    public float meatCurrentAMount;
    private Vector3 baseScale;
    private bool scaleCached;

    private void Awake()
    {
        InitMeatState();
    }

    public void InitMeatState()
    {
        CacheBaseScale();
        meatMaxAmount = Mathf.Max(0f, meatMaxAmount);

        if (meatMaxAmount > 0 && meatMaxAmount <= 10f)
            ChangeSize(MeatState.Small);
        else if (meatMaxAmount > 10 && meatMaxAmount <= 20f)
            ChangeSize(MeatState.Medium);
        else if(meatMaxAmount > 20f && meatMaxAmount <=30f)
            ChangeSize(MeatState.Masive);

        meatCurrentAMount = meatMaxAmount;
        UpdateVisual();
    }
    public void ChangeSize(MeatState changeState)
    {
        currentState = changeState;
        UpdateVisual();
    }

    public float Consume(float amount)
    {
        if (amount <= 0f || meatCurrentAMount <= 0f)
        {
            return 0f;
        }

        float consumed = Mathf.Min(amount, meatCurrentAMount);
        meatCurrentAMount -= consumed;
        UpdateVisual();
        return consumed;
    }

    public bool IsDepleted()
    {
        return meatCurrentAMount <= 0f;
    }

    public float GetNormalizedAmount()
    {
        if (meatMaxAmount <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(meatCurrentAMount / meatMaxAmount);
    }

    private void CacheBaseScale()
    {
        if (scaleCached)
        {
            return;
        }

        baseScale = transform.localScale;
        scaleCached = true;
    }

    private float GetStateScaleMultiplier()
    {
        switch (currentState)
        {
            case MeatState.Small:
                return 0.5f;
            case MeatState.Medium:
                return 0.7f;
            default:
                return 1f;
        }
    }

    private void UpdateVisual()
    {
        CacheBaseScale();
        transform.localScale = baseScale * GetStateScaleMultiplier() * GetNormalizedAmount();
    }
}
