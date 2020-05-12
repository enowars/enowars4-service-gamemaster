using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnPApp.Database
{
    public class PnPAppDbContextFactory : IDesignTimeDbContextFactory<PnPAppDbContext>
    {
        public static string CONNECTION_STRING = "Data Source=PnPApp.db";
        public PnPAppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PnPAppDbContext>();
            optionsBuilder.UseSqlite(CONNECTION_STRING);
            return new PnPAppDbContext(optionsBuilder.Options);
        }
    }
}
