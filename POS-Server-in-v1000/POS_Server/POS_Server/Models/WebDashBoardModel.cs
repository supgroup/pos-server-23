using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class WebDashBoardModel
    {
        public int purchasesCount { get; set; }
        public int salesCount { get; set; }
        public int vendorsCount { get; set; }
        public int customersCount { get; set; }
        public int onLineUsersCount { get; set; }
        public decimal balance { get; set; }
        public decimal netProfit { get; set; }
        public int branchId { get; set; }
    }
}