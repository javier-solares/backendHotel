using dw_backend.Functions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using dw_backend.Helpers.Responses;
using System.IO;
using System.Text.RegularExpressions;

namespace dw_backend.Functions
{
    public class FileValidator
    {
        public static JObject ValidatePrettyImage(IFormFile doc)
        {
            var file = doc;
            string imageName = file.FileName;
            string extension = Path.GetExtension(imageName);
            long sizeImage = file.Length;

            imageName = CleanText.RemoveSpaces(imageName);
            imageName = CleanText.RemoveAccents(imageName);

            Regex reg = new Regex(@"^.*\.(pdf) || \.(pdf)$ || \.(jpg|png|jpeg|webp)$");

            if (!reg.IsMatch(extension))
            {
                return Responses.Payback(1001, null);
            }

            return new JObject()
            {
                { "response", 1 },
                { "imageName", imageName },
                { "extension", extension },
                { "value", 1 },
                { "message", "Proceso realizado con éxito." }
            };
        }
    }
}
