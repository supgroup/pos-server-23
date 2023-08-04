﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class PosModel
    {
        public int posId { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string branchName { get; set; }
        public string branchCode { get; set; }
        public Nullable<decimal> balance { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Boolean canDelete { get; set; }
        public string note { get; set; }
        public Nullable<decimal> balanceAll { get; set; }
        public string boxState { get; set; }
        public byte isAdminClose { get; set; }
    }
}