using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AnimalBase : MonoBehaviour
{
    public enum AnimalState
    {
        Idle,
        Wander,
        SearchFood,
        CarryFood,
        EatWhileMove,
        Hunt,
        Escape,
        Reproduce,
        Hatch,
        Dead
    }

    [Header("Species Data")]
    [SerializeField] protected StatSetup statSetup;

    [Header("Scene References")]
    [SerializeField] protected SpriteRenderer bodyRenderer;
    [SerializeField] protected Rigidbody2D body;
    [SerializeField] protected Transform carryAnchor;
    [SerializeField] protected GameObject meatDropPrefab;
    [SerializeField] protected float maxSingleMeatAmount = 30f;
    [SerializeField] protected float meatDropScatterRadius = 0.35f;
    [SerializeField] protected float huntMoveSpeedBonus = 0.5f;

    [Header("Detection")]
    [SerializeField] protected LayerMask animalLayer;
    [SerializeField] protected LayerMask grassLayer;
    [SerializeField] protected LayerMask meatLayer;

    [Header("Runtime")]
    [SerializeField] protected AnimalState currentState = AnimalState.Idle;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float currentEnergy;
    [SerializeField] protected float lastAttackTime = float.NegativeInfinity;
    [SerializeField] protected float lastReproductionTime = float.NegativeInfinity;
    [SerializeField] protected GameObject carriedFood;
    [SerializeField] protected bool destroyOnDeath = true;
    [SerializeField] protected float pickupRangeMultiplier = 0.6f;
    [SerializeField] protected float carryMoveSpeedMultiplier = 0.6f;
    [SerializeField] protected Vector2 wanderDirection = Vector2.right;
    [SerializeField] protected float wanderRetargetMinTime = 1.5f;
    [SerializeField] protected float wanderRetargetMaxTime = 3f;
    [SerializeField] protected float nextWanderRetargetTime;
    [SerializeField] protected GameObject currentFoodTarget;
    [SerializeField] protected AnimalBase currentHuntTarget;
    [SerializeField] protected AnimalBase offspringSourcePrefab;
    [SerializeField] [Range(0.01f, 1f)] protected float maturityNormalized = 1f;
    [SerializeField] protected bool registeredWithGenerator;

    protected AnimalSpawnEntry sourceSpawnEntry;
    protected AnimalSpawnEntry registeredSpawnEntry;

    public StatSetup Setup => statSetup;
    public int SpeciesID => statSetup != null ? statSetup.speciesID : -1;
    public DietType DietType => statSetup != null ? statSetup.GetDietType() : DietType.Omnivore;
    public AnimalState CurrentState => currentState;
    public float MaxHealth => statSetup != null ? statSetup.maxHealth : 0f;
    public float MaxEnergy => statSetup != null ? statSetup.maxEnergy : 0f;
    public float VisionRange => statSetup != null ? statSetup.visionRange : 0f;
    public float AttackRange => statSetup != null ? Mathf.Max(0.01f, GetCurrentBodySize() * statSetup.attackRangePerSize) : 0f;
    public float NormalizedEnergy => MaxEnergy <= 0f ? 0f : currentEnergy / MaxEnergy;
    public float MaturityNormalized => maturityNormalized;
    public bool IsMature => maturityNormalized >= 1f;
    public bool IsDead => currentState == AnimalState.Dead;
    public bool IsHoldingFood => carriedFood != null;

    protected virtual void Reset()
    {
        CacheReferences();
    }

    protected virtual void Awake()
    {
        CacheReferences();
        InitializeFromSetup();
    }

    protected virtual void Update()
    {
        if (statSetup == null || IsDead)
        {
            return;
        }

        TickPassiveEnergy(Time.deltaTime);
        if (IsDead)
        {
            return;
        }

        TickMaturity(Time.deltaTime);
        HandleBehaviour(Time.deltaTime);
        SyncVisuals();
    }

    public virtual void InitializeFromSetup()
    {
        if (statSetup == null)
        {
            return;
        }

        RegisterSpeciesSetup();
        currentHealth = statSetup.maxHealth;
        currentEnergy = statSetup.maxEnergy;
        currentState = AnimalState.Hatch;
        carriedFood = null;
        currentFoodTarget = null;
        currentHuntTarget = null;
        maturityNormalized = Mathf.Clamp(statSetup.newbornMaturityNormalized, 0.01f, 1f);
        lastAttackTime = float.NegativeInfinity;
        lastReproductionTime = float.NegativeInfinity;
        PickNewWanderDirection(true);
        RegisterWithGenerator();
        SyncVisuals();
    }

    public virtual void InitializeFromSetup(StatSetup setup)
    {
        InitializeFromSetup(setup, null, null);
    }

    public virtual void InitializeFromSetup(StatSetup setup, AnimalBase sourcePrefab)
    {
        InitializeFromSetup(setup, null, sourcePrefab);
    }

    public virtual void InitializeFromSetup(StatSetup setup, AnimalSpawnEntry spawnEntry, AnimalBase sourcePrefab)
    {
        if (registeredWithGenerator)
        {
            UnregisterFromGenerator();
        }

        sourceSpawnEntry = spawnEntry;
        offspringSourcePrefab = sourcePrefab;
        statSetup = setup;
        InitializeFromSetup();
    }

    public virtual void InitializeAsOffspring(StatSetup setup, AnimalBase sourcePrefab)
    {
        InitializeFromSetup(setup, null, sourcePrefab);
    }

    public virtual void InitializeAsOffspring(StatSetup setup, AnimalSpawnEntry spawnEntry, AnimalBase sourcePrefab)
    {
        InitializeFromSetup(setup, spawnEntry, sourcePrefab);
    }

    protected virtual void CacheReferences()
    {
        if (bodyRenderer == null)
        {
            bodyRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }
    }

    protected virtual void HandleBehaviour(float deltaTime)
    {
        if (TryHandleEscape(deltaTime))
        {
            return;
        }

        if (IsHoldingFood)
        {
            currentState = AnimalState.EatWhileMove;
            ConsumeCarriedFood(deltaTime);
            if (!IsDead && IsHoldingFood)
            {
                Wander(carryMoveSpeedMultiplier, deltaTime);
            }

            return;
        }

        if (TryHandleReproduction())
        {
            return;
        }

        if (CanSeekFood())
        {
            if (DietType == DietType.Carnivore)
            {
                if (TryHandleFoodSearch(deltaTime))
                {
                    return;
                }

                if (TryHandleHunt(deltaTime))
                { 
                    return;
                }
            }
            else if (DietType == DietType.Omnivore)
            {
                if (TryHandleFoodSearch(deltaTime))
                {
                    return;
                }

                if (TryHandleHunt(deltaTime))
                {
                    return;
                }
            }
            else
            {
                if (TryHandleFoodSearch(deltaTime))
                {
                    return;
                }
            }
        }

        currentFoodTarget = null;
        currentHuntTarget = null;
        currentState = AnimalState.Wander;
        Wander(1f, deltaTime);
    }

    protected virtual void TickPassiveEnergy(float deltaTime)
    {
        SpendEnergy(statSetup.GetPassiveEnergyCostPerSecond() * deltaTime);
    }

    protected virtual void TickMaturity(float deltaTime)
    {
        if (statSetup == null || IsMature)
        {
            return;
        }

        maturityNormalized = Mathf.Min(1f, maturityNormalized + statSetup.maturityGrowthPerSecond * deltaTime);
    }

    public virtual bool CanSeekFood()
    {
        return statSetup != null && !IsDead && currentEnergy < MaxEnergy * statSetup.seekFoodThresholdNormalized;
    }

    public virtual bool CanReproduce()
    {
        if (statSetup == null || IsDead)
        {
            return false;
        }

        if (Time.time < lastReproductionTime + statSetup.reproductionCooldown)
        {
            return false;
        }

        if (!IsMature)
        {
            return false;
        }

        return currentEnergy >= statSetup.reproductionThreshold && currentEnergy >= statSetup.GetReproductionCost();
    }

    public virtual bool CanAttackNow()
    {
        return statSetup != null && !IsDead && Time.time >= lastAttackTime + statSetup.attackInterval;
    }

    public virtual bool IsSameSpecies(AnimalBase other)
    {
        return other != null && SpeciesID >= 0 && SpeciesID == other.SpeciesID;
    }

    public virtual bool CanHuntTarget(AnimalBase target)
    {
        if (statSetup == null || target == null || target == this || target.IsDead)
        {
            return false;
        }

        if (DietType == DietType.Herbivore)
        {
            return false;
        }

        if (IsSameSpecies(target) && !statSetup.CanHuntSameSpecies(currentEnergy))
        {
            return false;
        }

        return true;
    }

    public virtual bool ShouldFleeFrom(AnimalBase other)
    {
        if (other == null || other == this || other.IsDead)
        {
            return false;
        }

        if (DietType == DietType.Herbivore && (other.DietType == DietType.Carnivore || other.DietType == DietType.Omnivore))
        {
            return true;
        }

        return false;
    }

    public virtual bool TryAttack(AnimalBase target)
    {
        if (!CanHuntTarget(target) || !CanAttackNow())
        {
            return false;
        }

        Vector2 toTarget = target.transform.position - transform.position;
        if (toTarget.sqrMagnitude > AttackRange * AttackRange)
        {
            return false;
        }

        if (!SpendEnergy(statSetup.GetAttackEnergyCost()))
        {
            return false;
        }

        lastAttackTime = Time.time;
        target.ReceiveDamage(statSetup.attackDamage);
        return true;
    }

    public virtual void ReceiveDamage(float damageAmount)
    {
        if (IsDead || damageAmount <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damageAmount);
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public virtual void RestoreEnergy(float amount)
    {
        if (IsDead || amount <= 0f)
        {
            return;
        }

        currentEnergy = Mathf.Min(MaxEnergy, currentEnergy + amount);
    }

    public virtual void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
    }

    protected virtual bool SpendEnergy(float amount)
    {
        if (IsDead)
        {
            return false;
        }

        if (amount <= 0f)
        {
            return true;
        }

        currentEnergy = Mathf.Max(0f, currentEnergy - amount);
        if (currentEnergy <= 0f)
        {
            Die();
            return false;
        }

        return true;
    }

    public virtual bool TryPickFood(GameObject foodObject)
    {
        if (foodObject == null || carriedFood != null)
        {
            return false;
        }

        carriedFood = foodObject;
        Transform parentTarget = carryAnchor != null ? carryAnchor : transform;
        carriedFood.transform.SetParent(parentTarget);
        carriedFood.transform.localPosition = carryAnchor != null ? Vector3.zero : (Vector3)(GetViewDirection().normalized * GetPickupRange());

        currentState = AnimalState.CarryFood;
        return true;
    }

    public virtual GameObject ReleaseFood()
    {
        if (carriedFood == null)
        {
            return null;
        }

        GameObject releasedFood = carriedFood;
        if (carryAnchor != null && releasedFood.transform.parent == carryAnchor)
        {
            releasedFood.transform.SetParent(null);
        }
        else if (releasedFood.transform.parent == transform)
        {
            releasedFood.transform.SetParent(null);
        }

        releasedFood.transform.position = transform.position + (Vector3)(GetViewDirection().normalized * GetPickupRange());
        carriedFood = null;
        return releasedFood;
    }

    public virtual bool CanEatFood(GameObject foodObject)
    {
        if (foodObject == null)
        {
            return false;
        }

        if (foodObject.CompareTag("meat"))
        {
            return DietType != DietType.Herbivore;
        }

        if (foodObject.CompareTag("grass"))
        {
            return DietType != DietType.Carnivore;
        }

        return false;
    }

    public virtual int GetFoodPriority(GameObject foodObject)
    {
        if (foodObject == null)
        {
            return int.MaxValue;
        }

        bool isMeat = foodObject.CompareTag("meat");
        bool isGrass = foodObject.CompareTag("grass");

        switch (DietType)
        {
            case DietType.Carnivore:
                return isMeat ? 0 : int.MaxValue;
            case DietType.Herbivore:
                return isGrass ? 0 : int.MaxValue;
            default:
                if (isMeat)
                {
                    return 0;
                }

                if (isGrass)
                {
                    return 1;
                }

                return int.MaxValue;
        }
    }

    public virtual List<AnimalBase> ScanNearbyAnimals()
    {
        List<AnimalBase> animals = new List<AnimalBase>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, VisionRange, animalLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            AnimalBase animal = hits[i].GetComponent<AnimalBase>();
            if (animal != null && animal != this)
            {
                animals.Add(animal);
            } 
        }
         
        return animals;
    }

    public virtual List<GameObject> ScanVisibleFoods()
    {
        List<GameObject> visibleFoods = new List<GameObject>();
        int combinedMask = grassLayer.value | meatLayer.value;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, VisionRange, combinedMask);
        for (int i = 0; i < hits.Length; i++)
        {
            GameObject candidate = hits[i].gameObject;
            if (!CanEatFood(candidate))
            {
                continue;
            }

            if (IsInFoodView(candidate.transform.position))
            {
                visibleFoods.Add(candidate);
            }
        }

        return visibleFoods;
    }

    public virtual bool IsInFoodView(Vector3 targetPosition)
    {
        if (statSetup == null)
        {
            return false;
        }

        Vector2 forward = GetViewDirection();
        Vector2 direction = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
        float angle = Vector2.Angle(forward, direction);
        return angle <= statSetup.foodSearchAngle * 0.5f;
    }

    protected virtual Vector2 GetViewDirection()
    {
        return transform.right;
    }

    public virtual Color EvaluateDietColor()
    {
        float value = statSetup != null ? statSetup.dietHabit : 0.5f;
        if (value <= 0.5f)
        {
            return Color.Lerp(Color.red, Color.blue, value / 0.5f);
        }

        return Color.Lerp(Color.blue, Color.green, (value - 0.5f) / 0.5f);
    }

    protected virtual void SyncVisuals()
    {
        if (bodyRenderer != null)
        {
            bodyRenderer.color = EvaluateDietColor();
        }

        if (statSetup != null)
        {
            transform.localScale = Vector3.one * GetCurrentBodySize();
        }
    }

    protected virtual void Die()
    {
        if (IsDead)
        {
            return;
        }

        currentHealth = 0f;
        currentEnergy = 0f;
        currentState = AnimalState.Dead;
        ReleaseFood();
        currentFoodTarget = null;
        currentHuntTarget = null;
        UnregisterFromGenerator();
        SpawnDeathDrop();
        OnDeath();

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void SpawnDeathDrop()
    {
        if (meatDropPrefab == null || statSetup == null)
        {
            return;
        }

        float totalDropAmount = MaxEnergy;
        float chunkLimit = Mathf.Max(0.01f, maxSingleMeatAmount);
        int dropCount = Mathf.CeilToInt(totalDropAmount / chunkLimit);

        for (int i = 0; i < dropCount; i++)
        {
            float chunkAmount = Mathf.Min(chunkLimit, totalDropAmount);
            Vector3 spawnPosition = transform.position + GetDeathDropOffset(i, dropCount);
            GameObject meatObject = Instantiate(meatDropPrefab, spawnPosition, Quaternion.identity);
            MeatBase meat = meatObject.GetComponent<MeatBase>();
            if (meat != null)
            {
                meat.meatMaxAmount = chunkAmount;
                meat.InitMeatState();
            }

            totalDropAmount -= chunkAmount;
        }
    }

    protected virtual void OnDeath()
    {
    }

    public virtual AnimalGeneSnapshot BuildOffspringGenes()
    {
        return statSetup != null ? statSetup.CreateMutatedGenes() : default(AnimalGeneSnapshot);
    }

    public virtual float EvaluateSpeciesDifference(StatSetup otherSpecies)
    {
        return statSetup != null ? statSetup.EvaluateDifferenceScore(otherSpecies) : float.MaxValue;
    }

    public virtual float EvaluateSpeciesDifference(AnimalGeneSnapshot genes)
    {
        return statSetup != null ? statSetup.EvaluateDifferenceScore(genes) : float.MaxValue;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (statSetup == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, statSetup.visionRange);

        Vector2 forward = GetViewDirection();
        float halfAngle = statSetup.foodSearchAngle * 0.5f;
        Vector3 leftDirection = Quaternion.Euler(0f, 0f, -halfAngle) * forward;
        Vector3 rightDirection = Quaternion.Euler(0f, 0f, halfAngle) * forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftDirection * statSetup.visionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightDirection * statSetup.visionRange);
    }

    protected virtual void Wander(float speedMultiplier, float deltaTime)
    {
        PickNewWanderDirection(false);
        MoveAlong(wanderDirection, statSetup.moveSpeed * speedMultiplier, deltaTime);
    }

    protected virtual void PickNewWanderDirection(bool force)
    {
        if (!force && Time.time < nextWanderRetargetTime)
        {
            return;
        }

        wanderDirection = Random.insideUnitCircle.normalized;
        if (wanderDirection == Vector2.zero)
        {
            wanderDirection = transform.right;
        }

        nextWanderRetargetTime = Time.time + Random.Range(wanderRetargetMinTime, wanderRetargetMaxTime);
    }

    protected virtual void MoveTowards(Vector3 worldPosition, float speed, float deltaTime)
    {
        Vector2 direction = ((Vector2)worldPosition - (Vector2)transform.position).normalized;
        MoveAlong(direction, speed, deltaTime);
    }

    protected virtual void MoveAwayFrom(Vector3 worldPosition, float speed, float deltaTime)
    {
        Vector2 direction = ((Vector2)transform.position - (Vector2)worldPosition).normalized;
        MoveAlong(direction, speed, deltaTime);
    }

    protected virtual void MoveAlong(Vector2 direction, float speed, float deltaTime)
    {
        if (direction == Vector2.zero)
        {
            return;
        }

        RotateTowards(direction, deltaTime);
        Vector2 moveDirection = GetViewDirection().normalized;
        Vector2 nextPosition = (Vector2)transform.position + moveDirection * Mathf.Max(0f, speed) * deltaTime;

        if (body != null)
        {
            body.MovePosition(nextPosition);
        }
        else
        {
            transform.position = nextPosition;
        }
    }

    protected virtual void RotateTowards(Vector2 direction, float deltaTime)
    {
        if (direction == Vector2.zero || statSetup == null)
        {
            return;
        }

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, statSetup.turnSpeed * deltaTime);
    }

    protected virtual float GetPickupRange()
    {
        return Mathf.Max(0.2f, GetCurrentBodySize() * pickupRangeMultiplier);
    }

    protected virtual bool IsValidFoodTarget(GameObject foodObject)
    {
        return foodObject != null && CanEatFood(foodObject) && HasFoodAmount(foodObject);
    }

    protected virtual bool IsValidHuntTarget(AnimalBase target)
    {
        return target != null && CanHuntTarget(target) && IsInFoodView(target.transform.position);
    }

    protected virtual bool TryHandleReproduction()
    {
        if (!CanReproduce() || IsHoldingFood)
        {
            return false;
        }

        AnimalGeneSnapshot offspringGenes = BuildOffspringGenes();
        if (!SpendEnergy(statSetup.GetReproductionCost()))
        {
            return true;
        }

        lastReproductionTime = Time.time;
        currentState = AnimalState.Reproduce;
        currentFoodTarget = null;
        currentHuntTarget = null;
        SpawnEgg(offspringGenes);
        PickNewWanderDirection(true);
        return true;
    }

    protected virtual bool TryHandleEscape(float deltaTime)
    {
        AnimalBase threat = FindPriorityThreat();
        if (threat == null)
        {
            return false;
        }

        currentState = AnimalState.Escape;
        currentFoodTarget = null;
        currentHuntTarget = null;
        if (IsHoldingFood)
        {
            ReleaseFood();
        }

        MoveAwayFrom(threat.transform.position, statSetup.moveSpeed, deltaTime);
        return true;
    }

    protected virtual bool TryHandleFoodSearch(float deltaTime)
    {
        if (!IsValidFoodTarget(currentFoodTarget))
        {
            currentFoodTarget = FindBestFoodTarget();
        }

        if (currentFoodTarget == null)
        {
            return false;
        }

        currentHuntTarget = null;
        currentState = AnimalState.SearchFood;
        MoveTowards(currentFoodTarget.transform.position, statSetup.moveSpeed, deltaTime);

        if (Vector2.Distance(transform.position, currentFoodTarget.transform.position) <= GetPickupRange())
        {
            TryPickFood(currentFoodTarget);
            currentFoodTarget = null;
        }

        return true;
    }

    protected virtual bool TryHandleHunt(float deltaTime)
    {
        if (!IsValidHuntTarget(currentHuntTarget))
        {
            currentHuntTarget = FindBestHuntTarget();
        }

        if (currentHuntTarget == null)
        {
            return false;
        }

        currentFoodTarget = null;
        currentState = AnimalState.Hunt;
        float distanceToTarget = Vector2.Distance(transform.position, currentHuntTarget.transform.position);
        if (distanceToTarget <= AttackRange)
        {
            TryAttack(currentHuntTarget);
        }
        else
        {
            MoveTowards(currentHuntTarget.transform.position, GetCurrentMoveSpeed(), deltaTime);
        }

        return true;
    }

    protected virtual GameObject FindBestFoodTarget()
    {
        List<GameObject> visibleFoods = ScanVisibleFoods();
        GameObject bestFood = null;
        int bestPriority = int.MaxValue;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < visibleFoods.Count; i++)
        {
            GameObject candidate = visibleFoods[i];
            int priority = GetFoodPriority(candidate);
            if (priority == int.MaxValue)
            {
                continue;
            }

            float distance = ((Vector2)candidate.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (priority < bestPriority || (priority == bestPriority && distance < bestDistance))
            {
                bestPriority = priority;
                bestDistance = distance;
                bestFood = candidate;
            }
        }

        return bestFood;
    }

    protected virtual AnimalBase FindBestHuntTarget()
    {
        List<AnimalBase> nearbyAnimals = ScanNearbyAnimals();
        AnimalBase bestTarget = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < nearbyAnimals.Count; i++)
        {
            AnimalBase candidate = nearbyAnimals[i];
            if (!IsValidHuntTarget(candidate))
            {
                continue;
            }

            float distance = ((Vector2)candidate.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = candidate;
            }
        }

        return bestTarget;
    }

    protected virtual AnimalBase FindPriorityThreat()
    {
        if (DietType != DietType.Herbivore)
        {
            return null;
        }

        List<AnimalBase> nearbyAnimals = ScanNearbyAnimals();
        AnimalBase nearestCarnivore = null;
        float nearestCarnivoreDistance = float.MaxValue;
        float totalCarnivoreHealth = 0f;
        AnimalBase nearestOmnivore = null;
        float nearestOmnivoreDistance = float.MaxValue;
        int omnivoreCount = 0;

        for (int i = 0; i < nearbyAnimals.Count; i++)
        {
            AnimalBase candidate = nearbyAnimals[i];
            if (!ShouldFleeFrom(candidate))
            {
                continue;
            }

            float distance = ((Vector2)candidate.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (candidate.DietType == DietType.Carnivore)
            {
                totalCarnivoreHealth += candidate.MaxHealth;
                if (distance < nearestCarnivoreDistance)
                {
                    nearestCarnivoreDistance = distance;
                    nearestCarnivore = candidate;
                }
            }
            else if (candidate.DietType == DietType.Omnivore)
            {
                omnivoreCount++;
                if (distance < nearestOmnivoreDistance)
                {
                    nearestOmnivoreDistance = distance;
                    nearestOmnivore = candidate;
                }
            }
        }

        if (nearestCarnivore != null && totalCarnivoreHealth >= MaxHealth * 0.5f)
        {
            return nearestCarnivore;
        }

        if (nearestOmnivore != null && omnivoreCount >= 3)
        {
            return nearestOmnivore;
        }

        return null;
    }

    protected virtual void ConsumeCarriedFood(float deltaTime)
    {
        if (carriedFood == null || statSetup == null)
        {
            return;
        }

        float requestedAmount = statSetup.foodEnergyConvertRate * deltaTime;
        float receivedAmount = ConsumeFood(carriedFood, requestedAmount, out bool depleted);
        RestoreEnergy(receivedAmount);

        if (depleted)
        {
            Destroy(carriedFood);
            carriedFood = null;
            currentState = AnimalState.Wander;
            return;
        }

        if (currentEnergy >= MaxEnergy)
        {
            ReleaseFood();
            currentState = AnimalState.Wander;
        }
    }

    protected virtual float ConsumeFood(GameObject foodObject, float amount, out bool depleted)
    {
        depleted = false;
        if (foodObject == null || amount <= 0f)
        {
            return 0f;
        }

        MeatBase meat = foodObject.GetComponent<MeatBase>();
        if (meat != null)
        {
            float consumed = meat.Consume(amount);
            depleted = meat.IsDepleted();
            return consumed;
        }

        GrassBase grass = foodObject.GetComponent<GrassBase>();
        if (grass != null)
        {
            float consumed = grass.Consume(amount);
            depleted = grass.IsDepleted();
            return consumed;
        }

        return 0f;
    }

    protected virtual bool HasFoodAmount(GameObject foodObject)
    {
        if (foodObject == null)
        {
            return false;
        }

        MeatBase meat = foodObject.GetComponent<MeatBase>();
        if (meat != null)
        {
            return !meat.IsDepleted();
        }

        GrassBase grass = foodObject.GetComponent<GrassBase>();
        if (grass != null)
        {
            return !grass.IsDepleted();
        }

        return false;
    }

    protected virtual Vector3 GetDeathDropOffset(int index, int totalCount)
    {
        if (totalCount <= 1 || meatDropScatterRadius <= 0f)
        {
            return Vector3.zero;
        }

        float angleStep = 360f / totalCount;
        float angle = angleStep * index;
        Vector2 offset = Quaternion.Euler(0f, 0f, angle) * Vector2.right * meatDropScatterRadius;
        return offset;
    }

    protected virtual void SpawnEgg(AnimalGeneSnapshot offspringGenes)
    {
        GameObject eggObject = CreateEggObject();
        if (eggObject == null)
        {
            return;
        }

        EggBase egg = eggObject.GetComponent<EggBase>();
        if (egg == null)
        {
            egg = eggObject.AddComponent<EggBase>();
        }

        egg.Initialize(statSetup, offspringGenes, ResolveOffspringPrefab());
    }

    protected virtual GameObject CreateEggObject()
    {
        Vector3 spawnPosition = transform.position + (Vector3)(GetViewDirection().normalized * Mathf.Max(0.25f, statSetup.bodySize * 0.5f));
        if (statSetup != null && statSetup.eggPrefab != null)
        {
            return Instantiate(statSetup.eggPrefab, spawnPosition, Quaternion.identity);
        }

        GameObject eggObject = new GameObject(string.Format("{0} Egg", statSetup != null ? statSetup.speciesName : "Animal"));
        eggObject.transform.position = spawnPosition;
        return eggObject;
    }

    protected virtual AnimalBase ResolveOffspringPrefab()
    {
        if (offspringSourcePrefab != null)
        {
            return offspringSourcePrefab;
        }

        if (statSetup != null && statSetup.animalPrefab != null)
        {
            return statSetup.animalPrefab;
        }

        return null;
    }

    protected virtual float GetCurrentMoveSpeed()
    {
        float baseSpeed = statSetup != null ? statSetup.moveSpeed : 0f;
        if (currentState == AnimalState.Hunt)
        {
            return baseSpeed + huntMoveSpeedBonus;
        }

        return baseSpeed;
    }

    protected virtual float GetCurrentBodySize()
    {
        if (statSetup == null)
        {
            return 0f;
        }

        return statSetup.bodySize * Mathf.Clamp01(maturityNormalized);
    }

    protected virtual void RegisterSpeciesSetup()
    {
        if (statSetup != null)
        {
            SpeciesRegistry.Instance.RegisterSpecies(statSetup);
        }
    }

    protected virtual void RegisterWithGenerator()
    {
        if (registeredWithGenerator)
        {
            return;
        }

        PandoraGenerator generator = PandoraGenerator.Instance;
        if (generator != null)
        {
            registeredSpawnEntry = generator.RegisterAnimal(this, sourceSpawnEntry);
            registeredWithGenerator = registeredSpawnEntry != null;
        }
    }

    protected virtual void UnregisterFromGenerator()
    {
        if (!registeredWithGenerator)
        {
            return;
        }

        PandoraGenerator generator = PandoraGenerator.Instance;
        if (generator != null)
        {
            generator.UnregisterAnimal(this, registeredSpawnEntry);
        }

        registeredSpawnEntry = null;
        registeredWithGenerator = false;
    }

    protected virtual void OnDestroy()
    {
        UnregisterFromGenerator();
    }
}
