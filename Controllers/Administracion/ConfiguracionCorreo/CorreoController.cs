using dw_backend.Helpers;
using dw_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using dw_backend.Helpers.Responses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;


namespace dw_backend.Controllers.Administracion.ConfiguracionCorreo
{
    [Route("api/[controller]")]
    [ApiController]
    public class CorreoController : Controller
    {
        private IConfiguration Configuration;
        private readonly string _connectionString;

        public CorreoController(IConfiguration _configuration)
        {
            Configuration = _configuration;
            _connectionString = _configuration.GetConnectionString("MainConnection");
        }

        [HttpGet]
        [Route("all")]
        //[Authorize]
        public IActionResult All()
        {
            Responses result;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                                     
                    return BadRequest(Responses.Payback(1001, ex.ToString()));
                }

                using (SqlCommand cmd = new SqlCommand("crud_configuracion_correo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@opcion", 4);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataSet setter = new DataSet();

                    try
                    {
                        adapter.Fill(setter, "tabla");
                        if (setter.Tables["tabla"] == null)
                        {
                            
                            return BadRequest(Responses.Payback(7001, null));
                        }
                    }
                    catch (Exception ex)
                    {
                       
                        return BadRequest(Responses.Payback(1002, ex.ToString()));
                    }

                    if (setter.Tables["tabla"].Rows.Count <= 0)
                    {
                      
                        return BadRequest(Responses.Payback(2009, null));
                    }

                    return Ok(setter.Tables["tabla"]);
                }
            }
        }

        [HttpGet]
        [Produces("application/json")]
        [Route("one")]
        //[Authorize]
        public IActionResult GetOne()
        {
            Responses payback;
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                    }
                    catch (Exception ex)
                    {
                    
                        return BadRequest(Responses.Payback(7001, ex.ToString()));
                    }
                    using (SqlCommand cmd = new SqlCommand("crud_configuracion_correo", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@opcion", 5);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataSet setter = new DataSet();
                        try
                        {
                            adapter.Fill(setter, "correo");
                            if (setter.Tables["correo"] == null)
                            {
                             
                                return BadRequest(Responses.Payback(4, null));
                            }
                        }
                        catch (Exception ex)
                        {
                           
                            return BadRequest(Responses.Payback(7001, ex.ToString()));
                        }
                        if (setter.Tables["correo"].Rows.Count <= 0)
                        {
                           
                            return BadRequest(Responses.Payback(7001, null));
                        }
                        return Ok(setter.Tables["correo"]);
                    }
                }
            }
            catch (Exception ex)
            {
               
                return BadRequest(Responses.Payback(7001, ex.ToString()));
            }
        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Route("store")]
        //[Authorize]
        public IActionResult Store(JObject request)
        {
            Responses payback;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
               
                    return BadRequest(Responses.Payback(1001, ex.ToString()));
                }

                using (SqlCommand cmd = new SqlCommand("crud_configuracion_correo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@opcion", 1);
                    cmd.Parameters.AddWithValue("@smtpServer", request.GetValue("smtp").ToString());
                    cmd.Parameters.AddWithValue("@smtpPort", request.GetValue("port").ToString());
                    cmd.Parameters.AddWithValue("@emailServerAccount", request.GetValue("email").ToString());
                    cmd.Parameters.AddWithValue("@passwordEmailAccount", request.GetValue("password").ToString());
                    cmd.Parameters.AddWithValue("@usuario", request.GetValue("usuario").ToString());

                    int result = 0;

                    try
                    {
                        result = cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        
                        return BadRequest(Responses.Payback(1003, ex.ToString()));
                    }

                    if (result <= 0)
                    {
                       
                        return BadRequest(Responses.Payback(1003, null));
                    }

                    dynamic data = new JObject();
                    data.message = "Correo SMTP registrado exitosamente.";
                    data.response = 1;
                    data.value = 1;

                    return Ok(data);
                }
            }
        }
    }
}
