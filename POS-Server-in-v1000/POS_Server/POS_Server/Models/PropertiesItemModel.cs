using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class PropertiesItemModel
    {
        public int propertyItemId { get; set; }
        public string propertyItemName { get; set; }
        public Nullable<int> propertyId { get; set; }
        public string propertyName { get; set; }
        public Nullable<short> isDefault { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Boolean canDelete { get; set; }
        public int propertyIndex { get; set; }
    }
}