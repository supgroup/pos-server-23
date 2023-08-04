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
using System.Threading.Tasks;
using POS_Server.Classes;
using System.Data.Entity.Validation;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/ItemsTransfer")]
    public class ItemsTransferController : ApiController
    {
        CountriesController cc = new CountriesController();
        StorePropertyController sc = new StorePropertyController();
        List<ItemTransferModel> salesItems;
        [HttpPost]
        [Route("Get")]
        public async Task<string> Get(string token)
        {
          token = TokenManager.readToken(HttpContext.Current.Request);var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int invoiceId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        invoiceId = int.Parse(c.Value);
                    }
                }
                try
                {
                    var transferList = await Get(invoiceId);                  
                    return TokenManager.GenerateToken(transferList);
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        public async Task<List<ItemTransferModel>> Get(int invoiceId)
        {
            serialsController sc = new serialsController();
            using (incposdbEntities entity = new incposdbEntities())
            {
                var transferList = (from t in entity.itemsTransfer.Where(x => x.invoiceId == invoiceId)
                                    join u in entity.itemsUnits on t.itemUnitId equals u.itemUnitId
                                    join i in entity.items on u.itemId equals i.itemId
                                    join un in entity.units on u.unitId equals un.unitId
                                    join inv in entity.invoices on t.invoiceId equals inv.invoiceId
                                    select new ItemTransferModel()
                                    {
                                        itemsTransId = t.itemsTransId,
                                        itemId = i.itemId,
                                        itemName = i.name,
                                        quantity = t.quantity,
                                        invoiceId = entity.invoiceOrder.Where(x => x.itemsTransferId == t.itemsTransId).Select(x => x.orderId).FirstOrDefault() == null ? 0 : entity.invoiceOrder.Where(x => x.itemsTransferId == t.itemsTransId).Select(x => x.orderId).FirstOrDefault(),
                                        invNumber = inv.invNumber,
                                        createUserId = t.createUserId,
                                        updateUserId = t.updateUserId,
                                        notes = t.notes,
                                        createDate = t.createDate,
                                        updateDate = t.updateDate,
                                        itemUnitId = u.itemUnitId,
                                        price = t.price,
                                        unitName = un.name,
                                        unitId = un.unitId,
                                        barcode = u.barcode,
                                        itemType = i.type,
                                        offerId = t.offerId,
                                        itemUnitPrice = t.itemUnitPrice,
                                        offerType = t.offerType,
                                        offerValue = t.offerValue,
                                        offerName = t.offerName,
                                        itemTax = t.itemTax,
                                        warrantyId = t.warrantyId,
                                        warrantyName = t.warrantyName,
                                        warrantyDescription = t.warrantyDescription,
                                        isTaxExempt= t.isTaxExempt,
                                        VATRatio = t.VATRatio,
                                        itemSerials = entity.serials.Where(S => S.itemsTransId == t.itemsTransId)
                                                        .Select(S => new SerialModel()
                                                        {
                                                            itemsTransId = S.itemsTransId,
                                                            itemUnitId = S.itemUnitId,
                                                            serialNum = S.serialNum
                                                        }).ToList(),
                                        packageItems = (from S in entity.packages
                                                        join IU in entity.itemsUnits on S.childIUId equals IU.itemUnitId
                                                        join I in entity.items on IU.itemId equals I.itemId
                                                        where S.parentIUId ==u.itemUnitId
                                                        select new ItemModel()
                                                        {
                                                            isActive = S.isActive,
                                                            name = I.name,
                                                            type = I.type,
                                                            unitName = IU.units.name,
                                                            itemCount = S.quantity,
                                                            itemUnitId = IU.itemUnitId,
                                                            warrantyName = IU.warranty.name,
                                                            warrantyId=IU.warrantyId,

                                                        }).ToList(),
                                       ItemStoreProperties = (from SP in entity.storeProperties
                                                          join SPV in entity.storePropertiesValues on SP.storeProbId equals SPV.storeProbId
                                                          where SP.itemsTransId == t.itemsTransId
                                                          select new StorePropertyModel()
                                                          {
                                                              storeProbId = SP.storeProbId,
                                                              itemUnitId = SP.itemUnitId,
                                                              itemsTransId = SP.itemsTransId,
                                                              serialId = SP.serialId,
                                                              count = SP.count,
                                                              isSold = SP.isSold,
                                                              storeProbValueId = SPV.storeProbValueId,
                                                              propertyId = SPV.propertyId,
                                                              propertyItemId = SPV.propertyItemId,
                                                              propName = SPV.propertyName,
                                                              propValue = SPV.propertyValue,
                                                              serialNum = SP.serials.serialNum,
                                                              propertyIndex = SPV.properties.propertyIndex,

                                                          }).ToList(),
                                    })
                                    .ToList();

                int sequence = 1;
                foreach(var item in transferList)
                {
                    item.sequence = sequence;
                    sequence++;
                    if(item.ItemStoreProperties != null)
                    {
                        item.ItemStoreProperties = getItemProperties(item.ItemStoreProperties);                        
                    }
                }
                return transferList;
            }
        }

        private List<StorePropertyModel> getItemProperties(List<StorePropertyModel> itemStoreProp)
        {
            List<StorePropertyModel> spm = new List<StorePropertyModel>();
            var properties = itemStoreProp
                .Select(x => new {
                    storePropId = x.storeProbId,
                }).Distinct().ToList();

            foreach (var p in properties)
            {
                StorePropertyModel stp = new StorePropertyModel();
                var propValues = itemStoreProp.Where(x => x.storeProbId == p.storePropId).ToList();

                stp.storeProbId = p.storePropId;
              
                foreach (var pv in propValues)
                {
                    stp.serialId = pv.serialId;
                    stp.serialNum = pv.serialNum;
                    stp.itemsTransId = pv.itemsTransId;
                    stp.itemUnitId = pv.itemUnitId;
                    stp.isSold = pv.isSold;
                    stp.count = pv.count;
                    stp.branchId = pv.branchId;
                    stp.propValue += pv.propName + ": " + pv.propValue + ", ";
                    stp.notes += pv.propertyItemId.ToString() + ",";
                }

                spm.Add(stp);
            }
            return spm;
        }
        [HttpPost]
        [Route("GetWithCost")]
        public string GetWithCost(string token)
        {
          token = TokenManager.readToken(HttpContext.Current.Request);var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int invoiceId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        invoiceId = int.Parse(c.Value);
                    }

                }

                try
                {
                    serialsController sc = new serialsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var transferList = (from t in entity.itemsTransfer.Where(x => x.invoiceId == invoiceId)
                                            join u in entity.itemsUnits on t.itemUnitId equals u.itemUnitId
                                            join i in entity.items on u.itemId equals i.itemId
                                            join un in entity.units on u.unitId equals un.unitId
                                            join inv in entity.invoices on t.invoiceId equals inv.invoiceId
                                            select new ItemTransferModel()
                                            {
                                                itemsTransId = t.itemsTransId,
                                                itemId = i.itemId,
                                                itemName = i.name,
                                                quantity = t.quantity,
                                                invoiceId = entity.invoiceOrder.Where(x => x.itemsTransferId == t.itemsTransId).Select(x => x.orderId).FirstOrDefault() == null? 0 : entity.invoiceOrder.Where(x => x.itemsTransferId == t.itemsTransId).Select(x => x.orderId).FirstOrDefault(),
                                                invNumber = inv.invNumber,
                                                createUserId = t.createUserId,
                                                updateUserId = t.updateUserId,
                                                notes = t.notes,
                                                createDate = t.createDate,
                                                updateDate = t.updateDate,
                                                itemUnitId = u.itemUnitId,
                                                price = u.cost,
                                                unitName = un.name,
                                                unitId = un.unitId,
                                                barcode = u.barcode,
                                                //   itemSerials = sc.Get(t.itemsTransId),
                                                itemSerials = entity.serials
                    .Where(S => S.itemsTransId == t.itemsTransId)
                    .Select(S => new SerialModel()
                    {
                        serialId = S.serialId,
                        itemsTransId = S.itemsTransId,
                        itemUnitId = S.itemUnitId,
                        createDate = S.createDate,
                        isActive = S.isActive,
                        serialNum = S.serialNum,
                        createUserId = S.createUserId
                    }).ToList(),
                                                // itemSerial = t.itemSerial,
                                                itemType = i.type,
                                                offerId = t.offerId,
                                                itemUnitPrice = t.itemUnitPrice,
                                                offerType = t.offerType,
                                                offerValue = t.offerValue,
                                                itemTax = t.itemTax,
                                            })
                                            .ToList();

                        return TokenManager.GenerateToken(transferList);
                    }

                }
                catch { return TokenManager.GenerateToken("0"); }
            }
        }

        // add or update item transfer
        [HttpPost]
        [Route("Save")]
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
                int invoiceId = 0;
                string Object = "";
                List<itemsTransfer> newObject = new List<itemsTransfer>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "invoiceId")
                    {
                        invoiceId = int.Parse(c.Value);
                    }
                }
                if (newObject != null)
                {

             try
                {
                  // delete old invoice items
                    using (incposdbEntities entity = new incposdbEntities())
                        {                         
                            List<itemsTransfer> items = entity.itemsTransfer.Where(x => x.invoiceId == invoiceId).ToList();
                            entity.itemsTransfer.RemoveRange(items);
                            entity.SaveChanges();

                            var invoice = entity.invoices.Find(invoiceId);
                            for (int i = 0; i < newObject.Count; i++)
                            {
                                long itemUnitId = (long)newObject[i].itemUnitId;

                                #region get avg price for item
                                var avgPrice = entity.items.Where(m => m.itemId == entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => x.itemId).FirstOrDefault()).Select(m => m.avgPurchasePrice).Single();
                                #endregion

                                itemsTransfer t;
                                if (newObject[i].createUserId == 0 || newObject[i].createUserId == null)
                                {
                                    Nullable<int> id = null;
                                    newObject[i].createUserId = id;
                                }
                            if (newObject[i].offerId == 0 )
                                {
                                    Nullable<int> id = null;
                                    newObject[i].offerId = id;
                                }
                                if (newObject[i].itemSerial == null)
                                    newObject[i].itemSerial = "";

                                var transferEntity = entity.Set<itemsTransfer>();

                                newObject[i].invoiceId = invoiceId;
                                newObject[i].createDate = cc.AddOffsetTodate(DateTime.Now);
                                newObject[i].updateDate = cc.AddOffsetTodate(DateTime.Now);
                                newObject[i].updateUserId = newObject[i].createUserId;
                                newObject[i].purchasePrice = avgPrice;

                            t = entity.itemsTransfer.Add(newObject[i]);
                                entity.SaveChanges();
                             
                                //if (orderId != 0)
                                //{
                                //    invoiceOrder invoiceOrder = new invoiceOrder()
                                //    {
                                //        invoiceId = invoiceId,
                                //        orderId = orderId,
                                //        quantity = (int)newObject[i].quantity,
                                //        itemsTransferId = t.itemsTransId,
                                //    };
                                //    entity.invoiceOrder.Add(invoiceOrder);
                                //}
                                if(newObject[i].offerId != null && invoice.invType =="s")
                                {
                                    int offerId = (int)newObject[i].offerId;
                                    var offer = entity.itemsOffers.Where(x => x.iuId == itemUnitId && x.offerId == offerId).FirstOrDefault();                         

                                    offer.used += (int)newObject[i].quantity;
                                }
                            }
                            entity.SaveChanges();
                            message ="1";
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

        public string save(List<itemsTransfer> newObject, long invoiceId)
        {
            string message = "";

                using (incposdbEntities entity2 = new incposdbEntities())
                {

                    List<itemsTransfer> items = entity2.itemsTransfer.Where(x => x.invoiceId == invoiceId).ToList();
                    entity2.itemsTransfer.RemoveRange(items);
                    entity2.SaveChanges();

                    var invoice = entity2.invoices.Find(invoiceId);
                    for (int i = 0; i < newObject.Count; i++)
                    {
                        long itemUnitId = (long)newObject[i].itemUnitId;

                        #region get avg price for item
                        var avgPrice = entity2.items.Where(m => m.itemId == entity2.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => x.itemId).FirstOrDefault()).Select(m => m.avgPurchasePrice).Single();
                        #endregion

                        itemsTransfer t;
                        if (newObject[i].createUserId == 0 || newObject[i].createUserId == null)
                        {
                            Nullable<int> id = null;
                            newObject[i].createUserId = id;
                        }
                        if (newObject[i].offerId == 0)
                        {
                            Nullable<int> id = null;
                            newObject[i].offerId = id;
                        }
                        if (newObject[i].itemSerial == null)
                            newObject[i].itemSerial = "";

                        var transferEntity = entity2.Set<itemsTransfer>();

                        newObject[i].invoiceId = (int)invoiceId;
                        newObject[i].createDate = cc.AddOffsetTodate(DateTime.Now);
                        newObject[i].updateDate = cc.AddOffsetTodate(DateTime.Now);
                        newObject[i].updateUserId = newObject[i].createUserId;
                        newObject[i].purchasePrice = avgPrice;

                        t = entity2.itemsTransfer.Add(newObject[i]);
                       // entity.SaveChanges();


                        if (newObject[i].offerId != null && invoice.invType == "s")
                        {
                            int offerId = (int)newObject[i].offerId;
                            var offer = entity2.itemsOffers.Where(x => x.iuId == itemUnitId && x.offerId == offerId).FirstOrDefault();

                            offer.used += (int)newObject[i].quantity;
                        }
                    }
                    entity2.SaveChanges();
                    message = "1";
                }          
            return message;
        }
        public string saveWithSerials(List<itemsTransfer> newObject, List<ItemTransferModel> itemsTransfer, long invoiceId, 
                                   int branchId, bool isSold,bool isSalesInv = true, byte isActive = 1 )
        {
            string message = "";
            itemsTransfer t = new itemsTransfer();
            using (incposdbEntities entity2 = new incposdbEntities())
                {

                #region remove item properties
                var pvL = entity2.storePropertiesValues.Where(x => x.storeProperties.itemsTransfer.invoiceId == invoiceId).ToList();
                entity2.storePropertiesValues.RemoveRange(pvL);
                entity2.SaveChanges();

                var pL = entity2.storeProperties.Where(x => x.itemsTransfer.invoiceId == invoiceId).ToList();
                entity2.storeProperties.RemoveRange(pL);
                entity2.SaveChanges();

                #endregion

                #region remove item transfer serials
                List<serials> serials = entity2.serials.Where(x => x.itemsTransfer.invoices.invoiceId == invoiceId).ToList();
                entity2.serials.RemoveRange(serials);
                entity2.SaveChanges();
                #endregion
                //remove items transfer
                List<itemsTransfer> items = entity2.itemsTransfer.Where(x => x.invoiceId == invoiceId).ToList();
                    entity2.itemsTransfer.RemoveRange(items);
                    entity2.SaveChanges();

                    var invoice = entity2.invoices.Find(invoiceId);
                    for (int i = 0; i < newObject.Count; i++)
                    {
                        long itemUnitId = (long)newObject[i].itemUnitId;

                        #region get avg price for item
                        var avgPrice = entity2.items.Where(m => m.itemId == entity2.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => x.itemId).FirstOrDefault()).Select(m => m.avgPurchasePrice).Single();
                        #endregion

                        if (newObject[i].createUserId == 0 || newObject[i].createUserId == null)
                        {
                            Nullable<int> id = null;
                            newObject[i].createUserId = id;
                        }
                        if (newObject[i].offerId == 0)
                        {
                            Nullable<int> id = null;
                            newObject[i].offerId = id;
                        }
                        if (newObject[i].itemSerial == null)
                            newObject[i].itemSerial = "";

                        var transferEntity = entity2.Set<itemsTransfer>();

                        newObject[i].invoiceId = (int)invoiceId;
                        newObject[i].createDate = cc.AddOffsetTodate(DateTime.Now);
                        newObject[i].updateDate = cc.AddOffsetTodate(DateTime.Now);
                        newObject[i].updateUserId = newObject[i].createUserId;
                        newObject[i].purchasePrice = avgPrice;

                        t = entity2.itemsTransfer.Add(newObject[i]);
                       entity2.SaveChanges();
                    newObject[i].itemsTransId = t.itemsTransId;
                    #region save returned serial 
                    if (itemsTransfer[i].returnedSerials != null)
                    {
                        foreach (var sr in itemsTransfer[i].returnedSerials)
                        {
                            var serial = new serials()
                            {
                                itemsTransId = t.itemsTransId,
                                itemUnitId = sr.itemUnitId,
                                serialNum = sr.serialNum,
                                isSold = isSalesInv,
                                branchId = branchId,
                                createDate = cc.AddOffsetTodate(DateTime.Now),
                                updateDate = cc.AddOffsetTodate(DateTime.Now),
                                isActive = isActive,
                                createUserId = newObject[i].createUserId,
                                updateUserId = newObject[i].createUserId,
                            };
                            if (isSold.Equals(true))
                            {
                                var matchSerials = entity2.serials.Where(x => x.isSold == false
                                                                        && x.serialNum == sr.serialNum && x.isActive == 1
                                                                        && x.itemUnitId == sr.itemUnitId && x.branchId == branchId).ToList();
                                foreach (var s in matchSerials)
                                {
                                    s.isSold = true;
                                }
                            }
                            entity2.serials.Add(serial);
                        }
                    }
                    #endregion
                    #region save serials
                    else if (itemsTransfer[i].itemSerials != null)
                        foreach (var sr in itemsTransfer[i].itemSerials)
                        {
                            var serial = new serials()
                            {
                                itemsTransId = t.itemsTransId,
                                itemUnitId = sr.itemUnitId,
                                serialNum=sr.serialNum,
                                isSold = isSalesInv,
                                branchId = branchId,
                                createDate = cc.AddOffsetTodate(DateTime.Now),
                                updateDate = cc.AddOffsetTodate(DateTime.Now),
                                isActive = isActive,
                                createUserId = newObject[i].createUserId,
                                updateUserId = newObject[i].createUserId,
                            };
                            if(isSold.Equals(true))
                            {
                                var matchSerials = entity2.serials.Where(x => x.isSold == false 
                                                                        && x.serialNum == sr.serialNum && x.isActive == 1 
                                                                        && x.itemUnitId == sr.itemUnitId && x.branchId == branchId).ToList();
                                foreach(var s in matchSerials)
                                {
                                    s.isSold = true;
                                }
                            }
                            entity2.serials.Add(serial);            
                    }
                    #endregion

                    if (newObject[i].offerId != null && invoice.invType == "s")
                    {
                        int offerId = (int)newObject[i].offerId;
                        var offer = entity2.itemsOffers.Where(x => x.iuId == itemUnitId && x.offerId == offerId).FirstOrDefault();

                        offer.used += (int)newObject[i].quantity;
                    }
                    entity2.SaveChanges();


                }
                #region save store properties 
                for (int i = 0; i < newObject.Count; i++)
                {
                    if (itemsTransfer[i].ItemStoreProperties != null)
                        foreach (var sr in itemsTransfer[i].ItemStoreProperties)
                        {
                            List<string> propValueIds = sr.notes.Split(',').ToList();
                            propValueIds = propValueIds.Where(x => x != "").Select(x => x.Trim()).ToList();

                            storeProperties sProp2 = new storeProperties();
                            var sProp = new storeProperties()
                            {
                                itemsTransId = newObject[i].itemsTransId,
                                itemUnitId = sr.itemUnitId,
                                count = sr.count,
                                isSold = isSalesInv,
                                branchId = branchId,
                                notes = "",
                                createDate = cc.AddOffsetTodate(DateTime.Now),
                                updateDate = cc.AddOffsetTodate(DateTime.Now),
                                isActive = isActive,
                                createUserId = newObject[i].createUserId,
                                updateUserId = newObject[i].createUserId,
                            };

                            sProp = entity2.storeProperties.Add(sProp);
                            entity2.SaveChanges();
                            int storeProbId = sProp.storeProbId;
                            int storePropbId2 = 0;
                            bool isExist = false;
                            int existStorPropId = 0;

                            #region add properties to store in purchase
                            if(isSalesInv==false && isActive == 1)//purchase invoice
                            {
                                //check if itemUnit property already exist in DB
                                existStorPropId = sc.checkPropertyInStore((int)sProp.itemUnitId, (int)sProp.branchId, propValueIds,false);
                                
                                if (existStorPropId != 0)
                                {
                                    isExist = true;
                                    var existProp = entity2.storeProperties.Find(existStorPropId);
                                    existProp.count += sProp.count;
                                    existProp.updateUserId = sProp.updateUserId;
                                }
                                else
                                {
                                    sProp.itemsTransId = null;
                                    sProp2 = entity2.storeProperties.Add(sProp);
                                }
                                entity2.SaveChanges();

                                storePropbId2 = sProp2.storeProbId;
                            }
                            #endregion

                            #region decrease properties from store in sales
                           else if (isSalesInv == true && isActive == 1)//sales invoice
                            {
                                //check if itemUnit property already exist in DB
                                existStorPropId = sc.checkPropertyInStore((int)sProp.itemUnitId, (int)sProp.branchId, propValueIds, true);
                                //increase sale properties amount
                                if (existStorPropId != 0)
                                {
                                    isExist = true;
                                    var existProp = entity2.storeProperties.Find(existStorPropId);
                                    existProp.count += sProp.count;
                                    existProp.updateUserId = sProp.updateUserId;
                                }
                                else
                                {
                                    sProp.itemsTransId = null;
                                    sProp2 = entity2.storeProperties.Add(sProp);
                                }
                                entity2.SaveChanges();

                                storePropbId2 = sProp2.storeProbId;
                                //decrease availble properties
                                sc.decreaseStorePropertyCount((int) sr.itemUnitId,  branchId, propValueIds,sr.count,false,(int)newObject[i].updateUserId);
                              
                            }
                            #endregion
                            foreach (var pv in propValueIds)
                            {
                                if (!pv.Equals(""))
                                { 
                                    int propValId = int.Parse(pv.Trim());
                                    var pvModel = entity2.propertiesItems.Where(x => x.propertyItemId == propValId)
                                                    .Select(x => new PropertiesItemModel()
                                                    {
                                                        propertyId = x.propertyId,
                                                        propertyName = x.properties.name,
                                                        propertyItemName = x.name,
                                                    }).FirstOrDefault();

                                    var storePropValue = new storePropertiesValues()
                                    {
                                        propertyId = pvModel.propertyId,
                                        propertyItemId = propValId,
                                        propertyName = pvModel.propertyName,
                                        propertyValue = pvModel.propertyItemName,
                                        storeProbId = storeProbId,
                                        createUserId = newObject[i].createUserId,
                                        updateUserId = newObject[i].createUserId,
                                        createDate = cc.AddOffsetTodate(DateTime.Now),
                                        updateDate = cc.AddOffsetTodate(DateTime.Now),
                                    };

                                    entity2.storePropertiesValues.Add(storePropValue);
                                    entity2.SaveChanges();

                                    #region add properties values to store
                                    if (  isActive == 1 && !isExist)//purchase invoice
                                    {
                                        var storPropValue2 = storePropValue;
                                        storPropValue2.storeProbId = storePropbId2;
                                        entity2.storePropertiesValues.Add(storPropValue2);
                                    }
                                    #endregion
                                    entity2.SaveChanges();

                                }
                            }
                        }

                }
                entity2.SaveChanges();
             
            #endregion

            message = "1";
                }          
            return message;
        }

       
       
       

        public bool PropertiesAmountsAvailable(List<ItemTransferModel> billdetails,int branchId)
        {
            bool isExist = false;
            using (incposdbEntities entity = new incposdbEntities())
            {
                foreach (var t in billdetails)
                {
                    if (t.ItemStoreProperties != null)
                    {
                        foreach (var p in t.ItemStoreProperties)
                        {
                            List<string> propValueIds = p.notes.Split(',').ToList();
                            propValueIds = propValueIds.Where(x => x != "").Select(x => x.Trim()).ToList();

                            var check = entity.storeProperties
                                .Where(x => x.itemsTransId == null && x.itemUnitId == t.itemUnitId && x.branchId == branchId && x.isActive == 1 && x.isSold == false).ToList();
                           
                            if (check.Count > 0)
                            {
                                foreach (var sp in check)
                                {
                                    isExist = false;
                                    var pv = entity.storePropertiesValues.Where(x => x.storeProbId == sp.storeProbId).ToList();
                                    foreach (var pvv in pv)
                                    {
                                        isExist = propValueIds.Contains(pvv.propertyItemId.ToString().Trim());
                                        if (!isExist)
                                            break;
                                    }
                                    if (isExist && pv.Count == propValueIds.Count)
                                    {
                                        //check properties count
                                        var storeAmount = entity.storeProperties
                                                .Where(x => x.itemsTransId == null && x.itemUnitId == t.itemUnitId
                                                && x.branchId == branchId && x.isActive == 1 && x.isSold == false
                                                 && x.storeProbId == sp.storeProbId).Select(x => x.count).Single();

                                        if (storeAmount < p.count)
                                            return false;

                                    }

                                }

                            }
                            else
                                return false;
                        }
                    }
                }
            }
            return true;

        }
        public bool ReturnedPropAmountsAvailable(List<ItemTransferModel> billdetails,int branchId)
        {
            bool isExist = false;
            using (incposdbEntities entity = new incposdbEntities())
            {
                foreach (var t in billdetails)
                {
                    if (t.ReturnedProperties != null)
                    {
                        foreach (var p in t.ReturnedProperties)
                        {
                            List<string> propValueIds = p.notes.Split(',').ToList();
                            propValueIds = propValueIds.Where(x => x != "").Select(x => x.Trim()).ToList();

                            var check = entity.storeProperties
                                .Where(x => x.itemsTransId == null && x.itemUnitId == t.itemUnitId && x.branchId == branchId && x.isActive == 1 && x.isSold == false).ToList();
                           
                            //if (check.Count > 0)
                            //{
                                foreach (var sp in check)
                                {
                                    isExist = false;
                                    var pv = entity.storePropertiesValues.Where(x => x.storeProbId == sp.storeProbId).ToList();
                                    foreach (var pvv in pv)
                                    {
                                        isExist = propValueIds.Contains(pvv.propertyItemId.ToString().Trim());
                                        if (!isExist)
                                            break;
                                    }
                                    if (isExist && pv.Count == propValueIds.Count)
                                    {
                                        //check properties count
                                        var storeAmount = entity.storeProperties
                                                .Where(x => x.itemsTransId == null && x.itemUnitId == t.itemUnitId
                                                && x.branchId == branchId && x.isActive == 1 && x.isSold == false
                                                 && x.storeProbId == sp.storeProbId).Select(x => x.count).Single();

                                        if (storeAmount < p.count)
                                            return false;

                                    }

                                }

                            //}
                            //else
                            //    return false;
                        }
                    }
                }
            }
            return true;

        }
        public string saveImExItems(List<itemsTransfer> newObject,List<ItemTransferModel> itemsTransfer, long invoiceId,long exportInvId,
                                    int exportBranchId,int importBranchId, bool final=true)
        {
            string message = "";

            using (incposdbEntities entity2 = new incposdbEntities())
            {
                removeRelatedItemObjects(invoiceId);
                removeRelatedItemObjects(exportInvId);
                //#region  remove export item transfer serials
                //List<serials> serials = entity2.serials.Where(x => x.itemsTransfer.invoices.invoiceId == exportInvId).ToList();
                //entity2.serials.RemoveRange(serials);
                //entity2.SaveChanges();
                //#endregion
                //List<itemsTransfer> items = entity2.itemsTransfer.Where(x => x.invoiceId == invoiceId).ToList();
                //    List<itemsTransfer> expItems = entity2.itemsTransfer.Where(x => x.invoiceId == exportInvId).ToList();
                //    entity2.itemsTransfer.RemoveRange(items);
                //    entity2.itemsTransfer.RemoveRange(expItems);
                //    entity2.SaveChanges();


                for (int i = 0; i < newObject.Count; i++)
                {
                    itemsTransfer importItem;
                    itemsTransfer exportItem;

                    if (newObject[i].createUserId == 0 || newObject[i].createUserId == null)
                    {
                        Nullable<int> id = null;
                        newObject[i].createUserId = id;
                    }
                    if (newObject[i].offerId == 0)
                    {
                        Nullable<int> id = null;
                        newObject[i].offerId = id;
                    }
                    if (newObject[i].itemSerial == null)
                        newObject[i].itemSerial = "";

                    newObject[i].createDate = cc.AddOffsetTodate(DateTime.Now);
                    newObject[i].updateDate = cc.AddOffsetTodate(DateTime.Now);
                    newObject[i].updateUserId = newObject[i].createUserId;

                importItem = newObject[i];
                importItem.invoiceId = (int)invoiceId;
                    

                importItem = entity2.itemsTransfer.Add(importItem);
                entity2.SaveChanges();
                    int importItemId = importItem.itemsTransId;

                exportItem = newObject[i];
                exportItem.invoiceId = (int)exportInvId;
                exportItem = entity2.itemsTransfer.Add(exportItem);
                entity2.SaveChanges();
                   int exportItemId = exportItem.itemsTransId;

                if (itemsTransfer[i].itemSerials != null)
                {
                    foreach(var sr in itemsTransfer[i].itemSerials)
                    {
                        #region save serials with export invoice
                        byte isActive = 1;
                        if (final == false)
                            isActive = 0;

                        var serial = new serials()
                        {
                            serialNum = sr.serialNum,
                            branchId = importBranchId,
                            createDate = cc.AddOffsetTodate(DateTime.Now),
                            updateDate = cc.AddOffsetTodate(DateTime.Now),
                            createUserId = exportItem.createUserId,
                            updateUserId = exportItem.createUserId,
                            itemsTransId = exportItem.itemsTransId,
                            isActive = isActive,
                            isSold = false,
                            itemUnitId = exportItem.itemUnitId,                               
                    };
                        entity2.serials.Add(serial);
                        #endregion

                        #region unactivate creator branch serials
                        if (final == true)
                        {
                            var matchesSerials = entity2.serials.Where(x => x.serialNum == sr.serialNum && x.branchId == exportBranchId && x.isSold == false && x.isActive == 1).ToList();
                            foreach (var s in matchesSerials)
                                s.isActive = 0;
                        }
                        #endregion
                        entity2.SaveChanges();
                    }
                }

                    #region save properties with transfer
                if(itemsTransfer[i].ItemStoreProperties != null)
                    foreach (var sr in itemsTransfer[i].ItemStoreProperties)
                    {
                        List<string> propValueIds = sr.notes.Split(',').ToList();
                        propValueIds = propValueIds.Where(x => x != "").Select(x => x.Trim()).ToList();
                         
                        storeProperties secondBranchProp = new storeProperties();
                        var sProp = new storeProperties()
                        {
                            itemsTransId = exportItemId,
                            itemUnitId = sr.itemUnitId,
                            count = sr.count,
                            isSold = false,
                            branchId = exportBranchId,
                            notes = "",
                            createDate = cc.AddOffsetTodate(DateTime.Now),
                            updateDate = cc.AddOffsetTodate(DateTime.Now),
                            isActive = 1,
                            createUserId = newObject[i].createUserId,
                            updateUserId = newObject[i].createUserId,
                        };

                        sProp = entity2.storeProperties.Add(sProp);
                        entity2.SaveChanges();

                        int storeProbId = sProp.storeProbId;
                        bool isExist = false;
                        int existStorPropId = 0;

                        #region move properties to store in second branch
                        //check if itemUnit property already exist in DB
                        existStorPropId = sc.checkPropertyInStore((int)sProp.itemUnitId, importBranchId, propValueIds, false);

                        if (existStorPropId != 0)
                        {
                            isExist = true;
                            var existProp = entity2.storeProperties.Find(existStorPropId);
                            existProp.count += sProp.count;
                            existProp.updateUserId = sProp.updateUserId;
                        }
                        else
                        {
                            sProp.itemsTransId = null;
                                sProp.branchId = importBranchId;
                            secondBranchProp = entity2.storeProperties.Add(sProp);
                        }
                        entity2.SaveChanges();

                        sc.decreaseStorePropertyCount((int)sProp.itemUnitId,exportBranchId,propValueIds,sProp.count,false, (int)newObject[i].createUserId);

                        #endregion

                      
                        foreach (var pv in propValueIds)
                        {
                            if (!pv.Equals(""))
                            {
                                int propValId = int.Parse(pv.Trim());
                                var pvModel = entity2.propertiesItems.Where(x => x.propertyItemId == propValId)
                                                .Select(x => new PropertiesItemModel()
                                                {
                                                    propertyId = x.propertyId,
                                                    propertyName = x.properties.name,
                                                    propertyItemName = x.name,
                                                }).FirstOrDefault();

                                var storePropValue = new storePropertiesValues()
                                {
                                    propertyId = pvModel.propertyId,
                                    propertyItemId = propValId,
                                    propertyName = pvModel.propertyName,
                                    propertyValue = pvModel.propertyItemName,
                                    storeProbId = storeProbId,
                                    createUserId = newObject[i].createUserId,
                                    updateUserId = newObject[i].createUserId,
                                    createDate = cc.AddOffsetTodate(DateTime.Now),
                                    updateDate = cc.AddOffsetTodate(DateTime.Now),
                                };

                                entity2.storePropertiesValues.Add(storePropValue);
                                entity2.SaveChanges();

                                #region add properties values to store
                                if ( !isExist)//purchase invoice
                                {
                                    var storPropValue2 = storePropValue;
                                    storPropValue2.storeProbId = secondBranchProp.storeProbId;
                                    entity2.storePropertiesValues.Add(storPropValue2);
                                }
                                #endregion
                                entity2.SaveChanges();

                            }
                        }
                    }
                    #endregion
                }




                message = "1";
                }          
            return message;
        }
        [HttpPost]
        [Route("getOrderItems")]
        public string getOrderItems(string token)
        {
            string message = "";
            ItemsLocationsController ilc = new ItemsLocationsController();
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int branchId = 0;
                int invoiceId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                        branchId = int.Parse(c.Value);
                    else if (c.Type == "invoiceId")
                        invoiceId = int.Parse(c.Value);
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<ItemTransferModel> requiredTransfers = new List<ItemTransferModel>();
                        var itemsTransfer = entity.itemsTransfer.Where(x => x.invoiceId == invoiceId &&
                                            x.itemsUnits.items.type !="sr").ToList();
                        foreach (itemsTransfer tr in itemsTransfer)
                        {
                            var lockedQuantity = entity.itemsLocations
                                .Where(x => x.invoiceId == invoiceId && x.itemUnitId == tr.itemUnitId)
                                .Select(x => x.quantity).Sum();
                            var availableAmount = ilc.getBranchAmount((int)tr.itemUnitId, branchId);
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

                            long requiredQuantity = (long)tr.quantity - ((long)lockedQuantity + (long)availableAmount);
                            ItemTransferModel transfer = new ItemTransferModel()
                            {
                                invoiceId = invoiceId,
                                price = 0,
                                quantity = tr.quantity,
                                lockedQuantity = lockedQuantity,
                                newLocked = lockedQuantity,
                                availableQuantity = availableAmount,
                                itemUnitId = tr.itemUnitId,
                                itemId = item.itemId,
                                itemName = item.name,
                                unitName = item.unitName,
                            };
                            requiredTransfers.Add(transfer);

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

       private void removeRelatedItemObjects(long invoiceId)
        {
            using (incposdbEntities entity2 = new incposdbEntities())
            {
                #region remove item properties
                var pvL = entity2.storePropertiesValues.Where(x => x.storeProperties.itemsTransfer.invoiceId == invoiceId).ToList();
                entity2.storePropertiesValues.RemoveRange(pvL);
                entity2.SaveChanges();

                var pL = entity2.storeProperties.Where(x => x.itemsTransfer.invoiceId == invoiceId).ToList();
                entity2.storeProperties.RemoveRange(pL);
                entity2.SaveChanges();

                #endregion

                #region remove item transfer serials for return invoice
                List<serials> serials = entity2.serials.Where(x => x.itemsTransfer.invoices.invoiceId == invoiceId).ToList();
                entity2.serials.RemoveRange(serials);
                entity2.SaveChanges();
                #endregion
                #region remove items transfer for return invoice
                List<itemsTransfer> items = entity2.itemsTransfer.Where(x => x.invoiceId == invoiceId).ToList();
                entity2.itemsTransfer.RemoveRange(items);
                entity2.SaveChanges();
                #endregion
            }
        }
        public string saveReturnItems(List<itemsTransfer> returnedItems, List<ItemTransferModel> returnedItemsModel, long returnInvoiceId,
                                        long newSaleId, List<ItemTransferModel> salesItemsTransfer, int branchId,int sliceId)
        {
            string message = "";
            Calculate calc = new Calculate();
            salesItems = salesItemsTransfer;
            using (incposdbEntities entity2 = new incposdbEntities())
            {
                removeRelatedItemObjects(returnInvoiceId);
              
                var invoice = entity2.invoices.Find(returnInvoiceId);
                for (int i = 0; i < returnedItems.Count; i++)
                {
                    long itemUnitId = (long)returnedItems[i].itemUnitId;

                    
                    #region get avg price for item
                    var avgPrice = entity2.items.Where(m => m.itemId == entity2.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => x.itemId).FirstOrDefault()).Select(m => m.avgPurchasePrice).Single();
                    #endregion

                    itemsTransfer t;
                    if (returnedItems[i].createUserId == 0 || returnedItems[i].createUserId == null)
                    {
                        Nullable<int> id = null;
                        returnedItems[i].createUserId = id;
                    }
                    if (returnedItems[i].offerId == 0)
                    {
                        Nullable<int> id = null;
                        returnedItems[i].offerId = id;
                    }
                    if (returnedItems[i].itemSerial == null)
                        returnedItems[i].itemSerial = "";

                    var transferEntity = entity2.Set<itemsTransfer>();

                    returnedItems[i].invoiceId = (int)returnInvoiceId;
                    returnedItems[i].createDate = cc.AddOffsetTodate(DateTime.Now);
                    returnedItems[i].updateDate = cc.AddOffsetTodate(DateTime.Now);
                    returnedItems[i].updateUserId = returnedItems[i].createUserId;
                    returnedItems[i].purchasePrice = avgPrice;

                    t = entity2.itemsTransfer.Add(returnedItems[i]);
                    entity2.SaveChanges();

                    if (returnedItemsModel[i].returnedSerials != null)
                        foreach (var sr in returnedItemsModel[i].returnedSerials)
                        {
                            var serial = new serials()
                            {
                                itemsTransId = t.itemsTransId,
                                itemUnitId = sr.itemUnitId,
                                serialNum = sr.serialNum,
                                isSold = false,
                                branchId= branchId,
                                createDate = cc.AddOffsetTodate(DateTime.Now),
                                updateDate = cc.AddOffsetTodate(DateTime.Now),
                                isActive = 0,
                                createUserId = returnedItems[i].createUserId,
                                updateUserId = returnedItems[i].createUserId,
                            };
                            entity2.serials.Add(serial);
                            var prevSerials = entity2.serials.Where(x => x.itemUnitId == sr.itemUnitId && x.isActive == 1 && x.serialNum == sr.serialNum && x.branchId == branchId).ToList();
                            foreach (var s in prevSerials)
                            {
                                s.isSold = false;
                                var invtype = entity2.itemsTransfer.Where(x => x.itemsTransId == s.itemsTransId).Select(x => x.invoices.invType).FirstOrDefault();
                                if(invtype != null && invtype =="s")
                                    s.isActive = 0;
                            }
                            entity2.SaveChanges();


                        }

                    #region return offer quantity
                    if (returnedItems[i].offerId != null )
                    {
                        int offerId = (int)returnedItems[i].offerId;
                        var offer = entity2.itemsOffers.Where(x => x.iuId == itemUnitId && x.offerId == offerId).FirstOrDefault();

                        offer.used -= (int)returnedItems[i].quantity;
                    }
                    #endregion

                    entity2.SaveChanges();

                    #region calculate new sales quantity
                    updateItemTransferQuantity(itemUnitId,(long)returnedItems[i].quantity,"sales",sliceId);
                    #endregion

                }

                #region save store properties 
                for (int i = 0; i < returnedItems.Count; i++)
                {
                    if (returnedItemsModel[i].ReturnedProperties != null)
                        foreach (var sr in returnedItemsModel[i].ReturnedProperties)
                        {
                            List<string> propValueIds = sr.notes.Split(',').ToList();
                            propValueIds = propValueIds.Where(x => x != "").Select(x => x.Trim()).ToList();

                            storeProperties sProp2 = new storeProperties();
                            var sProp = new storeProperties()
                            {
                                itemsTransId = returnedItems[i].itemsTransId,
                                itemUnitId = sr.itemUnitId,
                                count = sr.count,
                                isSold = false,
                                branchId = branchId,
                                notes = "",
                                createDate = cc.AddOffsetTodate(DateTime.Now),
                                updateDate = cc.AddOffsetTodate(DateTime.Now),
                                isActive = 0,
                                createUserId = returnedItems[i].createUserId,
                                updateUserId = returnedItems[i].createUserId,
                            };

                            sProp = entity2.storeProperties.Add(sProp);
                            entity2.SaveChanges();
                            int storeProbId = sProp.storeProbId;

                            #region decrease properties from sales and increase availble store 
                            sc.decreaseStorePropertyCount((int)sr.itemUnitId, branchId, propValueIds, sr.count, true, (int)returnedItems[i].updateUserId);
                            sc.increaseStorePropertyCount((int)sr.itemUnitId, branchId, propValueIds, sr.count, false, (int)returnedItems[i].updateUserId);

                            #endregion
                            foreach (var pv in propValueIds)
                            {
                                if (!pv.Equals(""))
                                {
                                    int propValId = int.Parse(pv.Trim());
                                    var pvModel = entity2.propertiesItems.Where(x => x.propertyItemId == propValId)
                                                    .Select(x => new PropertiesItemModel()
                                                    {
                                                        propertyId = x.propertyId,
                                                        propertyName = x.properties.name,
                                                        propertyItemName = x.name,
                                                    }).FirstOrDefault();

                                    var storePropValue = new storePropertiesValues()
                                    {
                                        propertyId = pvModel.propertyId,
                                        propertyItemId = propValId,
                                        propertyName = pvModel.propertyName,
                                        propertyValue = pvModel.propertyItemName,
                                        storeProbId = storeProbId,
                                        createUserId = returnedItems[i].createUserId,
                                        updateUserId = returnedItems[i].createUserId,
                                        createDate = cc.AddOffsetTodate(DateTime.Now),
                                        updateDate = cc.AddOffsetTodate(DateTime.Now),
                                    };

                                    entity2.storePropertiesValues.Add(storePropValue);
                                    entity2.SaveChanges();
                                }
                            }
                        }

                }
                entity2.SaveChanges();

                #endregion
                #region save new sales invoice items 
                var zeroItems = salesItems.Where(x => x.quantity == 0).ToList();

                foreach (var it in salesItems)
                {
                    if (it.quantity.Equals(0) && salesItemsTransfer.Where(x=> x.itemUnitId == it.itemUnitId).FirstOrDefault() == null)
                        salesItems.Remove(it);
                }

                string jsonStr = JsonConvert.SerializeObject(salesItems);
                List<itemsTransfer> salesItemsObj = JsonConvert.DeserializeObject<List<itemsTransfer>>(jsonStr, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
             

                for (int i = 0; i < salesItems.Count; i++)
                {
                    long itemUnitId = (long)salesItemsObj[i].itemUnitId;

                    #region get avg price for item
                    var avgPrice = entity2.items.Where(m => m.itemId == entity2.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => x.itemId).FirstOrDefault()).Select(m => m.avgPurchasePrice).Single();
                    #endregion

                    itemsTransfer t;


                    salesItemsObj[i].invoiceId = (int)newSaleId;
                    salesItemsObj[i].purchasePrice = avgPrice;
                    salesItemsObj[i].itemSerial = "";

                    t = entity2.itemsTransfer.Add(salesItemsObj[i]);
                    try
                    {
                        entity2.SaveChanges();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                return "Property: {0} Error: {1}"+validationError.PropertyName;
                            }
                        }
                    }
                    if (salesItems[i].itemSerials != null)
                    {
                        var found = returnedItemsModel.Where(x => x.itemUnitId == salesItems[i].itemUnitId).FirstOrDefault();
                        if(found != null)
                            salesItems[i].itemSerials = salesItems[i].itemSerials.Where(x => !found.returnedSerials.Any(e => e.serialNum == x.serialNum)).ToList();
                        foreach (var sr in salesItems[i].itemSerials)
                        {
                            var serial = new serials()
                            {
                                itemsTransId = t.itemsTransId,
                                itemUnitId = sr.itemUnitId,
                                serialNum = sr.serialNum,
                                isSold = true,
                                branchId = branchId,
                                createDate = cc.AddOffsetTodate(DateTime.Now),
                                updateDate = cc.AddOffsetTodate(DateTime.Now),
                                isActive = 1,
                                createUserId = salesItems[i].createUserId,
                                updateUserId = salesItems[i].createUserId,
                            };
                            entity2.serials.Add(serial);
                            entity2.SaveChanges();


                        }
                    }
                }

                #endregion
                message = "1";
            }
            return message;
        }

        public string saveReturnPurItems(List<itemsTransfer> returnedItems, List<ItemTransferModel> returnedItemsModel, long returnInvoiceId,
                                        long newSaleId,int branchId, List<ItemTransferModel> purItemsTransfer)
        {
            string message = "";
            Calculate calc = new Calculate();
            salesItems = purItemsTransfer;
            using (incposdbEntities entity2 = new incposdbEntities())
            {
                #region remove item properties
                var pvL = entity2.storePropertiesValues.Where(x => x.storeProperties.itemsTransfer.invoiceId == returnInvoiceId).ToList();
                entity2.storePropertiesValues.RemoveRange(pvL);
                entity2.SaveChanges();

                var pL = entity2.storeProperties.Where(x => x.itemsTransfer.invoiceId == returnInvoiceId).ToList();
                entity2.storeProperties.RemoveRange(pL);
                entity2.SaveChanges();

                #endregion
                #region remove item transfer serials for return invoice
                List<serials> serials = entity2.serials.Where(x => x.itemsTransfer.invoices.invoiceId == returnInvoiceId).ToList();
                entity2.serials.RemoveRange(serials);
                entity2.SaveChanges();
                #endregion
                #region remove items transfer for return invoice
                List<itemsTransfer> items = entity2.itemsTransfer.Where(x => x.invoiceId == returnInvoiceId).ToList();
                entity2.itemsTransfer.RemoveRange(items);
                entity2.SaveChanges();
                #endregion

                var invoice = entity2.invoices.Find(returnInvoiceId);
                for (int i = 0; i < returnedItems.Count; i++)
                {
                    long itemUnitId = (long)returnedItems[i].itemUnitId;

                    
                    #region get avg price for item
                    var avgPrice = entity2.items.Where(m => m.itemId == entity2.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => x.itemId).FirstOrDefault()).Select(m => m.avgPurchasePrice).Single();
                    #endregion

                    itemsTransfer t;
                    if (returnedItems[i].createUserId == 0 || returnedItems[i].createUserId == null)
                    {
                        Nullable<int> id = null;
                        returnedItems[i].createUserId = id;
                    }
                    if (returnedItems[i].offerId == 0)
                    {
                        Nullable<int> id = null;
                        returnedItems[i].offerId = id;
                    }
                    if (returnedItems[i].itemSerial == null)
                        returnedItems[i].itemSerial = "";

                    var transferEntity = entity2.Set<itemsTransfer>();

                    returnedItems[i].invoiceId = (int)returnInvoiceId;
                    returnedItems[i].createDate = cc.AddOffsetTodate(DateTime.Now);
                    returnedItems[i].updateDate = cc.AddOffsetTodate(DateTime.Now);
                    returnedItems[i].updateUserId = returnedItems[i].createUserId;
                    returnedItems[i].purchasePrice = avgPrice;

                    t = entity2.itemsTransfer.Add(returnedItems[i]);
                    entity2.SaveChanges();
                    returnedItems[i].itemsTransId = t.itemsTransId;
                    #region save returned item serials
                    if (returnedItemsModel[i].returnedSerials != null)
                        foreach (var sr in returnedItemsModel[i].returnedSerials)
                        {
                            var serial = new serials()
                            {
                                itemsTransId = t.itemsTransId,
                                itemUnitId = sr.itemUnitId,
                                serialNum = sr.serialNum,
                                isSold = false,
                                branchId = branchId,
                                createDate = cc.AddOffsetTodate(DateTime.Now),
                                updateDate = cc.AddOffsetTodate(DateTime.Now),
                                isActive = 0,
                                createUserId = returnedItems[i].createUserId,
                                updateUserId = returnedItems[i].createUserId,
                            };
                            entity2.serials.Add(serial);
                            var prevSerials = entity2.serials.Where(x => x.itemUnitId == sr.itemUnitId && x.isActive == 1&& x.branchId == branchId && x.serialNum == sr.serialNum).ToList();
                            foreach (var s in prevSerials)
                            {
                                s.isSold = false;
                                s.isActive = 0;
                            }
                            entity2.SaveChanges();


                        }
                    #endregion
                    #region calculate new purchase quantity
                    updateItemTransferQuantity(itemUnitId,(long)returnedItems[i].quantity,"purchase");
                    #endregion

                }

                #region save store properties 
                for (int i = 0; i < returnedItems.Count; i++)
                {
                    if (returnedItemsModel[i].ReturnedProperties != null)
                        foreach (var sr in returnedItemsModel[i].ReturnedProperties)
                        {
                            List<string> propValueIds = sr.notes.Split(',').ToList();
                            propValueIds = propValueIds.Where(x => x != "").Select(x => x.Trim()).ToList();

                            storeProperties sProp2 = new storeProperties();
                            var sProp = new storeProperties()
                            {
                                itemsTransId = returnedItems[i].itemsTransId,
                                itemUnitId = sr.itemUnitId,
                                count = sr.count,
                                isSold = false,
                                branchId = branchId,
                                notes = "",
                                createDate = cc.AddOffsetTodate(DateTime.Now),
                                updateDate = cc.AddOffsetTodate(DateTime.Now),
                                isActive = 0,
                                createUserId = returnedItems[i].createUserId,
                                updateUserId = returnedItems[i].createUserId,
                            };

                            sProp = entity2.storeProperties.Add(sProp);
                            entity2.SaveChanges();
                            int storeProbId = sProp.storeProbId;
                            int storePropbId2 = 0;
                            bool isExist = false;
                            int existStorPropId = 0;

                            #region decrease properties from store 
                            sc.decreaseStorePropertyCount((int)sr.itemUnitId, branchId, propValueIds, sr.count,false, (int)returnedItems[i].updateUserId);
                            
                            #endregion
                            foreach (var pv in propValueIds)
                            {
                                if (!pv.Equals(""))
                                {
                                    int propValId = int.Parse(pv.Trim());
                                    var pvModel = entity2.propertiesItems.Where(x => x.propertyItemId == propValId)
                                                    .Select(x => new PropertiesItemModel()
                                                    {
                                                        propertyId = x.propertyId,
                                                        propertyName = x.properties.name,
                                                        propertyItemName = x.name,
                                                    }).FirstOrDefault();

                                    var storePropValue = new storePropertiesValues()
                                    {
                                        propertyId = pvModel.propertyId,
                                        propertyItemId = propValId,
                                        propertyName = pvModel.propertyName,
                                        propertyValue = pvModel.propertyItemName,
                                        storeProbId = storeProbId,
                                        createUserId = returnedItems[i].createUserId,
                                        updateUserId = returnedItems[i].createUserId,
                                        createDate = cc.AddOffsetTodate(DateTime.Now),
                                        updateDate = cc.AddOffsetTodate(DateTime.Now),
                                    };

                                    entity2.storePropertiesValues.Add(storePropValue);
                                    entity2.SaveChanges();
                                }
                            }
                        }

                }
                entity2.SaveChanges();

                #endregion

                #region save new purchase invoice items 

                foreach (var it in salesItems)
                {
                    if (it.quantity.Equals(0) && purItemsTransfer.Where(x=> x.itemUnitId == it.itemUnitId).FirstOrDefault() == null)
                        salesItems.Remove(it);

                }

                string jsonStr = JsonConvert.SerializeObject(salesItems);
                List<itemsTransfer> purItemsObj = JsonConvert.DeserializeObject<List<itemsTransfer>>(jsonStr, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
             

                for (int i = 0; i < salesItems.Count; i++)
                {
                    long itemUnitId = (long)purItemsObj[i].itemUnitId;

                    #region get avg price for item
                    var avgPrice = entity2.items.Where(m => m.itemId == entity2.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => x.itemId).FirstOrDefault()).Select(m => m.avgPurchasePrice).Single();
                    #endregion

                    itemsTransfer t;


                    purItemsObj[i].invoiceId = (int)newSaleId;
                    purItemsObj[i].purchasePrice = avgPrice;
                    purItemsObj[i].itemSerial = "";

                    t = entity2.itemsTransfer.Add(purItemsObj[i]);
                    try
                    {
                        entity2.SaveChanges();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                return "Property: {0} Error: {1}"+validationError.PropertyName;
                            }
                        }
                    }
                    if (salesItems[i].itemSerials != null)
                    {
                        var found = returnedItemsModel.Where(x => x.itemUnitId == salesItems[i].itemUnitId).FirstOrDefault();
                        if (found != null)
                            salesItems[i].itemSerials = salesItems[i].itemSerials.Where(x => !found.returnedSerials.Any(e => e.serialNum == x.serialNum)).ToList();
                        foreach (var sr in salesItems[i].itemSerials)
                        {
                            var serial = new serials()
                            {
                                itemsTransId = t.itemsTransId,
                                itemUnitId = sr.itemUnitId,
                                serialNum = sr.serialNum,
                                isSold = false,
                                branchId = branchId,
                                createDate = cc.AddOffsetTodate(DateTime.Now),
                                updateDate = cc.AddOffsetTodate(DateTime.Now),
                                isActive = 1,
                                createUserId = salesItems[i].createUserId,
                                updateUserId = salesItems[i].createUserId,
                            };
                            entity2.serials.Add(serial);
                            entity2.SaveChanges();


                        }
                    }
                }

                #endregion
                message = "1";
            }
            return message;
        }

        private List<ItemTransferModel> updateItemTransferQuantity(long itemUnitId, long requiredAmount,string invType="sales",int sliceId=0)
        {

            Dictionary<string, int> dic = new Dictionary<string, int>();
            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemInInvoice = salesItems.Where(x => x.itemUnitId == itemUnitId ).FirstOrDefault();

               if(itemInInvoice != null)
                {
                    int availableAmount = (int)itemInInvoice.quantity;
 
                    if (availableAmount >= requiredAmount)
                    {
                        itemInInvoice.quantity = availableAmount - requiredAmount;
                        requiredAmount = 0;
                    }
                    else if (availableAmount > 0)
                    {
                        itemInInvoice.quantity = 0;
                        requiredAmount = requiredAmount - availableAmount;
                    }

                    if (requiredAmount == 0)
                        return salesItems;
                }
                if (requiredAmount != 0)
                {
                    dic = checkUpperUnit(itemUnitId,  requiredAmount,invType,sliceId);

                    var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                    var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();


                    if (dic["remainQuantity"] > 0)
                    {
                        var item = salesItems.Where(x => x.itemUnitId == itemUnitId).FirstOrDefault();

                        if (item != null)
                        {
                            item.quantity = dic["remainQuantity"];
                        }
                        else
                        {
                            var itemUnit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId)
                                    .Select(x => new ItemUnitModel()
                                    {
                                        taxes = x.items.taxes,
                                        price = x.price,
                                        cost = x.cost,
                                    }).SingleOrDefault();

                            decimal price = 0;
                            decimal basicPrice = 0;

                            if (invType.Equals("sales"))
                            {
                                //var itemsTax_bool = (from s in entity.setting.Where(x => x.name == "itemsTax_bool")
                                //                     join sv in entity.setValues on s.settingId equals sv.settingId
                                //                     select new { sv.value }).SingleOrDefault();

                                var priceSlices  = entity.Prices.Where(x => x.itemUnitId == itemUnitId && x.sliceId == sliceId && x.isActive == true)
                                                        .Select(x => new PriceModel()
                                                        {
                                                            price = x.price,
                                                            basicPrice = x.price,
                                                            sliceId = x.sliceId,
                                                            name = x.name,
                                                        }).FirstOrDefault();

                                if (priceSlices == null)
                                {
                                    //if (itemsTax_bool != null && itemsTax_bool.ToString() == "true")
                                    //{
                                    //    Calculate Calc = new Calculate();

                                    //    price = (decimal)itemUnit.price + Calc.percentValue((decimal)itemUnit.price, (decimal)itemUnit.taxes);

                                    //}
                                    //else
                                        price = (decimal)itemUnit.price;
                                    basicPrice = (decimal)itemUnit.price;

                                }
                                else
                                {
                                    //if (itemsTax_bool != null && itemsTax_bool.ToString() == "true")
                                    //{
                                    //    Calculate Calc = new Calculate();

                                    //    price = (decimal)priceSlices.price + Calc.percentValue((decimal)priceSlices.price, (decimal)itemUnit.taxes);

                                    //}
                                    //else
                                        price = (decimal)priceSlices.price;

                                    basicPrice = (decimal)priceSlices.price;

                                }

                            }
                            else
                                price = basicPrice = (decimal)itemUnit.cost;

                            ItemTransferModel itemL = new ItemTransferModel();
                            itemL.itemUnitId = (int)itemUnitId;
                            itemL.quantity = dic["remainQuantity"];
                            itemL.price = price;
                            itemL.itemUnitPrice = basicPrice;
                            itemL.createUserId = salesItems[0].createUserId;
                            itemL.updateUserId = salesItems[0].updateUserId;
                            salesItems.Add(itemL);

                        }
                    }
                    if (dic["requiredQuantity"] > 0)
                    {
                        checkLowerUnit(itemUnitId,  dic["requiredQuantity"]);
                    }

                }
            }
            return salesItems;

        }

        private Dictionary<string, int> checkUpperUnit(long itemUnitId,  long requiredAmount, string invType="sales",int sliceId =0)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            dic.Add("remainQuantity", 0);
            dic.Add("requiredQuantity", 0);
            dic.Add("isConsumed", 0);
            int remainQuantity = 0;
            long firstRequir = requiredAmount;
            decimal newQuant = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).FirstOrDefault();

                if (upperUnit != null)
                {
                    decimal unitValue = (decimal)upperUnit.unitValue;
                    int breakNum = (int)Math.Ceiling(requiredAmount / unitValue);
                    newQuant = (decimal)(breakNum * upperUnit.unitValue);
                    var itemInInvoice = salesItems.Where(x => x.itemUnitId == upperUnit.itemUnitId && x.quantity > 0).FirstOrDefault();

                    if (itemInInvoice != null)
                    {
                        dic["isConsumed"] = 1;

                        if (breakNum <= itemInInvoice.quantity)
                        {
                            itemInInvoice.quantity = itemInInvoice.quantity - breakNum;
                            entity.SaveChanges();
                            remainQuantity = (int)newQuant - (int)firstRequir;
                            requiredAmount = 0;
                            // return remainQuantity;
                            dic["remainQuantity"] = remainQuantity;
                            dic["requiredQuantity"] = 0;

                            return dic;
                        }
                        else
                        {
                            itemInInvoice.quantity = 0;
                            breakNum = (int)(breakNum - itemInInvoice.quantity);
                            requiredAmount = requiredAmount - ((int)itemInInvoice.quantity * (int)upperUnit.unitValue);
                        }
                    }
                    if (breakNum != 0)
                    {
                        dic = new Dictionary<string, int>();

                        dic = checkUpperUnit(upperUnit.itemUnitId,  breakNum,invType);
                        var item = salesItems.Where(x => x.itemUnitId == upperUnit.itemUnitId).FirstOrDefault();

                        if (item != null)
                        {
                            item.quantity = dic["remainQuantity"];
                        }
                        else // item unit doesn't exist in main invoice
                        {
                            decimal price = 0;
                            decimal basicPrice = 0;
                            string itemsTax_bool = "";
                            decimal itemTax = 0;
                            if (invType.Equals("sales"))
                            {
                                 itemsTax_bool = (from s in entity.setting.Where(x => x.name == "itemsTax_bool")
                                                     join sv in entity.setValues on s.settingId equals sv.settingId
                                                     select new { sv.value }).SingleOrDefault().ToString();
                                var priceSlices = entity.Prices.Where(x => x.itemUnitId == itemUnitId && x.sliceId == sliceId && x.isActive == true)
                                                       .Select(x => new PriceModel()
                                                       {
                                                           price = x.price,
                                                           basicPrice = x.price,
                                                           sliceId = x.sliceId,
                                                           name = x.name,
                                                       }).FirstOrDefault();

                                if (priceSlices == null)
                                {

                                    if (itemsTax_bool != null && itemsTax_bool.ToString() == "true")
                                    {
                                        Calculate Calc = new Calculate();
                                        itemTax = (decimal)entity.itemsUnits.Where(x => x.itemUnitId == upperUnit.itemUnitId).Select(x => x.items.taxes).SingleOrDefault();
                                        price = (decimal)upperUnit.price + Calc.percentValue((decimal)upperUnit.price, (decimal)itemTax);

                                    }
                                    else
                                        price = (decimal)upperUnit.price;

                                    basicPrice = (decimal)upperUnit.price;
                                }
                                else
                                {
                                    if (itemsTax_bool != null && itemsTax_bool.ToString() == "true")
                                    {
                                        Calculate Calc = new Calculate();
                                        itemTax = (decimal)entity.itemsUnits.Where(x => x.itemUnitId == upperUnit.itemUnitId).Select(x => x.items.taxes).SingleOrDefault();
                                        price = (decimal)priceSlices.price + Calc.percentValue((decimal)priceSlices.price, (decimal)itemTax);

                                    }
                                    else
                                        price = (decimal)priceSlices.price;

                                    basicPrice = (decimal)priceSlices.price;

                                }
                            }
                            else
                                price = basicPrice = (decimal)upperUnit.cost;

                            ItemTransferModel itemL = new ItemTransferModel();
                            itemL.itemUnitId = upperUnit.itemUnitId;
                            itemL.quantity = dic["remainQuantity"];
                            itemL.price = price;
                            itemL.itemUnitPrice = upperUnit.price;
                            itemL.createUserId = salesItems[0].createUserId;
                            itemL.updateUserId = salesItems[0].updateUserId;
                            itemL.offerType = 0;
                            itemL.offerType = 0;
                            itemL.createDate = cc.AddOffsetTodate(DateTime.Now);
                            itemL.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            if (itemsTax_bool != null && itemsTax_bool.ToString() == "true")
                                itemL.itemTax = itemTax;
                            else
                                itemL.itemTax = 0;
                            salesItems.Add(itemL);
                        }

                        if (dic["isConsumed"] == 0)
                        {
                            dic["requiredQuantity"] = (int)requiredAmount;
                            dic["remainQuantity"] = 0;
                        }
                        else
                        {
                            dic["remainQuantity"] = (int)newQuant - (int)firstRequir;
                            dic["requiredQuantity"] = breakNum * (int)upperUnit.unitValue;
                        }
                        return dic;
                    }
                }
                else
                {
                    dic["remainQuantity"] = 0;
                    dic["requiredQuantity"] =(int) requiredAmount;

                    return dic;
                }
            }
            return dic;
        }

        private Dictionary<string, int> checkLowerUnit(long itemUnitId,  long requiredAmount)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            int remainQuantity = 0;
            int firstRequir = (int)requiredAmount;
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
                    var itemInInvoice = salesItems.Where(x => x.itemUnitId == lowerUnit.itemUnitId && x.quantity > 0).FirstOrDefault();
                       
                    if(itemInInvoice != null)
                    {
                        if (breakNum <= itemInInvoice.quantity)
                        {
                            itemInInvoice.quantity = itemInInvoice.quantity - breakNum;
      
                            remainQuantity = (int)newQuant - firstRequir;
                            requiredAmount = 0;
                            // return remainQuantity;
                            dic.Add("remainQuantity", remainQuantity);
                            return dic;
                        }
                        else
                        {
                            itemInInvoice.quantity = 0;
                            breakNum = (int)(breakNum - itemInInvoice.quantity);
                            requiredAmount = requiredAmount - ((int)itemInInvoice.quantity / (int)unit.unitValue);
                            entity.SaveChanges();
                        }
                    }
                    if (itemUnitId == lowerUnit.itemUnitId)
                        return dic;
                    if (breakNum != 0)
                    {
                        dic = new Dictionary<string, int>();
                        dic = checkLowerUnit(lowerUnit.itemUnitId, breakNum);

                        dic["remainQuantity"] = (int)newQuant - firstRequir;
                        dic["requiredQuantity"] = breakNum;
                        return dic;
                    }
                }
            }
            return dic;
        }

        [HttpPost]
        [Route("GetAvailableProperties")]
        public string GetAvailableProperties(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region params
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
                #endregion
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var propertiesList = (from SP in entity.storeProperties
                                              join SPV in entity.storePropertiesValues on SP.storeProbId equals SPV.storeProbId
                                              where SP.itemsTransId == null && SP.branchId == branchId
                                              && SP.itemUnitId == itemUnitId && SP.isActive == 1 && SP.isSold == false
                                              && SP.count > 0
                                              select new StorePropertyModel()
                                              {
                                                  storeProbId = SP.storeProbId,
                                                  itemUnitId = SP.itemUnitId,
                                                  itemsTransId = SP.itemsTransId,
                                                  serialId = SP.serialId,
                                                  count = SP.count,
                                                  isSold = SP.isSold,
                                                  storeProbValueId = SPV.storeProbValueId,
                                                  propertyId = SPV.propertyId,
                                                  propertyItemId = SPV.propertyItemId,
                                                  propName = SPV.propertyName,
                                                  propValue = SPV.propertyValue,
                                                  serialNum = SP.serials.serialNum,
                                                  propertyIndex = SPV.properties.propertyIndex,

                                              }).ToList();

                        var List = getItemProperties(propertiesList);
                        return TokenManager.GenerateToken(List);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

    }
}