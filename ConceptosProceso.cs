using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Bank
{

    public class ConceptosProceso
    {
        public int Cantidatad { set; get; }
        public List<Conceptos> Conceptos { set; get; }
    }
    public class Conceptos
    {
        public string Nombre { set; get; }
        public double Precio { set; get; }

    }
    public class masterr
    {       
        List<ConceptosProceso> _C = new List<ConceptosProceso>();
     public void Llenar()
        {
            _C.Add(new ConceptosProceso { Cantidatad = 0 ,
                Conceptos=llenar_conceptos()
            
            });
            var listaordernada = from l in _C
                                 orderby l.Cantidatad descending
                                 select l;
        }
        public List<Conceptos> llenar_conceptos()
        {
            List<Conceptos> _C = new List<Conceptos>();

            return _C;
        }

    }
}
