using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class AgentModel
    {
        public int agentId { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string company { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string image { get; set; }
        public string type { get; set; }
        public string accType { get; set; }
        public Nullable<decimal> balance { get; set; }
        public Nullable<byte> balanceType { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public string notes { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Nullable<decimal> maxDeserve { get; set; }
        public string fax { get; set; }
        public Boolean canDelete { get; set; }
        public bool isLimited { get; set; }
        public string payType { get; set; }
    }
}