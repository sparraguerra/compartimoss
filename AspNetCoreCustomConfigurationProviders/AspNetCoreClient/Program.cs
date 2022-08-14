using CustomConfigurationProviders.SqlServer;

var builder = WebApplication.CreateBuilder(args);

var  connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//Here we added our configuration provider.
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Configuration.AddSqlServer(sqlBuilder =>
                                                    sqlBuilder
                                                        .UseConnectionString(connectionString)
                                                        .UseCustomQuery("SELECT [Key], [Value] from dbo.Settings") // set query to retrieve configuration
                                                        .WithPrefix("AppSettings") // set configuration key prefix. Ex: AppSettings:Message
                                                        .ConfigureRefresh(TimeSpan.FromSeconds(20))); // set refreshing configuration timespan
}


builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));


// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
