using Ridged.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Register Infrastructure Layer (DbContext, Identity, Repositories)
builder.Services.AddInfrastructureLayer(builder.Configuration);

// Register MediatR for Vertical Slices
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Ridged.Application.AssemblyReference).Assembly));

// Register Authentication
builder.Services.AddAuthentication()
    .AddJwtBearer();

builder.Services.AddAuthorization();    
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.Run();