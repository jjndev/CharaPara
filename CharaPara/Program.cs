using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CharaPara.Data;
using CharaPara.Data.Model;
using CharaPara.App;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeLoggedIn", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.AddRazorPages();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/login";
    });

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password = new PasswordOptions()
        {
            RequiredUniqueChars = 1,
            RequireLowercase = true,
            RequireUppercase = true,
            RequireDigit = true,
            RequireNonAlphanumeric = false

        };
    })
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager<SignInManager<AppUser>>();

builder.Services.AddCascadingAuthenticationState();

//------------------------------
//custom services
//------------------------------
builder.Services.AddSingleton<ICloudImageUploadService, AmazonS3ImageUploadService>();
builder.Services.AddSingleton<IUserImagePathHandler, UserImagePathHandler>();
builder.Services.AddSingleton<IValidateImageService, ValidateImageService>();
builder.Services.AddSingleton<IUserInputMessageFormatService, UserInputMessageFormatService_MarkDig>();
builder.Services.AddSingleton<ImageCDNUrlService>();

builder.Services.AddSingleton<IUserImageCloudUploadManager, UserImageCloudUploadManager_AmazonS3>();

builder.Services.AddSingleton<IProfileTabEncoder, ProfileTabEncoder>();
builder.Services.AddTransient<IProfileAuthorizationService, ProfileAuthorizationService>(sp => 
    new ProfileAuthorizationService(sp.GetRequiredService<ApplicationDbContext>(), sp.GetRequiredService<UserManager<AppUser>>())
);

builder.Services.AddSingleton<IUserImageUploadHandler, UserImageUploadHandler>(sp => 
    new UserImageUploadHandler(
        sp.GetRequiredService<ICloudImageUploadService>(), 
        sp.GetRequiredService<IValidateImageService>(), 
        sp.GetRequiredService<IUserImagePathHandler>(),
        sp.GetRequiredService<IServiceProvider>()
    ));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}




app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


app.MapRazorPages();
//app.MapBlazorHub();
//app.MapFallbackToPage("/_Host");


//app.UseResponseCompression();


//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
