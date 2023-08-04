using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class PointsModel
    {
        public int pointId { get; set; }
        public Nullable<decimal> Cash { get; set; }
        public Nullable<int> CashPoints { get; set; }
        public Nullable<int> invoiceCount { get; set; }
        public Nullable<int> invoiceCountPoints { get; set; }
        public Nullable<decimal> CashArchive { get; set; }
        public Nullable<int> CashPointsArchive { get; set; }
        public Nullable<int> invoiceCountArchive { get; set; }
        public Nullable<int> invoiceCountPoinstArchive { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Nullable<int> agentId { get; set; }
        public bool canDelete { get; set; }
    }
}