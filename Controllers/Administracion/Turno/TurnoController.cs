
using dw_backend.DBConnections.SqlServer;
using dw_backend.Helpers;
using dw_backend.Helpers.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
//using dw_backend.Models.Rol;
using System;
using System.Data;
using System.Data.SqlClient;

namespace dw_backend.Controllers.Administracion.Turno
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurnoController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly string _commandText;
        public TurnoController(IConfiguration configuration)
        {
            _configuration = configuration;
            //_commandText = _configuration.GetValue<string>("DataBase:StoredProcedures:Empleado");
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
                    using (SqlCommand cmd = new SqlCommand("adm.crudtarea", conn))
                    {
                        cmd.Parameters.AddWithValue("@opcion", 4);
                        dynamic result = DataValidator.ValidateDatabaseData(cmd);
                        switch (result.Value<Int32>("response"))
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
                    using (SqlCommand cmd = new SqlCommand("adm.crudtarea", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", (Int32)request.id);
                        cmd.Parameters.AddWithValue("@opcion", 5);
                        dynamic result = DataValidator.ValidateDatabaseData(cmd);
                        switch (result.Value<Int32>("response"))
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


        [HttpPost]
        [Route("create")]
        [Produces("application/json")]
        //[Authorize]
        public IActionResult Create(dynamic request)
        {
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("adm.crudtarea", conn))
                    {
                        cmd.Parameters.AddWithValue("@datosEmpleado", (Int32)request.datosEmpleado);
                        cmd.Parameters.AddWithValue("@turno", (string)request.turno);
                        cmd.Parameters.AddWithValue("@fechaInicio", (string)request.fechaAsignacion);
                        cmd.Parameters.AddWithValue("@fechaFin", (string)request.fechaFin);
                        cmd.Parameters.AddWithValue("@usuario", (string)request.usuario);
                        cmd.Parameters.AddWithValue("@opcion", 1);
                        dynamic result = DataValidator.ValidateDatabaseData(cmd);
                        switch (result.Value<Int32>("response"))
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
                    using (SqlCommand cmd = new SqlCommand("adm.crudtarea", conn))
                    {
                        cmd.Parameters.AddWithValue("@datosEmpleado", (Int32)request.datosEmpleado);
                        cmd.Parameters.AddWithValue("@turno", (string)request.turno);
                        cmd.Parameters.AddWithValue("@fechaInicio", (string)request.fechaAsignacion);
                        cmd.Parameters.AddWithValue("@fechaFin", (string)request.fechaFin);
                        cmd.Parameters.AddWithValue("@usuario", (string)request.usuario);
                        cmd.Parameters.AddWithValue("@opcion", 2);
                        dynamic result = DataValidator.ValidateDatabaseData(cmd);
                        switch (result.Value<Int32>("response"))
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
                    using (SqlCommand cmd = new SqlCommand("adm.crudtarea", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", (Int32)request.id);
                        cmd.Parameters.AddWithValue("@estado", (Int32)request.estado);
                        cmd.Parameters.AddWithValue("@opcion", 3);
                        dynamic result = DataValidator.ValidateDatabaseData(cmd);
                        switch (result.Value<Int32>("response"))
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
        [Route("label")]
        [Produces("application/json")]
        //[Authorize]
        public IActionResult Label()
        {
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("adm.crudtarea", conn))
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
