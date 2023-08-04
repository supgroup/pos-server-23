using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class InvoiceTypesModel
    {
        public int invoiceTypeId { get; set; }
        public string invoiceType { get; set; }
        public string department { get; set; }
        public string notes { get; set; }
        public string allowPaperSize { get; set; }
        public Nullable<bool> isActive { get; set; }
        public Nullable<int> sequence { get; set; }

        // public string msgNotes { get; set; }
    }
}