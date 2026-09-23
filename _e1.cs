using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCenter.Common.Models
{
    public class Mst_Brand
    {
        public object OrgID { get; set; }

        public object BrandCode { get; set; }

        public object NetworkID { get; set; }

        public object BrandName { get; set; }

		public object NetworkBrandCode { get; set; }

		public object Remark { get; set; }

        public object FlagActive { get; set; }

        public object LogLUDTimeUTC { get; set; }

        public object LogLUBy { get; set; }

        public object SolutionCode { get; set; }

        public object FunctionActionType { get; set; }
    }
}
