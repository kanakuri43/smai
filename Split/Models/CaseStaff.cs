using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Split.Models
{
    [Table("D物件担当")]

    public class CaseStaff
    {
        [Column("物件連番")]
        public int CaseId { get; set; }
        [Column("社員コード")]
        public int EmployeeCode { get; set; }

    }
}
