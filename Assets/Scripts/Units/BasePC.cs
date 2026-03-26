using System;
using System.Collections.Generic;
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
        return Convert.ToString(DatabaseManager.Instance.ExecuteScalar($"SELECT name FROM weapons WHERE id = {GetMainhandID()}"));
    }

    public int GetOffhandID()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT off_hand_item FROM saved_pcs WHERE id = {UnitID}"));
    }

    public string GetOffhandName()
    {
        return Convert.ToString(DatabaseManager.Instance.ExecuteScalar($"SELECT name FROM weapons WHERE id = {GetOffhandID()}"));
    }

    public int GetArmorID()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT equipped_armor FROM saved_pcs WHERE id = {UnitID}"));
    }

    public string GetArmorName()
    {
        return Convert.ToString($"SELECT name FROM weapons WHERE id = {GetArmorID()}");
    }

    public int GetSubclass()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT subclass FROM saved_pcs WHERE id = {UnitID}"));
    }

    public bool IsSpellcaster()
    {
        return GetClassName() == "Bard" || GetClassName() == "Cleric" || GetClassName() == "Druid" || (GetClassName() == "Fighter" && GetSubclass() == 2) ||
        GetClassName() == "Paladin" || GetClassName() == "Ranger" || (GetClassName() == "Rogue" && GetSubclass() == 0) ||
        GetClassName() == "Sorcerer" || GetClassName() == "Warlock" || GetClassName() == "Wizard";
    }


    //COMBAT ACTIONS

    public void Dash()
    {
        CombatActions.Dash(this);
    }

    public void Disengage()
    {

    }
    
    public void Dodge()
    {
        
    }

    public void Help()
    {
        
    }

    public void Hide()
    {
        
    }

    //CLASS FEATURES

    public void PopulateMajorActions(List<MenuOption> menu)
    {
        //Aasimar healing touch
        //Cleric Divine Spark
        //Cleric Turn Undead
        //Life Cleric Preserve Life
        //Light Cleric Radiance of the Dawn
        //Trickery Cleric Blessing of the Trickster
        //Druid Wild Companion
        //Land Druid Land's Aid
        //Psi Warrior Telekinetic Movement - Uses based on number of dice + 1, needs to be decremented when other features use dice
    }

    public void PopulateAttacks(List<MenuOption> menu)
    {
        if (GetMainhandName() != "Shield" && GetMainhandName() != "Unarmed")
        {
            menu.Add(new MenuOption($"Mainhand ({GetMainhandName()})", () => CombatStateManager.Instance.DeclareAttack(GetMainhandID())));
        }

        if (GetOffhandName() != "Shield" && GetOffhandName() != "Unarmed")
        {
            menu.Add(new MenuOption($"Offhand ({GetOffhandName()})", () => CombatStateManager.Instance.DeclareAttack(GetOffhandID())));
        }

        //Dragonborn breath weapon
        //Druid Beast Form Attack
    }

    public void PopulateMinorActions(List<MenuOption> menu)
    {
        //Aasimar celestial revelation
        //Dragonborn draconic flight
        //Dwarf Stonecunning
        //Goliath Large Form
        //Cloud Goliath Cloud's Jaunt
        //Orc Adrenaline Rush

        if (GetClassName() == "Barbarian")
        {
            menu.Add(new MenuOption("Rage", () => RestoreHealth(1)));
        }

        //Wild Heart Barbarian Eagle Dash/Dodge
        //Zealot Barbarian Warrior of the Gods

        if (GetClassName() == "Bard")
        {
            menu.Add(new MenuOption("Bardic Inspiration", () => RestoreHealth(2)));
        }

        //Glamour Bard Mantle of Inspiration
        //Trickery Cleric Invoke Duplicity
        //Trickery Cleric Move Duplicate (only while summoned)
        //War Cleric War Priest
        //Druid Enter Wild Shape
        //Druid Exit Wild Shape (only while in Wild Shape)
        //Sea Druid Wrath of the Sea
        //Sea Druid Wrath of the Sea (While active)
        //Stars Druid Starry Form (Archer)
        //Stars Druid Starry Form (Chalice)
        //Stars Druid Starry Form (Dragon)
        //Stars Druid Archer Attack
        //Fighter Second Wind
    }

    public void PopulateFreeActions(List<MenuOption> menu)
    {
        //Drop Concentration
        //Drop Prone
        //Bard Font of Inspiration
        //Druid Wild Resurgence
        //Sea Druid dismiss Wrath of the Sea (While active)
        //Stars Druid Exit Starry Form
        //Fighter Action Surge
    }

    public void PopulateReactions()
    {
        //Fire Goliath Fire's Burn (Hit with attack)
        //Frost Goliath Frost's Chill (Hit with attack)
        //Hill Goliath Hill's Tumble (Hit with attack)
        //Stone Goliath Stone's Endurance (Take damage)
        //Storm Goliath Storm's Thunder (Take damage)
        //Halfling Luck (Roll 1)
        //Barbarian Reckless Attack (Declare attack)
        //World Tree Barbarian Vitality of the Tree (start of turn)
        //Zealot Barbarian Divine Fury (Radiant or Necrotic)
        //Use Inspiration (if inspired) - On missing attack
        //Use Inspiration (if inspired) - On failing save
        //Dance Bard Agile Strikes (Using Inspiration)
        //Glamour Bard Beguiling Magic (After casting enchantment or illusion spell)
        //Lore Bard Cutting Words
        //Valor Bard (if inspired) - On being hit
        //Valor Bard (if inspired) - On hitting attack
        //Light Cleric Warding Flare (Enemy declares attack)
        //War Cleric Guided Strike (Ally misses attack)
        //Stars Druid Chalice Healing (After healing)
        //Battlemaster Fighter Maneuvers (Need one row for each option)
        //Champion Fighter Remarkable Athlete (After critting)
        //Psi Warrior Protective Field (Ally takes damage)
        //Psi Warrior Psionic Strike (Hit attack)
    }
    

}
