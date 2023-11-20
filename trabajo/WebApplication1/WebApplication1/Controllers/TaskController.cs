using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using WebApplication1.Models;
using Dapper;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {

        //configurando el entorno para usar la cadena de coneccion , _config es la llave para usar la cadena de conexion
        private IConfiguration _Config;

        public TaskController(IConfiguration Config)
        {
            _Config = Config;
        }

        [HttpGet("{ProjectID}/tasks")]
        public async Task<ActionResult<List<Tasks>>> GetTasksByProjectID(int ProjectID)
        {
            try
            {
                using (var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection")))
                {
                    await conexion.OpenAsync();

                    // Obtener tareas por el ProjectID
                    var tasks = await conexion.QueryAsync<Tasks>("GetTasksByProjectID", new { ProjectID = ProjectID }, commandType: CommandType.StoredProcedure);

                    if (tasks != null && tasks.Any())
                    {
                        return Ok(tasks);
                    }
                    else
                    {
                        return NotFound("No se encontraron tareas para el proyecto con el ProjectID proporcionado.");
                    }
                }
            }
            catch (Exception ex)
            {
                var message = "Se produjo un error al obtener las tareas: " + ex.Message;
                return StatusCode(500, new { message });
            }
        }

        [HttpPost("{ProjectID}")]
        public async Task<ActionResult<CreateTaskcs>> CreateTask(int ProjectID, [FromBody] CreateTaskcs Task)
        {
            try
            {

                using (var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection")))
                {
                    await conexion.OpenAsync();


                    // Establecer el CreatorUserID con el ID del usuario proporcionado en la ruta

                    var parameters = new DynamicParameters();
                    parameters.Add("@Description", Task.Description );
                    parameters.Add("@StartDate", Task.StartDate );
                    parameters.Add("@EndDate",  Task.EndDate);
                    parameters.Add("@Status", Task.Status );  
                    parameters.Add("@ProjectID", ProjectID);

                    // Utilizar Execute en lugar de Query ya que estamos insertando y no esperamos un conjunto de resultados
                    await conexion.ExecuteAsync("InsertTask", parameters, commandType: CommandType.StoredProcedure);

                    // Retornar un mensaje indicando que el proyecto se ha creado exitosamente
                    return Ok("Tarea creada exitosamente.");
                }
            }
            catch (Exception ex)
            {
                var message = "Se produjo un error al crear la tarea: " + ex.Message;
                return StatusCode(500, new { message });
            }
        }

        [HttpDelete("{TaskID}")]
        // obteniendo id del Projecto a eliminar (este id es de la clase Project)
        public async Task<ActionResult> DeleteTask(int TaskID)
        {
            //manejo de errores con trycach
            try
            {
                using (var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection")))
                {
                    await conexion.OpenAsync();

                    var parametro = new DynamicParameters();
                    parametro.Add("@TaskID", TaskID);
                    await conexion.ExecuteAsync("DeleteTask", parametro, commandType: CommandType.StoredProcedure);

                    return Ok("Tarea eliminada correctamente.");
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error al eliminar la Tarea: {ex.Message}");
            }
        }

        [HttpPut("{TaskID}")]
        public async Task<ActionResult<CreateTaskcs>> UpdateProject(int TaskID, [FromBody] CreateTaskcs Task)
        {
            try
            {
                using (var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection")))
                {
                    await conexion.OpenAsync();

                    var parametro = new DynamicParameters();
                    parametro.Add("@Description", Task.Description);
                    parametro.Add("@StartDate", Task.StartDate);
                    parametro.Add("@EndDate", Task.EndDate);
                    parametro.Add("@Status", Task.Status);
                    parametro.Add("@TaskID", TaskID);

                    var affectedRows = await conexion.ExecuteAsync("UpdateTask", parametro, commandType: CommandType.StoredProcedure);

                    if (affectedRows > 0)
                    {
                       

                        return Ok("tarea Actualizada ");
                    }
                    else
                    {
                        return NotFound("Proyecto no encontrado");
                    }
                }
            }
            catch (Exception ex)
            {
                var message = "Se produjo un error al actualizar el proyecto: " + ex.Message;
                return StatusCode(500, new { message });
            }
        }

    }
}
