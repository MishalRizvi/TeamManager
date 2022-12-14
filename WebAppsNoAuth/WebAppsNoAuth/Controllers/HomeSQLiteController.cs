using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using WebAppsNoAuth.Models;

namespace WebAppsNoAuth.Controllers;

public class HomeSQLiteController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeSQLiteController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult Login()
    {
        return View();
    }
    public IActionResult Register()
    {
        return View();
    }

    public bool AddUser(string name, string email, string password, string institution)
    {
        Debug.WriteLine("entered home controller");
       Console.WriteLine("entered controller");
        using (SqliteConnection connection = new SqliteConnection("Data Source=WebAppsNoAuth.db"))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO [User] VALUES (@NAME, @EMAIL,@PASSWORD, 1, @INSTITUTION)";
            command.Parameters.AddWithValue("@NAME", name);
            command.Parameters.AddWithValue("@EMAIL", email);
            command.Parameters.AddWithValue("@PASSWORD", password);
            command.Parameters.AddWithValue("@INSTITUTION", institution);
            command.ExecuteNonQuery();

        }
        return true;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

