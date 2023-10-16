using dw_backend.Helpers;
using dw_backend.DBConnections.SqlServer;
using dw_backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using dw_backend.Helpers.Responses;
using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using MailKit.Net.Smtp;
using MimeKit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;




namespace dw_backend.Controllers.Administracion.Usuario
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _nameProcedure;
        private readonly string _updatePasswordTemplate;
        private readonly string _resetPasswordTemplate;
        private readonly string _newUserTemplate;

        public UsuarioController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("MainConnection");
            _updatePasswordTemplate = _configuration.GetValue<string>("MailTemplates:updatePassword");
            _resetPasswordTemplate = _configuration.GetValue<string>("MailTemplates:resetPassword");
            _newUserTemplate = _configuration.GetValue<string>("MailTemplates:newUser");
            _nameProcedure = "adm.crudUsuario";
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
                    using (SqlCommand cmd = new SqlCommand("adm.crudUsuario", conn))
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
        [Route("asignar-rol")]
        [Produces("application/json")]
        //[Authorize]
        public IActionResult StoreUpdateRoles(UserModel request)
        {
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("adm.crudUsuario", conn))
                    {
                        cmd.Parameters.AddWithValue("@opcion", 1);
                        //cmd.Parameters.AddWithValue("@id", request.id);
                        cmd.Parameters.AddWithValue("@persona", request.persona);
                        //cmd.Parameters.AddWithValue("@username", request.username);
                        cmd.Parameters.AddWithValue("@rol", request.rol);
                        //cmd.Parameters.AddWithValue("@estado", request.estado);
                        cmd.Parameters.AddWithValue("@usuario", request.usuario);
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
        [Route("get-roles")]
        [Produces("application/json")]
        //[Authorize]
        public IActionResult One(dynamic request)
        {
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("adm.crudUsuario", conn))
                    {
                        cmd.Parameters.AddWithValue("@persona", (Int32)request.id);
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


        //opcion 16 --opcion 4 --opcion 17
        [HttpPost]
        [Produces("application/json")]
        [Route("resetpass")]
        public IActionResult ResetPass(JObject request)
        {
            dynamic data;

            //string password0 = request.GetValue("password0").ToString();
            string password = request.GetValue("password").ToString();
            string codigo = request.GetValue("codigo").ToString();
            string usuario = request.GetValue("usuario").ToString();
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(codigo) || string.IsNullOrEmpty(usuario))
            {

                return BadRequest(0);
            }
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("adm.crudUsuario", conn))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@opcion", 16);
                        cmd.Parameters.AddWithValue("@codigoPass", codigo);
                        cmd.Parameters.AddWithValue("@username", usuario);

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataSet setter = new DataSet();

                        try
                        {

                            adapter.Fill(setter, "usuario");
                            if (setter.Tables["usuario"] == null)
                            {
                                data = new JObject();
                                data.value = 6;
                                data.message = "No existen datos relacionados con la busqueda.";
                                data.response = 6;

                                return BadRequest(data);
                            }
                        }

                        catch (Exception ex)
                        {
                            data = new JObject();
                            data.value = ex.ToString();
                            data.message = "No se ha podido realizar la buscqueda de datos.";
                            data.response = 7;

                            return BadRequest(data);
                        }

                        if (setter.Tables["usuario"].Rows.Count <= 0)
                        {
                            return BadRequest(0);
                        }

                        //get user data
                        string email = setter.Tables["usuario"].Rows[0]["email"].ToString();
                        int estadoUsuario = Int32.Parse(setter.Tables["usuario"].Rows[0]["estadoUsuario"].ToString());
                        int idPersona = Int32.Parse(setter.Tables["usuario"].Rows[0]["idPersona"].ToString());
                        string passHash = setter.Tables["usuario"].Rows[0]["password"].ToString();


                        cmd.Parameters.Clear();
                        adapter.Dispose();
                        setter.Dispose();
                        setter.Tables.Clear();

                        //transaction to send notification about password reset

                        cmd.CommandText = "adm.crudConfiguracionCorreo";
                        cmd.Parameters.AddWithValue("@opcion", 4);

                        SqlTransaction transaction = conn.BeginTransaction("resetPasswordTransaction");
                        cmd.Transaction = transaction;

                        try
                        {
                            adapter.SelectCommand = cmd;
                            adapter.Fill(setter, "emailServer");

                            if (setter.Tables["emailServer"] == null)
                            {
                                transaction.Rollback();
                                return BadRequest(0);
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return BadRequest(0);
                        }

                        if (setter.Tables["emailServer"].Rows.Count <= 0)
                        {
                            transaction.Rollback();
                            return BadRequest(0);
                        }

                        //get email server data
                        int smtpPort = Int32.Parse(setter.Tables["emailServer"].Rows[0]["smtpPort"].ToString());
                        string smtpServer = setter.Tables["emailServer"].Rows[0]["smtpServer"].ToString();
                        string passEmailAccount = setter.Tables["emailServer"].Rows[0]["passwordEmailAccount"].ToString();
                        string emailServerAccount = setter.Tables["emailServer"].Rows[0]["emailServerAccount"].ToString();

                        var mailMessage = new MimeMessage();
                        mailMessage.From.Add(new MailboxAddress("no-reply", emailServerAccount));
                        mailMessage.To.Add(new MailboxAddress(email, email));
                        mailMessage.Subject = "Su contraseña ha sido actualizada";
                        var bodyBuilder = new BodyBuilder();
                        string messageBody;

                        using (StreamReader reader = System.IO.File.OpenText(_updatePasswordTemplate))
                        {
                            messageBody = reader.ReadToEnd();
                            bodyBuilder.HtmlBody = string.Format(Regex.Replace(messageBody, @"[\r\n\t ]+", " "),
                                                    email,
                                                    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                            reader.Close();
                            reader.Dispose();
                        }

                        using (var smtpClient = new SmtpClient())
                        {

                            try
                            {
                                smtpClient.Connect(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                return BadRequest(0);
                            }

                            try
                            {
                                smtpClient.Authenticate(emailServerAccount, passEmailAccount);
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                return BadRequest(0);
                            }

                            cmd.Parameters.Clear();
                            setter.Tables.Clear();
                            setter.Dispose();
                            adapter.Dispose();

                            //Hash new Password
                            string hashNewPassword = BCrypt.Net.BCrypt.HashPassword(password);

                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "adm.crudUsuario";
                            cmd.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(password));
                            cmd.Parameters.AddWithValue("@codigoPass", codigo);
                            cmd.Parameters.AddWithValue("@opcion", 17);

                            try
                            {
                                int resultado = cmd.ExecuteNonQuery();
                                if (resultado >= 1)
                                {
                                    try
                                    {
                                        //send message and commit
                                        mailMessage.Body = bodyBuilder.ToMessageBody();
                                        smtpClient.Send(mailMessage);
                                        smtpClient.Disconnect(true);
                                        smtpClient.Dispose();

                                        data = new JObject();
                                        data.message = "Su contraseña se a actualizado exitosamente";
                                        data.value = 1;
                                        data.response = 1;
                                        transaction.Commit();
                                        return Ok(data);
                                    }
                                    catch (Exception ex)
                                    {
                                        transaction.Rollback();
                                        return BadRequest(0);
                                    }
                                }
                                else
                                {
                                    data = new JObject();
                                    data.message = "Ha ocurrido un problema con la solicitud para restablecer la contraseña";
                                    data.value = 1;
                                    data.response = 1;
                                    transaction.Rollback();
                                    return Ok(data);
                                }
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                return BadRequest(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Payback(1001, ex.ToString()));
            }
        }

        //opcion 8 -- opcion 4
        [HttpPost]
        [Produces("application/json")]
        [Route("credential-notify")]
        public IActionResult CredentialNotify(JObject request)
        {
            dynamic data;
            string id = request.GetValue("id").ToString();

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(0);
            }

            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("adm.crudUsuario", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id", Int32.Parse(request.GetValue("id").ToString()));
                        cmd.Parameters.AddWithValue("@opcion", 8);

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataSet setter = new DataSet();

                        try
                        {

                            adapter.Fill(setter, "usuario");
                            if (setter.Tables["usuario"] == null)
                            {
                                data = new JObject();
                                data.value = 6;
                                data.message = "No existen datos relacionados con la busqueda.";
                                data.response = 6;

                                return BadRequest(data);
                            }
                        }
                        catch (Exception ex)
                        {
                            data = new JObject();
                            data.value = ex.ToString();
                            data.message = "No se ha podido realizar la buscqueda de datos.";
                            data.response = 7;

                            return BadRequest(data);
                        }

                        if (setter.Tables["usuario"].Rows.Count <= 0)
                        {
                            return BadRequest(0);
                        }

                        //get user data
                        string email = setter.Tables["usuario"].Rows[0]["email"].ToString();
                        int estadoUsuario = Int32.Parse(setter.Tables["usuario"].Rows[0]["estadoUsuario"].ToString());
                        int idPersona = Int32.Parse(setter.Tables["usuario"].Rows[0]["idPersona"].ToString());
                        string userName = setter.Tables["usuario"].Rows[0]["username"].ToString();
                        string passHash = setter.Tables["usuario"].Rows[0]["password"].ToString();

                        if (BCrypt.Net.BCrypt.Verify("secreto123+", passHash))
                        {

                            cmd.Parameters.Clear();
                            adapter.Dispose();
                            setter.Dispose();
                            setter.Tables.Clear();

                            //transaction to send notification about password reset

                            cmd.CommandText = "adm.crudConfiguracionCorreo";
                            cmd.Parameters.AddWithValue("@opcion", 4);

                            SqlTransaction transaction = conn.BeginTransaction("resetPasswordTransaction");
                            cmd.Transaction = transaction;

                            try
                            {
                                adapter.SelectCommand = cmd;
                                adapter.Fill(setter, "emailServer");

                                if (setter.Tables["emailServer"] == null)
                                {
                                    transaction.Rollback();
                                    return BadRequest(0);
                                }
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                return BadRequest(0);
                            }

                            if (setter.Tables["emailServer"].Rows.Count <= 0)
                            {
                                transaction.Rollback();
                                return BadRequest(0);
                            }

                            //get email server data
                            int smtpPort = Int32.Parse(setter.Tables["emailServer"].Rows[0]["smtpPort"].ToString());
                            string smtpServer = setter.Tables["emailServer"].Rows[0]["smtpServer"].ToString();
                            string passEmailAccount = setter.Tables["emailServer"].Rows[0]["passwordEmailAccount"].ToString();
                            string emailServerAccount = setter.Tables["emailServer"].Rows[0]["emailServerAccount"].ToString();

                            var mailMessage = new MimeMessage();
                            mailMessage.From.Add(new MailboxAddress("no-reply", emailServerAccount));
                            mailMessage.To.Add(new MailboxAddress(email, email));
                            mailMessage.Subject = "Su usuario ha sido creado";
                            var bodyBuilder = new BodyBuilder();
                            string messageBody;

                            using (StreamReader reader = System.IO.File.OpenText(_newUserTemplate))
                            {
                                messageBody = reader.ReadToEnd();
                                bodyBuilder.HtmlBody = string.Format(Regex.Replace(messageBody, @"[\r\n\t ]+", " "),
                                                        email,
                                                        userName,
                                                        "secreto123+",
                                                        DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                                                        );
                                reader.Close();
                                reader.Dispose();
                            }

                            using (var smtpClient = new SmtpClient())
                            {

                                try
                                {
                                    smtpClient.Connect(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    return BadRequest(0);
                                }

                                try
                                {
                                    smtpClient.Authenticate(emailServerAccount, passEmailAccount);
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    return BadRequest(0);
                                }

                                cmd.Parameters.Clear();
                                setter.Tables.Clear();
                                setter.Dispose();
                                adapter.Dispose();

                                try
                                {


                                    //send message and commit
                                    mailMessage.Body = bodyBuilder.ToMessageBody();
                                    smtpClient.Send(mailMessage);
                                    smtpClient.Disconnect(true);
                                    smtpClient.Dispose();

                                    data = new JObject();
                                    data.message = "Se han enviado las credenciales exitosamente";
                                    data.value = 1;
                                    data.response = 1;
                                    transaction.Commit();
                                    return Ok(data);


                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    return BadRequest(0);
                                }
                            }

                        }
                        else
                        {
                            data = new JObject();
                            data.message = "Las credenciales ya han sido enviadas";
                            data.value = 2;
                            data.response = 2;
                            return Ok(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Payback(1001, ex.ToString()));
            }

        }



        //opcion 18

        [HttpPost]
        [Route("tknreset")]
        //[Authorize]
        public IActionResult TtknReset(JObject request)
        {
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("adm.crudUsuario", conn))
                    {
                        cmd.Parameters.AddWithValue("@opcion", 18);
                        cmd.Parameters.AddWithValue("@codigoPass", request.GetValue("codigo").ToString());
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
        //opcion 10 
        [HttpPost]
        [Produces("application/json")]
        [Route("reset-password")]
        public IActionResult ResetPasword(JObject request)
        {
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("adm.crudUsuario", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", request.GetValue("id").ToString());
                        cmd.Parameters.AddWithValue("@opcion", 10);
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
        private string GenerateRandomPinEmail()
        {
            int randomPin = new Random().Next(100002, 999998);
            string pinToken = randomPin.ToString();

            return pinToken;
        }



        //opcion 8 opcion 4 opcion 7
        [HttpPost]
        [Produces("application/json")]
        [Route("reset/default/password")]
        public IActionResult DefaultPass(JObject request)
        {

            dynamic data;
            string parameter, parameterValue;

            //if receive parameter email

            if (request.ContainsKey("email"))
            {
                if (string.IsNullOrEmpty(request.GetValue("email").ToString()))
                {

                    return BadRequest(0);
                }
                parameterValue = request.GetValue("email").ToString();
                parameter = "@email";
            }
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("adm.crudUsuario", conn))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@email", request.GetValue("email").ToString());
                        cmd.Parameters.AddWithValue("@opcion", 8);

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataSet setter = new DataSet();

                        try
                        {

                            adapter.Fill(setter, "usuario");
                            if (setter.Tables["usuario"] == null)
                            {
                                data = new JObject();
                                data.value = 6;
                                data.message = "No existen datos relacionados con la busqueda.";
                                data.response = 6;

                                return BadRequest(data);
                            }
                        }
                        catch (Exception ex)
                        {
                            data = new JObject();
                            data.value = ex.ToString();
                            data.message = "No se ha podido realizar la buscqueda de datos.";
                            data.response = 7;

                            return BadRequest(data);
                        }

                        if (setter.Tables["usuario"].Rows.Count <= 0)
                        {

                            return BadRequest(0);
                        }

                        //get user data
                        string email = setter.Tables["usuario"].Rows[0]["email"].ToString();
                        int estadoPersona = Int32.Parse(setter.Tables["usuario"].Rows[0]["estadoPersona"].ToString());
                        int estadoUsuario = Int32.Parse(setter.Tables["usuario"].Rows[0]["estadoUsuario"].ToString());
                        int idUsuario = Int32.Parse(setter.Tables["usuario"].Rows[0]["idUsuario"].ToString());
                        int idPersona = Int32.Parse(setter.Tables["usuario"].Rows[0]["idPersona"].ToString());

                        if (estadoUsuario == 2) //Already reset password
                        {
                            data = new JObject();
                            data.message = "Ya se le ha enviado un PIN para restablecer su contraseña, revise su buzón de correo";
                            data.response = 4;
                            data.value = 4;
                            return Ok(data);
                        }
                        else if (estadoUsuario != 1) //Not found email
                        {
                            data = new JObject();
                            data.message = "No se ha encontrado la direccion de correo ingresada";
                            data.response = 5;
                            data.value = 5;
                            return Ok(data);
                        }

                        cmd.Parameters.Clear();
                        adapter.Dispose();
                        setter.Dispose();
                        setter.Tables.Clear();

                        //transaction to send notification about password reset

                        cmd.CommandText = "adm.crudConfiguracionCorreo";
                        cmd.Parameters.AddWithValue("@opcion", 4);

                        SqlTransaction transaction = conn.BeginTransaction("sendPinTransaction");
                        cmd.Transaction = transaction;

                        try
                        {
                            adapter.SelectCommand = cmd;
                            adapter.Fill(setter, "emailServer");

                            if (setter.Tables["emailServer"] == null)
                            {
                                transaction.Rollback();

                                return BadRequest(0);
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();

                            return BadRequest(0);
                        }

                        if (setter.Tables["emailServer"].Rows.Count <= 0)
                        {
                            transaction.Rollback();

                            return BadRequest(0);
                        }

                        string pinEmail = GenerateRandomPinEmail();
                        //string hashPinEmail = BCrypt.Net.BCrypt.HashPassword(pinEmail);

                        //get email server data
                        int smtpPort = Int32.Parse(setter.Tables["emailServer"].Rows[0]["smtpPort"].ToString());
                        string smtpServer = setter.Tables["emailServer"].Rows[0]["smtpServer"].ToString();
                        string passEmailAccount = setter.Tables["emailServer"].Rows[0]["passwordEmailAccount"].ToString();
                        string emailServerAccount = setter.Tables["emailServer"].Rows[0]["emailServerAccount"].ToString();

                        var mailMessage = new MimeMessage();
                        mailMessage.From.Add(new MailboxAddress("no-reply", emailServerAccount));
                        mailMessage.To.Add(new MailboxAddress(email, email));
                        mailMessage.Subject = "Notificación de reseteo de contraseña";
                        var bodyBuilder = new BodyBuilder();
                        string messageBody;

                        using (StreamReader reader = System.IO.File.OpenText(_resetPasswordTemplate))
                        {
                            messageBody = reader.ReadToEnd();
                            bodyBuilder.HtmlBody = string.Format(Regex.Replace(messageBody, @"[\r\n\t ]+", " "),
                                                    email,
                                                    pinEmail,
                                                    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                            reader.Close();
                            reader.Dispose();
                        }

                        using (var smtpClient = new SmtpClient())
                        {

                            try
                            {
                                smtpClient.Connect(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();

                                return BadRequest(0);
                            }

                            try
                            {
                                smtpClient.Authenticate(emailServerAccount, passEmailAccount);
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();

                                return BadRequest(0);
                            }

                            cmd.Parameters.Clear();
                            setter.Tables.Clear();
                            setter.Dispose();
                            adapter.Dispose();


                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "adm.crudUsuario";
                            cmd.Parameters.AddWithValue("@id", idUsuario);
                            cmd.Parameters.AddWithValue("@password", pinEmail);
                            cmd.Parameters.AddWithValue("@usuario", idPersona);
                            cmd.Parameters.AddWithValue("@opcion", 7);

                            try
                            {
                                int result = cmd.ExecuteNonQuery();
                                if (result >= 1)
                                {
                                    try
                                    {
                                        //send message and commit
                                        mailMessage.Body = bodyBuilder.ToMessageBody();
                                        smtpClient.Send(mailMessage);
                                        smtpClient.Disconnect(true);
                                        smtpClient.Dispose();

                                        data = new JObject();
                                        data.message = "Se le ha enviado un PIN a su correo para poder restablecer su contraseña";
                                        data.value = 1;
                                        data.response = 1;
                                        transaction.Commit();
                                        return Ok(data);
                                    }
                                    catch (Exception ex)
                                    {
                                        transaction.Rollback();

                                        return BadRequest(0);
                                    }
                                }
                                else
                                {
                                    data = new JObject();
                                    data.message = "Ha ocurrido un problema con la solicitud para restablecer la contraseña";
                                    data.value = 1;
                                    data.response = 1;
                                    transaction.Rollback();
                                    return Ok(data);
                                }
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();

                                return BadRequest(0);
                            }
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
        [Produces("application/json")]
        [Route("cambiarpass")]
        public IActionResult CambiarPass(dynamic request)
        {
            try
            {
                using (SqlConnection conn = MainConnection.Connection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("adm.crudUsuario", conn))
                    {

                        cmd.Parameters.AddWithValue("@id", (int)request.id);
                        cmd.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword((string)request.password));
                        cmd.Parameters.AddWithValue("@usuario", (string)request.usuario);
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


        }
 }
