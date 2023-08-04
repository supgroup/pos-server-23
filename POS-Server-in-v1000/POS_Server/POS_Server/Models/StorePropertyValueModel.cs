using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class StorePropertyValueModel
    {
        public int storeProbValueId { get; set; }
        public Nullable<int> propertyId { get; set; }
        public Nullable<int> propertyItemId { get; set; }
        public Nullable<int> storeProbId { get; set; }
        public string propertyName { get; set; }
        public string propertyValue { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public int isActive { get; set; }

        public int propertyIndex { get; set; }

    }
}