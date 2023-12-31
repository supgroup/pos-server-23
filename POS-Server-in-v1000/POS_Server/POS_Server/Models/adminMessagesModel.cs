﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class adminMessagesModel
    {
        public int msgId { get; set; }
        public string title { get; set; }
        public string msgContent { get; set; }
        public bool isActive { get; set; }
        public string notes { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public bool canDelete { get; set; }
        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorName { get; set; }
        public Nullable<int> mainMsgId { get; set; }

    }
}