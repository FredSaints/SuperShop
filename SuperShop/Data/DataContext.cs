using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperShop.Data.Entities;
using System.Linq;

namespace SuperShop.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<Product> Products { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<OrderDetailTemp> OrderDetailTemp { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<City> Cities { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        /// <summary>
        /// Quando criamos o modelo, primeiro vamos buscar todas as fk de todas as tabelas
        /// que têm o comportamento de apagar em cascata, e alteramos esse comportamento para restrict.
        /// </summary>
        /// <param name="modelBuilder"></param>
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    var cascadeFKs = modelBuilder.Model
        //        .GetEntityTypes()
        //        .SelectMany(t => t.GetForeignKeys())
        //        .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

        //    foreach (var fk in cascadeFKs)
        //    {
        //        fk.DeleteBehavior = DeleteBehavior.Restrict;
        //    }

        //    base.OnModelCreating(modelBuilder);
        //}
    }
}
