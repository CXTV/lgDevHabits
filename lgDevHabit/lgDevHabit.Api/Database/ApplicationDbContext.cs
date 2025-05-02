using lgDevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace lgDevHabit.Api.Database;


public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Habit> Habits { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<HabitTag> HabitTags { get; set; }
    //用户表
    public DbSet<User> Users { get; set; }
    //github Token表
    public DbSet<GitHubAccessToken> GitHubAccessTokens { get; set; }
    //Entry表
    public DbSet<Entry> Entries { get; set; }
    // EntryImportJob表
    public DbSet<EntryImportJob> EntryImportJobs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Application);

        //自动加载所有DbSet<T>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

    }
}
