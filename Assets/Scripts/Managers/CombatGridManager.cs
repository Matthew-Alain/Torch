using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using static UnityEngine.Debug;
using UnityEngine.EventSystems;

public class CombatGridManager : MonoBehaviour
{
    public static CombatGridManager Instance { get; private set; }
    private int width, height;
    [SerializeField] private Tile grassTile, mountainTile;
    [SerializeField] private Transform cam;


    public List<Tile> tilesList = new List<Tile>();
    public Dictionary<Vector2, Tile> tiles;

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
        // DontDestroyOnLoad(gameObject);
    }

    //Create the grid
    public void GenerateGrid(int encounterID)
    {
        width = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(   //Get the width of the grid
            $"SELECT MAX(x) FROM grid_contents WHERE encounter_id = {encounterID}"
        ));

        height = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(    //Get the height of the grid
            $"SELECT MAX(y) FROM grid_contents WHERE encounter_id = {encounterID}"
        ));


        tiles = new Dictionary<Vector2, Tile>(); //Create a dictionary of tiles
        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                int tileID = 0;
                int tileType = 0;
                
                DatabaseManager.Instance.ExecuteReader(
                    $"SELECT tile_type, tile_id FROM grid_contents WHERE encounter_id = {encounterID} AND x = {x} AND y = {y}",
                    reader =>
                    {
                        tileType = Convert.ToInt32(reader["tile_type"]);
                        tileID = Convert.ToInt32(reader["tile_id"]);
                    }
                );

                Tile tileToSpawn = grassTile;
                switch (tileType)
                {
                    case 0:
                        tileToSpawn = grassTile;
                        break;
                    case 1:
                        tileToSpawn = mountainTile;
                        break;
                    default:
                        Log("No tile information found for "+x+", "+y+" - defaulting to grass tile");
                        break;
                }
                
                var spawnedTile = Instantiate(tileToSpawn, new Vector3(x, y), Quaternion.identity); //Create that tile
                spawnedTile.name = $"Tile {x}, {y}"; //Name the tile for tracking purposes

                spawnedTile.Init(encounterID, tileID, x, y); //Initialize the tile

                tilesList.Add(spawnedTile);

                tiles[new Vector2(x, y)] = spawnedTile; //Add the tile to the tiles array

            }
        }

        // cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10); //Offset the camera for better viewing

        CombatStateManager.Instance.ChangeState(GameState.SpawnHeroes); //Once the grid is generated, now spawn the heroes
    }

    public Tile GetPCSpawnTile(int PCID, int encounterID)
    {
        int x = -1;
        int y = -1;

        DatabaseManager.Instance.ExecuteReader(
            $"SELECT x, y FROM grid_contents WHERE unit_id = {PCID} AND encounter_id = {encounterID}",
            reader =>
            {
                while (reader.Read())
                {
                    x = Convert.ToInt32(reader["x"]);
                    y = Convert.ToInt32(reader["y"]);
                }
            }
        );

        Vector2 key = new Vector2(x, y);
        if (tiles.TryGetValue(key, out Tile tile))
        {
            return tile;
        }
        else
        {
            LogWarning("There is a missing tile for unit " + PCID);
            return null; // or handle missing tile
        }
    }

    public Tile GetMonsterSpawnTile(int monsterID, int encounterID)
    {
        int x = -1;
        int y = -1;
        DatabaseManager.Instance.ExecuteReader(
            $"SELECT x, y FROM grid_contents WHERE unit_id = {monsterID} AND encounter_id = {encounterID}",
            reader =>
            {
                while (reader.Read())
                {
                    x = Convert.ToInt32(reader["x"]);
                    y = Convert.ToInt32(reader["y"]);
                }
            }
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
        // GenerateGrid(DatabaseManager.Instance.encounterToLoad);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }
}
