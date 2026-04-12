using DBPCLabs.Components;


var builder = WebApplication.CreateBuilder(args);

// Repositories
builder.Services.AddScoped<DBPCLabs.Repositories.LaboratoryRepository>();
builder.Services.AddScoped<DBPCLabs.Repositories.ComputerRepository>();
builder.Services.AddScoped<DBPCLabs.Repositories.SoftwareRepository>();
builder.Services.AddScoped<DBPCLabs.Repositories.GroupRepository>();
builder.Services.AddScoped<DBPCLabs.Repositories.StudentRepository>();
builder.Services.AddScoped<DBPCLabs.Repositories.TeacherRepository>();
builder.Services.AddScoped<DBPCLabs.Repositories.DepartmentRepository>();
builder.Services.AddScoped<DBPCLabs.Repositories.ReservationRepository>();
builder.Services.AddScoped<DBPCLabs.Repositories.ComputerSoftwareRepository>();
// Localization
builder.Services.AddLocalization();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// languages
var supportedCultures = new[] { "uk-UA", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0]) 
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();