using LinqKit;
using Newtonsoft.Json.Linq;
using POS_Server.Models;
using POS_Server.Models.VM;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Classes;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/PosEcommerce")]
    public class PosEcommerceController : ApiController
    {
        CountriesController cc = new CountriesController();
        public List<CategoryEcommerceModel> activeCategories { get; set; }
        // GET api/<controller>
        [HttpPost]
        [Route("GetDashBoardInfo")]
        public string GetDashBoardInfo(string token)
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
                int branchId = 0;
                int userId = 0;
                DateTime startDate = cc.AddOffsetTodate(DateTime.Now);

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

                    //else if (c.Type == "startDate")
                    //{
                    //    startDate = DateTime.Parse(c.Value);
                    //    startDate = DateTime.Parse(startDate.ToString().Split(' ')[0]);

                    //}

                }
                #endregion

                try
                {
                    WebDashBoardModel dashBoardModel = new WebDashBoardModel();
                    dashBoardModel.branchId = branchId;

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var searchPredicate = PredicateBuilder.New<invoices>();

                        BranchesController bc = new BranchesController();
                        var branchesList = bc.BranchesByUser(userId);
                        var branchIds = branchesList.Select(x => x.branchId).ToList();

                        #region purchases count
                        searchPredicate = searchPredicate.And(x => x.isActive == true && (x.invType == "p" || x.invType == "pw"));
                        if (branchId != 0)
                            searchPredicate = searchPredicate.And(x => x.branchId == branchId);
                        else if (userId != 2)
                        {
                            searchPredicate = searchPredicate.And(x => branchIds.Contains((int)x.branchId));
                        }

                        //if(endDate == null)
                        //    searchPredicate = searchPredicate.And(x => EntityFunctions.TruncateTime(x.updateDate) == startDate);
                        //else
                        //    searchPredicate = searchPredicate.And(x => EntityFunctions.TruncateTime(x.updateDate) >= startDate
                        //                                            && EntityFunctions.TruncateTime(x.updateDate) <= endDate);


                        dashBoardModel.purchasesCount = entity.invoices.Where(searchPredicate).ToList().Count();

                        #endregion

                        #region vendors count
                        searchPredicate = PredicateBuilder.New<invoices>();
                        searchPredicate = searchPredicate.And(x => x.isActive == true && (x.invType == "p" || x.invType == "pw"));
                        searchPredicate = searchPredicate.And(x => x.agentId != null);

                        dashBoardModel.vendorsCount = entity.invoices.Where(searchPredicate).Select(x => x.agentId).ToList().Distinct().Count();
                        #endregion

                        #region sales count
                        searchPredicate = PredicateBuilder.New<invoices>();

                        searchPredicate = searchPredicate.And(x => x.isActive == true && x.invType == "s");
                        if (branchId != 0)
                            searchPredicate = searchPredicate.And(x => x.branchId == branchId);
                        else if (userId != 2)
                        {
                            searchPredicate = searchPredicate.And(x => branchIds.Contains((int)x.branchId));
                        }

                        dashBoardModel.salesCount = entity.invoices.Where(searchPredicate).Count();


                        #endregion

                        #region customers count
                        searchPredicate = PredicateBuilder.New<invoices>();
                        searchPredicate = searchPredicate.And(x => x.isActive == true && x.invType == "s" && x.agentId != null);

                        dashBoardModel.customersCount = entity.invoices.Where(searchPredicate).Select(x => x.agentId).Distinct().Count();
                        #endregion

                        var posSearchPredicat = PredicateBuilder.New<pos>();
                        posSearchPredicat = posSearchPredicat.And(x => x.isActive == 1);

                        if (branchId != 0)
                            posSearchPredicat = posSearchPredicat.And(x => x.branchId == branchId);
                        else if (userId != 2)
                        {
                            posSearchPredicat = posSearchPredicat.And(x => branchIds.Contains((int)x.branchId));
                        }

                        #region online users
                        dashBoardModel.onLineUsersCount = (from log in entity.usersLogs
                                                           join p in entity.pos.Where(posSearchPredicat) on log.posId equals p.posId
                                                           join u in entity.users on log.userId equals u.userId

                                                           where (log.sOutDate == null && log.users.isOnline == 1)

                                                           select new
                                                           {
                                                               log.userId,

                                                           }).Distinct().Count();
                        #endregion

                        #region balance
                        try
                        {
                            dashBoardModel.balance = (decimal)entity.pos.Where(posSearchPredicat).Select(x => x.balance).Sum();
                        }
                        catch
                        {
                            dashBoardModel.balance = 0;

                        }
                        #endregion
                        return TokenManager.GenerateToken(dashBoardModel);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken(new WebDashBoardModel());
                }
            }
        }

        [HttpPost]
        [Route("getAccuracy")]
        public string getAccuracy(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var accuracy = entity.setValues.Where(x => x.setting.name == "accuracy").Select(x => x.value).FirstOrDefault();
                    return TokenManager.GenerateToken(accuracy);
                }

            }

        }

        [HttpPost]
        [Route("GetPermissions")]
        public string GetPermissions(string token)
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
                int userId = 0;
                string dashBoardPermission = "dashboard";
                string accountRepPermission = "accountsReports_view";
                string itemsStorage_transfer = "itemsStorage_transfer";
                string itemsStorage_reports = "itemsStorage_reports";
                string deliveryPermission = "setUserSetting_delivery";

                string result = "";

                JArray jArray = new JArray();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                #endregion
                using (incposdbEntities entity = new incposdbEntities())
                {
                    result += "{";
                    #region dashBoard Permission
                    var groupObjects = (from GO in entity.groupObject
                                        where GO.showOb == 1 && GO.objects.name.Contains(dashBoardPermission)
                                        join U in entity.users on GO.groupId equals U.groupId
                                        where U.userId == userId
                                        select new
                                        {
                                            GO.id,
                                            GO.showOb,
                                        }).FirstOrDefault();

                    result += "showDashBoard:";
                    if (groupObjects != null)
                    {
                        result += "true,";
                    }
                    else
                    {
                        result += "false,";
                    }
                    #endregion  
                    #region account Reports (sales+purchases) Permission
                    groupObjects = (from GO in entity.groupObject
                                    where GO.showOb == 1 && GO.objects.name.Contains(accountRepPermission)
                                    join U in entity.users on GO.groupId equals U.groupId
                                    where U.userId == userId
                                    select new
                                    {
                                        GO.id,
                                        GO.showOb,
                                    }).FirstOrDefault();

                    result += "showAccountRep:";
                    if (groupObjects != null)
                    {
                        result += "true,";
                    }
                    else
                    {
                        result += "false,";
                    }
                    #endregion 
                    #region stock Permission
                    groupObjects = (from GO in entity.groupObject
                                    where GO.showOb == 1 && (GO.objects.name.Contains(itemsStorage_transfer) || GO.objects.name.Contains(itemsStorage_reports))
                                    join U in entity.users on GO.groupId equals U.groupId
                                    where U.userId == userId
                                    select new
                                    {
                                        GO.id,
                                        GO.showOb,
                                    }).FirstOrDefault();

                    result += "showStock:";
                    if (groupObjects != null)
                    {
                        result += "true,";
                    }
                    else
                    {
                        result += "false,";
                    }
                    #endregion
                    #region delivery Permission
                    groupObjects = (from GO in entity.groupObject
                                    where GO.showOb == 1 && GO.objects.name.Contains(deliveryPermission)
                                    join U in entity.users on GO.groupId equals U.groupId
                                    where U.userId == userId
                                    select new
                                    {
                                        GO.id,
                                        GO.showOb,
                                    }).FirstOrDefault();

                    result += "showDelivery:";
                    if (groupObjects != null)
                    {
                        result += "true";
                    }
                    else
                    {
                        result += "false";
                    }

                    #endregion
                    result += "}";
                    return TokenManager.GenerateToken(result);

                }

            }

        }
        [HttpPost]
        [Route("getCurrency")]
        public string getCurrency(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var currency = entity.countriesCodes
                        .Where(x => x.isDefault == 1)
                        .Select(c => new
                        {
                            c.countryId,
                            c.code,
                            c.currency,
                            c.name,
                            c.isDefault,
                            c.currencyId,

                        }).FirstOrDefault();

                    if (currency != null)
                        return TokenManager.GenerateToken(currency.currency);
                    else
                        return TokenManager.GenerateToken("");

                }

            }

        }
        [HttpPost]
        [Route("getUserLanguage")]
        public string getUserLanguage(string token)
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
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                #endregion
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var lang = (from sv in entity.setValues.Where(x => x.setting.name == "language")
                                join su in entity.userSetValues.Where(x => x.userId == userId) on sv.valId equals su.valId
                                select new setValuesModel()
                                {
                                    value = sv.value,
                                    name = sv.setting.name,
                                }).FirstOrDefault();

                    if (lang != null)
                        return TokenManager.GenerateToken(lang.value);
                    else
                        return TokenManager.GenerateToken("en");

                }

            }

        }

        #region category
        [HttpPost]
        [Route("GetAllCategories")]
        public string GetAllCategories(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                //IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                //foreach (Claim c in claims)
                //{
                //    if (c.Type == "itemId")
                //    {
                //        categoryId = int.Parse(c.Value);
                //    }
                //    else if (c.Type == "userId")
                //    {
                //        userId = int.Parse(c.Value);
                //    }
                //}
                #endregion
                try
                {


                    List<CategoryEcommerceModel> mainList = new List<CategoryEcommerceModel>();

                    mainList = GetCategories();

                    //foreach (CategoryEcommerceModel row in mainList)
                    //{
                    //    row.childCategories = GetCategoriesbyParentId(row.categoryId);
                    //    foreach (CategoryEcommerceModel rowc in row.childCategories)
                    //    {
                    //        rowc.childCategories = GetCategoriesbyParentId(rowc.categoryId);
                    //    }
                    //}

                    return TokenManager.GenerateToken(mainList);
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken(ex.ToString());
                }
            }
        }


        //[HttpPost]
        //[Route("GetTreeCategories")]
        //public string GetTreeCategories(string token)
        //{
        //    token = TokenManager.readToken(HttpContext.Current.Request);
        //    var strP = TokenManager.GetPrincipal(token);
        //    if (strP != "0") //invalid authorization
        //    {
        //        return TokenManager.GenerateToken(strP);
        //    }
        //    else
        //    {
        //        #region parameters
        //        IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
        //        //foreach (Claim c in claims)
        //        //{
        //        //    if (c.Type == "itemId")
        //        //    {
        //        //        categoryId = int.Parse(c.Value);
        //        //    }
        //        //    else if (c.Type == "userId")
        //        //    {
        //        //        userId = int.Parse(c.Value);
        //        //    }
        //        //}
        //        #endregion
        //        try
        //        {

        //            activeCategories = GetCategories();
        //            List<CategoryEcommerceModel> mainList = new List<CategoryEcommerceModel>();
        //            List<CategoryEcommerceModel> refList = new List<CategoryEcommerceModel>();
        //            mainList = GetCategoriesroot();
        //            mainList = GetChildCategories(mainList);

        //            //foreach (CategoryEcommerceModel row in mainList)
        //            //{
        //            //    row.childCategories = GetCategoriesbyParentId(row.categoryId);
        //            //    foreach (CategoryEcommerceModel rowc in row.childCategories)
        //            //    {
        //            //        rowc.childCategories = GetCategoriesbyParentId(rowc.categoryId);
        //            //    }
        //            //}

        //            return TokenManager.GenerateToken(mainList);
        //        }
        //        catch (Exception ex)
        //        {
        //            return TokenManager.GenerateToken(ex.ToString());
        //        }
        //    }
        //}

        //public List<CategoryEcommerceModel> GetChildCategories(List<CategoryEcommerceModel> mainList)
        //{

        //    foreach (CategoryEcommerceModel row in mainList)
        //    {
        //        row.childCategories = GetCategoriesbyParentId(row.categoryId);

        //    }

        //    return mainList;
        //}
        //public List<CategoryEcommerceModel> GetCategoriesbyParentId(int? parentId)
        //{

        //    var categoriesList =activeCategories.Where(x => x.parentId == parentId && x.isActive == 1).ToList().OrderBy(x => x.createDate).ToList();

        //    //using (incposdbEntities entity = new incposdbEntities())
        //    //{

        //    //    var categoriesList = (from p in entity.categories.Where(x => x.parentId == parentId && x.isActive == 1)
        //    //                          select new CategoryEcommerceModel()
        //    //                          {
        //    //                              categoryId = p.categoryId,
        //    //                              name = p.name,
        //    //                              categoryCode = p.categoryCode,
        //    //                              createDate = p.createDate,
        //    //                              createUserId = p.createUserId,
        //    //                              details = p.details,
        //    //                              image = p.image,
        //    //                              notes = p.notes,
        //    //                              parentId = p.parentId,
        //    //                              taxes = p.taxes,
        //    //                              fixedTax = p.fixedTax,
        //    //                              updateDate = p.updateDate,
        //    //                              updateUserId = p.updateUserId,
        //    //                              isActive = p.isActive,

        //    //                          }).ToList().OrderBy(x => x.createDate).ToList();

        //    //    foreach (CategoryEcommerceModel rowch in categoriesList)
        //    //    {
        //    //        rowch.childCategories = GetCategoriesbyParentId(rowch.categoryId);
        //    //    }
        //    //    return categoriesList;


        //    //}
        //    foreach (CategoryEcommerceModel rowch in categoriesList)
        //    {
        //        rowch.childCategories = GetCategoriesbyParentId(rowch.categoryId);
        //    }
        //    return categoriesList;

        //}

        //public List<CategoryEcommerceModel> GetCategoriesroot()
        //{

        //    var categoriesList = activeCategories.Where(x => x.parentId == 0 && x.isActive == 1).ToList();

        //    return categoriesList;
        //    //using (incposdbEntities entity = new incposdbEntities())
        //    //{

        //    //    var categoriesList = (from p in entity.categories.Where(x => x.parentId == 0 && x.isActive == 1)
        //    //                          select new CategoryEcommerceModel()
        //    //                          {
        //    //                              categoryId = p.categoryId,
        //    //                              name = p.name,
        //    //                              categoryCode = p.categoryCode,
        //    //                              createDate = p.createDate,
        //    //                              createUserId = p.createUserId,
        //    //                              details = p.details,
        //    //                              image = p.image,
        //    //                              notes = p.notes,
        //    //                              parentId = p.parentId,
        //    //                              taxes = p.taxes,
        //    //                              fixedTax = p.fixedTax,
        //    //                              updateDate = p.updateDate,
        //    //                              updateUserId = p.updateUserId,
        //    //                              isActive = p.isActive,

        //    //                          }).ToList().OrderBy(x => x.createDate).ToList();

        //    //    return categoriesList;


        //    //}

        //}
        public List<CategoryEcommerceModel> GetCategories()
        {


            using (incposdbEntities entity = new incposdbEntities())
            {

                var categoriesList = (from p in entity.categories.Where(x => x.isActive == 1)
                                      select new CategoryEcommerceModel()
                                      {
                                          categoryId = p.categoryId,
                                          name = p.name,
                                          categoryCode = p.categoryCode,
                                          createDate = p.createDate,
                                          createUserId = p.createUserId,
                                          details = p.details,
                                          image = p.image,
                                          notes = p.notes,
                                          parentId = p.parentId,
                                          taxes = p.taxes,
                                          fixedTax = p.fixedTax,
                                          updateDate = p.updateDate,
                                          updateUserId = p.updateUserId,
                                          isActive = p.isActive,

                                      }).ToList().OrderBy(x => x.createDate).ToList();


                return categoriesList;


            }

        }

        // category path
        //[HttpPost]
        //[Route("GetCategoryPath")]
        //public string GetCategoryPath(string token)
        //{
        //    token = TokenManager.readToken(HttpContext.Current.Request);
        //    List<CategoryEcommerceModel> treecat = new List<CategoryEcommerceModel>();
        //    var strP = TokenManager.GetPrincipal(token);
        //    if (strP != "0") //invalid authorization
        //    {
        //        return TokenManager.GenerateToken(strP);
        //    }
        //    else
        //    {
        //        int categoryID = 0;
        //        IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
        //        foreach (Claim c in claims)
        //        {
        //            if (c.Type == "itemId")
        //            {
        //                categoryID = int.Parse(c.Value);
        //            }
        //        }
        //        using (incposdbEntities entity = new incposdbEntities())
        //        {
        //            int parentid = categoryID; // if want to show the last category 
        //            while (parentid > 0)
        //            {
        //                CategoryEcommerceModel tempcate = new CategoryEcommerceModel();
        //                var category = entity.categories.Where(c => c.categoryId == parentid)
        //                    .Select(p => new {
        //                        p.categoryId,
        //                        p.name,
        //                        p.categoryCode,
        //                        //p.createDate,
        //                        //p.createUserId,
        //                        //p.details,
        //                        p.image,
        //                        //p.notes,
        //                        p.parentId,
        //                        //p.taxes,
        //                          //p.fixedTax ,
        //                        //p.updateDate,
        //                        //p.updateUserId,
        //                    }).FirstOrDefault();


        //                tempcate.categoryId = category.categoryId;

        //                tempcate.name = category.name;
        //                tempcate.categoryCode = category.categoryCode;
        //                //tempcate.createDate = category.createDate;
        //                //tempcate.createUserId = category.createUserId;
        //                //tempcate.details = category.details;
        //                tempcate.image = category.image;
        //                //tempcate.notes = category.notes;
        //                tempcate.parentId = category.parentId;
        //                //tempcate.taxes = category.taxes;
        //                //tempcate.fixedTax = category.fixedTax;
        //                //tempcate.updateDate = category.updateDate;
        //                //tempcate.updateUserId = category.updateUserId;


        //                parentid = (int)tempcate.parentId;

        //                treecat.Add(tempcate);

        //            }
        //           treecat.Reverse();
        //            return TokenManager.GenerateToken(treecat);

        //        }


        //    }
        //}

        #endregion
        //
        #region categories of parent
        //public     List<int> idsList = new List<int>();
        //[HttpPost]
        //[Route("GetCategoriesOfparent")]
        //public string GetCategoriesOfparent(string token)
        //{
        //    token = TokenManager.readToken(HttpContext.Current.Request);
        //    var strP = TokenManager.GetPrincipal(token);
        //    if (strP != "0") //invalid authorization
        //    {
        //        return TokenManager.GenerateToken(strP);
        //    }
        //    else
        //    {
        //        #region parameters
        //      int  categoryId = 0;
        //        IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
        //        foreach (Claim c in claims)
        //        {
        //            if (c.Type == "itemId")
        //            {
        //                categoryId = int.Parse(c.Value);
        //            }
        //            //else if (c.Type == "userId")
        //            //{
        //            //    userId = int.Parse(c.Value);
        //            //}
        //        }
        //        #endregion
        //        try
        //        {

        //            activeCategories = GetCategories();

        //List<CategoryEcommerceModel> mainList = new List<CategoryEcommerceModel>();
        //            idsList.Add(categoryId);
        //            mainList = GetCategoriesroot(categoryId);
        //            mainList = GetsonCategories(mainList);

        //            //foreach (CategoryEcommerceModel row in mainList)
        //            //{
        //            //    row.childCategories = GetCategoriesbyParentId(row.categoryId);
        //            //    foreach (CategoryEcommerceModel rowc in row.childCategories)
        //            //    {
        //            //        rowc.childCategories = GetCategoriesbyParentId(rowc.categoryId);
        //            //    }
        //            //}

        //            return TokenManager.GenerateToken(idsList);
        //        }
        //        catch (Exception ex)
        //        {
        //            return TokenManager.GenerateToken(ex.ToString());
        //        }
        //    }
        //}
        public List<CategoryEcommerceModel> GetCategoriesroot(int parentId)
        {

            var categoriesList = activeCategories.Where(x => x.parentId == parentId && x.isActive == 1).ToList();

            return categoriesList;
            //using (incposdbEntities entity = new incposdbEntities())
            //{

            //    var categoriesList = (from p in entity.categories.Where(x => x.parentId == 0 && x.isActive == 1)
            //                          select new CategoryEcommerceModel()
            //                          {
            //                              categoryId = p.categoryId,
            //                              name = p.name,
            //                              categoryCode = p.categoryCode,
            //                              createDate = p.createDate,
            //                              createUserId = p.createUserId,
            //                              details = p.details,
            //                              image = p.image,
            //                              notes = p.notes,
            //                              parentId = p.parentId,
            //                              taxes = p.taxes,
            //                              fixedTax = p.fixedTax,
            //                              updateDate = p.updateDate,
            //                              updateUserId = p.updateUserId,
            //                              isActive = p.isActive,

            //                          }).ToList().OrderBy(x => x.createDate).ToList();

            //    return categoriesList;


            //}

        }

        //public List<CategoryEcommerceModel> GetsonCategories(List<CategoryEcommerceModel> mainList)
        //{

        //    foreach (CategoryEcommerceModel row in mainList)
        //    {
        //        row.childCategories = GetSonsbyParentId(row.categoryId);
        //        idsList.Add(row.categoryId);
        //    }

        //    return mainList;
        //}
        //public List<CategoryEcommerceModel> GetSonsbyParentId(int? parentId)
        //{

        //    var categoriesList = activeCategories.Where(x => x.parentId == parentId && x.isActive == 1).ToList().OrderBy(x => x.createDate).ToList();

        //    //using (incposdbEntities entity = new incposdbEntities())
        //    //{

        //    //    var categoriesList = (from p in entity.categories.Where(x => x.parentId == parentId && x.isActive == 1)
        //    //                          select new CategoryEcommerceModel()
        //    //                          {
        //    //                              categoryId = p.categoryId,
        //    //                              name = p.name,
        //    //                              categoryCode = p.categoryCode,
        //    //                              createDate = p.createDate,
        //    //                              createUserId = p.createUserId,
        //    //                              details = p.details,
        //    //                              image = p.image,
        //    //                              notes = p.notes,
        //    //                              parentId = p.parentId,
        //    //                              taxes = p.taxes,
        //    //                              fixedTax = p.fixedTax,
        //    //                              updateDate = p.updateDate,
        //    //                              updateUserId = p.updateUserId,
        //    //                              isActive = p.isActive,

        //    //                          }).ToList().OrderBy(x => x.createDate).ToList();

        //    //    foreach (CategoryEcommerceModel rowch in categoriesList)
        //    //    {
        //    //        rowch.childCategories = GetCategoriesbyParentId(rowch.categoryId);
        //    //    }
        //    //    return categoriesList;


        //    //}
        //    foreach (CategoryEcommerceModel rowch in categoriesList)
        //    {
        //        rowch.childCategories = GetSonsbyParentId(rowch.categoryId);
        //        idsList.Add(rowch.categoryId);
        //    }
        //    return categoriesList;

        //}

        #endregion
        #region item
        public List<ItemEcommerceModel> GetItems()
        {
            ItemsController itc = new ItemsController();

            DateTime cmpdate = cc.AddOffsetTodate(DateTime.Now).AddDays(itc.newdays);
            DateTime now = cc.AddOffsetTodate(DateTime.Now);
            List<int> offerIds = new List<int>();
            List<int> iuIds = new List<int>();
            List<itemsOffers> activeitemsoffer = new List<itemsOffers>();
            //List<ItemOfferModel> activeitemsoffer = new List<ItemOfferModel>();
            List<int> ValidiuIds = new List<int>();

            List<itemsUnits> itemunitListList = new List<itemsUnits>();
            List<ItemEcommerceModel> categoriesList = new List<ItemEcommerceModel>();

            using (incposdbEntities entity = new incposdbEntities())
            {
                // all iu in itemsOffers
                List<itemsOffers> itemsoffer = entity.itemsOffers.ToList();
                List<offers> activeOffers = entity.offers.Where(X => X.isActive == 1 && X.startDate <= now && X.endDate > now).ToList();

                // all active Offers
                //  itemunitListList = entity.itemsUnits.Where(X => X.isActive == 1  ).ToList();
                offerIds = activeOffers.Select(X => X.offerId).ToList();
                // iu has quantity and his offer is valid in itemsOffers
                //activeitemsoffer = itemsoffer.Where(X => (X.quantity - X.used) > 0 && offerIds.Contains((int)X.offerId)).AsEnumerable().Select(X => new ItemOfferModel {
                //    ioId = X.ioId,
                //    iuId = X.iuId,
                //    offerId = X.offerId,
                //    offerName = X.offers.name,
                //    discountType = X.offers.discountType,
                //    discountValue = X.offers.discountValue,

                //}).ToList();

                activeitemsoffer = entity.itemsOffers.Where(X => (X.quantity - X.used) > 0 && offerIds.Contains((int)X.offerId)).ToList();
                ValidiuIds = activeitemsoffer.Select(X => (int)X.iuId).ToList();

                categoriesList = (from I in entity.items
                                  where I.isActive == 1
                                  select new ItemEcommerceModel()
                                  {
                                      itemId = I.itemId,
                                      name = I.name,
                                      code = I.code,
                                      categoryId = I.categoryId,

                                      //  categoryName = x.name,
                                      max = I.max,
                                      maxUnitId = I.maxUnitId,
                                      minUnitId = I.minUnitId,
                                      min = I.min,

                                      parentId = I.parentId,
                                      isActive = I.isActive,
                                      image = I.image,
                                      type = I.type,
                                      details = I.details,
                                      taxes = I.taxes,
                                      createDate = I.createDate,
                                      updateDate = I.updateDate,

                                      isNew = DateTime.Compare((DateTime)I.createDate, cmpdate) >= 0 ? 1 : 0,
                                      //parentName = entity.items.Where(m => m.itemId == I.parentId).FirstOrDefault().name,
                                      //minUnitName = entity.units.Where(m => m.unitId == I.minUnitId).FirstOrDefault().name,
                                      //maxUnitName = entity.units.Where(m => m.unitId == I.minUnitId).FirstOrDefault().name,
                                      isOffer = ValidiuIds.Contains(I.itemsUnits.Where(X => X.defaultSale == 1 && X.itemId == I.itemId).Select(X => X.itemUnitId).FirstOrDefault()) == true ? 1 : 0,
                                      //avgPurchasePrice = I.avgPurchasePrice
                                      ItemUnitList = I.itemsUnits.Where(X => X.isActive == 1 && X.itemId == I.itemId && X.defaultSale == 1).Select(X => new ItemUnitEcommerceModel
                                      {
                                          unitName = X.units.name,
                                          unitId = X.unitId,
                                          unitValue = X.unitValue,
                                          defaultSale = X.defaultSale,
                                          price = X.price,
                                          //basicPrice=X.basicPrice,
                                          cost = X.cost,
                                          barcode = X.barcode,
                                          itemUnitId = X.itemUnitId,
                                          subUnitId = X.subUnitId,
                                          storageCostId = X.storageCostId,
                                          isActive = X.isActive,
                                          // offerName = activeitemsoffer.ToList().Where(io => X.itemUnitId.Equals(io.iuId)).ToList().Count()>0?  "-":"",
                                          offerName = X.itemsOffers.Where(io => ValidiuIds.Contains((int)io.iuId)).FirstOrDefault().offers.name,
                                          offerId = X.itemsOffers.Where(io => ValidiuIds.Contains((int)io.iuId)).FirstOrDefault().offers.offerId,

                                          discountType = X.itemsOffers.Where(io => ValidiuIds.Contains((int)io.iuId)).FirstOrDefault().offers.discountType,

                                          discountValue = X.itemsOffers.Where(io => ValidiuIds.Contains((int)io.iuId)).FirstOrDefault().offers.discountValue,

                                      }).ToList(),

                                  })
                 .ToList().OrderBy(x => x.createDate).ToList();


                //  DateTime.Compare((DateTime)I.createDate, cmpdate) >= 0;

                return categoriesList;

            }

        }

        public ItemEcommerceModel GetItemById(int itemId)
        {


            ItemsController itc = new ItemsController();

            DateTime cmpdate = cc.AddOffsetTodate(DateTime.Now).AddDays(itc.newdays);
            DateTime now = cc.AddOffsetTodate(DateTime.Now);
            List<int> offerIds = new List<int>();
            List<int> iuIds = new List<int>();
            List<itemsOffers> activeitemsoffer = new List<itemsOffers>();
            //List<ItemOfferModel> activeitemsoffer = new List<ItemOfferModel>();
            List<int> ValidiuIds = new List<int>();

            List<itemsUnits> itemunitListList = new List<itemsUnits>();
            ItemEcommerceModel itemModel = new ItemEcommerceModel();

            using (incposdbEntities entity = new incposdbEntities())
            {
                // all iu in itemsOffers
                List<itemsOffers> itemsoffer = entity.itemsOffers.ToList();
                List<offers> activeOffers = entity.offers.Where(X => X.isActive == 1 && X.startDate <= now && X.endDate > now).ToList();

                // all active Offers
                //  itemunitListList = entity.itemsUnits.Where(X => X.isActive == 1  ).ToList();
                offerIds = activeOffers.Select(X => X.offerId).ToList();
                // iu has quantity and his offer is valid in itemsOffers
                //activeitemsoffer = itemsoffer.Where(X => (X.quantity - X.used) > 0 && offerIds.Contains((int)X.offerId)).AsEnumerable().Select(X => new ItemOfferModel {
                //    ioId = X.ioId,
                //    iuId = X.iuId,
                //    offerId = X.offerId,
                //    offerName = X.offers.name,
                //    discountType = X.offers.discountType,
                //    discountValue = X.offers.discountValue,

                //}).ToList();

                activeitemsoffer = entity.itemsOffers.Where(X => (X.quantity - X.used) > 0 && offerIds.Contains((int)X.offerId)).ToList();
                ValidiuIds = activeitemsoffer.Select(X => (int)X.iuId).ToList();

                itemModel = (from I in entity.items
                             where I.isActive == 1 && I.itemId == itemId
                             select new ItemEcommerceModel()
                             {
                                 itemId = I.itemId,
                                 name = I.name,
                                 code = I.code,
                                 categoryId = I.categoryId,
                                 //  categoryName = x.name,
                                 max = I.max,
                                 maxUnitId = I.maxUnitId,
                                 minUnitId = I.minUnitId,
                                 min = I.min,

                                 parentId = I.parentId,
                                 isActive = I.isActive,
                                 image = I.image,
                                 type = I.type,
                                 details = I.details,
                                 taxes = I.taxes,
                                 createDate = I.createDate,
                                 updateDate = I.updateDate,

                                 isNew = DateTime.Compare((DateTime)I.createDate, cmpdate) >= 0 ? 1 : 0,
                                 //parentName = entity.items.Where(m => m.itemId == I.parentId).FirstOrDefault().name,
                                 //minUnitName = entity.units.Where(m => m.unitId == I.minUnitId).FirstOrDefault().name,
                                 //maxUnitName = entity.units.Where(m => m.unitId == I.minUnitId).FirstOrDefault().name,
                                 isOffer = ValidiuIds.Contains(I.itemsUnits.Where(X => X.defaultSale == 1 && X.itemId == I.itemId).Select(X => X.itemUnitId).FirstOrDefault()) == true ? 1 : 0,
                                 //avgPurchasePrice = I.avgPurchasePrice
                                 ItemUnitList = I.itemsUnits.Where(X => X.isActive == 1 && X.itemId == I.itemId && X.defaultSale == 1).Select(X => new ItemUnitEcommerceModel
                                 {
                                     unitName = X.units.name,
                                     unitId = X.unitId,
                                     unitValue = X.unitValue,
                                     defaultSale = X.defaultSale,
                                     price = X.price,
                                     //basicPrice=X.basicPrice,
                                     cost = X.cost,
                                     barcode = X.barcode,
                                     itemUnitId = X.itemUnitId,
                                     subUnitId = X.subUnitId,
                                     storageCostId = X.storageCostId,
                                     isActive = X.isActive,
                                     // offerName = activeitemsoffer.ToList().Where(io => X.itemUnitId.Equals(io.iuId)).ToList().Count()>0?  "-":"",
                                     offerName = X.itemsOffers.Where(io => ValidiuIds.Contains((int)io.iuId)).FirstOrDefault().offers.name,
                                     offerId = X.itemsOffers.Where(io => ValidiuIds.Contains((int)io.iuId)).FirstOrDefault().offers.offerId,

                                     discountType = X.itemsOffers.Where(io => ValidiuIds.Contains((int)io.iuId)).FirstOrDefault().offers.discountType,

                                     discountValue = X.itemsOffers.Where(io => ValidiuIds.Contains((int)io.iuId)).FirstOrDefault().offers.discountValue,

                                 }).ToList(),

                                 //Properties = I.itemsProp.Where(X => X.itemId == I.itemId && X.propertiesItems.isActive == 1)
                                 //.Select(X => new PropertyModel
                                 //{
                                 //    propertyId = X.propertiesItems.properties.propertyId,
                                 //    name = X.propertiesItems.properties.name,

                                 //}).Distinct().ToList(),
                             })
                 .ToList().FirstOrDefault();

                foreach (var prop in itemModel.Properties)
                {
                    prop.ItemPropValues = entity.itemsProp.Where(M =>  M.propertiesItems.properties.propertyId == prop.propertyId && M.propertiesItems.isActive == 1)
                                     .Select(M => new itemsPropModel
                                     {
                                         itemPropId = M.itemPropId,
                                         propertyItemId = M.propertyItemId,
                                        // itemId = M.itemId,
                                         isDefault = M.propertiesItems.isDefault,
                                         propName = M.propertiesItems.properties.name,
                                         propValue = M.propertiesItems.name,
                                     }).Distinct().ToList();
                }
                return itemModel;
            }

        }

        [HttpPost]
        [Route("GetAllItems")]
        public string GetAllItems(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                //IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                //foreach (Claim c in claims)
                //{
                //    if (c.Type == "itemId")
                //    {
                //        categoryId = int.Parse(c.Value);
                //    }
                //    else if (c.Type == "userId")
                //    {
                //        userId = int.Parse(c.Value);
                //    }
                //}
                #endregion
                try
                {


                    //    List<ItemEcommerceModel> itemList = new List<ItemEcommerceModel>();

                    var itemList = GetItems();

                    //foreach (CategoryEcommerceModel row in mainList)
                    //{
                    //    row.childCategories = GetCategoriesbyParentId(row.categoryId);
                    //    foreach (CategoryEcommerceModel rowc in row.childCategories)
                    //    {
                    //        rowc.childCategories = GetCategoriesbyParentId(rowc.categoryId);
                    //    }
                    //}

                    return TokenManager.GenerateToken(itemList);
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken(ex.ToString());
                }
            }
        }

        [HttpPost]
        [Route("GetItemById")]
        public string GetItemById(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                int itemId = 0;
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        itemId = int.Parse(c.Value);
                    }
                    //else if (c.Type == "userId")
                    //{
                    //    userId = int.Parse(c.Value);
                    //}
                }
                #endregion
                try
                {


                    ItemEcommerceModel item = new ItemEcommerceModel();

                    item = GetItemById(itemId);

                    //foreach (CategoryEcommerceModel row in mainList)
                    //{
                    //    row.childCategories = GetCategoriesbyParentId(row.categoryId);
                    //    foreach (CategoryEcommerceModel rowc in row.childCategories)
                    //    {
                    //        rowc.childCategories = GetCategoriesbyParentId(rowc.categoryId);
                    //    }
                    //}

                    return TokenManager.GenerateToken(item);
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        #endregion
        #region setting
        [HttpPost]
        [Route("GetSetting")]
        public string GetSetting(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters

                #endregion
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var list = (from s in entity.setting
                                    join v in entity.setValues on s.settingId equals v.settingId
                                    //  where !s.notes.ToString().Equals("emailtemp")
                                    select new SettingEcommerceModel()
                                    {
                                        valId = v.valId,
                                        value = v.value,
                                        isDefault = v.isDefault,
                                        isSystem = v.isSystem,
                                        notes = v.notes,
                                        settingId = v.settingId,
                                        settingName = s.name,
                                        settingNotes = s.notes,

                                    }).ToList().Where(x => x.settingNotes != "emailtemp").ToList();
                        SettingEcommerceModel currency = new SettingEcommerceModel();
                        var country = entity.countriesCodes.Where(x => x.isDefault == 1).FirstOrDefault();

                        currency.value = country.currency;

                        currency.settingName = "currency";
                        currency.isDefault = 1;
                        currency.isSystem = 1;
                        list.Add(currency);
                        return TokenManager.GenerateToken(list);


                    }



                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken(ex.ToString());
                }
            }
        }

        [HttpPost]
        [Route("GetDefaultCountry")]
        public string GetDefaultCountry(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                //int mainBranchId = 0;
                //int isDefault = 0;

                //IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                //foreach (Claim c in claims)
                //{
                //    if (c.Type == "isDefault")
                //    {
                //        isDefault = int.Parse(c.Value);
                //    }


                //}

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {


                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var list = entity.countriesCodes
                   .Where(c => c.isDefault == 1)
                   .Select(c => new CountryModel
                   {
                       countryId = c.countryId,
                       code = c.code,
                       currency = c.currency,
                       name = c.name,
                       isDefault = c.isDefault,
                       currencyId = c.currencyId,
                       citiesList = c.cities.Where(y => y.countryId == c.countryId).Select(y => new CityModel
                       {
                           cityId = y.cityId,
                           cityCode = y.cityCode,
                           countryId = y.countryId,
                       }
                       ).ToList(),
                   }).FirstOrDefault();


                        return TokenManager.GenerateToken(list);

                    }

                }
                catch
                {
                    CountryModel cntry = new CountryModel();
                    return TokenManager.GenerateToken(cntry);
                }

            }




        }

        #endregion

        #region customer
        [HttpPost]
        [Route("GetCustomerbyId")]
        public string GetCustomerbyId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            int agentId = 0;

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "agentId")
                    {
                        agentId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var agent = entity.agents
                   .Where(p => p.agentId == agentId && p.isActive == 1 && p.type == "c")
                   .Select(p => new CustomerEcommerceModel
                   {
                       agentId = p.agentId,
                       name = p.name,
                       code = p.code,
                       company = p.company,
                       address = p.address,
                       email = p.email,
                       phone = p.phone,
                       mobile = p.mobile,
                       image = p.image,
                       type = p.type,
                       accType = p.accType,
                       balance = p.balance,
                       balanceType = p.balanceType,
                       notes = p.notes,
                       isActive = p.isActive,
                       createDate = p.createDate,
                       updateDate = p.updateDate,
                       maxDeserve = p.maxDeserve,
                       fax = p.fax,
                       isLimited = p.isLimited,
                       payType = p.payType,
                       userName = p.userName,
                       password = p.password,
                       lastName = p.lastName,
                       language = p.language,
                       countryId = p.countryId,
                       countryName = p.countryName,
                       cityName = p.cityName,
                   })
                   .FirstOrDefault();

                    return TokenManager.GenerateToken(agent);
                }
            }
        }

        [HttpPost]
        [Route("SaveCustomer")]
        public string SaveCustomer(string token)
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
                string agentObject = "";
                agents agentObj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        agentObject = c.Value.Replace("\\", string.Empty);
                        agentObject = agentObject.Trim('"');
                        agentObj = JsonConvert.DeserializeObject<agents>(agentObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    agents agent;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var agentEntity = entity.Set<agents>();
                        if (agentObj.agentId == 0)
                        {
                            ProgramInfo programInfo = new ProgramInfo();
                            int agentMaxCount = 0;

                            agentMaxCount = programInfo.getCustomerCount();


                            int agentCount = entity.agents.Where(x => x.type == "c").Count();
                            if (agentCount >= agentMaxCount && agentMaxCount != -1)
                            {
                                message = "-1";
                                return TokenManager.GenerateToken(message);
                            }
                            else
                            {
                                agentObj.createDate = cc.AddOffsetTodate(DateTime.Now);
                                agentObj.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                agentObj.balance =0;
                                agentObj.balanceType =0;
                                agentObj.maxDeserve = 0;
                                agentObj.payType = "multiple";
                                agentObj.accType = "";

                                agentObj.isShopCustomer = true;
                                //  agentObj.updateUserId = agentObj.createUserId;
                                agentObj.balanceType = 0;
                                agent = agentEntity.Add(agentObj);
                            }
                        }
                        else
                        {
                            agent = entity.agents.Where(p => p.agentId == agentObj.agentId).First();
                            //  agent.accType = agentObj.accType;
                            agent.address = agentObj.address;
                            //  agent.code = agentObj.code;
                            agent.company = agentObj.company;
                            agent.email = agentObj.email;
                            agent.image = agentObj.image;
                            agent.mobile = agentObj.mobile;
                            agent.name = agentObj.name;
                            agent.notes = agentObj.notes;
                            agent.phone = agentObj.phone;
                            // agent.type = agentObj.type;
                            //  agent.maxDeserve = agentObj.maxDeserve;
                            agent.fax = agentObj.fax;
                            agent.updateDate = cc.AddOffsetTodate(DateTime.Now);// server current date
                                                            // agent.updateUserId = agentObj.updateUserId;
                                                            //    agent.isActive = agentObj.isActive;
                                                            // agent.balance = agentObj.balance;
                                                            // agent.balanceType = agentObj.balanceType;
                                                            //  agent.isLimited = agentObj.isLimited;
                                                            //  agent.payType = agentObj.payType;

                            agent.userName = agentObj.userName;
                            agent.lastName = agentObj.lastName;
                            agent.password = agentObj.password;
                            agent.countryId = agentObj.countryId;
                            agent.cityId = agentObj.cityId;
                            agent.language = agentObj.language;
                            //   agent.isShopCustomer = agentObj.isShopCustomer;
                            agent.countryName = agentObj.countryName;
                            agent.cityName = agentObj.cityName;



                        }
                        entity.SaveChanges();
                        message = agent.agentId.ToString();

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

        ////////
        [HttpPost]
        [Route("GetloginCustomer")]
        public string GetloginCustomer(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            List<CustomerEcommerceModel> usersList = new List<CustomerEcommerceModel>();
            CustomerEcommerceModel user = new CustomerEcommerceModel();
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string userName = "";
                string password = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "userName")
                    {
                        userName = c.Value;
                    }
                    else if (c.Type == "password")
                    {
                        password = c.Value;
                    }
                }

                CustomerEcommerceModel emptyuser = new CustomerEcommerceModel();

                emptyuser.createDate = cc.AddOffsetTodate(DateTime.Now);
                emptyuser.updateDate = cc.AddOffsetTodate(DateTime.Now);
                //emptyuser.username = userName;
                //emptyuser.createUserId = 0;
                //emptyuser.updateUserId = 0;
                emptyuser.agentId = 0;
                emptyuser.isActive = 0;

                emptyuser.balance = 0;
                emptyuser.balanceType = 0;
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        usersList = entity.agents.Where(c => c.isActive == 1 && c.userName == userName && c.type == "c")
                        .Select(c => new CustomerEcommerceModel
                        {
                            agentId = c.agentId,
                            name = c.name,
                            code = c.code,
                            company = c.company,
                            address = c.address,
                            email = c.email,
                            phone = c.phone,
                            mobile = c.mobile,
                            image = c.image,
                            type = c.type,
                            accType = c.accType,
                            balance = c.balance,
                            balanceType = c.balanceType,
                            notes = c.notes,
                            isActive = c.isActive,
                            createDate = c.createDate,
                            updateDate = c.updateDate,
                            maxDeserve = c.maxDeserve,
                            fax = c.fax,
                            isLimited = c.isLimited,
                            payType = c.payType,
                            userName = c.userName,
                            password = c.password,
                            lastName = c.lastName,
                            language = c.language,
                            countryId = c.countryId,
                            countryName = c.countryName,
                            cityName = c.cityName,

                        })
                        .ToList();

                        if (usersList == null || usersList.Count <= 0)
                        {

                            user = emptyuser;
                            // rong user
                            //   user.notes = "nooo";
                            return TokenManager.GenerateToken(user);
                        }
                        else
                        {
                            user = usersList.Where(i => i.userName == userName && i.password == password).FirstOrDefault();
                            if (user.password.Equals(password))
                            {
                                // correct username and pasword
                                return TokenManager.GenerateToken(user);
                            }
                            else
                            {
                                // rong pass return just username
                                user = emptyuser;
                                user.userName = userName;
                                return TokenManager.GenerateToken(user);

                            }
                        }
                    }

                }
                catch
                {
                    return TokenManager.GenerateToken(emptyuser);
                }
            }
        }

        [HttpPost]
        [Route("changePassword")]
        public string changePassword(string token)
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
                string agentObject = "";
                agents agentObj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        agentObject = c.Value.Replace("\\", string.Empty);
                        agentObject = agentObject.Trim('"');
                        agentObj = JsonConvert.DeserializeObject<agents>(agentObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    agents agent = new agents();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var agentEntity = entity.Set<agents>();
                        if (agentObj.agentId == 0)
                        {

                        }
                        else
                        {
                            agent = entity.agents.Where(p => p.agentId == agentObj.agentId).First();
                            agent.updateDate = cc.AddOffsetTodate(DateTime.Now);// server current date
                            agent.password = agentObj.password;
                            entity.SaveChanges();
                        }
                        message = agent.agentId.ToString();
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

        #endregion
        #region contents
        [HttpPost]
        [Route("GetContentsByName")]
        public string GetContentsByName(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
         string name = "";
            string lang = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "name")
                    {
                        name = c.Value;
                    }else if (c.Type == "lang")
                    {
                        lang = c.Value;
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var item = entity.siteContents
                   .Where(s => s.name == name  && s.lang==lang)
                   .Select(s => new siteContentsModel
                   {
                       id = s.id,
                       name = s.name,
                       title=s.title,
                       contents=s.contents,
                       lang=s.lang,
                       category=s.category,
                   }).FirstOrDefault();

                    return TokenManager.GenerateToken(item);
                }
            }
        }
        [HttpPost]
        [Route("GetsocialAndslide")]
        public string GetsocialAndslide(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string name = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                //IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                //foreach (Claim c in claims)
                //{
                //    if (c.Type == "agentId")
                //    {
                //        name = c.Value;
                //    }
                //}
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var item = entity.siteContents
                   .Where(s => s.category != "page")
                   .Select(s => new siteContentsModel
                   {
                       id = s.id,
                       name = s.name,
                       title = s.title,
                       contents = s.contents,
                       lang = s.lang,
                       category = s.category,
                   }).ToList();
                    return TokenManager.GenerateToken(item);
                }
            }
        }
        [HttpPost]
        [Route("GetAllContents")]
        public string GetAllContents(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
         
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                //IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                //foreach (Claim c in claims)
                //{
                //    if (c.Type == "agentId")
                //    {
                //        name = c.Value;
                //    }
                //}
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var item = entity.siteContents
                    
                   .Select(s => new siteContentsModel
                   {
                       id = s.id,
                       name = s.name,
                       title = s.title,
                       contents = s.contents,
                       lang = s.lang,
                       category = s.category,
                   }).ToList();
                    return TokenManager.GenerateToken(item);
                }
            }
        }
        [HttpPost]
        [Route("SaveContent")]
        public string SaveContent(string token)
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
                string Object = "";
                siteContents Obj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        Obj = JsonConvert.DeserializeObject<siteContents>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
               int res= SaveContent(Obj);
                    return TokenManager.GenerateToken(res.ToString());
                }
                catch(Exception ex)
                {
                    message = "0";
                    return TokenManager.GenerateToken(ex.ToString());
                }
            }
        }

        public int SaveContent(siteContents Obj)
        {

            int message = 0;
                try
                {
                    siteContents contentsrow = new siteContents();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var contentEntity = entity.Set<siteContents>();
                        if (Obj.id == 0)
                        {


                        }
                        else
                        {
                            contentsrow = entity.siteContents.Where(s => s.name == Obj.name && s.lang == Obj.lang).First();

                            contentsrow.title = Obj.title;
                            contentsrow.contents = Obj.contents;
                        }
                        entity.SaveChanges();
                        message = contentsrow.id ;

                    }
                    return message;
                }
                catch (Exception ex)
                {
                     
                    return  0;
                }
           
        }

        [HttpPost]
        [Route("SaveContentsList")]
        public string SaveContentsList(string token)
        {
            //string Object string newObject
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
                List<siteContents> newObject = new List<siteContents>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<siteContents>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                        int res = 0;
                        if (newObject.Count() > 0)
                        {
                            foreach (siteContents  row in newObject)
                            {
                                res = SaveContent (row);
                            }
                        }

                        return TokenManager.GenerateToken(res.ToString());

                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                }
                return TokenManager.GenerateToken(message);
            }
        }

        #endregion
        #region contact 
        [HttpPost]
        [Route("GetContactByDate")]
        public string GetContactByDate(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            DateTime date=new DateTime();
            Calculate calc = new Calculate();
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "date")
                    {
                        date =DateTime.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var list = entity.siteContact
                  
                   .Select(s => new siteContactModel
                   {
                       id = s.id,
                       name = s.name,
                       mobile = s.mobile,
                       email = s.email,
                       subject = s.subject,
                       subjectId = s.subjectId,
                       message=s.message,
                       status = s.status,
                       createDate = s.createDate,
                       updateDate = s.updateDate,
                       updateUserId = s.updateUserId,
                       isActive =s.isActive,
                   }).ToList();
                    list = list.Where(X => DateTime.Compare((DateTime)calc.changeDateformat(X.createDate, "yyyy-MM-dd"), (DateTime)calc.changeDateformat(date, "yyyy-MM-dd")) == 0).ToList();
                    return TokenManager.GenerateToken(list);
                }
            }
        }

        [HttpPost]
        [Route("SaveContact")]
        public string SaveContact(string token)
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
                string Object = "";
                siteContact Obj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        Obj = JsonConvert.DeserializeObject<siteContact>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    siteContact row = new siteContact();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var siteEntity = entity.Set<siteContact>();
                        if (Obj.id == 0)
                        {
                            Obj.createDate = cc.AddOffsetTodate(DateTime.Now);
                            Obj.updateDate = Obj.createDate;
                            Obj = siteEntity.Add(Obj);
                        }
                        else
                        {
                            row = entity.siteContact.Where(s => s.id == Obj.id  ).First();
                            row.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            row.status = Obj.status;
                        }
                        entity.SaveChanges();
                        message = Obj.id.ToString();

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
        [Route("DeleteContact")]
        public string DeleteContact(string token)
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
                 
                int id = 0;
                bool final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        id = int.Parse(c.Value);
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
                            siteContact contactDelete = entity.siteContact.Find(id);
                            entity.siteContact.Remove(contactDelete);
                            message = entity.SaveChanges().ToString();
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

                            siteContact  Obj = entity.siteContact.Find(id);
                            Obj.isActive = false;
                            Obj.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);
                        }
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }
            }
        }

        #endregion

        #region subject of contact 
        [HttpPost]
        [Route("GetAllSubject")]
        public string GetAllSubject(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string lang = "";

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
             
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var item = entity.siteSubject
                   
                   .Select(s => new siteSubjectModel
                   {
                       subjectId = s.subjectId,
                       name = s.name,
                       isActive = s.isActive,
                       value = s.value,
                       lang = s.lang,
                    
                   }).FirstOrDefault();

                    return TokenManager.GenerateToken(item);
                }
            }
        }
        [HttpPost]
        [Route("GetSubjectBylang")]
        public string GetSubjectBylang(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string lang = "";

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "lang")
                    {
                        lang = c.Value;
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var list = entity.siteSubject
                   .Where(s => s.lang == lang && s.isActive == true)
                   .Select(s => new siteSubjectModel
                   {
                       subjectId = s.subjectId,
                       name = s.name,
                       isActive = s.isActive,
                       value = s.value,
                       lang = s.lang,

                   }).ToList();

                    return TokenManager.GenerateToken(list);
                }
            }
        }

        [HttpPost]
        [Route("SaveSubject")]
        public string SaveSubject(string token)
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
                string Object = "";
                siteSubject Obj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        Obj = JsonConvert.DeserializeObject<siteSubject>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    siteSubject row = new siteSubject();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var siteEntity = entity.Set<siteSubject>();
                        if (Obj.subjectId == 0)
                        {
                            
                            Obj = siteEntity.Add(Obj);
                        }
                        else
                        {
                            row = entity.siteSubject.Where(s => s.subjectId == Obj.subjectId).First();
                            row.name = Obj.name;
                            row.value = Obj.value;
                            row.lang = Obj.lang;
                            row.isActive = Obj.isActive;
                        }
                        entity.SaveChanges();
                        message = Obj.subjectId.ToString();

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
        [Route("DeleteSubject")]
        public string DeleteSubject(string token)
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

                int id = 0;
                bool final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        id = int.Parse(c.Value);
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
                            siteSubject objDelete = entity.siteSubject.Find(id);
                            entity.siteSubject.Remove(objDelete);
                            message = entity.SaveChanges().ToString();
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

                            siteSubject Obj = entity.siteSubject.Find(id);
                            Obj.isActive = false;
                                   message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);
                        }
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }
            }
        }

        #endregion
    }
}