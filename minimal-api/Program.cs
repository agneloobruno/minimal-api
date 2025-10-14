using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

#region Builder

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new() { Title = "Minimal API", Version = "v1" });

    c.TagActionsBy(api =>
    {
        var tags = api.ActionDescriptor.EndpointMetadata
            .OfType<string>()
            .Where(m => new[] { "Home", "Administradores", "Veiculos" }.Contains(m))
            .ToArray();

        if (tags.Length > 0)
        {
            return tags;
        }

        return new[] { api.ActionDescriptor.DisplayName?.Split('(').FirstOrDefault() ?? "Default" };
    });

    c.DocInclusionPredicate((docName, api) => true);

    c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
});

builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
app.MapPost("/administradores/login", ([FromBody]LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
        return Results.Ok("Login realizado com sucesso!");
    else
        return Results.Unauthorized();
}).WithTags("Administradores"); 
#endregion

#region Veiculos
 app.MapPost("/veiculos", ([FromBody]VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
 {
     var veiculo = new MinimalApi.Dominio.Entidades.Veiculo
     {
         Nome = veiculoDTO.Nome,
         Marca = veiculoDTO.Marca,
         Ano = veiculoDTO.Ano
     };
    veiculoServico.Adicionar(veiculo);

     return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
 }).WithTags("Veiculos");

app.MapGet("/veiculos/", ([FromQuery] int pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
        return Results.NotFound();

    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
        return Results.NotFound();

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);

    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
        return Results.NotFound();

    veiculoServico.Deletar(veiculo);

    return Results.NoContent();
}).WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion

