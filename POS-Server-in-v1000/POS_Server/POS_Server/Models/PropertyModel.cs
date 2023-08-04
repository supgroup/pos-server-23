﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class PropertyModel
    {
        public int propertyId { get; set; }
        public string name { get; set; }
        public string propertyValues { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Boolean canDelete { get; set; }
        public int propertyIndex { get; set; }

        public List<itemsPropModel> ItemPropValues { get; set; }
        public List<PropertiesItemModel> PropertiesItems { get; set; }
    }
}