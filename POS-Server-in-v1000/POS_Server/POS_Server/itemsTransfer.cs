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
    
    public partial class itemsTransfer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public itemsTransfer()
        {
            this.invoiceOrder = new HashSet<invoiceOrder>();
            this.itemTransferOffer = new HashSet<itemTransferOffer>();
            this.serials = new HashSet<serials>();
            this.storeProperties = new HashSet<storeProperties>();
        }
    
        public int itemsTransId { get; set; }
        public Nullable<long> quantity { get; set; }
        public Nullable<int> invoiceId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public string notes { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public string itemSerial { get; set; }
        public Nullable<int> inventoryItemLocId { get; set; }
        public Nullable<int> offerId { get; set; }
        public Nullable<decimal> profit { get; set; }
        public Nullable<decimal> purchasePrice { get; set; }
        public string cause { get; set; }
        public Nullable<decimal> itemTax { get; set; }
        public Nullable<decimal> itemUnitPrice { get; set; }
        public Nullable<decimal> offerValue { get; set; }
        public Nullable<decimal> offerType { get; set; }
        public string warrantyName { get; set; }
        public string warrantyDescription { get; set; }
        public Nullable<int> warrantyId { get; set; }
        public Nullable<int> priceId { get; set; }
        public string offerName { get; set; }
        public decimal VATRatio { get; set; }
        public bool isTaxExempt { get; set; }
    
        public virtual inventoryItemLocation inventoryItemLocation { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<invoiceOrder> invoiceOrder { get; set; }
        public virtual invoices invoices { get; set; }
        public virtual itemsUnits itemsUnits { get; set; }
        public virtual offers offers { get; set; }
        public virtual Prices Prices { get; set; }
        public virtual warranty warranty { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<itemTransferOffer> itemTransferOffer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<serials> serials { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<storeProperties> storeProperties { get; set; }
    }
}
