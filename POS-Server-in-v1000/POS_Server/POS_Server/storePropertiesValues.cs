//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace POS_Server
{
    using System;
    using System.Collections.Generic;
    
    public partial class storePropertiesValues
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
    
        public virtual properties properties { get; set; }
        public virtual propertiesItems propertiesItems { get; set; }
        public virtual storeProperties storeProperties { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}