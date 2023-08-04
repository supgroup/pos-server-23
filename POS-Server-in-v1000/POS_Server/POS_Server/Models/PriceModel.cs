using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class PriceModel
    {
        public int priceId { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public Nullable<int> sliceId { get; set; }
        public string notes { get; set; }
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> basicPrice { get; set; }
        public Nullable<decimal> priceTax { get; set; }
        public string name { get; set; }
        public Boolean canDelete { get; set; }
        public string sliceName { get; set; }
        public string unitName { get; set; }
        public string itemName { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<int> unitId { get; set; }
        public string itemType { get; set; }
        public Nullable<decimal> avgPurchasePrice { get; set; }
        public Nullable<decimal> unitCost { get; set; }
        public Nullable<decimal> itemUnitPrice { get; set; }
        public Nullable<decimal> itemUnitCost{ get; set; }

    }
}