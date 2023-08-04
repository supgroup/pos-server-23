using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class PosSettingModel
    {
        public int posSettingId { get; set; }
        public Nullable<int> posId { get; set; }
        public Nullable<int> saleInvPrinterId { get; set; }
        public Nullable<int> reportPrinterId { get; set; }
        public Nullable<int> saleInvPapersizeId { get; set; }

        public string posSerial { get; set; }

        public Nullable<int> repprinterId { get; set; }
        public string repname { get; set; }
        public string repprintFor { get; set; }

        public Nullable<int> salprinterId { get; set; }
        public string salname { get; set; }
        public string salprintFor { get; set; }

        public Nullable<int> sizeId { get; set; }
        public string paperSize1 { get; set; }
        public Nullable<int> docPapersizeId { get; set; }
        public string docPapersize { get; set; }
        public string saleSizeValue { get; set; }
        public string docSizeValue { get; set; }
        public string programName { get; set; }
        public string versionName { get; set; }

    }
}