using Newtonsoft.Json;
using POS_Server.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using POS_Server.Models.VM;
using System.Security.Claims;
using System.Web;

using Newtonsoft.Json.Converters;
using System.Threading.Tasks;
using LinqKit;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/ItemsLocations")]
    public class ItemsLocationsController : ApiController
    {
        CountriesController cc = new CountriesController();
        ItemsUnitsController itemsUnitsController = new ItemsUnitsController();
        GroupObjectController group = new GroupObjectController();
        NotificationController notificationController = new NotificationController();
        notificationUserController notUserController = new notificationUserController();

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
                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var docImageList = (from b in entity.itemsLocations
                                            where b.quantity > 0 && b.invoiceId == null
                                            join u in entity.itemsUnits on b.itemUnitId equals u.itemUnitId
                                            join i in entity.items on u.itemId equals i.itemId
                                            join l in entity.locations on b.locationId equals l.locationId
                                            join s in entity.sections on l.sectionId equals s.sectionId
                                            where s.branchId == branchId && s.isFreeZone != 1

                                            select new ItemLocationModel
                                            {
                                                createDate = b.createDate,
                                                createUserId = b.createUserId,
                                                endDate = b.endDate,
                                                itemsLocId = b.itemsLocId,
                                                itemUnitId = b.itemUnitId,
                                                locationId = b.locationId,
                                                note = b.note,
                                                quantity = b.quantity,
                                                startDate = b.startDate,

                                                updateDate = b.updateDate,
                                                updateUserId = b.updateUserId,
                                                itemName = i.name,
                                                location = l.x + l.y + l.z,
                                                section = s.name,
                                                sectionId = s.sectionId,
                                                itemType = i.type,
                                                unitName = u.units.name,
                                                invoiceId = b.invoiceId,
                                                isExpired = i.isExpired,
                                            }).ToList().OrderBy(x => x.location).ToList();


                        return TokenManager.GenerateToken(docImageList);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        [HttpPost]
        [Route("GetLockedItems")]
        public string GetLockedItems(string token)
        {
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
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var lockedUnitList = (from b in entity.itemsLocations
                                            where b.quantity > 0 && b.invoiceId != null
                                            join u in entity.itemsUnits on b.itemUnitId equals u.itemUnitId
                                            join it in entity.itemsTransfer on b.invoiceId equals it.invoiceId
                                            join i in entity.items on u.itemId equals i.itemId
                                            join l in entity.locations on b.locationId equals l.locationId
                                            where l.sections.branchId == branchId && it.invoiceId == b.invoiceId && it.itemUnitId == u.itemUnitId

                                            select new ItemLocationModel
                                            {
                                                createDate = b.createDate,
                                                createUserId = b.createUserId,
                                                endDate = b.endDate,
                                                itemsLocId = b.itemsLocId,
                                                itemUnitId = b.itemUnitId,
                                                locationId = b.locationId,
                                                note = b.note,
                                                lockedQuantity = b.quantity,
                                                quantity = it.quantity,
                                                startDate = b.startDate,

                                                updateDate = b.updateDate,
                                                updateUserId = b.updateUserId,
                                                itemName = i.name,
                                                location = l.x + l.y + l.z,
                                                section = l.sections.name,
                                                sectionId = l.sectionId,
                                                itemType = i.type,
                                                unitName = u.units.name,
                                                invoiceId = b.invoiceId,
                                                invNumber = b.invoices.invNumber,
                                                invType = b.invoices.invType,
                                                isExpired = i.isExpired,
                                            }).ToList().OrderBy(x => x.location).ToList();

                        var notLockedUnits  = (from it in entity.itemsTransfer
                                               .Where(x => x.invoices.branchCreatorId == branchId 
                                                        && x.invoices.isActive == true
                                                        && x.invoices.invType == "ors"
                                                        && x.itemsUnits.items.type != "sr"
                                                        && !entity.itemsLocations.Any(s => s.invoiceId == x.invoiceId)) 

                                                select new ItemLocationModel
                                                {
                                                    lockedQuantity = 0,
                                                    quantity = it.quantity,
                                                    itemName = it.itemsUnits.items.name,
                                                    itemType = it.itemsUnits.items.type,
                                                    unitName = it.itemsUnits.units.name,
                                                    invoiceId = it.invoiceId,
                                                    invNumber = it.invoices.invNumber,
                                                    invType = it.invoices.invType,
                                                    isExpired = it.itemsUnits.items.isExpired,
                                                }).ToList().OrderBy(x => x.location).ToList();

                        lockedUnitList.AddRange(notLockedUnits);
                        return TokenManager.GenerateToken(lockedUnitList);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        [HttpPost]
        [Route("GetAll")]
        public string GetAll(string token)
        {
            //int branchId string token
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

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var items = (from b in entity.itemsLocations
                                            where b.quantity > 0 && b.invoiceId == null
                                            join u in entity.itemsUnits on b.itemUnitId equals u.itemUnitId
                                            join i in entity.items on u.itemId equals i.itemId
                                            join l in entity.locations on b.locationId equals l.locationId
                                            join s in entity.sections on l.sectionId equals s.sectionId
                                            where s.branchId == branchId

                                            select new ItemLocationModel
                                            {
                                                createDate = b.createDate,
                                                createUserId = b.createUserId,
                                                endDate = b.endDate,
                                                itemsLocId = b.itemsLocId,
                                                itemUnitId = b.itemUnitId,
                                                itemId = u.itemId,
                                                locationId = b.locationId,
                                                note = b.note,
                                                quantity = b.quantity,
                                                startDate = b.startDate,
                                                
                                                updateDate = b.updateDate,
                                                updateUserId = b.updateUserId,
                                                itemName = i.name,
                                                location = l.x + l.y + l.z,
                                                section = s.name,
                                                sectionId = s.sectionId,
                                                itemType = i.type,
                                                unitName = u.units.name,
                                                invoiceId = b.invoiceId,
                                                isExpired = i.isExpired,
                                                avgPurchasePrice =i.avgPurchasePrice,
                                            }).ToList().OrderBy(x => x.location).ToList();


                        
                        foreach(var i in items)
                        {
                            i.avgPurchasePrice = i.avgPurchasePrice * itemsUnitsController.multiplyFactorWithSmallestUnit((int)i.itemId,(int)i.itemUnitId);
                        }
                        return TokenManager.GenerateToken(items);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        [HttpPost]
        [Route("getForDestroy")]
        public string getForDestroy(string token)
        {
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

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var items = (from  u in entity.itemsUnits 
                                            join i in entity.items on u.itemId equals i.itemId

                                     where u.itemUnitId == entity.itemsLocations.Where(x => x.itemUnitId == u.itemUnitId
                                                            && x.quantity > 0 && x.invoiceId == null 
                                                            && x.locations.branchId == branchId).Select(x => x.itemUnitId).FirstOrDefault()
                                            select new ItemLocationModel
                                            {
                                                itemUnitId = u.itemUnitId,
                                                itemId = u.itemId,
                                                itemName = i.name,
                                                itemType = i.type,
                                                unitName = u.units.name,
                                                avgPurchasePrice =i.avgPurchasePrice,
                                            }).ToList().Distinct().ToList();


                        
                        foreach(var i in items)
                        {
                            i.avgPurchasePrice = i.avgPurchasePrice * itemsUnitsController.multiplyFactorWithSmallestUnit((int)i.itemId,(int)i.itemUnitId);
                        }
                        return TokenManager.GenerateToken(items);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        [HttpPost]
        [Route("getAmountByItemLocId")]
        public string getAmountByItemLocId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request); 
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {


                int itemLocId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemLocId")
                    {
                        itemLocId = int.Parse(c.Value);
                    }


                }

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemLoc = entity.itemsLocations.Find(itemLocId);

                        if (itemLoc == null)
                            //  return NotFound();
                            return TokenManager.GenerateToken("0");
                        else
                            //   return Ok(itemLoc.quantity);

                            return TokenManager.GenerateToken(itemLoc.quantity.ToString());
                    }



                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        [HttpPost]
        [Route("getWithSequence")]
        public string getWithSequence(string token)
        {

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
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemLocList = (from b in entity.itemsLocations
                                           where b.quantity > 0 && b.invoiceId == null
                                           join u in entity.itemsUnits on b.itemUnitId equals u.itemUnitId
                                           join un in entity.units on u.unitId equals un.unitId
                                           join i in entity.items on u.itemId equals i.itemId
                                           join l in entity.locations on b.locationId equals l.locationId
                                           join s in entity.sections on l.sectionId equals s.sectionId
                                           where s.branchId == branchId && s.isFreeZone != 1

                                           select new ItemLocationModel
                                           {
                                               createDate = b.createDate,
                                               createUserId = b.createUserId,
                                               endDate = b.endDate,
                                               itemsLocId = b.itemsLocId,
                                               itemUnitId = b.itemUnitId,
                                               locationId = b.locationId,
                                               note = b.note,
                                               quantity = b.quantity,
                                               startDate = b.startDate,

                                               updateDate = b.updateDate,
                                               updateUserId = b.updateUserId,
                                               itemName = i.name,
                                               location = l.x + l.y + l.z,
                                               section = s.name,
                                               unitName = un.name,
                                               sectionId = s.sectionId,
                                               itemType = i.type,
                                               isExpired = i.isExpired,
                                           }).ToList();
                        int sequence = 1;
                        foreach (ItemLocationModel i in itemLocList)
                        {
                            i.sequence = sequence;
                            sequence++;
                        }
                        return TokenManager.GenerateToken(itemLocList);

                    }



                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
          
        }
        [HttpPost]
        [Route("GetFreeZoneItems")]
        public string GetFreeZoneItems(string token)
        {
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

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var docImageList = (from b in entity.itemsLocations
                                            where b.quantity > 0 && b.invoiceId == null
                                            join u in entity.itemsUnits on b.itemUnitId equals u.itemUnitId
                                            join i in entity.items on u.itemId equals i.itemId
                                            join l in entity.locations on b.locationId equals l.locationId
                                            join s in entity.sections on l.sectionId equals s.sectionId
                                            where s.branchId == branchId && s.isFreeZone == 1

                                            select new ItemLocationModel
                                            {
                                                createDate = b.createDate,
                                                createUserId = b.createUserId,
                                                endDate = b.endDate,
                                                itemsLocId = b.itemsLocId,
                                                itemUnitId = b.itemUnitId,
                                                locationId = b.locationId,
                                                note = b.note,
                                                quantity = b.quantity,
                                                startDate = b.startDate,

                                                updateDate = b.updateDate,
                                                updateUserId = b.updateUserId,
                                                itemName = i.name,
                                                sectionId = s.sectionId,
                                                isFreeZone = s.isFreeZone,
                                                itemType = i.type,
                                                location = l.x + l.y + l.z,
                                                section = s.name,
                                                unitName = u.units.name,
                                                isExpired = i.isExpired,
                                            })
                                        .ToList();


                        return TokenManager.GenerateToken(docImageList);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }

           
        }
        [HttpPost]
        [Route("GetByItemUnitId")]
        public string GetByItemUnitId(string token)
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
                int locationId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);
                    }
                    else if (c.Type == "locationId")
                    {
                        locationId = int.Parse(c.Value);
                    }



                }

                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var docImageList = entity.itemsLocations
                            .Where(b => b.itemUnitId == itemUnitId && b.locationId == locationId && b.invoiceId == null)
                            .Select(b => new
                            {
                                b.createDate,
                                b.createUserId,
                                b.endDate,
                                b.itemsLocId,
                                b.itemUnitId,
                                b.locationId,
                                b.note,
                                b.quantity,
                                b.startDate,

                                b.updateDate,
                                b.updateUserId,
                                b.itemsUnits.items.isExpired,
                            })
                        .ToList();

                        return TokenManager.GenerateToken(docImageList);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }

        }


        [HttpPost]
        [Route("save")]
        public string save(string token)
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
                List<itemsLocations> newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsLocations>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                        itemsLocations item;
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            foreach (itemsLocations itemLoc in newObject)
                            {
                                if (itemLoc.updateUserId == 0 || itemLoc.updateUserId == null)
                                {
                                    Nullable<int> id = null;
                                    itemLoc.updateUserId = id;
                                }
                                if (itemLoc.createUserId == 0 || itemLoc.createUserId == null)
                                {
                                    Nullable<int> id = null;
                                    itemLoc.createUserId = id;
                                }
                                var itemEntity = entity.Set<itemsLocations>();
                                item = itemEntity.Find(itemLoc.itemUnitId, itemLoc.locationId);
                                if (item == null)
                                {
                                    itemLoc.createDate = cc.AddOffsetTodate(DateTime.Now);
                                    itemLoc.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                    itemLoc.updateUserId = itemLoc.createUserId;

                                    item = itemEntity.Add(itemLoc);
                                }
                                else
                                {
                                    item.quantity = itemLoc.quantity;
                                    item.startDate = itemLoc.startDate;
                                    item.endDate = itemLoc.endDate;
                                    item.note = itemLoc.note;
                                    item.invoiceId = itemLoc.invoiceId;
                                    item.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                    item.updateUserId = itemLoc.updateUserId;
                                }
                            }

                            message = entity.SaveChanges().ToString();
                            //  return true;
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
        }
       
        [HttpPost]
        [Route("receiptInvoice")]
        public string receiptInvoice(string token)
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
                int branchId = 0;
                int userId = 0;
                string objectName = "";
                string notificationObj = "";
                InvoicesController ic = new InvoicesController();

                List<itemsTransfer> newObject = null;
                List<ItemTransferModel> transferObjectModel = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        transferObjectModel = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "branchId")
                        branchId = int.Parse(c.Value);
                    else if (c.Type == "userId")
                        userId = int.Parse(c.Value);
                    else if (c.Type == "objectName")
                        objectName = c.Value;
                    else if (c.Type == "notificationObj")
                        notificationObj = c.Value;
                }

                if (newObject != null)
                {
                    try
                    {
                        recieptItems(branchId, transferObjectModel, userId,objectName,notificationObj);

                        return TokenManager.GenerateToken("1");
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
        }

        public async Task recieptItems(int branchId, List<ItemTransferModel> newObject, int userId,string objectName, string notificationObj)
        {
            InvoicesController ic = new InvoicesController();
            using (incposdbEntities entity = new incposdbEntities())
            {
                #region get lack of orders items
                var orders = await ic.getUnhandeledOrdersList("ors", 0, branchId);

                List<ItemTransferModel> requiredTransfers = new List<ItemTransferModel>();
                foreach (InvoiceModel invoice in orders)
                {
                    var itemsTransfer = entity.itemsTransfer.Where(x => x.invoiceId == invoice.invoiceId
                                                            && x.itemsUnits.items.type != "sr").ToList();

                    foreach (itemsTransfer tr in itemsTransfer)
                    {
                        var lockedQuantity = entity.itemsLocations
                            .Where(x => x.invoiceId == invoice.invoiceId && x.itemUnitId == tr.itemUnitId)
                            .Select(x => x.quantity).Sum();
                        var availableAmount = getBranchAmount((int)tr.itemUnitId, branchId);
                        var item = (from i in entity.items
                                    join u in entity.itemsUnits on i.itemId equals u.itemId
                                    where u.itemUnitId == tr.itemUnitId
                                    select new ItemModel()
                                    {
                                        itemId = i.itemId,
                                        name = i.name,
                                        unitName = u.units.name,
                                    }).FirstOrDefault();
                        if (lockedQuantity == null)
                            lockedQuantity = 0;

                        if ((lockedQuantity + availableAmount) < tr.quantity) // there is a shortage in order amount
                        {
                            long requiredQuantity = (long)tr.quantity - ((long)lockedQuantity + (long)availableAmount);

                            ItemTransferModel transfer = new ItemTransferModel()
                            {
                                invNumber = invoice.invNumber,
                                invoiceId = invoice.invoiceId,
                                price = 0,
                                quantity = requiredQuantity,
                                itemUnitId = tr.itemUnitId,
                                itemId = item.itemId,
                                itemName = item.name,
                                unitName = item.unitName,
                            };
                            requiredTransfers.Add(transfer);
                        }

                    }
                }
                #endregion
                #region reciept all amount in freezone
                var freeZoneLocation = (from s in entity.sections.Where(x => x.branchId == branchId && x.isFreeZone == 1)
                                        join l in entity.locations on s.sectionId equals l.sectionId
                                        select l.locationId).SingleOrDefault();
                foreach (var item in newObject)
                {
                    var itemId = entity.itemsUnits.Where(x => x.itemUnitId == item.itemUnitId).Select(x => x.itemId).Single();
                    var itemV = entity.items.Find(itemId);

                    if (item.itemType != "sr")
                    {
                        increaseItemQuantity(item.itemUnitId.Value, freeZoneLocation, (int)item.quantity, userId);
                        #region amount notification
                        bool isExcedded = isExceddMaxQuantity((int)item.itemUnitId, branchId, userId);
                        if (isExcedded == true) //add notification
                        {
                            notificationController.addNotifications(objectName, notificationObj, branchId, itemV.name);
                        }
                        #endregion

                        #region add amount to reserved amount
                        foreach (ItemTransferModel it in requiredTransfers)
                        {
                            lockItem(it.itemUnitId.Value, (int)it.invoiceId, branchId, (int)it.quantity, userId);
                        }

                        #endregion
                    }
                    if (item.offerId != 0 && item.offerId != null) //if invoice is bounce
                    {
                        int offerId = (int)item.offerId;
                        int itemUnitId = (int)item.itemUnitId;
                        var offer = entity.itemsOffers.Where(x => x.iuId == itemUnitId && x.offerId == offerId).FirstOrDefault();
                        offer.used -= (int)item.quantity;
                        entity.SaveChanges();
                    }
                   
                }
                #endregion

               
            }
        }
        public bool isExceddMaxQuantity(int itemUnitId, int branchId, int userId)
        {
            bool isExcedded = false;
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemId = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => x.itemId).Single();
                    var item = entity.items.Find(itemId);
                    int maxUnitId = (int)item.maxUnitId;
                    int maxQuantity = (int)item.max;
                    if (maxQuantity == 0)
                        return false;
                    var maxUnit = entity.itemsUnits.Where(x => x.itemId == itemId && x.unitId == maxUnitId).FirstOrDefault();
                    if (maxUnit == null)
                        isExcedded = false;
                    else
                    {
                        int itemUnitQuantity = getItemAmount(maxUnit.itemUnitId, branchId);
                        if (itemUnitQuantity >= maxQuantity)
                        {
                            isExcedded = true;
                        }
                        if (isExcedded == false)
                        {
                            int smallestItemUnit = entity.itemsUnits.Where(x => x.itemId == itemId && x.subUnitId == x.unitId).Select(x => x.itemUnitId).Single();
                            int smallUnitQuantity = getLevelItemUnitAmount(smallestItemUnit, maxUnit.itemUnitId, branchId);
                            int unitValue = itemsUnitsController.getLargeUnitConversionQuan(smallestItemUnit, maxUnit.itemUnitId);
                            int quantity = 0;
                            if (unitValue != 0)
                                quantity = smallUnitQuantity / unitValue;

                            quantity += itemUnitQuantity;
                            if (quantity >= maxQuantity)
                            {
                                isExcedded = true;
                            }
                        }

                    }
                }
            }
            catch
            {
            }
            return isExcedded;
        }

        [HttpPost]
        [Route("generatePackage")]
        public string generatePackage(string token)
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
                #region params
                int packageParentId = 0;
                int quantity = 0;
                int locationId = 0;
                int branchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                    else if (c.Type == "locationId")
                    {
                        locationId = int.Parse(c.Value);

                    }
                    else if (c.Type == "quantity")
                    {
                        quantity = int.Parse(c.Value);

                    }
                    else if (c.Type == "packageParentId")
                    {
                        packageParentId = int.Parse(c.Value);

                    }
                }
                #endregion
                try
                {
                    
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var packageIems = (from S in entity.packages
                                           join IU in entity.itemsUnits on S.childIUId equals IU.itemUnitId
                                           join I in entity.items on IU.itemId equals I.itemId
                                           where S.parentIUId == packageParentId
                                           select new PackageModel
                                           {
                                               childIUId = S.childIUId,
                                               quantity = S.quantity,
                                               type = I.type,
                                               citemId=I.itemId,
                                               avgPurchasePrice = I.avgPurchasePrice,
                                           }).ToList();

                        #region check available quantity of package items
                        foreach (var p in packageIems)
                        {
                            if (!p.type.Equals("sr"))
                            {
                                int branchQuantity = 0;
                                int itemQuanP = p.quantity;
                                int itemUnitId = (int)p.childIUId;
                                int requireQuan = itemQuanP * quantity;

                                branchQuantity = getBranchAmount(itemUnitId, branchId);

                                if (requireQuan > branchQuantity)
                                {
                                    return TokenManager.GenerateToken("-3");
                                }
                            }

                        }
                        #endregion
                        ItemsUnitsController uc = new ItemsUnitsController();
                        decimal avgPurchasePrice = 0;
                        foreach (PackageModel item in packageIems)
                        {
                            if (!item.type.Equals("sr"))
                            {
                                int itemQuantity = item.quantity * quantity;
                                int itemUnitId = (int)item.childIUId;
                                convertToPackage(itemUnitId, branchId, itemQuantity, userId);

                                var itemInLocs = (from b in entity.branches
                                                  where b.branchId == branchId
                                                  join s in entity.sections on b.branchId equals s.branchId
                                                  join l in entity.locations on s.sectionId equals l.sectionId
                                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                                  where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null
                                                  select new
                                                  {
                                                      il.itemsLocId,
                                                      il.quantity,
                                                      il.itemUnitId,
                                                      il.locationId,
                                                      s.sectionId,
                                                  }).ToList();

                                for (int i = 0; i < itemInLocs.Count; i++)
                                {
                                    int availableAmount = (int)itemInLocs[i].quantity;
                                    var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                                    itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                    if (availableAmount >= itemQuantity)
                                    {
                                        itemL.quantity = availableAmount - itemQuantity;
                                        itemQuantity = 0;
                                        entity.SaveChanges();
                                    }
                                    else if (availableAmount > 0)
                                    {
                                        itemL.quantity = 0;
                                        itemQuantity = itemQuantity - availableAmount;
                                        entity.SaveChanges();
                                    }
                                    if (itemQuantity == 0)
                                        break;
                                }
                            }
                        }
                    
                        increaseItemQuantity(packageParentId, locationId, quantity, userId);

                        var itemId = entity.itemsUnits.Where(x => x.itemUnitId == packageParentId).Select(x => x.itemId).FirstOrDefault();
                        var package = entity.items.Find(itemId);
                        package.avgPurchasePrice = uc.calculatePackagePrice(packageParentId) ;
                        entity.SaveChanges();
                    }
                    return TokenManager.GenerateToken("1");
            }
            catch
            {
                message = "0";
                return TokenManager.GenerateToken(message);
            }
        }
       
    }
        [HttpPost]
        [Route("decomposePackage")]
        public string decomposePackage(string token)
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
                #region params
                int packageParentId = 0;
                int quantity = 0;
                int locationId = 0;
                int branchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                    else if (c.Type == "locationId")
                    {
                        locationId = int.Parse(c.Value);

                    }
                    else if (c.Type == "quantity")
                    {
                        quantity = int.Parse(c.Value);

                    }
                    else if (c.Type == "packageParentId")
                    {
                        packageParentId = int.Parse(c.Value);

                    }
                }
                #endregion
                try
                {
                    
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var packageIems = (from S in entity.packages
                                           join IU in entity.itemsUnits on S.childIUId equals IU.itemUnitId
                                           join I in entity.items on IU.itemId equals I.itemId
                                           where S.parentIUId == packageParentId
                                           select new PackageModel
                                           {
                                               childIUId = S.childIUId,
                                               quantity = S.quantity,
                                               type = I.type,
                                           }).ToList();

                        #region check available quantity of package
                      // int branchQuantity = getBranchAmount(packageParentId, branchId);
                       int locationQuantity = getAmountInLocation( locationId,packageParentId);

                        if (quantity > locationQuantity)
                        {
                            return TokenManager.GenerateToken("-3");
                        }

                        #endregion

                        #region decrease package quantity
                        decreaseItemQuantity(packageParentId, locationId, quantity, userId);
                        #endregion

                        #region increase package items quantity
                        foreach (PackageModel item in packageIems)
                        {
                            if (!item.type.Equals("sr"))
                            {
                                int itemQuantity = item.quantity * quantity;
                                int itemUnitId = (int)item.childIUId;
                                increaseItemQuantity(itemUnitId, locationId, itemQuantity, userId);
                              
                            }
                        }
                        #endregion
                       
                        var itemId = entity.itemsUnits.Where(x => x.itemUnitId == packageParentId).Select(x => x.itemId).FirstOrDefault();
                        var package = entity.items.Find(itemId);
                        package.avgPurchasePrice = itemsUnitsController.calculatePackagePrice(packageParentId);
                        entity.SaveChanges();
                    }
                    return TokenManager.GenerateToken("1");
            }
            catch
            {
                message = "0";
                return TokenManager.GenerateToken(message);
            }
        }

        
    }
        public void convertToPackage(int itemUnitId,  int branchId, int requiredAmount, int userId)
        {
            int locationId = 0;
            Dictionary<string, int> dic = new Dictionary<string, int>();
            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemInLocs = (from s in entity.sections
                                  where s.branchId == branchId
                                  join l in entity.locations on s.sectionId equals l.sectionId
                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                  where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null
                                  select new
                                  {
                                      il.itemsLocId,
                                      il.quantity,
                                      il.itemUnitId,
                                      il.locationId,
                                      il.updateDate,
                                      s.sectionId,
                                  }).ToList().OrderBy(x => x.updateDate).ToList();
                for (int i = 0; i < itemInLocs.Count; i++)
                {
                    int availableAmount = (int)itemInLocs[i].quantity;
                    int lockedAmount = 0;
                    var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                    itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                    if (availableAmount >= requiredAmount)
                    {
                        lockedAmount = requiredAmount;
                        requiredAmount = 0;
                    }
                    else if (availableAmount > 0)
                    {
                        requiredAmount = requiredAmount - availableAmount;
                        lockedAmount = availableAmount;
                        entity.SaveChanges();
                    }
                    if (requiredAmount == 0)
                        break;
                }

                if (requiredAmount != 0)
                {
                    dic = lockUpperUnit(itemUnitId, branchId, requiredAmount, userId);

                     if ((dic["remainQuantity"] + dic["lockedQuantity"])> 0)
                    {
                        var item = (from il in entity.itemsLocations
                                    where il.itemUnitId == itemUnitId && il.invoiceId == null
                                    join l in entity.locations on il.locationId equals l.locationId
                                    join s in entity.sections on l.sectionId equals s.sectionId
                                    where s.branchId == branchId
                                    select new
                                    {
                                        il.itemsLocId,
                                    }).FirstOrDefault();
                        if (item != null)
                        {
                            var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                            itemloc.quantity += dic["remainQuantity"] + dic["lockedQuantity"];
                            entity.SaveChanges();
                        }
                        else
                        {
                            var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();
                            locationId = dic["locationId"];
                            if ((locationId == 0 && locationId == null) && locations.Count > 1)
                                locationId = locations[0].locationId; // free zoon
                            itemsLocations itemL = new itemsLocations();
                            itemL.itemUnitId = itemUnitId;
                            itemL.locationId = locationId;
                            itemL.quantity = dic["remainQuantity"] + dic["lockedQuantity"];
                            itemL.createDate = cc.AddOffsetTodate(DateTime.Now);
                            itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            itemL.createUserId = userId;
                            itemL.updateUserId = userId;
                            itemL.invoiceId = null;

                            entity.itemsLocations.Add(itemL);
                            entity.SaveChanges();
                        }
                    }

                    if (dic["requiredQuantity"] > 0)
                    {
                        dic = lockLowerUnit(itemUnitId, branchId, dic["requiredQuantity"], userId);
                        if (dic["lockedQuantity"] > 0)
                        {
                            var item = (from il in entity.itemsLocations
                                        where il.itemUnitId == itemUnitId && il.invoiceId == null
                                        join l in entity.locations on il.locationId equals l.locationId
                                        join s in entity.sections on l.sectionId equals s.sectionId
                                        where s.branchId == branchId
                                        select new
                                        {
                                            il.itemsLocId,
                                        }).FirstOrDefault();
                            if (item != null)
                            {
                                var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                                itemloc.quantity += dic["remainQuantity"];
                                entity.SaveChanges();
                            }
                            else
                            {
                                var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();
                                locationId = dic["locationId"];
                                if ((locationId == 0 && locationId == null) && locations.Count > 1)
                                    locationId = locations[0].locationId; // free zoon
                                itemsLocations itemL = new itemsLocations();
                                itemL.itemUnitId = itemUnitId;
                                itemL.locationId = locationId;
                                itemL.quantity = dic["remainQuantity"];
                                itemL.createDate = cc.AddOffsetTodate(DateTime.Now);
                                itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                itemL.createUserId = userId;
                                itemL.updateUserId = userId;
                                itemL.invoiceId = null;

                                entity.itemsLocations.Add(itemL);
                                entity.SaveChanges();
                            }
                        }
                    }
                }
            }
        }
        [HttpPost]
        [Route("receiptOrder")]
        public string receiptOrder(string token)
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
                string orderList = "";

                int toBranch = 0;
                int userId = 0;
                string objectName = "";
                string notificationObj = "";

                List<itemsLocations> newObject = new List<itemsLocations>();
                List<itemsTransfer> items = new List<itemsTransfer>();
                List<ItemTransferModel> transferModel = new List<ItemTransferModel>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsLocations>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "orderList")
                    {
                        orderList = c.Value.Replace("\\", string.Empty);
                        orderList = orderList.Trim('"');
                        items = JsonConvert.DeserializeObject<List<itemsTransfer>>(orderList, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        transferModel = JsonConvert.DeserializeObject<List<ItemTransferModel>>(orderList, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }

                    else if (c.Type == "toBranch")
                    {
                        toBranch = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                    else if (c.Type == "objectName")
                    {
                        objectName = c.Value;

                    }
                    else if (c.Type == "notificationObj")
                    {
                        notificationObj = c.Value;

                    }

                }
                if (newObject != null)
                {
                    try
                    {
                        receiptOrder(newObject, transferModel, toBranch, userId, objectName, notificationObj);
                        return TokenManager.GenerateToken("1");
                       
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
       
        }

        public void receiptOrder(List<itemsLocations> newObject,List<ItemTransferModel> items, int toBranch,int userId,string objectName,string notificationObj)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                #region decreasAmounts from current branch
                foreach (itemsLocations item in newObject)
                {
                    itemsLocations itemL = new itemsLocations();

                    itemL = entity.itemsLocations.Find(item.itemsLocId);
                    itemL.quantity -= item.quantity;
                    itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                    itemL.updateUserId = userId;
                    entity.SaveChanges();  
                }
                #endregion
                #region add amount to distination branch
                recieptItems(toBranch, items, userId, objectName, notificationObj);
                #endregion
            }
        }

        public void transferQuantity(List<itemsLocations> newObject, List<ItemTransferModel> items, int toBranch, int userId, string objectName, string notificationObj)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                #region add amount to distination branch
                recieptItems(toBranch, items, userId, objectName, notificationObj);
                #endregion

                #region decreasAmounts from current branch
                foreach (var row in items)
                {
                    var quantity = row.quantity;
                    var unitLocations = newObject.Where(x => x.itemUnitId == row.itemUnitId).ToList();
                    foreach (itemsLocations item in unitLocations)
                    {
                        itemsLocations itemL = new itemsLocations();

                        itemL = entity.itemsLocations.Find(item.itemsLocId);
                        if(quantity < itemL.quantity)
                        {
                            itemL.quantity -= quantity;
                            quantity = 0;
                        }
                        else
                        {
                            quantity -= itemL.quantity;
                            itemL.quantity = 0;
                        }
                        //itemL.quantity -= item.quantity;
                        itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                        itemL.updateUserId = userId;
                        entity.SaveChanges();

                        if (quantity == 0)
                            break;
                    }
                }
                #endregion
               
            }
        }
        [HttpPost]
        [Route("transferAmountbetweenUnits")]
        public string transferAmountbetweenUnits(string token)
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
                #region params
                int locationId = 0;
                int itemLocId = 0;
                int toItemUnitId = 0;
                int fromQuantity = 0;
                int toQuantity = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "locationId")
                    {
                        locationId = int.Parse(c.Value);

                    }
                    else if (c.Type == "itemLocId")
                    {
                        itemLocId = int.Parse(c.Value);

                    }
                    else if (c.Type == "toItemUnitId")
                    {
                        toItemUnitId = int.Parse(c.Value);

                    }
                    else if (c.Type == "fromQuantity")
                    {
                        fromQuantity = int.Parse(c.Value);

                    }
                    else if (c.Type == "toQuantity")
                    {
                        toQuantity = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }

                    #endregion

                }
                try
                {
                  
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        #region check location quantity
                        var locationQuantity = entity.itemsLocations.Where(x => x.itemsLocId == itemLocId).FirstOrDefault().quantity;
                        if(locationQuantity < fromQuantity)
                            return TokenManager.GenerateToken("-3");

                        #endregion

                        decreaseItemLocationQuantity(itemLocId, fromQuantity, userId, "", "");
                        increaseItemQuantity(toItemUnitId, locationId, toQuantity, userId);
                    }

                    return TokenManager.GenerateToken("1");
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }

            }

           
        }
        private void increaseItemQuantity(int itemUnitId, int locationId, int quantity, int userId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemUnit = (from il in entity.itemsLocations
                                where il.itemUnitId == itemUnitId && il.locationId == locationId && il.invoiceId == null
                                select new { il.itemsLocId }
                                ).FirstOrDefault();
                itemsLocations itemL = new itemsLocations();
                if (itemUnit == null)//add item in new location
                {
                    itemL.itemUnitId = itemUnitId;
                    itemL.locationId = locationId;
                    itemL.quantity = quantity;
                    itemL.createDate = cc.AddOffsetTodate(DateTime.Now);
                    itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                    itemL.createUserId = userId;
                    itemL.updateUserId = userId;

                    entity.itemsLocations.Add(itemL);
                }
                else
                {
                    itemL = entity.itemsLocations.Find(itemUnit.itemsLocId);
                    itemL.quantity += quantity;
                    itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                    itemL.updateUserId = userId;
                }
                entity.SaveChanges();
            }
        }


        [HttpPost]
        [Route("trasnferItem")]
        public string trasnferItem(string token)
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
                int itemLocId = 0;
                ItemLocationModel newObject = new ItemLocationModel();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemLocId")
                    {
                        itemLocId = int.Parse(c.Value);

                    }

                    else if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<ItemLocationModel>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }

                }

                if (newObject != null)
                {
                    try
                    {
                       
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var oldItemL = entity.itemsLocations.Find(itemLocId);
                            #region check quantity before transfer
                            if (oldItemL.quantity < newObject.quantity)
                                return TokenManager.GenerateToken("-3");
                            #endregion


                            int userId = (int)newObject.updateUserId;
                            long newQuantity = (long)oldItemL.quantity - (long)newObject.quantity;
                            oldItemL.quantity = (long)newQuantity;
                            oldItemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            oldItemL.updateUserId = userId;


                            var newtemLocation = (from il in entity.itemsLocations
                                                  where il.itemUnitId == newObject.itemUnitId && il.locationId == newObject.locationId
                                                  && il.startDate == newObject.startDate && il.endDate == newObject.endDate && il.invoiceId == newObject.invoiceId
                                                  select new { il.itemsLocId }
                                           ).FirstOrDefault();

                            itemsLocations newItemL;
                            if (newtemLocation == null)//add item in new location
                            {
                                newItemL = new itemsLocations();
                                newItemL.createDate = cc.AddOffsetTodate(DateTime.Now);
                                newItemL.createUserId = (int)newObject.createUserId;
                                if (newObject.endDate != null)
                                    newItemL.endDate = newObject.endDate;
                                if (newObject.startDate != null)
                                    newItemL.startDate = newObject.startDate;
                                newItemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                newItemL.updateUserId = (int)newObject.createUserId;
                                newItemL.itemUnitId = (int)newObject.itemUnitId;
                                newItemL.locationId = (int)newObject.locationId;
                                newItemL.note = newObject.note;
                                newItemL.quantity = (long)newObject.quantity;
                                newItemL.invoiceId = newObject.invoiceId;
                                entity.itemsLocations.Add(newItemL);
                            }
                            else
                            {
                                newItemL = new itemsLocations();
                                newItemL = entity.itemsLocations.Find(newtemLocation.itemsLocId);
                                newQuantity = (long)newItemL.quantity + (long)newObject.quantity;
                                newItemL.quantity = (long)newQuantity;
                                newItemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                newItemL.updateUserId = (int)newObject.updateUserId;

                            }
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
                    return TokenManager.GenerateToken("0");
                }

            }


        }

          [HttpPost]
        [Route("changeUnitExpireDate")]
        public string changeUnitExpireDate(string token)
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
                int itemLocId = 0;
                int userId = 0;
                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemLocId")
                    {
                        itemLocId = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                    else if (c.Type == "startDate")
                    {
                        startDate = DateTime.Parse(c.Value);
                    }
                     else if (c.Type == "endDate")
                    {
                        endDate = DateTime.Parse(c.Value);
                    }

                }

                try
                {
                       
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemLoc = entity.itemsLocations.Find(itemLocId);

                        itemLoc.startDate = startDate;
                        itemLoc.endDate = endDate;
                        itemLoc.updateUserId = userId;                     
                         entity.SaveChanges().ToString();
    
                        return TokenManager.GenerateToken("1");
                    }


                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }


            }


        }

        [HttpPost]
        [Route("updateItemQuantity")]
        public string updateItemQuantity(string token)
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
                int itemUnitId = 0;
                int branchId = 0;
                int requiredAmount = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);

                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }
                    else if (c.Type == "requiredAmount")
                    {
                        requiredAmount = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }

                }

                try
                {
                    Dictionary<string, int> dic = new Dictionary<string, int>();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemInLocs = (from b in entity.branches
                                          where b.branchId == branchId
                                          join s in entity.sections on b.branchId equals s.branchId
                                          join l in entity.locations on s.sectionId equals l.sectionId
                                          join il in entity.itemsLocations on l.locationId equals il.locationId
                                          where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null
                                          select new
                                          {
                                              il.itemsLocId,
                                              il.quantity,
                                              il.itemUnitId,
                                              il.locationId,
                                              il.updateDate,
                                              s.sectionId,
                                          }).ToList().OrderBy(x => x.updateDate).ToList();
                        for (int i = 0; i < itemInLocs.Count; i++)
                        {
                            int availableAmount = (int)itemInLocs[i].quantity;
                            var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                            itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            if (availableAmount >= requiredAmount)
                            {
                                itemL.quantity = availableAmount - requiredAmount;
                                requiredAmount = 0;
                                entity.SaveChanges();
                            }
                            else if (availableAmount > 0)
                            {
                                itemL.quantity = 0;
                                requiredAmount = requiredAmount - availableAmount;
                                entity.SaveChanges();
                            }

                            if (requiredAmount == 0)

                                //  return Ok(3);
                                return TokenManager.GenerateToken("3");
                        }
                        if (requiredAmount != 0)
                        {
                            //int newQuant = checkUpperUnit(itemUnitId, branchId, requiredAmount,userId);
                            dic = checkUpperUnit(itemUnitId, branchId, requiredAmount, userId);

                            var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                            var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();


                            if (dic["remainQuantity"] > 0)
                            {
                                var item = (from il in entity.itemsLocations
                                            where il.itemUnitId == itemUnitId && il.invoiceId == null
                                            join l in entity.locations on il.locationId equals l.locationId
                                            join s in entity.sections on l.sectionId equals s.sectionId
                                            where s.branchId == branchId
                                            select new
                                            {
                                                il.itemsLocId,
                                            }).FirstOrDefault();
                                if (item != null)
                                {
                                    var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                                    //itemloc.quantity = (int)newQuant;
                                    itemloc.quantity = dic["remainQuantity"];
                                    entity.SaveChanges();
                                }
                                else
                                {
                                    var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();
                                    // if (locations.Count > 0)
                                    // {
                                    int locationId = dic["locationId"];
                                    if (locationId == 0 && locations.Count > 1)
                                        locationId = locations[0].locationId; // free zoon
                                    itemsLocations itemL = new itemsLocations();
                                    itemL.itemUnitId = itemUnitId;
                                    itemL.locationId = locationId;
                                    itemL.quantity = dic["remainQuantity"];
                                    itemL.createDate = cc.AddOffsetTodate(DateTime.Now);
                                    itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                    itemL.createUserId = userId;
                                    itemL.updateUserId = userId;

                                    entity.itemsLocations.Add(itemL);
                                    entity.SaveChanges();
                                }
                            }
                            if (dic["requiredQuantity"] > 0)
                            {
                                checkLowerUnit(itemUnitId, branchId, dic["requiredQuantity"], userId);
                            }

                        }
                    }
                    //  return Ok(2);
                    return TokenManager.GenerateToken("2");

                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }
            }           

        }

        public int updateItemQuantity(int itemUnitId, int branchId, int requiredAmount, int userId)
        {

            Dictionary<string, int> dic = new Dictionary<string, int>();
            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemInLocs = (from b in entity.branches
                                  where b.branchId == branchId
                                  join s in entity.sections on b.branchId equals s.branchId
                                  join l in entity.locations on s.sectionId equals l.sectionId
                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                  where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null
                                  select new
                                  {
                                      il.itemsLocId,
                                      il.quantity,
                                      il.itemUnitId,
                                      il.locationId,
                                      il.updateDate,
                                      s.sectionId,
                                  }).ToList().OrderBy(x => x.updateDate).ToList();
                for (int i = 0; i < itemInLocs.Count; i++)
                {
                    int availableAmount = (int)itemInLocs[i].quantity;
                    var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                    itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                    if (availableAmount >= requiredAmount)
                    {
                        itemL.quantity = availableAmount - requiredAmount;
                        requiredAmount = 0;
                        entity.SaveChanges();
                    }
                    else if (availableAmount > 0)
                    {
                        itemL.quantity = 0;
                        requiredAmount = requiredAmount - availableAmount;
                        entity.SaveChanges();
                    }

                    if (requiredAmount == 0)
                        return (3);
                }
                if (requiredAmount != 0)
                {
                    //int newQuant = checkUpperUnit(itemUnitId, branchId, requiredAmount,userId);
                    dic = checkUpperUnit(itemUnitId, branchId, requiredAmount, userId);

                    var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                    var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();


                    if (dic["remainQuantity"] > 0)
                    {
                        var item = (from il in entity.itemsLocations
                                    where il.itemUnitId == itemUnitId && il.invoiceId == null
                                    join l in entity.locations on il.locationId equals l.locationId
                                    join s in entity.sections on l.sectionId equals s.sectionId
                                    where s.branchId == branchId
                                    select new
                                    {
                                        il.itemsLocId,
                                    }).FirstOrDefault();
                        if (item != null)
                        {
                            var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                            //itemloc.quantity = (int)newQuant;
                            itemloc.quantity = dic["remainQuantity"];
                            entity.SaveChanges();
                        }
                        else
                        {
                            var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();
                            // if (locations.Count > 0)
                            // {
                            int locationId = dic["locationId"];
                            if (locationId == 0 && locations.Count > 1)
                                locationId = locations[0].locationId; // free zoon
                            itemsLocations itemL = new itemsLocations();
                            itemL.itemUnitId = itemUnitId;
                            itemL.locationId = locationId;
                            itemL.quantity = dic["remainQuantity"];
                            itemL.createDate = cc.AddOffsetTodate(DateTime.Now);
                            itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            itemL.createUserId = userId;
                            itemL.updateUserId = userId;

                            entity.itemsLocations.Add(itemL);
                            entity.SaveChanges();
                        }
                    }
                    if (dic["requiredQuantity"] > 0)
                    {
                        checkLowerUnit(itemUnitId, branchId, dic["requiredQuantity"], userId);
                    }

                }
            }
            return (2);

        }

        private Dictionary<string, int> checkUpperUnit(int itemUnitId, int branchId, int requiredAmount, int userId)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            dic.Add("remainQuantity", 0);
            dic.Add("locationId", 0);
            dic.Add("requiredQuantity", 0);
            dic.Add("isConsumed", 0);
            int remainQuantity = 0;
            int firstRequir = requiredAmount;
            decimal newQuant = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (upperUnit != null)
                {
                    decimal unitValue = (decimal)upperUnit.unitValue;
                    int breakNum = (int)Math.Ceiling(requiredAmount / unitValue);
                    newQuant = (decimal)(breakNum * upperUnit.unitValue);
                    var itemInLocs = (from b in entity.branches
                                      where b.branchId == branchId
                                      join s in entity.sections on b.branchId equals s.branchId
                                      join l in entity.locations on s.sectionId equals l.sectionId
                                      join il in entity.itemsLocations on l.locationId equals il.locationId
                                      where il.itemUnitId == upperUnit.itemUnitId && il.quantity > 0 && il.invoiceId == null
                                      select new
                                      {
                                          il.itemsLocId,
                                          il.quantity,
                                          il.itemUnitId,
                                          il.locationId,
                                          il.updateDate,
                                          s.sectionId,
                                      }).ToList().OrderBy(x => x.updateDate).ToList();

                    for (int i = 0; i < itemInLocs.Count; i++)
                    {
                        dic["isConsumed"] = 1;
                        var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                        var smallUnitLocId = entity.itemsLocations.Where(x => x.itemUnitId == itemUnitId && x.invoiceId == null).
                            Select(x => x.itemsLocId).FirstOrDefault();

                        if (breakNum <= itemInLocs[i].quantity)
                        {
                            itemL.quantity = itemInLocs[i].quantity - breakNum;
                            entity.SaveChanges();
                            remainQuantity = (int)newQuant - firstRequir;
                            requiredAmount = 0;
                            // return remainQuantity;
                            dic["remainQuantity"] = remainQuantity;
                            dic["locationId"] = (int)itemInLocs[i].locationId;
                            dic["requiredQuantity"] = 0;

                            return dic;
                        }
                        else
                        {
                            itemL.quantity = 0;
                            breakNum = (int)(breakNum - itemInLocs[i].quantity);
                            requiredAmount = requiredAmount - ((int)itemInLocs[i].quantity * (int)upperUnit.unitValue);
                            entity.SaveChanges();
                        }
                        if (breakNum == 0)
                            break;
                    }
                    if (breakNum != 0)
                    {
                        dic = new Dictionary<string, int>();
                        // remainQuantity = checkUpperUnit(upperUnit.itemUnitId, branchId, breakNum, userId);
                        dic = checkUpperUnit(upperUnit.itemUnitId, branchId, breakNum, userId);
                        var item = (from s in entity.sections
                                    where s.branchId == branchId
                                    join l in entity.locations on s.sectionId equals l.sectionId
                                    join il in entity.itemsLocations on l.locationId equals il.locationId
                                    where il.itemUnitId == upperUnit.itemUnitId && il.invoiceId == null
                                    select new
                                    {
                                        il.itemsLocId,
                                    }).FirstOrDefault();
                        if (item != null)
                        {
                            var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                            //itemloc.quantity = (int)remainQuantity;
                            itemloc.quantity = dic["remainQuantity"];
                            entity.SaveChanges();
                        }
                        else
                        {
                            var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();

                            int locationId = dic["locationId"];
                            if (locationId == 0 && locations.Count > 1)
                                locationId = locations[0].locationId; // free zoon

                            itemsLocations itemL = new itemsLocations();
                            //itemL.itemUnitId = itemUnitId;
                            itemL.itemUnitId = upperUnit.itemUnitId;
                            itemL.locationId = locationId;
                            itemL.quantity = dic["remainQuantity"];
                            itemL.createDate = cc.AddOffsetTodate(DateTime.Now);
                            itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            itemL.createUserId = userId;
                            itemL.updateUserId = userId;

                            entity.itemsLocations.Add(itemL);
                            entity.SaveChanges();

                        }

                        //dic["remainQuantity"] = (int)newQuant - firstRequir;
                        //dic["requiredQuantity"]= breakNum * (int)upperUnit.unitValue;
                        ///////////////////
                        if (dic["isConsumed"] == 0)
                        {
                            dic["requiredQuantity"] = requiredAmount;
                            dic["remainQuantity"] = 0;
                        }
                        else
                        {
                            dic["remainQuantity"] = (int)newQuant - firstRequir;
                            dic["requiredQuantity"] = breakNum * (int)upperUnit.unitValue;
                        }
                        return dic;
                    }
                }
                else
                {
                    dic["remainQuantity"] = 0;
                    dic["requiredQuantity"] = requiredAmount;
                    dic["locationId"] = 0;

                    return dic;
                }
            }
            return dic;
        }
        private Dictionary<string, int> checkLowerUnit(int itemUnitId, int branchId, int requiredAmount, int userId)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            int remainQuantity = 0;
            int firstRequir = requiredAmount;
            decimal newQuant = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId, x.subUnitId, x.unitValue }).FirstOrDefault();
                var lowerUnit = entity.itemsUnits.Where(x => x.unitId == unit.subUnitId && x.itemId == unit.itemId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (lowerUnit != null)
                {
                    decimal unitValue = (decimal)unit.unitValue;
                    int breakNum = (int)requiredAmount * (int)unitValue;
                    newQuant = (decimal)Math.Ceiling(breakNum / (decimal)lowerUnit.unitValue);
                    var itemInLocs = (from b in entity.branches
                                      where b.branchId == branchId
                                      join s in entity.sections on b.branchId equals s.branchId
                                      join l in entity.locations on s.sectionId equals l.sectionId
                                      join il in entity.itemsLocations on l.locationId equals il.locationId
                                      where il.itemUnitId == lowerUnit.itemUnitId && il.quantity > 0 && il.invoiceId == null
                                      select new
                                      {
                                          il.itemsLocId,
                                          il.quantity,
                                          il.itemUnitId,
                                          il.locationId,
                                          il.updateDate,
                                          s.sectionId,
                                      }).ToList().OrderBy(x => x.updateDate).ToList();

                    for (int i = 0; i < itemInLocs.Count; i++)
                    {

                        var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                        var smallUnitLocId = entity.itemsLocations.Where(x => x.itemUnitId == itemUnitId && x.invoiceId == null).
                            Select(x => x.itemsLocId).FirstOrDefault();


                        if (breakNum <= itemInLocs[i].quantity)
                        {
                            itemL.quantity = itemInLocs[i].quantity - breakNum;
                            entity.SaveChanges();
                            remainQuantity = (int)newQuant - firstRequir;
                            requiredAmount = 0;
                            // return remainQuantity;
                            dic.Add("remainQuantity", remainQuantity);
                            dic.Add("locationId", (int)itemInLocs[i].locationId);
                            return dic;
                        }
                        else
                        {
                            itemL.quantity = 0;
                            breakNum = (int)(breakNum - itemInLocs[i].quantity);
                            requiredAmount = requiredAmount - ((int)itemInLocs[i].quantity / (int)unit.unitValue);
                            entity.SaveChanges();
                        }
                        if (breakNum == 0)
                            break;
                    }
                    if (itemUnitId == lowerUnit.itemUnitId)
                        return dic;
                    if (breakNum != 0)
                    {
                        dic = new Dictionary<string, int>();
                        dic = checkLowerUnit(lowerUnit.itemUnitId, branchId, breakNum, userId);
                        //var item = (from  s in entity.sections where s.branchId == branchId
                        //            join l in entity.locations on s.sectionId equals l.sectionId
                        //                   join il in entity.itemsLocations on l.locationId equals il.locationId
                        //                   where il.itemUnitId == lowerUnit.itemUnitId
                        //                   select new
                        //                   {
                        //                       il.itemsLocId,
                        //                   }).FirstOrDefault();
                        //if (item != null)
                        //{
                        //    var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                        //    //itemloc.quantity = (int)remainQuantity;
                        //    itemloc.quantity = dic["remainQuantity"];
                        //    entity.SaveChanges();
                        //}
                        //else
                        //{
                        //    var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();
                        //   // if (locations.Count > 0)
                        //   // {
                        //        int locationId = dic["locationId"];
                        //    if (locationId == 0 && locations.Count > 1)
                        //        locationId = locations[0].locationId; // free zoon
                        //    //if (locations.Count > 1)
                        //    //    locationId = locations[1].locationId;
                        //    //else
                        //    //    locationId = locations[0].locationId;
                        //    itemsLocations itemL = new itemsLocations();
                        //        itemL.itemUnitId = itemUnitId;
                        //        itemL.locationId = locationId;
                        //        itemL.quantity =  dic["remainQuantity"];
                        //        itemL.createDate = cc.AddOffsetTodate(DateTime.Now);
                        //        itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                        //        itemL.createUserId = userId;
                        //        itemL.updateUserId = userId;

                        //        entity.itemsLocations.Add(itemL);
                        //        entity.SaveChanges();

                        //}
                        //remainQuantity = (int)newQuant - firstRequir;
                        //return (int)remainQuantity;

                        dic["remainQuantity"] = (int)newQuant - firstRequir;
                        dic["requiredQuantity"] = breakNum;
                        return dic;
                    }
                }
            }
            return dic;
        }

        [HttpPost]
        [Route("getAmountInBranch")]
        public string getAmountInBranch(string token)
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
                int itemUnitId = 0;
                int branchId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemUnitId")
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

                   int amount = getBranchAmount(itemUnitId,branchId);
                    return TokenManager.GenerateToken(amount.ToString());
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }
            }          
        }


        public int getBranchAmount(int itemUnitId, int branchId,int invoiceId=0)
        {

            int amount = 0;
            amount += getItemUnitAmount(itemUnitId, branchId,invoiceId); // from bigger unit
            amount += getSmallItemUnitAmount(itemUnitId, branchId,invoiceId);
            
            return amount;
        }



        private int getItemUnitAmount(int itemUnitId, int branchId, int invoiceId = 0)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var searchPredicate = PredicateBuilder.New<itemsLocations>();
                if (invoiceId == 0)
                    searchPredicate = searchPredicate.And(x => x.invoiceId == null);//for available quantity
                else
                    searchPredicate = searchPredicate.And(x => x.invoiceId != null);//for reserved quantity

                var itemInLocs = (from b in entity.branches
                                  where b.branchId == branchId
                                  join s in entity.sections on b.branchId equals s.branchId
                                  join l in entity.locations on s.sectionId equals l.sectionId
                                  join il in entity.itemsLocations.Where(searchPredicate) on l.locationId equals il.locationId
                                  where il.itemUnitId == itemUnitId && il.quantity > 0 
                                  select new
                                  {
                                      il.itemsLocId,
                                      il.quantity,
                                      il.itemUnitId,
                                      il.locationId,
                                      s.sectionId,
                                  }).ToList();
                for (int i = 0; i < itemInLocs.Count; i++)
                {
                    amount += (int)itemInLocs[i].quantity;
                }

                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (upperUnit == null)
                    return amount;
                if ((upperUnit != null && itemUnitId == upperUnit.itemUnitId))
                    return amount;
                if (upperUnit != null)
                    amount += (int)upperUnit.unitValue * getItemUnitAmount(upperUnit.itemUnitId, branchId,invoiceId);

                return amount;
            }
        }
        private int getSmallItemUnitAmount(int itemUnitId, int branchId,int invoiceId=0)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var searchPredicate = PredicateBuilder.New<itemsLocations>();
                if (invoiceId == 0)
                    searchPredicate = searchPredicate.And(x => x.invoiceId == null);//for available quantity
                else
                    searchPredicate = searchPredicate.And(x => x.invoiceId != null);//for reserved quantity

                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.subUnitId, x.unitId, x.unitValue, x.itemId }).FirstOrDefault();

                var smallUnit = entity.itemsUnits.Where(x => x.unitId == unit.subUnitId && x.itemId == unit.itemId).Select(x => new { x.itemUnitId }).FirstOrDefault();
                if (smallUnit == null || smallUnit.itemUnitId == itemUnitId)
                {
                    return 0;
                }
                else
                {
                    var itemInLocs = (from b in entity.branches
                                      where b.branchId == branchId
                                      join s in entity.sections on b.branchId equals s.branchId
                                      join l in entity.locations on s.sectionId equals l.sectionId
                                      join il in entity.itemsLocations.Where(searchPredicate) on l.locationId equals il.locationId
                                      where il.itemUnitId == smallUnit.itemUnitId && il.quantity > 0 
                                      select new
                                      {
                                          il.itemsLocId,
                                          il.quantity,
                                          il.itemUnitId,
                                          il.locationId,
                                          s.sectionId,
                                      }).ToList();
                    for (int i = 0; i < itemInLocs.Count; i++)
                    {
                        amount += (int)itemInLocs[i].quantity;
                    }
                    if (unit.unitValue != 0)
                        amount = amount / (int)unit.unitValue;

                    else
                        amount += getSmallItemUnitAmount(smallUnit.itemUnitId, branchId,invoiceId) / (int)unit.unitValue;

                    return amount;
                }
            }
        }
        private int getItemAmount(int itemUnitId, int branchId)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemInLocs = (from b in entity.branches
                                  where b.branchId == branchId
                                  join s in entity.sections on b.branchId equals s.branchId
                                  join l in entity.locations on s.sectionId equals l.sectionId
                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                  where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null
                                  select new
                                  {
                                      il.itemsLocId,
                                      il.quantity,
                                      il.itemUnitId,
                                      il.locationId,
                                      s.sectionId,
                                  }).ToList();
                for (int i = 0; i < itemInLocs.Count; i++)
                {
                    amount += (int)itemInLocs[i].quantity;
                }

                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if ((upperUnit != null && itemUnitId == upperUnit.itemUnitId) || upperUnit == null)
                    return amount;
                if (upperUnit != null)
                    amount += (int)upperUnit.unitValue * getItemUnitAmount(upperUnit.itemUnitId, branchId);

                return amount;
            }
        }
        public int getAllItemAmount(int itemUnitId, int branchId)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemInLocs = (from b in entity.branches
                                  where b.branchId == branchId
                                  join s in entity.sections on b.branchId equals s.branchId
                                  join l in entity.locations on s.sectionId equals l.sectionId
                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                  where il.itemUnitId == itemUnitId && il.quantity > 0
                                  select new
                                  {
                                      il.itemsLocId,
                                      il.quantity,
                                      il.itemUnitId,
                                      il.locationId,
                                      s.sectionId,
                                  }).ToList();
                for (int i = 0; i < itemInLocs.Count; i++)
                {
                    amount += (int)itemInLocs[i].quantity;
                }

                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if ((upperUnit != null && itemUnitId == upperUnit.itemUnitId) || upperUnit == null)
                    return amount;
                if (upperUnit != null)
                    amount += (int)upperUnit.unitValue * getItemUnitAmount(upperUnit.itemUnitId, branchId);

                return amount;
            }
        }
        private int getLevelItemUnitAmount(int itemUnitId, int topLevelUnit, int branchId)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemInLocs = (from b in entity.branches
                                  where b.branchId == branchId
                                  join s in entity.sections on b.branchId equals s.branchId
                                  join l in entity.locations on s.sectionId equals l.sectionId
                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                  where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null
                                  select new
                                  {
                                      il.itemsLocId,
                                      il.quantity,
                                      il.itemUnitId,
                                      il.locationId,
                                      s.sectionId,
                                  }).ToList();
                for (int i = 0; i < itemInLocs.Count; i++)
                {
                    amount += (int)itemInLocs[i].quantity;
                }

                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if ((upperUnit != null && itemUnitId == upperUnit.itemUnitId) || upperUnit == null)
                    return amount;
                if (upperUnit != null && upperUnit.itemUnitId != topLevelUnit)
                    amount += (int)upperUnit.unitValue * getLevelItemUnitAmount(upperUnit.itemUnitId, topLevelUnit, branchId);

                return amount;
            }
        }


        [HttpPost]
        [Route("getUnitAmount")]
        public string getUnitAmount(string token)
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

                int itemUnitId = 0;
                int branchId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemUnitId")
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
                    var amount =  getUnitAmount(branchId,itemUnitId);
                    return TokenManager.GenerateToken(amount.ToString());
                    //using (incposdbEntities entity = new incposdbEntities())
                    //{
                    //    var amount = (from b in entity.branches
                    //                  where b.branchId == branchId
                    //                  join s in entity.sections on b.branchId equals s.branchId
                    //                  join l in entity.locations on s.sectionId equals l.sectionId
                    //                  join il in entity.itemsLocations on l.locationId equals il.locationId
                    //                  where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null
                    //                  select new
                    //                  {
                    //                      il.itemsLocId,
                    //                      il.quantity,
                    //                      il.itemUnitId,
                    //                      il.locationId,
                    //                      s.sectionId,
                    //                  }).ToList().Sum(x => x.quantity);
                    //    //   return (int)amount;
                    //    return TokenManager.GenerateToken(amount.ToString());
                    //}

                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }

            }
           
        }

        private long getUnitAmount(int branchId, int itemUnitId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                long amount =(long) (from  s in entity.sections.Where(x =>  x.branches.branchId == branchId) 
                              join l in entity.locations on s.sectionId equals l.sectionId
                              join il in entity.itemsLocations on l.locationId equals il.locationId
                              where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null
                              select new
                              {
                                  il.itemsLocId,
                                  il.quantity,
                                  il.itemUnitId,
                                  il.locationId,
                                  s.sectionId,
                              }).ToList().Sum(x => x.quantity);

              
                return (long)amount;
            }
        }
        [HttpPost]
        [Route("getAmountInLocation")]
        public string getAmountInLocation(string token)
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

                int itemUnitId = 0;
                int locationId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);

                    }
                    else if (c.Type == "locationId")
                    {
                        locationId = int.Parse(c.Value);

                    }

                }

                try
                {
                    int amount = getAmountInLocation(locationId, itemUnitId);
                    return TokenManager.GenerateToken(amount.ToString());
                    //using (incposdbEntities entity = new incposdbEntities())
                    //{
                    //    var amount = (from l in entity.locations
                    //                  where l.locationId == locationId
                    //                  join il in entity.itemsLocations on l.locationId equals il.locationId
                    //                  where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null
                    //                  select new
                    //                  {
                    //                      il.itemsLocId,
                    //                      il.quantity,
                    //                  }).ToList().Sum(x => x.quantity);

                    //    return TokenManager.GenerateToken(amount.ToString());
                    //}

                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }

            }

            
        }

        private int getAmountInLocation(int locationId,int itemUnitId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var amount = (from l in entity.locations
                              where l.locationId == locationId
                              join il in entity.itemsLocations on l.locationId equals il.locationId
                              where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null
                              select new
                              {
                                  il.itemsLocId,
                                  il.quantity,
                              }).ToList().Sum(x => x.quantity);

                return (int)amount;
            }
        }

        [HttpPost]
        [Route("returnInvoice")]
        public string returnInvoice(string token)
        {
            //string string itemLocationObject, int branchId, int userId
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

                int branchId = 0;
                int userId = 0;

                List<itemsTransfer> newObject = new List<itemsTransfer>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }


                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }


                }

                if (newObject != null)
                {
                    try
                    {

                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var freeZoneLocation = (from s in entity.sections.Where(x => x.branchId == branchId && x.isFreeZone == 1)
                                                    join l in entity.locations on s.sectionId equals l.sectionId
                                                    select l.locationId).SingleOrDefault();
                            foreach (itemsTransfer item in newObject)
                            {
                                decreaseItemQuantity(item.itemUnitId.Value, freeZoneLocation, (int)item.quantity, userId);
                            }
                        }
                        // return Ok(1);
                        return TokenManager.GenerateToken("1");
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
        }
        [HttpPost]
        [Route("destroyItem")]
        public string destroyItem(string token)
        {
            //string itemLocationObject, int userId
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

                int userId = 0;

                List<itemsLocations> newObject = new List<itemsLocations>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsLocations>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }


                }

                if (newObject != null)
                {
                    try
                    {

                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            foreach (itemsLocations item in newObject)
                            {
                                itemsLocations itemL = new itemsLocations();

                                itemL = entity.itemsLocations.Find(item.itemsLocId);
                                itemL.quantity -= item.quantity;
                                itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                itemL.updateUserId = userId;
                            }
                            entity.SaveChanges();
                        }

                        // return Ok(1);
                        return TokenManager.GenerateToken("1");
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
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //itemLocationObject = itemLocationObject.Replace("\\", string.Empty);
            //itemLocationObject = itemLocationObject.Trim('"');

            //if (valid)
            //{
            //    List<itemsLocations> itemList = JsonConvert.DeserializeObject<List<itemsLocations>>(itemLocationObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        foreach (itemsLocations item in itemList)
            //        {
            //            itemsLocations itemL = new itemsLocations();

            //            itemL = entity.itemsLocations.Find(item.itemsLocId);
            //            itemL.quantity -= item.quantity;
            //            itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
            //            itemL.updateUserId = userId;
            //        }
            //        entity.SaveChanges();
            //    }
            //}
            //return Ok(1);
        }
        private void decreaseItemQuantity(int itemUnitId, int locationId, int quantity, int userId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemUnit = (from il in entity.itemsLocations
                                where il.itemUnitId == itemUnitId && il.locationId == locationId && il.invoiceId == null
                                select new { il.itemsLocId }
                                ).FirstOrDefault();
                itemsLocations itemL = new itemsLocations();

                if (itemUnit != null)
                {
                    itemL = entity.itemsLocations.Find(itemUnit.itemsLocId);
                    itemL.quantity -= quantity;
                    itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                    itemL.updateUserId = userId;
                    entity.SaveChanges();
                }
            }
        }



        [HttpPost]
        [Route("decreaseItemLocationQuantity")]
        public string decreaseItemLocationQuantity(string token)
        {

            //int itemLocId, int quantity, int userId, string objectName, string notificationObj
            //string itemLocationObject, int userId
            string message = "";

          token = TokenManager.readToken(HttpContext.Current.Request); 
 var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {



                int itemLocId = 0;
                int quantity = 0;
                int userId = 0;

                string objectName = "";
                string notificationObj = "";

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemLocId")
                    {
                        itemLocId = int.Parse(c.Value);

                    }
                    else if (c.Type == "quantity")
                    {
                        quantity = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                    else if (c.Type == "objectName")
                    {
                        objectName = c.Value;

                    }
                    else if (c.Type == "notificationObj")
                    {
                        notificationObj = c.Value;

                    }



                }


                try
                {



                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        itemsLocations itemL = new itemsLocations();

                        itemL = entity.itemsLocations.Find(itemLocId);
                        itemL.quantity -= quantity;
                        itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                        itemL.updateUserId = userId;
                        entity.SaveChanges();
                        if (objectName != "")
                        {
                            var branchId = (from l in entity.itemsLocations
                                            where l.itemsLocId == itemLocId
                                            select l.locations.branchId).Single();
                            bool isExcedded = isExceddMinQuantity((int)itemL.itemUnitId, (int)branchId, userId);
                            if (isExcedded == true) //add notification
                            {
                                var itemId = entity.itemsUnits.Where(x => x.itemUnitId == itemL.itemUnitId).Select(x => x.itemId).Single();
                                var itemV = entity.items.Find(itemId);
                                notificationController.addNotifications(objectName, notificationObj, (int)branchId, itemV.name);
                            }
                        }

                        return TokenManager.GenerateToken("1");
                    }
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }

            } 
        }

        public void decreaseItemLocationQuantity(int itemLocId, int quantity, int userId, string objectName, string notificationObj)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                itemsLocations itemL = new itemsLocations();

                itemL = entity.itemsLocations.Find(itemLocId);
                itemL.quantity -= quantity;
                itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                itemL.updateUserId = userId;
                entity.SaveChanges();
                if (objectName != "")
                {
                    var branchId = (from l in entity.itemsLocations
                                    where l.itemsLocId == itemLocId
                                    select l.locations.branchId).Single();
                    bool isExcedded = isExceddMinQuantity((int)itemL.itemUnitId, (int)branchId, userId);
                    if (isExcedded == true) //add notification
                    {
                        var itemId = entity.itemsUnits.Where(x => x.itemUnitId == itemL.itemUnitId).Select(x => x.itemId).Single();
                        var itemV = entity.items.Find(itemId);
                        notificationController.addNotifications(objectName, notificationObj, (int)branchId, itemV.name);
                    }
                }
            }
        }

        public bool isExceddMinQuantity(int itemUnitId, int branchId, int userId)
        {
            bool isExcedded = false;
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemId = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => x.itemId).Single();
                    var item = entity.items.Find(itemId);
                    int minUnitId = (int)item.minUnitId;
                    int minQuantity = (int)item.min;
                    if (minQuantity == 0)
                        return false;
                    var minUnit = entity.itemsUnits.Where(x => x.itemId == itemId && x.unitId == minUnitId).FirstOrDefault();
                    if (minUnit == null)
                        isExcedded = false;
                    else
                    {
                        int itemUnitQuantity = getItemAmount(minUnit.itemUnitId, branchId);
                        if (itemUnitQuantity <= minQuantity)
                        {
                            isExcedded = true;
                        }
                        if (isExcedded == false)
                        {
                            int smallestItemUnit = entity.itemsUnits.Where(x => x.itemId == itemId && x.subUnitId == x.unitId).Select(x => x.itemUnitId).Single();
                            int smallUnitQuantity = getLevelItemUnitAmount(smallestItemUnit, minUnit.itemUnitId, branchId);
                            int unitValue = itemsUnitsController.getLargeUnitConversionQuan(smallestItemUnit, minUnit.itemUnitId);
                            int quantity = 0;
                            if (unitValue != 0)
                                quantity = smallUnitQuantity / unitValue;

                            quantity += itemUnitQuantity;
                            if (quantity <= minQuantity)
                                isExcedded = true;
                        }
                    }
                }
            }
            catch
            {
            }
            return isExcedded;
        }



        [HttpPost]
        [Route("decraseAmounts")]
        public string decraseAmounts(string token)
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

                int branchId = 0;
                int userId = 0;
                int mainInvId = 0;
                string objectName = "";
                string notificationObj = "";

                List<itemsTransfer> newObject = new List<itemsTransfer>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }
                    else if (c.Type == "mainInvId")
                    {
                        mainInvId = int.Parse(c.Value);
                    }
                    else if (c.Type == "objectName")
                    {
                        objectName = c.Value;

                    }
                    else if (c.Type == "notificationObj")
                    {
                        notificationObj = c.Value;

                    }

                }

                if (newObject != null)
                {
                    try
                    {

                        decraseAmounts(newObject, notificationObj, objectName,mainInvId, branchId,userId);
                        //using (incposdbEntities entity = new incposdbEntities())
                        //{
                        //    foreach (itemsTransfer item in newObject)
                        //    {
  
                        //        if (mainInvId == null || mainInvId == 0)
                        //        {
                        //            updateItemQuantity(item.itemUnitId.Value, branchId, (int)item.quantity, userId);
                        //        }
                        //        else
                        //        {
                        //            var itl = entity.itemsLocations.Where(x => x.invoiceId == mainInvId).ToList();
                        //            entity.itemsLocations.RemoveRange(itl);
                        //            entity.SaveChanges();
                        //        }
                        //        bool isExcedded = isExceddMinQuantity((int)item.itemUnitId, (int)branchId, userId);
                        //        if (isExcedded == true) //add notification
                        //        {
                        //            var itemId = entity.itemsUnits.Where(x => x.itemUnitId == item.itemUnitId).Select(x => x.itemId).Single();
                        //            var itemV = entity.items.Find(itemId);
                        //            notificationController.addNotifications(objectName, notificationObj, (int)branchId, itemV.name);
                        //        }
                        //    }
                        //}
                        //  return true;

                        return TokenManager.GenerateToken("1");
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
        }

        public void decraseAmounts(List<itemsTransfer> newObject,string notificationObj,string objectName,int? mainInvId,int branchId,int userId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                foreach (itemsTransfer item in newObject)
                {

                    if (mainInvId == null || mainInvId == 0)
                    {
                        updateItemQuantity(item.itemUnitId.Value, branchId, (int)item.quantity, userId);
                    }
                    else
                    {
                        var itl = entity.itemsLocations.Where(x => x.invoiceId == mainInvId).ToList();
                        entity.itemsLocations.RemoveRange(itl);
                        entity.SaveChanges();
                    }
                    bool isExcedded = isExceddMinQuantity((int)item.itemUnitId, (int)branchId, userId);
                    if (isExcedded == true) //add notification
                    {
                        var itemId = entity.itemsUnits.Where(x => x.itemUnitId == item.itemUnitId).Select(x => x.itemId).Single();
                        var itemV = entity.items.Find(itemId);
                        notificationController.addNotifications(objectName, notificationObj, (int)branchId, itemV.name);
                    }
                }
            }
        }
        [HttpPost]
        [Route("reserveItems")]
        public string reserveItems(string token)
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
                int invoiceId = 0;
                int branchId = 0;
                int userId = 0;

                List<itemsTransfer> newObject = new List<itemsTransfer>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "invoiceId")
                    {
                        invoiceId = int.Parse(c.Value);

                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                }

                if (newObject != null)
                {
                    try
                    {

                        reserveItems(newObject,invoiceId,branchId,userId);
                        return TokenManager.GenerateToken("1");
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
        }

        public void reserveItems(List<itemsTransfer> newObject,int invoiceId, int branchId,int userId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                foreach (itemsTransfer item in newObject)
                {
                    lockItem(item.itemUnitId.Value, invoiceId, branchId, (int)item.quantity, userId);
                }
            }
        }
        [HttpPost]
        [Route("reReserveItems")]
        public string reReserveItems(string token)
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
                int invoiceId = 0;
                int branchId = 0;
                int userId = 0;

                List<ItemTransferModel> newObject = new List<ItemTransferModel>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "invoiceId")
                    {
                        invoiceId = int.Parse(c.Value);

                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                }

                if (newObject != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            foreach (ItemTransferModel item in newObject)
                            {
                                if (item.newLocked > item.lockedQuantity)
                                {
                                    int lockedQuantity = (int)(item.newLocked - item.lockedQuantity);
                                    lockItem(item.itemUnitId.Value, invoiceId, branchId, lockedQuantity, userId);
                                }
                                else if(item.newLocked < item.lockedQuantity)
                                {
                                    int unreserveQnt = (int)(item.lockedQuantity - item.newLocked);
                                    unlockQuantity(invoiceId, (int)item.itemUnitId, unreserveQnt);
                                }
                            }
                        }
                        // return true;

                        return TokenManager.GenerateToken("1");
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
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //itemLocationObject = itemLocationObject.Replace("\\", string.Empty);
            //itemLocationObject = itemLocationObject.Trim('"');

            //if (valid)
            //{
            //    List<itemsTransfer> itemList = JsonConvert.DeserializeObject<List<itemsTransfer>>(itemLocationObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });

            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        foreach (itemsTransfer item in itemList)
            //        {
            //            lockItem(item.itemUnitId.Value, invoiceId, branchId, (int)item.quantity, userId);
            //        }
            //    }
            //    return true;
            //}
            //else
            //    return false;
        }
        [HttpPost]
        [Route("lockItem")]
        public string lockItem(string token)
        {
            //int itemUnitId, int invoiceId, int branchId, int requiredAmount, int userId


            string message = "";

          token = TokenManager.readToken(HttpContext.Current.Request); 
 var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int itemUnitId = 0;
                int invoiceId = 0;
                int branchId = 0;
                int userId = 0;

                int requiredAmount = 0;
                int locationId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invoiceId")
                    {
                        invoiceId = int.Parse(c.Value);

                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                    else if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);

                    }
                    else if (c.Type == "requiredAmount")
                    {
                        requiredAmount = int.Parse(c.Value);

                    }


                }

                //if (newObject != null)
                //{
                try
                {


                    Dictionary<string, int> dic = new Dictionary<string, int>();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemInLocs = (from s in entity.sections
                                          where s.branchId == branchId
                                          join l in entity.locations on s.sectionId equals l.sectionId
                                          join il in entity.itemsLocations on l.locationId equals il.locationId
                                          where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null
                                          select new
                                          {
                                              il.itemsLocId,
                                              il.quantity,
                                              il.itemUnitId,
                                              il.locationId,
                                              il.updateDate,
                                              s.sectionId,
                                          }).ToList().OrderBy(x => x.updateDate).ToList();
                        for (int i = 0; i < itemInLocs.Count; i++)
                        {
                            int availableAmount = (int)itemInLocs[i].quantity;
                            int lockedAmount = 0;
                            var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                            itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            if (availableAmount >= requiredAmount)
                            {
                                itemL.quantity = availableAmount - requiredAmount;
                                lockedAmount = requiredAmount;
                                requiredAmount = 0;
                                entity.SaveChanges();
                            }
                            else if (availableAmount > 0)
                            {
                                itemL.quantity = 0;
                                requiredAmount = requiredAmount - availableAmount;
                                lockedAmount = availableAmount;
                                entity.SaveChanges();
                            }
                            if (lockedAmount > 0)
                                increaseLockedItem((int)itemInLocs[i].itemUnitId, (int)itemInLocs[i].locationId, lockedAmount, invoiceId, userId);

                            if (requiredAmount == 0)
                                // return Ok(3);
                                return TokenManager.GenerateToken("3");
                        }

                        if (requiredAmount != 0)
                        {
                            dic = lockUpperUnit(itemUnitId, branchId, requiredAmount, userId);

                            //var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                            //var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();
                            if (dic["remainQuantity"] > 0)
                            {
                                var item = (from il in entity.itemsLocations
                                            where il.itemUnitId == itemUnitId && il.invoiceId == null
                                            join l in entity.locations on il.locationId equals l.locationId
                                            join s in entity.sections on l.sectionId equals s.sectionId
                                            where s.branchId == branchId
                                            select new
                                            {
                                                il.itemsLocId,
                                            }).FirstOrDefault();
                                if (item != null)
                                {
                                    var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                                    itemloc.quantity += dic["remainQuantity"];
                                    entity.SaveChanges();
                                }
                                else
                                {
                                    var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();
                                    locationId = dic["locationId"];
                                    if ((locationId == 0 && locationId == null) && locations.Count > 1)
                                        locationId = locations[0].locationId; // free zoon
                                    itemsLocations itemL = new itemsLocations();
                                    itemL.itemUnitId = itemUnitId;
                                    itemL.locationId = locationId;
                                    itemL.quantity = dic["remainQuantity"];
                                    itemL.createDate = cc.AddOffsetTodate(DateTime.Now);
                                    itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                    itemL.createUserId = userId;
                                    itemL.updateUserId = userId;
                                    itemL.invoiceId = null;

                                    entity.itemsLocations.Add(itemL);
                                    entity.SaveChanges();
                                }
                            }
                            // return Ok(dic["lockedQuantity"] +":"+ dic["remainQuantity"]+ ":"+ dic["requiredQuantity"]);
                            // reserve items
                            if (dic["lockedQuantity"] > 0)
                            {
                                int lockedQuantity = dic["lockedQuantity"];
                                if (lockedQuantity > requiredAmount)
                                    lockedQuantity = requiredAmount;
                                var item = (from il in entity.itemsLocations
                                            where il.itemUnitId == itemUnitId && il.invoiceId == invoiceId
                                            select new
                                            {
                                                il.itemsLocId,
                                            }).FirstOrDefault();
                                if (item != null)
                                {
                                    var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                                    locationId = (int)itemloc.locationId;
                                }
                                else
                                {
                                    var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();
                                    locationId = dic["locationId"];
                                    if (locationId == 0 && locations.Count > 1)
                                        locationId = locations[0].locationId; // free zoon
                                }

                                increaseLockedItem(itemUnitId, locationId, lockedQuantity, invoiceId, userId);
                            }
                            //return Ok(dic["requiredQuantity"]);
                            if (dic["requiredQuantity"] > 0)
                            {
                                dic = lockLowerUnit(itemUnitId, branchId, dic["requiredQuantity"], userId);
                                if (dic["lockedQuantity"] > 0)
                                {
                                    var item = (from il in entity.itemsLocations
                                                where il.itemUnitId == itemUnitId && il.invoiceId == invoiceId
                                                join l in entity.locations on il.locationId equals l.locationId
                                                join s in entity.sections on l.sectionId equals s.sectionId
                                                where s.branchId == branchId
                                                select new
                                                {
                                                    il.itemsLocId,
                                                }).FirstOrDefault();
                                    if (item != null)
                                    {
                                        var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                                        locationId = (int)itemloc.locationId;
                                    }
                                    else
                                    {
                                        var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();
                                        locationId = dic["locationId"];
                                        if (locationId == 0 && locations.Count > 1)
                                            locationId = locations[0].locationId; // free zoon
                                    }
                                    increaseLockedItem(itemUnitId, locationId, dic["lockedQuantity"], invoiceId, userId);
                                }
                            }

                        }
                    }
                    //  return Ok(2);
                    return TokenManager.GenerateToken("2");
                }

                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }


            }

        }



        public int lockItem(int itemUnitId, int invoiceId, int branchId, int requiredAmount, int userId)
        {

            int locationId = 0;
            Dictionary<string, int> dic = new Dictionary<string, int>();
            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemInLocs = (from s in entity.sections
                                  where s.branchId == branchId
                                  join l in entity.locations on s.sectionId equals l.sectionId
                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                  where il.itemUnitId == itemUnitId && il.quantity > 0 && il.invoiceId == null
                                  select new
                                  {
                                      il.itemsLocId,
                                      il.quantity,
                                      il.itemUnitId,
                                      il.locationId,
                                      il.updateDate,
                                      s.sectionId,
                                  }).ToList().OrderBy(x => x.updateDate).ToList();
                for (int i = 0; i < itemInLocs.Count; i++)
                {
                    int availableAmount = (int)itemInLocs[i].quantity;
                    int lockedAmount = 0;
                    var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                    itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                    if (availableAmount >= requiredAmount)
                    {
                        itemL.quantity = availableAmount - requiredAmount;
                        lockedAmount = requiredAmount;
                        requiredAmount = 0;
                        entity.SaveChanges();
                    }
                    else if (availableAmount > 0)
                    {
                        itemL.quantity = 0;
                        requiredAmount = requiredAmount - availableAmount;
                        lockedAmount = availableAmount;
                        entity.SaveChanges();
                    }
                    if (lockedAmount > 0)
                        increaseLockedItem((int)itemInLocs[i].itemUnitId, (int)itemInLocs[i].locationId, lockedAmount, invoiceId, userId);

                    if (requiredAmount == 0)
                        return (3);
                }

                if (requiredAmount != 0)
                {
                    dic = lockUpperUnit(itemUnitId, branchId, requiredAmount, userId);

                    //var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                    //var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();
                    if (dic["remainQuantity"] > 0)
                    {
                        var item = (from il in entity.itemsLocations
                                    where il.itemUnitId == itemUnitId && il.invoiceId == null
                                    join l in entity.locations on il.locationId equals l.locationId
                                    join s in entity.sections on l.sectionId equals s.sectionId
                                    where s.branchId == branchId
                                    select new
                                    {
                                        il.itemsLocId,
                                    }).FirstOrDefault();
                        if (item != null)
                        {
                            var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                            itemloc.quantity += dic["remainQuantity"];
                            entity.SaveChanges();
                        }
                        else
                        {
                            var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();
                            locationId = dic["locationId"];
                            if ((locationId == 0 && locationId == null) && locations.Count > 1)
                                locationId = locations[0].locationId; // free zoon
                            itemsLocations itemL = new itemsLocations();
                            itemL.itemUnitId = itemUnitId;
                            itemL.locationId = locationId;
                            itemL.quantity = dic["remainQuantity"];
                            itemL.createDate = cc.AddOffsetTodate(DateTime.Now);
                            itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            itemL.createUserId = userId;
                            itemL.updateUserId = userId;
                            itemL.invoiceId = null;

                            entity.itemsLocations.Add(itemL);
                            entity.SaveChanges();
                        }
                    }
                    // return Ok(dic["lockedQuantity"] +":"+ dic["remainQuantity"]+ ":"+ dic["requiredQuantity"]);
                    // reserve items
                    if (dic["lockedQuantity"] > 0)
                    {
                        int lockedQuantity = dic["lockedQuantity"];
                        if (lockedQuantity > requiredAmount)
                            lockedQuantity = requiredAmount;
                        var item = (from il in entity.itemsLocations
                                    where il.itemUnitId == itemUnitId && il.invoiceId == invoiceId
                                    select new
                                    {
                                        il.itemsLocId,
                                    }).FirstOrDefault();
                        if (item != null)
                        {
                            var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                            locationId = (int)itemloc.locationId;
                        }
                        else
                        {
                            var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();
                            locationId = dic["locationId"];
                            if (locationId == 0 && locations.Count > 1)
                                locationId = locations[0].locationId; // free zoon
                        }

                        increaseLockedItem(itemUnitId, locationId, lockedQuantity, invoiceId, userId);
                    }
                    //return Ok(dic["requiredQuantity"]);
                    if (dic["requiredQuantity"] > 0)
                    {
                        dic = lockLowerUnit(itemUnitId, branchId, dic["requiredQuantity"], userId);
                        if (dic["lockedQuantity"] > 0)
                        {
                            var item = (from il in entity.itemsLocations
                                        where il.itemUnitId == itemUnitId && il.invoiceId == invoiceId
                                        join l in entity.locations on il.locationId equals l.locationId
                                        join s in entity.sections on l.sectionId equals s.sectionId
                                        where s.branchId == branchId
                                        select new
                                        {
                                            il.itemsLocId,
                                        }).FirstOrDefault();
                            if (item != null)
                            {
                                var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                                locationId = (int)itemloc.locationId;
                            }
                            else
                            {
                                var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();
                                locationId = dic["locationId"];
                                if (locationId == 0 && locations.Count > 1)
                                    locationId = locations[0].locationId; // free zoon
                            }
                            increaseLockedItem(itemUnitId, locationId, dic["lockedQuantity"], invoiceId, userId);
                        }
                    }

                }
            }
            return (2);
        }


        private Dictionary<string, int> lockLowerUnit(int itemUnitId, int branchId, int requiredAmount, int userId)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            int remainQuantity = 0;
            int firstRequir = requiredAmount;
            int lockedQuantity = 0;
            decimal newQuant = 0;
            dic.Add("lockedQuantity", 0);
            dic.Add("remainQuantity", 0);
            dic.Add("locationId", 0);

            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId, x.subUnitId, x.unitValue }).FirstOrDefault();
                var lowerUnit = entity.itemsUnits.Where(x => x.unitId == unit.subUnitId && x.itemId == unit.itemId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (lowerUnit != null && lowerUnit.itemUnitId != itemUnitId)
                {
                    decimal unitValue = (decimal)unit.unitValue;
                    int breakNum = (int)requiredAmount * (int)unitValue;
                    newQuant = (decimal)Math.Ceiling(breakNum / (decimal)unit.unitValue);
                    var itemInLocs = (from b in entity.branches
                                      where b.branchId == branchId
                                      join s in entity.sections on b.branchId equals s.branchId
                                      join l in entity.locations on s.sectionId equals l.sectionId
                                      join il in entity.itemsLocations on l.locationId equals il.locationId
                                      where il.itemUnitId == lowerUnit.itemUnitId && il.quantity > 0 && il.invoiceId == null
                                      select new
                                      {
                                          il.itemsLocId,
                                          il.quantity,
                                          il.itemUnitId,
                                          il.locationId,
                                          il.updateDate,
                                          s.sectionId,
                                      }).ToList().OrderBy(x => x.updateDate).ToList();

                    for (int i = 0; i < itemInLocs.Count; i++)
                    {

                        var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);

                        if (breakNum <= (int)itemInLocs[i].quantity)
                        {
                            itemL.quantity = itemInLocs[i].quantity - breakNum;
                            entity.SaveChanges();
                            remainQuantity = (int)newQuant - firstRequir;
                            requiredAmount = 0;
                            lockedQuantity = breakNum;

                            dic["remainQuantity"] = remainQuantity;
                            dic["locationId"] = (int)itemInLocs[i].locationId;
                            dic["lockedQuantity"] += lockedQuantity / (int)unit.unitValue;
                            return dic;
                        }
                        else
                        {
                            itemL.quantity = 0;
                            breakNum = (int)(breakNum - itemInLocs[i].quantity);
                            requiredAmount = requiredAmount - ((int)itemInLocs[i].quantity / (int)unit.unitValue);
                            lockedQuantity += (int)itemInLocs[i].quantity / (int)unit.unitValue;
                            entity.SaveChanges();
                            dic["lockedQuantity"] += lockedQuantity;
                        }
                        if (breakNum == 0)
                            break;
                    }
                    if (itemUnitId == lowerUnit.itemUnitId)
                        return dic;
                    if (breakNum != 0)
                    {
                        dic = new Dictionary<string, int>();
                        dic = lockLowerUnit(lowerUnit.itemUnitId, branchId, breakNum, userId);

                        dic["remainQuantity"] = (int)newQuant - firstRequir;
                        dic["requiredQuantity"] = breakNum;
                        dic["lockedQuantity"] += ((int)newQuant - firstRequir) / (int)unit.unitValue;
                        return dic;
                    }
                }
            }
            return dic;
        }
        private Dictionary<string, int> lockUpperUnit(int itemUnitId, int branchId, int requiredAmount, int userId)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            dic.Add("remainQuantity", 0);
            dic.Add("locationId", 0);
            dic.Add("requiredQuantity", 0);
            dic.Add("lockedQuantity", 0);
            dic.Add("isConsumed", 0);

            int remainQuantity = 0;
            int firstRequir = requiredAmount;
            decimal newQuant = 0;
            int lockedAmount = 0;
            int isConsumed = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId, x.unitValue }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (upperUnit != null && upperUnit.itemUnitId != itemUnitId)
                {
                    decimal unitValue = (decimal)upperUnit.unitValue;
                    int breakNum = (int)Math.Ceiling(requiredAmount / unitValue);
                    newQuant = (decimal)(breakNum * upperUnit.unitValue);
                    var itemInLocs = (from b in entity.branches
                                      where b.branchId == branchId
                                      join s in entity.sections on b.branchId equals s.branchId
                                      join l in entity.locations on s.sectionId equals l.sectionId
                                      join il in entity.itemsLocations on l.locationId equals il.locationId
                                      where il.itemUnitId == upperUnit.itemUnitId && il.quantity > 0 && il.invoiceId == null
                                      select new
                                      {
                                          il.itemsLocId,
                                          il.quantity,
                                          il.itemUnitId,
                                          il.locationId,
                                          il.updateDate,
                                          s.sectionId,
                                      }).ToList().OrderBy(x => x.updateDate).ToList();

                    for (int i = 0; i < itemInLocs.Count; i++)
                    {
                        dic["isConsumed"] = 1;
                        isConsumed = 1;
                        var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);

                        if (breakNum <= itemInLocs[i].quantity)
                        {
                            itemL.quantity = itemInLocs[i].quantity - breakNum;
                            entity.SaveChanges();
                            remainQuantity = (int)newQuant - firstRequir;

                            // lockedAmount += breakNum * (int)upperUnit.unitValue;
                            //lockedAmount = breakNum ;
                            lockedAmount = firstRequir;
                            requiredAmount = 0;
                            // return remainQuantity;
                            dic["remainQuantity"] = remainQuantity;
                            dic["locationId"] = (int)itemInLocs[i].locationId;
                            dic["requiredQuantity"] = 0;
                            //dic["lockedQuantity"] += lockedAmount * (int)upperUnit.unitValue;
                            dic["lockedQuantity"] += lockedAmount;

                            return dic;
                        }
                        else
                        {
                            itemL.quantity = 0;
                            breakNum = (int)(breakNum - itemInLocs[i].quantity);
                            lockedAmount += (int)itemInLocs[i].quantity;
                            // lockedAmount = (int)itemInLocs[i].quantity;
                            requiredAmount = requiredAmount - ((int)itemInLocs[i].quantity * (int)upperUnit.unitValue);
                            entity.SaveChanges();
                            dic["locationId"] = (int)itemInLocs[i].locationId;
                            dic["requiredQuantity"] = requiredAmount;
                            // dic["lockedQuantity"] += lockedAmount ;


                        }
                        if (breakNum == 0)
                            break;
                    }
                    if (breakNum != 0)
                    {
                        dic = new Dictionary<string, int>();
                        dic = lockUpperUnit(upperUnit.itemUnitId, branchId, breakNum, userId);


                        //if (dic["isConsumed"] == 1)
                        //{
                        int locationId = dic["locationId"];
                        if (locationId == 0)
                        {
                            var locations = entity.locations.Where(x => x.branchId == branchId && x.isActive == 1).Select(x => new { x.locationId }).OrderBy(x => x.locationId).ToList();

                            if (locationId == 0 && locations.Count >= 1)
                                locationId = locations[0].locationId; // free zoon
                        }
                        var item = (from s in entity.sections
                                    where s.branchId == branchId
                                    join l in entity.locations on s.sectionId equals l.sectionId
                                    join il in entity.itemsLocations on l.locationId equals il.locationId
                                    where il.itemUnitId == upperUnit.itemUnitId && il.invoiceId == null
                                    && il.locationId == locationId
                                    select new
                                    {
                                        il.itemsLocId,
                                    }).FirstOrDefault();
                        if (item != null)
                        {
                            var itemloc = entity.itemsLocations.Find(item.itemsLocId);
                            itemloc.quantity += dic["remainQuantity"];
                            entity.SaveChanges();
                        }
                        else
                        {

                            itemsLocations itemL = new itemsLocations();
                            itemL.itemUnitId = upperUnit.itemUnitId;
                            itemL.locationId = locationId;
                            itemL.quantity = dic["remainQuantity"];
                            itemL.createDate = cc.AddOffsetTodate(DateTime.Now);
                            itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            itemL.createUserId = userId;
                            itemL.updateUserId = userId;

                            entity.itemsLocations.Add(itemL);
                            entity.SaveChanges();

                        }
                        //}
                        //dic["remainQuantity"] = (int)newQuant - firstRequir;
                        // dic["lockedQuantity"] += ((int)newQuant - firstRequir) * (int)upperUnit.unitValue;
                        dic["locationId"] = locationId;
                        if (dic["lockedQuantity"] > 0)
                        {
                            isConsumed = 1;
                            //int locked = dic["lockedQuantity"] ;
                            //if(locked >= breakNum)
                            //    dic["lockedQuantity"] = (int)upperUnit.unitValue * lockedAmount + breakNum ;
                            //else
                            //    dic["lockedQuantity"] = (int)upperUnit.unitValue * lockedAmount + locked;
                            lockedAmount += dic["lockedQuantity"] * (int)upperUnit.unitValue;
                            dic["lockedQuantity"] = lockedAmount;
                        }
                        //else
                        //     dic["lockedQuantity"] = dic["lockedQuantity"] * (int)upperUnit.unitValue;
                        // isConsumed = dic["isConsumed"];
                        if (isConsumed == 0)
                        {
                            dic["requiredQuantity"] = requiredAmount;
                            dic["remainQuantity"] = 0;
                        }
                        else
                        {
                            dic["remainQuantity"] = (int)newQuant - firstRequir;
                            // dic["requiredQuantity"] = breakNum * (int)upperUnit.unitValue;
                            dic["requiredQuantity"] = dic["requiredQuantity"] * (int)upperUnit.unitValue;
                        }
                        return dic;
                    }
                }
                else
                {
                    dic["remainQuantity"] = 0;
                    dic["requiredQuantity"] = requiredAmount;
                    dic["locationId"] = 0;
                    dic["lockedQuantity"] = 0;
                    return dic;
                }
            }
            return dic;
        }
        private void increaseLockedItem(int itemUnitId, int locationId, int quantity, int invoiceId, int userId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemUnit = (from il in entity.itemsLocations
                                where il.itemUnitId == itemUnitId && il.locationId == locationId && il.invoiceId == invoiceId
                                select new { il.itemsLocId }
                                ).FirstOrDefault();
                itemsLocations itemL = new itemsLocations();
                if (itemUnit == null)//add item in new location
                {
                    itemL.itemUnitId = itemUnitId;
                    itemL.locationId = locationId;
                    itemL.quantity = quantity;
                    itemL.createDate = cc.AddOffsetTodate(DateTime.Now);
                    itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                    itemL.createUserId = userId;
                    itemL.updateUserId = userId;
                    itemL.invoiceId = invoiceId;

                    entity.itemsLocations.Add(itemL);
                }
                else
                {
                    itemL = entity.itemsLocations.Find(itemUnit.itemsLocId);
                    itemL.quantity += quantity;
                    itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                    itemL.updateUserId = userId;
                }
                entity.SaveChanges();
            }
        }

        [HttpPost]
        [Route("unitsConversion")]
        public string unitsConversion(string token)
        {
            string message = "";

            token = TokenManager.readToken(HttpContext.Current.Request); 
            var strP = TokenManager.GetPrincipal(token);
            //if (strP != "0") //invalid authorization
            //{
            //    return TokenManager.GenerateToken(strP);
            //}
            //else
            {
                #region params
                string Object = "";
                int branchId = 0;
                int fromItemUnit = 0;
                int toItemUnit = 0;
                int fromQuantity = 0;
                int toQuantity = 0;
                int userId = 0;

                itemsUnits newObject = new itemsUnits();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<itemsUnits>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }
                    else if (c.Type == "fromItemUnit")
                    {
                        fromItemUnit = int.Parse(c.Value);

                    }
                    else if (c.Type == "toItemUnit")
                    {
                        toItemUnit = int.Parse(c.Value);

                    }
                    else if (c.Type == "fromQuantity")
                    {
                        fromQuantity = int.Parse(c.Value);

                    }
                    else if (c.Type == "toQuantity")
                    {
                        toQuantity = int.Parse(c.Value);

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);

                    }
                   

                }
                #endregion

                try
                {
                    if (newObject != null)
                    {
                        //try
                        {
                            #region check amount in branch
                            long branchQuantity = getUnitAmount(branchId, fromItemUnit);

                            if (branchQuantity < fromQuantity)
                                return TokenManager.GenerateToken("-3");

                            #endregion

                            #region covert from unit (fromItemUnit) is bigger than the last (toItemUnit)
                            if (newObject.itemUnitId != 0)// covert from unit (fromItemUnit) is bigger than the last (toItemUnit)
                            {
                                using (incposdbEntities entity = new incposdbEntities())
                                {
                                    var itemInLocs = (from s in entity.sections.Where(x => x.branchId == branchId)
                                                      join l in entity.locations.Where(x => x.branchId == branchId) on s.sectionId equals l.sectionId
                                                      join il in entity.itemsLocations on l.locationId equals il.locationId
                                                      where il.itemUnitId == fromItemUnit && il.quantity > 0 && il.invoiceId == null
                                                      select new
                                                      {
                                                          il.itemsLocId,
                                                          il.quantity,
                                                          il.itemUnitId,
                                                          il.locationId,
                                                          s.sectionId,
                                                      }).ToList();

                                    int unitValue = getUnitValue(fromItemUnit, toItemUnit);

                                    for (int i = 0; i < itemInLocs.Count; i++)
                                    {
                                        int toQuant = 0;
                                        int availableAmount = (int)itemInLocs[i].quantity;
                                        var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                                        itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                        if (availableAmount >= fromQuantity)
                                        {
                                            itemL.quantity = availableAmount - fromQuantity;
                                            toQuant = fromQuantity * unitValue;
                                            fromQuantity = 0;
                                            entity.SaveChanges();
                                        }
                                        else if (availableAmount > 0)
                                        {
                                            itemL.quantity = 0;
                                            fromQuantity = fromQuantity - availableAmount;
                                            toQuant = availableAmount * unitValue;
                                            entity.SaveChanges();
                                        }

                                        increaseItemQuantity(toItemUnit, (int)itemInLocs[i].locationId, toQuant, userId);

                                        if (fromQuantity == 0)
                                            break;
                                    }
                                }
                                // return true;
                                // return TokenManager.GenerateToken("1");
                            }
                            #endregion
                            //#region from small to large
                            //else
                            //{
                            //    using (incposdbEntities entity = new incposdbEntities())
                            //    {
                            //        var itemInLocs = (from b in entity.branches
                            //                          where b.branchId == branchId
                            //                          join s in entity.sections on b.branchId equals s.branchId
                            //                          join l in entity.locations on s.sectionId equals l.sectionId
                            //                          join il in entity.itemsLocations on l.locationId equals il.locationId
                            //                          where il.itemUnitId == fromItemUnit && il.quantity > 0 && il.invoiceId == null
                            //                          select new
                            //                          {
                            //                              il.itemsLocId,
                            //                              il.quantity,
                            //                              il.itemUnitId,
                            //                              il.locationId,
                            //                              s.sectionId,
                            //                          }).ToList();
                            //        return toItemUnit + "d" + fromItemUnit;
                            //        int unitValue = getUnitValue(toItemUnit, fromItemUnit);
                            //        int i = 0;
                            //        for (i = 0; i < itemInLocs.Count; i++)
                            //        {
                            //            int availableAmount = (int)itemInLocs[i].quantity;
                            //            var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                            //            itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            //            if (availableAmount >= fromQuantity)
                            //            {
                            //                itemL.quantity = availableAmount - fromQuantity;
                            //                fromQuantity = 0;
                            //                entity.SaveChanges();
                            //            }
                            //            else if (availableAmount > 0)
                            //            {
                            //                itemL.quantity = 0;
                            //                fromQuantity = fromQuantity - availableAmount;
                            //                entity.SaveChanges();
                            //            }



                            //            if (fromQuantity == 0)
                            //                //  return true;
                            //                return TokenManager.GenerateToken("1");
                            //        }
                            //        increaseItemQuantity(toItemUnit, (int)itemInLocs[i].locationId, toQuantity, userId);
                            //        //  return true;
                            //        return TokenManager.GenerateToken("1");
                            //    }
                            //    #endregion
                            //}
                            return TokenManager.GenerateToken("1");


                        }



                    }
                    #region from small to large
                    else
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var itemInLocs = (from b in entity.branches
                                              where b.branchId == branchId
                                              join s in entity.sections on b.branchId equals s.branchId
                                              join l in entity.locations on s.sectionId equals l.sectionId
                                              join il in entity.itemsLocations on l.locationId equals il.locationId
                                              where il.itemUnitId == fromItemUnit && il.quantity > 0 && il.invoiceId == null
                                              select new
                                              {
                                                  il.itemsLocId,
                                                  il.quantity,
                                                  il.itemUnitId,
                                                  il.locationId,
                                                  s.sectionId,
                                              }).ToList();

                            int unitValue = getUnitValue(toItemUnit, fromItemUnit);
                            int i = 0;
                            for (i = 0; i < itemInLocs.Count; i++)
                            {
                                int availableAmount = (int)itemInLocs[i].quantity;
                                var itemL = entity.itemsLocations.Find(itemInLocs[i].itemsLocId);
                                itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                if (availableAmount >= fromQuantity)
                                {
                                    itemL.quantity = availableAmount - fromQuantity;
                                    fromQuantity = 0;
                                    entity.SaveChanges();
                                }
                                else if (availableAmount > 0)
                                {
                                    itemL.quantity = 0;
                                    fromQuantity = fromQuantity - availableAmount;
                                    entity.SaveChanges();
                                }



                                if (fromQuantity == 0)
                                    break;
                            }
                            increaseItemQuantity(toItemUnit, (int)itemInLocs[i].locationId, toQuantity, userId);
                            //  return true;
                            return TokenManager.GenerateToken("1");
                        }
                        #endregion
                    }
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }
            }

        }



        private int getUnitValue(int itemunitId, int smallestItemUnitId)
        {
            int unitValue = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemunitId).Select(x => new { x.subUnitId, x.unitId, x.unitValue, x.itemId }).FirstOrDefault();
                //if (unit.unitId == unit.subUnitId)
                //    return (int)unit.unitValue;

                int smallUnitId = entity.itemsUnits.Where(x => x.unitId == unit.subUnitId && x.itemId == unit.itemId).Select(x => x.itemUnitId).Single();
                unitValue = (int)unit.unitValue;
                if (itemunitId == smallestItemUnitId)
                    return unitValue;
                else
                {
                    unitValue = unitValue * getUnitValue(smallUnitId, smallestItemUnitId);
                }
            }
            return unitValue;
        }


        [HttpPost]
        [Route("getSpecificItemLocation")]
        public string getSpecificItemLocation(string token)
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
                int branchId = 0;

                string newObject = "";
                List<int> ids = new List<int>();
                List<string> strIds = new List<string>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemUnitsIds")
                    {
                        newObject = c.Value;
                        strIds = newObject.Split(',').ToList();
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }

                if (strIds != null)
                {
                    try
                    {
                        for (int i = 0; i < strIds.Count; i++)
                        {
                            if (!strIds[i].Equals(""))
                                ids.Add(int.Parse(strIds[i]));
                        }

                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var locList = (from b in entity.itemsLocations
                                           where b.quantity > 0 && b.invoiceId == null && ids.Contains((int)b.itemUnitId)
                                           join u in entity.itemsUnits on b.itemUnitId equals u.itemUnitId
                                           join i in entity.items on u.itemId equals i.itemId
                                           join l in entity.locations on b.locationId equals l.locationId
                                           join s in entity.sections on l.sectionId equals s.sectionId
                                           where s.branchId == branchId

                                           select new ItemLocationModel
                                           {
                                               createDate = b.createDate,
                                               createUserId = b.createUserId,
                                               endDate = b.endDate,
                                               itemsLocId = b.itemsLocId,
                                               itemUnitId = b.itemUnitId,
                                               locationId = b.locationId,
                                               note = b.note,
                                               quantity = b.quantity,
                                               startDate = b.startDate,

                                               updateDate = b.updateDate,
                                               updateUserId = b.updateUserId,
                                               itemName = i.name,
                                               unitName = u.units.name,
                                               sectionId = s.sectionId,
                                               isFreeZone = s.isFreeZone,
                                               itemType = i.type,
                                               location = l.x +  l.y + l.z,
                                               section = s.name,
                                               isExpired = i.isExpired,
                                               isSelected = 0,
                                           }).OrderBy(a => a.endDate)
                                            .ToList();


                            //  return Ok(locList);
                            return TokenManager.GenerateToken(locList);
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
        }

        [HttpPost]
        [Route("GetLackItems")]
        public async Task < string> GetLackItems(string token)
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
                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }
                try
                {
                    InvoicesController c = new InvoicesController();
                    var orders = await c.getUnhandeledOrdersList("ors", 0, branchId);

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<ItemLocationModel> requiredTransfers = new List<ItemLocationModel>();
                        foreach (InvoiceModel invoice in orders)
                        {
                            var itemsTransfer = entity.itemsTransfer.Where(x => x.invoiceId == invoice.invoiceId 
                                                                    && x.itemsUnits.items.type != "sr").ToList();
                            foreach (itemsTransfer tr in itemsTransfer)
                            {
                                var lockedQuantity = entity.itemsLocations
                                    .Where(x => x.invoiceId == invoice.invoiceId && x.itemUnitId == tr.itemUnitId)
                                    .Select(x => x.quantity).Sum();
                                var availableAmount = getBranchAmount((int)tr.itemUnitId, branchId);
                                var item = (from i in entity.items
                                            join u in entity.itemsUnits on i.itemId equals u.itemId
                                            where u.itemUnitId == tr.itemUnitId
                                            select new ItemModel()
                                            {
                                                itemId = i.itemId,
                                                name = i.name,
                                                unitName = u.units.name,
                                            }).FirstOrDefault();
                                if (lockedQuantity == null)
                                    lockedQuantity = 0;
                                if ((lockedQuantity + availableAmount) < tr.quantity) // there is a shortage in order amount
                                {
                                    long requiredQuantity = (long)tr.quantity - ((long)lockedQuantity + (long)availableAmount);

                                    var itemUnit = requiredTransfers.Where(x => x.itemUnitId == tr.itemUnitId).FirstOrDefault();
                                    if (itemUnit == null)
                                    {
                                        ItemLocationModel transfer = new ItemLocationModel()
                                        {
                                            quantity = requiredQuantity,
                                            itemUnitId = tr.itemUnitId,
                                            itemName = item.name,
                                            unitName = item.unitName,
                                        };
                                        requiredTransfers.Add(transfer);
                                    }
                                    else
                                        itemUnit.quantity += requiredQuantity;
                                }

                            }
                        }
                        return TokenManager.GenerateToken(requiredTransfers);
                    }
                }

                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }

            }
        }
        [HttpPost]
        [Route("getShortageItems")]
        public async Task < string> getShortageItems(string token)
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
                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }
                try
                {
                    InvoicesController c = new InvoicesController();
                    var orders = await c.getUnhandeledOrdersList("ors", 0, branchId);

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<ItemTransferModel> requiredTransfers = new List<ItemTransferModel>();
                        foreach (InvoiceModel invoice in orders)
                        {
                            var itemsTransfer = entity.itemsTransfer.Where(x => x.invoiceId == invoice.invoiceId 
                                                                    && x.itemsUnits.items.type != "sr").ToList();
                            foreach (itemsTransfer tr in itemsTransfer)
                            {
                                var item = (from i in entity.items
                                            join u in entity.itemsUnits on i.itemId equals u.itemId
                                            where u.itemUnitId == tr.itemUnitId
                                            select new ItemModel()
                                            {
                                                itemId = i.itemId,
                                                name = i.name,
                                                unitName = u.units.name,
                                                price = u.price,
                                            }).FirstOrDefault();
 
                                var itemUnit = requiredTransfers.Where(x => x.itemUnitId == tr.itemUnitId).FirstOrDefault();
                                if (itemUnit == null)
                                {
                                    ItemTransferModel transfer = new ItemTransferModel()
                                    {
                                        price = item.price,
                                        quantity = tr.quantity,
                                        itemUnitId = tr.itemUnitId,
                                        itemId = item.itemId,
                                        itemName = item.name,
                                        unitName = item.unitName,
                                    };
                                    requiredTransfers.Add(transfer);
                                }
                                else
                                    itemUnit.quantity += tr.quantity;

                            }

                        }
                        List<ItemTransferModel> shortageList = new List<ItemTransferModel>();
                        foreach (var item in requiredTransfers)
                        {
                            var lockedQuantity = entity.itemsLocations
                                   .Where(x => x.itemUnitId == item.itemUnitId
                                   && x.invoiceId != null
                                   && x.locations.branchId == branchId)
                                   .Select(x => x.quantity).Sum();

                            if (lockedQuantity == null)
                                lockedQuantity = 0;
                            #region available and ordered  amounts 
                            var availableAmount = getBranchAmount((int)item.itemUnitId, branchId);

                            var orderedQuantity = entity.itemsTransfer.Where(x => x.invoices.branchCreatorId == branchId
                                                                                && x.invoices.isActive == true
                                                                                && ((x.invoices.invType == "po" && !entity.invoices.Any(y => y.invoiceMainId == x.invoices.invoiceId && x.invoices.isActive == true))
                                                                                            || (x.invoices.invType == "exw" && x.invoices.isActive == true))
                                                                                && x.itemUnitId == item.itemUnitId)
                                                                    .Select(x => x.quantity).Sum();
                            if (orderedQuantity != null)
                                availableAmount += (int)orderedQuantity;
                            #endregion

                            if ((lockedQuantity + availableAmount) < item.quantity) // there is a shortage in order amount
                            {

                                long requiredQuantity = (long)item.quantity - ((long)lockedQuantity + (long)availableAmount);

                                var itemUnit = requiredTransfers.Where(x => x.itemUnitId == item.itemUnitId).FirstOrDefault();
                                itemUnit.quantity = requiredQuantity;
                                shortageList.Add(itemUnit);
                            }
                        }
                        return TokenManager.GenerateToken(shortageList);
                    }
                }

                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }

            }
        }
        [HttpPost]
        [Route("getShortageNoPackageItems")]
        public async Task < string> getShortageNoPackageItems(string token)
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
                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }
                try
                {
                    InvoicesController c = new InvoicesController();
                    var orders = await c.getUnhandeledOrdersList("ors", 0, branchId);

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<ItemTransferModel> requiredTransfers = new List<ItemTransferModel>();
                        foreach (InvoiceModel invoice in orders)
                        {
                            var itemsTransfer = entity.itemsTransfer.Where(x => x.invoiceId == invoice.invoiceId 
                                                                    && x.itemsUnits.items.type != "sr"
                                                                    && x.itemsUnits.items.type != "p").ToList();

                            foreach (itemsTransfer tr in itemsTransfer)
                            {
                                //var lockedQuantity = entity.itemsLocations
                                //    .Where(x => x.invoiceId == invoice.invoiceId && x.itemUnitId == tr.itemUnitId)
                                //    .Select(x => x.quantity).Sum();

                                //#region available and ordered  amounts 
                                //var availableAmount = getBranchAmount((int)tr.itemUnitId, branchId);

                                //var orderedQuantity = entity.itemsTransfer.Where(x => x.invoices.branchCreatorId == branchId
                                //                                                    && x.invoices.isActive == true
                                //                                                    && ((x.invoices.invType == "po" && !entity.invoices.Any(y => y.invoiceMainId == x.invoices.invoiceId && x.invoices.isActive==true)) 
                                //                                                                || (x.invoices.invType == "exw" && x.invoices.isActive ==true))
                                //                                                    && x.itemUnitId == tr.itemUnitId)
                                //                                        .Select(x => x.quantity).Sum();
                                //if(orderedQuantity != null)
                                //    availableAmount += (int)orderedQuantity;
                                //#endregion

                                var item = (from i in entity.items
                                            join u in entity.itemsUnits on i.itemId equals u.itemId
                                            where u.itemUnitId == tr.itemUnitId
                                            select new ItemModel()
                                            {
                                                itemId = i.itemId,
                                                name = i.name,
                                                unitName = u.units.name,
                                                price = u.price,
                                            }).FirstOrDefault();
                                //if (lockedQuantity == null)
                                //    lockedQuantity = 0;
                                //if ((lockedQuantity + availableAmount) < tr.quantity) // there is a shortage in order amount
                                //{
                                //    long requiredQuantity = (long)tr.quantity - ((long)lockedQuantity + (long)availableAmount);

                                    var itemUnit = requiredTransfers.Where(x => x.itemUnitId == tr.itemUnitId).FirstOrDefault();
                                    if (itemUnit == null)
                                    {
                                        ItemTransferModel transfer = new ItemTransferModel()
                                        {
                                            price = item.price,
                                            quantity = tr.quantity,
                                            itemUnitId = tr.itemUnitId,
                                            itemId = item.itemId,
                                            itemName = item.name,
                                            unitName = item.unitName,
                                        };
                                        requiredTransfers.Add(transfer);
                                    }
                                    else
                                        itemUnit.quantity += tr.quantity;
                                //}

                            }
                           
                        }
                        List<ItemTransferModel> shortageList = new List<ItemTransferModel>();
                        foreach (var item in requiredTransfers)
                        {
                            var lockedQuantity = entity.itemsLocations
                                   .Where(x =>  x.itemUnitId == item.itemUnitId 
                                   && x.invoiceId != null
                                   && x.locations.branchId == branchId)
                                   .Select(x => x.quantity).Sum();

                            if (lockedQuantity == null)
                                lockedQuantity = 0;
                            #region available and ordered  amounts 
                            var availableAmount = getBranchAmount((int)item.itemUnitId, branchId);

                            var orderedQuantity = entity.itemsTransfer.Where(x => x.invoices.branchCreatorId == branchId
                                                                                && x.invoices.isActive == true
                                                                                && ((x.invoices.invType == "po" && !entity.invoices.Any(y => y.invoiceMainId == x.invoices.invoiceId && x.invoices.isActive == true))
                                                                                            || (x.invoices.invType == "exw" && x.invoices.isActive == true))
                                                                                && x.itemUnitId == item.itemUnitId)
                                                                    .Select(x => x.quantity).Sum();
                            if (orderedQuantity != null)
                                availableAmount += (int)orderedQuantity;
                            #endregion

                            if ((lockedQuantity + availableAmount) < item.quantity) // there is a shortage in order amount
                            {

                                long requiredQuantity = (long)item.quantity - ((long)lockedQuantity + (long)availableAmount);

                                var itemUnit = requiredTransfers.Where(x => x.itemUnitId == item.itemUnitId).FirstOrDefault();
                                itemUnit.quantity = requiredQuantity;
                                shortageList.Add(itemUnit);
                            }
                        }
                        return TokenManager.GenerateToken(shortageList);
                    }
                }

                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }

            }
        }
        [HttpPost]
        [Route("isThereLack")]
        public async Task<string> isThereLack(string token)
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
                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);

                    }
                }
                try
                {
                     message = await isThereLack(branchId);
                    return TokenManager.GenerateToken(message);
                    
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }

            }
        }

        public async Task< string> isThereLack(int branchId)
        {
            InvoicesController c = new InvoicesController();
            var orders = await c.getUnhandeledOrdersList("ors", 0, branchId);

            using (incposdbEntities entity = new incposdbEntities())
            {
                List<ItemTransferModel> requiredTransfers = new List<ItemTransferModel>();
                foreach (InvoiceModel invoice in orders)
                {
                    var itemsTransfer = entity.itemsTransfer.Where(x => x.invoiceId == invoice.invoiceId && x.itemsUnits.items.type != "sr").ToList();
                    foreach (itemsTransfer tr in itemsTransfer)
                    {
                        var lockedQuantity = entity.itemsLocations
                            .Where(x => x.invoiceId == invoice.invoiceId && x.itemUnitId == tr.itemUnitId)
                            .Select(x => x.quantity).Sum();
                        var availableAmount = getBranchAmount((int)tr.itemUnitId, branchId);

                        if (lockedQuantity == null)
                            lockedQuantity = 0;
                        if ((lockedQuantity + availableAmount) < tr.quantity) // there is a shortage in order amount
                        {
                            return "yes";
                        }

                    }
                }
                return "no";
            }
        }
        public async Task< string> isThereLackNoPackage(int branchId)
        {
            InvoicesController c = new InvoicesController();
            var orders = await c.getUnhandeledOrdersList("ors", 0, branchId);

            using (incposdbEntities entity = new incposdbEntities())
            {
                List<ItemTransferModel> requiredTransfers = new List<ItemTransferModel>();
                foreach (InvoiceModel invoice in orders)
                {
                    var itemsTransfer = entity.itemsTransfer
                                        .Where(x => x.invoiceId == invoice.invoiceId
                                         && x.itemsUnits.items.type != "sr" 
                                         && x.itemsUnits.items.type != "p").ToList();
                    foreach (itemsTransfer tr in itemsTransfer)
                    {
                        var lockedQuantity = entity.itemsLocations
                            .Where(x => x.invoiceId == invoice.invoiceId && x.itemUnitId == tr.itemUnitId)
                            .Select(x => x.quantity).Sum();
                        var availableAmount = getBranchAmount((int)tr.itemUnitId, branchId);
                        
                        if (lockedQuantity == null)
                            lockedQuantity = 0;
                        if ((lockedQuantity + availableAmount) < tr.quantity) // there is a shortage in order amount
                        {
                            return "yes";
                        }

                    }
                }
                return "no";
            }
        }
        [HttpPost]
        [Route("unlockItem")]
        public string unlockItem(string token)
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
                int branchId = 0;
                itemsLocations newObject = new itemsLocations();
                        
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {

                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<itemsLocations>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "branchId")
                        branchId = int.Parse(c.Value);
                }

                if (newObject != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var itemLoc = (from b in entity.itemsLocations
                                           where b.invoiceId == null && b.itemUnitId == newObject.itemUnitId && b.locationId == newObject.locationId
                                           && b.startDate == newObject.startDate && b.endDate == newObject.endDate
                                           && b.locations.sections.branchId == branchId
                                           select new ItemLocationModel
                                           {
                                               itemsLocId = b.itemsLocId,
                                           }).FirstOrDefault();
                            var orderItem = entity.itemsLocations.Find(newObject.itemsLocId);
                            if (orderItem.quantity == newObject.quantity)
                                entity.itemsLocations.Remove(orderItem);
                            else
                                orderItem.quantity -= newObject.quantity;

                            if (itemLoc == null)
                            {
                                var loc = new itemsLocations()
                                {
                                    locationId = newObject.locationId,
                                    quantity = newObject.quantity,
                                    createDate = cc.AddOffsetTodate(DateTime.Now),
                                    updateDate = cc.AddOffsetTodate(DateTime.Now),
                                    createUserId = newObject.createUserId,
                                    updateUserId = newObject.createUserId,
                                    startDate = newObject.startDate,
                                    endDate = newObject.endDate,
                                    itemUnitId = newObject.itemUnitId,
                                    note = newObject.note,
                                };
                                entity.itemsLocations.Add(loc);
                            }
                            else
                            {
                                var loc = entity.itemsLocations.Find(itemLoc.itemsLocId);
                                loc.quantity += newObject.quantity;
                                loc.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                loc.updateUserId = newObject.updateUserId;

                            }
                            entity.SaveChanges();
                        }
                       // return Ok();
                        return TokenManager.GenerateToken("1");
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
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid) // APIKey is valid
            //{
            //    itemsLocations il = JsonConvert.DeserializeObject<itemsLocations>(itemLocation, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var itemLoc = (from b in entity.itemsLocations
            //                       where b.invoiceId == null && b.itemUnitId == il.itemUnitId && b.locationId == il.locationId
            //                       && b.startDate == il.startDate && b.endDate == il.endDate
            //                       select new ItemLocationModel
            //                       {
            //                           itemsLocId = b.itemsLocId,
            //                       }).FirstOrDefault();
            //        var orderItem = entity.itemsLocations.Find(il.itemsLocId);
            //        if (orderItem.quantity == il.quantity)
            //            entity.itemsLocations.Remove(orderItem);
            //        else
            //            orderItem.quantity -= il.quantity;

            //        if (itemLoc == null)
            //        {
            //            var loc = new itemsLocations()
            //            {
            //                locationId = il.locationId,
            //                quantity = il.quantity,
            //                createDate = DateTime.Now,
            //                updateDate = DateTime.Now,
            //                createUserId = il.createUserId,
            //                updateUserId = il.createUserId,
            //                startDate = il.startDate,
            //                endDate = il.endDate,
            //                itemUnitId = il.itemUnitId,
            //                note = il.note,
            //            };
            //            entity.itemsLocations.Add(loc);
            //        }
            //        else
            //        {
            //            var loc = entity.itemsLocations.Find(itemLoc.itemsLocId);
            //            loc.quantity += il.quantity;
            //            loc.updateDate = cc.AddOffsetTodate(DateTime.Now);
            //            loc.updateUserId = il.updateUserId;

            //        }
            //        entity.SaveChanges();
            //    }
            //    return Ok();
            //}
            ////else
            //return NotFound();


        }
        [HttpPost]
        [Route("unlockItems")]
        public string unlockItems(string token)
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
                int branchId = 0;
                List<itemsTransfer> newObject = new List<itemsTransfer>();
                        
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {

                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "branchId")
                        branchId = int.Parse(c.Value);
                }
                try
                {
                    //using (incposdbEntities entity = new incposdbEntities())
                    //{
                    //    foreach(itemsTransfer item in newObject)
                    //    {
                    //        var itemLocId = (from b in entity.itemsLocations
                    //                       where b.invoiceId == item.invoiceId && b.itemUnitId == item.itemUnitId
                    //                       select new
                    //                       {
                    //                            b.itemsLocId,
                    //                       }).SingleOrDefault();
                    //        if (itemLocId != null)
                    //        {
                    //            var reservedItem = entity.itemsLocations.Find(itemLocId.itemsLocId);
                    //            var itemLoc = (from b in entity.itemsLocations
                    //                           where b.invoiceId == null && b.itemUnitId == reservedItem.itemUnitId && b.locationId == reservedItem.locationId
                    //                           && b.startDate == reservedItem.startDate && b.endDate == reservedItem.endDate 
                    //                           && b.locations.sections.branches.branchId == branchId
                    //                           select new ItemLocationModel
                    //                           {
                    //                               itemsLocId = b.itemsLocId,
                    //                           }).FirstOrDefault();

                    //            if (itemLoc == null)
                    //            {
                    //                var loc = new itemsLocations()
                    //                {
                    //                    locationId = reservedItem.locationId,
                    //                    quantity = reservedItem.quantity,
                    //                    createDate = DateTime.Now,
                    //                    updateDate = DateTime.Now,
                    //                    createUserId = reservedItem.createUserId,
                    //                    updateUserId = reservedItem.createUserId,
                    //                    startDate = reservedItem.startDate,
                    //                    endDate = reservedItem.endDate,
                    //                    itemUnitId = reservedItem.itemUnitId,
                    //                    note = reservedItem.note,
                    //                };
                    //                entity.itemsLocations.Add(loc);
                    //            }
                    //            else
                    //            {
                    //                var loc = entity.itemsLocations.Find(itemLoc.itemsLocId);
                    //                loc.quantity += reservedItem.quantity;
                    //                loc.updateDate = cc.AddOffsetTodate(DateTime.Now);
                    //                loc.updateUserId = reservedItem.updateUserId;

                    //            }
                    //            entity.itemsLocations.Remove(reservedItem);
                    //        }
                    //    }
                    //    entity.SaveChanges();


                    //}
                    unlockItems(newObject, branchId);
                    return TokenManager.GenerateToken("1");
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }
            }
        }

        public void unlockItems(List<itemsTransfer> newObject,int branchId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                foreach (itemsTransfer item in newObject)
                {
                    var itemLocId = (from b in entity.itemsLocations
                                     where b.invoiceId == item.invoiceId && b.itemUnitId == item.itemUnitId
                                     select new
                                     {
                                         b.itemsLocId,
                                     }).SingleOrDefault();
                    if (itemLocId != null)
                    {
                        var reservedItem = entity.itemsLocations.Find(itemLocId.itemsLocId);
                        var itemLoc = (from b in entity.itemsLocations
                                       where b.invoiceId == null && b.itemUnitId == reservedItem.itemUnitId && b.locationId == reservedItem.locationId
                                       && b.startDate == reservedItem.startDate && b.endDate == reservedItem.endDate
                                       && b.locations.sections.branches.branchId == branchId
                                       select new ItemLocationModel
                                       {
                                           itemsLocId = b.itemsLocId,
                                       }).FirstOrDefault();

                        if (itemLoc == null)
                        {
                            var loc = new itemsLocations()
                            {
                                locationId = reservedItem.locationId,
                                quantity = reservedItem.quantity,
                                createDate = cc.AddOffsetTodate(DateTime.Now),
                                updateDate = cc.AddOffsetTodate(DateTime.Now),
                                createUserId = reservedItem.createUserId,
                                updateUserId = reservedItem.createUserId,
                                startDate = reservedItem.startDate,
                                endDate = reservedItem.endDate,
                                itemUnitId = reservedItem.itemUnitId,
                                note = reservedItem.note,
                            };
                            entity.itemsLocations.Add(loc);
                        }
                        else
                        {
                            var loc = entity.itemsLocations.Find(itemLoc.itemsLocId);
                            loc.quantity += reservedItem.quantity;
                            loc.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            loc.updateUserId = reservedItem.updateUserId;

                        }
                        entity.itemsLocations.Remove(reservedItem);
                    }
                }
                entity.SaveChanges();


            }
        }
        public void unlockQuantity(int invoiceId, int itemUnitId, int quantity)
        {
            string Object = "";  
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemLoc = entity.itemsLocations.Where(b => b.invoiceId == invoiceId && b.itemUnitId == itemUnitId).FirstOrDefault();
                    itemLoc.quantity -= quantity;
                    if (itemLoc.quantity == 0)
                        entity.itemsLocations.Remove(itemLoc);
                    var location = entity.itemsLocations.Where(x => x.invoiceId == null && x.locationId == itemLoc.locationId &&
                                    x.itemUnitId == itemLoc.itemUnitId && x.startDate == itemLoc.startDate && x.endDate == itemLoc.endDate).FirstOrDefault();


                    if (location == null)
                    {
                        var loc = new itemsLocations()
                        {
                            locationId = itemLoc.locationId,
                            quantity = quantity,
                            createDate = cc.AddOffsetTodate(DateTime.Now),
                            updateDate = cc.AddOffsetTodate(DateTime.Now),
                            createUserId = itemLoc.createUserId,
                            updateUserId = itemLoc.createUserId,
                            startDate = itemLoc.startDate,
                            endDate = itemLoc.endDate,
                            itemUnitId = itemLoc.itemUnitId,
                            note = itemLoc.note,
                        };
                        entity.itemsLocations.Add(loc);
                    }
                    else
                    {
                        location.quantity += quantity;
                        location.updateDate = cc.AddOffsetTodate(DateTime.Now);
                        location.updateUserId = itemLoc.updateUserId;
                    }
                    entity.SaveChanges();
                }
            }
            catch
            {
               
            }   
        }

        public  string checkItemsAmounts(List<ItemTransferModel> billDetails,int branchId)
        {
            string res = "";
            ItemsOffersController ioc = new ItemsOffersController();
            foreach(var item in billDetails)
            {
                int availableAmount = getBranchAmount((int)item.itemUnitId, branchId);
                if (item.offerId != 0 && item.offerId != null)
                {
                    int remainAmount = ioc.getRemain((int)item.offerId, (int)item.itemUnitId);
                    if ((availableAmount < item.quantity  && item.itemType != "sr") || remainAmount < item.quantity)
                    {
                        if (item.itemName != null) res = item.itemName;
                        else
                            res = "-3";
                        return res;
                    }
                }
                else if (availableAmount < item.quantity && item.itemType != "sr")
                {
                    if (item.itemName != null)
                        res = item.itemName;
                    else
                        res = "-3";
                    return res;
                }
            }
            return res;
        }
        public  string checkLocationAmounts(int itemLocId,int quantity)
        {
            string res = "";
            using (incposdbEntities entity = new incposdbEntities())
            {
                var locationQuantity = entity.itemsLocations.Find(itemLocId).quantity;
                if (locationQuantity < quantity)
                    return "-3";
            }
            return res;
        }

        public List<ItemLocationModel> GetAlmostExpired()
        {
            try
            {
                List<ItemLocationModel> almostExpired = new List<ItemLocationModel>();
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var storageUnitList = (from b in entity.itemsLocations
                                        where b.quantity > 0 && b.invoiceId == null
                                        join u in entity.itemsUnits on b.itemUnitId equals u.itemUnitId
                                        join i in entity.items.Where(x => x.isExpired == true) on u.itemId equals i.itemId
                                        join l in entity.locations on b.locationId equals l.locationId
                                        join s in entity.sections on l.sectionId equals s.sectionId

                                        select new ItemLocationModel
                                        {
                                            createDate = b.createDate,
                                            createUserId = b.createUserId,
                                            itemsLocId = b.itemsLocId,
                                            itemUnitId = b.itemUnitId,
                                            locationId = b.locationId,
                                            note = b.note,
                                            quantity = b.quantity,
                                            startDate = b.startDate,
                                            endDate = b.endDate,
                                            branchId = s.branchId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            itemName = i.name,
                                            location = l.x + l.y + l.z,
                                            section = s.name,
                                            sectionId = s.sectionId,
                                            itemType = i.type,
                                            unitName = u.units.name,
                                            invoiceId = b.invoiceId,
                                            isExpired = i.isExpired,
                                            alertDays = i.alertDays,
                                        }).ToList().OrderBy(x => x.location).ToList();


                    foreach(var row in storageUnitList)
                    {
                        DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-row.alertDays));
                        if(row.endDate < DateTime.Now)
                        {
                            row.alertDays = -1 * (DateTime.Now - (DateTime)row.endDate).Days;
                            almostExpired.Add(row);

                        }
                        else if (row.endDate < dt)
                        {
                            row.alertDays = ((DateTime)row.endDate - dt).Days;
                            almostExpired.Add(row);
                        }
                    }
                    return almostExpired;
                }
            }
            catch
            {
                return null;
            }

        }

      

    }
}