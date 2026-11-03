using System.Data;
using Microsoft.EntityFrameworkCore;
using BackendGeneral.Api.Data;

namespace BackendGeneral.Api.Services;

public class DbViewsService
{
    private readonly AppDbContext _db;
    public DbViewsService(AppDbContext db) => _db = db;

    public async Task<List<Dictionary<string, object?>>> QueryViewAsync(
        string sql, params (string name, object? value)[] args)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in args)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        var rows = new List<Dictionary<string, object?>>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
            rows.Add(row);
        }
        return rows;
    }

    public async Task<T?> ScalarAsync<T>(string sql, params (string, object?)[] args)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in args)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
        var r = await cmd.ExecuteScalarAsync();
        return r is null or DBNull ? default : (T)Convert.ChangeType(r, typeof(T));
    }
}