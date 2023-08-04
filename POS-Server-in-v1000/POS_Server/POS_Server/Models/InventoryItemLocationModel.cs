using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class InventoryItemLocationModel
    {
        public int id { get; set; }
        public int sequence { get; set; }
        public Nullable<bool> isDestroyed { get; set; }
        public Nullable<int> amount { get; set; }
        public Nullable<int> amountDestroyed { get; set; }
        public Nullable<int> quantity { get; set; }  //realAmount
        public Nullable<int> itemLocationId { get; set; }
        public Nullable<int> inventoryId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public string notes { get; set; }
        public Boolean canDelete { get; set; }
        public string itemName { get; set; }
        public int itemId { get; set; }
        public int unitId { get; set; }
        public int itemUnitId { get; set; }
        public string location { get; set; }
        public string section { get; set; }
        public string unitName { get; set; }
        public string inventoryNum { get; set; }
        public Nullable<System.DateTime> inventoryDate { get; set; }
        public string itemType { get; set; }
        public string cause { get; set; }
        public string fallCause { get; set; }

        public bool isFalls { get; set; }
        public Nullable<decimal> avgPurchasePrice { get; set; }
        public Nullable<decimal> total { get; set; }
    }
}