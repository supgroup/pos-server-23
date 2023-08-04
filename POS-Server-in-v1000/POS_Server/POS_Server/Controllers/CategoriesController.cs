using Newtonsoft.Json;
using POS_Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Data.Entity.Migrations;
using POS_Server.Models.VM;
using System.Security.Claims;
using Newtonsoft.Json.Converters;
using System.Web;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/Categories")]
    public class CategoriesController : ApiController
    {
        CountriesController cc = new CountriesController();
        // GET api/category
        List<int> categoriesId = new List<int>();

        [HttpPost]
        [Route("GetAllCategories")]
        public string GetAllCategories(string token)
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
                //int userId = 0;
                //IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                //foreach (Claim c in claims)
                //{
                //    if (c.Type == "itemId")
                //    {
                //        userId = int.Parse(c.Value);
                //    }
                //}
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var categoriesList = (from p in entity.categories 
                             //join cu in entity.categoryuser on p.categoryId equals cu.categoryId where cu.userId == userId
                             select new CategoryModel()
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
                                 //sequence = cu.sequence,
                                 isTaxExempt = p.isTaxExempt,
                             }).ToList().OrderBy(x => x.sequence).ToList();
                    if (categoriesList.Count > 0)
                    {
                        for (int i = 0; i < categoriesList.Count; i++)
                        {
                            canDelete = false;
                            if (categoriesList[i].isActive == 1)
                            {
                                int categoryId = (int)categoriesList[i].categoryId;
                                var items = entity.items.Where(x => x.categoryId == categoryId).Select(b => new { b.itemId }).FirstOrDefault();
                                var childCategoryL = entity.categories.Where(x => x.parentId == categoryId).Select(b => new { b.categoryId }).FirstOrDefault();

                                if ((items is null) && (childCategoryL is null))
                                    canDelete = true;
                            }
                            categoriesList[i].canDelete = canDelete;
                        }
                    }
                    
                    return TokenManager.GenerateToken(categoriesList);

                }
            }
        }

        //[HttpPost]
        //[Route("GetSubCategories")]
        //public string GetSubCategories(string token)
        //{
        //    token = TokenManager.readToken(HttpContext.Current.Request);
        //    var strP = TokenManager.GetPrincipal(token);
        //    if (strP != "0") //invalid authorization
        //    {
        //        return TokenManager.GenerateToken(strP);
        //    }
        //    else
        //    {
        //        int categoryId = 0;
        //        int userId = 0;
        //        IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
        //        foreach (Claim c in claims)
        //        {
        //            if (c.Type == "itemId")
        //            {
        //                categoryId = int.Parse(c.Value);
        //            } else if (c.Type == "userId")
        //            {
        //                userId = int.Parse(c.Value);
        //            }
        //        }
        //        using (incposdbEntities entity = new incposdbEntities())
        //        {
        //            if (categoryId != 0)
        //            {
        //                var categoriesList = (from p in entity.categories.Where(x => x.parentId == categoryId && x.isActive == 1)
        //                         join cu in entity.categoryuser on p.categoryId equals cu.categoryId
        //                         where cu.userId == userId
        //                         select new CategoryModel() {
        //                             categoryId = p.categoryId,
        //                             name = p.name,
        //                             categoryCode = p.categoryCode,
        //                             createDate = p.createDate,
        //                             createUserId = p.createUserId,
        //                             details = p.details,
        //                             image = p.image,
        //                             notes = p.notes,
        //                             parentId = p.parentId,
        //                             taxes = p.taxes,
        //                             fixedTax = p.fixedTax,
        //                             updateDate = p.updateDate,
        //                             updateUserId = p.updateUserId,
        //                             isActive = p.isActive,
        //                             sequence = cu.sequence,
        //                             isTaxExempt = p.isTaxExempt,
        //                         }).ToList().OrderBy(x => x.sequence).ToList();

        //            return TokenManager.GenerateToken(categoriesList);
        //            }
        //            else
        //            {
        //                var categoriesList = (from p in entity.categories.Where(x => x.parentId == 0 && x.isActive == 1)
        //                                      join cu in entity.categoryuser on p.categoryId equals cu.categoryId
        //                                      where cu.userId == userId
        //                                      select new CategoryModel()
        //                                      {
        //                                          categoryId = p.categoryId,
        //                                          name = p.name,
        //                                          categoryCode = p.categoryCode,
        //                                          createDate = p.createDate,
        //                                          createUserId = p.createUserId,
        //                                          details = p.details,
        //                                          image = p.image,
        //                                          notes = p.notes,
        //                                          parentId = p.parentId,
        //                                          taxes = p.taxes,
        //                                          fixedTax = p.fixedTax,
        //                                          updateDate = p.updateDate,
        //                                          updateUserId = p.updateUserId,
        //                                          isActive = p.isActive,
        //                                          sequence = cu.sequence,
        //                                          isTaxExempt = p.isTaxExempt,
        //                                      }).ToList().OrderBy(x => x.sequence).ToList();

                      
        //            return TokenManager.GenerateToken(categoriesList);
        //            }
        //        }
        //    }
        //}
        // GET api/category/5
        [HttpPost]
        [Route("GetCategoryByID")]
        public string GetCategoryByID(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int categoryId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        categoryId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {

                    var category = entity.categories
                   .Where(c => c.categoryId == categoryId)
                   .Select(p => new {
                       p.categoryId,
                       p.name,
                       p.categoryCode,
                       p.createDate,
                       p.createUserId,
                       p.details,
                       p.image,
                       p.notes,
                       p.parentId,
                       p.taxes,
                       p.fixedTax,
                       p.updateDate,
                       p.updateUserId,
                       p.isTaxExempt,
                   })
                   .FirstOrDefault();
                    return TokenManager.GenerateToken(category);
                }
            }
        }
        [HttpPost]
        [Route("GetCategoryTreeByID")]
        public string GetCategoryTreeByID(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            List<categories> treecat = new List<categories>();
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int categoryID = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        categoryID = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    int parentid = categoryID; // if want to show the last category 
                    while (parentid > 0)
                    {
                        categories tempcate = new categories();
                        var category = entity.categories.Where(c => c.categoryId == parentid)
                            .Select(p => new {
                                p.categoryId,
                                p.name,
                                p.categoryCode,
                                p.createDate,
                                p.createUserId,
                                p.details,
                                p.image,
                                p.notes,
                                p.parentId,
                                p.taxes,
                                p.fixedTax,
                                p.updateDate,
                                p.updateUserId,
                                p.isTaxExempt,
                            }).FirstOrDefault();


                        tempcate.categoryId = category.categoryId;

                        tempcate.name = category.name;
                        tempcate.categoryCode = category.categoryCode;
                        tempcate.createDate = category.createDate;
                        tempcate.createUserId = category.createUserId;
                        tempcate.details = category.details;
                        tempcate.image = category.image;
                        tempcate.notes = category.notes;
                        tempcate.parentId = category.parentId;
                        tempcate.taxes = category.taxes;
                        tempcate.fixedTax = category.fixedTax;
                        tempcate.isTaxExempt = category.isTaxExempt;
                        tempcate.updateDate = category.updateDate;
                        tempcate.updateUserId = category.updateUserId;
                         

                        parentid = (int)tempcate.parentId;

                        treecat.Add(tempcate);

                    }
                    return TokenManager.GenerateToken(treecat);

                }


            }
        }

        //[HttpPost]
        //[Route("GetSubCategoriesSeq")]
        //public string GetSubCategoriesSeq(string token)
        //{
        //    token = TokenManager.readToken(HttpContext.Current.Request);
        //    var strP = TokenManager.GetPrincipal(token);
        //    if (strP != "0") //invalid authorization
        //    {
        //        return TokenManager.GenerateToken(strP);
        //    }
        //    else
        //    {
        //        int categoryId = 0;
        //        int userId = 0;
        //        IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
        //        foreach (Claim c in claims)
        //        {
        //            if (c.Type == "itemId")
        //            {
        //                categoryId = int.Parse(c.Value);
        //            }
        //            else if (c.Type == "userId")
        //            {
        //                userId = int.Parse(c.Value);
        //            }
        //        }
        //        using (incposdbEntities entity = new incposdbEntities())
        //        {
        //            if (categoryId != 0)
        //            {
        //                var categoriesList = (from C in entity.categories
        //                                      join S in entity.categoryuser on C.categoryId equals S.categoryId into jS
        //                                      from jSS in jS.DefaultIfEmpty()
        //                                      select new
        //                                      {
        //                                          C.categoryId,
        //                                          C.name,
        //                                          C.categoryCode,
        //                                          C.createDate,
        //                                          C.createUserId,
        //                                          C.details,
        //                                          C.image,
        //                                          C.notes,
        //                                          C.parentId,
        //                                          C.taxes,
        //                                          C.fixedTax,
        //                                          C.updateDate,
        //                                          C.updateUserId,
        //                                          C.isActive,
        //                                          C.isTaxExempt,
        //                                          jSS.sequence,
        //                                          jSS.userId,

        //                                      }


        //              ).Where(c => c.parentId == categoryId && c.isActive == 1 && c.userId == userId).OrderBy(c => c.sequence)

        //               .ToList();
                      
        //            return TokenManager.GenerateToken(categoriesList);
        //            }
        //            else
        //            {
        //                var categoriesList = (from C in entity.categories
        //                                      join S in entity.categoryuser on C.categoryId equals S.categoryId into jS
        //                                      from jSS in jS.DefaultIfEmpty()
        //                                      select new
        //                                      {
        //                                          C.categoryId,
        //                                          C.name,
        //                                          C.categoryCode,
        //                                          C.createDate,
        //                                          C.createUserId,
        //                                          C.details,
        //                                          C.image,
        //                                          C.notes,
        //                                          C.parentId,
        //                                          C.taxes,
        //                                          C.fixedTax,
        //                                          C.updateDate,
        //                                          C.updateUserId,
        //                                          C.isActive,
        //                                          jSS.sequence,
        //                                          jSS.userId,
        //                                          C.isTaxExempt,
        //                                      }


        //              ).Where(c => c.parentId == 0 && c.isActive == 1 && c.userId == userId)
        //               .ToList();
                      
        //            return TokenManager.GenerateToken(categoriesList);
        //            }
        //        }
        //    }
        //}
        // add or update category
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
                string categoryObject = "";
                categories newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        categoryObject = c.Value.Replace("\\", string.Empty);
                        categoryObject = categoryObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<categories>(categoryObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
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
                try
                {
                    categories tmpCategory;                    
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var categoryEntity = entity.Set<categories>();
                       // var catEntity = entity.Set<categoryuser>();
                        if (newObject.categoryId == 0)
                        {
                            newObject.createDate = cc.AddOffsetTodate(DateTime.Now);
                            newObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            newObject.updateUserId = newObject.createUserId;

                            tmpCategory = categoryEntity.Add(newObject);

                            // get all users
                           // var users = entity.users.Where(x => x.isActive == 1).Select(x => x.userId).ToList();
                           // for (int i = 0; i < users.Count; i++)
                            //{
                              //  int userId = users[i];
                               // var sequence = entity.categoryuser.Where(x => x.userId == userId).Select(x => x.sequence).Max();
                                //if (sequence == null)
                                //    sequence = 0;
                                //sequence++;
                                //categoryuser cu = new categoryuser()
                                //{
                                //    categoryId = tmpCategory.categoryId,
                                //    userId = userId,
                                //    sequence = sequence,
                                //    createDate = cc.AddOffsetTodate(DateTime.Now),
                                //    updateDate = cc.AddOffsetTodate(DateTime.Now),
                                //    createUserId = newObject.createUserId,
                                //    updateUserId = newObject.updateUserId,
                                //};
                                //catEntity.Add(cu);
                            //}
                           entity.SaveChanges();
                        }
                        else
                        {
                            tmpCategory = entity.categories.Where(p => p.categoryId == newObject.categoryId).First();
                            tmpCategory.categoryCode = newObject.categoryCode;
                            tmpCategory.details = newObject.details;
                            tmpCategory.name = newObject.name;
                            tmpCategory.notes = newObject.notes;
                            tmpCategory.parentId = newObject.parentId;
                            tmpCategory.taxes = newObject.taxes;
                            tmpCategory.fixedTax = newObject.fixedTax;
                            tmpCategory.isTaxExempt = newObject.isTaxExempt;
                            tmpCategory.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            tmpCategory.updateUserId = newObject.updateUserId;
                            tmpCategory.isActive = newObject.isActive;
                            entity.SaveChanges();
                            int categoryId = tmpCategory.categoryId;
                            short? isActivecat = tmpCategory.isActive;
                            int? updateuser = tmpCategory.updateUserId;
                            //update is active sons and items sons
                            // get all sub categories of categoryId

                            List<categories> categoriesList = entity.categories
                             .ToList()
                              .Select(p => new categories
                              {
                                  categoryId = p.categoryId,
                                  name = p.name,
                                  parentId = p.parentId,
                              })
                             .ToList();

                            categoriesId = new List<int>();
                            List<int> catIdlist = new List<int>();
                            categoriesId.Add(categoryId);
                            ItemsController icls = new ItemsController();

                            var result = Recursive(categoriesList, categoryId).ToList();


                            foreach (var r in result)
                            {
                                catIdlist.Add(r.categoryId);

                            }

                            // end sub cat
                            // disactive selected category
                       
                            // disactive subs categories

                            List<categories> sonList = entity.categories.Where(U => catIdlist.Contains(U.categoryId)).ToList();

                            if (sonList.Count > 0)
                            {
                                for (int i = 0; i < sonList.Count; i++)
                                {

                                    sonList[i].isActive = isActivecat;
                                    sonList[i].updateUserId = updateuser;
                                    sonList[i].updateDate = cc.AddOffsetTodate(DateTime.Now);


                                    entity.categories.AddOrUpdate(sonList[i]);

                                }
                                entity.SaveChanges();
                            }
                            if (tmpCategory.fixedTax == 1)
                            {
                                var categories = entity.categories.Where(U => catIdlist.Contains((int)U.categoryId)).ToList();
                                categories.ForEach(a => a.taxes = tmpCategory.taxes);
                            }
                            // disactive items related to selected category and subs
                            catIdlist.Add(categoryId);
                           
                            var catitems = entity.items.Where(U => catIdlist.Contains((int)U.categoryId)).ToList();
                            if (catitems.Count > 0)
                            {
                                for (int i = 0; i < catitems.Count; i++)
                                {
                                    if(tmpCategory.fixedTax == 1)
                                        catitems[i].taxes = tmpCategory.taxes;
                                    catitems[i].isActive = (byte)isActivecat;
                                    catitems[i].updateUserId = updateuser;
                                    catitems[i].updateDate = cc.AddOffsetTodate(DateTime.Now);
                                    entity.items.AddOrUpdate(catitems[i]);

                                }
                                entity.SaveChanges();
                            }
                        }
                    }
                    message =  tmpCategory.categoryId.ToString();
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
            string message = "0";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int categoryId = 0;
                int userId = 0;
                Boolean final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        categoryId = int.Parse(c.Value);
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
                            var childCategories = entity.categories.Where(u => u.parentId == categoryId && u.isActive == 1).FirstOrDefault();

                            if (childCategories == null)
                            {
                               // entity.categoryuser.RemoveRange(entity.categoryuser.Where(x => x.categoryId == categoryId));

                                var tmpCategory = entity.categories.Where(p => p.categoryId == categoryId).First();
                                entity.categories.Remove(tmpCategory);

                                message = entity.SaveChanges().ToString();
                                return TokenManager.GenerateToken(message);
                            }
                            else
                                message = "0";
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
                        {  // get all sub categories of categoryId
                            List<categories> categoriesList = entity.categories
                             .ToList()
                              .Select(p => new categories
                              {
                                  categoryId = p.categoryId,
                                  name = p.name,
                                  parentId = p.parentId,
                              })
                             .ToList();

                            categoriesId = new List<int>();
                            List<int>  catIdlist = new List<int>();
                            categoriesId.Add(categoryId);
                            ItemsController icls = new ItemsController();
                           
                            var result =Recursive(categoriesList, categoryId).ToList();
                           
                            
                            foreach (var r in result)
                            {
                                catIdlist.Add(r.categoryId);
                             
                            }
                            
                            // end sub cat
                            // disactive selected category
                            var tmpCategory = entity.categories.Where(p => p.categoryId == categoryId).First();
                            tmpCategory.isActive = 0;
                            tmpCategory.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            tmpCategory.updateUserId = userId;
                            entity.categories.AddOrUpdate(tmpCategory);
                            entity.SaveChanges();

                       // disactive subs categories

                           List<categories> sonList = entity.categories.Where(U => catIdlist.Contains(U.categoryId)).ToList();

                            if (sonList.Count > 0)
                            {
                                for (int i = 0; i < sonList.Count; i++)
                                {
                                    sonList[i].isActive = 0;
                                    sonList[i].updateDate = cc.AddOffsetTodate(DateTime.Now);
                                    sonList[i].updateUserId = userId;
                                    entity.categories.AddOrUpdate(sonList[i]);

                                }
                                entity.SaveChanges();
                            }
                            // disactive items related to selected category and subs
                            catIdlist.Add(categoryId);
                              var catitems = entity.items.Where(U => catIdlist.Contains((int)U.categoryId)).ToList();
                                if (catitems.Count > 0)
                                {
                                    for (int i = 0; i < catitems.Count; i++)
                                    {
                                    catitems[i].isActive = 0;
                                    catitems[i].updateDate = cc.AddOffsetTodate(DateTime.Now);
                                    catitems[i].updateUserId = userId;
                                    entity.items.AddOrUpdate(catitems[i]);
                                   
                                    }
                                   entity.SaveChanges();

                                }



                            message = "1";
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


        [Route("UpdateImage")]
        public string UpdateImage(string token)
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
                string categoryObject = "";
                categories catObj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        categoryObject = c.Value.Replace("\\", string.Empty);
                        categoryObject = categoryObject.Trim('"');
                        catObj = JsonConvert.DeserializeObject<categories>(categoryObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    categories category;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var agentEntity = entity.Set<agents>();
                        category = entity.categories.Where(p => p.categoryId == catObj.categoryId).First();
                        category.image = catObj.image;
                        entity.SaveChanges();
                    }
                    message =  category.categoryId.ToString();
                    return TokenManager.GenerateToken(message);
                }

                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }
            }
        }
        [Route("PostCategoryImage")]
        public IHttpActionResult PostCategoryImage()
        {

            try
            {
                var httpRequest = HttpContext.Current.Request;

                foreach (string file in httpRequest.Files)
                {

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    string imageName = postedFile.FileName;
                    string imageWithNoExt = Path.GetFileNameWithoutExtension(postedFile.FileName);

                    if (postedFile != null && postedFile.ContentLength > 0)
                    {

                        int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB

                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png", ".bmp", ".jpeg", ".tiff",".jfif" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();

                        if (!AllowedFileExtensions.Contains(extension))
                        {

                            var message = string.Format("Please Upload image of type .jpg,.gif,.png.");
                            return Ok(message);
                        }
                        else if (postedFile.ContentLength > MaxContentLength)
                        {

                            var message = string.Format("Please Upload a file upto 1 mb.");

                            return Ok(message);
                        }
                        else
                        {
                            //  check if image exist
                            var pathCheck = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\category"), imageWithNoExt);
                            var files = Directory.GetFiles(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\category"), imageWithNoExt + ".*");
                            if (files.Length > 0)
                            {
                                File.Delete(files[0]);
                            }

                            //Userimage myfolder name where i want to save my image
                            var filePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\category"), imageName);
                            postedFile.SaveAs(filePath);

                        }
                    }

                    var message1 = string.Format("Image Updated Successfully.");
                    return Ok(message1);
                }
                var res = string.Format("Please Upload a image.");

                return Ok(res);
            }
            catch
            {
                var res = string.Format("some Message");

                return Ok(res);
            }
        }
        //[HttpGet]
        //[Route("GetImage")]
        //public HttpResponseMessage GetImage(string imageName)
        //{
        //    if (String.IsNullOrEmpty(imageName))
        //        return Request.CreateResponse(HttpStatusCode.BadRequest);

        //    string localFilePath;

        //    localFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\category"), imageName);

        //    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
        //    if (System.IO.File.Exists(localFilePath))
        //    {
        //        response.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
        //        response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
        //        response.Content.Headers.ContentDisposition.FileName = imageName;
        //    }
        //    else
        //    {
        //        response.Content = null;
        //    }
        //    return response;
        //}

        [HttpPost]
        [Route("GetImage")]
        public string GetImage(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string imageName = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "imageName")
                    {
                        imageName = c.Value;
                    }
                }
                if (String.IsNullOrEmpty(imageName))
                    return TokenManager.GenerateToken("0");

                string localFilePath;

                try
                {
                    localFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\category"), imageName);

                    byte[] b = System.IO.File.ReadAllBytes(localFilePath);
                    return TokenManager.GenerateToken(Convert.ToBase64String(b));
                }
                catch
                {
                    return TokenManager.GenerateToken(null);

                }
            }
        }
        [HttpPost]
        public IEnumerable<categories> Recursive(List<categories> categoriesList, int toplevelid)
        {
            List<categories> inner = new List<categories>();

            foreach (var t in categoriesList.Where(item => item.parentId == toplevelid))
            {
                categoriesId.Add(t.categoryId);
                inner.Add(t);
                inner = inner.Union(Recursive(categoriesList, t.categoryId)).ToList();
            }

            return inner;
        }

        }
}