using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Bank.ResponseBanregio
{
    public class Rechazos
    {
        public int consecutivo { get; set; }
        public List<Errores> errores { get; set; }
    }
}
