using System;
using TMPro;
using UnityEngine;

public class CombatDiceRollingManager : MonoBehaviour
{
    [SerializeField] private static GameObject diceRollInfo, diceRollStatus;

    public static bool CalculateResult(int dieResult, int modifier, int proficiency, int DC)
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
    
    public static bool UpdateRollStatus(bool success)
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
