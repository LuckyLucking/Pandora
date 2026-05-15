using System.Collections;
using UnityEngine;

public class GrassEntity : MonoBehaviour
{
    public MeshRenderer mesh;

    public Material newMaterial;
    public Material oldMaterial;
    public Material dryMaterial;

    public GrassType currentType;
    private GrassType newGrass, oldGrass, dryGrass;

    public float grassFullfillAmount = 10f;

    private void Awake()
    {
        if (mesh == null)
        {
            mesh = GetComponent<MeshRenderer>();
        }
    }

    private void Start()
    {
        InitGrassType(); 
        currentType = newGrass;
        ApplyMaterial(newMaterial);
    }

    public void GrassGrow()
    {
        if (currentType == newGrass)
        {
            currentType = oldGrass;
            ApplyMaterial(oldMaterial);
        }
        else if (currentType == oldGrass)
        {
            currentType = dryGrass;
            ApplyMaterial(dryMaterial);
        }
        else if (currentType == dryGrass)
        {
            currentType = newGrass;
            ApplyMaterial(newMaterial);
        }
    }

    private void InitGrassType()
    {
        newGrass = new GrassType();
        oldGrass = new GrassType();
        dryGrass = new GrassType();

        newGrass.fullfillAmount = grassFullfillAmount;

        oldGrass.fullfillAmount = grassFullfillAmount * 3f;

        dryGrass.fullfillAmount = grassFullfillAmount * 2f;
    }

    private void ApplyMaterial(Material material)
    {
        if (mesh == null || material == null)
        {
            return;
        }

        mesh.sharedMaterial = material;
    }
}

public class GrassType
{
    public float fullfillAmount;
}
