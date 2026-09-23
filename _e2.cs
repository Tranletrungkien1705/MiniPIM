using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCenter.Common.Models
{
    public class Mst_Unit
    {
        public object UnitCode { get; set; }    // Mã UnitCodeSys ngầm, ko show giao diện

        public object OrgID { get; set; }

        public object NetworkID { get; set; }

        public object UnitCodeUser { get; set; }    // Mã đơn vị tính người dùng nhập

        public object UnitName { get; set; }    // Tên đơn vị tính

        public object Remark { get; set; }  // Ghi chú, mô tả

        public object FlagActive { get; set; }  // Trạng thái

        public object CodeGuid { get; set; }

        public object LogLUDTimeUTC { get; set; }

        public object LogLUBy { get; set; }
        
        public object SolutionCode { get; set; }

        public object FunctionActionType { get; set; }

        public object DTimeUsed { get; set; }  // Cờ đã tham gia nghiệp vụ
    }
}
