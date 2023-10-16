using dw_backend.Helpers.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using static iText.Svg.SvgConstants;

namespace dw_backend.Helpers
{
    public class DataValidator
    {
        public static JObject ValidateDatabaseData(SqlCommand cmd)
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
                      
                        data.Add("response", 7001);
                        data.Add("data", null);
                        data.Add("value", 0);
                        data.Add("message", "No existen datos relacionados con la búsqueda.");
                        return data;
                        
                }
            }
            catch (Exception ex)
            {
                data.Add("response", 7001);
                data.Add("data", null);
                data.Add("value", 0);
                data.Add("message", "No existen datos relacionados con la búsqueda.");
                return data;
               
            }

            if (setter.Tables["tabla"].Rows.Count <= 0)
            {
                data.Add("response", 2009);
                data.Add("data", null);
                data.Add("value", 0);
                data.Add("message", "No se han encontrado datos relacionados con la búsqueda.");
                return data;
                
            }

            return new JObject()
            {
                { "response", 1 },
                { "data", JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(setter.Tables["tabla"])) },
                { "value", 1 },
                { "message", "Proceso realizado con éxito." }
            };
        }
    }
}
