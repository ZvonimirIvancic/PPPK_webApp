using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MongoDB.Driver;
using Minio;
using DrugiProjekt.Services;
using DrugiProjekt.Repositories;
using DrugiProjekt.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// MongoDB Configuration
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDBAtlas");
    return new MongoClient(connectionString);
});

builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "tcga_cancer_data";
    return client.GetDatabase(databaseName);
});

// MinIO Configuration
builder.Services.AddSingleton<IMinioClient>(serviceProvider =>
{
    var endpoint = builder.Configuration["MinIO:Endpoint"] ?? "localhost:9000";
    var accessKey = builder.Configuration["MinIO:AccessKey"] ?? "minioadmin";
    var secretKey = builder.Configuration["MinIO:SecretKey"] ?? "minioadmin";
    var useSSL = bool.Parse(builder.Configuration["MinIO:UseSSL"] ?? "false");

    return new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey)
        .WithSSL(useSSL)
        .Build();
});

// Services
builder.Services.AddScoped<ITCGADataService, TCGADataService>();
builder.Services.AddScoped<IGeneExpressionService, GeneExpressionService>();
builder.Services.AddScoped<IPatientDataService, PatientDataService>();
builder.Services.AddScoped<IVisualizationService, VisualizationService>();
builder.Services.AddScoped<IXenaScraperService, XenaScraperService>();
builder.Services.AddScoped<IMinIOStorageService, MinIOStorageService>();

// Repositories
builder.Services.AddScoped<IPatientGeneExpressionRepository, PatientGeneExpressionRepository>();
builder.Services.AddScoped<ICancerCohortRepository, CancerCohortRepository>();

// Background services for data processing
builder.Services.AddHostedService<DataProcessingBackgroundService>();

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

// Initialize MongoDB collections and MinIO buckets
using (var scope = app.Services.CreateScope())
{
    var mongoDb = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    var minioClient = scope.ServiceProvider.GetRequiredService<IMinioClient>();

    await InitializeDataStores(mongoDb, minioClient);
}

app.Run();

static async Task InitializeDataStores(IMongoDatabase mongoDb, IMinioClient minioClient)
{
    // Initialize MongoDB collections
    var collections = new[] { "patient_gene_expressions", "cancer_cohorts", "processing_logs" };

    foreach (var collectionName in collections)
    {
        try
        {
            await mongoDb.CreateCollectionAsync(collectionName);
        }
        catch (MongoCommandException ex) when (ex.Code == 48) // Collection already exists
        {
            // Ignore - collection already exists
        }
    }

    // Initialize MinIO buckets
    var buckets = new[] { "tcga-tsv-files", "processed-data", "logs" };

    foreach (var bucketName in buckets)
    {
        try
        {
            var bucketExists = await minioClient.BucketExistsAsync(
                new Minio.DataModel.Args.BucketExistsArgs().WithBucket(bucketName));

            if (!bucketExists)
            {
                await minioClient.MakeBucketAsync(
                    new Minio.DataModel.Args.MakeBucketArgs().WithBucket(bucketName));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing bucket {bucketName}: {ex.Message}");
        }
    }
}