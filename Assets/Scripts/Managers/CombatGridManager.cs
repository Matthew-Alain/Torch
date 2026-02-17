using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using static UnityEngine.Debug;

public class CombatGridManager : MonoBehaviour
{
    public static CombatGridManager Instance { get; private set; }
    private int width, height;
    [SerializeField] private Tile grassTile, mountainTile;
    [SerializeField] private Transform cam;

    private Dictionary<Vector2, Tile> tiles;

    void Awake()
    {
        //Check if an instance already exists that isn't this
        if (Instance != null && Instance != this)
        {
            //If it does, destroy it
            Destroy(gameObject);
            return;
        }

        //This just allows manager scripts to be stored in a folder in the editor for organization, but during runtime, get deteached to avoid errors
        if (transform.parent != null)
        {
            transform.parent = null; // Detach from parent
        }

        //Now safe to create a new instance
        Instance = this;    
        DontDestroyOnLoad(gameObject);
    }

    //Create the grid
    public void GenerateGrid(int encounterID)
    {
        width = 1+Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(   //Get the width of the grid
            "SELECT MAX(x) FROM grid_default_contents WHERE encounter_id = (@encounterID)",
            ("@encounterID", encounterID)
        ));

        height = 1 + Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(    //Get the height of the grid
            "SELECT MAX(y) FROM grid_default_contents WHERE encounter_id = (@encounterID)",
            ("@encounterID", encounterID)
        ));


        tiles = new Dictionary<Vector2, Tile>(); //Create a dictionary of tiles
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int tileID = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar( //Get the character's species
                    "SELECT tile FROM grid_default_contents WHERE encounter_id = (@encounterID) AND x = (@x) AND y = (@y)",
                    ("@encounterID", encounterID),
                    ("@x", x),
                    ("@y", y)
                ));

                Tile tileToSpawn = grassTile;
                switch (tileID)
                {
                    case 0:
                        tileToSpawn = grassTile;
                        break;
                    case 1:
                        tileToSpawn = mountainTile;
                        break;
                    default:
                        Debug.Log("No tile information found for "+x+", "+y+" - defaulting to grass tile");
                        break;
                }
                
                var spawnedTile = Instantiate(tileToSpawn, new Vector3(x, y), Quaternion.identity); //Create that tile
                spawnedTile.name = $"Tile {x}, {y}"; //Name the tile for tracking purposes

                spawnedTile.Init(encounterID, x, y); //Initialize the tile

                tiles[new Vector2(x, y)] = spawnedTile; //Add the tile to the tiles array

            }
        }

        cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10); //Offset the camera for better viewing

        CombatStateManager.Instance.ChangeState(GameState.SpawnHeroes); //Once the grid is generated, now spawn the heroes
    }

    public Tile GetPCSpawnTile(int PCID)
    {
        int x = -1;
        int y = -1;
        // return tiles.Where(t => t.Key.x < width / 2 && t.Value.Walkable).OrderBy(t => UnityEngine.Random.value).First().Value;
        DatabaseManager.Instance.ExecuteReader(
            "SELECT x, y FROM grid_default_contents WHERE content = @PCID",
            reader =>
            {
                while (reader.Read())
                {
                    x = Convert.ToInt32(reader["x"]);
                    y = Convert.ToInt32(reader["y"]);
                }
            },
            ("@PCID", PCID)
        );

        Vector2 key = new Vector2(x, y);
        if (tiles.TryGetValue(key, out Tile tile))
        {
            return tile;
        }
        else
        {
            return null; // or handle missing tile
        }
    }

    public Tile GetMonsterSpawnTile()
    {
        return tiles.Where(t => t.Key.x > width / 2 && t.Value.Walkable).OrderBy(t => UnityEngine.Random.value).First().Value;
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (tiles.TryGetValue(pos, out var tile)) //If the tile is available
        {
            return tile;
        }

        return null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateGrid(0);
    }
}
