using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<OfficerDb>(opt => opt.UseInMemoryDatabase("OfficerList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
    app.UseSwagger();
    app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello!");

app.MapGet("/officers", async (OfficerDb db) =>
   await db.Officers.ToListAsync());

app.MapGet("/officers/{id}", async (int id, OfficerDb db) =>
    await db.Officers.FindAsync(id)
        is Officer officer
            ? Results.Ok(officer)
            : Results.NotFound());

//app.MapGet("/officers/{officeID}", async (int officeID, OfficerDb db) =>
//    await db.Officers.FindAsync(officeID)
//        is Officer officer
//            ? Results.Ok(officer)
//            : Results.NotFound());

app.MapGet("/office/{OfficeID}", async (int OfficeID, OfficerDb db) =>
    await db.Officers.Where(x => x.OfficeId == OfficeID).ToListAsync());




app.MapPost("/officers", async (Officer officer, OfficerDb db) =>
{
    officer.Created = DateTime.Now;
    db.Officers.Add(officer);
    await db.SaveChangesAsync();

    return Results.Created($"/officers/{officer.Id}", officer);
});

app.MapPut("/officers/{id}", async (int id, Officer inputOfficer, OfficerDb db) =>
{
    var officer = await db.Officers.FindAsync(id);

    if (officer is null) return Results.NotFound();

    officer.Name = inputOfficer.Name;
    officer.Position = inputOfficer.Position;
	officer.Department = inputOfficer.Department;
	officer.ImageURL = inputOfficer.ImageURL;
    officer.Description = inputOfficer.Description;
    officer.OfficeId = inputOfficer.OfficeId;


    //officer.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/officers/{id}", async (int id, OfficerDb db) =>
{
    if (await db.Officers.FindAsync(id) is Officer todo)
    {
        db.Officers.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

app.MapDelete("/officers", async (OfficerDb db) =>
{
    await db.Database.EnsureDeletedAsync();
    await db.SaveChangesAsync();
    return Results.Ok(null);

});

app.Run();

class Officer
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
    public string? ImageURL { get; set; }
	public byte[]? Image { get; set; } = null;
	public string? Description { get; set; }
    public int OfficeId { get; set; }
    public DateTime Created { get; set; }
  

}

class OfficerDb : DbContext
{
    public OfficerDb(DbContextOptions<OfficerDb> options) : base(options)
    {
    }

    public DbSet<Officer> Officers => Set<Officer>();
}
