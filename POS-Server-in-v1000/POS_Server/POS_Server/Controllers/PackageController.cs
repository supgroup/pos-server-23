using Newtonsoft.Json;
using POS_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using POS_Server.Models.VM;
using System.Security.Claims;
using System.Web;

using Newtonsoft.Json.Converters;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/Package")]
    public class PackageController : ApiController
    {
        CountriesController cc = new CountriesController();
        // GET api/<controller>
        [HttpPost]
        [Route("Get")]
        public string Get(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var List = (from S in entity.packages
                                    select new PackageModel()
                                    {
                                        packageId = S.packageId,
                                        parentIUId = S.parentIUId,
                                        childIUId = S.childIUId,
                                        quantity = S.quantity,
                                        isActive = S.isActive,
                                        notes = S.notes,
                                        createUserId = S.createUserId,
                                        updateUserId = S.updateUserId,
                                        createDate = S.createDate,
                                        updateDate = S.updateDate,
                                        canDelete = true,



                                    }).ToList();

                        var list = (from iu in entity.itemsUnits
                                    join it in entity.items on iu.itemId equals it.itemId
                                    join iuloc in entity.itemsLocations on iu.itemUnitId equals iuloc.itemUnitId

                                    where it.type == "p"
                                    select new
                                    {
                                        piuId = iu.itemUnitId,
                                        iuloc.itemsLocId,
                                        it.itemId,
                                        iu.unitId,

                                    }


                                    //  select()
                                    ).ToList();



                        return TokenManager.GenerateToken(List);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }


            //var re = Request;
            //var headers = re.Headers;
            //string token = "";
            //bool canDelete = false;

            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid) // APIKey is valid
            //{
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var List = (from S in  entity.packages                                         
            //                             select new PackageModel()
            //                             {
            //                           packageId=S.packageId,
            //                           parentIUId=S.parentIUId,
            //                           childIUId=S.childIUId,
            //                           quantity=S.quantity,
            //                           isActive=S.isActive,
            //                           notes=S.notes,
            //                           createUserId=S.createUserId,
            //                           updateUserId=S.updateUserId,
            //                           createDate=S.createDate,
            //                           updateDate=S.updateDate,
            //                           canDelete=true,



            //                             }).ToList();

            //        if (List == null)
            //            return NotFound();
            //        else
            //            return Ok(List);
            //    }
            //}
            ////else
            //return NotFound();
        }


        //
        //[HttpPost]
        //[Route("GetPackwithNames")]
        //public IHttpActionResult GetPackwithNames()
        //{
        //    var re = Request;
        //    var headers = re.Headers;
        //    string token = "";


        //    if (headers.Contains("APIKey"))
        //    {
        //        token = headers.GetValues("APIKey").First();
        //    }
        //    Validation validation = new Validation();
        //    bool valid = validation.CheckApiKey(token);

        //    if (valid) // APIKey is valid
        //    {
        //        using (incposdbEntities entity = new incposdbEntities())
        //        {
        //            var List = (from S in entity.packages
        //                        join CIU in entity.itemsUnits on S.childIUId equals CIU.itemUnitId
        //                        join PIU in entity.itemsUnits on S.parentIUId equals PIU.itemUnitId

        //                        select new PackageModel()
        //                        {
        //                            packageId = S.packageId,
        //                            parentIUId = S.parentIUId,
        //                            childIUId = S.childIUId,
        //                            quantity = S.quantity,
        //                            isActive = S.isActive,
        //                            notes = S.notes,
        //                            createUserId = S.createUserId,
        //                            updateUserId = S.updateUserId,
        //                            createDate = S.createDate,
        //                            updateDate = S.updateDate,
        //                            // parent
        //                            pitemId=PIU.itemId,
        //                            pitemName=PIU.items.name,
        //                            punitId=PIU.unitId,
        //                            punitName=PIU.units.name,
        //                            // child
        //                            citemId = CIU.itemId,
        //                            citemName = CIU.items.name,
        //                            cunitId = CIU.unitId,
        //                           cunitName = CIU.units.name,

        //                        }).ToList();

        //            if (List == null)
        //                return NotFound();
        //            else
        //                return Ok(List);
        //        }
        //    }
        //    //else
        //    return NotFound();
        //}


        public List<int> canNotUpdatePack()
        {

            List<int> listg = new List<int>();

            using (incposdbEntities entity = new incposdbEntities())
            {

                var list = (from iu in entity.itemsUnits
                            join it in entity.items on iu.itemId equals it.itemId
                            join iuloc in entity.itemsLocations on iu.itemUnitId equals iuloc.itemUnitId

                            where it.type == "p"
                            select new
                            {
                                //piuId = iu.itemUnitId,
                                //itemsLocId= iuloc.itemsLocId,
                                it.itemId,
                                iuloc.quantity,
                                //unitId=   iu.unitId,
                                //type=it.type,
                            }).ToList();

                listg = list.GroupBy(g => g.itemId).Where(q => q.Sum(s => s.quantity) > 0).Select(x =>
                          x.First().itemId).ToList();

            }
            return listg;
        }
        [HttpPost]
        [Route("GetPackages")]
        public string GetPackages(string token)
        {
            //public string Get(string token)
            //{

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                bool canDelete = false;
                try
                {
                    List<int> noUpdateList = new List<int>();

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        noUpdateList = canNotUpdatePack();
                        var List = (from I in entity.items
                                    join C in entity.categories on I.categoryId equals C.categoryId into JC

                                    from CC in JC.DefaultIfEmpty()
                                    where I.type == "p"

                                    select new ItemModel()
                                    {
                                        itemId = I.itemId,
                                        code = I.code,
                                        name = I.name,
                                        details = I.details,
                                        type = I.type,
                                        image = I.image,
                                        taxes = I.taxes,
                                        isActive = I.isActive,
                                        min = I.min,
                                        max = I.max,
                                        categoryId = I.categoryId,

                                        parentId = I.parentId,
                                        createDate = I.createDate,
                                        updateDate = I.updateDate,
                                        createUserId = I.createUserId,
                                        updateUserId = I.updateUserId,
                                        minUnitId = I.minUnitId,
                                        maxUnitId = I.maxUnitId,
                                        categoryName = CC.name,
                                        canDelete = true,
                                        canUpdate = noUpdateList.Contains(I.itemId) ? false : true,
                                        

                                    }).ToList();

                        foreach (var um in List)
                        {
                            canDelete = false;
                            if (um.isActive == 1)
                            {
                                var loc = entity.itemsLocations.Where(x => x.itemsUnits.items.itemId == um.itemId && x.quantity > 0).FirstOrDefault();
                                var inventory = entity.inventoryItemLocation.Where(x => x.itemsLocations.itemsUnits.items.itemId == um.itemId ).FirstOrDefault();
                                var inv = entity.itemsTransfer.Where(x => x.itemsUnits.items.itemId == um.itemId ).FirstOrDefault();
                                if (loc == null && inv == null && inventory == null)
                                    canDelete = true;
                            }
                            um.canDelete = canDelete;
                        }
                        return TokenManager.GenerateToken(List);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }


            //var re = Request;
            //var headers = re.Headers;
            //string token = "";
            //bool canDelete = false;

            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid) // APIKey is valid
            //{
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {

            //        //
            //        /*
            //        var templist = (from S in entity.packages
            //                        join IU in entity.itemsUnits on S.parentIUId equals IU.itemUnitId

            //                        select new
            //                        {
            //                            IU.itemId

            //                        }
            //                      ).ToList();
            //        List<int> listcontain = new List<int>();
            //        foreach (var row in templist)
            //        {
            //            listcontain.Add(int.Parse(row.itemId.ToString()));
            //        }
            //        */
            //        var List = (from I in entity.items
            //                    join C in entity.categories on I.categoryId equals C.categoryId into JC

            //                    from CC in JC.DefaultIfEmpty()
            //                    where I.type == "p"

            //                    select new ItemModel()
            //                    {
            //                        itemId = I.itemId,
            //                        code = I.code,
            //                        name = I.name,
            //                        details = I.details,
            //                        type = I.type,
            //                        image = I.image,
            //                        taxes = I.taxes,
            //                        isActive = I.isActive,
            //                        min = I.min,
            //                        max = I.max,
            //                        categoryId = I.categoryId,

            //                        parentId = I.parentId,
            //                        createDate = I.createDate,
            //                        updateDate = I.updateDate,
            //                        createUserId = I.createUserId,
            //                        updateUserId = I.updateUserId,
            //                        minUnitId = I.minUnitId,
            //                        maxUnitId = I.maxUnitId,
            //                        categoryName = CC.name,

            //                        canDelete = true,



            //                    }).ToList();



            //        if (List == null)
            //            return NotFound();
            //        else
            //            return Ok(List);
            //    }
            //}
            ////else
            //return NotFound();
        }
         [HttpPost]
        [Route("canUpdate")]
        public string canUpdate(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int itemUnitId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);
                    }


                }
                try
                {
               
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var list = (from iu in entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId)
                                    join iuloc in entity.itemsLocations.Where(x => x.quantity > 0) on iu.itemUnitId equals iuloc.itemUnitId
                                    select new
                                    {
                                        iuloc.quantity,
                                    }).FirstOrDefault();

                        if(list is null)
                        return TokenManager.GenerateToken("1");
                        else
                            return TokenManager.GenerateToken("0");

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("-1");
                }
            }


        }

        // GET api/<controller>
        [HttpPost]
        [Route("GetByID")]
        public string GetByID(string token)
        {
            // public string GetUsersByGroupId(string token)
            token = TokenManager.readToken(HttpContext.Current.Request); var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int packageId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "packageId")
                    {
                        packageId = int.Parse(c.Value);
                    }


                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var row = entity.packages
                       .Where(u => u.packageId == packageId)
                       .Select(S => new
                       {
                           S.packageId,
                           S.parentIUId,
                           S.childIUId,
                           S.quantity,
                           S.isActive,
                           S.notes,
                           S.createUserId,
                           S.updateUserId,
                           S.createDate,
                           S.updateDate,


                       })
                       .FirstOrDefault();

                        return TokenManager.GenerateToken(row);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }

            //var re = Request;
            //var headers = re.Headers;
            //string token = "";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid)
            //{
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var row = entity.packages
            //       .Where(u => u.packageId == packageId)
            //       .Select(S => new
            //       {
            //          S.packageId,
            //          S.parentIUId,
            //          S.childIUId,
            //          S.quantity,
            //            S.isActive,
            //         S.notes,
            //          S.createUserId,
            //           S.updateUserId,
            //          S.createDate,
            //         S.updateDate,


            //       })
            //       .FirstOrDefault();

            //        if (row == null)
            //            return NotFound();
            //        else
            //            return Ok(row);
            //    }
            //}
            //else
            //    return NotFound();
        }

        // add or update location
        [HttpPost]
        [Route("Savep")]
        public string Save(string token)
        {

            string message = "";



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string Object = "";
                packages newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<packages>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {


                    //  bondes tmpObject = null;

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
                    if (newObject.parentIUId == 0 || newObject.parentIUId == null)
                    {
                        Nullable<int> id = null;
                        newObject.parentIUId = id;
                    }
                    if (newObject.childIUId == 0 || newObject.childIUId == null)
                    {
                        Nullable<int> id = null;
                        newObject.childIUId = id;
                    }

                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var locationEntity = entity.Set<packages>();
                            if (newObject.packageId == 0)
                            {
                                newObject.createDate = cc.AddOffsetTodate(DateTime.Now);
                                newObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                newObject.updateUserId = newObject.createUserId;


                                locationEntity.Add(newObject);
                                entity.SaveChanges();
                                message = newObject.packageId.ToString();
                            }
                            else
                            {
                                var tmpObject = entity.packages.Where(p => p.packageId == newObject.packageId).FirstOrDefault();

                                tmpObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                tmpObject.updateUserId = newObject.updateUserId;

                                tmpObject.packageId = newObject.packageId;
                                tmpObject.parentIUId = newObject.parentIUId;
                                tmpObject.childIUId = newObject.childIUId;
                                tmpObject.quantity = newObject.quantity;

                                tmpObject.notes = newObject.notes;
                                tmpObject.isActive = newObject.isActive;
                                entity.SaveChanges();

                                message = tmpObject.packageId.ToString();
                            }
                            //  entity.SaveChanges();
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
                    return TokenManager.GenerateToken("0");
                }


            }

            //var re = Request;
            //var headers = re.Headers;
            //string token = "";
            //string message = "";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid)
            //{
            //    Object = Object.Replace("\\", string.Empty);
            //    Object = Object.Trim('"');
            //    packages newObject = JsonConvert.DeserializeObject<packages>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
            //    if (newObject.updateUserId == 0 || newObject.updateUserId == null)
            //    {
            //        Nullable<int> id = null;
            //        newObject.updateUserId = id;
            //    }
            //    if (newObject.createUserId == 0 || newObject.createUserId == null)
            //    {
            //        Nullable<int> id = null;
            //        newObject.createUserId = id;
            //    }
            //    if (newObject.parentIUId == 0 || newObject.parentIUId == null)
            //    {
            //        Nullable<int> id = null;
            //        newObject.parentIUId = id;
            //    }
            //    if (newObject.childIUId == 0 || newObject.childIUId == null)
            //    {
            //        Nullable<int> id = null;
            //        newObject.childIUId = id;
            //    }

            //    try
            //    {
            //        using (incposdbEntities entity = new incposdbEntities())
            //        {
            //            var locationEntity = entity.Set<packages>();
            //            if (newObject.packageId == 0)
            //            {
            //                newObject.createDate = cc.AddOffsetTodate(DateTime.Now);
            //                newObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
            //                newObject.updateUserId = newObject.createUserId;


            //                locationEntity.Add(newObject);
            //                entity.SaveChanges();
            //                message = newObject.packageId.ToString();
            //            }
            //            else
            //            {
            //                var tmpObject = entity.packages.Where(p => p.packageId == newObject.packageId).FirstOrDefault();

            //                tmpObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
            //                tmpObject.updateUserId = newObject.updateUserId;

            //                tmpObject.packageId = newObject.packageId;
            //                tmpObject.parentIUId = newObject.parentIUId;
            //                tmpObject.childIUId = newObject.childIUId;
            //                tmpObject.quantity = newObject.quantity;

            //                tmpObject.notes  =newObject.notes;
            //                tmpObject.isActive=newObject.isActive;
            //                entity.SaveChanges();

            //                message = tmpObject.packageId.ToString();
            //            }
            //          //  entity.SaveChanges();
            //        }
            //    }
            //    catch
            //    {
            //        message = "-1";
            //    }
            //}
            //return message;
        }

        [HttpPost]
        [Route("Delete")]
        public string Delete(string token)
        {
            string message = "";

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int packageId = 0;
                int userId = 0;
                bool final = false;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "packageId")
                    {
                        packageId = int.Parse(c.Value);
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
                            var itemUnitId = entity.itemsUnits.Where(x => x.itemId == packageId).FirstOrDefault().itemUnitId;

                            #region remove related locations
                            var pLoc = entity.itemsLocations.Where(x => x.itemUnitId == itemUnitId).ToList();
                           
                            if ( pLoc != null && pLoc.Count != 0)
                            {
                                entity.itemsLocations.RemoveRange(pLoc);
                               
                            }
                            #endregion

                            #region unrelate package and its items
                           
                            var childs = entity.packages.Where(x => x.parentIUId == itemUnitId).ToList();
                            if (childs != null && childs.Count != 0)
                            {
                                entity.packages.RemoveRange(childs);
                                
                            }
                            entity.SaveChanges();
                            #endregion
                            var objectDelete = entity.items.Find(packageId);

                            entity.items.Remove(objectDelete);
                            entity.SaveChanges().ToString();

                            message = "1";
                            return TokenManager.GenerateToken(message);
                        }
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }
                else
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var objectDelete = entity.items.Find(packageId);

                            objectDelete.isActive = 0;
                            objectDelete.updateUserId = userId;
                            objectDelete.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            message = entity.SaveChanges().ToString();

                            //  return message.ToString();
                            return TokenManager.GenerateToken(message);
                        }
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }
            }

            //var re = Request;
            //var headers = re.Headers;
            //string token = "";
            //int message = 0;
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}

            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);
            //if (valid)
            //{
            //    if (final)
            //    {
            //        try
            //        {
            //            using (incposdbEntities entity = new incposdbEntities())
            //            {
            //                packages objectDelete = entity.packages.Find(packageId);

            //                entity.packages.Remove(objectDelete);
            //                message = entity.SaveChanges();

            //                return message.ToString();
            //            }
            //        }
            //        catch
            //        {
            //            return "-1";
            //        }
            //    }
            //    else
            //    {
            //        try
            //        {
            //            using (incposdbEntities entity = new incposdbEntities())
            //            {
            //                packages objectDelete = entity.packages.Find(packageId);

            //                objectDelete.isActive = 0;
            //                objectDelete.updateUserId = userId;
            //                objectDelete.updateDate = cc.AddOffsetTodate(DateTime.Now);
            //                message = entity.SaveChanges();

            //                return message.ToString(); ;
            //            }
            //        }
            //        catch
            //        {
            //            return "-2";
            //        }
            //    }
            //}
            //else
            //    return "-3";
        }

        // GET api/<controller>
        [HttpPost]
        [Route("GetChildsByParentId")]
        public string GetChildsByParentId(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int parentIUId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "parentIUId")
                    {
                        parentIUId = int.Parse(c.Value);
                    }


                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var list = (from S in entity.packages
                                    join IU in entity.itemsUnits on S.childIUId equals IU.itemUnitId
                                    join I in entity.items on IU.itemId equals I.itemId
                                    where S.parentIUId == parentIUId
                                    select new PackageModel()
                                   {
                                       packageId =S.packageId,
                                       parentIUId = S.parentIUId,
                                       childIUId = S.childIUId,
                                       quantity = S.quantity,
                                       isActive = S.isActive,
                                       notes = S.notes,
                                       createUserId = S.createUserId,
                                       updateUserId = S.updateUserId,
                                       createDate = S.createDate,
                                       updateDate = S.updateDate,
                                       citemName = I.name,
                                       type = I.type,

                                   }).ToList();


                        return TokenManager.GenerateToken(list);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }

        }

        #region
        [HttpPost]
        [Route("UpdatePackByParentId")]
        public string UpdatePackByParentId(string token)
        {

            //newplist


            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                List<packages> newitofObj = new List<packages>();
                string newlist = "";

                int userId = 0;
                int parentIUId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        newlist = c.Value.Replace("\\", string.Empty);
                        newlist = newlist.Trim('"');
                        newitofObj = JsonConvert.DeserializeObject<List<packages>>(newlist, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "parentIUId")
                    {
                        parentIUId = int.Parse(c.Value);
                    }


                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                    int res = 0;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var iuoffer = entity.packages.Where(p => p.parentIUId == parentIUId);
                        if (iuoffer.Count() > 0)
                        {
                            entity.packages.RemoveRange(iuoffer);
                        }
                        if (newitofObj.Count() > 0)
                        {
                            foreach (packages newitofrow in newitofObj)
                            {
                                newitofrow.parentIUId = parentIUId;

                                if (newitofrow.createUserId == null || newitofrow.createUserId == 0)
                                {
                                    newitofrow.createDate = cc.AddOffsetTodate(DateTime.Now);
                                    newitofrow.updateDate = cc.AddOffsetTodate(DateTime.Now);

                                    newitofrow.createUserId = userId;
                                    newitofrow.updateUserId = userId;
                                }
                                else
                                {
                                    newitofrow.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                    newitofrow.updateUserId = userId;

                                }

                            }
                            entity.packages.AddRange(newitofObj);
                        }
                        res = entity.SaveChanges();

                        // return res;
                        return TokenManager.GenerateToken(res.ToString());
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }


            }

            //int userId = 0;
            //int parentIUId = 0;
            //var re = Request;
            //var headers = re.Headers;
            //int res = 0;
            //string token = "";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //if (headers.Contains("parentIUId"))
            //{
            //    parentIUId = Convert.ToInt32(headers.GetValues("parentIUId").First());
            //}
            //if (headers.Contains("userId"))
            //{
            //    userId = Convert.ToInt32(headers.GetValues("userId").First());
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);
            //newplist = newplist.Replace("\\", string.Empty);
            //newplist = newplist.Trim('"');
            //List<packages> newitofObj = JsonConvert.DeserializeObject<List<packages>>(newplist, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
            //if (valid)
            //{
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var iuoffer = entity.packages.Where(p => p.parentIUId == parentIUId);
            //        if (iuoffer.Count() > 0)
            //        {
            //            entity.packages.RemoveRange(iuoffer);
            //        }
            //        if (newitofObj.Count() > 0)
            //        {
            //            foreach (packages newitofrow in newitofObj)
            //            {
            //                newitofrow.parentIUId = parentIUId;

            //                if (newitofrow.createUserId == null || newitofrow.createUserId == 0)
            //                {
            //                    newitofrow.createDate = cc.AddOffsetTodate(DateTime.Now);
            //                    newitofrow.updateDate = cc.AddOffsetTodate(DateTime.Now);

            //                    newitofrow.createUserId = userId;
            //                    newitofrow.updateUserId = userId;
            //                }
            //                else
            //                {
            //                    newitofrow.updateDate = cc.AddOffsetTodate(DateTime.Now);
            //                    newitofrow.updateUserId = userId;

            //                }

            //            }
            //            entity.packages.AddRange(newitofObj);
            //        }
            //        res = entity.SaveChanges();

            //        return res;

            //    }

            //}
            //else
            //{
            //    return -1;
            //}

        }
        #endregion


        public List<PackageModel> GetChildsByParentId(int parentIUId)
        {
            List<PackageModel> list = new List<PackageModel>();

            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    list = (from S in entity.packages
                            join IU in entity.itemsUnits on S.childIUId equals IU.itemUnitId
                            join I in entity.items on IU.itemId equals I.itemId
                            where S.parentIUId == parentIUId
                            select new PackageModel()
                            {

                                packageId = S.packageId,
                                parentIUId = S.parentIUId,
                                childIUId = S.childIUId,
                                quantity = S.quantity,
                                isActive = S.isActive,
                                avgPurchasePrice = I.avgPurchasePrice,
                                citemId=I.itemId,
                                citemName = I.name,
                                type=I.type,
                                iuCost = IU.cost
                            }).ToList();

                    return list;

                }
            }
            catch
            {
                return list;
            }
        }

    }
}