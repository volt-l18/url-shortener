using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Data;
using UrlShortener.Api.Models;
using HashidsNet;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Extract connection configurations from configuration manager
var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");
var redisInstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName");
var hashidsSalt = builder.Configuration.GetValue<string>("Hashids:Salt");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(dbConnectionString));

builder.Services.AddSingleton<IHashids>(_ => new Hashids(hashidsSalt, 8));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = redisInstanceName;
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/shorten", async (ShortenRequest request, AppDbContext db, IHashids hashids, IDistributedCache cache) =>
{
    if (!Uri.TryCreate(request.LongUrl, UriKind.Absolute, out _))
    {
        return Results.BadRequest("Invalid URL format.");
    }

    var entry = new ShortenedUrl
    {
        LongUrl = request.LongUrl,
        UserId = request.UserId ?? "anonymous",
        ExpiresAt = request.ExpirationDays.HasValue 
            ? DateTime.UtcNow.AddDays(request.ExpirationDays.Value) 
            : null
    };

    db.Urls.Add(entry);
    await db.SaveChangesAsync();

    entry.ShortCode = hashids.Encode(entry.Id);
    await db.SaveChangesAsync();

    var cacheOptions = new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
    };
    await cache.SetStringAsync(entry.ShortCode, entry.LongUrl, cacheOptions);

    return Results.Ok(new { ShortUrl = $"http://localhost:5236/{entry.ShortCode}" });
});

app.MapGet("/{code}", async (string code, AppDbContext db, IDistributedCache cache) =>
{
    var cachedUrl = await cache.GetStringAsync(code);
    if (!string.IsNullOrEmpty(cachedUrl))
    {
        return Results.Redirect(cachedUrl);
    }

    var entry = await db.Urls.FirstOrDefaultAsync(u => u.ShortCode == code);
    if (entry == null)
    {
        return Results.NotFound("Short URL not found.");
    }

    if (entry.ExpiresAt.HasValue && entry.ExpiresAt < DateTime.UtcNow)
    {
        return Results.BadRequest("This short URL has expired.");
    }

    await cache.SetStringAsync(code, entry.LongUrl, new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
    });

    return Results.Redirect(entry.LongUrl);
});

app.Run();

public record ShortenRequest(string LongUrl, string? UserId, int? ExpirationDays);
