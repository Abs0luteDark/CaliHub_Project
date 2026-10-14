using CaliHub_DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// משיכת מחרוזת החיבור מתוך ה-User Secrets
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// מוודאים שבאמת קיבלנו מחרוזת תקינה
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in user secrets.");
}

// קריאה לפעולת האתחול של ה-DAL
await CaliHub_DAL.DatabaseInitializer.InitializeAsync(connectionString);

// הוספת שם מסד הנתונים למחרוזת החיבור עבור UserDB
string fullConnectionString = connectionString.EndsWith(";")
    ? $"{connectionString}Database=calihubdb;"
    : $"{connectionString};Database=calihubdb;";

// רישום UserDB במנגנון הזרקת התלויות (Dependency Injection)
builder.Services.AddSingleton(new UserDB(fullConnectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();