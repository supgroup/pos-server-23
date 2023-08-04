using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class messagesPosModel
    {
        public int msgPosId { get; set; }
        public Nullable<int> msgId { get; set; }
        public Nullable<int> posId { get; set; }
        public bool isReaded { get; set; }
        public string notes { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
     
        public bool canDelete { get; set; }
        public string posName { get; set; }
        public string branchName { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<int> userReadId { get; set; }
        public string userRead { get; set; }
        public string toUserFullName{ get; set; }
        public string userReadName { get; set; }
        public string userReadLastName { get; set; }
        //user
        public Nullable<int> toUserId { get; set; }

        //message

        public string title { get; set; }
        public string msgContent { get; set; }
        public bool isActive { get; set; }
      
        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorName { get; set; }
        public string msgCreatorName { get; set; }
        public string msgCreatorLast { get; set; }
        public Nullable<int> mainMsgId { get; set; }
 
        // public string msgNotes { get; set; }
    }
}