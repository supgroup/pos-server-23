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
    
    public partial class siteContact
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string subject { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> subjectId { get; set; }
        public bool isActive { get; set; }
        public Nullable<int> updateUserId { get; set; }
    
        public virtual siteSubject siteSubject { get; set; }
        public virtual users users { get; set; }
    }
}
