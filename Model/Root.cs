using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Bank.Model
{
    class Root
    {
        public class Archivo
        {
            public string claveArchivo { get; set; }
            public string cuenta { get; set; }
            public List<Exito> exitos { get; set; }
            public List<Rechazo> rechazos { get; set; }
        }

        public class Errores
        {
            public int codigo { get; set; }
            public string descripcion { get; set; }
        }
        public class Exito
        {
            public int consecutivo { get; set; }
            public string bandaMagnetica { get; set; }
        }

        public class Rechazo
        {
            public int consecutivo { get; set; }
            public string bandaMagnetica { get; set; }
            public List<Errores> error { get; set; }
        }

        public class ResponseBanregio
        {
            public List<Archivo> archivos { get; set; }
        }
    }
}
