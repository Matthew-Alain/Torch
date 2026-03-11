using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using Debug = UnityEngine.Debug;

public class AbilityScoreManager : MonoBehaviour
{
    //Base stats
    public Button btnStrDown, btnDexDown, btnConDown, btnIntDown, btnWisDown, btnChaDown;
    private List<Button> statDownButtons = new List<Button>();
    public TMP_Text txt_base_STR, txt_base_DEX, txt_base_CON, txt_base_INT, txt_base_WIS, txt_base_CHA; 
    private List<TMP_Text> txtBaseStats = new List<TMP_Text>();
    public Button btnStrUp, btnDexUp, btnConUp, btnIntUp, btnWisUp, btnChaUp;
    private List<Button> statUpButtons = new List<Button>();

    //Origin Stats
    public Button btnOriginStrDown, btnOriginDexDown, btnOriginConDown, btnOriginIntDown, btnOriginWisDown, btnOriginChaDown;
    private List<Button> originDownButtons = new List<Button>();
    public TMP_Text txt_origin_STR, txt_origin_DEX, txt_origin_CON, txt_origin_INT, txt_origin_WIS, txt_origin_CHA;
    private List<TMP_Text> txtOriginStats = new List<TMP_Text>();
    public Button ObtnStrUp, ObtnDexUp, ObtnConUp, ObtnIntUp, ObtnWisUp, ObtnChaUp;
    private List<Button> originUpButtons = new List<Button>();

    //Final scores and mods
    public TMP_Text txt_strength, txt_dexterity, txt_constitution, txt_intelligence, txt_wisdom, txt_charisma;
    private List<TMP_Text> txtFinalStats = new List<TMP_Text>();

    public TMP_Text txt_mSTR, txt_mDEX, txt_mCON, txt_mINT, txt_mWIS, txt_mCHA;
    private List<TMP_Text> txtModsList = new List<TMP_Text>();

    //From database
    private int base_str, base_dex, base_con, base_int, base_wis, base_cha;
    private List<int> baseStatList = new List<int>();
    private int origin_str, origin_dex, origin_con, origin_int, origin_wis, origin_cha;
    private List<int> originStatList = new List<int>();
    private int asi_str, asi_dex, asi_con, asi_int, asi_wis, asi_cha;
    private List<int> asiStatList = new List<int>();
    private int strength, dexterity, constitution, intelligence, wisdom, charisma;
    private List<int> finalStatsList = new List<int>();
    private int mSTR, mDEX, mCON, mINT, mWIS, mCHA;
    private List<int> modsList = new List<int>();


    private int maxPointBuy = 27;
    private int currentPointBuy;
    private int maxOriginPoints = 3;
    private int currentOriginPoints;
    public Button btnBack;
    

    public int PCID;

