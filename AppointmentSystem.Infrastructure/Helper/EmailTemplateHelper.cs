using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.Infrastructure.Helper
{
    public static class EmailTemplateHelper
    {
        public static string LoadTemplate(string fileName, Dictionary<string, string> replacements)
        {
            //Creamos el path de la ruta donde tenemos el template guardado.

            var path = Path.Combine(AppContext.BaseDirectory, "Templates", "Email", fileName);

            //Verificamos que el archivo exista

            if (!File.Exists(path))
                throw new FileNotFoundException($"No se encontró la plantilla de email: {path}");

            //Leemos el contenido del archivo 

            var content = File.ReadAllText(path);

            //Reemplazamos las variables en el contenido del template, asi recorremos un diccionario de key value
            foreach (var kvp in replacements)
            {
                content = content.Replace($"{{{kvp.Key}}}", kvp.Value);
            }

            return content;
        }
    }

}
