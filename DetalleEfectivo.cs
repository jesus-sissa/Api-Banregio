using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Bank
{
    public class DetalleEfectivo
    {

        public string fechaRecepcion { get; set; }
        public decimal importe { get; set; }
        public List< Denominacion> desglose { get; set; }
    }
}
