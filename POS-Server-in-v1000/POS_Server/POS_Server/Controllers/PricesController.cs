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
    [RoutePrefix("api/Prices")]
    public class PricesController : ApiController
    {
        CountriesController cc = new CountriesController();
        // GET api/<controller> get all Prices
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
                    var itemList = entity.Prices

                   .Select(S => new PriceModel()
                   {
                       priceId = S.priceId,
                       itemUnitId = S.itemUnitId,
                       sliceId = S.sliceId,
                       notes = S.notes,
                       isActive = S.isActive,
                       createDate = S.createDate,
                       updateDate = S.updateDate,
                       createUserId = S.createUserId,
                       updateUserId = S.updateUserId,
                       price = S.price,
                       name = S.name,




                   })
                   .ToList();

                    // can delet or not
                    if (itemList.Count > 0)
                    {
                        foreach (PriceModel item in itemList)
                        {
                            canDelete = false;

                            int Id = (int)item.priceId;
                            var Pricesitem = entity.itemsTransfer.Where(x => x.priceId == Id).Select(x => new { x.priceId }).FirstOrDefault();

                            if ((Pricesitem is null))
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
                    var item = entity.Prices
                   .Where(S => S.priceId == Id)
                   .Select(S => new
                   {
                       S.priceId,
                       S.itemUnitId,
                       S.sliceId,
                       S.notes,
                       S.isActive,
                       S.createDate,
                       S.updateDate,
                       S.createUserId,
                       S.updateUserId,
                       S.price,
                       S.name,


                   })
                   .FirstOrDefault();
                    return TokenManager.GenerateToken(item);
                }
            }
        }

        //
        [HttpPost]
        [Route("getByitemUnitId")]
        public string getByitemUnitId(string token)
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
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var items = entity.Prices
                       .Where(S => S.itemUnitId == Id)
                       .Select(S => new
                       {
                           S.priceId,
                           S.itemUnitId,
                           S.sliceId,
                           S.notes,
                           S.isActive,
                           S.createDate,
                           S.updateDate,
                           S.createUserId,
                           S.updateUserId,
                           S.price,
                           S.name,
                           sliceName = S.slices.name,
                       }).ToList();
                        return TokenManager.GenerateToken(items);
                    }
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken("0");
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
                Prices newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        itemObject = c.Value.Replace("\\", string.Empty);
                        itemObject = itemObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<Prices>(itemObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        Prices tmpObject = new Prices();
                        var sliceEntity = entity.Set<Prices>();
                        DateTime datenow = cc.AddOffsetTodate(DateTime.Now);
                        if (newObject.priceId == 0)
                        {

                            newObject.createDate = datenow;
                            newObject.updateDate = datenow;
                            newObject.updateUserId = newObject.createUserId;
                            tmpObject = sliceEntity.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject.priceId.ToString();

                        }
                        else
                        {

                            tmpObject = entity.Prices.Where(p => p.priceId == newObject.priceId).FirstOrDefault();
                            tmpObject.priceId = newObject.priceId;
                            tmpObject.itemUnitId = newObject.itemUnitId;
                            tmpObject.sliceId = newObject.sliceId;
                            tmpObject.notes = newObject.notes;
                            tmpObject.isActive = newObject.isActive;
                            //  tmpObject.createDate = newObject.createDate;

                            //tmpObject.createUserId = newObject.createUserId;
                            tmpObject.updateUserId = newObject.updateUserId;
                            tmpObject.price = newObject.price;
                            tmpObject.name = newObject.name;



                            tmpObject.updateDate = datenow;// server current date;

                            entity.SaveChanges();
                            message = tmpObject.priceId.ToString();
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
                int priceId = 0;
                int userId = 0;
                bool final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        priceId = int.Parse(c.Value);
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
                            Prices Obj = entity.Prices.Find(priceId);
                            entity.Prices.Remove(Obj);
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
                            Prices Obj = entity.Prices.Find(priceId);

                            Obj.isActive = false;
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
        [Route("getBySliceId")]
        public string getBySliceId(string token)
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
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<PriceModel> items = entity.Prices
                         .Where(S => S.sliceId == Id)
                         .Select(S => new PriceModel
                         {
                             priceId = S.priceId,
                             itemUnitId = S.itemUnitId,
                             sliceId = S.sliceId,
                             notes = S.notes,
                             isActive = S.isActive,
                             createDate = S.createDate,
                             updateDate = S.updateDate,
                             createUserId = S.createUserId,
                             updateUserId = S.updateUserId,
                             price = S.price,
                             name = S.name,
                             sliceName = S.slices.name,
                             unitName = S.itemsUnits.units.name,
                             itemName = S.itemsUnits.items.name,
                             itemId = S.itemsUnits.itemId,
                             unitId = S.itemsUnits.unitId,
                             itemType = S.itemsUnits.items.type,
                             avgPurchasePrice = S.itemsUnits.items.avgPurchasePrice,
                             itemUnitPrice = S.itemsUnits.price,
                             itemUnitCost = S.itemsUnits.cost,

                         }).ToList();
                        StatisticsController sts = new StatisticsController();

                        foreach (var row in items)
                        {
                            decimal unitpurchasePrice = 0;
                            if (row.itemType == "p")
                            {
                                unitpurchasePrice = sts.AvgPackagePurPrice((int)row.itemUnitId);
                            }
                            else
                            {
                                if (row.avgPurchasePrice == null || row.avgPurchasePrice == 0)
                                {
                                    unitpurchasePrice = (decimal)row.itemUnitCost;
                                }
                                else
                                {
                                    unitpurchasePrice = sts.AvgPurPrice((int)row.itemUnitId, (int)row.itemId, row.avgPurchasePrice == null ? 0 : row.avgPurchasePrice);

                                }
                            }
                            row.unitCost = unitpurchasePrice;
                        }
                        return TokenManager.GenerateToken(items);
                    }
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken(ex.ToString());
                }


            }
        }
    }
}