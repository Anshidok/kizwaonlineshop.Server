using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using kizwaonlineshop.Server.Model;

namespace kizwaonlineshop.Server.Data
{
    public class kizwacartContext : DbContext
    {
        public kizwacartContext(DbContextOptions<kizwacartContext> options)
            : base(options)
        {
        }

        public DbSet<User> user { get; set; }
        public DbSet<Products_master> product { get; set; }
        public DbSet<Product_Cart> prodcart { get; set; }
    }

}
