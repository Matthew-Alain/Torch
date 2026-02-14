using System.IO;
using UnityEngine;
using System.Collections;
using System.Data;
using Mono.Data.Sqlite;

public class DatabaseManager : MonoBehaviour
{

    private const string dbName = "torch.db";
    public static DatabaseManager Instance;
    private string dbPath;
    private SqliteConnection connection;

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

        if (transform.parent != null)
        {
            transform.parent = null; // Detach from parent
        }
    
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

    // The syntax to use this is:
    // using (var command = DatabaseManager.Instance.CreateCommand(
    // "SELECT * FROM Players WHERE Id = @id",
    // ("@id", 5)))
    // {
    //     ...
    // }

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
    // "SELECT Id, Name FROM Players",
    // reader =>
    // {
    //     while(reader.Read())
    //     {
    //         Debug.Log(reader["Name"]);
    //     }
    // });

    void OnDestroy() //When the application quits
    {
        connection?.Close();
        connection?.Dispose();
        connection = null;
    }

}

