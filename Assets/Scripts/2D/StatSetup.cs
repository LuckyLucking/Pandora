using UnityEngine;

public enum DietType
{
    Carnivore,
    Omnivore,
    Herbivore
}

[System.Serializable]
public struct AnimalGeneSnapshot
{
    [Range(0f, 1f)] public float dietHabit;
    [Min(0.01f)] public float bodySize;
    [Min(0f)] public float moveSpeed;
    [Min(0f)] public float turnSpeed;
    [Min(0.1f)] public float visionRange;
    [Min(0.1f)] public float maxHealth;
    [Min(0.1f)] public float maxEnergy;
    [Min(0f)] public float reproductionThreshold;
    [Min(0f)] public float reproductionCooldown;
    [Min(0f)] public float hatchDuration;
    [Range(0f, 1f)] public float attackEnergyCostNormalized;
    [Min(0f)] public float attackDamage;
}

[System.Serializable]
public class EvolutionScoreWeights
{
    [Min(0f)] public float dietHabit = 1f;
    [Min(0f)] public float bodySize = 1f;
    [Min(0f)] public float moveSpeed = 1f;
    [Min(0f)] public float turnSpeed = 1f;
    [Min(0f)] public float visionRange = 1f;
    [Min(0f)] public float maxHealth = 1f;
    [Min(0f)] public float maxEnergy = 1f;
    [Min(0f)] public float reproductionThreshold = 1f;
    [Min(0f)] public float reproductionCooldown = 1f;
    [Min(0f)] public float hatchDuration = 1f;
    [Min(0f)] public float attackEnergyCostNormalized = 1f;
    [Min(0f)] public float attackDamage = 1f;
}

[CreateAssetMenu(menuName = "PandoraData/Stat Setup", fileName = "Stat Setup")]
public class StatSetup : ScriptableObject
{
    [Header("Identity")]
    public int speciesID;
    public string speciesName = "New Species";

    [Header("Diet")]
    [Range(0f, 1f)] public float dietHabit = 0.5f;

    [Header("Movement")]
    [Min(0.01f)] public float bodySize = 1f;
    [Min(0f)] public float moveSpeed = 2f;
    [Min(0f)] public float turnSpeed = 360f;
    [Min(0.1f)] public float visionRange = 5f;
    [Range(1f, 360f)] public float foodSearchAngle = 120f;

    [Header("Life")]
    [Min(0.1f)] public float maxHealth = 100f;
    [Min(0.1f)] public float maxEnergy = 100f;
    [Min(0f)] public float reproductionThreshold = 80f;
    [Min(0f)] public float reproductionCooldown = 20f;
    [Min(0f)] public float hatchDuration = 10f;

    [Header("Energy Rules")]
    [Min(0f)] public float energyLossK = 1f;
    [Min(0f)] public float foodEnergyConvertRate = 10f;
    [Range(0f, 1f)] public float seekFoodThresholdNormalized = 0.9f;
    [Range(0f, 1f)] public float sameSpeciesHuntThresholdNormalized = 0.2f;
    [Range(0f, 1f)] public float reproductionCostNormalized = 0.2f;

    [Header("Combat")]
    [Min(0f)] public float attackDamage = 10f;
    [Range(0f, 1f)] public float attackEnergyCostNormalized = 0.05f;
    [Min(0.01f)] public float attackRangePerSize = 0.5f;
    [Min(0.01f)] public float attackInterval = 1f;

    [Header("Mutation")]
    [Range(0f, 1f)] public float mutationChance = 0.15f;
    [Range(0f, 1f)] public float mutationPercent = 0.1f;

    [Header("Evolution")]
    public EvolutionScoreWeights evolutionWeights = new EvolutionScoreWeights();
    [Min(0f)] public float evolutionThreshold = 3f;

    public DietType GetDietType()
    {
        if (dietHabit < 0.4f)
        {
            return DietType.Carnivore;
        }

        if (dietHabit <= 0.6f)
        {
            return DietType.Omnivore;
        }

        return DietType.Herbivore;
    }

    public float GetAttackRange()
    {
        return Mathf.Max(0.01f, bodySize * attackRangePerSize);
    }

    public float GetPassiveEnergyCostPerSecond()
    {
        return moveSpeed * energyLossK / Mathf.Max(0.01f, 2f * bodySize);
    }

    public float GetAttackEnergyCost()
    {
        return maxEnergy * attackEnergyCostNormalized;
    }

    public float GetReproductionCost()
    {
        return maxEnergy * reproductionCostNormalized;
    }

    public bool CanHuntSameSpecies(float currentEnergy)
    {
        return currentEnergy < maxEnergy * sameSpeciesHuntThresholdNormalized;
    }

    public AnimalGeneSnapshot CaptureGenes()
    {
        return new AnimalGeneSnapshot
        {
            dietHabit = dietHabit,
            bodySize = bodySize,
            moveSpeed = moveSpeed,
            turnSpeed = turnSpeed,
            visionRange = visionRange,
            maxHealth = maxHealth,
            maxEnergy = maxEnergy,
            reproductionThreshold = reproductionThreshold,
            reproductionCooldown = reproductionCooldown,
            hatchDuration = hatchDuration,
            attackEnergyCostNormalized = attackEnergyCostNormalized,
            attackDamage = attackDamage
        };
    }

