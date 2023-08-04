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
using System.Data.Entity.SqlServer;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/ItemsUnits")]
    public class ItemsUnitsController : ApiController
    {
        CountriesController cc = new CountriesController();
        List<int> itemUnitsIds = new List<int>();
        private Classes.Calculate Calc = new Classes.Calculate();
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
                int itemId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                        itemId = int.Parse(c.Value);
                }
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemUnitsList = (from IU in entity.itemsUnits
                                             where (IU.itemId == itemId && IU.isActive == 1)
                                             join u in entity.units on IU.unitId equals u.unitId into lj
                                             from v in lj.DefaultIfEmpty()
                                             join u1 in entity.units on IU.subUnitId equals u1.unitId into tj
                                             from v1 in tj.DefaultIfEmpty()
                                             select new ItemUnitModel()
                                             {
                                                 itemUnitId = IU.itemUnitId,
                                                 unitId = IU.unitId,
                                                 mainUnit = v.name,
                                                 createDate = IU.createDate,
                                                 createUserId = IU.createUserId,
                                                 defaultPurchase = IU.defaultPurchase,
                                                 defaultSale = IU.defaultSale,
                                                 price = IU.price,
                                                 cost = IU.cost,
                                                 subUnitId = IU.subUnitId,
                                                 smallUnit = v1.name,
                                                 unitValue = IU.unitValue,
                                                 barcode = IU.barcode,
                                                 updateDate = IU.updateDate,
                                                 updateUserId = IU.updateUserId,
                                                 storageCostId = IU.storageCostId,
                                                 warrantyId = IU.warrantyId,
                                                 hasWarranty = IU.hasWarranty,
                                                 skipProperties = IU.skipProperties,
                                                 skipSerialsNum = IU.skipSerialsNum,
                                                 packCost=IU.packCost,
                                             })
                                                         .ToList();
                        return TokenManager.GenerateToken(itemUnitsList);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        [HttpPost]
        [Route("GetIU")]
        public string GetIU(string token)
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
                        var itemUnitsList = (from IU in entity.itemsUnits
                                             select new
                                             {
                                                 itemUnitId = IU.itemUnitId,
                                                 unitId = IU.unitId,
                                                 itemId = IU.itemId,
                                                 unitValue = IU.unitValue,

                                                 createDate = IU.createDate,
                                                 createUserId = IU.createUserId,
                                                 defaultPurchase = IU.defaultPurchase,
                                                 defaultSale = IU.defaultSale,
                                                 price = IU.price,
                                                 cost = IU.cost,
                                                 subUnitId = IU.subUnitId,

                                                 barcode = IU.barcode,
                                                 updateDate = IU.updateDate,
                                                 updateUserId = IU.updateUserId,

                                                 storageCostId = IU.storageCostId,
                                                 purchasePrice = IU.purchasePrice,
                                                 IU.isActive,
                                                 warrantyId = IU.warrantyId,
                                                 hasWarranty = IU.hasWarranty,
                                                 skipProperties = IU.skipProperties,
                                                 skipSerialsNum = IU.skipSerialsNum,
                                                 packCost = IU.packCost,
                                             })
                                                         .ToList();


                        return TokenManager.GenerateToken(itemUnitsList);
                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }

        }


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
                        var itemUnitsList = (from IU in entity.itemsUnits
                                             where (IU.itemUnitId == itemUnitId)
                                             join u in entity.units on IU.unitId equals u.unitId into lj
                                             from v in lj.DefaultIfEmpty()
                                             join u1 in entity.units on IU.subUnitId equals u1.unitId into tj
                                             from v1 in tj.DefaultIfEmpty()
                                             select new ItemUnitModel()
                                             {
                                                 itemUnitId = IU.itemUnitId,
                                                 unitId = IU.unitId,
                                                 itemId = IU.itemId,
                                                 createDate = IU.createDate,
                                                 createUserId = IU.createUserId,
                                                 defaultPurchase = IU.defaultPurchase,
                                                 defaultSale = IU.defaultSale,
                                                 price = IU.price,
                                                 cost = IU.cost,
                                                 subUnitId = IU.subUnitId,
                                                 unitValue = IU.unitValue,
                                                 barcode = IU.barcode,
                                                 updateDate = IU.updateDate,
                                                 updateUserId = IU.updateUserId,
                                                 storageCostId = IU.storageCostId,
                                                 isActive = IU.isActive,
                                                 warrantyId = IU.warrantyId,
                                                 hasWarranty = IU.hasWarranty,
                                                 skipProperties = IU.skipProperties,
                                                 skipSerialsNum = IU.skipSerialsNum,
                                                 packCost = IU.packCost,
                                             }).FirstOrDefault();
                        return TokenManager.GenerateToken(itemUnitsList);

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
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int itemId = 0;

                bool canDelete = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                        itemId = int.Parse(c.Value);
                }

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemUnitsList = (from IU in entity.itemsUnits
                                             where (IU.itemId == itemId)
                                             join u in entity.units on IU.unitId equals u.unitId into lj
                                             from v in lj.DefaultIfEmpty()
                                             join u1 in entity.units on IU.subUnitId equals u1.unitId into tj
                                             from v1 in tj.DefaultIfEmpty()
                                             select new ItemUnitModel()
                                             {
                                                 itemUnitId = IU.itemUnitId,
                                                 unitId = IU.unitId,
                                                 mainUnit = v.name,
                                                 createDate = IU.createDate,
                                                 createUserId = IU.createUserId,
                                                 defaultPurchase = IU.defaultPurchase,
                                                 defaultSale = IU.defaultSale,
                                                 price = IU.price,
                                                 cost = IU.cost,
                                                 subUnitId = IU.subUnitId,
                                                 smallUnit = v1.name,
                                                 unitValue = IU.unitValue,
                                                 barcode = IU.barcode,
                                                 updateDate = IU.updateDate,
                                                 updateUserId = IU.updateUserId,
                                                 storageCostId = IU.storageCostId,
                                                 isActive = IU.isActive,
                                                 warrantyId = IU.warrantyId,
                                                 hasWarranty = IU.hasWarranty,
                                                 skipProperties = IU.skipProperties,
                                                 skipSerialsNum = IU.skipSerialsNum,
                                                 packCost = IU.packCost,
                                             })
                                             .ToList();
                        foreach (ItemUnitModel um in itemUnitsList)
                        {
                            canDelete = false;
                            if (um.isActive == 1)
                            {
                                var purItem = entity.itemsTransfer.Where(x => x.itemUnitId == um.itemUnitId).Select(b => new { b.itemsTransId, b.itemUnitId }).FirstOrDefault();
                                var packages = entity.packages.Where(x => x.childIUId == um.itemUnitId || x.packageId == um.itemUnitId).Select(x => new { x.packageId, x.parentIUId }).FirstOrDefault();
                                if (purItem == null && packages == null)
                                    canDelete = true;
                            }
                            um.canDelete = canDelete;
                        }

                        return TokenManager.GenerateToken(itemUnitsList);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        // add or update item unit
        [HttpPost]
        [Route("Save")]
        public string Save(string token)
        {

           // string message = "";


            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string Object = "";
                itemsUnits newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<itemsUnits>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {

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
                    if (newObject.storageCostId == 0 || newObject.storageCostId == null)
                    {
                        Nullable<int> id = null;
                        newObject.storageCostId = id;
                    }
                    if (newObject.warrantyId == 0 || newObject.warrantyId == null)
                    {
                        Nullable<int> id = null;
                        newObject.warrantyId = id;
                    }
                    try
                    {
                        int itemUnitId = 0;
                        itemsUnits tmpItemUnit = new itemsUnits();
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var itemUnitEntity = entity.Set<itemsUnits>();
                            if (newObject.itemUnitId == 0)
                            {
                                var iu = entity.itemsUnits.Where(x => x.itemId == newObject.itemId).FirstOrDefault();
                                if (iu == null)
                                {
                                    newObject.defaultSale = 1;
                                    newObject.defaultPurchase = 1;
                                }
                                else
                                {
                                    //create
                                    // set the other default sale or purchase to 0 if the new object.default is 1

                                    if (newObject.defaultSale == 1)
                                    { // get the row with same itemId of newObject
                                        itemsUnits defItemUnit = entity.itemsUnits.Where(p => p.itemId == newObject.itemId && p.defaultSale == 1).FirstOrDefault();
                                        if (defItemUnit != null)
                                        {
                                            defItemUnit.defaultSale = 0;
                                            entity.SaveChanges();
                                        }
                                    }
                                    if (newObject.defaultPurchase == 1)
                                    {
                                        var defItemUnit = entity.itemsUnits.Where(p => p.itemId == newObject.itemId && p.defaultPurchase == 1).FirstOrDefault();
                                        if (defItemUnit != null)
                                        {
                                            defItemUnit.defaultPurchase = 0;
                                            entity.SaveChanges();
                                        }
                                    }
                                }
                                newObject.createDate = cc.AddOffsetTodate(DateTime.Now);
                                newObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                newObject.updateUserId = newObject.createUserId;
                                newObject.isActive = 1;

                               tmpItemUnit = itemUnitEntity.Add(newObject);
                            }
                            else
                            {
                                //update
                                // set the other default sale or purchase to 0 if the new object.default is 1
                               itemUnitId = newObject.itemUnitId;
                                tmpItemUnit = entity.itemsUnits.Find(itemUnitId);

                                if (newObject.defaultSale == 1)
                                {
                                    itemsUnits saleItemUnit = entity.itemsUnits.Where(p => p.itemId == tmpItemUnit.itemId && p.defaultSale == 1).FirstOrDefault();
                                    if (saleItemUnit != null)
                                    {
                                        saleItemUnit.defaultSale = 0;
                                        entity.SaveChanges();
                                    }
                                }
                                if (newObject.defaultPurchase == 1)
                                {
                                    var defItemUnit = entity.itemsUnits.Where(p => p.itemId == tmpItemUnit.itemId && p.defaultPurchase == 1).FirstOrDefault();
                                    if (defItemUnit != null)
                                    {
                                        defItemUnit.defaultPurchase = 0;
                                        entity.SaveChanges();
                                    }
                                }
                                tmpItemUnit.barcode = newObject.barcode;
                                tmpItemUnit.price = newObject.price;
                                tmpItemUnit.cost = newObject.cost;
                                tmpItemUnit.subUnitId = newObject.subUnitId;
                                tmpItemUnit.unitId = newObject.unitId;
                                tmpItemUnit.unitValue = newObject.unitValue;
                                tmpItemUnit.defaultPurchase = newObject.defaultPurchase;
                                tmpItemUnit.defaultSale = newObject.defaultSale;
                                tmpItemUnit.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                tmpItemUnit.updateUserId = newObject.updateUserId;
                                tmpItemUnit.storageCostId = newObject.storageCostId;
                                tmpItemUnit.isActive = newObject.isActive;
                                tmpItemUnit.warrantyId = newObject.warrantyId;
                                tmpItemUnit.hasWarranty = newObject.hasWarranty;
                                tmpItemUnit.skipProperties = newObject.skipProperties;
                                tmpItemUnit.skipSerialsNum = newObject.skipSerialsNum;
                                tmpItemUnit.packCost = newObject.packCost;
                                
                            }

                            entity.SaveChanges();
                            //message = itemUnitId.ToString();

                            var item = entity.items.Where(x => x.itemId == newObject.itemId).FirstOrDefault();
                            if (item.type == "p")
                            {
                                item.avgPurchasePrice = calculatePackagePrice(tmpItemUnit.itemUnitId);
                                entity.SaveChanges();
                            }
                            return TokenManager.GenerateToken(tmpItemUnit.itemUnitId.ToString());

                        }
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }


                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }


            }

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
                int ItemUnitId = 0;
                int userId = 0;
                bool final = false;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "ItemUnitId")
                    {
                        ItemUnitId = int.Parse(c.Value);
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

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        if (final)
                        {
                            itemsUnits itemUnit = entity.itemsUnits.Find(ItemUnitId);

                            entity.itemsUnits.Remove(itemUnit);

                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);

                        }
                        else
                        {

                            itemsUnits unitDelete = entity.itemsUnits.Find(ItemUnitId);
                            unitDelete.isActive = 0;
                            unitDelete.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            unitDelete.updateUserId = userId;

                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);

                        }
                    }
                }

                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        [HttpPost]
        [Route("GetAllBarcodes")]
        public string GetAllBarcodes(string token)
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
                        var barcods = (from i in entity.itemsUnits
                                       join u in entity.units on i.unitId equals u.unitId

                                       select new ItemUnitModel()
                                       {
                                           itemId = i.itemId,
                                           barcode = i.barcode,
                                           unitId = i.unitId,
                                           itemUnitId = i.itemUnitId,
                                           mainUnit = u.name,
                                           cost = i.cost,
                                           avgPurchasePrice = i.items.avgPurchasePrice,
                                       }).ToList();

                        foreach(var unit in barcods)
                            unit.cost = unit.avgPurchasePrice == 0 ? unit.cost : unit.avgPurchasePrice * multiplyFactorWithSmallestUnit((int)unit.itemId, unit.itemUnitId);
                        return TokenManager.GenerateToken(barcods);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        [HttpPost]
        [Route("GetallItemsUnits")]
        public string GetallItemsUnits(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            itemsPropController pc = new itemsPropController();

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
                        var itemUnitsList = (from IU in entity.itemsUnits

                                             join u in entity.units on IU.unitId equals u.unitId
                                             where u.isActive == 1

                                             join i in entity.items on IU.itemId equals i.itemId
                                             orderby i.name
                                             select new ItemUnitModel()
                                             {
                                                 itemUnitId = IU.itemUnitId,
                                                 unitId = IU.unitId,
                                                 itemId = IU.itemId,
                                                 mainUnit = u.name,
                                                 createDate = IU.createDate,
                                                 createUserId = IU.createUserId,
                                                 defaultPurchase = IU.defaultPurchase,
                                                 defaultSale = IU.defaultSale,
                                                 price = IU.price,
                                                 basicPrice = IU.price,
                                                 cost = IU.cost,
                                                 avgPurchasePrice = i.avgPurchasePrice,
                                                 subUnitId = IU.subUnitId,
                                                 unitValue = IU.unitValue,
                                                 barcode = IU.barcode,
                                                 updateDate = IU.updateDate,
                                                 updateUserId = IU.updateUserId,
                                                 itemName = i.name,
                                                 itemCode = i.code,
                                                 unitName = u.name,
                                                 storageCostId = IU.storageCostId,
                                                 isActive = IU.isActive,
                                                 warrantyId = IU.warrantyId,
                                                 hasWarranty = IU.hasWarranty,
                                                 skipProperties = IU.skipProperties,
                                                 skipSerialsNum = IU.skipSerialsNum,
                                                 packCost = IU.packCost,
                                             })
                                             .ToList();

                        foreach (var unit in itemUnitsList)
                        {
                            unit.cost = unit.avgPurchasePrice == 0 ? unit.cost : unit.avgPurchasePrice * multiplyFactorWithSmallestUnit((int)unit.itemId, unit.itemUnitId);
                            unit.ItemProperties = pc.GetByItemUnitId((int)unit.itemUnitId);
                        }

                        return TokenManager.GenerateToken(itemUnitsList);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        [HttpPost]
        [Route("GetForSale")]
        public string GetForSale(string token)
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
                    itemsPropController pc = new itemsPropController();
                    DateTime datenow = cc.AddOffsetTodate(DateTime.Now);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemUnitsList = (from IU in entity.itemsUnits

                                             join u in entity.units on IU.unitId equals u.unitId
                                             where u.isActive == 1

                                             join i in entity.items on IU.itemId equals i.itemId
                                             orderby i.name
                                             select new ItemSalePurModel()
                                             {
                                                 itemUnitId = IU.itemUnitId,

                                                 unitId = IU.unitId,
                                                 itemId = IU.itemId,
                                                 createDate = IU.createDate,
                                                 createUserId = IU.createUserId,
                                                 defaultPurchase = IU.defaultPurchase,
                                                 defaultSale = IU.defaultSale,
                                                 price = IU.price,
                                                 //basicPrice = IU.price,
                                                 taxes = i.taxes,
                                                 updateDate = IU.updateDate,
                                                 updateUserId = IU.updateUserId,
                                                 unitName = u.name,
                                                 isActive = IU.isActive,
                                                 warrantyId = IU.warrantyId,
                                                 hasWarranty = IU.hasWarranty,
                                                 warrantyDescription = IU.warranty.description,
                                                 warrantyName = IU.warranty.name,
                                                 skipProperties = IU.skipProperties,
                                                 skipSerialsNum = IU.skipSerialsNum,
                                                 packCost = IU.packCost,

                                             }).ToList();

                        var itemsofferslist = (from off in entity.offers

                                               join itof in entity.itemsOffers on off.offerId equals itof.offerId // itemsOffers and offers 
                                               join iu in entity.itemsUnits on itof.iuId equals iu.itemUnitId
                                               select new ItemSalePurModel()
                                               {
                                                   itemId = iu.itemId,
                                                   itemUnitId = itof.iuId,
                                                   offerName = off.name,
                                                   offerId = off.offerId,
                                                   discountValue = off.discountValue,
                                                   isNew = 0,
                                                   isOffer = 1,
                                                   isActiveOffer = off.isActive,
                                                   startDate = off.startDate,
                                                   endDate = off.endDate,
                                                   unitId = iu.unitId,
                                                   itemCount = itof.quantity,
                                                   price = iu.price,
                                                   discountType = off.discountType,
                                                   desPrice = iu.price,
                                                   defaultSale = iu.defaultSale,
                                                   used = itof.used,
                                                   packCost = iu.packCost,

                                               }).ToList();
                        itemsofferslist = itemsofferslist.Where(IO => (IO.isActiveOffer == 1 && DateTime.Compare(((DateTime)IO.startDate).Date, datenow.Date) <= 0 && System.DateTime.Compare(((DateTime)IO.endDate).Date, datenow.Date) >= 0 && IO.itemCount > IO.used)
                                         && (((DateTime)IO.startDate)).TimeOfDay <= datenow.TimeOfDay && ((DateTime)IO.endDate).TimeOfDay >= datenow.TimeOfDay)
                                         .Distinct().ToList();

                        foreach (var row in itemUnitsList)
                        {
                           // row.priceTax = row.price + (row.price * row.taxes / 100);
                            row.ItemProperties = pc.GetByItemUnitId((int)row.itemUnitId);
                            #region slices prices for item
                            row.SalesPrices = entity.Prices.Where(x => x.itemUnitId == row.itemUnitId && x.isActive == true)
                                                    .Select(x => new PriceModel()
                                                    {
                                                        price = x.price,
                                                        basicPrice = x.price,
                                                        sliceId = x.sliceId,
                                                        name = x.name,
                                                    }).ToList();

                            //foreach (var pr in row.SalesPrices)
                            //{
                            //    pr.priceTax = pr.price + Calc.percentValue(pr.price, row.taxes);
                            //}
                            #endregion

                            decimal? totaldis = 0;
                            foreach (var itofflist in itemsofferslist)
                            {
                                if (row.itemUnitId == itofflist.itemUnitId)
                                {
                                    row.isOffer = 1;
                                    row.offerId = itofflist.offerId;
                                    row.offerName = itofflist.offerName;
                                    row.price = itofflist.price;
                                    //row.priceTax = row.price + (row.price * row.taxes / 100);
                                    row.discountType = itofflist.discountType;
                                    row.discountValue = itofflist.discountValue;
                                    if (itofflist.used == null)
                                        itofflist.used = 0;

                                    if (row.discountType == "1") // value
                                    {

                                        totaldis += row.discountValue;
                                    }
                                    else if (row.discountType == "2") // percent
                                    {

                                        totaldis += Calc.percentValue(row.price, row.discountValue);

                                    }

                                }
                            }

                            row.price = row.price - totaldis;
                           // row.priceTax = row.price + (row.price * row.taxes / 100);
                        }
                        return TokenManager.GenerateToken(itemUnitsList);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        [HttpPost]
        [Route("GetActiveItemsUnits")]
        public string GetActiveItemsUnits(string token)
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
                        var itemUnitsList = (from IU in entity.itemsUnits

                                             join u in entity.units on IU.unitId equals u.unitId

                                             select new ItemUnitModel()
                                             {
                                                 itemUnitId = IU.itemUnitId,
                                                 unitId = IU.unitId,
                                                 itemId = IU.itemId,
                                                 mainUnit = u.name,
                                                 defaultPurchase = IU.defaultPurchase,
                                                 defaultSale = IU.defaultSale,
                                                 price = IU.price,
                                                 cost = IU.cost,
                                                 unitValue = IU.unitValue,
                                                 barcode = IU.barcode,
                                                 unitName = u.name,
                                                 warrantyId = IU.warrantyId,
                                                 hasWarranty = IU.hasWarranty,
                                                 skipProperties = IU.skipProperties,
                                                 skipSerialsNum = IU.skipSerialsNum,
                                                 packCost =IU.packCost,
                                             }).ToList();
                        return TokenManager.GenerateToken(itemUnitsList);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }


        [HttpPost]
        [Route("GetUnitsForSales")]
        public string GetUnitsForSales(string token)
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
                        branchId = int.Parse(c.Value);
                }
                try
                {
                    ItemsLocationsController ilc = new ItemsLocationsController();
                    DateTime datenow = cc.AddOffsetTodate(DateTime.Now);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemUnitsList = (from u in entity.itemsUnits
                                             where u.isActive == 1
                                            // join il in entity.itemsLocations on u.itemUnitId equals il.itemUnitId
                                            // join l in entity.locations on il.locationId equals l.locationId
                                            // join s in entity.sections.Where(x => x.branchId == branchId) on l.sectionId equals s.sectionId
                                           && (u.itemId == (from ux in entity.itemsUnits
                                                                where u.itemId == ux.itemId
                                                                where ux.isActive == 1
                                                               // join il in entity.itemsLocations on ux.itemUnitId equals il.itemUnitId
                                                               // join l in entity.locations on il.locationId equals l.locationId
                                                               // join s in entity.sections.Where(x => x.branchId == branchId) on l.sectionId equals s.sectionId
                                                               // where il.quantity > 0
                                                                select ux.itemId).FirstOrDefault() || u.items.type=="sr")
                                             select new ItemSalePurModel()
                                             {
                                                 itemId = u.itemId,
                                                 barcode = u.barcode,
                                                 unitName = u.units.name,
                                                 itemUnitId = u.itemUnitId,
                                                 price = u.price,
                                                 basicPrice = u.price,
                                                 taxes = u.items.taxes,
                                                 warrantyId = u.warrantyId,
                                                 hasWarranty = u.hasWarranty,
                                                 warrantyDescription = u.warranty.description,
                                                 warrantyName = u.warranty.name,
                                                 skipProperties = u.skipProperties,
                                                 skipSerialsNum =u.skipSerialsNum,
                                                 packCost = u.packCost,
                                             }).ToList();


                        var itemsofferslist = (from off in entity.offers

                                               join itof in entity.itemsOffers on off.offerId equals itof.offerId // itemsOffers and offers 

                                               //  join iu in entity.itemsUnits on itof.iuId  equals  iu.itemUnitId //itemsUnits and itemsOffers
                                               join iu in entity.itemsUnits on itof.iuId equals iu.itemUnitId
                                               //from un in entity.units
                                               select new ItemSalePurModel()
                                               {
                                                   itemId = iu.itemId,
                                                   itemUnitId = itof.iuId,
                                                   offerName = off.name,
                                                   offerId = off.offerId,
                                                   discountValue = off.discountValue,
                                                   isNew = 0,
                                                   isOffer = 1,
                                                   isActiveOffer = off.isActive,
                                                   startDate = off.startDate,
                                                   endDate = off.endDate,
                                                   unitId = iu.unitId,
                                                   used = itof.used,
                                                   price = iu.price,
                                                   discountType = off.discountType,
                                                   desPrice = iu.price,
                                                   defaultSale = iu.defaultSale,
                                                   itemCount = itof.quantity,
                                                   skipProperties = iu.skipProperties,
                                                   skipSerialsNum = iu.skipSerialsNum,
                                                   packCost = iu.packCost,


                                               }).ToList();
                        itemsofferslist = itemsofferslist.Where(IO => (IO.isActiveOffer == 1 && DateTime.Compare(((DateTime)IO.startDate).Date, datenow.Date) <= 0 && System.DateTime.Compare(((DateTime)IO.endDate).Date, datenow.Date) >= 0 && IO.defaultSale == 1 && IO.itemCount > IO.used)
                                              && (((DateTime)IO.startDate)).TimeOfDay <= datenow.TimeOfDay && ((DateTime)IO.endDate).TimeOfDay >= datenow.TimeOfDay)
                                              .Distinct().ToList();

                        foreach (var iunlist in itemUnitsList)
                        {
                            //unit untity
                            iunlist.itemCount = ilc.getBranchAmount((int)iunlist.itemUnitId, branchId);
                            // end is new
                            decimal? totaldis = 0;
                            iunlist.price = (decimal)iunlist.price + Calc.percentValue(iunlist.price, iunlist.taxes);
                            iunlist.priceTax = (decimal)iunlist.price + Calc.percentValue(iunlist.price, iunlist.taxes);
                            #region slices prices for item
                            iunlist.SalesPrices = entity.Prices.Where(x => x.itemUnitId == iunlist.itemUnitId && x.isActive == true)
                                                    .Select(x => new PriceModel()
                                                    {
                                                        price = x.price,
                                                        basicPrice = x.price,
                                                        sliceId = x.sliceId,
                                                        name = x.name,
                                                    }).ToList();

                            foreach (var pr in iunlist.SalesPrices)
                            {
                                pr.priceTax = pr.price + Calc.percentValue(pr.price, iunlist.taxes);
                            }
                            #endregion
                            foreach (var itofflist in itemsofferslist)
                            {
                                if (iunlist.itemUnitId == itofflist.itemUnitId)
                                {
                                    // get unit name of item that has the offer
                                    using (incposdbEntities entitydb = new incposdbEntities())
                                    { // put it in item
                                        var un = entitydb.units
                                         .Where(a => a.unitId == itofflist.unitId)
                                            .Select(u => new
                                            {
                                                u.name
                                           ,
                                                u.unitId
                                            }).FirstOrDefault();
                                        iunlist.unitName = un.name;
                                    }
                                    iunlist.price = itofflist.price;
                                    iunlist.price = iunlist.price + (iunlist.price * iunlist.taxes / 100);

                                    iunlist.discountType = itofflist.discountType;
                                    iunlist.discountValue = itofflist.discountValue;
                                    if (iunlist.discountType == "1") // value
                                    {

                                        totaldis = totaldis + iunlist.discountValue;
                                    }
                                    else if (iunlist.discountType == "2") // percent
                                    {

                                        totaldis = totaldis + Calc.percentValue(iunlist.price, iunlist.discountValue);

                                    }
                                }
                            }
                            iunlist.priceTax = iunlist.priceTax - totaldis;
                        }
                        return TokenManager.GenerateToken(itemUnitsList);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }

        }


        [HttpPost]
        [Route("GetbyOfferId")]
        public string GetbyOfferId(string token)
        {

            // public string GetUsersByGroupId(string token)int itemId
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int offerId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "offerId")
                    {
                        offerId = int.Parse(c.Value);
                    }
                }

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemUnitsList = (from IU in entity.itemsUnits
                                             join IO in entity.itemsOffers on IU.itemUnitId equals IO.iuId
                                             join u in entity.units on IU.unitId equals u.unitId

                                             join i in entity.items on IU.itemId equals i.itemId
                                             orderby i.name
                                             where IO.offerId == offerId
                                             select new ItemUnitModel()
                                             {
                                                 itemUnitId = IU.itemUnitId,
                                                 unitId = IU.unitId,
                                                 itemId = IU.itemId,
                                                 mainUnit = u.name,
                                                 createDate = IU.createDate,
                                                 createUserId = IU.createUserId,
                                                 defaultPurchase = IU.defaultPurchase,
                                                 defaultSale = IU.defaultSale,
                                                 price = IU.price,
                                                 cost = IU.cost,
                                                 subUnitId = IU.subUnitId,
                                                 unitValue = IU.unitValue,
                                                 barcode = IU.barcode,
                                                 updateDate = IU.updateDate,
                                                 updateUserId = IU.updateUserId,
                                                 itemName = i.name,
                                                 itemCode = i.code,
                                                 unitName = u.name,
                                                 storageCostId = IU.storageCostId,
                                                 warrantyId = IU.warrantyId,
                                                 hasWarranty = IU.hasWarranty,
                                                 skipProperties = IU.skipProperties,
                                                 skipSerialsNum = IU.skipSerialsNum,
                                                 packCost = IU.packCost,
                                             }).ToList();

                        return TokenManager.GenerateToken(itemUnitsList);
                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        [HttpPost]
        [Route("getSmallItemUnits")]
        public string getSmallItemUnits(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int itemId = 0;
                int itemUnitId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        itemId = int.Parse(c.Value);
                    }
                    else if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);

                    }
                }

                try
                {
                    var units = getSmallItemUnits(itemId, itemUnitId);
                    return TokenManager.GenerateToken(units);
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        public List<ItemUnitModel> getSmallItemUnits(int itemId, int itemUnitId)
        {
            itemsPropController pc = new itemsPropController();
            using (incposdbEntities entity = new incposdbEntities())
            {
                // get all sub item units 
                List<itemsUnits> unitsList = entity.itemsUnits
                 .ToList().Where(x => x.itemId == itemId)
                  .Select(p => new itemsUnits
                  {
                      itemUnitId = p.itemUnitId,
                      unitId = p.unitId,
                      subUnitId = p.subUnitId,

                  })
                 .ToList();

                var unitId = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => x.unitId).Single();
                itemUnitsIds = new List<int>();
                itemUnitsIds.Add(itemUnitId);

                var result = Recursive(unitsList, (int)unitId);

                var units = (from iu in entity.itemsUnits.Where(x => x.itemId == itemId)
                             join u in entity.units on iu.unitId equals u.unitId
                             select new ItemUnitModel()
                             {
                                 unitId = iu.unitId,
                                 itemUnitId = iu.itemUnitId,
                                 subUnitId = iu.subUnitId,
                                 mainUnit = u.name,

                             }).Where(p => !itemUnitsIds.Contains((int)p.itemUnitId)).ToList();

                foreach (var u in units)
                {
                    u.unitValue = getUnitConversionQuan(itemUnitId, u.itemUnitId);
                    u.ItemProperties = pc.GetByItemUnitId((int)u.itemUnitId);
                }
                return units;
            }
        }
        public IEnumerable<itemsUnits> Recursive(List<itemsUnits> unitsList, int smallLevelid)
        {
            List<itemsUnits> inner = new List<itemsUnits>();

            foreach (var t in unitsList.Where(item => item.subUnitId == smallLevelid && item.unitId != smallLevelid))
            {

                itemUnitsIds.Add(t.itemUnitId);
                inner.Add(t);

                if (t.unitId.Value == smallLevelid)
                    return inner;
                inner = inner.Union(Recursive(unitsList, t.unitId.Value)).ToList();
            }

            return inner;
        }

        [HttpPost]
        [Route("largeToSmallUnitQuan")]
        public string largeToSmallUnitQuan(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int fromItemUnit = 0;
                int toItemUnit = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "fromItemUnit")
                    {
                        fromItemUnit = int.Parse(c.Value);
                    }
                    else if (c.Type == "toItemUnit")
                    {
                        toItemUnit = int.Parse(c.Value);

                    }
                }
                try
                {
                    int amount = 0;
                    amount += getUnitConversionQuan(fromItemUnit, toItemUnit);
                    return TokenManager.GenerateToken(amount.ToString());
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        public int multiplyFactorWithSmallestUnit(int itemId, int itemUnitId)
        {
            int multiplyFactor = 1;
            using (incposdbEntities entity = new incposdbEntities())
            {
                //var itemUnits = (from i in entity.itemsUnits where (i.itemId == itemId) select (i.itemUnitId)).ToList();

                // var smallestUnit = entity.itemsUnits.Where(iu => itemUnits.Contains((int)iu.itemUnitId) && iu.unitId == iu.subUnitId).FirstOrDefault();
                var smallestUnit = entity.itemsUnits.Where(iu => iu.itemId == itemId && iu.unitId == iu.subUnitId && iu.isActive == 1).FirstOrDefault();

                if (smallestUnit != null && smallestUnit.itemUnitId.Equals(itemUnitId))
                    return multiplyFactor;
                if (smallestUnit != null)
                {
                    if (!smallestUnit.Equals(itemUnitId))
                        multiplyFactor = getUnitConversionQuan(itemUnitId, smallestUnit.itemUnitId);
                }
                return multiplyFactor;
            }
        }

        private int getUnitConversionQuan(int fromItemUnit, int toItemUnit)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == toItemUnit).FirstOrDefault();

                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId && x.isActive == 1)
                    .Select(x => new ItemUnitModel()
                    {
                        unitValue = x.unitValue,
                        itemUnitId = x.itemUnitId
                    }).FirstOrDefault();
                if (upperUnit != null)
                {
                    try
                    {
                        amount = (int)upperUnit.unitValue;
                    }
                    catch { }

                    if (fromItemUnit == upperUnit.itemUnitId)
                        return amount;

                    amount += (int)upperUnit.unitValue * getUnitConversionQuan(fromItemUnit, upperUnit.itemUnitId);
                }
                //if (fromItemUnit == upperUnit.itemUnitId)
                //    return amount;
                //if (upperUnit != null)
                //    amount += (int)upperUnit.unitValue * getUnitConversionQuan(fromItemUnit, upperUnit.itemUnitId);

                return amount;
            }
        }


        [HttpPost]
        [Route("smallToLargeUnitQuan")]
        public string smallToLargeUnitQuan(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int fromItemUnit = 0;
                int toItemUnit = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "fromItemUnit")
                    {
                        fromItemUnit = int.Parse(c.Value);
                    }
                    else if (c.Type == "toItemUnit")
                    {
                        toItemUnit = int.Parse(c.Value);

                    }
                }
                try
                {
                    int amount = 0;
                    amount = getLargeUnitConversionQuan(fromItemUnit, toItemUnit);
                    return TokenManager.GenerateToken(amount.ToString());
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }



        public int getLargeUnitConversionQuan(int fromItemUnit, int toItemUnit)
        {
            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == toItemUnit).Select(x => new { x.unitId, x.itemId, x.subUnitId, x.unitValue }).FirstOrDefault();
                var smallUnit = entity.itemsUnits.Where(x => x.unitId == unit.subUnitId && x.itemId == unit.itemId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (toItemUnit == smallUnit.itemUnitId)
                {
                    amount = 1;
                    return amount;
                }

                if (smallUnit != null)
                    amount += (int)unit.unitValue * getLargeUnitConversionQuan(fromItemUnit, smallUnit.itemUnitId);

                return amount;
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
                    itemsPropController pc = new itemsPropController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        invListm = (from IU in entity.itemsUnits
                                    join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                    join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                    where ITEM.type != "sr"
                                    select new ItemUnitModel
                                    {

                                        // item unit     
                                        itemName = ITEM.name,
                                        itemType = ITEM.type,
                                        unitName = UNIT.name,
                                        itemUnitId = IU.itemUnitId,

                                        itemId = IU.itemId,
                                        unitId = IU.unitId,


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
                        }).ToList();
                        foreach (ItemUnitModel row in serialList)
                        {
                            row.quantity = ilCnrlr.getAllItemAmount((int)row.itemUnitId, branchId);
                            row.PropertiesCount = entity.storeProperties.Where(s => s.branchId == branchId
                              && s.itemUnitId == row.itemUnitId
                              && s.itemsTransId == null && s.isActive == 1 && s.isSold == false
                              ).ToList().Sum(s => s.count);
                            row.serialsCount = entity.serials.Where(C => C.isSold == false && C.isActive == 1 && C.branchId == branchId && C.itemUnitId == row.itemUnitId).ToList().Count();
                            row.ItemProperties = pc.GetByItemUnitId((int)row.itemUnitId);
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

        public decimal calculatePackagePrice(int packageParentId)
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
                                       citemId = I.itemId,
                                       avgPurchasePrice = I.avgPurchasePrice,
                                   }).ToList();

                decimal avgPurchasePrice = 0;
                foreach (PackageModel item in packageIems)
                {
                    if (!item.type.Equals("sr"))
                    {
                        int multiplyFactor = multiplyFactorWithSmallestUnit((int)item.citemId, (int)item.childIUId);

                        avgPurchasePrice += item.quantity * multiplyFactor * (decimal)item.avgPurchasePrice;
                    }
                }

                var packageCost = entity.itemsUnits.Find(packageParentId);

                if(packageCost.packCost != null)
                    avgPurchasePrice += (decimal)packageCost.packCost;
                return avgPurchasePrice;
            }
        }
    }
}