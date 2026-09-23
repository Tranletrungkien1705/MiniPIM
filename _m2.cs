using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCenter.Common.Models
{
    public class RQ_Mst_Brand : WARQBase
    {
        public string Rt_Cols_Mst_Brand { get; set; }

        public Mst_Brand Mst_Brand { get; set; }

        public List<Mst_Brand> Lst_Mst_Brand { get; set; }
    }
}
