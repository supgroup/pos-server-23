using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class DashModel
    {



    }
    public class TotalPurSale
    {
        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorName { get; set; }
        public Nullable<decimal> totalPur { get; set; }
        public Nullable<decimal> totalSale { get; set; }
        public int countPur { get; set; }
        public int countSale { get; set; }
        public int day { get; set; }

    }
    public class UserOnlineCount
    {
        //  public Nullable<int> branchId { get; set; }
        public int branchId { get; set; }
        public string branchName { get; set; }
        public int userOnlineCount { get; set; }

        public int allPos { get; set; }
        public int offlineUsers { get; set; }
        //   public List<userOnlineInfo> userOnlinelist = new List<userOnlineInfo>();
    }
    public class PosOnlineCount
    {
        //  public Nullable<int> branchId { get; set; }
        public int branchId { get; set; }
        public string branchName { get; set; }
        public int posOnlineCount { get; set; }
        public int allPos { get; set; }
        public int offlinePos { get; set; }

        //   public List<userOnlineInfo> userOnlinelist = new List<userOnlineInfo>();
    }

    public class userOnlineInfo
    {
        public Nullable<int> branchId { get; set; }
        public string branchName { get; set; }
        public Nullable<byte> branchisActive { get; set; }
        public Nullable<int> posId { get; set; }
        public string posName { get; set; }
        public Nullable<byte> posisActive { get; set; }
        public Nullable<int> userId { get; set; }
        public string usernameAccount { get; set; }
        public string userName { get; set; }
        public string lastname { get; set; }
        public string job { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public Nullable<short> userisActive { get; set; }
        public Nullable<byte> isOnline { get; set; }
        public string image { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }

    }
    public class BranchOnlineCount
    {

        public int branchOnline { get; set; }
        public int branchAll { get; set; }
        public int branchOffline { get; set; }


    }

    public class IUStorage
    {


        public string itemName { get; set; }
        public string unitName { get; set; }
        public int itemUnitId { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<int> unitId { get; set; }
        public string branchName { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<long> quantity { get; set; }



    }
    public class CardsSts
    {

        public int cardId { get; set; }
        public string name { get; set; }
        public Nullable<decimal> total { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Nullable<bool> hasProcessNum { get; set; }
        public string image { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<int> posId { get; set; }
        public string branchName { get; set; }
    }

    public class AgentsCountbyBranch
    {
        public int vendorsCount { get; set; }
        public int customersCount { get; set; }
        public int branchId { get; set; }
        public string branchName { get; set; }
    }
}