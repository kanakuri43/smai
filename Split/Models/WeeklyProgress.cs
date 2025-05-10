using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Split.Models
{
    [Table("S進捗")]
    public class WeeklyProgress
    {
        [Column("月度")]
        public int YearMonth { get; set; }

        [Column("日付")]
        public int Date { get; set; }

        [Column("社員コード")]
        public int EmployeeCode { get; set; }

        [Column("売上実績")]
        public decimal SalesResult { get; set; }

        [Column("売上見込1")]
        public decimal SalesEstimate1 { get; set; }

        [Column("売上見込2")]
        public decimal SalesEstimate2 { get; set; }

        [Column("粗利実績")]
        public decimal ProfitResult { get; set; }

        [Column("粗利見込1")]
        public decimal ProfitEstimate1 { get; set; }

        [Column("粗利見込2")]
        public decimal ProfitEstimate2 { get; set; }
        [Column("進捗区分")]
        public byte ProgressType { get; set; }

    }
}
