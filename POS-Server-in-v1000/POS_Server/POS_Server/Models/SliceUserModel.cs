using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class SliceUserModel
    {

        public int sliceUserId { get; set; }
        public Nullable<int> sliceId { get; set; }
        public Nullable<int> userId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }

        // user
  
        public string username { get; set; }
       
        public string uname { get; set; }
        public string  lastname { get; set; }
        public string  job { get; set; }
        public string workHours { get; set; }
        public string  phone { get; set; }
        public string  mobile { get; set; }
        public string  email { get; set; }
     
        public string  address { get; set; }
        public short? uisActive { get; set; }
        public byte?  isOnline { get; set; }
    
     
        //slice
 
        public string name { get; set; }
        

    }
    }