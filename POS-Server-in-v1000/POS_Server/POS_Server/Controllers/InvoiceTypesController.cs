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
    [RoutePrefix("api/InvoiceTypes")]
    public class InvoiceTypesController : ApiController
    {
        CountriesController cc = new CountriesController();
        // GET api/<controller> get all InvoiceTypes
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
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemList = entity.InvoiceTypes

                   .Select(S => new InvoiceTypesModel()
                   {
                       invoiceTypeId = S.invoiceTypeId,
                       invoiceType = S.invoiceType,
                       department = S.department,
                       notes = S.notes,
                       allowPaperSize = S.allowPaperSize,
                       isActive = S.isActive,
                       sequence=S.sequence,

                   })
                   .ToList().Where(x=>x.isActive==true).ToList().OrderBy(x=>x.sequence);

                    // can delet or not

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
                    var item = entity.InvoiceTypes
                   .Where(S => S.invoiceTypeId == Id)
                   .Select(S => new
                   {
                       S.invoiceTypeId,
                       S.invoiceType,
                       S.department,
                       S.notes,
                        S.allowPaperSize,
                        S.isActive,


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
                InvoiceTypes newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        itemObject = c.Value.Replace("\\", string.Empty);
                        itemObject = itemObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<InvoiceTypes>(itemObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        InvoiceTypes tmpObject = new InvoiceTypes();
                        var sEntity = entity.Set<InvoiceTypes>();
                        DateTime datenow = cc.AddOffsetTodate(DateTime.Now);
                        if (newObject.invoiceTypeId == 0)
                        {
                          
                            tmpObject = sEntity.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject.invoiceTypeId.ToString();
                        }
                        else
                        {

                            tmpObject = entity.InvoiceTypes.Where(p => p.invoiceTypeId == newObject.invoiceTypeId).FirstOrDefault();
                            tmpObject.invoiceTypeId = newObject.invoiceTypeId;
                            tmpObject.invoiceType = newObject.invoiceType;
                            tmpObject.department = newObject.department;
                            tmpObject.notes = newObject.notes;

                            entity.SaveChanges();
                            message = tmpObject.invoiceTypeId.ToString();
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
                long invoiceTypeId = 0;
                long userId = 0;
                bool final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        invoiceTypeId = long.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = long.Parse(c.Value);
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
                            InvoiceTypes Obj = entity.InvoiceTypes.Find(invoiceTypeId);
                            entity.InvoiceTypes.Remove(Obj);
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
                return TokenManager.GenerateToken(message);
         
            }
        }

        [HttpPost]
        [Route("GetTypesOfPrinter")]
        public string GetTypesOfPrinter(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                long printerId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        printerId = long.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemList = (from ip in entity.invoiceTypesPrinters
                                        //join pr in entity.posPrinters on ip.printerId equals pr.printerId
                                        //join p in entity.pos on m.posId equals p.posId
                                    join invType in entity.InvoiceTypes on ip.invoiceTypeId equals invType.invoiceTypeId
                                    // join paper in entity.paperSize on pr.sizeId equals paper.sizeId
                                    where ip.printerId == printerId 
                                    select new InvoiceTypesModel
                                    {
                                        invoiceTypeId = invType.invoiceTypeId,
                                        invoiceType = invType.invoiceType,
                                        department = invType.department,
                                        notes = invType.notes,
                                        allowPaperSize = invType.allowPaperSize,
                                        isActive = invType.isActive,
                                        sequence = invType.sequence,                                         
                                    }).ToList().OrderBy(x=>x.sequence);
                    return TokenManager.GenerateToken(itemList);
                }
            }
        }
    }
}