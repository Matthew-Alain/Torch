using UnityEngine;
using System;


public class DiceRoller
{
    public static int Roll(string diceToRoll)
    {
        string[] parts = diceToRoll.Split('d');

        int diceNumber = int.Parse(parts[0]);
        int diceSize = int.Parse(parts[1]);

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

    public static int Roll(int diceNumber, int diceSize)
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

    public static int Rolld20()
    {
        bool withAdvantage = CheckForAdvantage();
        bool withDisadvantage = CheckForDisadvantage();

        return Rolld20(withAdvantage, withDisadvantage);
    }


    public static int Rolld20(bool withAdvantage, bool withDisadvantage)
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

    public static bool CheckForAdvantage()
    {
        bool hasAdvantage = false;

        return hasAdvantage;
    }

    public static bool CheckForDisadvantage()
    {
        bool hasDisadvantage = false;

        return hasDisadvantage;
    }
}