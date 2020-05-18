using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gamemaster.Database
{
    public class GamemasterDbContextFactory : IDesignTimeDbContextFactory<GamemasterDbContext>
    {
        public static string CONNECTION_STRING = "Data Source=Gamemaster.db";
        public GamemasterDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GamemasterDbContext>();
            optionsBuilder.UseSqlite(CONNECTION_STRING);
            return new GamemasterDbContext(optionsBuilder.Options);
        }
    }
}
