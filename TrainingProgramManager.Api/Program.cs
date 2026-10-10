using Microsoft.EntityFrameworkCore;
using TrainingProgramManager.Api.Data;
using TrainingProgramManager.Api.Entities;
using TrainingProgramManager.Api.Services.Auth;
using TrainingProgramManager.Api.Services.Workshops;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// EN: Registers the services that produce standard RFC 9457 ProblemDetails responses.
// HU: Regisztrálja a szabványos RFC 9457 ProblemDetails válaszokat előállító szolgáltatásokat.
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<TrainingProgramDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// EN: Registers the core Identity services (user management) backed by EF Core; no sign-in, roles or authentication yet.
// HU: Regisztrálja az alap Identity szolgáltatásokat (felhasználókezelés) EF Core tárolással; bejelentkezés, szerepkörök és hitelesítés még nincs.
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<TrainingProgramDbContext>();

builder.Services.AddScoped<IWorkshopRepository, WorkshopRepository>();
builder.Services.AddScoped<IWorkshopService, WorkshopService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TrainingProgramDbContext>();

    // EN: IgnoreQueryFilters() makes the seed guard see soft-deleted rows too, so seeding runs only when the table is physically empty.
    // HU: Az IgnoreQueryFilters() miatt a seed ellenőrzés a soft delete-tel törölt sorokat is látja, így a seedelés csak fizikailag üres táblánál fut le.
    if (!await dbContext.Workshops.IgnoreQueryFilters().AnyAsync())
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
else
{
    // EN: Outside Development, unhandled exceptions are turned into ProblemDetails responses without leaking details.
    // HU: Fejlesztési környezeten kívül a kezeletlen kivételek ProblemDetails válaszokká alakulnak részletek kiszivárogtatása nélkül.
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
