var builder = WebApplication.CreateBuilder(args);

// Fügt die Services hinzu.
builder.Services.AddControllersWithViews();

// Für den Login Session Cookie.
// Der AccountController speichert dort den Username vom eingeloggten Nutzer.
// Es gibt kein Login System mit Datenbank dahinter.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
});

var app = builder.Build();

// Konfiguriert die HTTP Pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Der Standard HSTS Wert ist 30 Tage. Für richtige Produktion evtl anpassen, siehe https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

// Eine Route reicht für alle Controller. Zum Beispiel geht /Towers automatisch zu TowersController.Index().
// Es braucht keine extra Route pro Controller.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
