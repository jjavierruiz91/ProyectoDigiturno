using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GobernacionTurnos.Controllers
{
    public class TurnoController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public TurnoController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        [HttpGet]
        public IActionResult TomarTurno()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TomarTurno(string tipoTramite, bool esPrioritario, string identificacion, string nombre)
        {
            var prefijo = "GDC";
            var fecha = DateTime.Now.ToString("yyMMdd");
            var random = new Random();
            var consecutivo = random.Next(1, 9999);
            var numeroTicket = $"{prefijo}-{fecha}-{consecutivo:D4}";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = @"
                    INSERT INTO Turnos (NumeroTicket, TipoTramiteId, Estado, FechaCreacion, Prioridad, IdentificacionCiudadano, NombreCiudadano)
                    VALUES (@NumeroTicket, @TipoTramiteId, 'Espera', @FechaCreacion, @Prioridad, @Identificacion, @Nombre);
                    SELECT SCOPE_IDENTITY();";
                
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@NumeroTicket", numeroTicket);
                    cmd.Parameters.AddWithValue("@TipoTramiteId", int.Parse(tipoTramite));
                    cmd.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Prioridad", esPrioritario ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Identificacion", string.IsNullOrEmpty(identificacion) ? DBNull.Value : (object)identificacion);
                    cmd.Parameters.AddWithValue("@Nombre", string.IsNullOrEmpty(nombre) ? DBNull.Value : (object)nombre);
                    
                    int nuevoId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return RedirectToAction("TurnoGenerado", new { id = nuevoId, ticket = numeroTicket });
                }
            }
        }

        [HttpGet]
        public IActionResult TurnoGenerado(int id, string ticket)
        {
            ViewBag.NumeroTicket = ticket;
            return View();
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult PanelLlamada()
        {
            var funcionarioId = HttpContext.Session.GetString("FuncionarioId");
            if (string.IsNullOrEmpty(funcionarioId))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTurnosEnEspera()
        {
            var turnosList = new List<object>();
            TurnoViewModel turnoActual = null;
            var todosTurnos = new List<object>();
            
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                
                // Obtener turno actual en atención o llamado
                string sqlActual = @"
                    SELECT TOP 1 Id, NumeroTicket, NombreCiudadano, IdentificacionCiudadano, Estado
                    FROM Turnos 
                    WHERE Estado IN ('Llamado', 'EnAtencion')
                    ORDER BY FechaLlamado DESC";
                
                using (SqlCommand cmd = new SqlCommand(sqlActual, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        turnoActual = new TurnoViewModel
                        {
                            Id = reader.GetInt32(0),
                            NumeroTicket = reader.GetString(1),
                            NombreCiudadano = reader.IsDBNull(2) ? "Anónimo" : reader.GetString(2),
                            Identificacion = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            Estado = reader.GetString(4)
                        };
                    }
                }
                
                // Obtener turnos en espera
                string sqlEspera = @"
                    SELECT Id, NumeroTicket, Prioridad, NombreCiudadano, IdentificacionCiudadano
                    FROM Turnos 
                    WHERE Estado = 'Espera'
                    ORDER BY Prioridad DESC, FechaCreacion ASC";
                
                using (SqlCommand cmd = new SqlCommand(sqlEspera, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        turnosList.Add(new
                        {
                            id = reader.GetInt32(0),
                            numeroTicket = reader.GetString(1),
                            prioridad = reader.GetInt32(2),
                            nombre = reader.IsDBNull(3) ? "Anónimo" : reader.GetString(3),
                            identificacion = reader.IsDBNull(4) ? "" : reader.GetString(4)
                        });
                    }
                }
                
                // Obtener todos los turnos del día
                string sqlTodos = @"
                    SELECT Id, NumeroTicket, NombreCiudadano, IdentificacionCiudadano, Estado, 
                           FORMAT(FechaCreacion, 'HH:mm:ss') as HoraCreacion,
                           FORMAT(FechaInicioAtencion, 'HH:mm:ss') as InicioAtencion,
                           FORMAT(FechaFinAtencion, 'HH:mm:ss') as FinAtencion
                    FROM Turnos 
                    WHERE CAST(FechaCreacion AS DATE) = CAST(GETDATE() AS DATE)
                    ORDER BY Id DESC";
                
                using (SqlCommand cmd = new SqlCommand(sqlTodos, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        todosTurnos.Add(new
                        {
                            id = reader.GetInt32(0),
                            numeroTicket = reader.GetString(1),
                            nombre = reader.IsDBNull(2) ? "Anónimo" : reader.GetString(2),
                            identificacion = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            estado = reader.GetString(4),
                            horaCreacion = reader.GetString(5),
                            inicioAtencion = reader.IsDBNull(6) ? "-" : reader.GetString(6),
                            finAtencion = reader.IsDBNull(7) ? "-" : reader.GetString(7)
                        });
                    }
                }
            }
            
            return Json(new { turnos = turnosList, turnoActual, todosTurnos });
        }

        [HttpPost]
        public async Task<IActionResult> LlamarSiguiente([FromBody] LlamarRequest request)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                
                string sqlGet = @"
                    SELECT TOP 1 Id, NumeroTicket 
                    FROM Turnos 
                    WHERE Estado = 'Espera'
                    ORDER BY Prioridad DESC, FechaCreacion ASC";
                
                int turnoId = 0;
                string numeroTicket = "";
                
                using (SqlCommand cmd = new SqlCommand(sqlGet, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        turnoId = reader.GetInt32(0);
                        numeroTicket = reader.GetString(1);
                    }
                    else
                    {
                        return Json(new { success = false, mensaje = "No hay turnos en espera" });
                    }
                }
                
                string sqlUpdate = @"
                    UPDATE Turnos 
                    SET Estado = 'Llamado', FechaLlamado = @FechaLlamado
                    WHERE Id = @TurnoId";
                
                using (SqlCommand cmd = new SqlCommand(sqlUpdate, conn))
                {
                    cmd.Parameters.AddWithValue("@TurnoId", turnoId);
                    cmd.Parameters.AddWithValue("@FechaLlamado", DateTime.Now);
                    await cmd.ExecuteNonQueryAsync();
                }
                
                return Json(new { success = true, numeroTicket });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LlamarTurnoEspecifico([FromBody] LlamarTurnoRequest request)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                
                string sql = @"
                    UPDATE Turnos 
                    SET Estado = 'Llamado', FechaLlamado = @FechaLlamado
                    WHERE Id = @TurnoId AND Estado = 'Espera'";
                
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TurnoId", request.TurnoId);
                    cmd.Parameters.AddWithValue("@FechaLlamado", DateTime.Now);
                    int rows = await cmd.ExecuteNonQueryAsync();
                    
                    if (rows > 0)
                    {
                        string sqlTicket = "SELECT NumeroTicket FROM Turnos WHERE Id = @Id";
                        using (SqlCommand cmdTicket = new SqlCommand(sqlTicket, conn))
                        {
                            cmdTicket.Parameters.AddWithValue("@Id", request.TurnoId);
                            string ticket = (await cmdTicket.ExecuteScalarAsync())?.ToString() ?? "";
                            return Json(new { success = true, numeroTicket = ticket });
                        }
                    }
                    
                    return Json(new { success = false, mensaje = "El turno ya no está disponible" });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> IniciarAtencion()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                
                string sql = @"
                    UPDATE Turnos 
                    SET Estado = 'EnAtencion', FechaInicioAtencion = @FechaInicio
                    WHERE Estado = 'Llamado'";
                
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", DateTime.Now);
                    await cmd.ExecuteNonQueryAsync();
                }
                
                return Json(new { success = true });
            }
        }

        [HttpPost]
        public async Task<IActionResult> FinalizarAtencion()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                
                string sql = @"
                    UPDATE Turnos 
                    SET Estado = 'Atendido', FechaFinAtencion = @FechaFin
                    WHERE Estado = 'EnAtencion'";
                
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FechaFin", DateTime.Now);
                    await cmd.ExecuteNonQueryAsync();
                }
                
                return Json(new { success = true });
            }
        }
    }

    public class TurnoViewModel
    {
        public int Id { get; set; }
        public string NumeroTicket { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public int Prioridad { get; set; }
        public string NombreCiudadano { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string? TipoTramite { get; set; }
    }

    public class LlamarRequest
    {
        public int VentanillaId { get; set; }
    }

    public class LlamarTurnoRequest
    {
        public int TurnoId { get; set; }
        public int VentanillaId { get; set; }
    }
}