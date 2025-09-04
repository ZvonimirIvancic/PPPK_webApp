using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using PrviProjekt.Data;
using PrviProjekt.Repositories;
using PrviProjekt.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Database connection - Supabase PostgreSQL
builder.Services.AddDbContext<MedicinskiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupabaseConnection"),
    npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
        npgsqlOptions.CommandTimeout(30);
    }));

// Repository Factory Pattern
builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();

// Repositories
builder.Services.AddScoped<IPacijentRepository, PacijentRepository>();
builder.Services.AddScoped<IMedicinskaDokumentacijaRepository, MedicinskaDokumentacijaRepository>();
builder.Services.AddScoped<IPreglediRepository, PreglediRepository>();
builder.Services.AddScoped<ISlikeRepository, SlikeRepository>();
builder.Services.AddScoped<IReceptiRepository, ReceptiRepository>();

// Services
builder.Services.AddScoped<IPacijentService, PacijentService>();
builder.Services.AddScoped<IPreglediService, PreglediService>();
builder.Services.AddScoped<IReceptiService, ReceptiService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IExportService, ExportService>();

// Lazy Loading
builder.Services.AddScoped(typeof(Lazy<>), typeof(Lazy<>));

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Database migration on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MedicinskiDbContext>();
    context.Database.EnsureCreated();
}

app.Run();