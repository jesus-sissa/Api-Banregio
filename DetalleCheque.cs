using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Bank
{
    public class DetalleCheque
    {
        public string tipoCheque { get; set; }
        public int referenciaControl { get; set; }
        public string bandaMagnetica { get; set; }
        public string numeroSeguridadInvisible { get; set; }
        public string fechaRecepcion { get; set; }
        public decimal importe { get; set; }
    }
}
