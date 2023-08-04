using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class posPrintersModel
    {
        public int printerId { get; set; }
        public string name { get; set; }
        public string printerName { get; set; }
        public string notes { get; set; }
        public Nullable<int> posId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public string purpose { get; set; }
        public byte isActive { get; set; }
        public int copiesCount { get; set; }
        public bool canDelete { get; set; }
        public Nullable<int> sizeId { get; set; }
        public string sizeName { get; set; }
        public string sizeValue { get; set; }
        public List<invoiceTypesPrintersModel> invoiceTypesPrintersList { get; set; }
        
    }
}