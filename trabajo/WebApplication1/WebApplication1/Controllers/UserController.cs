using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        //configurando el entorno para usar la cadena de coneccion , _config es la llave para usar la cadena de conexion
        private IConfiguration _Config;

        public UserController(IConfiguration Config)
        {
            _Config = Config;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            using var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection"));
            conexion.Open();
            var users = conexion.Query<User>("GetAllUsers", commandType: System.Data.CommandType.StoredProcedure);
            return Ok(users);
        }
        [HttpPut]
        public async Task<ActionResult<List<User>>> UpdateUser(User us)
        {
            using var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection"));
            conexion.Open();
            var parametro = new DynamicParameters();
            parametro.Add("@UserID", us.UserID);
            parametro.Add("@UserName", us.UserName);
            parametro.Add("@Email", us.Email);
            parametro.Add("@Password", us.Password);
            var oCliente = conexion.Query<User>("UpdateUser", parametro, commandType: System.Data.CommandType.StoredProcedure);
            return Ok(oCliente);
        }


        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            try
            {
                using var connection = new SqlConnection(_Config.GetConnectionString("DefaultConnection"));
                connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@UserName", user.UserName);
                parameters.Add("@Email", user.Email);
                parameters.Add("@Password", user.Password);

                var result = connection.Query<User>("InsertUser", parameters, commandType: CommandType.StoredProcedure);

                if (result != null)
                {
                    var message = "Usuario creado exitosamente.";
                    return Ok(message);
                }
                else
                {
                    var message = "No se pudo crear el usuario.";
                    return BadRequest(new { message });
                }
            }
            catch (Exception ex)
            {
                var message = "Se produjo un error al crear el usuario: " + ex.Message;
                return StatusCode(500, new { message });
            }
        }

        [HttpDelete("{UserID}")]
        // obteniendo id del libro a eliminar (este id es de la clase Libros)
        public async Task<ActionResult> DeleteUser(int UserID)
        {   
            //manejo de errores con trycach
            try
            {
                using (var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection")))
                {
                    await conexion.OpenAsync();

                    var parametro = new DynamicParameters();
                    parametro.Add("@UserID", UserID);
                    await conexion.ExecuteAsync("DeleteUser", parametro, commandType: CommandType.StoredProcedure);

                    return Ok("usuario eliminada correctamente.");
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error al eliminar el usuario: {ex.Message}");
            }
        }


        [HttpPost("Login")]
        public async Task<ActionResult> LoginUser(LoginRequest loginRequest)
        {
            try
            {
                using (var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection")))
                {
                    await conexion.OpenAsync();

                    var parametro = new DynamicParameters();
                    parametro.Add("@Email", loginRequest.Email);
                    parametro.Add("@Password", loginRequest.Password);

                    // Aquí asumimos que el procedimiento almacenado "LoginUser" devuelve el usuario si las credenciales son válidas
                    var usuario = await conexion.QueryFirstOrDefaultAsync<User>("LoginUser", parametro, commandType: CommandType.StoredProcedure);

                    if (usuario != null)
                    {
                        // Las credenciales son válidas, puedes devolver un token de acceso u otra información según sea necesario
                        var mensaje = "Inicio de sesión exitoso.";
                        return Ok(new { mensaje, Usuario = usuario });
                    }
                    else
                    {
                        // Las credenciales no son válidas
                        var mensaje = "Credenciales incorrectas.";
                        return BadRequest(new { mensaje });
                    }
                }
            }
            catch (Exception ex)
            {
                var mensaje = "Se produjo un error al intentar iniciar sesión: " + ex.Message;
                return StatusCode(500, new { mensaje });
            }
        }

    }
}
