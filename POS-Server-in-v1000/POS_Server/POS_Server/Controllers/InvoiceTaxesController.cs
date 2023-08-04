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
    [RoutePrefix("api/InvoiceTaxes")]
    public class InvoiceTaxesController : ApiController
    {
        CountriesController cc = new CountriesController();
        // GET api/<controller> get all posPrinters
        [HttpPost]
        [Route("GetAll")]
        public string GetAll(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            //bool canDelete = false;
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemList = entity.posPrinters

                   .Select(S => new posPrintersModel()
                   {
                       printerId = S.printerId,
                       name = S.name,
                       printerName = S.printerName,
                       notes = S.notes,
                       posId = S.posId,
                       createDate = S.createDate,
                       updateDate = S.updateDate,
                       createUserId = S.createUserId,
                       updateUserId = S.updateUserId,
                       purpose = S.purpose,
                       isActive = S.isActive,
                       copiesCount = S.copiesCount,


                   })
                   .ToList();

                    // can delet or not
                    //if (itemList.Count > 0)
                    //{
                    //    foreach (posPrintersModel item in itemList)
                    //    {
                    //        canDelete = false;

                    //        long Id = (long)item.printerId;
                    //        var rowitem = entity.itemsPrinters.Where(x => x.printerId == Id).Select(x => new { x.printerId }).FirstOrDefault();
                    //        var rowinvtype = entity.invoiceTypesPrinters.Where(x => x.printerId == Id).Select(x => new { x.printerId }).FirstOrDefault();

                    //        if ((rowitem is null) && (rowinvtype is null))
                    //            canDelete = true;

                    //        item.canDelete = canDelete;
                    //    }
                    //}
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
                    var item = entity.posPrinters
                   .Where(S => S.printerId == Id)
                   .Select(S => new
                   {
                       S.printerId,
                       S.name,
                       S.printerName,
                       S.notes,
                       S.posId,
                       S.createDate,
                       S.updateDate,
                       S.createUserId,
                       S.updateUserId,
                       S.purpose,
                       S.isActive,
                       S.copiesCount,



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
                posPrinters newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        itemObject = c.Value.Replace("\\", string.Empty);
                        itemObject = itemObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<posPrinters>(itemObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        posPrinters tmpObject = new posPrinters();
                        var sEntity = entity.Set<posPrinters>();
                        DateTime datenow = cc.AddOffsetTodate(DateTime.Now);
                        if (newObject.printerId == 0)
                        {

                            newObject.createDate = datenow;
                            newObject.updateDate = datenow;
                            newObject.updateUserId = newObject.createUserId;
                            tmpObject = sEntity.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject.printerId.ToString();

                        }
                        else
                        {

                            tmpObject = entity.posPrinters.Where(p => p.printerId == newObject.printerId).FirstOrDefault();
                          //var  tmpsize = entity.paperSize.Where(p => p.sizeValue== "A4").FirstOrDefault();
                          //  if (tmpsize.sizeId== newObject.sizeId)
                          //  {
                          //      //delete rows with invtype is small size if printer paper size changed to A4
                          //      var smalllist = entity.invoiceTypesPrinters.Where(p => p.InvoiceTypes.allowPaperSize == "small" && p.printerId == newObject.printerId).ToList();
                          //      entity.invoiceTypesPrinters.RemoveRange(smalllist);
                          //      entity.SaveChanges();
                          //  }
                            tmpObject.printerId = newObject.printerId;
                            tmpObject.name = newObject.name;
                            tmpObject.printerName = newObject.printerName;
                            tmpObject.notes = newObject.notes;
                            tmpObject.posId = newObject.posId;
                            tmpObject.updateDate = datenow;
                            tmpObject.updateUserId = newObject.updateUserId;
                            tmpObject.purpose = newObject.purpose;
                            tmpObject.isActive = newObject.isActive;
                            tmpObject.copiesCount = newObject.copiesCount;
                            tmpObject.sizeId = newObject.sizeId;
                            entity.SaveChanges();
                            message = tmpObject.printerId.ToString();
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
                long printerId = 0;
                long userId = 0;
                bool final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        printerId = long.Parse(c.Value);
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
                //if (final)
                //{
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            posPrinters Obj = entity.posPrinters.Find(printerId);
                            //check
                            // var rowitem = entity.itemsPrinters.Where(x => x.printerId == printerId).Select(x => new { x.printerId }).FirstOrDefault();
                            //delete related rows
                            var Listinvtype = entity.invoiceTypesPrinters.Where(x => x.printerId == printerId).ToList();
                            entity.invoiceTypesPrinters.RemoveRange(Listinvtype);
                            
                            entity.SaveChanges();
                                entity.posPrinters.Remove(Obj);
                                message = entity.SaveChanges().ToString();
                           
                            return TokenManager.GenerateToken(message);
                        }
                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                //}
                //else
                //{
                //    try
                //    {
                //        using (incposdbEntities entity = new incposdbEntities())
                //        {
                //            posPrinters Obj = entity.posPrinters.Find(printerId);

                //            Obj.isActive = 0;
                //            Obj.updateUserId = userId;
                //            Obj.updateDate = cc.AddOffsetTodate(DateTime.Now);

                //            message = entity.SaveChanges().ToString();
                //            return TokenManager.GenerateToken(message);
                //        }
                //    }
                //    catch
                //    {
                //        message = "0";
                //        return TokenManager.GenerateToken(message);
                //    }
                //}
            }
        }

        [HttpPost]
        [Route("GetByPosId")]
        public string GetByPosId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int posId = 0;
                bool canDelete = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        posId = int.Parse(c.Value);
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var itemList = (from S in entity.posPrinters
                                        where S.posId == posId
                                        select new posPrintersModel()
                                        {
                                            printerId = S.printerId,
                                            name = S.name,
                                            printerName = S.printerName,
                                            notes = S.notes,
                                            posId = S.posId,
                                            createDate = S.createDate,
                                            updateDate = S.updateDate,
                                            createUserId = S.createUserId,
                                            updateUserId = S.updateUserId,
                                            purpose = S.purpose,
                                            isActive = S.isActive,
                                            copiesCount = S.copiesCount,
                                            sizeId = S.sizeId,
                                            sizeName = S.paperSize.paperSize1,
                                             sizeValue = S.paperSize.sizeValue,

                                        })
                           .ToList();
                        invoiceTypesPrintersController invtpctrlr = new invoiceTypesPrintersController();
                    
                        foreach (posPrintersModel row in itemList)
                        {
                            row.invoiceTypesPrintersList = invtpctrlr.GetByPrinterId(row.printerId);                       
                        }
                        // can delet or not
                        //if (itemList.Count > 0)
                        //{
                        //    foreach (posPrintersModel item in itemList)
                        //    {
                        //        canDelete = false;

                        //        int pId = (long)item.printerId;
                        //        var rowitem = entity.itemsPrinters.Where(x => x.printerId == pId).Select(x => new { x.printerId }).FirstOrDefault();
                        //        var rowinvtype = entity.invoiceTypesPrinters.Where(x => x.printerId == pId).Select(x => new { x.printerId }).FirstOrDefault();

                        //        if ((rowitem is null) && (rowinvtype is null))
                        //            canDelete = true;

                        //        item.canDelete = canDelete;
                        //    }
                        //}
                        return TokenManager.GenerateToken(itemList);
                    }
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

    }
}