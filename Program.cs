/*
 * API: ROI API
 * Description: This API provides endpoints for managing people and departments in the ROI system.
 * Version: 1.0
 * Developer: [Your Name]
 * Date: [Current Date]
 *
 * Notes:
 * - This API follows RESTful principles for its design.
 * - Endpoints are provided for CRUD operations on people and departments.
 * - Uses Entity Framework Core for database operations.
 * - Implements OpenAPI (Swagger) for documentation and testing.
 *
 * Change Log:
 * [Date] [Developer Name] [Change Description]
 * [Date] [Developer Name] [Change Description]
 *
 * To-do:
 * - Implement additional validation for input data.
 * - Add support for authentication and authorization.
 * - Enhance error handling to provide more informative responses.
 *
 * Contributors:
 * - [Contributor 1]
 * - [Contributor 2]
 * - [Contributor 3]
 */

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RoiDatabaseContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        // Allow SPECIFIC origins to access our API
        //policy.WithOrigins("http://localhost:19006", "https://app.roi.com.au");

        builder.AllowAnyOrigin()    // Allow ANY origin to access our API
               .AllowAnyHeader()    // Allow any HTTP header
               .AllowAnyMethod();
    });

    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

    options.AddPolicy("AllowLocalhost8080", builder =>
    {
        builder.WithOrigins("http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();


// Enable CORS (default policy)
app.UseCors();
// app.UseCors("AllowLocalhost8080");
// app.UseCors("AllowAll");


// GET all departments
app.MapGet("/api/departments", async (RoiDatabaseContext dbContext) =>
{
    var departments = await dbContext.Departments.ToListAsync();
    return Results.Ok(departments);
});

// GET a department by ID
app.MapGet("/api/departments/{id}", async (int id, RoiDatabaseContext dbContext) =>
{
    var department = await dbContext.Departments.FindAsync(id);

    if (department == null)
    {
        return Results.NotFound("Department not found.");
    }

    return Results.Ok(department);
});

// Add a new department
app.MapPost("/api/departments", async (RoiDatabaseContext dbContext, Department department) =>
{
    dbContext.Departments.Add(department);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/departments/{department.Id}", department);
}).RequireCors("AllowAll");

// Update a department
app.MapPut("/api/departments/{id}", async (int id, RoiDatabaseContext dbContext, Department updatedDepartment) =>
{
    var existingDepartment = await dbContext.Departments.FindAsync(id);

    if (existingDepartment == null)
    {
        return Results.NotFound("Department not found.");
    }

    existingDepartment.Name = updatedDepartment.Name;

    await dbContext.SaveChangesAsync();

    return Results.Ok(existingDepartment);
}).RequireCors("AllowAll");

// DELETE a department
app.MapDelete("/api/departments/{id}", async (int id, RoiDatabaseContext dbContext) =>
{
    var department = await dbContext.Departments.FindAsync(id);

    if (department == null)
    {
        return Results.NotFound("Department not found.");
    }

    dbContext.Departments.Remove(department);
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
}).RequireCors("AllowAll");

// GET all people
app.MapGet("/api/people", async (RoiDatabaseContext dbContext) =>
{
    var people = await dbContext.People
        .Include(p => p.Department)
        .ToListAsync();

    return Results.Ok(people);
});

// GET a person by ID
app.MapGet("/api/people/{id}", async (int id, RoiDatabaseContext dbContext) =>
{
    var person = await dbContext.People
        .Include(p => p.Department)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (person == null)
    {
        return Results.NotFound("Person not found.");
    }

    return Results.Ok(person);
});

// Add a new person
app.MapPost("/api/people", async (RoiDatabaseContext dbContext, Person person) =>
{
    dbContext.People.Add(person);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/people/{person.Id}", person);
}).RequireCors("AllowAll");

// Update a person
app.MapPut("/api/people/{id}", async (int id, RoiDatabaseContext dbContext, Person updatedPerson) =>
{
    Console.WriteLine(updatedPerson);

    var existingPerson = await dbContext.People.FindAsync(id);

    if (existingPerson == null)
    {
        return Results.NotFound("Person not found.");
    }

    existingPerson.Name = updatedPerson.Name;
    existingPerson.Phone = updatedPerson.Phone;
    existingPerson.DepartmentId = updatedPerson.DepartmentId;
    existingPerson.Street = updatedPerson.Street;
    existingPerson.City = updatedPerson.City;
    existingPerson.State = updatedPerson.State;
    existingPerson.Zip = updatedPerson.Zip;
    existingPerson.Country = updatedPerson.Country;

    await dbContext.SaveChangesAsync();

    return Results.Ok(existingPerson);
}).RequireCors("AllowAll");

// DELETE a person
app.MapDelete("/api/people/{id}", async (int id, RoiDatabaseContext dbContext) =>
{
    var person = await dbContext.People.FindAsync(id);

    if (person == null)
    {
        return Results.NotFound("Person not found.");
    }

    dbContext.People.Remove(person);
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
}).RequireCors("AllowAll");


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RoiDatabaseContext>();
    await SeedData(dbContext);
}

app.Run();

async Task SeedData(RoiDatabaseContext context)
{
    if (!context.Departments.Any())
    {
        var departments = new List<Department>
        {
            new Department { Name = "General" },
            new Department { Name = "Information Communications Technology" },
            new Department { Name = "Finance" },
            new Department { Name = "Marketing" },
            new Department { Name = "Human Resources" }
        };
        context.Departments.AddRange(departments);
        await context.SaveChangesAsync();
    }

    if (!context.People.Any())
    {
        var people = new List<Person>
        {
            new Person { Name = "John Smith", Phone = "02 9988 2211", DepartmentId = 1, Street = "1 Code Lane", City = "Javaville", State = "NSW", Zip = "0100", Country = "Australia" },
            new Person { Name = "Sue White", Phone = "03 8899 2255", DepartmentId = 2, Street = "16 Bit way", City = "Byte Cove", State = "QLD", Zip = "1101", Country = "Australia" },
            new Person { Name = "Bob O' Bits", Phone = "05 7788 2255", DepartmentId = 3, Street = "8 Silicon Road", City = "Cloud Hills", State = "VIC", Zip = "1001", Country = "Australia" },
            new Person { Name = "Mary Blue", Phone = "06 4455 9988", DepartmentId = 2, Street = "4 Processor Boulevard", City = "Appletson", State = "NT", Zip = "1010", Country = "Australia" },
            new Person { Name = "Mick Green", Phone = "02 9988 1122", DepartmentId = 3, Street = "700 Bandwidth Street", City = "Bufferland", State = "NSW", Zip = "0110", Country = "Australia" }        };
        context.People.AddRange(people);
        await context.SaveChangesAsync();
    }
}

public class RoiDatabaseContext : DbContext
{
    public RoiDatabaseContext(DbContextOptions<RoiDatabaseContext> options)
        : base(options)
    {
    }

    public DbSet<Person> People { get; set; }
    public DbSet<Department> Departments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Person>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Department>()
            .HasKey(d => d.Id);

        modelBuilder.Entity<Department>()
            .Property(d => d.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(0, 1); // Specifies seed and increment for identity column
                                      // .ValueGeneratedNever();

        modelBuilder.Entity<Person>()
            .HasOne(p => p.Department)
            .WithMany(d => d.People)
            .HasForeignKey(p => p.DepartmentId);
    }
}

public partial class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public int? DepartmentId { get; set; }
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string Zip { get; set; } = null!;
    public string Country { get; set; } = null!;
    public virtual Department? Department { get; set; }
}

public partial class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Person> People { get; set; } = new List<Person>();
}