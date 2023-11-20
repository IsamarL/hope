using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        //configurando el entorno para usar la cadena de coneccion , _config es la llave para usar la cadena de conexion
        private IConfiguration _Config;

        public ProjectController(IConfiguration Config)
        {
            _Config = Config;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Project>>> GetProjectsByUserId(int userId)
        {
            try
            {
                using (var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection")))
                {
                    await conexion.OpenAsync();

                    // Obtener proyectos por el UserId del creador
                    var projects = await conexion.QueryAsync<Project>("GetProjectsByUserId", new { UserId = userId }, commandType: CommandType.StoredProcedure);

                    if (projects != null && projects.Any())
                    {
                        return Ok(projects);
                    }
                    else
                    {
                        return NotFound("No se encontraron proyectos para el usuario con el UserId proporcionado.");
                    }
                }
            }
            catch (Exception ex)
            {
                var message = "Se produjo un error al obtener los proyectos: " + ex.Message;
                return StatusCode(500, new { message });
            }
        }

        [HttpPost("{userId}")]
        public async Task<ActionResult<CreateProject>> CreateProject(int userId, [FromBody] CreateProject project)
        {
            try
            {
               
                using (var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection")))
                {
                    await conexion.OpenAsync();


                    // Establecer el CreatorUserID con el ID del usuario proporcionado en la ruta

                    var parameters = new DynamicParameters();
                    parameters.Add("@ProjectName", project.ProjectName);
                    parameters.Add("@StartDate", project.StartDate);
                    parameters.Add("@EndDate", project.EndDate);
                    parameters.Add("@Status", project.Status);
                    parameters.Add("@CreatorUserID", userId);
                   
                    // Utilizar Execute en lugar de Query ya que estamos insertando y no esperamos un conjunto de resultados
                    await conexion.ExecuteAsync("InsertProject", parameters, commandType: CommandType.StoredProcedure);

                    // Retornar un mensaje indicando que el proyecto se ha creado exitosamente
                    return Ok("Proyecto creado exitosamente.");
                }
            }
            catch (Exception ex)
            {
                var message = "Se produjo un error al crear el proyecto: " + ex.Message;
                return StatusCode(500, new { message });
            }
        }

        [HttpDelete("{ProjectID}")]
        // obteniendo id del Projecto a eliminar (este id es de la clase Project)
        public async Task<ActionResult> DeleteProject(int ProjectID)
        {
            //manejo de errores con trycach
            try
            {
                using (var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection")))
                {
                    await conexion.OpenAsync();

                    var parametro = new DynamicParameters();
                    parametro.Add("@ProjectID", ProjectID);
                    await conexion.ExecuteAsync("DeleteProject", parametro, commandType: CommandType.StoredProcedure);

                    return Ok("Projecto eliminada correctamente.");
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error al eliminar el Projecto: {ex.Message}");
            }
        }

            [HttpPut("{ProjectID}")]
            public async Task<ActionResult<CreateProject>> UpdateProject(int ProjectID, [FromBody] CreateProject updatedProject)
            {
                try
                {
                    using (var conexion = new SqlConnection(_Config.GetConnectionString("DefaultConnection")))
                    {
                        await conexion.OpenAsync();

                        var parametro = new DynamicParameters();
                        parametro.Add("@ProjectID", ProjectID);
                        parametro.Add("@ProjectName", updatedProject.ProjectName);
                        parametro.Add("@StartDate", updatedProject.StartDate);
                        parametro.Add("@EndDate", updatedProject.EndDate);
                        parametro.Add("@Status", updatedProject.Status);

                        var affectedRows = await conexion.ExecuteAsync("UpdateProject", parametro, commandType: CommandType.StoredProcedure);

                        if (affectedRows > 0)
                        {
                            // Obtener el proyecto actualizado desde la base de datos
                            var updatedProjectFromDb = await conexion.QueryFirstOrDefaultAsync<Project>("GetProjectById", new { ProjectID }, commandType: CommandType.StoredProcedure);

                            return Ok(updatedProjectFromDb);
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
    

