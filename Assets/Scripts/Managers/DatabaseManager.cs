using SQLite;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{

    SQLiteConnection db;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "torch.db");
        db = new SQLiteConnection(path);
        Debug.Log("Database path: " + path);

        db.CreateTable<PlayerData>();
        Debug.Log("PlayerData created");

        db.CreateTable<saved_objects>();
        Debug.Log("created saved_objects table");

        db.CreateTable<encounters>();
        Debug.Log("Created encounters table");

        db.CreateTable<tutorials>();
        Debug.Log("Created tutorials table");

        db.CreateTable<grid_contents>();
        Debug.Log("Created grid_contents table");

        db.CreateTable<monsters>();
        Debug.Log("Created monsters table");
        
        db.CreateTable<monster_features>();
        Debug.Log("Created monster_features table");

        db.CreateTable<monster_actions>();
        Debug.Log("Created monster_actions table");

        db.CreateTable<monster_attacks>();
        Debug.Log("Created monster_attacks table");
        
        db.CreateTable<saved_pcs>();
        Debug.Log("Created saved_pcs table");

        db.CreateTable<species>();
        Debug.Log("Created species table");

        db.CreateTable<dndclasses>();
        Debug.Log("Created dndclasses table");

        db.CreateTable<subclasses>();
        Debug.Log("Created subclasses table");

        db.CreateTable<features>();
        Debug.Log("Created features table");

        db.CreateTable<spells>();
        Debug.Log("Created spells table");

        db.CreateTable<weapons>();
        Debug.Log("Created weapons table");

        db.CreateTable<damage_types>();
        Debug.Log("Created damage_types table");

        PlayerData player = new PlayerData
        {
            PlayerName = "Test",
            Level = 5,
            Gold = 250
        };

        db.Insert(player);

        var players = db.Table<PlayerData>().ToList();

        foreach (var p in players)
        {
            Debug.Log(p.PlayerName + " Lv. " + p.Level + " gold. " + p.Gold);
        }

        var playerTest = db.Table<PlayerData>().Where(p => p.PlayerName == "Test").FirstOrDefault();

        playerTest.Gold += 100;
        db.Update(playerTest);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public class PlayerData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string PlayerName { get; set; }
        public int Level { get; set; }
        public int Gold { get; set; }
    }

    public class saved_objects
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string game_object { get; set; }
        public int grid_position { get; set; }
    }

    public class encounters
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public string difficulty { get; set; }
        public string description { get; set; }
        public int tutorials { get; set; }
        public string background_filename { get; set; }
        public int grid_contents { get; set; }
    }

    public class tutorials
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool skipped { get; set; }
    }

    public class grid_contents
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int content { get; set; }
    }

    public class monsters
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public int strength { get; set; }
        public int dexterity { get; set; }
        public int constitution { get; set; }
        public int intelligence { get; set; }
        public int wisdom { get; set; }
        public int charisma { get; set; }
        public int hp { get; set; }
        public int ac { get; set; }
        public int features { get; set; }
        public int actions { get; set; }
    }

    public class monster_features
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class monster_actions
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int chance { get; set; }
        public int attack { get; set; }
    }

    public class monster_attacks
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public int hit_modifier { get; set; }
        public int dice_number { get; set; }
        public int dice_size { get; set; }
        public int damage_type { get; set; }
        public int range { get; set; }
    }

    public class saved_pcs
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public int species { get; set; }
        public int dndclass { get; set; }
        public int level { get; set; }
        public int strength { get; set; }
        public int dexterity { get; set; }
        public int constitution { get; set; }
        public int intelligence { get; set; }
        public int wisdom { get; set; }
        public int charisma { get; set; }
        public string armour { get; set; }
        public int prepared_spells { get; set; }
        public int inventory { get; set; }
    }

    public class species
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public int lineages { get; set; }
        public string features { get; set; }
    }

    public class dndclasses
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public int subclasses { get; set; }
        public bool spellcaster { get; set; }
        public string features { get; set; }
        public int known_spells { get; set; }
    }

    public class subclasses
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public bool spellcaster { get; set; }
        public string features { get; set; }
    }

    public class features
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string source { get; set; }
        public int level { get; set; }
    }

    public class spells
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int level { get; set; }
        public string school { get; set; }
        public string cast_time { get; set; }
        public bool requires_concentration { get; set; }
        public int duration { get; set; }
        public int range { get; set; }
        public int area_size { get; set; }
        public string area_shape { get; set; }
        public string valid_targets { get; set; }
        public string save_type { get; set; }
        public int dice_number { get; set; }
        public int dice_size { get; set; }
        public int damage_type { get; set; }
        public int classes { get; set; }
    }
    
    public class weapons
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public int dice_number { get; set; }
        public int dice_size { get; set; }
        public int damage_type { get; set; }
        public int range { get; set; }
        public string stat { get; set; }
    }

    public class damage_types
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    void OnApplicationQuit()
    {
        db.Close();
    }
}

