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
    [RoutePrefix("api/Taxes")]
    public class TaxesController : ApiController
    {
        CountriesController cc = new CountriesController();
        // GET api/<controller> get all posPrinters
        [HttpPost]
        [Route("Get")]
        public string Get(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string type = "";
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
                    var taxesList = entity.taxes
                   .Select(p => new TaxesModel
                   {
                       taxId = p.taxId,
                       name = p.name,
                       taxTypeId = p.taxTypeId,
                       nameAr = p.nameAr,
                       rate = p.rate,
                       createUserId = p.createUserId,
                       updateUserId = p.updateUserId,
                       notes = p.notes,
                       isActive = p.isActive,
                       createDate = p.createDate,
                       updateDate = p.updateDate,
                       taxType=p.taxType,
                   })
                   .ToList();
                    if (taxesList.Count > 0)
                    {
                        for (int i = 0; i < taxesList.Count; i++)
                        {
                            canDelete = false;
                            if (taxesList[i].isActive == true)
                            {
                                int taxId = (int)taxesList[i].taxId;
                                var invoicesL = entity.invoiceTaxes.Where(x => x.taxId == taxId).Select(b => new { b.invoiceId }).FirstOrDefault();
                               
                                if (invoicesL is null) 
                                    canDelete = true;
                            }
                            taxesList[i].canDelete = canDelete;
                        }
                    }
                    return TokenManager.GenerateToken(taxesList);
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
                    var item = entity.taxes
                   .Where(p => p.taxId == Id)
                   .Select(p => new
                   {
                       taxId = p.taxId,
                       name = p.name,
                       taxTypeId = p.taxTypeId,
                       nameAr = p.nameAr,
                       rate = p.rate,
                       createUserId = p.createUserId,
                       updateUserId = p.updateUserId,
                       notes = p.notes,
                       isActive = p.isActive,
                       createDate = p.createDate,
                       updateDate = p.updateDate,
                       taxType = p.taxType,

                   })
                   .FirstOrDefault();
                    return TokenManager.GenerateToken(item);
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
                taxes newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        itemObject = c.Value;
                        newObject = JsonConvert.DeserializeObject<taxes>(itemObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        taxes tmpObject = new taxes();
                        var sEntity = entity.Set<taxes>();
                        DateTime datenow = cc.AddOffsetTodate(DateTime.Now);
                        if (newObject.taxId == 0)
                        {

                            newObject.createDate = datenow;
                            newObject.updateDate = datenow;
                            newObject.isActive = true;
                            newObject.updateUserId = newObject.createUserId;
                            tmpObject = sEntity.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject.taxId.ToString();

                        }
                        else
                        {

                            tmpObject = entity.taxes.Find(newObject.taxId);

                            tmpObject.taxTypeId = newObject.taxTypeId;
                            tmpObject.name = newObject.name;
                            tmpObject.nameAr = newObject.nameAr;
                            tmpObject.rate = newObject.rate;
                            tmpObject.notes = newObject.notes;
                            tmpObject.updateDate = datenow;
                            tmpObject.updateUserId = newObject.updateUserId;
                            tmpObject.isActive = newObject.isActive;
                            tmpObject.taxType = newObject.taxType;
                            
                            entity.SaveChanges();
                            message = tmpObject.taxId.ToString();
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
                long taxId = 0;
                int userId = 0;
                bool final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        taxId = long.Parse(c.Value);
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
                            taxes Obj = entity.taxes.Find(taxId);
                          
                            entity.taxes.Remove(Obj);
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
                            taxes Obj = entity.taxes.Find(taxId);

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

    }
}