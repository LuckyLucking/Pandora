using UnityEngine;

[DisallowMultipleComponent]
public class EggBase : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer eggRenderer;
    [SerializeField] protected float hatchTimer;
    [SerializeField] protected float hatchDuration = 10f;
    [SerializeField] protected StatSetup parentSpecies;
    [SerializeField] protected AnimalGeneSnapshot offspringGenes;
    [SerializeField] protected AnimalBase offspringPrefab;

    protected virtual void Reset()
    {
        CacheReferences();
    }

    protected virtual void Awake()
    {
        CacheReferences();
        UpdateVisual();
    }

    protected virtual void Update()
    {
        if (parentSpecies == null)
        {
            return;
        }

        hatchTimer += Time.deltaTime;
        if (hatchTimer >= hatchDuration)
        {
            Hatch();
        }
    }

    public virtual void Initialize(StatSetup sourceSpecies, AnimalGeneSnapshot genes, AnimalBase prefab)
    {
        parentSpecies = sourceSpecies;
        offspringGenes = genes;
        offspringPrefab = prefab != null ? prefab : (sourceSpecies != null ? sourceSpecies.animalPrefab : null);
        hatchDuration = Mathf.Max(0.01f, genes.hatchDuration > 0f ? genes.hatchDuration : (sourceSpecies != null ? sourceSpecies.hatchDuration : 0.01f));
        hatchTimer = 0f;
        CacheReferences();
        UpdateVisual();
    }

    protected virtual void CacheReferences()
    {
        if (eggRenderer == null)
        {
            eggRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    protected virtual void Hatch()
    {
        StatSetup resolvedSpecies = SpeciesRegistry.Instance.ResolveSpecies(parentSpecies, offspringGenes);
        AnimalBase hatchPrefab = offspringPrefab != null ? offspringPrefab : (resolvedSpecies != null ? resolvedSpecies.animalPrefab : null);
        if (hatchPrefab == null || resolvedSpecies == null)
        {
            Debug.LogWarning("Egg hatch failed because no offspring prefab or species data was available.", this);
            Destroy(gameObject);
            return;
        }

        AnimalBase offspring = Instantiate(hatchPrefab, transform.position, hatchPrefab.transform.rotation);
        offspring.InitializeAsOffspring(resolvedSpecies, hatchPrefab);
        Destroy(gameObject);
    }

    protected virtual void UpdateVisual()
    {
        if (eggRenderer != null)
        {
            eggRenderer.color = EvaluateDietColor(offspringGenes.dietHabit);
        }

        float displaySize = Mathf.Max(0.25f, offspringGenes.bodySize * 0.5f);
        transform.localScale = Vector3.one * displaySize;
    }

    protected virtual Color EvaluateDietColor(float dietValue)
    {
        dietValue = Mathf.Clamp01(dietValue);
        if (dietValue <= 0.5f)
        {
            return Color.Lerp(Color.red, Color.blue, dietValue / 0.5f);
        }

        return Color.Lerp(Color.blue, Color.green, (dietValue - 0.5f) / 0.5f);
    }
}
