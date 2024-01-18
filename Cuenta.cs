using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Bank
{
    public class Cuenta
    {
        public string numeroCuenta { get; set; }
        public string instituto { get; set; }
        public string claveArchivo { get; set; }
        public int numeroDepositos { get; set; }
        public decimal importeTotalEfectivo { get; set; }
        public decimal importeTotalChequesPropios { get; set; }
        public decimal importeTotalChequesOtros { get; set; }
        public string razonSocialCliente { get; set; }
        public List< Deposito> depositos { get; set; }

    }

    public class CuentaI
    {
        public string id { get; set; }
        public string numeroCuenta { get; set; }
        public string instituto { get; set; }
        public string claveArchivo { get; set; }
        public int numeroDepositos { get; set; }
        public decimal importeTotalEfectivo { get; set; }
        public decimal importeTotalChequesPropios { get; set; }
        public decimal importeTotalChequesOtros { get; set; }
        public string razonSocialCliente { get; set; }
        public List<Deposito> depositos { get; set; }

    }
}
