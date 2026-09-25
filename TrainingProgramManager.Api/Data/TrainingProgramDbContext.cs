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

        public DbSet<Event> Events => Set<Event>();

        public DbSet<Room> Rooms => Set<Room>();

        public DbSet<Speaker> Speakers => Set<Speaker>();

        public DbSet<Tag> Tags => Set<Tag>();

        public DbSet<Participant> Participants => Set<Participant>();

        public DbSet<Registration> Registrations => Set<Registration>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // EN: Event 1:N Workshop through Workshop.EventId.
            // HU: Event 1:N Workshop kapcsolat a Workshop.EventId mezőn keresztül.
            modelBuilder.Entity<Workshop>()
                .HasOne(w => w.Event)
                .WithMany(e => e.Workshops)
                .HasForeignKey(w => w.EventId);

            // EN: Room 1:N Workshop through Workshop.RoomId.
            // HU: Room 1:N Workshop kapcsolat a Workshop.RoomId mezőn keresztül.
            modelBuilder.Entity<Workshop>()
                .HasOne(w => w.Room)
                .WithMany(r => r.Workshops)
                .HasForeignKey(w => w.RoomId);

            // EN: Speaker 1:N Workshop through Workshop.SpeakerId.
            // HU: Speaker 1:N Workshop kapcsolat a Workshop.SpeakerId mezőn keresztül.
            modelBuilder.Entity<Workshop>()
                .HasOne(w => w.Speaker)
                .WithMany(s => s.Workshops)
                .HasForeignKey(w => w.SpeakerId);

            // EN: Workshop 1:N Registration through Registration.WorkshopId.
            // HU: Workshop 1:N Registration kapcsolat a Registration.WorkshopId mezőn keresztül.
            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Workshop)
                .WithMany(w => w.Registrations)
                .HasForeignKey(r => r.WorkshopId);

            // EN: Participant 1:N Registration through Registration.ParticipantId.
            // HU: Participant 1:N Registration kapcsolat a Registration.ParticipantId mezőn keresztül.
            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Participant)
                .WithMany(p => p.Registrations)
                .HasForeignKey(r => r.ParticipantId);

            // EN: Workshop N:N Tag through the collection navigations.
            // HU: Workshop N:N Tag kapcsolat a gyűjtemény navigációkon keresztül.
            modelBuilder.Entity<Workshop>()
                .HasMany(w => w.Tags)
                .WithMany(t => t.Workshops);

            base.OnModelCreating(modelBuilder);
        }
    }
}
