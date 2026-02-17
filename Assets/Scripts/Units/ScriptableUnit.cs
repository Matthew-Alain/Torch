using System.IO.Enumeration;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Scriptable Unit")]
public class ScriptableUnit : ScriptableObject
{
    public Faction Faction;
    public BaseUnit UnitPrefab;
    public int UnitID;

}

public enum Faction
{
    PC = 0,
    Monster = 1
}