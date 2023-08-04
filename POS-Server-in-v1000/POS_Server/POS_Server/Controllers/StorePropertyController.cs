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
    [RoutePrefix("api/StoreProperty")]
    public class StorePropertyController : ApiController
    {
        CountriesController cc = new CountriesController();

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
                                            propertyIndex = v.properties.propertyIndex,
                                        }).ToList().OrderBy(v => v.propertyIndex).ToList(),
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

        public bool PropertiesAmountsAvailable(List<string> propValueIds, int itemUnitId,int branchId,int count)
        {
            bool isExist = false;
            using (incposdbEntities entity = new incposdbEntities())
            {
                var check = entity.storeProperties
                    .Where(x => x.itemsTransId == null && x.itemUnitId == itemUnitId && x.branchId == branchId && x.isActive == 1 && x.isSold == false).ToList();

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
                                    .Where(x => x.itemsTransId == null && x.itemUnitId == itemUnitId
                                    && x.branchId == branchId && x.isActive == 1 && x.isSold == false
                                        && x.storeProbId == sp.storeProbId).Select(x => x.count).Single();

                            if (storeAmount < count)
                                return false;

                        }

                    }

                }
                else
                    return false;

            }
            return true;

        }
        [HttpPost]
        [Route("SoldProperties")]
        public string SoldProperties(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                string valIds = "";
                int itemUnitId = 0;
                int branchId = 0;
                int userId = 0;
                int count = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "valIds")
                    {
                        valIds = c.Value;
                    }
                    else if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "count")
                    {
                        count = int.Parse(c.Value);
                    }
                }
                try
                {
                    List<string> propValueIds = valIds.Split(',').ToList();
                    propValueIds = propValueIds.Where(x => x != "").Select(x => x.Trim()).ToList();

                    #region check properties in store
                    bool propCheck = PropertiesAmountsAvailable(propValueIds,itemUnitId, branchId,count);

                    if (!propCheck)
                    {
                        return TokenManager.GenerateToken("-10");
                    }
                    #endregion
                    decreaseStorePropertyCount(itemUnitId, branchId, propValueIds, count, false, userId);

                    int existStorPropId = checkPropertyInStore(itemUnitId, branchId, propValueIds, true);
                    if (existStorPropId != 0)
                    {
                        increaseStorePropertyCount(itemUnitId, branchId, propValueIds, count, true, userId);
                    }
                    else
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var sProp = new storeProperties()
                            {
                                itemUnitId = itemUnitId,
                                count = count,
                                isSold = true,
                                branchId = branchId,
                                notes = "",
                                createDate = cc.AddOffsetTodate(DateTime.Now),
                                updateDate = cc.AddOffsetTodate(DateTime.Now),
                                isActive = 1,
                                createUserId = userId,
                                updateUserId = userId,
                            };
                            sProp = entity.storeProperties.Add(sProp);
                            entity.SaveChanges();

                            foreach (var pv in propValueIds)
                            {
                                if (!pv.Equals(""))
                                {
                                    int propValId = int.Parse(pv.Trim());
                                    var pvModel = entity.propertiesItems.Where(x => x.propertyItemId == propValId)
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
                                        storeProbId = sProp.storeProbId,
                                        createUserId = userId,
                                        updateUserId = userId,
                                        createDate = cc.AddOffsetTodate(DateTime.Now),
                                        updateDate = cc.AddOffsetTodate(DateTime.Now),
                                    };

                                    entity.storePropertiesValues.Add(storePropValue);
                                    entity.SaveChanges();
                                }
                            }
                        }
                    }
                    return TokenManager.GenerateToken("1");
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }
        [HttpPost]
        [Route("AddProperties")]
        public string AddProperties(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                string valIds = "";
                int itemUnitId = 0;
                int branchId = 0;
                int userId = 0;
                int count = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "valIds")
                    {
                        valIds = c.Value;
                    }
                    else if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "count")
                    {
                        count = int.Parse(c.Value);
                    }
                }
              try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        #region compare item unit quantity in branch
                        ItemsLocationsController ic = new ItemsLocationsController();
                        int storeAmount = ic.getBranchAmount(itemUnitId, branchId);
                        //get item unit properties quantity
                        var propsCountList = entity.storeProperties
                                            .Where(x => x.itemUnitId == itemUnitId 
                                            && x.branchId == branchId && x.isActive == 1 
                                            && x.isSold == false
                                            && x.itemsTransId == null).Select(x => x.count).ToList();


                        long propsCount = 0;
                        if (propsCountList.Count > 0)
                        { 
                            propsCount = propsCountList.Select(x => x).Sum();
                        }

                        if ((propsCount + count) > storeAmount)
                            return TokenManager.GenerateToken("-1");
                        #endregion

                        List<string> propValueIds = valIds.Split(',').ToList();
                        propValueIds = propValueIds.Where(x => x != "").Select(x => x.Trim()).ToList();

                        int existStorPropId = checkPropertyInStore(itemUnitId, branchId, propValueIds, false);
                        if (existStorPropId != 0)
                        {
                            increaseStorePropertyCount(itemUnitId, branchId, propValueIds, count, false, userId);
                        }
                        else
                        {

                            var sProp = new storeProperties()
                            {
                                itemUnitId = itemUnitId,
                                count = count,
                                isSold = false,
                                branchId = branchId,
                                notes = "",
                                createDate = cc.AddOffsetTodate(DateTime.Now),
                                updateDate = cc.AddOffsetTodate(DateTime.Now),
                                isActive = 1,
                                createUserId = userId,
                                updateUserId = userId,
                            };
                            sProp = entity.storeProperties.Add(sProp);
                            entity.SaveChanges();

                            foreach (var pv in propValueIds)
                            {
                                if (!pv.Equals(""))
                                {
                                    int propValId = int.Parse(pv.Trim());
                                    var pvModel = entity.propertiesItems.Where(x => x.propertyItemId == propValId)
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
                                        storeProbId = sProp.storeProbId,
                                        createUserId = userId,
                                        updateUserId = userId,
                                        createDate = cc.AddOffsetTodate(DateTime.Now),
                                        updateDate = cc.AddOffsetTodate(DateTime.Now),
                                    };

                                    entity.storePropertiesValues.Add(storePropValue);
                                    entity.SaveChanges();
                                }

                            }
                        }
                        return TokenManager.GenerateToken("1");
                    }
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }
        [HttpPost]
        [Route("UpdateProperties")]
        public string UpdateProperties(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                string valIds = "";
                int itemUnitId = 0;
                int branchId = 0;
                int userId = 0;
                int count = 0;
                int storPropId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "valIds")
                    {
                        valIds = c.Value;
                    }
                    else if (c.Type == "itemUnitId")
                    {
                        itemUnitId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "count")
                    {
                        count = int.Parse(c.Value);
                    }
                    else if (c.Type == "storPropId")
                    {
                        storPropId = int.Parse(c.Value);
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<string> propValueIds = valIds.Split(',').ToList();
                        propValueIds = propValueIds.Where(x => x != "").Select(x => x.Trim()).ToList();

                        #region compare item unit quantity in branch
                        ItemsLocationsController ic = new ItemsLocationsController();
                        int storeAmount = ic.getAllItemAmount(itemUnitId, branchId);
                        //get item unit properties quantity
                        var propsCount = entity.storeProperties.Where(x => x.itemUnitId == itemUnitId 
                                            && x.branchId == branchId && x.isActive == 1
                                            && x.isSold == false && x.itemsTransId == null).ToList().Sum(x => x.count);

                        //int existStorPropId = checkPropertyInStore(itemUnitId, branchId, propValueIds, false);
                        //var prop = entity.storeProperties.Find(existStorPropId);
                        var prop = entity.storeProperties.Find(storPropId);
                        if ((propsCount - prop.count + count) > storeAmount)
                            return TokenManager.GenerateToken("-1");
                        #endregion

                        prop.count = count;
                        prop.updateDate = cc.AddOffsetTodate(DateTime.Now);
                        prop.updateUserId = userId;

                        entity.SaveChanges();
                    }
                    return TokenManager.GenerateToken("1");
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }
        public int checkPropertyInStore(int itemUnitId, int branchId, List<string> propValueIds, bool isSold)
        {
            bool isExist = false;
            int storePropId = 0;
            using (incposdbEntities entity2 = new incposdbEntities())
            {
                var check = entity2.storeProperties
                    .Where(x => x.itemsTransId == null && x.itemUnitId == itemUnitId && x.branchId == branchId && x.isActive == 1 && x.isSold == isSold).ToList();
                if (check.Count > 0)
                {
                    foreach (var p in check)
                    {
                        isExist = false;
                        var pv = entity2.storePropertiesValues.Where(x => x.storeProbId == p.storeProbId).ToList();
                        foreach (var pvv in pv)
                        {
                            isExist = propValueIds.Contains(pvv.propertyItemId.ToString());
                            if (!isExist)
                                break;

                        }
                        if (isExist && pv.Count == propValueIds.Count)
                        {
                            storePropId = p.storeProbId;
                            break;
                        }

                    }

                }
            }
            return storePropId;
        }
        public void decreaseStorePropertyCount(int itemUnitId, int branchId, List<string> propValueIds, long count, bool isSold, int userId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var storeProp = (from SP in entity.storeProperties
                                                     .Where(x => x.itemsTransId == null && x.itemUnitId == itemUnitId
                                                     && x.branchId == branchId && x.isActive == 1 && x.isSold == isSold)
                                 join SPV in entity.storePropertiesValues on SP.storeProbId equals SPV.storeProbId
                                 where propValueIds.Contains(SPV.propertyItemId.ToString())
                                 select new { SP.storeProbId }).FirstOrDefault();

                var existProp = entity.storeProperties.Find(storeProp.storeProbId);
                existProp.count -= count;
                existProp.updateUserId = userId;
                existProp.updateDate = cc.AddOffsetTodate(DateTime.Now);

                entity.SaveChanges();
            }
        }
        public void increaseStorePropertyCount(int itemUnitId, int branchId, List<string> propValueIds, long count, bool isSold, int userId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var storeProp = (from SP in entity.storeProperties
                                                     .Where(x => x.itemsTransId == null && x.itemUnitId == itemUnitId
                                                     && x.branchId == branchId && x.isActive == 1 && x.isSold == isSold)
                                 join SPV in entity.storePropertiesValues on SP.storeProbId equals SPV.storeProbId
                                 where propValueIds.Contains(SPV.propertyItemId.ToString())
                                 select new { SP.storeProbId }).FirstOrDefault();

                var existProp = entity.storeProperties.Find(storeProp.storeProbId);
                existProp.count += count;
                existProp.updateUserId = userId;
                existProp.updateDate = cc.AddOffsetTodate(DateTime.Now);

                entity.SaveChanges();
            }
        }

    }
}