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
    
    public partial class InvoiceTypes
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public InvoiceTypes()
        {
            this.invoiceTypesPrinters = new HashSet<invoiceTypesPrinters>();
        }
    
        public int invoiceTypeId { get; set; }
        public string invoiceType { get; set; }
        public string department { get; set; }
        public string notes { get; set; }
        public string allowPaperSize { get; set; }
        public Nullable<bool> isActive { get; set; }
        public Nullable<int> sequence { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<invoiceTypesPrinters> invoiceTypesPrinters { get; set; }
    }
}