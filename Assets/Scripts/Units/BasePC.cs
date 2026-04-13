using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BasePC : BaseUnit
{
    public Func<bool> OnTurnEnded { get; internal set; }

    public override void Initialize()
    {
        SetName(GetName());
    }

    public int GetSpeciesID()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT species FROM saved_pcs WHERE id = {UnitID}"));
    }

    public string GetSpeciesName()
    {
        return Convert.ToString(DatabaseManager.Instance.ExecuteScalar($"SELECT name FROM species WHERE id = {GetSpeciesID()}"));
    }

    public int GetOriginFeat()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT origin_feat FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int GetLevel()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT level FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int BarbarianLevel()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT barbarian_level FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int BardLevel()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT bard_level FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int ClericLevel()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT cleric_level FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int DruidLevel()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT druid_level FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int FighterLevel()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT fighter_level FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int MonkLevel()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT monk_level FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int PaladinLevel()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT paladin_level FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int RangerLevel()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT ranger_level FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int RogueLevel()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT rogue_level FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int SorcererLevel()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT sorcerer_level FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int WarlockLevel()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT warlock_level FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int WizardLevel()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT wizard_level FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int GetClassLevelFromID(int id)
    {
        return id switch
        {
            0 => BarbarianLevel(),
            1 => BardLevel(),
            2 => ClericLevel(),
            3 => DruidLevel(),
            4 => FighterLevel(),
            5 => MonkLevel(),
            6 => PaladinLevel(),
            7 => RangerLevel(),
            8 => RogueLevel(),
            9 => SorcererLevel(),
            10 => WarlockLevel(),
            11 => WizardLevel(),
            _ => 0,
        };
    }

    public int GetSubclass()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT subclass FROM saved_pcs WHERE id = {UnitID}"));
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

    public bool IsSpellcaster()
    {
        return Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT COUNT(*) FROM pc_spells WHERE unit_id = {UnitID}"));
    }

    public int GetCasterLevel()
    {
        int casterLevel = 0;

        casterLevel += BardLevel() + ClericLevel() + DruidLevel() + SorcererLevel() + WizardLevel();
        casterLevel += (int)Math.Ceiling((float)(PaladinLevel() / 2f));
        casterLevel += (int)Math.Ceiling((float)(RangerLevel() / 2f));

        if (FighterLevel() >= 3 && GetSubclass() == 2)
        {
            casterLevel += (int)Math.Ceiling((float)(FighterLevel() / 3f));
        }

        if (RogueLevel() >= 3 && GetSubclass() == 0)
        {
            casterLevel += (int)Math.Ceiling((float)(RogueLevel() / 3f));
        }
        // Debug.Log("Caster Level: " + casterLevel);

        return casterLevel;
    }


    //COMBAT ACTIONS

    public void ActionTemplate()
    {
        // CombatStateManager.Instance.StartTargetSelection(
        //     TargetType.Monster, //Change depending on if the valid target is a Monster, PC, Unit, or Tile
        //     (target) =>
        //     {
        //         CombatActions.MeleeWeaponAttack(
        //             this,
        //             CombatStateManager.Instance.declaredWeapon,
        //             target
        //         );

        //         UseResource("major_action");
        //     },
        //     (target) =>
        //     {
        //         if (GetResource("major_action") <= 0)
        //         {
        //             return (false, "You don't have a major action available");
        //         }

        //         if (!RangeHelper.IsTargetInRange(this, target, 1)) //Range in tiles
        //         {
        //             return (false, "Target is out of range");
        //         }

        //         return (true, "");
        //     }
        // );
    }

    public void Help()
    {
        CombatStateManager.Instance.StartTargetSelection(
            TargetType.Monster, this, 1, //Change depending on if the valid target is a Monster, PC, Unit, or Tile
            (target) =>
            {
                CombatActions.Help(target);

                UseResource("major_action");
            },
            (target) =>
            {
                if (GetResource("major_action") <= 0)
                {
                    return (false, "You don't have a major action available");
                }

                if (!RangeHelper.IsTargetInRange(this, target, 1))
                {
                    return (false, "Target is out of range");
                }

                return (true, "");
            }
        );
    }

    public void Hide()
    {
        if (UseResource("major_action"))
        {
            CombatActions.Hide(this);
        }
    }

    public IEnumerator Attack(int weaponID)
    {
        BaseUnit target = null;

        yield return StartCoroutine(CombatStateManager.Instance.StartTargetSelection(
            TargetType.Monster, this, 1, //Change depending on if the valid target is a Monster, PC, Unit, or Tile
            (selectedTarget) =>
            {
                target = selectedTarget;
            },
            (selectedTarget) =>
            {
                if (GetResource("current_number_of_attacks") == 0)
                {
                    return (false, "You have used all of your attacks this turn");
                }

                if (GetResource("major_action") <= 0 && (GetResource("current_number_of_attacks") == GetResource("max_number_of_attacks")))
                {
                    return (false, "You don't have a major action available");
                }

                if (!RangeHelper.IsTargetInRange(this, selectedTarget, RangeHelper.GetMaximumRange(weaponID))) //Range in tiles
                {
                    return (false, "Target is out of range");
                }

                return (true, "");
            }
        ));

        if(target != null)
        {
            yield return StartCoroutine(CombatActions.AttackWithWeapon(this, target, weaponID, (success) =>
            {
                if (success)
                {
                    if (!(GetResource("current_number_of_attacks") < GetResource("max_number_of_attacks")))
                    {
                        UseResource("major_action");
                    }
                    UseResource("current_number_of_attacks");
                }
            }));
        }
    }

    public IEnumerator ShoveBack()
    {
        BaseUnit target = null;
        yield return StartCoroutine(CombatStateManager.Instance.StartTargetSelection(
            TargetType.Monster, this, 1, //Change depending on if the valid target is a Monster, PC, Unit, or Tile
            (selectedTarget) =>
            {
                target = selectedTarget;
            },
            (target) =>
            {
                if (GetResource("current_number_of_attacks") == 0)
                {
                    return (false, "You have used all of your attacks this turn");
                }

                if (GetResource("major_action") <= 0 && (GetResource("current_number_of_attacks") == GetResource("max_number_of_attacks")))
                {
                    return (false, "You don't have a major action available");
                }

                if (!RangeHelper.IsTargetInRange(this, target, 1)) //Range in tiles
                {
                    return (false, "Target is out of range");
                }

                return (true, "");
            }
        ));

        if (target != null)
        {
            yield return StartCoroutine(PushTarget(target, 1));

            if (GetResource("current_number_of_attacks") < GetResource("max_number_of_attacks"))
            {
                UseResource("current_number_of_attacks");
            }
            else
            {
                UseResource("major_action");
                UseResource("current_number_of_attacks");
            }
        }
    }

    public void ShoveProne()
    {
        BaseUnit target = null;

        StartCoroutine(CombatStateManager.Instance.StartTargetSelection(
            TargetType.Monster, this, 1, //Change depending on if the valid target is a Monster, PC, Unit, or Tile
            (selectedTarget) =>
            {
                target = selectedTarget;
            },
            (target) =>
            {
                if (GetResource("current_number_of_attacks") == 0)
                {
                    return (false, "You have used all of your attacks this turn");
                }

                if (GetResource("major_action") <= 0 && (GetResource("current_number_of_attacks") == GetResource("max_number_of_attacks")))
                {
                    return (false, "You don't have a major action available");
                }

                if (!RangeHelper.IsTargetInRange(this, target, 1)) //Range in tiles
                {
                    return (false, "Target is out of range");
                }

                return (true, "");
            }
        ));
        
        if(target != null)
        {
            bool failed;
            if (target.GetStat("mSTR") > target.GetStat("mDEX"))
            {
                failed = target.MakeSave("STR", GetStat("str_dc"));
            }
            else
            {
                failed = target.MakeSave("DEX", GetStat("str_dc"));
            }

            if (failed)
            {
                target.SetCondition("prone", true);
            }

            if (GetResource("current_number_of_attacks") < GetResource("max_number_of_attacks"))
            {
                UseResource("current_number_of_attacks");
            }
            else
            {
                UseResource("major_action");
                UseResource("current_number_of_attacks");
            }
        }
    }

    public IEnumerator MakeDeathSave()
    {
        if (GetCondition("dying"))
        {
            yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} is dying"));
            int result = DiceRoller.Rolld20(false, false);
            if (result == 20)
                yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} rolled a natural 20 on their death save, and is no longer dying!"));
            else
                yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} rolled a {result} on their death save"));
            
            if (result == 1)
            {
                yield return FailDeathSave(2);
            }
            else if (result < 10)
            {
                yield return FailDeathSave(1);
            }
            else if (result == 20)
            {
                RestoreHealth(1);
            }
            else
            {
                yield return PassDeathSave(1);
            }
        }
    }

    public IEnumerator FailDeathSave(int number)
    {
        int currentFails = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT death_save_fails FROM unit_info WHERE id = {UnitID}"));
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_info SET death_save_fails = {currentFails + number} WHERE id = {UnitID}");
        yield return CheckForDeath(currentFails);
    }

    public IEnumerator PassDeathSave(int number)
    {
        int currentPasses = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT death_save_successes FROM unit_info WHERE id = {UnitID}"));
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_info SET death_save_successes = {currentPasses + number} WHERE id = {UnitID}");
        yield return CheckForStable(currentPasses);
    }

    public IEnumerator CheckForDeath(int currentFails)
    {
        if (currentFails >= 3)
        {
            yield return StartCoroutine(Die());
        }
        else
        {
            EndTurn();
        }
    }

    public IEnumerator CheckForStable(int currentPasses)
    {
        if (currentPasses >= 3)
        {
            SetCondition("dying", false);
        }
        EndTurn();
        yield return null;
    }

    public void ClearDeathSaves()
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_info SET death_save_successes = 0, death_save_fails = 0 WHERE id = {UnitID}");
        SetCondition("dying", false);
        SetCondition("unconscious", false);
    }

    public IEnumerator FallUnconscious()
    {

        // Debug.Log("Fallen unconscious");
        SetCondition("unconscious", true);
        SetCondition("prone", true);
        SetCondition("dying", true);
        yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} is dying!"));
        // Log($"{unit.UnitName} is dying!");

        if (InitiativeTracker.Instance.currentTurnUnit == this)
        {
            EndTurn();
        }

        // yield return new WaitForSeconds(0.4f);
        // StartCoroutine(CombatStateManager.Instance.CheckForGameOver());
    }

    //CLASS FEATURES

    public void PopulateMajorActions(List<MenuOption> menu)
    {
        //Aasimar Healing Touch
        menu.Add(new MenuOption($"Healing Touch", () => SpeciesFeatures.HealingHands(this),
            () => GetSpeciesName() == "Aasimar",
            () => GetResource("healing_touch") > 0 && GetResource("major_action") > 0));

        //Cleric Divine Spark
        //Cleric Turn Undead
        //Life Cleric Preserve Life
        //Light Cleric Radiance of the Dawn
        //Trickery Cleric Blessing of the Trickster
        //Druid Wild Companion
        //Land Druid Land's Aid
        //Psi Warrior Telekinetic Movement - Uses based on number of dice + 1, needs to be decremented when other features use dice
        //Paladin Find Steed
        //Ancients Paladin Nature's Wrath
        //Warlock Armor of Shadows
        //Warlock Pact of the Chain
        //Warlock Fiendish Vigor
        //Warlock Master of Myriad Forms (Aquatic Adaptation)
        //Warlock Master of Myriad Forms (Natural Weapons)
        //Warlock One with Shadows
        //Healer use healer's kit
    }

    public void PopulateAttacks(List<MenuOption> menu)
    {

        menu.Add(new MenuOption($"Mainhand ({GetMainhandName()})", () => StartCoroutine(Attack(GetMainhandID())),
            () => GetMainhandName() != "Shield" && GetMainhandName() != "Unarmed",
            () => GetResource("major_action") > 0 ||
                (GetResource("current_number_of_attacks") < GetResource("max_number_of_attacks") && GetResource("current_number_of_attacks") > 0)));

        menu.Add(new MenuOption($"Offhand ({GetOffhandName()})", () => StartCoroutine(Attack(GetOffhandID())),
            () => GetOffhandName() != "Shield" && GetOffhandName() != "Unarmed",
            () => GetResource("major_action") > 0 ||
                (GetResource("current_number_of_attacks") < GetResource("max_number_of_attacks") && GetResource("current_number_of_attacks") > 0)));

        //Dragonborn breath weapon
        //Druid Beast Form Attack
        //Battlemaster Fighter (Commander's Strike)
        //Beastmaster Ranger Command Companion
        //Soulknife Rogue Psychic Blade
        //Warlock Pact of the Chain Forgo Attack
    }

    public void PopulateMinorActions(List<MenuOption> menu)
    {
        //Light weapon offhand attack
        menu.Add(new MenuOption($"Offhand Attack", () => StartCoroutine(Attack(GetOffhandID())), //Make sure this only costs minor action
            () => Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT light FROM weapons WHERE id = {GetOffhandID()}")) &&
                Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT light FROM weapons WHERE id = {GetMainhandID()}")),
            () => GetResource("minor_action") > 0 || GetResource("nick_attack_available") > 0)); //TODO: add way to check if character attacked with a light weapon this turn

        //Aasimar celestial revelation
        menu.Add(new MenuOption($"Celestial Revelation (Heavenly Wings)", () => SpeciesFeatures.CelestialRevelation1(this),
            () => GetSpeciesID() == 0 && GetLevel() >= 3,
            () => GetResource("celestial_revelation") > 0 && GetResource("minor_action") > 0));

        menu.Add(new MenuOption($"Celestial Revelation (Inner Radiance)", () => SpeciesFeatures.CelestialRevelation1(this),
            () => GetSpeciesID() == 0 && GetLevel() >= 3,
            () => GetResource("celestial_revelation") > 0 && GetResource("minor_action") > 0));

        menu.Add(new MenuOption($"Celestial Revelation (Necrotic Shroud)", () => SpeciesFeatures.CelestialRevelation1(this),
            () => GetSpeciesID() == 0 && GetLevel() >= 3,
            () => GetResource("celestial_revelation") > 0 && GetResource("minor_action") > 0));

        //Dragonborn draconic flight
        menu.Add(new MenuOption($"Draconic Flight", () => SpeciesFeatures.DraconicFlight(this),
            () => GetSpeciesID() >= 1 && GetSpeciesID() <= 5 && GetLevel() >= 5,
            () => GetResource("draconic_flight") > 0 && GetResource("minor_action") > 0));

        //Dwarf Stonecunning
        menu.Add(new MenuOption($"Stonecunning", () => SpeciesFeatures.Stonecunning(this),
            () => GetSpeciesID() == 6,
            () => GetResource("stonecunning") > 0 && GetResource("minor_action") > 0));

        //Goliath Large Form
        menu.Add(new MenuOption($"Large Form", () => SpeciesFeatures.LargeForm(this),
            () => GetSpeciesID() >= 12 && GetSpeciesID() <= 17 && GetLevel() >= 5,
            () => GetResource("large_form") > 0 && GetResource("minor_action") > 0));

        //Cloud Goliath Cloud's Jaunt
        menu.Add(new MenuOption($"Cloud's Jaunt", () => SpeciesFeatures.CloudsJaunt(this),
            () => GetSpeciesID() == 12,
            () => GetResource("giant_ancestry") > 0 && GetResource("minor_action") > 0));

        //Orc Adrenaline Rush
        menu.Add(new MenuOption($"Adrenaline Rush", () => SpeciesFeatures.AdrenalineRush(this),
            () => GetSpeciesID() == 20,
            () => GetResource("adrenaline_rush") > 0 && GetResource("minor_action") > 0));

        //Barbarian rage
        menu.Add(new MenuOption($"Rage", () => ClassFeatures.Rage(this),
            () => BarbarianLevel() > 0,
            () => GetResource("rage") > 0 && GetResource("minor_action") > 0));

        //Wild Heart Barbarian Eagle Dash/Dodge
        menu.Add(new MenuOption($"Disengage + Dash", () => ClassFeatures.EagleDashDisengage(this),
            () => BarbarianLevel() >= 3 && GetSubclass() == 1,
            () => GetCondition("raging") && GetResource("minor_action") > 0));

        //Zealot Barbarian Warrior of the Gods
        menu.Add(new MenuOption($"Warrior of the Gods Healing ({GetResource("warrior_of_the_gods_available")})", () => ClassFeatures.OpenWarriorOfTheGodsMenu(this),
            () => BarbarianLevel() >= 3 && GetSubclass() == 3,
            () => GetResource("warrior_of_the_gods_available") > 0 && GetResource("minor_action") > 0));

        //Bard bardic inspiration
        menu.Add(new MenuOption($"Bardic Inspiration", () => ClassFeatures.BardicInspiration(this),
            () => BardLevel() > 0,
            () => GetResource("current_inspiration") > 0 && GetResource("minor_action") > 0));


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
        //Battlemaster Fighter (Evasive Footwork)
        //Battlemaster Fighter (Feinting Attack)
        //Battlemaster Fighter (Lunging Attack)
        //Battlemaster Fighter (Rally)
        //Monk Bonus Unarmed Strike
        //Monk Flurry of Blows (Allows two uses of free action attack)
        //Monk Patient Defense
        //Monk Empowered Patient Defense
        //Monk Step of the Wind
        //Monk Empowered Step of the Wind
        //Paladin Lay on Hands
        //Glory Paladin Peerless Athlete
        //Beastmaster Ranger Command Companion
        //Rogue Cunning Action (Dash)
        //Rogue Cunning Action (Disengage)
        //Rogue Cunning Action (Hide)
        //Rogue Steady Aim
        //Thief Rogue Fast Hands
        //Sorcerer Innate Sorcery
        //Sorcerer Font of Magic (Convert to Spell Slot)
        //Sorcerer Metamagic (Quickened Spell) (Before casting spell with action, and bonus action hasn't been used)
        //Warlock Otherworldly Leap
        //Warlock Investment of the Chain Master command attack
        //Celestial Warlock Healing Light
        //Abjurer Wizard Refresh Arcane Ward
        //Chef feat Eat Treat
        //Durable feat Speedy Recovery
        //Poisoner feat Apply Poison
        //Telekinetic feat Telekinetic Shove
        //Offhand Light Attack (Dual Wielder)
        //Great Weapon Master feat Hew (after critting with melee weapon)
        //Polearm Master feat Pole Strike
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
        //Battlemaster Fighter (Bait and Switch)
        //Monk Flurry of Blows (Only after using flurry of blows)
        //Mercy Monk Hand of Healing (only afer using flurry of blows)
        //Devotion Paladin Sacred Weapon
        //Devotion Paladin dismiss Sacred Weapon (While active)
        //Vengance Paladin Vow of Enmity
        //Vengance Paladin move Vow of Enmity
        //Hunter Ranger Horde Breaker
        //Sorcerer Font of Magic (Convert to Sorcery Points)
    }

    public void PopulateReactions()
    {
        //Opportunity Attack (character moves out of melee range)
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
        //Battlemaster Fighter (Ambush) (After rolling stealth / initiative)
        //Battlemaster Fighter (Disarming Strike) (Hit with attack)
        //Battlemaster Fighter (Distracting Strike) (Hit with attack)
        //Battlemaster Fighter (Goading Attack) (Hit with attack)
        //Battlemaster Fighter (Maneuvering Attack) (Hit with attack)
        //Use Commander's Strike (When fighter uses Commander's Strike)
        //Use Maneuvering Attack (When fighter uses Maneuvering Attack)
        //Battlemaster Fighter (Menacing Attack) (Hit with attack)
        //Battlemaster Fighter (Parry) (Damaged by melee attack)
        //Battlemaster Fighter (Precision Attack) (Miss with attack)
        //Battlemaster Fighter (Pushing Attack) (Hit with attack)
        //Battlemaster Fighter (Riposte) (Missed by melee attack)
        //Battlemaster Fighter (Sweeping Attack) (Hit with melee attack)
        //Battlemaster Fighter (Trip Attack) (Hit with attack)
        //Champion Fighter Remarkable Athlete (After critting)
        //Psi Warrior Protective Field (Ally takes damage)
        //Psi Warrior Psionic Strike (Hit attack)
        //Monk Deflect Attacks (Take bldg/pierce/slash damage)
        //Monk Deflect Attacks Reflection (deflect attack reduces damage to 0)
        //Monk Slow Fall (take fall damage)
        //Monk Stunning Strike (hit with monk weapon)
        //Mercy Monk Hand of Harm (hit with monk weapon)
        //Elements Monk change damage type (hit with unarmed strike)
        //Shadow Monk move Darkness (start of turn)
        //Open Hand Monk Addle (hit with flurry of blows)
        //Open Hand Monk Push (hit with flurry of blows)
        //Open Hand Monk Topple (hit with flurry of blows)
        //Glory Paladin Inspiring Smite (after cast Divine Smite)
        //Fey Wanderer Ranger Dreadful Strikes (hit with weapon)
        //Gloom Stalker Ranger Dread Ambusher (hit with weapon)
        //Hunter Ranger Colossus Slayer (hit with weapon)
        //Rogue Sneak Attack (hit with finesse / ranged weapon)
        //Rogue Cunning Strike (Poison) (hit with finesse / ranged weapon)
        //Rogue Cunning Strike (Trip) (hit with finesse / ranged weapon)
        //Rogue Cunning Strike (Withdraw) (hit with finesse / ranged weapon)
        //Rogue Uncanny Dodge (hit by attack)
        //Soulknife Rogue Psi-Bolstered Knack (fail ability check)
        //Soulknife Rogue Psychic Blade OA (character moves away)
        //Sorcerer Metamagic (Careful Spell) (Cast spell with multiple targets)
        //Sorcerer Metamagic (Distant Spell) (Before casting spell with range)
        //Sorcerer Metamagic (Empowered Spell) (After doing damage with spell) (Additive)
        //Sorcerer Metamagic (Extended Spell) (Cast spell with duration)
        //Sorcerer Metamagic (Heightened Spell) (Cast spell with save)
        //Sorcerer Metamagic (Seeking Spell) (Miss with spell attack) (Additive)
        //Sorcerer Metamagic (Transmuted Spell) (Cast spell that deals Acid, Cold, Fire, Lightning, Poison, or Thunder damage)
        //Sorcerer Metamagic (Twinned Spell) (Cast spell that can be upcast to increase target)
        //Clockwork Sorcerer Restore Balance (character about to roll a d20)
        //Wild Magic Sorcerer Wild Magic Surge (cast leveled spell)
        //Wild Magic Sorcrerer Tides of Chaos (about to roll a d20)
        //Warlock Pact of the Blade change damage type (hit with weapon)
        //Warlock Repelling Blast (hit with cantrip)
        //Warlock Eldritch Smite (hit with weapon)
        //Warlock Investment of the Chain Master change damage type
        //Archfey Warlock Steps of the Fey (Refreshing Step) (after teleporting)
        //Archfey Warlock Steps of the Fey (Taunting Step) (after teleporting)
        //Great Old One Warlock Psychic Spells (do damage with spell)
        //Diviner Wizard Portent 1 (after creature rolls d20)
        //Diviner Wizard Portent 2 (after creature rolls d20)
        //Lucky feat grant advantage (before you roll d20)
        //Lucky feat grant disadvantage (before enemy rolls to hit)
        //Savage Attacker feat reroll damage (after rolling damage)
        //Tavern Brawler feat push (after hit with unarmed strike)
        //Crusher feat push (after damage with bludgeoning weapon)
        //Mage Slayer feat Guarded Mind (after fail mental save)
        //Piercer feat Puncture (after hit with piercing damage)
        //Slasher feat Hamstring (after damage with slashing weapon)
        //Charger feat Charge Attack (Damage) (before hitting with melee weapon after moving 10 feet)
        //Charger feat Charge Attack (Push) (before hitting with melee weapon after moving 10 feet)
        //Defensive Duelist feat Parry (after hit by melee attack)
        //Grappler feat Punch and Grab (after hitting with unarmed strike)
        //Polearm Master feat Reactive Strike (enemy moves into melee range)
        //Sentinel feat Guardian (enemy disengages or attacks ally)
        //War Caster feat Reactive Spell (enemy leaves melee range)
        //Shield Master feat Shield Bash (Push) (after hitting with melee attack)
        //Shield Master feat Shield Bash (Prone) (after hitting with melee attack)
        //Shield Master feat Interpose Shield (after succeeding DEX save)
    }

    public void PopulateSpells(List<MenuOption> menu, string actionCost, int spellLevel)
    {
        if (!IsSpellcaster())
            return;

        if(spellLevel == 0)
        {
            DatabaseManager.Instance.ExecuteReader(
                $"SELECT pc_spells.* FROM pc_spells JOIN spells ON pc_spells.spell_id = spells.id WHERE pc_spells.unit_id = {UnitID} AND spells.cast_time = '{actionCost}' AND spells.level = 0",
            reader =>
            {
                while (reader.Read())
                {
                    int spellID = Convert.ToInt32(reader["spell_id"]);
                    string spellcastingAbility = Convert.ToString(reader["spellcasting_ability"]);

                    menu.Add(new MenuOption($"{SpellsManager.GetName(spellID)}",
                        () => StartCoroutine(SpellsManager.CastSpell(this, spellID, spellLevel, spellcastingAbility)),
                        () => true,
                        () => GetResource(SpellsManager.GetCastTime(spellID)) > 0));
                }
            });
        }
        else
        {
            DatabaseManager.Instance.ExecuteReader(
                $"SELECT pc_spells.* FROM pc_spells JOIN spells ON pc_spells.spell_id = spells.id "+
                $"WHERE pc_spells.unit_id = {UnitID} AND spells.cast_time = '{actionCost}' AND spells.level > 0 AND spells.level <= {spellLevel}",
            reader =>
            {
                while (reader.Read())
                {
                    int spellID = Convert.ToInt32(reader["spell_id"]);
                    string spellcastingAbility = Convert.ToString(reader["spellcasting_ability"]);

                    menu.Add(new MenuOption($"{SpellsManager.GetName(spellID)}",
                        () => StartCoroutine(SpellsManager.CastSpell(this, spellID, spellLevel, spellcastingAbility)),
                        () => true,
                        () => GetResource(SpellsManager.GetCastTime(spellID)) > 0));
                }
            });
        }
    }
}