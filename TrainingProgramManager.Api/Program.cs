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
        dbContext.Workshops.AddRange(
            new Workshop { Title = "ASP.NET Core alapok", Tag = "backend" },
            new Workshop { Title = "EF Core bevezetés", Tag = "database" },
            new Workshop { Title = "JWT authentication alapok", Tag = "security" });

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
