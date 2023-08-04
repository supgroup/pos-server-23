using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Models;
using POS_Server.Models.VM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/slices")]
    public class SlicesController : ApiController
    {
        CountriesController cc = new CountriesController();
        // GET api/<controller> get all slices
        [HttpPost]
        [Route("GetAll")]
        public string GetAll(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            Boolean canDelete = false;
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemList = entity.slices

                   .Select(S => new SliceModel()
                   {
                       sliceId = S.sliceId,
                       name = S.name,
                       notes = S.notes,
                       createDate = S.createDate,
                       updateDate = S.updateDate,
                       createUserId = S.createUserId,
                       updateUserId = S.updateUserId,
                       isActive = S.isActive,
                   })
                   .ToList();

                    // can delet or not
                    if (itemList.Count > 0)
                    {
                        foreach (SliceModel item in itemList)
                        {
                            canDelete = false;
                            int Id = (int)item.sliceId;
                            var Pricesitem = entity.Prices.Where(x => x.sliceId == Id).Select(x => new { x.sliceId }).FirstOrDefault();
                            var slicuser = entity.sliceUser.Where(x => x.sliceId == Id).Select(x => new { x.sliceId }).FirstOrDefault();
                            if ((Pricesitem is null) && (slicuser is null))
                                canDelete = true;
                            item.canDelete = canDelete;
                        }
                    }
                    return TokenManager.GenerateToken(itemList);
                }
            }
        }
        // GET api/<controller>  Get card By ID 
        [HttpPost]
        [Route("GetById")]
        public string GetById(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int Id = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        Id = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var item = entity.slices
                   .Where(S => S.sliceId == Id)
                   .Select(S => new
                   {
                       S.sliceId,
                       S.name,
                       S.notes,
                       S.createDate,
                       S.updateDate,
                       S.createUserId,
                       S.updateUserId,
                       S.isActive,

                   })
                   .FirstOrDefault();
                    return TokenManager.GenerateToken(item);
                }
            }
        }

        [HttpPost]
        [Route("Save")]
        public string Save(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string itemObject = "";
                slices newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        itemObject = c.Value.Replace("\\", string.Empty);
                        itemObject = itemObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<slices>(itemObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        slices tmpObject = new slices();
                        var sliceEntity = entity.Set<slices>();
                        DateTime datenow = cc.AddOffsetTodate(DateTime.Now);
                        if (newObject.sliceId == 0)
                        {

                            newObject.createDate = datenow;
                            newObject.updateDate = datenow;
                            newObject.updateUserId = newObject.createUserId;
                            tmpObject = sliceEntity.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject.sliceId.ToString();

                        }
                        else
                        {

                            tmpObject = entity.slices.Where(p => p.sliceId == newObject.sliceId).FirstOrDefault();
                            tmpObject.sliceId = newObject.sliceId;
                            tmpObject.name = newObject.name;
                            tmpObject.notes = newObject.notes;
                            //tmpObject.createDate = newObject.createDate;

                            //   tmpObject.createUserId = newObject.createUserId;
                            tmpObject.updateUserId = newObject.updateUserId;
                            tmpObject.isActive = newObject.isActive;

                            tmpObject.updateDate = datenow;// server current date;

                            entity.SaveChanges();
                            message = tmpObject.sliceId.ToString();
                        }

                    }
                    return TokenManager.GenerateToken(message);
                }

                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }
            }
        }
        [HttpPost]
        [Route("Delete")]
        public string Delete(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int sliceId = 0;
                int userId = 0;
                bool final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        sliceId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "final")
                    {
                        final = bool.Parse(c.Value);
                    }
                }
                if (final)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            //check if there are another active slice in table 
                            var slicList = entity.slices.Where(s => s.sliceId != sliceId && s.isActive == true);
                            if (slicList != null && slicList.Count() > 0)
                            {
                                slices Obj = entity.slices.Find(sliceId);
                                entity.slices.Remove(Obj);
                                message = entity.SaveChanges().ToString();
                            }
                            else
                            {
                                message = "-1";
                            }
                            return TokenManager.GenerateToken(message);
                        }
                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                }
                else
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var slicList = entity.slices.Where(s => s.sliceId != sliceId && s.isActive == true);
                            if (slicList != null && slicList.Count() > 0)
                            {
                                slices Obj = entity.slices.Find(sliceId);
                                Obj.isActive = false;
                                Obj.updateUserId = userId;
                                Obj.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                message = entity.SaveChanges().ToString();
                            }
                            else
                            {
                                message = "-1";
                            }

                            return TokenManager.GenerateToken(message);
                        }
                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                }
            }
        }

        [HttpPost]
        [Route("GetAllowedSlicesByUserId")]
        public string GetAllowedSlicesByUserId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                List<SliceModel> List = new List<SliceModel>();
                try{ 
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var usr = entity.users.Where(x => x.userId == userId).FirstOrDefault();

                    if (usr.isAdmin == true)
                    {
                        List = (from B in entity.slices 
                                where B.isActive == true
                                select new SliceModel()
                                {
                                    // sliceUserId = S.sliceUserId,
                                    sliceId = B.sliceId,
                                    //   userId = S.userId,
                                    isActive = B.isActive,
                                    notes = B.notes,
                                    createDate = B.createDate,
                                    updateDate = B.updateDate,
                                    createUserId = B.createUserId,
                                    updateUserId = B.updateUserId,

                                    // slice

                                    name = B.name,

                                    // user

                                }).ToList();
                    }
                    else
                    {
                        List = (from S in entity.sliceUser
                                join B in entity.slices on S.sliceId equals B.sliceId into JB
                                join U in entity.users on S.userId equals U.userId into JU
                                from JBB in JB.DefaultIfEmpty()
                                from JUU in JU.DefaultIfEmpty()
                                where S.userId == userId && JBB.isActive == true
                                select new SliceModel()
                                {
                                    sliceId = JBB.sliceId,
                                    name = JBB.name,
                                    //   userId = S.userId,
                                    isActive = JBB.isActive,
                                    notes = JBB.notes,
                                    createDate = JBB.createDate,
                                    updateDate = JBB.updateDate,
                                    createUserId = JBB.createUserId,
                                    updateUserId = JBB.updateUserId,
                                }).ToList();
                    }

                    return TokenManager.GenerateToken(List);
                }
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
    }
}