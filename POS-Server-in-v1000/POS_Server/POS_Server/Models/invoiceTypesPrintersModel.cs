using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class invoiceTypesPrintersModel
    {
        public int invTypePrinterId { get; set; }
        public Nullable<int> printerId { get; set; }
        public Nullable<int> invoiceTypeId { get; set; }
        public Nullable<int> sizeId { get; set; }
        public string notes { get; set; }
        public int copyCount { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public string printerName { get; set; }
        public string invoiceTypeName { get; set; }
        public string sizeName { get; set; }
        public string invoiceType { get; set; }
        public string sizeValue { get; set; }
        public string printerSysName { get; set; }

        // public string msgNotes { get; set; }
    }
}