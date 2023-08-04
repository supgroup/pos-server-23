using Newtonsoft.Json;
using POS_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/Memberships")]
    public class MembershipsController : ApiController
    {
        CountriesController cc = new CountriesController();
        // GET api/<controller>
        [HttpPost]
        [Route("Get")]
        public IHttpActionResult Get()
        {
            var re = Request;
            var headers = re.Headers;
            string token = "";
            bool canDelete = false;

            if (headers.Contains("APIKey"))
            {
                token = headers.GetValues("APIKey").First();
            }
            Validation validation = new Validation();
            bool valid = validation.CheckApiKey(token);

            if (valid) // APIKey is valid
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var List = (from S in  entity.memberships                                         
                                         select new MembershipsModel()
                                         {
 
                                            createDate = S.createDate,
                                            updateDate = S.updateDate,
                                            createUserId = S.createUserId,
                                            updateUserId=S.updateUserId,

                                             notes = S.notes,
                                             isActive = S.isActive,
                                             membershipId = S.membershipId,
                                             name = S.name,
                                             deliveryDiscount=S.deliveryDiscount,
                                             deliveryDiscountType=S.deliveryDiscountType,
                                             invoiceDiscount=S.invoiceDiscount,
                                             invoiceDiscountType=S.invoiceDiscountType,
                                          


                                         }).ToList();
                    /*

 public int membershipId { get; set; }
        public string name { get; set; }
        public Nullable<decimal> deliveryDiscount { get; set; }
        public string deliveryDiscountType { get; set; }
        public Nullable<decimal> invoiceDiscount { get; set; }
        public string invoiceDiscountType { get; set; }
        public Nullable<decimal> subscriptionFee { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public bool canDelete { get; set; }


membershipId
name
deliveryDiscount
deliveryDiscountType
invoiceDiscount
invoiceDiscountType
subscriptionFee
notes
createDate
updateDate
createUserId
updateUserId
isActive
canDelete


                    */

                    if (List.Count > 0)
                    {
                        for (int i = 0; i < List.Count; i++)
                        {
                            if (List[i].isActive == 1)
                            {
                                int membershipId = (int)List[i].membershipId;
                                var itemsI= entity.agentMemberships.Where(x => x.membershipId == membershipId).Select(b => new { b.agentMembershipsId }).FirstOrDefault();
                               
                                if ((itemsI is null)  )
                                    canDelete = true;
                            }
                            List[i].canDelete = canDelete;
                        }
                    }

                    if (List == null)
                        return NotFound();
                    else
                        return Ok(List);
                }
            }
            //else
            return NotFound();
        }

        // GET api/<controller>
        [HttpPost]
        [Route("GetByID")]
        public IHttpActionResult GetByID(int membershipId)
        {
            var re = Request;
            var headers = re.Headers;
            string token = "";
            if (headers.Contains("APIKey"))
            {
                token = headers.GetValues("APIKey").First();
            }
            Validation validation = new Validation();
            bool valid = validation.CheckApiKey(token);

            if (valid)
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var row = entity.memberships
                   .Where(u => u.membershipId == membershipId)
                   .Select(S => new
                   {
                           S.membershipId,
                           S.name,
                      
                           S.notes,
                           S.createDate,
                           S.updateDate,
                           S.createUserId,
                           S.updateUserId,
                           S.isActive,

                          S.deliveryDiscount,
                          S.deliveryDiscountType,
                          S.invoiceDiscount,
                          S.invoiceDiscountType,
                       



                   })
                   .FirstOrDefault();

                    if (row == null)
                        return NotFound();
                    else
                        return Ok(row);
                }
            }
            else
                return NotFound();
        }

        // add or update location
        [HttpPost]
        [Route("Save")]
        public string Save(string Object)
        {
            var re = Request;
            var headers = re.Headers;
            string token = "";
            string message = "";
            if (headers.Contains("APIKey"))
            {
                token = headers.GetValues("APIKey").First();
            }
            Validation validation = new Validation();
            bool valid = validation.CheckApiKey(token);

            if (valid)
            {
                Object = Object.Replace("\\", string.Empty);
                Object = Object.Trim('"');
                memberships newObject = JsonConvert.DeserializeObject<memberships>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                if (newObject.updateUserId == 0 || newObject.updateUserId == null)
                {
                    Nullable<int> id = null;
                    newObject.updateUserId = id;
                }
                if (newObject.createUserId == 0 || newObject.createUserId == null)
                {
                    Nullable<int> id = null;
                    newObject.createUserId = id;
                }
               
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var locationEntity = entity.Set<memberships>();
                        if (newObject.membershipId == 0)
                        {
                            newObject.createDate = cc.AddOffsetTodate(DateTime.Now);
                            newObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            newObject.updateUserId = newObject.createUserId;
                         
                      
                            locationEntity.Add(newObject);
                            entity.SaveChanges();
                            message = newObject.membershipId.ToString();
                        }
                        else
                        {
                            var tmpObject = entity.memberships.Where(p => p.membershipId == newObject.membershipId).FirstOrDefault();

                            tmpObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            tmpObject.updateUserId = newObject.updateUserId;

                            tmpObject.name  =newObject.name;
                      
                            tmpObject.notes  =newObject.notes;
                            tmpObject.isActive=newObject.isActive;
                            tmpObject.deliveryDiscount = newObject.deliveryDiscount;
                            tmpObject.deliveryDiscountType = newObject.deliveryDiscountType;
                          tmpObject.invoiceDiscount = newObject.invoiceDiscount;
                          tmpObject.invoiceDiscountType = newObject.invoiceDiscountType;
                     

                            entity.SaveChanges();

                            message = tmpObject.membershipId.ToString();
                        }
                      //  entity.SaveChanges();
                    }
                }
                catch
                {
                    message = "-1";
                }
            }
            return message;
        }

        [HttpPost]
        [Route("Delete")]
        public string Delete(int membershipId, int userId,bool final)
        {
            var re = Request;
            var headers = re.Headers;
            string token = "";
            int message = 0;
            if (headers.Contains("APIKey"))
            {
                token = headers.GetValues("APIKey").First();
            }

            Validation validation = new Validation();
            bool valid = validation.CheckApiKey(token);
            if (valid)
            {
                if (final)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            memberships objectDelete = entity.memberships.Find(membershipId);

                            entity.memberships.Remove(objectDelete);
                        message=    entity.SaveChanges();
                          
                            return message.ToString();
                        }
                    }
                    catch
                    {
                        return "-1";
                    }
                }
                else
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            memberships objectDelete = entity.memberships.Find(membershipId);

                            objectDelete.isActive = 0;
                            objectDelete.updateUserId = userId;
                            objectDelete.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            message= entity.SaveChanges();

                            return message.ToString(); ;
                        }
                    }
                    catch
                    {
                        return "-2";
                    }
                }
            }
            else
                return "-3";
        }



    }
}