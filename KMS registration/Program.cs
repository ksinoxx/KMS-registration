using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlite("Data Source=KMS.db"));
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", (ApplicationContext db) =>
{
    return db.KMSs.ToList();
});
app.MapPost("/", ([FromBody] KMS dto, ApplicationContext db) => 
{
    var validationResults = new List<ValidationResult>();
    if (!Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true))
    {
        return Results.BadRequest(validationResults);
    }
    dto.Id = Guid.NewGuid();
    db.KMSs.Add(dto);
    db.SaveChanges();
    return Results.Created($"/{dto.Numphone}", dto);
});
app.MapPut("/", ([FromQuery] Guid id, UpdateKMSDTO dto, ApplicationContext db) => 
{
    KMS buffer = db.KMSs.FirstOrDefault(b => b.Id == id);
    if (buffer == null)
    {
        return Results.NotFound();
    }
    var updatedKMS = new KMS
    {
        Name = dto.name,
        Firstname = dto.firstname,
        Numphone = dto.numphone
    };
    var validationResults = new List<ValidationResult>();
    if (!Validator.TryValidateObject(buffer, new ValidationContext(buffer), validationResults, true))
    { return Results.BadRequest(validationResults); }
    db.SaveChanges();
    return Results.Json(buffer);
});
app.MapDelete("/", ([FromQuery] Guid id, ApplicationContext db) => 
{
    KMS buffer = db.KMSs.FirstOrDefault(b => b.Id == id); ;
    db.KMSs.Remove(buffer);
    db.SaveChanges();
});
app.Run();


public class KMS
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Firstname { get; set; }
    public string Patronymic { get; set; }
    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^(?:\+?(\d{1}))?[-. (]?\d{3}[-. )]?\d{3}[-. ]?\d{4}$", ErrorMessage = "Incorrect phone number format")]
    public string Numphone { get; set; }
}
public class ApplicationContext : DbContext
{
    public DbSet<KMS> KMSs { get; set; } = null!;
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
}
record class UpdateKMSDTO (string name, string firstname,string numphone);