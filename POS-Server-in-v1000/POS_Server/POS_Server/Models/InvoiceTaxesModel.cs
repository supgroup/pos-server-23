using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class InvoiceTaxesModel
    {
        public int invoiceTaxId { get; set; }
        public Nullable<int> taxId { get; set; }
        public Nullable<decimal> rate { get; set; }
        public Nullable<decimal> taxValue { get; set; }
        public string notes { get; set; }
        public Nullable<int> invoiceId { get; set; }
        public string taxType { get; set; }
        public string name { get; set; }
        public string nameAr { get; set; }
    }
}