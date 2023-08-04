using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class ItemLocationModel
    {
        public int sequence { get; set; }
        public int itemsLocId { get; set; }
        public Nullable<int> locationId { get; set; }
        public Nullable<int> sectionId { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<long> quantity { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<System.DateTime> startDate { get; set; }
        public Nullable<System.DateTime> endDate { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public Nullable<int> itemId { get; set; }
        public string note { get; set; }
        public string itemName { get; set; }
        public string location { get; set; }
        public string section { get; set; }
        public string unitName { get; set; }
        public string itemType { get; set; }
        public Nullable<decimal> storeCost { get; set; }
        public Nullable<byte> isFreeZone { get; set; }
        public Nullable<int> invoiceId { get; set; }
        public string invNumber { get; set; }
        public string invType { get; set; }
        public Nullable<long> lockedQuantity { get; set; }
        public string sectionName { get; set; }
        public bool isExpired { get; set; }
        public int alertDays { get; set; }
        public Nullable<decimal> avgPurchasePrice { get; set; }
        public Nullable<byte> isSelected { get; set; }


    }
}