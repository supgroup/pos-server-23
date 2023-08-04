using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class ErrorModel
    {
        public int errorId { get; set; }
        public string num { get; set; }
        public string msg { get; set; }
        public string stackTrace { get; set; }
        public string targetSite { get; set; }
        public Nullable<int> posId { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public string programNamePos { get; set; }
        public string versionNamePos { get; set; }
        public string programNameServer { get; set; }
        public string versionNameServer { get; set; }

        public string source { get; set; }
        public string method { get; set; }

    }
}