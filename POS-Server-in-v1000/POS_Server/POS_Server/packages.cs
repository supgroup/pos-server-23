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
    
    public partial class packages
    {
        public int packageId { get; set; }
        public Nullable<int> parentIUId { get; set; }
        public Nullable<int> childIUId { get; set; }
        public int quantity { get; set; }
        public byte isActive { get; set; }
        public string notes { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
    
        public virtual itemsUnits itemsUnits { get; set; }
        public virtual itemsUnits itemsUnits1 { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}
