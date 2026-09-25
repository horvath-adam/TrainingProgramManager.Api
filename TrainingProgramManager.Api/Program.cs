using Microsoft.EntityFrameworkCore;
using TrainingProgramManager.Api.Data;
using TrainingProgramManager.Api.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<TrainingProgramDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// EN: EnsureCreatedAsync is a teaching shortcut for this module; migrations are covered later.
// HU: Az EnsureCreatedAsync egy tanítási célú megoldás ebben a modulban, a migrációkról később lesz szó.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TrainingProgramDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    if (!await dbContext.Workshops.AnyAsync())
    {
        var conferenceEvent = new Event { Name = "Dev Konferencia 2026" };

        var mainRoom = new Room { Name = "Nagyterem", Capacity = 120 };
        var workshopRoom = new Room { Name = "Workshop terem", Capacity = 40 };

        var kovacsSpeaker = new Speaker { FullName = "Kovács Anna" };
        var nagySpeaker = new Speaker { FullName = "Nagy Bence" };

        var backendTag = new Tag { Name = "backend" };
        var databaseTag = new Tag { Name = "database" };
        var securityTag = new Tag { Name = "security" };

        var aspNetWorkshop = new Workshop
        {
            Title = "ASP.NET Core alapok",
            StartsAt = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.FromHours(2)),
            EndsAt = new DateTimeOffset(2026, 10, 1, 11, 0, 0, TimeSpan.FromHours(2)),
            Event = conferenceEvent,
            Room = mainRoom,
            Speaker = kovacsSpeaker,
            Tags = new List<Tag> { backendTag }
        };

        var efCoreWorkshop = new Workshop
        {
            Title = "EF Core kapcsolatok",
            StartsAt = new DateTimeOffset(2026, 10, 1, 11, 30, 0, TimeSpan.FromHours(2)),
            EndsAt = new DateTimeOffset(2026, 10, 1, 13, 30, 0, TimeSpan.FromHours(2)),
            Event = conferenceEvent,
            Room = workshopRoom,
            Speaker = kovacsSpeaker,
            Tags = new List<Tag> { backendTag, databaseTag }
        };

        var jwtWorkshop = new Workshop
        {
            Title = "JWT authentication alapok",
            StartsAt = new DateTimeOffset(2026, 10, 1, 14, 0, 0, TimeSpan.FromHours(2)),
            EndsAt = new DateTimeOffset(2026, 10, 1, 16, 0, 0, TimeSpan.FromHours(2)),
            Event = conferenceEvent,
            Room = workshopRoom,
            Speaker = nagySpeaker,
            Tags = new List<Tag> { backendTag, securityTag }
        };

        var participant = new Participant { DisplayName = "Tóth Gábor", Email = "toth.gabor@example.com" };

        var registration = new Registration
        {
            Participant = participant,
            Workshop = efCoreWorkshop,
            RegisteredAt = DateTimeOffset.UtcNow,
            Status = "Confirmed"
        };

        participant.Registrations.Add(registration);
        efCoreWorkshop.Registrations.Add(registration);

        dbContext.Workshops.AddRange(aspNetWorkshop, efCoreWorkshop, jwtWorkshop);
        dbContext.Registrations.Add(registration);

        await dbContext.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // EN: Swagger UI reads the built-in OpenAPI document, it does not generate its own.
    // HU: A Swagger UI a beépített OpenAPI dokumentumot olvassa, nem generál sajátot.
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "TrainingProgramManager.Api v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
