using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using E_commerceApi.Infrastructure.Data;

namespace E_commerceApi.Tests.Helpers;

public static class DbContextFactory
{
    public static (AppDbContext context, SqliteConnection connection) Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }
}
