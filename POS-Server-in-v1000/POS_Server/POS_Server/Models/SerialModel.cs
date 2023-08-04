using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class SerialModel
    {
        public int serialId { get; set; }
        public Nullable<int> itemsTransId { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public string serialNum { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public bool isSold { get; set; }
        public Nullable<int> branchId { get; set; }
        public bool isManual { get; set; }
        public bool canDelete { get; set; }
        

    }
}