using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GundamZoneProject.Models
{
        public class Overview
        {
            public int ProductId { get; set; }
            public string Title { get; set; }
            public string Image { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }
}