    void Awake()
    {
        PCID = DatabaseManager.Instance.lastPCEdited;

        DatabaseManager.Instance.ExecuteReader(
            "SELECT base_str, base_dex, base_con, base_int, base_wis, base_cha, " +
                "origin_str, origin_dex, origin_con, origin_int, origin_wis, origin_cha, " +
                "asi_str, asi_dex, asi_con, asi_int, asi_wis, asi_cha, " +
                "strength, dexterity, constitution, intelligence, wisdom, charisma, " +
                "mSTR, mDEX, mCON, mINT, mWIS, mCHA, " +
                "current_point_buy, current_origin_points " +
                "FROM pc_stats WHERE id = (@PCID)",
            reader =>
            {
                while (reader.Read())
                {
                    base_str = Convert.ToInt32(reader["base_str"]);
                    base_dex = Convert.ToInt32(reader["base_dex"]);
                    base_con = Convert.ToInt32(reader["base_con"]);
                    base_int = Convert.ToInt32(reader["base_int"]);
                    base_wis = Convert.ToInt32(reader["base_wis"]);
                    base_cha = Convert.ToInt32(reader["base_cha"]);

                    origin_str = Convert.ToInt32(reader["origin_str"]);
                    origin_dex = Convert.ToInt32(reader["origin_dex"]);
                    origin_con = Convert.ToInt32(reader["origin_con"]);
                    origin_int = Convert.ToInt32(reader["origin_int"]);
                    origin_wis = Convert.ToInt32(reader["origin_wis"]);
                    origin_cha = Convert.ToInt32(reader["origin_cha"]);

                    asi_str = Convert.ToInt32(reader["asi_str"]);
                    asi_dex = Convert.ToInt32(reader["asi_dex"]);
                    asi_con = Convert.ToInt32(reader["asi_con"]);
                    asi_int = Convert.ToInt32(reader["asi_int"]);
                    asi_wis = Convert.ToInt32(reader["asi_wis"]);
                    asi_cha = Convert.ToInt32(reader["asi_cha"]);

                    strength = Convert.ToInt32(reader["strength"]);
                    dexterity = Convert.ToInt32(reader["dexterity"]);
                    constitution = Convert.ToInt32(reader["constitution"]);
                    intelligence = Convert.ToInt32(reader["intelligence"]);
                    wisdom = Convert.ToInt32(reader["wisdom"]);
                    charisma = Convert.ToInt32(reader["charisma"]);

                    mSTR = Convert.ToInt32(reader["mSTR"]);
                    mDEX = Convert.ToInt32(reader["mDEX"]);
                    mCON = Convert.ToInt32(reader["mCON"]);
                    mINT = Convert.ToInt32(reader["mINT"]);
                    mWIS = Convert.ToInt32(reader["mWIS"]);
                    mCHA = Convert.ToInt32(reader["mCHA"]);

                    currentPointBuy = Convert.ToInt32(reader["current_point_buy"]);
                    currentOriginPoints = Convert.ToInt32(reader["current_origin_points"]);
                }
            },
            ("@PCID", PCID)
        );
            }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        statDownButtons.Add(btnStrDown);
        statDownButtons.Add(btnDexDown);
        statDownButtons.Add(btnConDown);
        statDownButtons.Add(btnIntDown);
        statDownButtons.Add(btnWisDown);
        statDownButtons.Add(btnChaDown);

        txtBaseStats.Add(txt_base_STR);
        txtBaseStats.Add(txt_base_DEX);
        txtBaseStats.Add(txt_base_CON);
        txtBaseStats.Add(txt_base_INT);
        txtBaseStats.Add(txt_base_WIS);
        txtBaseStats.Add(txt_base_CHA);

        statUpButtons.Add(btnStrUp);
        statUpButtons.Add(btnDexUp);
        statUpButtons.Add(btnConUp);
        statUpButtons.Add(btnIntUp);
        statUpButtons.Add(btnWisUp);
        statUpButtons.Add(btnChaUp);

        originDownButtons.Add(btnOriginStrDown);
        originDownButtons.Add(btnOriginDexDown);
        originDownButtons.Add(btnOriginConDown);
        originDownButtons.Add(btnOriginIntDown);
        originDownButtons.Add(btnOriginWisDown);
        originDownButtons.Add(btnOriginChaDown);

        txtOriginStats.Add(txt_origin_STR);
        txtOriginStats.Add(txt_origin_DEX);
        txtOriginStats.Add(txt_origin_CON);
        txtOriginStats.Add(txt_origin_INT);
        txtOriginStats.Add(txt_origin_WIS);
        txtOriginStats.Add(txt_origin_CHA);

        originUpButtons.Add(ObtnStrUp);
        originUpButtons.Add(ObtnDexUp);
        originUpButtons.Add(ObtnConUp);
        originUpButtons.Add(ObtnIntUp);
        originUpButtons.Add(ObtnWisUp);
        originUpButtons.Add(ObtnChaUp);

        txtFinalStats.Add(txt_strength);
        txtFinalStats.Add(txt_dexterity);
        txtFinalStats.Add(txt_constitution);
        txtFinalStats.Add(txt_intelligence);
        txtFinalStats.Add(txt_wisdom);
        txtFinalStats.Add(txt_charisma);

