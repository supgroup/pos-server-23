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
    
    public partial class Prices
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Prices()
        {
            this.itemsTransfer = new HashSet<itemsTransfer>();
        }
    
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
        public string name { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<itemsTransfer> itemsTransfer { get; set; }
        public virtual itemsUnits itemsUnits { get; set; }
        public virtual slices slices { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}
