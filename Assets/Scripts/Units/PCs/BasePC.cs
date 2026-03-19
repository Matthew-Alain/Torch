using System;
using Unity.VisualScripting;
using UnityEngine;

public class BasePC : BaseUnit
{
    public string GetClassName()
    {
        int class_id = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT dnd_class_1 FROM saved_pcs WHERE id = {UnitID}"));
        return Convert.ToString(DatabaseManager.Instance.ExecuteScalar($"SELECT name FROM dndclasses WHERE id = {class_id}"));
    }

    public int GetClassID()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT dnd_class_1 FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int GetSpecies()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT species FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int GetOriginFeat()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT origin_feat FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int GetMainhandID()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT main_hand_item FROM saved_pcs WHERE id = {UnitID}"));
    }

    public string GetMainhandName()
    {
        return Convert.ToString($"SELECT name FROM weapons WHERE id = {GetMainhandID()}");
    }

    public int GetOffhandID()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT off_hand_item FROM saved_pcs WHERE id = {UnitID}"));
    }

    public string GetOffhandName()
    {
        return Convert.ToString($"SELECT name FROM weapons WHERE id = {GetOffhandID()}");
    }

    public int GetArmorID()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT equipped_armor FROM saved_pcs WHERE id = {UnitID}"));
    }
    
    public string GetArmorName()
    {
        return Convert.ToString($"SELECT name FROM weapons WHERE id = {GetArmorID()}");
    }

}
