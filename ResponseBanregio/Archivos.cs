using Api_Bank.ResponseBanregio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Bank.ResponseBanregio
{
    public class Archivos
    {
        public string claveArchivo { get; set; }
        public string cuenta { get; set; }
        public List<Exitos> exitos { get; set; }
        public List<Rechazos> rechazos { get; set; }
    }
}
