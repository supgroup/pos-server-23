using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class MembershipsModel
    {
        public int membershipId { get; set; }
        public string name { get; set; }
        public Nullable<decimal> deliveryDiscount { get; set; }
        public string deliveryDiscountType { get; set; }
        public Nullable<decimal> invoiceDiscount { get; set; }
        public string invoiceDiscountType { get; set; }
        public Nullable<decimal> subscriptionFee { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public bool canDelete { get; set; }
    }
}