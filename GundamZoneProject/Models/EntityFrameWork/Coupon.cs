using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GundamZoneProject.Models.EntityFrameWork
{
    public class Coupon
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public int DiscountType { get; set; } // 1 = %, 2 = tiền

        public decimal DiscountValue { get; set; }

        public decimal MinOrderValue { get; set; }

        public int Quantity { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
