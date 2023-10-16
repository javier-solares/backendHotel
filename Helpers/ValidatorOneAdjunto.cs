using dw_backend.Helpers.Responses;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace dw_backend.Helpers
{
    public class ValidatorOneAdjunto
    {
        public static JObject ValidateDatabaseData(SqlCommand cmd, string _root, string _replaceFile)
        {
            cmd.CommandType = CommandType.StoredProcedure;
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

            JArray anAdjunto = new JArray();

            string path;
            string imageBase64;
            byte[] doc64;
            string extension;
            string adjunto;
            string format;

            if (!string.IsNullOrEmpty(setter.Tables["tabla"].Rows[0]["ubicacion"].ToString()))
            {
                path = Path.Combine(_root, setter.Tables["tabla"].Rows[0]["ubicacion"].ToString());

                try
                {
                    extension = setter.Tables["tabla"].Rows[0]["extension"].ToString();
                    doc64 = System.IO.File.ReadAllBytes(path);
                }
                catch (Exception)
                {
                    path = Path.Combine(_root, _replaceFile);
                    doc64 = System.IO.File.ReadAllBytes(path);
                    extension = Path.GetExtension(path);
                    extension = extension.Replace(".", "");
                }
            }
            else
            {
                path = Path.Combine(_root, _replaceFile);
                doc64 = System.IO.File.ReadAllBytes(path);
                extension = Path.GetExtension(path);
                extension = extension.Replace(".", "");
            }

            format = extension == "pdf" ? "data:application/" : "data:image/";
            //format = (extension == "pdf" ? "data:application/" : extension == "png" ? "data:application/" : "data:image/");



            imageBase64 = Convert.ToBase64String(doc64);
            adjunto = format + extension + ";base64," + imageBase64;

            try
            {
                anAdjunto.Add(new JObject(
                                 new JProperty("id", setter.Tables["tabla"].Rows[0]["id"].ToString()),
                                 new JProperty("nombre", setter.Tables["tabla"].Rows[0]["nombre"].ToString()),
                                 new JProperty("descripcion", setter.Tables["tabla"].Rows[0]["descripcion"].ToString()),
                                 new JProperty("estado", setter.Tables["tabla"].Rows[0]["estado"].ToString()),
                                 new JProperty("extension", setter.Tables["tabla"].Rows[0]["extension"].ToString()),
                                 new JProperty("adjunto", adjunto),
                                 new JProperty("response", 1)
                                ));
            }
            catch (Exception ex)
            {
                anAdjunto.Add(new JObject(
                                 new JProperty("message", ex.Message),
                                 new JProperty("id", "NA"),
                                 new JProperty("nombre", "NA"),
                                 new JProperty("descripcion", "NA"),
                                 new JProperty("estado", "NA"),
                                 new JProperty("extension", "NA"),
                                 new JProperty("adjunto", "NA"),
                                 new JProperty("response", 0)
                                ));
            }

            return new JObject()
            {
                { "response", 1 },
                { "data", anAdjunto },
                { "value", 1 },
                { "message", "Proceso realizado con éxito." }
            };
        }
    }
}
