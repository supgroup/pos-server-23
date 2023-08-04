using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class TaxesModel
    {
        public int taxId { get; set; }
        public string name { get; set; }
        public string nameAr { get; set; }
        public Nullable<decimal> rate { get; set; }
        public Nullable<int> taxTypeId { get; set; }
        public Nullable<bool> isActive { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public bool canDelete { get; set; }
        public string taxType { get; set; }
    }
}