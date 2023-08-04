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





    [RoutePrefix("api/AgentMemberships")]
    public class AgentMembershipsController : ApiController
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
                    var List = (from S in  entity.agentMemberships                                         
                                         select new AgentMembershipsModel()
                                         {
                                            agentMembershipsId=S.agentMembershipsId,
                                        
                                  
                                            notes=S.notes,
                                            isActive=S.isActive,
                                            createDate = S.createDate,
                                            updateDate = S.updateDate,
                                            createUserId = S.createUserId,
                                            updateUserId=S.updateUserId,
                                        
                                            membershipId=S.membershipId,
                                            agentId=S.agentId,
                                            startDate=S.startDate,
                                            EndDate=S.EndDate,
                                         

                                            canDelete=true,
                                         }).ToList();
                    /*
                     * 
 

 agentMembershipsId
membershipId
agentId
startDate
EndDate
Amount
notes
createDate
updateDate
createUserId
updateUserId
isActive



                    */

           

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
        public IHttpActionResult GetByID(int agentMembershipsId)
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
                    var row = entity.agentMemberships
                   .Where(u => u.agentMembershipsId == agentMembershipsId)
                   .Select(S => new
                   {
                           S.agentMembershipsId,
                      
                           S.notes,
                           S.createDate,
                           S.updateDate,
                           S.createUserId,
                           S.updateUserId,
                           S.isActive,
                           S.membershipId,
                           S.agentId,
                           S.startDate,
                           S.EndDate,
                      


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
                agentMemberships newObject = JsonConvert.DeserializeObject<agentMemberships>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
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

                if (newObject.agentId == 0 || newObject.agentId == null)
                {
                    Nullable<int> id = null;
                    newObject.agentId = id;
                }
                if (newObject.membershipId == 0 || newObject.membershipId == null)
                {
                    Nullable<int> id = null;
                    newObject.membershipId = id;
                }

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var locationEntity = entity.Set<agentMemberships>();
                        if (newObject.agentMembershipsId == 0)
                        {
                            newObject.createDate = cc.AddOffsetTodate(DateTime.Now);;
                            newObject.updateDate = cc.AddOffsetTodate(DateTime.Now);;
                            newObject.updateUserId = newObject.createUserId;
                         
                      
                            locationEntity.Add(newObject);
                            entity.SaveChanges();
                            message = newObject.agentMembershipsId.ToString();
                        }
                        else
                        {
                            var tmpObject = entity.agentMemberships.Where(p => p.agentMembershipsId == newObject.agentMembershipsId).FirstOrDefault();

                            tmpObject.updateDate = cc.AddOffsetTodate(DateTime.Now);;
                            tmpObject.updateUserId = newObject.updateUserId;
                            tmpObject.notes  =newObject.notes;
                            tmpObject.isActive=newObject.isActive;

                            tmpObject.membershipId = newObject.membershipId;
                           tmpObject.agentId=newObject.agentId;
                           tmpObject.startDate=newObject.startDate;
                           tmpObject.EndDate=newObject.EndDate;
                           

                            entity.SaveChanges();

                            message = tmpObject.agentMembershipsId.ToString();
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
        public string Delete(int agentMembershipsId, int userId,bool final)
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
                            agentMemberships objectDelete = entity.agentMemberships.Find(agentMembershipsId);

                            entity.agentMemberships.Remove(objectDelete);
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
                            agentMemberships objectDelete = entity.agentMemberships.Find(agentMembershipsId);

                            objectDelete.isActive = 0;
                            objectDelete.updateUserId = userId;
                            objectDelete.updateDate = cc.AddOffsetTodate(DateTime.Now);;
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