    public AnimalGeneSnapshot CreateMutatedGenes()
    {
        AnimalGeneSnapshot genes = CaptureGenes();

        MutateClamped(ref genes.dietHabit, 0f, 1f);
        MutateMin(ref genes.bodySize, 0.01f);
        MutateMin(ref genes.moveSpeed, 0f);
        MutateMin(ref genes.turnSpeed, 0f);
        MutateMin(ref genes.visionRange, 0.1f);
        MutateMin(ref genes.maxHealth, 0.1f);
        MutateMin(ref genes.maxEnergy, 0.1f);
        MutateMin(ref genes.reproductionCooldown, 0f);
        MutateMin(ref genes.hatchDuration, 0f);
        MutateClamped(ref genes.attackEnergyCostNormalized, 0f, 1f);
        MutateMin(ref genes.attackDamage, 0f);

        MutateMin(ref genes.reproductionThreshold, 0f);
        genes.reproductionThreshold = Mathf.Clamp(genes.reproductionThreshold, 0f, genes.maxEnergy);

        return genes;
    }

    public float EvaluateDifferenceScore(StatSetup other)
    {
        if (other == null)
        {
            return float.MaxValue;
        }

        return EvaluateDifferenceScore(other.CaptureGenes());
    }

    public float EvaluateDifferenceScore(AnimalGeneSnapshot otherGenes)
    {
        AnimalGeneSnapshot sourceGenes = CaptureGenes();

        float score = 0f;
        score += GetWeightedPercentDifference(sourceGenes.dietHabit, otherGenes.dietHabit, evolutionWeights.dietHabit);
        score += GetWeightedPercentDifference(sourceGenes.bodySize, otherGenes.bodySize, evolutionWeights.bodySize);
        score += GetWeightedPercentDifference(sourceGenes.moveSpeed, otherGenes.moveSpeed, evolutionWeights.moveSpeed);
        score += GetWeightedPercentDifference(sourceGenes.turnSpeed, otherGenes.turnSpeed, evolutionWeights.turnSpeed);
        score += GetWeightedPercentDifference(sourceGenes.visionRange, otherGenes.visionRange, evolutionWeights.visionRange);
        score += GetWeightedPercentDifference(sourceGenes.maxHealth, otherGenes.maxHealth, evolutionWeights.maxHealth);
        score += GetWeightedPercentDifference(sourceGenes.maxEnergy, otherGenes.maxEnergy, evolutionWeights.maxEnergy);
        score += GetWeightedPercentDifference(sourceGenes.reproductionThreshold, otherGenes.reproductionThreshold, evolutionWeights.reproductionThreshold);
        score += GetWeightedPercentDifference(sourceGenes.reproductionCooldown, otherGenes.reproductionCooldown, evolutionWeights.reproductionCooldown);
        score += GetWeightedPercentDifference(sourceGenes.hatchDuration, otherGenes.hatchDuration, evolutionWeights.hatchDuration);
        score += GetWeightedPercentDifference(sourceGenes.attackEnergyCostNormalized, otherGenes.attackEnergyCostNormalized, evolutionWeights.attackEnergyCostNormalized);
        score += GetWeightedPercentDifference(sourceGenes.attackDamage, otherGenes.attackDamage, evolutionWeights.attackDamage);

        return score;
    }

    public bool ShouldBecomeNewSpecies(AnimalGeneSnapshot otherGenes)
    {
        return EvaluateDifferenceScore(otherGenes) > evolutionThreshold;
    }

    private void MutateMin(ref float value, float minValue)
    {
        if (!ShouldMutate())
        {
            return;
        }

        value *= 1f + Random.Range(-mutationPercent, mutationPercent);
        value = Mathf.Max(minValue, value);
    }

    private void MutateClamped(ref float value, float minValue, float maxValue)
    {
        if (!ShouldMutate())
        {
            return;
        }

        value *= 1f + Random.Range(-mutationPercent, mutationPercent);
        value = Mathf.Clamp(value, minValue, maxValue);
    }

    private bool ShouldMutate()
    {
        return mutationChance > 0f && Random.value <= mutationChance;
    }

    private static float GetWeightedPercentDifference(float source, float target, float weight)
    {
        if (weight <= 0f)
        {
            return 0f;
        }

        float baseline = Mathf.Max(Mathf.Abs(source), 0.0001f);
        return Mathf.Abs(target - source) / baseline * weight;
    }

    private void OnValidate()
    {
        bodySize = Mathf.Max(0.01f, bodySize);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        turnSpeed = Mathf.Max(0f, turnSpeed);
        visionRange = Mathf.Max(0.1f, visionRange);
        maxHealth = Mathf.Max(0.1f, maxHealth);
        maxEnergy = Mathf.Max(0.1f, maxEnergy);
        reproductionThreshold = Mathf.Clamp(reproductionThreshold, 0f, maxEnergy);
        reproductionCooldown = Mathf.Max(0f, reproductionCooldown);
        hatchDuration = Mathf.Max(0f, hatchDuration);
        energyLossK = Mathf.Max(0f, energyLossK);
        foodEnergyConvertRate = Mathf.Max(0f, foodEnergyConvertRate);
        attackDamage = Mathf.Max(0f, attackDamage);
        attackRangePerSize = Mathf.Max(0.01f, attackRangePerSize);
        attackInterval = Mathf.Max(0.01f, attackInterval);
        evolutionThreshold = Mathf.Max(0f, evolutionThreshold);
    }
}
