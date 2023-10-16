using dw_backend.DBConnections.SqlServer;
using dw_backend.Helpers;
using dw_backend.Helpers.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using dw_backend.Functions;
//using dw_backend.Models.Rol;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;


namespace dw_backend.Controllers.GestionHabitaciones.AdjuntoHabitaciones
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdjuntoHabitacionesController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly string _commandText;
        private readonly string _rootFileHabitaciones;
        private string _root;
        private readonly string _replaceFile;
        public AdjuntoHabitacionesController(IConfiguration configuration)
        {
            _configuration = configuration;
            //_commandText = _configuration.GetValue<string>("DataBase:StoredProcedures:Genero");
            _rootFileHabitaciones = _configuration.GetValue<string>("Files:pathImageFotoHabitaciones");
            _root = _configuration.GetValue<string>("Root");
            _replaceFile = _configuration.GetValue<string>("ReplaceFiles:pathImage");
        }

  



        private string GenerateRandomName()
        {
            int value = new Random().Next(100002, 999998);
            string parseValue = value.ToString();

            return parseValue;
        }

        [HttpPost]
        [Route("create")]
        [Produces("application/json")]
        //[Authorize]
        public IActionResult Create(IFormFile file, [FromForm] string usuario, [FromForm] Int32 habitaciones )
        {
            try
            {

                if (!Directory.Exists(_root + _rootFileHabitaciones)) Directory.CreateDirectory(Path.Combine(_root + _rootFileHabitaciones));

                dynamic prettyImage = FileValidator.ValidatePrettyImage(file);

                if (prettyImage.Value<Int32>("response") == 0)
                {
                    return BadRequest(prettyImage.response);
                }
                string fileName = file.FileName;
                string extension = Path.GetExtension(fileName);
                string code = GenerateRandomName();
                string thisTime = DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
                string oldFile = fileName;
                string newFile = CleanText.RemoveSpaces(fileName.Split(extension)[0] + "-" + code + "-" + thisTime + extension);

                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("gh.crudImgHabitacion", conn))
                    {

                        string archivoDB = Path.Combine(_rootFileHabitaciones, newFile);
                        string archivoServ = Path.Combine(_root, archivoDB);


                        cmd.Parameters.AddWithValue("@nombre", newFile);
                        cmd.Parameters.AddWithValue("@ruta", archivoDB);
                        cmd.Parameters.AddWithValue("@extension",(String)prettyImage.extension);
                        cmd.Parameters.AddWithValue("@habitaciones", habitaciones);
                        cmd.Parameters.AddWithValue("@usuario", usuario);
                        cmd.Parameters.AddWithValue("@opcion", 1);
                        dynamic result = CreateFileValidator.ValidateDatabaseData(cmd, conn, file, archivoServ);
                        switch (result.Value<Int32>("response"))
                        {
                            case 0:
                                return BadRequest(result);
                            case 1:
                                return Ok(result);
                            default:
                                return BadRequest(result.response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Payback(1001, ex.ToString()));
            }
        }


        [HttpPost]
        [Route("update")]
        [Produces("application/json")]
        //[Authorize]
        public IActionResult Update(dynamic request)
        {
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("gh.crudImgHabitacion", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", (Int32)request.id);
                        cmd.Parameters.AddWithValue("@nombre", (string)request.nombre);
                        cmd.Parameters.AddWithValue("@usuario", (string)request.usuario);
                        cmd.Parameters.AddWithValue("@opcion", 2);
                        dynamic result = DataValidator.ValidateDatabaseData(cmd);
                        switch (result.Value<Int32>("response"))
                        {
                            case 0:
                                return BadRequest(result);
                            case 1:
                                return Ok(result);
                            default:
                                return BadRequest(result.response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Payback(1001, ex.ToString()));
            }
        }


        [HttpPost]
        [Route("one-archivo")]
        [Produces("application/json")]
        //[Authorize]
        public IActionResult OneArchivo(dynamic request)
        {
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("gh.crudImgHabitacion", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", (Int32)request.id);
                        cmd.Parameters.AddWithValue("@opcion", 7);
                        //dynamic result = DataValidator.ValidateDatabaseData(cmd);
                        dynamic result = ValidatorOneAdjunto.ValidateDatabaseData(cmd, _root, _replaceFile);
                        switch (result.Value<int>("response"))
                        {
                            case 0:
                                return BadRequest(result.response);
                            case 1:
                                return Ok(result);
                            default:
                                return BadRequest(result.response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                 return BadRequest(Responses.Payback(1001, ex.ToString()));
            }
        }



        [HttpGet]
        [Route("all")]
        [Produces("application/json")]
        //[Authorize]
        public IActionResult All()
        {
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("gh.crudImgHabitacion", conn))
                    {
                        cmd.Parameters.AddWithValue("@opcion", 4);
                        dynamic result = DataValidator.ValidateDatabaseData(cmd);
                        switch (result.Value<Int32>("response"))
                        {
                            case 0:
                                return BadRequest(result);
                            case 1:
                                return Ok(result);
                            default:
                                return BadRequest(result.response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Payback(1001, ex.ToString()));
            }
        }


        [HttpPost]
        [Route("one")]
        [Produces("application/json")]
        //[Authorize]
        public IActionResult One(dynamic request)
        {
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("gh.crudImgHabitacion", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", (Int32)request.id);
                        cmd.Parameters.AddWithValue("@opcion", 5);
                        dynamic result = DataValidator.ValidateDatabaseData(cmd);
                        switch (result.Value<Int32>("response"))
                        {
                            case 0:
                                return BadRequest(result);
                            case 1:
                                return Ok(result);
                            default:
                                return BadRequest(result.response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Payback(1001, ex.ToString()));
            }
        }


        [HttpPost]
        [Route("status")]
        [Produces("application/json")]
        //[Authorize]
        public IActionResult Status(dynamic request)
        {
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("gh.crudImgHabitacion", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", (Int32)request.id);
                        cmd.Parameters.AddWithValue("@estado", (Int32)request.estado);
                        cmd.Parameters.AddWithValue("@opcion", 3);
                        dynamic result = DataValidator.ValidateDatabaseData(cmd);
                        switch (result.Value<Int32>("response"))
                        {
                            case 0:
                                return BadRequest(result);
                            case 1:
                                return Ok(result);
                            default:
                                return BadRequest(result.response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Payback(1001, ex.ToString()));
            }
        }

        [HttpGet]
        [Route("label")]
        [Produces("application/json")]
        //[Authorize]
        public IActionResult Label()
        {
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("gh.crudImgHabitacion", conn))
                    {
                        cmd.Parameters.AddWithValue("@opcion", 6);
                        dynamic result = DataValidator.ValidateDatabaseData(cmd);
                        switch (result.Value<Int32>("response"))
                        {
                            case 0:
                                return BadRequest(result);
                            case 1:
                                return Ok(result);
                            default:
                                return BadRequest(result.response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Payback(1001, ex.ToString()));
            }
        }
    }
}