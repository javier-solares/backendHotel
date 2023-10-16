using Newtonsoft.Json.Linq;

namespace dw_backend.Helpers.Responses
{
    public class Responses
    {

        private static int val;
        private static string valException;

        public static JObject Payback(int value, string? ex)
        {
            val = value;
            valException = string.IsNullOrEmpty(ex) ? value.ToString() : ex;

            JObject data = new JObject();

            switch (val)
            {
                //success code
                case 1:
                    data.Add("response", 1001);
                    data.Add("data", value);
                    data.Add("value", value);
                    data.Add("message", "Proceso realizado con éxito.");
                    return data;

                // empty or null codes 2000 -2999
                case 2001:
                    data.Add("response", 2001);
                    data.Add("data", value);
                    data.Add("value", value);
                    data.Add("message", "El valor del identificador no puede ir vacío.");
                    return data;

                case 2002:
                    data.Add("response", 2002);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "El valor inicial no puede ir vacío.");
                    return data;

                case 2003:
                    data.Add("response", 2003);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "El valor final no puede ir vacío.");
                    return data;

                case 2004:
                    data.Add("response", 2004);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "Debe definir el campo del dato inicial.");
                    return data;

                case 2005:
                    data.Add("response", 2005);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "Debe definir el campo del dato final.");
                    return data;

                case 2006:
                    data.Add("response", 2006);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "Debe definir el campo del valor de estación.");
                    return data;

                case 2007:
                    data.Add("response", 2007);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "Debe definir el campo del valor del tamaño de imagen.");
                    return data;

                case 2008:
                    data.Add("response", 2008);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "El valor del tamaño de la imagen no puede ir vacío.");
                    return data;

                case 2009:
                    data.Add("response", 0);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "No se han encontrado datos relacionados con la búsqueda.");
                    return data;

                case 2010:
                    data.Add("response", 2010);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "Uno o más parámetros requeridos no contienen valor.");
                    return data;

                case 2011:
                    data.Add("response", 2011);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "Parámetro principal es requerido.");
                    return data;

                //files codes 6000 - 6999
                case 6001:
                    data.Add("response", 6001);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "No se ha podido descargar el recurso solicitado.");
                    return data;

                case 6002:
                    data.Add("response", 6002);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "No se ha encontrado el archivo solicitado.");
                    return data;

                case 6003:
                    data.Add("response", 6003);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "Cantidad de archivos sobre pasa el límite sooportado (10).");
                    return data;

                //null codes 7000 - 7999
                case 7001:
                    data.Add("response", 7001);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "No existen datos relacionados con la búsqueda.");
                    return data;

                default:
                    data.Add("response", 0);
                    data.Add("data", null);
                    data.Add("value", value);
                    data.Add("message", "Proceso no realizado.");
                    return data;
            }
        }

    }
}
