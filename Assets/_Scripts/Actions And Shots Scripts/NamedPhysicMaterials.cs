using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NamedPhysicMaterials
{
    public string Name;
    public PhysicMaterial PhysicMaterial;

    public static PhysicMaterial GetPhysicMaterialByName(List<NamedPhysicMaterials> namedPhysicMaterials, string name)
    {
        foreach (NamedPhysicMaterials namedPhysicMaterial in namedPhysicMaterials)
        {
            if (namedPhysicMaterial.Name == name)
            {
                return namedPhysicMaterial.PhysicMaterial;
            }
        }

        return null;
    }
}
