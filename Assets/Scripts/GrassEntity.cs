using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassEntity : MonoBehaviour
{
    public MeshRenderer mesh;

    public Material newMaterial;
    public Material oldMaterial;
    public Material dryMaterial;

    public GrassType currentType;
    private GrassType newGrass, oldGrass,dryGrass;

    private float lastGrowTime;

    private void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        mesh.material = newMaterial;

        InitGrassType();
        currentType = newGrass;
        lastGrowTime = 0;
    }
    private void Update()
    {
        if (Time.time - lastGrowTime >= currentType.growInterval)
        {
            lastGrowTime = Time.time;
            GrassGrow();
        }

    }
    private void InitGrassType()
    {
        newGrass = new GrassType();
        oldGrass = new GrassType();
        dryGrass = new GrassType();

        newGrass.growInterval = 20;
        newGrass.fullfillAmount = 10;

        oldGrass.growInterval = 40;
        oldGrass.fullfillAmount = 30;

        dryGrass.growInterval = 60;
        dryGrass.fullfillAmount = 20;
    }
    private void GrassGrow()
    {
        if (currentType == newGrass)
        {
            currentType = oldGrass;
            mesh.material = oldMaterial;
        }
        else if (currentType == oldGrass)
        {
            currentType = dryGrass;
            mesh.material = dryMaterial;
        }
        else if (currentType == dryGrass)
            Destroy(gameObject); 
    }
}
public class GrassType
{
    public float growInterval;
    public float fullfillAmount;
}