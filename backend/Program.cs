using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CCL.Backend;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Port=5432;Database=ccl_inventario;Username=postgres;Password=postgres";
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ccl-secret-key-cambiar-en-local-1234567890";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CCL";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CCL";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Local", policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowAnyOrigin();
    });
});

var app = builder.Build();

app.UseCors("Local");
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Productos.Any())
    {
        db.Productos.AddRange(
            new Producto { Nombre = "Cemento", Cantidad = 100 },
            new Producto { Nombre = "Arena", Cantidad = 50 },
            new Producto { Nombre = "Ladrillo", Cantidad = 200 }
        );
        db.SaveChanges();
    }
}

app.MapPost("/auth/login", ([FromBody] LoginRequest request) =>
{
    if (request.Usuario != "admin" || request.Clave != "admin123")
    {
        return Results.Unauthorized();
    }

    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(jwtKey);
    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, request.Usuario)
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddHours(8),
        Issuer = jwtIssuer,
        Audience = jwtAudience,
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var jwt = tokenHandler.WriteToken(token);

    return Results.Ok(new LoginResponse { Token = jwt });
})
.AllowAnonymous();

app.MapPost("/productos/movimiento", async ([FromBody] MovimientoRequest request, AppDbContext db) =>
{
    if (request.Cantidad <= 0)
    {
        return Results.BadRequest("Cantidad debe ser mayor que cero.");
    }

    var tipo = request.Tipo.Trim().ToLower();
    if (tipo != "entrada" && tipo != "salida")
    {
        return Results.BadRequest("Tipo debe ser entrada o salida.");
    }

    var nombre = request.Nombre.Trim();
    if (nombre == string.Empty)
    {
        return Results.BadRequest("Nombre es obligatorio.");
    }

    var producto = await db.Productos.FirstOrDefaultAsync(p => p.Nombre.ToLower() == nombre.ToLower());
    if (producto == null)
    {
        if (tipo == "salida")
        {
            return Results.BadRequest("El producto no existe.");
        }

        producto = new Producto
        {
            Nombre = nombre,
            Cantidad = request.Cantidad
        };
        db.Productos.Add(producto);
    }
    else
    {
        if (tipo == "entrada")
        {
            producto.Cantidad += request.Cantidad;
        }
        else
        {
            if (producto.Cantidad < request.Cantidad)
            {
                return Results.BadRequest("No hay suficiente inventario.");
            }

            producto.Cantidad -= request.Cantidad;
        }
    }

    await db.SaveChangesAsync();

    return Results.Ok(producto);
})
.RequireAuthorization();

app.MapGet("/productos/inventario", async (AppDbContext db) =>
{
    var productos = await db.Productos
        .OrderBy(p => p.Nombre)
        .ToListAsync();

    return Results.Ok(productos);
})
.RequireAuthorization();

app.Run();
