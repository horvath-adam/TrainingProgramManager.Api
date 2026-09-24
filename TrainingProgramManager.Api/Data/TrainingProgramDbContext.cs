using Microsoft.EntityFrameworkCore;
using TrainingProgramManager.Api.Entities;

namespace TrainingProgramManager.Api.Data
{
    public class TrainingProgramDbContext : DbContext
    {
        public TrainingProgramDbContext(DbContextOptions<TrainingProgramDbContext> options)
            : base(options)
        {
        }

        public DbSet<Workshop> Workshops => Set<Workshop>();
    }
}
