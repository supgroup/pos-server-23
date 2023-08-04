﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class InventoryModel
    {
        public int inventoryId { get; set; }
        public int branchId { get; set; }
        public int posId { get; set; }
        public string num { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public string notes { get; set; }
        public Boolean canDelete { get; set; }
        public string inventoryType { get; set; }
        public Nullable<int> mainInventoryId { get; set; }
    }
}