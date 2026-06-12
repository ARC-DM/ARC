using Microsoft.Data.Sqlite; 

namespace ArcDaemon;

public class AuthenticationHandler
{
    static SqliteConnection _connection = new SqliteConnection("Data Source=test.db");
    
    public void CreateDatabase()
    {
        _connection.Open();
        using var command = _connection.CreateCommand();
        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS CommandList (
                                  Id INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE,
                                  CommandName TEXT NOT NULL UNIQUE,
                                  PermissionLevel TEXT NOT NULL
                              );
                              """;
        
        command.ExecuteNonQuery();

        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS Users (
                                  Id TEXT PRIMARY KEY UNIQUE,
                                  UserName TEXT NOT NULL,
                                  PermissionLevel TEXT NOT NULL
                              );
                              """;
        
        command.ExecuteNonQuery();
    }

    public void AddCommand(string commandName, string permissionLevel)
    {
        _connection.Open();
        using var command = _connection.CreateCommand();
        command.CommandText = """
                              INSERT OR IGNORE INTO CommandList (CommandName, PermissionLevel)
                              VALUES (
                                      $CommandName, $PermissionLevel
                              );
                              """;
        
        command.Parameters.AddWithValue("$CommandName", commandName);
        command.Parameters.AddWithValue("$PermissionLevel", permissionLevel);
        
        command.ExecuteNonQuery();
    }

    public static void AddUser(string username, string permissionLevel)
    {
        _connection.Open();
        using var command = _connection.CreateCommand();
        command.CommandText = """
                              INSERT OR IGNORE INTO Users
                              VALUES (
                                      $Id, $Username, $PermissionLevel
                              );
                              """;
        
        command.Parameters.AddWithValue("$Id", Guid.NewGuid());
        command.Parameters.AddWithValue("$Username", username);
        command.Parameters.AddWithValue("$PermissionLevel", permissionLevel);
        
        command.ExecuteNonQuery();
    }

    public static bool DoesUserExist(string username)
    {
        _connection.Open();
        using var command = _connection.CreateCommand();
        command.CommandText = """
                              SELECT EXISTS(SELECT 1 FROM Users WHERE UserName = $Username);
                              """;
        
        command.Parameters.AddWithValue("$Username", username);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            if (reader.GetString(0) == "0")
            {
                return false;
            }
        }
        return true;
    }
}