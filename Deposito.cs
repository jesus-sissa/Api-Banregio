using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Bank
{
   public class Deposito
    {

        public int consecutivo { get; set; }
        public string divisa { get; set; }
        public Int64 remesa { get; set; }
        public string referencia { get; set; }
        public decimal importeTotal { get; set; }
        public decimal importeFicha { get; set; }
        public decimal diferencia { get; set; }
        public string tipoDiferencia { get; set; }
        public DetalleEfectivo detalleEfectivo { get; set; }
        public List< DetalleCheque> detalleCheque { get; set; }
    }
}
