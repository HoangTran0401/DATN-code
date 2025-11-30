using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GundamZoneProject.Models.EntityFrameWork
{
    public class Import
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int OldStock { get; set; }   
        public int NewStock { get; set; }   
        public DateTime ImportDate { get; set; } = DateTime.Now;

        public virtual Product Product { get; set; }
    }

}