        txtModsList.Add(txt_mSTR);
        txtModsList.Add(txt_mDEX);
        txtModsList.Add(txt_mCON);
        txtModsList.Add(txt_mINT);
        txtModsList.Add(txt_mWIS);
        txtModsList.Add(txt_mCHA);

        baseStatList.Add(base_str);
        baseStatList.Add(base_dex);
        baseStatList.Add(base_con);
        baseStatList.Add(base_int);
        baseStatList.Add(base_wis);
        baseStatList.Add(base_cha);

        originStatList.Add(origin_str);
        originStatList.Add(origin_dex);
        originStatList.Add(origin_con);
        originStatList.Add(origin_int);
        originStatList.Add(origin_wis);
        originStatList.Add(origin_cha);

        asiStatList.Add(asi_str);
        asiStatList.Add(asi_dex);
        asiStatList.Add(asi_con);
        asiStatList.Add(asi_int);
        asiStatList.Add(asi_wis);
        asiStatList.Add(asi_cha);

        modsList.Add(mSTR);
        modsList.Add(mDEX);
        modsList.Add(mCON);
        modsList.Add(mINT);
        modsList.Add(mWIS);
        modsList.Add(mCHA);

        finalStatsList.Add(strength);
        finalStatsList.Add(dexterity);
        finalStatsList.Add(constitution);
        finalStatsList.Add(intelligence);
        finalStatsList.Add(wisdom);
        finalStatsList.Add(charisma);

