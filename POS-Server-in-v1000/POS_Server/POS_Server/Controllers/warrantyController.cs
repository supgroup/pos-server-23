using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web;
using POS_Server.Models;
namespace POS_Server.Controllers
{
    [RoutePrefix("api/warranty")]
    public class warrantyController : ApiController
    {
        CountriesController cc = new CountriesController();
        // GET api/<controller>
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
                bool canDelete = false;
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemList = (from S in entity.warranty


                                    select new warrantyModel
                                    {
                                        warrantyId = S.warrantyId,
                                        name = S.name,
                                        description = S.description,
                                        notes = S.notes,
                                        isActive = S.isActive,
                                        createDate = S.createDate,
                                        updateDate = S.updateDate,
                                        createUserId = S.createUserId,
                                        updateUserId = S.updateUserId,


                                    }).ToList();
                    if (itemList.Count > 0)
                    {
                        for (int i = 0; i < itemList.Count; i++)
                        {
                            canDelete = false;


                            int warrantyId = (int)itemList[i].warrantyId;
                          
                            var operationsiu = entity.itemsUnits.Where(x => x.warrantyId == warrantyId).Select(b => new { b.itemId }).ToList();
                            var operationsit = entity.itemsTransfer.Where(x => x.warrantyId == warrantyId).Select(b => new { b.itemsTransId }).ToList();

                            if (operationsiu.Count == 0 && operationsit.Count==0)
                            {
                                canDelete = true;
                            }
                            itemList[i].canDelete = canDelete;
                        }
                    }

                    return TokenManager.GenerateToken(itemList);
                }
            }
        }
        [HttpPost]
        [Route("GetbyId")]
        public string GetbyId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                {
                    int Id = 0;
                    IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                    foreach (Claim c in claims)
                    {
                        if (c.Type == "Id")
                        {
                            Id = int.Parse(c.Value);
                        }
                    }
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var item = (from S in entity.warranty
                                    where S.warrantyId== Id
                                    select new
                                    {
                                        S.warrantyId,
                                        S.name,
                                        S.description,
                                        S.notes,
                                        S.isActive,
                                        S.createDate,
                                        S.updateDate,
                                        S.createUserId,
                                        S.updateUserId,
                                    }).FirstOrDefault();
                        return TokenManager.GenerateToken(item);
                    }
                }
            }
        }

        // add or update location
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
                string  Object = "";
                warranty newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                         Object = c.Value.Replace("\\", string.Empty);
                         Object =  Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<warranty>( Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        warranty tmpObject = new warranty();
                        var warrantyEntity = entity.Set<warranty>();
                        if (newObject.warrantyId == 0)
                        {
                            newObject.createDate = cc.AddOffsetTodate(DateTime.Now);
                            newObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            newObject.updateUserId = newObject.createUserId;

                            tmpObject = warrantyEntity.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject.warrantyId.ToString();
                            return TokenManager.GenerateToken(message);
                        }
                        else
                        {
                            tmpObject = entity.warranty.Where(p => p.warrantyId == newObject.warrantyId).FirstOrDefault();
                            
                            tmpObject.name = newObject.name;
                            tmpObject.description = newObject.description;
                            tmpObject.notes = newObject.notes;
                            tmpObject.isActive = newObject.isActive;
                            tmpObject.createDate = newObject.createDate;
                       
                           // tmpObject.createUserId = newObject.createUserId;
                            tmpObject.updateUserId = newObject.updateUserId;
                            tmpObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
                          
                            entity.SaveChanges();
                            message = tmpObject.warrantyId.ToString();
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
                int itemId = 0;
                int userId = 0;
                Boolean final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        itemId = int.Parse(c.Value);
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
                            warranty Obj = entity.warranty.Find(itemId);

                            entity.warranty.Remove( Obj);
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
                            warranty Obj = entity.warranty.Find(itemId);

                             Obj.isActive = false;
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
}