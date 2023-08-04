using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class CategoryModel
    {
        public int categoryId { get; set; }
        public string categoryCode { get; set; }
        public string name { get; set; }
        public string details { get; set; }
        public string image { get; set; }
        public Nullable<short> isActive { get; set; }
        public Nullable<decimal> taxes { get; set; }
        public Nullable<byte> fixedTax { get; set; }
        public Nullable<int> parentId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public string notes { get; set; }
        public Boolean canDelete { get; set; }
        public Nullable<int> sequence { get; set; }
        public Nullable<int> id { get; set; }
        public bool isTaxExempt { get; set; }

    }
}