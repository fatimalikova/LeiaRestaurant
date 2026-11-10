using AutoMapper;
using Leia.Application.Profiles;
using Leia.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Leia.Tests.Helpers
{
    public sealed class TestDb : IDisposable
    {
        private readonly SqliteConnection _connection;

        public AppDbContext Db { get; }
        public IMapper Mapper { get; }

        public TestDb()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            Db = new AppDbContext(options);
            Db.Database.EnsureCreated();

            Mapper = new MapperConfiguration(c => c.AddProfile<MapperProfile>()).CreateMapper();
        }

        public void Dispose()
        {
            Db.Dispose();
            _connection.Dispose();
        }
    }
}