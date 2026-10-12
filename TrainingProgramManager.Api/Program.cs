using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TrainingProgramManager.Api.Data;
using TrainingProgramManager.Api.Data.Seed;
using TrainingProgramManager.Api.Entities;
using TrainingProgramManager.Api.Security;
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

// EN: Registers Identity user and role management backed by EF Core; authentication remains JWT bearer-based, without Identity cookie sign-in.
// HU: Regisztrálja az Identity felhasználó- és szerepkörkezelést EF Core tárolással; a hitelesítés továbbra is JWT bearer alapú, Identity cookie-s bejelentkezés nélkül.
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TrainingProgramDbContext>();

// EN: JWT bearer is the default authentication scheme; incoming tokens are validated against the Jwt configuration section (teaching setup, not production).
// HU: A JWT bearer az alapértelmezett hitelesítési séma; a bejövő tokenek ellenőrzése a Jwt konfigurációs szakasz alapján történik (oktatási beállítás, nem éles).
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:SecretKey"]!)),

            NameClaimType = "name",
            ClockSkew = TimeSpan.Zero
        };
    });

// EN: Authentication answers "who is the caller?" (the JWT bearer setup above); authorization answers "what may this caller do?" (the policies below).
// HU: A hitelesítés azt válaszolja meg, hogy "ki a hívó?" (a fenti JWT bearer beállítás); az engedélyezés azt, hogy "mit tehet a hívó?" (az alábbi policy-k).
builder.Services.AddAuthorization(options =>
{
    // EN: OrganizerOnly is role-based: only users in the Organizer role pass; Admin, Participant and Speaker alone do not.
    // HU: Az OrganizerOnly szerepkör alapú: csak az Organizer szerepkörű felhasználók felelnek meg; az Admin, Participant vagy Speaker önmagában nem.
    options.AddPolicy(AuthorizationPolicies.OrganizerOnly, policy =>
        policy.RequireRole(ApplicationRoles.Organizer));

    // EN: EventManagement needs an authenticated user in the Admin OR Organizer role; one RequireRole call with two names means "any of these roles", not "all of them".
    // HU: Az EventManagement hitelesített felhasználót igényel Admin VAGY Organizer szerepkörrel; egyetlen RequireRole hívás két névvel azt jelenti, hogy "bármelyik", nem azt, hogy "mindkettő".
    options.AddPolicy(AuthorizationPolicies.EventManagement, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(ApplicationRoles.Admin, ApplicationRoles.Organizer));

    // EN: VerifiedEmail is claim-based: it checks a specific fact about the user (email_verified = true), which is not a role; no such claim is issued yet, so nobody passes for now.
    // HU: A VerifiedEmail claim alapú: a felhasználóról szóló konkrét tényt ellenőrzi (email_verified = true), ami nem szerepkör; ilyen claim még nem kerül kiadásra, ezért egyelőre senki sem felel meg.
    options.AddPolicy(AuthorizationPolicies.VerifiedEmail, policy =>
        policy.RequireClaim("email_verified", "true"));
});

builder.Services.AddScoped<IWorkshopRepository, WorkshopRepository>();
builder.Services.AddScoped<IWorkshopService, WorkshopService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

// EN: Ensures the application roles exist before the app starts serving requests; safe to run on every startup.
// HU: Gondoskodik róla, hogy az alkalmazás szerepkörei létezzenek, mielőtt az app kiszolgálja a kéréseket; minden indításkor biztonságosan futtatható.
using (var roleScope = app.Services.CreateScope())
{
    var roleDbContext = roleScope.ServiceProvider.GetRequiredService<TrainingProgramDbContext>();

    // EN: Roles are stored in the Identity tables, which exist only after the migrations were applied, so the seeder must not run on an outdated schema.
    // HU: A szerepkörök az Identity táblákban tárolódnak, amelyek csak a migrációk alkalmazása után léteznek, ezért a seeder nem futhat elavult séma mellett.
    // EN: The application only CHECKS migration readiness and never applies migrations itself: schema changes stay an explicit, deliberate step (dotnet ef database update).
    // HU: Az alkalmazás csak ELLENŐRZI a migrációk állapotát, és sosem alkalmazza azokat: a sémamódosítás tudatos, explicit lépés marad (dotnet ef database update).
    // EN: Startup stops with an error instead of continuing, because running against an outdated schema would fail later in less obvious ways.
    // HU: Az indulás hibával leáll a folytatás helyett, mert elavult séma mellett a futás később, nehezebben érthető módon hibázna.
    var pendingMigrations = (await roleDbContext.Database.GetPendingMigrationsAsync()).ToList();

    if (pendingMigrations.Count > 0)
    {
        throw new InvalidOperationException(
            "The database has unapplied EF Core migrations: " + string.Join(", ", pendingMigrations) +
            ". Apply the existing migrations explicitly (for example with 'dotnet ef database update') before starting the application.");
    }

    await IdentitySeeder.SeedRolesAsync(roleScope.ServiceProvider);
}

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
