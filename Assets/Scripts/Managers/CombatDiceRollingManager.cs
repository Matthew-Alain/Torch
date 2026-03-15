using System;
using TMPro;
using UnityEngine;

public class CombatDiceRollingManager : MonoBehaviour
{
    [SerializeField] private GameObject diceRollInfo, diceRollStatus;
    private int PCID;

    void Awake()
    {
        PCID = DatabaseManager.Instance.lastPCEdited;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void RollToHit(int attackID, int targetID)
    {

        int dieRoll = Rolld20(CheckForAdvantage(), CheckForDisadvantage());

        int mod = 0;
        DatabaseManager.Instance.ExecuteReader(
            "SELECT hit_mod FROM attacks WHERE id = @attackID",
            reader =>
            {
                mod = GetModifier(Convert.ToString(reader["hit_mod"]));
            },
                ("@attackID", attackID)
        );
        
        
        int prof = GetProficiency("all_simple");

        int AC = GetAC(0);

        bool hit = CalculateResult(dieRoll, mod, prof, AC);

        if (hit)
        {
            DoDamage(attackID, targetID, 0);
        }
    }

    void DoDamage(int attackID, int affectedUnitID, int damageBonus)
    {
        //Look up attackID damage dice
        string damageRoll = "2d8";

        string[] splitRoll = damageRoll.Split('d');

        int damage = RollDice(Convert.ToInt32(splitRoll[0]), Convert.ToInt32(splitRoll[1]), damageBonus);

        //reduce HP of affected unit
    }

    int RollDice(int diceNumber, int diceSize, int bonus)
    {
        int total = 0;

        for (int i = 1; i <= diceNumber; i++)
        {
            int result = UnityEngine.Random.Range(1, diceSize + 1);
            Debug.Log("Roll " + i + "/" + diceNumber + ": " + result);
            total += result;
        }
        Debug.Log("For a total of: " + total);

        return total;
    }

    bool CheckForAdvantage()
    {
        bool hasAdvantage = false;

        return hasAdvantage;
    }

    bool CheckForDisadvantage()
    {
        bool hasDisadvantage = false;

        return hasDisadvantage;
    }

    int Rolld20(bool withAdvantage, bool withDisadvantage)
    {
        int dieRoll;

        if (withAdvantage && !withDisadvantage)
        {
            int roll1 = UnityEngine.Random.Range(1, 20 + 1);
            int roll2 = UnityEngine.Random.Range(1, 20 + 1);
            Debug.Log("Roll 1: " + roll1 + ", Roll2: " + roll2);
            dieRoll = Math.Max(roll1, roll2);
        }
        else if (withDisadvantage && !withAdvantage)
        {
            int roll1 = UnityEngine.Random.Range(1, 20 + 1);
            int roll2 = UnityEngine.Random.Range(1, 20 + 1);
            Debug.Log("Roll 1: " + roll1 + ", Roll2: " + roll2);
            dieRoll = Math.Min(roll1, roll2);
        }
        else
        {
            dieRoll = UnityEngine.Random.Range(1, 20 + 1);
        }
        Debug.Log("Final die roll: " + dieRoll);

        //Check for automatic rerolls (halfling luck)

        return dieRoll;
    }

    int GetModifier(string mod)
    {
        int modifier = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
            "SELECT @mod FROM pc_stats WHERE id = (@PCID)",
            ("@mod", mod),
            ("@PCID", PCID)
        ));
        Debug.Log("Modifier: " + modifier);

        return modifier;
    }

    int GetProficiency(string roll)
    {
        bool isProficient = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar(
            "SELECT @column_name FROM pc_proficiencies WHERE id = (@PCID)",
            ("@column_name", roll),
            ("@PCID", PCID)
        ));

        if (isProficient)
        {
            int pcLevel = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
                "SELECT level FROM saved_pcs WHERE id = (@PCID)",
                ("@PCID", PCID)
            ));

            if (pcLevel == 5)
            {
                return 3;
            }
            else
            {
                return 2;
            }
        }

        Debug.Log("Not proficient");
        return 0;
    }
    
    int GetAC(int unitID)
    {
        
        return 10;
    }

    bool CalculateResult(int dieResult, int modifier, int proficiency, int DC)
    {
        string modifierText;

        if (modifier >= 0)
        {
            modifierText = "+" + modifier;
        }
        else
        {
            modifierText = "" + modifier;
        }

        int total = dieResult + modifier + proficiency;

        diceRollInfo.GetComponentInChildren<TMP_Text>().text = "You rolled: " + dieResult +
        ", with a modifier of " + modifierText +
        ", and a proficiency bonus of +" + proficiency +
        ", for a total of " + total;
        
        if(total >= DC)
        {
            return UpdateRollStatus(true);
        }
        else
        {
            return UpdateRollStatus(false);
        }
    }
    
    bool UpdateRollStatus(bool success)
    {
        if (success)
        {
            diceRollStatus.GetComponentInChildren<TMP_Text>().text = "Success!";
            return true;
        }
        else
        {
            diceRollStatus.GetComponentInChildren<TMP_Text>().text = "Failed...";
            bool nowSucceeds = false;
            
            //Check for rerolls (bardic inspiration, shield spell, etc.)

            return nowSucceeds;
        }
    }
}
