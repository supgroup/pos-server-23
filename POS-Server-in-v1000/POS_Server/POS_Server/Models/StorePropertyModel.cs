using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class StorePropertyModel
    {
        public int storeProbId { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public Nullable<int> itemsTransId { get; set; }
        public Nullable<int> serialId { get; set; }
        public Nullable<int> branchId { get; set; }
        public long count { get; set; }
        public bool isSold { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<byte> isActive { get; set; }

        public bool isManual { get; set; }

        #region storePropertyValues attributes
        public int storeProbValueId { get; set; }
        public Nullable<int> propertyId { get; set; }
        public Nullable<int> propertyItemId { get; set; }
        public string propName { get; set; }
        public string propValue { get; set; }
        public int propertyIndex { get; set; }

        #endregion
        public List<StorePropertyValueModel> StorePropertiesValueList { get; set; }
        #region serial attributes
        public string serialNum { get; set; }
        public string StorePropertiesValueString { get; set; }
        #endregion
    }
}