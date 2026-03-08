using System.IO;
using UnityEngine;
using System.Collections;
using System.Data;
using Mono.Data.Sqlite;

public class DatabaseManager : MonoBehaviour
{

    //Database configurations
    private const string dbName = "torch.db";
    public static DatabaseManager Instance;
    private string dbPath;
    private SqliteConnection connection;

    //Data flags to be used between scenes, but not stored in the database
    public int lastPCEdited;
    public int lastScene;

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

        dbPath = Path.Combine(Application.persistentDataPath, dbName);

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, "Database", dbName);

        if (File.Exists(dbPath)) //If the database already exists, do nothing
        {
            return;
        }

        // Make sure persistent directory exists
        Directory.CreateDirectory(Application.persistentDataPath);

        File.Copy(sourcePath, dbPath);
        Debug.Log("New database copy created.");
    }

    private void OpenConnection()
    {
        if (connection != null)
            return;

        connection = new SqliteConnection("URI=file:" + dbPath);
        connection.Open();
    }


    public SqliteCommand CreateCommand(string query, params (string, object)[] parameters)
    {
        OpenConnection();

        SqliteCommand command = connection.CreateCommand();
        command.CommandText = query;

        foreach (var (name, value) in parameters)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            command.Parameters.Add(param);
        }

        return command;
    }

    // Use this if you are not expecting a result (like insert, update, delete, etc.)
    public int ExecuteNonQuery(string query, params (string, object)[] parameters)
    {
        using (var cmd = CreateCommand(query, parameters))
        {
            return cmd.ExecuteNonQuery();
        }
    }

    // Syntax:
    // int result = DatabaseManager.Instance.ExecuteNonQuery(
    //     "INSERT INTO Players (Name, Score) VALUES (@name, @score)",
    //     ("@name", "Alice"),
    //     ("@score", 100)
    // );
    // Debug.Log("Rows inserted: " + result);

    // Use this if you are expecting a result
    public object ExecuteScalar(string query, params (string, object)[] parameters)
    {
        using (var cmd = CreateCommand(query, parameters))
        {
            return cmd.ExecuteScalar();
        }
    }

    // Use this if you are expecting multiple rows
    public void ExecuteReader(string query, System.Action<SqliteDataReader> handleRows, params (string, object)[] parameters)
    {
        using (var cmd = CreateCommand(query, parameters))
        using (var reader = cmd.ExecuteReader())
        {
            handleRows(reader);
        }
    }

    // The syntax for using this is:
    // DatabaseManager.Instance.ExecuteReader(
    //     "SELECT dnd_class_1, dnd_class_2 FROM saved_pcs WHERE id = (@PCID)",
    //     reader =>
    //     {
    //         while (reader.Read())
    //         {
    //             if (level.value >=0) classIDs.Add(Convert.ToInt32(reader["dnd_class_1"]));
    //             if (level.value >=1) classIDs.Add(Convert.ToInt32(reader["dnd_class_2"]));
    //         }
    //     },
    //     ("@PCID", PCID)
    // );

    void OnDestroy() //When the application quits
    {
        connection?.Close();
        connection?.Dispose();
        connection = null;
    }

}

