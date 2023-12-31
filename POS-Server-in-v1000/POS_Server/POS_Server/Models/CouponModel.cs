﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class CouponModel
    {
        public int cId { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Nullable<byte> discountType { get; set; }
        public Nullable<decimal> discountValue { get; set; }
        public Nullable<System.DateTime> startDate { get; set; }
        public Nullable<System.DateTime> endDate { get; set; }
        public string notes { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<int> remainQ { get; set; }
        public Nullable<decimal> invMin { get; set; }
        public Nullable<decimal> invMax { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public string barcode { get; set; }
        public Boolean canDelete { get; set; }
        public string details { get; set; }
    }
}