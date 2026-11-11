using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using TskMgmt.Api;

namespace TskMgmt.Api.Context
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Entities.Task> Tasks { get; set; }
        public DbSet<Entities.Project> Projects { get; set; }
        public DbSet<Entities.User> Users { get; set; }
        public DbSet<Entities.Comment> Comments { get; set; }
        public DbSet<Entities.RevokedToken> RevokedTokens { get; set; }
        public DbSet<Entities.RefreshToken> RefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            //Just added sequeantial guid generator for the Ids of the entities.
            //This will generate a sequential guid for the Ids of the entities.
            //This will help to improve the performance of the database.
            //This is only situational, if you want to use sequential guid generator for the Ids of the entities, you can use this.

            modelBuilder.Entity<Entities.Task>() // Create a guid Id Sequencial
                .Property(u => u.Id)
                .HasValueGenerator<SequentialGuidValueGenerator>();
            modelBuilder.Entity<Entities.Project>() 
                .Property(u => u.Id)
                .HasValueGenerator<SequentialGuidValueGenerator>();
            modelBuilder.Entity<Entities.User>()
                .Property(u => u.Id)
                .HasValueGenerator<SequentialGuidValueGenerator>();
            modelBuilder.Entity<Entities.Comment>()
                .Property(u => u.Id)
                .HasValueGenerator<SequentialGuidValueGenerator>();

            //RevokedToken entity configuration
            modelBuilder.Entity<Entities.RevokedToken>()
                .HasIndex(r => r.TokenId) //Ef will create a unique index on the TokenId column in the RevokedTokens table.
                .IsUnique(); // Ensure TokenId is unique


            //This uses Fluent API configuration to configure the RefreshToken entity.
            modelBuilder.Entity<Entities.RefreshToken>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.TokenHash)
                    .IsRequired()
                    .HasMaxLength(64);

                e.HasIndex(x => x.TokenHash)
                    .IsUnique();

                e.Property(x => x.UserId)
                    .IsRequired();

                e.HasIndex(x => x.UserId);

            });
        }
    }
}
