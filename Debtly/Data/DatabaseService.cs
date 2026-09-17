using Debtly.Models;
using Microsoft.Data.Sqlite;

namespace Debtly.Data;

public class DatabaseService
{
    private readonly string _databasePath;

    public DatabaseService()
    {
        _databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "debtly.db3");
    }

    public async Task InitializeAsync()
    {
        await using var connection = new SqliteConnection(
            $"Data Source={_databasePath}");

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            CREATE TABLE IF NOT EXISTS People
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Transactions
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PersonId INTEGER NOT NULL,
                Amount REAL NOT NULL,
                IsFromPerson INTEGER NOT NULL,
                Note TEXT,
                Date TEXT NOT NULL,
                FOREIGN KEY (PersonId) REFERENCES People(Id)
            );
            """;

        await command.ExecuteNonQueryAsync();
    }

    public async Task AddPersonAsync(Person person)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={_databasePath}");

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO People (Name)
            VALUES ($name);
            """;

        command.Parameters.AddWithValue("$name", person.Name);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdatePersonAsync(Person person)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={_databasePath}");

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE People
            SET Name = $name
            WHERE Id = $id;
            """;

        command.Parameters.AddWithValue("$name", person.Name);
        command.Parameters.AddWithValue("$id", person.Id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeletePersonAsync(int personId)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={_databasePath}");

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            DELETE FROM Transactions
            WHERE PersonId = $personId;

            DELETE FROM People
            WHERE Id = $personId;
            """;

        command.Parameters.AddWithValue("$personId", personId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Person>> GetPeopleAsync()
    {
        var people = new List<Person>();

        await using var connection = new SqliteConnection(
            $"Data Source={_databasePath}");

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            SELECT Id, Name
            FROM People
            ORDER BY Id DESC;
            """;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            people.Add(new Person
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }

        return people;
    }

    public async Task AddTransactionAsync(Transaction transaction)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={_databasePath}");

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO Transactions
            (
                PersonId,
                Amount,
                IsFromPerson,
                Note,
                Date
            )
            VALUES
            (
                $personId,
                $amount,
                $isFromPerson,
                $note,
                $date
            );
            """;

        command.Parameters.AddWithValue(
            "$personId",
            transaction.PersonId);

        command.Parameters.AddWithValue(
            "$amount",
            transaction.Amount);

        command.Parameters.AddWithValue(
            "$isFromPerson",
            transaction.IsFromPerson ? 1 : 0);

        command.Parameters.AddWithValue(
            "$note",
            transaction.Note);

        command.Parameters.AddWithValue(
            "$date",
            transaction.Date.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Transaction>> GetTransactionsForPersonAsync(
        int personId)
    {
        var transactions = new List<Transaction>();

        await using var connection = new SqliteConnection(
            $"Data Source={_databasePath}");

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                Id,
                PersonId,
                Amount,
                IsFromPerson,
                Note,
                Date
            FROM Transactions
            WHERE PersonId = $personId
            ORDER BY Date DESC;
            """;

        command.Parameters.AddWithValue(
            "$personId",
            personId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            transactions.Add(new Transaction
            {
                Id = reader.GetInt32(0),
                PersonId = reader.GetInt32(1),
                Amount = reader.GetDecimal(2),
                IsFromPerson = reader.GetInt32(3) == 1,
                Note = reader.IsDBNull(4)
                    ? ""
                    : reader.GetString(4),
                Date = DateTime.Parse(
                    reader.GetString(5))
            });
        }

        return transactions;
    }

    public async Task<List<Transaction>> GetAllTransactionsAsync()
    {
        var transactions = new List<Transaction>();

        await using var connection = new SqliteConnection(
            $"Data Source={_databasePath}");

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                Id,
                PersonId,
                Amount,
                IsFromPerson,
                Note,
                Date
            FROM Transactions
            ORDER BY Date DESC;
            """;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            transactions.Add(new Transaction
            {
                Id = reader.GetInt32(0),
                PersonId = reader.GetInt32(1),
                Amount = reader.GetDecimal(2),
                IsFromPerson = reader.GetInt32(3) == 1,
                Note = reader.IsDBNull(4)
                    ? ""
                    : reader.GetString(4),
                Date = DateTime.Parse(
                    reader.GetString(5))
            });
        }

        return transactions;
    }

    public async Task UpdateTransactionAsync(
        Transaction transaction)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={_databasePath}");

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE Transactions
            SET
                Amount = $amount,
                IsFromPerson = $isFromPerson,
                Note = $note,
                Date = $date
            WHERE Id = $id;
            """;

        command.Parameters.AddWithValue(
            "$amount",
            transaction.Amount);

        command.Parameters.AddWithValue(
            "$isFromPerson",
            transaction.IsFromPerson ? 1 : 0);

        command.Parameters.AddWithValue(
            "$note",
            transaction.Note);

        command.Parameters.AddWithValue(
            "$date",
            transaction.Date.ToString("O"));

        command.Parameters.AddWithValue(
            "$id",
            transaction.Id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteTransactionAsync(
        int transactionId)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={_databasePath}");

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            DELETE FROM Transactions
            WHERE Id = $id;
            """;

        command.Parameters.AddWithValue(
            "$id",
            transactionId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAllDataAsync()
    {
        await using var connection = new SqliteConnection(
            $"Data Source={_databasePath}");

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            DELETE FROM Transactions;
            DELETE FROM People;
            """;

        await command.ExecuteNonQueryAsync();
    }
}