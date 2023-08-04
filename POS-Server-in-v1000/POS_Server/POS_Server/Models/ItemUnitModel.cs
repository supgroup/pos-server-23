using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class ItemUnitModel
    {
        public Nullable<decimal> packCost { get; set; }
        public int itemUnitId { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<int> unitId { get; set; } 
        public Nullable<int> unitValue { get; set; }
        public Nullable<short> defaultSale { get; set; }
        public Nullable<short> defaultPurchase { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> basicPrice { get; set; }
        public Nullable<decimal> cost { get; set; }
        public Nullable<decimal> avgPurchasePrice { get; set; }
        public string barcode { get; set; }
        public string mainUnit { get; set; }
        public string smallUnit { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> subUnitId { get; set; }
        public string itemName { get; set; }
        public string itemCode { get; set; }
        public string unitName { get; set; }
        public Nullable<int> storageCostId { get; set; }
        public Boolean canDelete { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Nullable<decimal> taxes { get; set; }
        public Nullable<int> warrantyId { get; set; }
        public bool hasWarranty { get; set; }
        public List<itemsPropModel> ItemProperties { get; set; }
        public string itemType { get; set; }
        public Nullable<int> branchId { get; set; }
        //public string branchName { get; set; }
        //public string branchType { get; set; }
        public Nullable<long> quantity { get; set; }
        public Nullable<long> serialsCount { get; set; }
        public Nullable<long> PropertiesCount { get; set; }
        public bool skipProperties { get; set; }
        public bool skipSerialsNum { get; set; }
    }
}