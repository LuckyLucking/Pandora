using UnityEngine;

public class GrassBase : MonoBehaviour
{
    public float grassMaxAmount;
    public float grassCurrentAmount;
    private Vector3 baseScale;
    private bool scaleCached;

    private void Awake()
    {
        InitializeGrass();
    }

    public void InitializeGrass()
    {
        CacheBaseScale();
        grassMaxAmount = Mathf.Max(0f, grassMaxAmount);
        grassCurrentAmount = grassMaxAmount;
        UpdateVisual();
    }

    public float Consume(float amount)
    {
        if (amount <= 0f || grassCurrentAmount <= 0f)
        {
            return 0f;
        }

        float consumed = Mathf.Min(amount, grassCurrentAmount);
        grassCurrentAmount -= consumed;
        UpdateVisual();
        return consumed;
    }

    public bool IsDepleted()
    {
        return grassCurrentAmount <= 0f;
    }

    public float GetNormalizedAmount()
    {
        if (grassMaxAmount <= 0f)
        {
            return 0f; 
        }

        return Mathf.Clamp01(grassCurrentAmount / grassMaxAmount);
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

    private void UpdateVisual()
    {
        CacheBaseScale();
        transform.localScale = baseScale * GetNormalizedAmount();
    }
}
