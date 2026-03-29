using UnityEngine;

public abstract class Feature : ScriptableObject
{
    public string abilityName;
    public abstract void Activate(BaseUnit user);
}