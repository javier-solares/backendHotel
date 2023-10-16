using dw_backend.Helpers.Responses;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using static iText.Svg.SvgConstants;

namespace dw_backend.Helpers
{
    public class CreateFileValidator
    {
        public static JObject ValidateDatabaseData(SqlCommand cmd, SqlConnection conn, IFormFile file, string fotoServ)
        {

            cmd.CommandType = CommandType.StoredProcedure;
            SqlTransaction transaction;
            transaction = conn.BeginTransaction("imagenTransaction");
            cmd.Transaction = transaction;

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet setter = new DataSet();
            JObject data = new JObject();

            try
            {
                adapter.Fill(setter, "tabla");
                if (setter.Tables["tabla"] == null)
                {
                    data.Add("response", 1001);
                    data.Add("data", null);
                    data.Add("value", 0);
                    data.Add("message", "Proceso realizado con éxito.");
                    return data;
                }
            }
            catch (Exception ex)
            {
                data.Add("response", 1001);
                data.Add("data", null);
                data.Add("value", 0);
                data.Add("message", "Proceso realizado con éxito.");
                return data;
            }

            if (setter.Tables["tabla"].Rows.Count <= 0)
            {
                data.Add("response", 1001);
                data.Add("data", null);
                data.Add("value", 0);
                data.Add("message", "Proceso realizado con éxito.");
                return data;
            }

            using (FileStream fileStream = new FileStream(fotoServ, FileMode.Create))
            {
                try
                {
                    file.CopyTo(fileStream);
                    transaction.Commit();

                    return new JObject()
                    {
                        { "response", 1 },
                        { "data", JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(setter.Tables["tabla"])) },
                        { "value", 1 },
                        { "message", "Proceso realizado con éxito." }
                    };
                }
                catch (Exception ex)
                {
                    data.Add("response", 1001);
                    data.Add("data", null);
                    data.Add("value", 0);
                    data.Add("message", "Proceso realizado con éxito.");
                    return data;
                }
            }
        }
    }
}
