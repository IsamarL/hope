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
    public class CommentController : ControllerBase
    {
        //configurando el entorno para usar la cadena de coneccion , _config es la llave para usar la cadena de conexion
        private IConfiguration _Config;

        public CommentController(IConfiguration Config)
        {
            _Config = Config;
        }

        [HttpGet("task/{TaskID}/comments")]
        public async Task<ActionResult<List<Comment>>> GetCommentsByTaskID(int TaskID)
        {
            try
            {
                using (var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection")))
                {
                    await conexion.OpenAsync();

                    // Obtener comentarios por el TaskID
                    var comments = await conexion.QueryAsync<Comment>("GetCommentsByTaskID", new { TaskID = TaskID }, commandType: CommandType.StoredProcedure);

                    if (comments != null && comments.Any())
                    {
                        return Ok(comments);
                    }
                    else
                    {
                        return NotFound("No se encontraron comentarios para la tarea con el TaskID proporcionado.");
                    }
                }
            }
            catch (Exception ex)
            {
                var message = "Se produjo un error al obtener los comentarios: " + ex.Message;
                return StatusCode(500, new { message });
            }
        }

        [HttpPost("{TaskID}")]
        public async Task<ActionResult<CreateComment>> CreateTask(int TaskID, [FromBody] CreateComment comment)
        {
            try
            {

                using (var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection")))
                {
                    await conexion.OpenAsync();


                    // Establecer el CreatorUserID con el ID del usuario proporcionado en la ruta

                    var parameters = new DynamicParameters();
                    parameters.Add("@CommentText", comment.CommentText);
                    parameters.Add("@TaskID", TaskID);

                    // Utilizar Execute en lugar de Query ya que estamos insertando y no esperamos un conjunto de resultados
                    await conexion.ExecuteAsync("InsertComment", parameters, commandType: CommandType.StoredProcedure);

                    // Retornar un mensaje indicando que el proyecto se ha creado exitosamente
                    return Ok("comentario creada exitosamente.");
                }
            }
            catch (Exception ex)
            {
                var message = "Se produjo un error al crear el comentario: " + ex.Message;
                return StatusCode(500, new { message });
            }
        }

        [HttpDelete("{CommentID}")]
        // obteniendo id del Projecto a eliminar (este id es de la clase Project)
        public async Task<ActionResult> DeleteComment(int CommentID)
        {
            //manejo de errores con trycach
            try
            {
                using (var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection")))
                {
                    await conexion.OpenAsync();

                    var parametro = new DynamicParameters();
                    parametro.Add("@CommentID", CommentID);
                    await conexion.ExecuteAsync("DeleteComment", parametro, commandType: CommandType.StoredProcedure);

                    return Ok("Comentario eliminado correctamente.");
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error al eliminar el comentario: {ex.Message}");
            }
        }

    }
}
