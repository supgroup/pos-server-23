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
    
    public partial class posSetting
    {
        public int posSettingId { get; set; }
        public Nullable<int> posId { get; set; }
        public Nullable<int> saleInvPrinterId { get; set; }
        public Nullable<int> reportPrinterId { get; set; }
        public Nullable<int> saleInvPapersizeId { get; set; }
        public string posSerial { get; set; }
        public Nullable<int> docPapersizeId { get; set; }
        public string posDeviceCode { get; set; }
        public Nullable<int> posSerialId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public string programName { get; set; }
        public string versionName { get; set; }
    
        public virtual paperSize paperSize { get; set; }
        public virtual pos pos { get; set; }
        public virtual posSerials posSerials { get; set; }
        public virtual printers printers { get; set; }
        public virtual printers printers1 { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}
