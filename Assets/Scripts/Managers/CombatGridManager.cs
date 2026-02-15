using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatGridManager : MonoBehaviour
{
    public static CombatGridManager Instance { get; private set; }
    [SerializeField] private int width, height;
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

        //Now safe to create a new instance
        Instance = this;    
        DontDestroyOnLoad(gameObject);
    }

    //Create the grid
    public void GenerateGrid()
    {
        tiles = new Dictionary<Vector2, Tile>(); //Create a dictionary of tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var randomTile = Random.Range(0, 6) == 3 ? mountainTile : grassTile; //Randomly make either a mountain or grass tile
                var spawnedTile = Instantiate(randomTile, new Vector3(x, y), Quaternion.identity); //Create that tile
                spawnedTile.name = $"Tile {x}, {y}"; //Name the tile for tracking purposes

                spawnedTile.Init(x, y); //Initialize the tile

                tiles[new Vector2(x, y)] = spawnedTile; //Add the tile to the tiles array

            }
        }

        cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10); //Offset the camera for better viewing

        CombatManager.Instance.ChangeState(GameState.SpawnHeroes); //Once the grid is generated, now spawn the heroes
    }

    public Tile GetPCSpawnTile()
    {
        return tiles.Where(t => t.Key.x < width / 2 && t.Value.Walkable).OrderBy(t => Random.value).First().Value;
    }

    public Tile GetMonsterSpawnTile()
    {
        return tiles.Where(t => t.Key.x > width / 2 && t.Value.Walkable).OrderBy(t => Random.value).First().Value;
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
        GenerateGrid();
    }
}
