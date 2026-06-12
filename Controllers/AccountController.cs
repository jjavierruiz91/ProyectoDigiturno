using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System;

namespace GobernacionTurnos.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string usuario, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT Id, NombreCompleto, Rol, VentanillaId 
                        FROM Funcionarios 
                        WHERE Usuario = @Usuario AND PasswordHash = @Password AND Activo = 1";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Usuario", usuario);
                        cmd.Parameters.AddWithValue("@Password", password);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                HttpContext.Session.SetString("FuncionarioId", reader.GetInt32(0).ToString());
                                HttpContext.Session.SetString("FuncionarioNombre", reader.GetString(1));
                                HttpContext.Session.SetString("FuncionarioRol", reader.GetString(2));
                                
                                int ventanillaId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                                if (ventanillaId > 0)
                                {
                                    HttpContext.Session.SetInt32("VentanillaId", ventanillaId);
                                }
                                
                                if (reader.GetString(2) == "Supervisor")
                                {
                                    return RedirectToAction("Dashboard", "Turno");
                                }
                                else
                                {
                                    return RedirectToAction("PanelLlamada", "Turno");
                                }
                            }
                        }
                    }
                }
                
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}