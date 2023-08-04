using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Models;
using POS_Server.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/serials")]
    public class serialsController : ApiController
    {
        CountriesController cc = new CountriesController();
        // GET api/<controller>

        [HttpPost]
        [Route("GetAll")]
        public string GetAll(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            bool canDelete = false;
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemList = entity.serials

                   .Select(S => new SerialStsModel()
                   {
                       serialId = S.serialId,
                       itemsTransId = S.itemsTransId,
                       itemUnitId = S.itemUnitId,
                       serialNum = S.serialNum,
                       isActive = S.isActive,
                       createDate = S.createDate,
                       updateDate = S.updateDate,
                       createUserId = S.createUserId,
                       updateUserId = S.updateUserId,
                       isSold = S.isSold,
                       branchId = S.branchId,
                       itemName = S.itemsUnits.items.name,
                       unitName = S.itemsUnits.units.name,
                       itemId = S.itemsUnits.itemId,
                       unitId = S.itemsUnits.unitId,
                   }).ToList();

                    // can delet or not
                    //if (itemList.Count > 0)
                    //{
                    //    foreach (SerialModel item in itemList)
                    //    {
                    //        canDelete = false;

                    //        int Id = (int)item.serialId;
                    //        var Pricesitem = entity.itemsTransfer.Where(x => (int)x.serialId == Id).Select(x => new { x.serialId }).FirstOrDefault();

                    //        if ((Pricesitem is null))
                    //            canDelete = true;

                    //        item.canDelete = canDelete;
                    //    }
                    //}
                    return TokenManager.GenerateToken(itemList);
                }
            }
        }

        public List<SerialModel> Get(int itemTransferId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var serialsList = entity.serials
                     .Where(S => S.itemsTransId == itemTransferId)
                     .Select(S => new SerialModel()
                     {
                         serialId = S.serialId,
                         itemsTransId = S.itemsTransId,
                         itemUnitId = S.itemUnitId,
                         createDate = S.createDate,
                         isActive = S.isActive,
                         serialNum = S.serialNum,
                         isSold = S.isSold,
                         branchId = S.branchId,
                         createUserId = S.createUserId
                     }).ToList();
                return serialsList;
            }

        }
        // add or update serial
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
                string serialObject = "";
                serials newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        serialObject = c.Value.Replace("\\", string.Empty);
                        serialObject = serialObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<serials>(serialObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        serials tmpSerial = new serials();
                        var serialEntity = entity.Set<serials>();
                        DateTime datenow = cc.AddOffsetTodate(DateTime.Now);
                        if (newObject.serialId == 0)
                        {
                            decimal r = CanSaveSerial(newObject, "s");
                            if (r > 0)
                            {


                                newObject.createDate = datenow;
                                newObject.updateDate = datenow;
                                newObject.updateUserId = newObject.createUserId;
                                tmpSerial = serialEntity.Add(newObject);

                                entity.SaveChanges();
                                message = tmpSerial.serialId.ToString();
                                return TokenManager.GenerateToken(message);
                            }
                            else
                            {
                                return TokenManager.GenerateToken(r.ToString());
                            }
                        }
                        else
                        {
                            // update
                            tmpSerial = entity.serials.Where(p => p.serialId == newObject.serialId).FirstOrDefault();

                            decimal r = 0;
                            if (tmpSerial.isActive == 0 && newObject.isActive == 1)
                            {
                                //active changed
                                r = CanSaveSerial(newObject, "u");
                                if (r > 0)
                                {
                                    tmpSerial.serialNum = newObject.serialNum;
                                    tmpSerial.isActive = newObject.isActive;
                                    tmpSerial.isSold = newObject.isSold;
                                    tmpSerial.branchId = newObject.branchId;
                                    tmpSerial.updateDate = datenow;
                                    tmpSerial.updateUserId = newObject.updateUserId;

                                    tmpSerial.itemsTransId = newObject.itemsTransId;
                                    tmpSerial.itemUnitId = newObject.itemUnitId;
                                    tmpSerial.isSold = newObject.isSold;
                                    tmpSerial.branchId = newObject.branchId;
                                    entity.SaveChanges();
                                    message = tmpSerial.serialId.ToString();
                                }
                                else
                                {
                                    return TokenManager.GenerateToken(r.ToString());
                                }
                            }
                            else
                            {
                              bool   rep = CheckNotRepeat(newObject);
                                if (rep)
                                {
                                    tmpSerial.serialNum = newObject.serialNum;
                                    tmpSerial.isActive = newObject.isActive;
                                    tmpSerial.isSold = newObject.isSold;
                                    tmpSerial.branchId = newObject.branchId;
                                    tmpSerial.updateDate = datenow;
                                    tmpSerial.updateUserId = newObject.updateUserId;

                                    tmpSerial.itemsTransId = newObject.itemsTransId;
                                    tmpSerial.itemUnitId = newObject.itemUnitId;
                                    tmpSerial.isSold = newObject.isSold;
                                    tmpSerial.branchId = newObject.branchId;
                                    entity.SaveChanges();
                                    message = tmpSerial.serialId.ToString();
                                }
                                else
                                {
                               
                                    return TokenManager.GenerateToken("-2.2");
                                }
                          
                            }



                            return TokenManager.GenerateToken(message);
                        }
                    }
                }
                catch
                {
                    message = "0";
                }
            }
            return message;
        }


        //
        public decimal Save(serials newObject)
        {
            decimal message = 0;
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    serials tmpSerial = new serials();
                    var serialEntity = entity.Set<serials>();
                    DateTime datenow = cc.AddOffsetTodate(DateTime.Now);
                    if (newObject.serialId == 0)
                    {
                        decimal r = CanSaveSerial(newObject, "s");
                        if (r > 0)
                        {
                            newObject.createDate = datenow;
                            newObject.updateDate = datenow;
                            newObject.updateUserId = newObject.createUserId;
                            tmpSerial = serialEntity.Add(newObject);
                            entity.SaveChanges();
                            message = tmpSerial.serialId;
                            return message;
                        }
                        else
                        {
                            return r;
                        }
                    }
                    else
                    {
                        // update
                        tmpSerial = entity.serials.Where(p => p.serialId == newObject.serialId).FirstOrDefault();
                        decimal r = 0;
                        if (tmpSerial.isActive == 0 && newObject.isActive == 1)
                        {
                            //active changed
                            r = CanSaveSerial(newObject, "u");
                            if (r > 0)
                            {
                                tmpSerial.serialNum = newObject.serialNum;
                                tmpSerial.isActive = newObject.isActive;
                                tmpSerial.isSold = newObject.isSold;
                                tmpSerial.branchId = newObject.branchId;
                                tmpSerial.updateDate = datenow;
                                tmpSerial.updateUserId = newObject.updateUserId;

                                tmpSerial.itemsTransId = newObject.itemsTransId;
                                tmpSerial.itemUnitId = newObject.itemUnitId;
                                tmpSerial.isSold = newObject.isSold;
                                tmpSerial.branchId = newObject.branchId;
                                entity.SaveChanges();
                                message = tmpSerial.serialId;
                            }
                            else
                            {
                                return r;
                            }
                        }
                        else
                        {
                            bool rep = CheckNotRepeat(newObject);
                            if (rep)
                            {
                                tmpSerial.serialNum = newObject.serialNum;
                                tmpSerial.isActive = newObject.isActive;
                                tmpSerial.isSold = newObject.isSold;
                                tmpSerial.branchId = newObject.branchId;
                                tmpSerial.updateDate = datenow;
                                tmpSerial.updateUserId = newObject.updateUserId;

                                tmpSerial.itemsTransId = newObject.itemsTransId;
                                tmpSerial.itemUnitId = newObject.itemUnitId;
                                tmpSerial.isSold = newObject.isSold;
                                tmpSerial.branchId = newObject.branchId;
                                entity.SaveChanges();
                                message = tmpSerial.serialId;
                            }
                            else
                            {
                                message = (decimal)-2.2;
                            }
                        }
                        return message;
                    }
                }
            }
            catch
            {
                message = 0;
            }

            return message;
        }
        [HttpPost]
        [Route("GetSerialsByIsSold")]
        public string GetSerialsByIsSold(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                bool isSold = false;
                int itemUnitId = 0;
                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "isSold")
                    {
                        isSold = bool.Parse(c.Value);
                    }
                    else if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var serialsList = entity.serials
                        .Where(S => S.isSold == isSold && S.itemUnitId == itemUnitId && S.branchId == branchId && S.isActive == 1)
                        .Select(S => new SerialModel()
                        {
                            itemUnitId = S.itemUnitId,
                            serialNum = S.serialNum,
                            createDate = null,
                            updateDate = null,
                        }).Distinct().ToList();
                        return TokenManager.GenerateToken(serialsList);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        [HttpPost]
        [Route("GetMainInvoiceSerials")]
        public string GetMainInvoiceSerials(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainInvoiceId = 0;
                int itemUnitId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainInvoiceId")
                    {
                        mainInvoiceId = int.Parse(c.Value);
                    }
                    else if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var lastMainInvId = entity.invoices.Where(x => x.invNumber == entity.invoices.Where(i => i.invoiceId == mainInvoiceId).FirstOrDefault().invNumber).Max(x => x.invoiceId);

                        var serialsList = entity.serials
                        .Where(S => S.itemsTransfer.invoices.invoiceId == lastMainInvId && S.itemUnitId == itemUnitId && S.isActive == 1)
                        .Select(S => new SerialModel()
                        {
                            itemUnitId = S.itemUnitId,
                            serialNum = S.serialNum,
                            createDate = null,
                            updateDate = null,
                        }).Distinct().ToList();
                        return TokenManager.GenerateToken(serialsList);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        [HttpPost]
        [Route("SerialsCanAdded")]
        public string SerialsCanAdded(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string jsonObj = "";
                List<SerialModel> serials = new List<SerialModel>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "serials")
                    {
                        jsonObj = c.Value.Replace("\\", string.Empty);
                        jsonObj = jsonObj.Trim('"');
                        serials = JsonConvert.DeserializeObject<List<SerialModel>>(jsonObj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }

                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        foreach (var sr in serials)
                        {
                            if (sr.isManual == true)
                            {
                                var serialsList = entity.serials
                                .Where(S => S.serialNum == sr.serialNum
                                        && S.itemUnitId == sr.itemUnitId
                                        && S.isActive == 1)
                                .FirstOrDefault();

                                if (serialsList != null)
                                    return TokenManager.GenerateToken("0");
                            }

                        }
                        return TokenManager.GenerateToken("1");

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("-1");
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
                int Id = 0;
                int userId = 0;
                bool final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        Id = int.Parse(c.Value);
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
                            serials Obj = entity.serials.Find(Id);
                            entity.serials.Remove(Obj);
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
                else
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            serials Obj = entity.serials.Find(Id);

                            Obj.isActive = 0;
                            Obj.updateUserId = userId;
                            Obj.updateDate = cc.AddOffsetTodate(DateTime.Now);
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

        [HttpPost]
        [Route("GetIUbyBranch")]
        public string GetIUbyBranch(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<SerialStsModel> invListm = new List<SerialStsModel>();
                    List<SerialStsModel> serialList = new List<SerialStsModel>();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        invListm = (from L in entity.locations
                                    join IUL in entity.itemsLocations on L.locationId equals IUL.locationId
                                    join IU in entity.itemsUnits on IUL.itemUnitId equals IU.itemUnitId
                                    join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                    join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                    //join S in entity.sections on L.sectionId equals S.sectionId into JS
                                    join B in entity.branches on L.branchId equals B.branchId
                                    //    from SRL in entity.serials.Where(x=>x.branchId==B.branchId)

                                    //from JSS in JS.DefaultIfEmpty()
                                    ////  from SRL in JSR.DefaultIfEmpty()//
                                    where (B.branchId == branchId && ITEM.type == "sn")
                                    select new SerialStsModel
                                    {

                                        // item unit     
                                        itemName = ITEM.name,
                                        //ITEM.min,
                                        //ITEM.max,
                                        //ITEM.minUnitId,
                                        //ITEM.maxUnitId,
                                        itemType = ITEM.type,
                                        //minUnitName = entity.units.Where(x => x.unitId == ITEM.minUnitId).FirstOrDefault().name,
                                        //maxUnitName = entity.units.Where(x => x.unitId == ITEM.maxUnitId).FirstOrDefault().name,
                                        unitName = UNIT.name,
                                        itemUnitId = IU.itemUnitId,

                                        itemId = IU.itemId,
                                        unitId = IU.unitId,


                                        branchId = L.branchId,
                                        branchName = B.name,
                                        branchType = B.type,

                                        //itemslocations

                                        itemsLocId = IUL.itemsLocId,
                                        locationId = IUL.locationId,
                                        quantity = IUL.quantity,

                                        //startDate = IUL.startDate,
                                        //endDate = IUL.endDate,

                                        //IULnote = IUL.note,
                                        //IU.storageCostId,

                                        //storageCostName = IU.storageCostId != null ? entity.storageCost.Where(X => X.storageCostId == IU.storageCostId).FirstOrDefault().name : "",


                                        //IUL.updateDate,

                                        // Location
                                        //L.x,
                                        //L.y,
                                        //L.z,

                                        //LocisActive = L.isActive,
                                        //   sectionId = L.sectionId != null ? (int)L.sectionId : 0,
                                        //Locnote = L.note,
                                        //L.branchId,
                                        //LocisFreeZone = L.isFreeZone,


                                        // section

                                        //Secname = JSS.name,
                                        //SecisActive = JSS.isActive,
                                        //Secnote = JSS.note,
                                        //SecisFreeZone = JSS.isFreeZone,


                                        //username

                                        //  I.invoiceId,
                                        //    JBB.name

                                        //serialId = SRL.serialId,
                                        //serialNum = SRL.serialNum,
                                        //isActive = SRL.isActive,
                                        //isSold = SRL.isSold,
                                        //sitemUnitId =SRL.itemUnitId,
                                        count = entity.serials.Where(C => C.isSold == false && C.isActive == 1 && C.branchId == B.branchId && C.itemUnitId == IU.itemUnitId).Count(),

                                    }).ToList();
                    }
                    serialList = invListm.GroupBy(S => S.itemUnitId).Select(X => new SerialStsModel
                    {
                        branchId = X.FirstOrDefault().branchId,
                        branchName = X.FirstOrDefault().branchName,
                        itemUnitId = X.FirstOrDefault().itemUnitId,
                        itemName = X.FirstOrDefault().itemName,
                        unitName = X.FirstOrDefault().unitName,
                        itemId = X.FirstOrDefault().itemId,
                        unitId = X.FirstOrDefault().unitId,
                        //  quantity = X.Sum(q => q.quantity),
                        count = X.FirstOrDefault().count,

                    }).ToList();
                    ItemsLocationsController ilCnrlr = new ItemsLocationsController();

                    foreach (SerialStsModel row in serialList)
                    {
                        row.quantity = ilCnrlr.getAllItemAmount((int)row.itemUnitId, branchId);

                    }
                    return TokenManager.GenerateToken(serialList);
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken(ex.ToString());
                }

            }


        }

        [HttpPost]
        [Route("GetIUBranchWithCount")]
        public string GetIUBranchWithCount(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<ItemUnitModel> invListm = new List<ItemUnitModel>();
                    List<ItemUnitModel> serialList = new List<ItemUnitModel>();
                    ItemsLocationsController ilCnrlr = new ItemsLocationsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        invListm = (from IU in entity.itemsUnits
                                        //on IUL.itemUnitId equals IU.itemUnitId

                                        //join IUL in entity.itemsLocations on IU.itemUnitId equals IUL.itemUnitId
                                        //join L in entity.locations on IUL.locationId equals L.locationId
                                    join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                    join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                    //join S in entity.sections on L.sectionId equals S.sectionId into JS
                                    //   join B in entity.branches on L.branchId equals B.branchId
                                    //    from SRL in entity.serials.Where(x=>x.branchId==B.branchId)

                                    //from JSS in JS.DefaultIfEmpty()
                                    ////  from SRL in JSR.DefaultIfEmpty()//
                                    //  where (B.branchId == branchId )
                                    select new ItemUnitModel
                                    {

                                        // item unit     
                                        itemName = ITEM.name,
                                        //ITEM.min,
                                        //ITEM.max,
                                        //ITEM.minUnitId,
                                        //ITEM.maxUnitId,
                                        itemType = ITEM.type,
                                        //minUnitName = entity.units.Where(x => x.unitId == ITEM.minUnitId).FirstOrDefault().name,
                                        //maxUnitName = entity.units.Where(x => x.unitId == ITEM.maxUnitId).FirstOrDefault().name,
                                        unitName = UNIT.name,
                                        itemUnitId = IU.itemUnitId,

                                        itemId = IU.itemId,
                                        unitId = IU.unitId,


                                        //branchId = L.branchId,
                                        //branchName = B.name,
                                        //branchType = B.type,

                                        //itemslocations

                                        //itemsLocId = IUL.itemsLocId,
                                        //locationId = IUL.locationId,
                                        // quantity = IUL.quantity,

                                        //startDate = IUL.startDate,
                                        //endDate = IUL.endDate,

                                        //IULnote = IUL.note,
                                        //IU.storageCostId,

                                        //storageCostName = IU.storageCostId != null ? entity.storageCost.Where(X => X.storageCostId == IU.storageCostId).FirstOrDefault().name : "",


                                        //IUL.updateDate,

                                        // Location
                                        //L.x,
                                        //L.y,
                                        //L.z,

                                        //LocisActive = L.isActive,
                                        //   sectionId = L.sectionId != null ? (int)L.sectionId : 0,
                                        //Locnote = L.note,
                                        //L.branchId,
                                        //LocisFreeZone = L.isFreeZone,


                                        // section

                                        //Secname = JSS.name,
                                        //SecisActive = JSS.isActive,
                                        //Secnote = JSS.note,
                                        //SecisFreeZone = JSS.isFreeZone,


                                        //username

                                        //  I.invoiceId,
                                        //    JBB.name

                                        //serialId = SRL.serialId,
                                        //serialNum = SRL.serialNum,
                                        //isActive = SRL.isActive,
                                        //isSold = SRL.isSold,
                                        //sitemUnitId =SRL.itemUnitId,
                                        //   serialsCount = entity.serials.Where(C => C.isSold == false && C.isActive == 1 && C.branchId == B.branchId && C.itemUnitId == IU.itemUnitId).Count(),

                                    }).ToList();
                        //    branches branch=entity.branches
                        serialList = invListm.GroupBy(S => S.itemUnitId).Select(X => new ItemUnitModel
                        {

                            branchId = X.FirstOrDefault().branchId,
                            //  branchName = X.FirstOrDefault().branchName,
                            itemUnitId = X.FirstOrDefault().itemUnitId,
                            itemName = X.FirstOrDefault().itemName,
                            unitName = X.FirstOrDefault().unitName,
                            itemId = X.FirstOrDefault().itemId,
                            unitId = X.FirstOrDefault().unitId,
                            itemType = X.FirstOrDefault().itemType,
                            //  quantity = X.Sum(q => q.quantity),
                            //  serialsCount = X.FirstOrDefault().serialsCount,
                            //PropertiesCount= entity.storeProperties.Where(s=>s.branchId==branchId
                            //&& s.itemUnitId == X.FirstOrDefault().itemUnitId && s.isSold==false           
                            //).ToList().Sum(s=>s.count),
                            //serialsCount = entity.serials.Where(C => C.isSold == false && C.isActive == 1 && C.branchId == branchId && C.itemUnitId == X.FirstOrDefault().itemUnitId).ToList().Count(),

                        }).ToList();


                        foreach (ItemUnitModel row in serialList)
                        {
                            row.quantity = ilCnrlr.getAllItemAmount((int)row.itemUnitId, branchId);
                            row.PropertiesCount = entity.storeProperties.Where(s => s.branchId == branchId
                              && s.itemUnitId == row.itemUnitId && s.isSold == false
                              ).ToList().Sum(s => s.count);
                            row.serialsCount = entity.serials.Where(C => C.isSold == false && C.isActive == 1 && C.branchId == branchId && C.itemUnitId == row.itemUnitId).ToList().Count();

                        }

                    }


                    return TokenManager.GenerateToken(serialList);
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken(ex.ToString());
                }

            }


        }

        [HttpPost]
        [Route("GetSerialsAvailable")]
        public string GetSerialsAvailable(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                int branchId = 0;
                int itemUnitId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);
                    }
                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<SerialStsModel> invListm = new List<SerialStsModel>();

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        invListm = (from S in entity.serials
                                    join B in entity.branches on S.branchId equals B.branchId
                                    // join IT in entity.itemsTransfer on S.itemsTransId equals IT.itemsTransId
                                    //  join I in entity.invoices on IT.invoiceId equals I.invoiceId
                                    join IU in entity.itemsUnits on S.itemUnitId equals IU.itemUnitId
                                    join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                    join UNIT in entity.units on IU.unitId equals UNIT.unitId

                                    where (B.branchId == branchId && ITEM.type != "sr"
                                    && S.itemUnitId == itemUnitId && S.isSold == false && S.isActive == 1)
                                    select new SerialStsModel
                                    {
                                        serialId = S.serialId,
                                        itemsTransId = S.itemsTransId,
                                        itemUnitId = S.itemUnitId,
                                        serialNum = S.serialNum,
                                        isActive = S.isActive,
                                        createDate = S.createDate,
                                        updateDate = S.updateDate,
                                        createUserId = S.createUserId,
                                        updateUserId = S.updateUserId,
                                        isSold = S.isSold,
                                        branchId = S.branchId,

                                        itemName = ITEM.name,
                                        unitName = UNIT.name,
                                        branchName = B.name,
                                        //invoiceId = I.invoiceId,
                                        //invNumber = I.invNumber,
                                    }).ToList();
                    }
                    return TokenManager.GenerateToken(invListm);
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        public bool CheckNotRepeat(serials serial)
        {
            bool res = false;
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var serialsList = entity.serials
                    .Where(S => S.serialNum == serial.serialNum
                            && S.itemUnitId == serial.itemUnitId
                            && S.serialId != serial.serialId //4 update
                          && S.isActive == 1
                         ).FirstOrDefault();

                    if (serialsList != null)
                    {
                        //repeated
                        return false;
                    }

                    else
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        public bool CheckCountAvailable(int branchId, int itemUnitId, string op)
        {
            bool res = false;
            try
            {
                List<SerialStsModel> invListm = new List<SerialStsModel>();
                List<SerialStsModel> serialList = new List<SerialStsModel>();
                int count = 0;
                int quantity = 0;
                using (incposdbEntities entity = new incposdbEntities())
                {
                    count = entity.serials.Where(C => C.isSold == false && C.isActive == 1 && C.branchId == branchId && C.itemUnitId == itemUnitId).ToList().Count();

                    //invListm = (from L in entity.locations
                    //            join IUL in entity.itemsLocations on L.locationId equals IUL.locationId
                    //            join IU in entity.itemsUnits on IUL.itemUnitId equals IU.itemUnitId
                    //            join ITEM in entity.items on IU.itemId equals ITEM.itemId
                    //            join UNIT in entity.units on IU.unitId equals UNIT.unitId
                    //            join B in entity.branches on L.branchId equals B.branchId
                    //            where (B.branchId == branchId && ITEM.type != "sr" && IU.itemUnitId == itemUnitId)
                    //            select new SerialStsModel
                    //            {
                    //                //itemName = ITEM.name,

                    //                //itemType = ITEM.type,
                    //                //unitName = UNIT.name,
                    //                itemUnitId = IU.itemUnitId,

                    //                //itemId = IU.itemId,
                    //                //unitId = IU.unitId,


                    //                //branchId = L.branchId,
                    //                //branchName = B.name,
                    //                //branchType = B.type,
                    //                //itemsLocId = IUL.itemsLocId,
                    //                //locationId = IUL.locationId,
                    //                quantity = IUL.quantity,
                    //                count = entity.serials.Where(C => C.isSold == false && C.isActive == 1 && C.branchId == B.branchId && C.itemUnitId == IU.itemUnitId).Count(),
                    //            }).ToList();
                }
                //serialList = invListm.GroupBy(S => S.itemUnitId).Select(X => new SerialStsModel
                //{
                //    //branchId = X.FirstOrDefault().branchId,
                //    //branchName = X.FirstOrDefault().branchName,
                //    //itemUnitId = X.FirstOrDefault().itemUnitId,
                //    //itemName = X.FirstOrDefault().itemName,
                //    //unitName = X.FirstOrDefault().unitName,
                //    //itemId = X.FirstOrDefault().itemId,
                //    //unitId = X.FirstOrDefault().unitId,
                //    quantity = X.Sum(q => q.quantity),
                //    count = X.FirstOrDefault().count,
                //}).ToList();

                ItemsLocationsController ilCnrlr = new ItemsLocationsController();
                quantity = ilCnrlr.getAllItemAmount(itemUnitId, branchId);
                // SerialStsModel serialobj = new SerialStsModel();
                //if (count > 0)
                //{
                //    serialobj = serialList.FirstOrDefault();

                //}

                if (op == "s")
                {
                    //save
                    if (quantity > count)
                    {
                        res = true;
                    }
                    else
                    {
                        res = false;
                    }
                }
                else
                {
                    //update
                    if (quantity >= count)
                    {
                        res = true;
                    }
                    else
                    {
                        res = false;
                    }

                }


                return res;
            }
            catch (Exception ex)
            {
                return false;
                //  ex.ToString() ;
            }
        }

        public decimal CanSaveSerial(serials serial, string op)
        {
            try
            {
                if (CheckNotRepeat(serial))
                {
                    if (CheckCountAvailable((int)serial.branchId, (int)serial.itemUnitId, op))
                    {
                        //can save
                        return 1;
                    }
                    else
                    {
                        //no quantity
                        return (decimal)-3.3;
                    }
                }
                else
                {
                    //repeated
                    return (decimal)-2.2;
                }

            }
            catch
            {
                //error
                return (decimal)-1.1;
            }

        }


        [HttpPost]
        [Route("SaveSerialsList")]
        public string SaveSerialsList(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string jsonObj = "";
                string strObj = "";
                List<string> selialsLst = new List<string>();
                serials newObject = new serials();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "selialsLst")
                    {
                        jsonObj = c.Value.Replace("\\", string.Empty);
                        jsonObj = jsonObj.Trim('"');
                        selialsLst = JsonConvert.DeserializeObject<List<string>>(jsonObj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "itemObject")
                    {
                        strObj = c.Value.Replace("\\", string.Empty);
                        strObj = strObj.Trim('"');
                        newObject = JsonConvert.DeserializeObject<serials>(strObj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                }
                try
                {
                    decimal saveRes = 0;
                    serials serialtmp = new serials();
                    int count = 0;
                    foreach (string serialnum in selialsLst)
                    {
                        serialtmp = new serials();
                        serialtmp.serialId = 0;
                        serialtmp.itemUnitId = newObject.itemUnitId;
                        serialtmp.createUserId = newObject.createUserId;
                        serialtmp.updateUserId = newObject.updateUserId;
                        serialtmp.branchId = newObject.branchId;
                        serialtmp.isActive = newObject.isActive;
                        serialtmp.isSold = newObject.isSold;
                        //  serialtmp = newObject;
                        serialtmp.serialNum = serialnum;
                        saveRes = 0;
                        saveRes = Save(serialtmp);
                        if (saveRes > 0)
                        {
                            count++;
                        }
                    }
                    return TokenManager.GenerateToken(count);
                }
                catch
                {
                    return TokenManager.GenerateToken("-1");
                }
            }
        }
        // chang isSold
        [HttpPost]
        [Route("UpdateSerialsList")]
        public string UpdateSerialsList(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string jsonObj = "";

                List<serials> selialsLst = new List<serials>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "selialsLst")
                    {
                        jsonObj = c.Value.Replace("\\", string.Empty);
                        jsonObj = jsonObj.Trim('"');
                        selialsLst = JsonConvert.DeserializeObject<List<serials>>(jsonObj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }

                }
                try
                {
                    decimal saveRes = 0;

                    int count = 0;
                    foreach (serials serialrow in selialsLst)
                    {


                        saveRes = 0;
                        saveRes = Save(serialrow);
                        if (saveRes > 0)
                        {
                            count++;
                        }
                    }
                    return TokenManager.GenerateToken(count);
                }
                catch
                {
                    return TokenManager.GenerateToken("-1");
                }
            }
        }

        [HttpPost]
        [Route("SoldSerialsList")]
        public string SoldSerialsList(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string jsonObj = "";

                int userId = 0;
                List<string> selialsLst = new List<string>();
                serials newObject = new serials();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "selialsLst")
                    {
                        jsonObj = c.Value.Replace("\\", string.Empty);
                        jsonObj = jsonObj.Trim('"');
                        selialsLst = JsonConvert.DeserializeObject<List<string>>(jsonObj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                try
                {
                    DateTime datenow = cc.AddOffsetTodate(DateTime.Now);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        decimal saveRes = 0;
                        serials serialtmp = new serials();
                        int count = 0;
                        var list = entity.serials.Where(s => selialsLst.Contains(s.serialNum) && s.isSold == false).ToList();
                        list.ForEach(s => { s.isSold = true; s.updateDate = datenow; s.updateUserId = userId; });
                        int res = entity.SaveChanges();

                        return TokenManager.GenerateToken(res);
                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("-1");
                }
            }
        }

        [HttpPost]
        [Route("GetPropertiesAvailable")]
        public string GetPropertiesAvailable(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                int branchId = 0;
                int itemUnitId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);
                    }
                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<StorePropertyModel> invListm = new List<StorePropertyModel>();

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        invListm = (from S in entity.storeProperties
                                    join B in entity.branches on S.branchId equals B.branchId
                                    // join IT in entity.itemsTransfer on S.itemsTransId equals IT.itemsTransId
                                    //  join I in entity.invoices on IT.invoiceId equals I.invoiceId
                                    join IU in entity.itemsUnits on S.itemUnitId equals IU.itemUnitId
                                    join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                    join UNIT in entity.units on IU.unitId equals UNIT.unitId

                                    where (B.branchId == branchId
                                    && S.itemUnitId == itemUnitId && S.isSold == false)
                                    select new StorePropertyModel
                                    {
                                        serialId = S.serialId,
                                        itemsTransId = S.itemsTransId,
                                        itemUnitId = S.itemUnitId,
                                        isActive = S.isActive,
                                        createDate = S.createDate,
                                        updateDate = S.updateDate,
                                        createUserId = S.createUserId,
                                        updateUserId = S.updateUserId,
                                        isSold = S.isSold,
                                        branchId = S.branchId,
                                        count = S.count,
                                        storeProbId = S.storeProbId,
                                        notes = S.notes,
                                        StorePropertiesValueList = entity.storePropertiesValues.Where(v => v.storeProbId == S.storeProbId).Select(v => new StorePropertyValueModel
                                        {
                                            storeProbValueId = v.storeProbValueId,
                                            propertyId = v.propertyId,
                                            propertyItemId = v.propertyItemId,
                                            storeProbId = v.storeProbId,
                                            propertyName = v.propertyName,
                                            propertyValue = v.propertyValue,
                                            notes = v.notes,
                                            createDate = v.createDate,
                                            updateDate = v.updateDate,
                                            createUserId = v.createUserId,
                                            updateUserId = v.updateUserId,
                                            isActive = v.isActive,

                                        }).ToList(),
                                        //itemName = ITEM.name,
                                        //unitName = UNIT.name,
                                        //branchName = B.name,
                                        //invoiceId = I.invoiceId,
                                        //invNumber = I.invNumber,
                                    }).ToList();
                    }
                    return TokenManager.GenerateToken(invListm);
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

    }
}