        btnBack.onClick.AddListener(SaveAbilityScores);
        UpdateText();
    }

    public void StatDown(int index)
    {
        if (baseStatList[index] <= 8)
        {
            Debug.Log("Stat " + index + " cannot be less than 8");
        }
        else
        {
            if (baseStatList[index] >= 14)
            {
                currentPointBuy -= 2;
            }
            else
            {
                currentPointBuy -= 1;
            }
            baseStatList[index] -= 1;
            if (baseStatList[index] <= 8)
            {
                statDownButtons[index].interactable = false;
            }
            UpdateText();
        }
    }

    public void StatUp(int index)
    {
        if (baseStatList[index] >= 15)
        {
            Debug.Log("Stat " + index + " cannot be more than 15");
        }
        else if (currentPointBuy >= maxPointBuy || (baseStatList[index] >= 13 && maxPointBuy - currentPointBuy <= 1))
        {
            Debug.Log("Not enough points");
        }
        else
        {
            if (baseStatList[index] >= 13)
            {
                currentPointBuy += 2;
            }
            else
            {
                currentPointBuy += 1;
            }
            baseStatList[index] += 1;
            if (baseStatList[index] >= 13)
            {
                statUpButtons[index].interactable = false;
            }
            UpdateText();
        }
    }
    
    public void OriginDown(int index)
    {
        if(originStatList[index] <= 0)
        {
            Debug.Log("Stat " + index + " origin bonus cannot be less than 0");
        }else
        {
            currentOriginPoints -= 1;
            originStatList[index] -= 1;
            if (originStatList[index] <= 0)
            {
                originDownButtons[index].interactable = false;
            }
            UpdateText();
        }
    }

    public void OriginUp(int index)
    {
        if (originStatList[index] >= 2)
        {
            Debug.Log("Stat " + index + " origin bonus cannot be more than 2");
        }
        else if (currentOriginPoints >= 3)
        {
            Debug.Log("Not enough points");
        }
        else
        {
            currentOriginPoints += 1;
            originStatList[index] += 1;
            if (originStatList[index] >= 2)
            {
                originDownButtons[index].interactable = false;
            }
            UpdateText();
        }
    }

    void UpdateText()
    {
        if (currentPointBuy < 0)
        {
            Debug.LogWarning("YOU GOT A NEGATIVE ABILITY SCORE!");
        }
        if (currentPointBuy > 27)
        {
            Debug.LogWarning("YOU GOT OVER THE MAX POINTS!");
        }

        DisableButtons();
        CalculateStats();

        for (int i = 0; i < 6; i++)
        {
            txtBaseStats[i].text = baseStatList[i].ToString();
            txtOriginStats[i].text = originStatList[i].ToString();
            txtFinalStats[i].text = finalStatsList[i].ToString();
            txtModsList[i].text = AddPlusMinusSign(modsList[i]);
        }
    }

    void DisableButtons()
    {
        for (int i = 0; i < 6; i++)
        {
            //Stat down buttons
            if (baseStatList[i] <= 8)
            {
                statDownButtons[i].interactable = false;
            }
            else
            {
                statDownButtons[i].interactable = true;
            }

            //Stat up buttons
            if (currentPointBuy >= maxPointBuy || baseStatList[i] >= 15 || (baseStatList[i] >= 13 && maxPointBuy - currentPointBuy <= 1))
            {
                statUpButtons[i].interactable = false;
            }
            else
            {
                statUpButtons[i].interactable = true;
            }

            //Origin down buttons
            if (originStatList[i] <= 0)
            {
                originDownButtons[i].interactable = false;
            }
            else
            {
                originDownButtons[i].interactable = true;
            }

            //Origin up buttons
            if (currentOriginPoints >= maxOriginPoints || originStatList[i] >= 2)
            {
                originUpButtons[i].interactable = false;
            }
            else
            {
                originUpButtons[i].interactable = true;
            }
        }
    }

    void CalculateStats()
    {
        for (int i = 0; i < 6; i++)
        {
            finalStatsList[i] = baseStatList[i] + originStatList[i] + asiStatList[i];
            modsList[i] = (int)Math.Floor((finalStatsList[i] - 10) / 2m);
        }
    }

    string AddPlusMinusSign(int value)
    {
        string textToReturn = "";
        if (value >= 0)
        {
            textToReturn += "+" + value;
        }
        else
        {
            textToReturn += value;
        }
        return textToReturn;
    }

    void SaveAbilityScores()
    {
        DatabaseManager.Instance.ExecuteNonQuery(
            "UPDATE pc_stats SET base_str = @base_str, base_dex = @base_dex, base_con = @base_con, base_int = @base_int, base_wis = @base_wis, base_cha = @base_cha, " +
                "origin_str = @origin_str, origin_dex = @origin_dex, origin_con = @origin_con, origin_int = @origin_int, origin_wis = @origin_wis, origin_cha = @origin_cha, " +
                "strength = @strength, dexterity = @dexterity, constitution = @constitution, intelligence = @intelligence, wisdom = @wisdom, charisma = @charisma, " +
                "mSTR = @mSTR, mDEX = @mDEX, mCON = @mCON, mINT = @mINT, mWIS = @mWIS, mCHA = @mCHA, " +
                "current_point_buy = @current_point_buy, current_origin_points = @current_origin_points " +
                "WHERE id = @id",
            ("@base_str", baseStatList[0]),
            ("@base_dex", baseStatList[1]),
            ("@base_con", baseStatList[2]),
            ("@base_int", baseStatList[3]),
            ("@base_wis", baseStatList[4]),
            ("@base_cha", baseStatList[5]),

            ("@origin_str", originStatList[0]),
            ("@origin_dex", originStatList[1]),
            ("@origin_con", originStatList[2]),
            ("@origin_int", originStatList[3]),
            ("@origin_wis", originStatList[4]),
            ("@origin_cha", originStatList[5]),

            ("@strength", finalStatsList[0]),
            ("@dexterity", finalStatsList[1]),
            ("@constitution", finalStatsList[2]),
            ("@intelligence", finalStatsList[3]),
            ("@wisdom", finalStatsList[4]),
            ("@charisma", finalStatsList[5]),

            ("@mSTR", modsList[0]),
            ("@mDEX", modsList[1]),
            ("@mCON", modsList[2]),
            ("@mINT", modsList[3]),
            ("@mWIS", modsList[4]),
            ("@mCHA", modsList[5]),

            ("@current_point_buy", currentPointBuy),
            ("@current_origin_points", currentOriginPoints),

            ("@id", PCID)
        );
    }



}
