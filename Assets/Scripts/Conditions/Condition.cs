using System;

public class Condition
{
    static DatabaseManager db = DatabaseManager.Instance;
    public int conditionID;
    public string conditionName;
    public BaseUnit source, target;
    public int saveDC;
    public int turnsLeft;
    public string saveType;
    public EndCondition endCondition;

    public Condition(int conditionID, BaseUnit source, BaseUnit target, int saveDC, EndCondition endCondition)
    {
        this.conditionID = conditionID;
        this.source = source;
        this.target = target;
        this.saveDC = saveDC;
        this.endCondition = endCondition;
        
    }
    
    public void ApplyCondition()
    {
        new Condition(1, CombatUnitManager.Instance.GetUnitByID(0), CombatUnitManager.Instance.GetUnitByID(1), 1, global::EndCondition.Save_Ends);
    }
    
    public void Init()
    {

        conditionName = Convert.ToString(db.ExecuteScalar($"SELECT name FROM conditions_info WHERE id = {conditionID}"));
    }

    public void EndCondition()
    {
        db.ExecuteNonQuery($"DELETE FROM active_conditions WHERE condition_id = {conditionID} AND source_unit_id = {source.UnitID} AND target_unit_id = {target.UnitID}");
    }


}


public enum EndCondition
{
    Save_Ends,
    Source_Starts_Turn,
    Source_Ends_Turn,
    Target_Starts_Turn,
    Target_Ends_Turn,
    Duration_Expires,
    Moves

}