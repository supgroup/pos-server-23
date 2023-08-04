using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Models;
using POS_Server.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web;

namespace POS_Server.Controllers
{

    [RoutePrefix("api/SliceUser")]
    public class SliceUserController : ApiController
    {
        CountriesController cc = new CountriesController();
        // GET api/<controller>
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
                    var List = (from S in entity.sliceUser
                                join B in entity.slices on S.sliceId equals B.sliceId into JB
                                join U in entity.users on S.userId equals U.userId into JU
                                from JBB in JB.DefaultIfEmpty()
                                from JUU in JU.DefaultIfEmpty()
                                select new SliceUserModel()
                                {
                                    sliceUserId = S.sliceUserId,
                                    sliceId = S.sliceId,
                                    userId = S.userId,
                                    isActive = S.isActive,
                                    notes = S.notes,
                                    createDate = S.createDate,
                                    updateDate = S.updateDate,
                                    createUserId = S.createUserId,
                                    updateUserId = S.updateUserId,

                                    // slice

                                    name = JBB.name,

                                    // user
                                    username = JUU.username,
                                    uname = JUU.name,
                                    lastname = JUU.lastname,
                                }).ToList();
                    return TokenManager.GenerateToken(List);
                }
            }
        }
        [HttpPost]
        [Route("GetSlicesByUserId")]
        public string GetSlicesByUserId(string token)
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
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var List = (from S in entity.sliceUser
                                    join B in entity.slices on S.sliceId equals B.sliceId into JB
                                    join U in entity.users on S.userId equals U.userId into JU
                                    from JBB in JB.DefaultIfEmpty()
                                    from JUU in JU.DefaultIfEmpty()
                                    where S.userId == userId
                                    select new SliceUserModel()
                                    {
                                        sliceUserId = S.sliceUserId,
                                        sliceId = S.sliceId,
                                        userId = S.userId,
                                        isActive = S.isActive,
                                        notes = S.notes,
                                        createDate = S.createDate,
                                        updateDate = S.updateDate,
                                        createUserId = S.createUserId,
                                        updateUserId = S.updateUserId,
                                        // slice
                                        name = JBB.name,
                                        // user
                                        username = JUU.username,
                                        uname = JUU.name,
                                        lastname = JUU.lastname,
                                    }).ToList();
                        return TokenManager.GenerateToken(List);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

     
        // GET api/<controller>
        [HttpPost]
        [Route("GetByID")]
        public string GetByID(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int sliceUserId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        sliceUserId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var row = entity.sliceUser
                   .Where(u => u.sliceUserId == sliceUserId)
                   .Select(S => new
                   {
                       S.sliceUserId,
                       S.sliceId,
                       S.userId,
                       S.isActive,
                       S.notes,
                       S.createDate,
                       S.updateDate,
                       S.createUserId,
                       S.updateUserId,

                   })
                   .FirstOrDefault();
                    return TokenManager.GenerateToken(row);
                }
            }
        }
        // add or update location//BranchesUsers/UpdateBranchByUserId"
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
                string Objects = "";
                sliceUser newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Objects = c.Value.Replace("\\", string.Empty);
                        Objects = Objects.Trim('"');
                        newObject = JsonConvert.DeserializeObject<sliceUser>(Objects, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
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

                if (newObject.sliceId == 0 || newObject.sliceId == null)
                {
                    Nullable<int> id = null;
                    newObject.sliceId = id;
                }
                if (newObject.userId == 0 || newObject.userId == null)
                {
                    Nullable<int> id = null;
                    newObject.userId = id;
                }

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var locationEntity = entity.Set<sliceUser>();
                        if (newObject.sliceUserId == 0)
                        {
                            newObject.createDate = cc.AddOffsetTodate(DateTime.Now);
                            newObject.updateDate = newObject.createDate;
                            newObject.updateUserId = newObject.createUserId;


                            locationEntity.Add(newObject);
                            entity.SaveChanges();
                            message = newObject.sliceUserId.ToString();
                        }
                        else
                        {
                            var tmpObject = entity.sliceUser.Where(p => p.sliceUserId == newObject.sliceUserId).FirstOrDefault();

                            tmpObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            tmpObject.updateUserId = newObject.updateUserId;
                            tmpObject.sliceUserId = newObject.sliceUserId;

                            tmpObject.sliceId = newObject.sliceId;
                            tmpObject.userId = newObject.userId;
                            tmpObject.isActive = newObject.isActive;
                            tmpObject.notes = newObject.notes;


                            entity.SaveChanges();

                            message = tmpObject.sliceUserId.ToString();
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
        //update slices list by userId
        [HttpPost]
        [Route("UpdateSliceByUserId")]
        public string UpdateSliceByUserId(string token)
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
                string sliceUserObject = "";
                List<sliceUser> newListObj = null;
                int userId = 0;
                int updateUserId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "newList")
                    {
                        sliceUserObject = c.Value.Replace("\\", string.Empty);
                        sliceUserObject = sliceUserObject.Trim('"');
                        newListObj = JsonConvert.DeserializeObject<List<sliceUser>>(sliceUserObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else
                  if (c.Type == "updateUserId")
                    {
                        updateUserId = int.Parse(c.Value);
                    }
                }
                List<sliceUser> items = null;
                // delete old  
                using (incposdbEntities entity = new incposdbEntities())
                {
                    items = entity.sliceUser.Where(x => x.userId == userId).ToList();
                    if (items != null)
                    {
                        entity.sliceUser.RemoveRange(items);
                        try
                        { entity.SaveChanges(); }
                        catch (Exception ex)
                        {
                            message = "-2";
                            return TokenManager.GenerateToken(message);
                        }
                    }

                }
                DateTime dnow = cc.AddOffsetTodate(DateTime.Now);
                using (incposdbEntities entity = new incposdbEntities())
                {
                    for (int i = 0; i < newListObj.Count; i++)
                    {
                        if (newListObj[i].updateUserId == 0 || newListObj[i].updateUserId == null)
                        {
                            Nullable<int> id = null;
                            newListObj[i].updateUserId = id;
                        }
                        if (newListObj[i].createUserId == 0 || newListObj[i].createUserId == null)
                        {
                            Nullable<int> id = null;
                            newListObj[i].createUserId = id;
                        }
                        if (newListObj[i].sliceId == 0 || newListObj[i].sliceId == null)
                        {
                            Nullable<int> id = null;
                            newListObj[i].sliceId = id;
                        }
                        if (newListObj[i].userId == 0 || newListObj[i].userId == null)
                        {
                            Nullable<int> id = null;
                            newListObj[i].userId = id;
                        }
                        var branchEntity = entity.Set<sliceUser>();

                        newListObj[i].createDate = dnow;
                        newListObj[i].updateDate = dnow;
                        newListObj[i].updateUserId = updateUserId;
                        newListObj[i].userId = userId;

                        branchEntity.Add(newListObj[i]);
                        entity.SaveChanges();
                    }
                    try
                    {
                        entity.SaveChanges().ToString();
                        message = "1";
                        return TokenManager.GenerateToken(message);
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
                int sliceUserId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        sliceUserId = int.Parse(c.Value);
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        sliceUser objectDelete = entity.sliceUser.Find(sliceUserId);

                        entity.sliceUser.Remove(objectDelete);
                        message = entity.SaveChanges().ToString();
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
}