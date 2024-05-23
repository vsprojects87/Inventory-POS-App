using InventoryPOS.Data;
using InventoryPOS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionstrings = builder.Configuration.GetConnectionString("cs");
builder.Services.AddDbContext<ApplicationDBContext>(option => option.UseSqlServer(connectionstrings));

builder.Services.AddIdentity<AppUser, IdentityRole>(
    options =>
    {
        options.Password.RequiredUniqueChars = 0;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDBContext>().AddDefaultTokenProviders();

builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("DeleteRolePolicy",
        policy=> policy.RequireClaim("Delete Role").RequireClaim("Create Role"));

    option.AddPolicy("EditRolePolicy",
       policy => policy.RequireClaim("Edit Role"));

    option.AddPolicy("AdminRolePolicy",
		policy => policy.RequireRole("Admin"));
});
// here if user wants to delete the role he need to have both delete role and create role claim
// in second option we are creating the adminrolepolicy which requires role admin
// both the options are not same, first is for claims and other is for roles
// role is also claim to 'type role' where claim is policy


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
