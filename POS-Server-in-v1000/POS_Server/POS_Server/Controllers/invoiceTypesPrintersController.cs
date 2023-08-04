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
    [RoutePrefix("api/invoiceTypesPrinters")]
    public class invoiceTypesPrintersController : ApiController
    {
        CountriesController cc = new CountriesController();
        // GET api/<controller> get all invoiceTypesPrinters
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
                    var itemList = entity.invoiceTypesPrinters

                   .Select(S => new invoiceTypesPrintersModel()
                   {
                       invTypePrinterId = S.invTypePrinterId,
                       printerId = S.printerId,
                       invoiceTypeId = S.invoiceTypeId,
                       sizeId = S.sizeId,
                       notes = S.notes,
                       copyCount = S.copyCount,
                       createDate = S.createDate,
                       updateDate = S.updateDate,
                       createUserId = S.createUserId,
                       updateUserId = S.updateUserId,
                       sizeName = S.posPrinters.paperSize.paperSize1,
                       invoiceTypeName = S.InvoiceTypes.notes,
                   })
                   .ToList();

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
                    var item = entity.invoiceTypesPrinters
                   .Where(S => S.invTypePrinterId == Id)
                   .Select(S => new
                   {
                       S.invTypePrinterId,
                       S.printerId,
                       S.invoiceTypeId,
                       S.sizeId,
                       S.notes,
                       S.copyCount,
                       S.createDate,
                       S.updateDate,
                       S.createUserId,
                       S.updateUserId,
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
                invoiceTypesPrinters newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        itemObject = c.Value.Replace("\\", string.Empty);
                        itemObject = itemObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<invoiceTypesPrinters>(itemObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        invoiceTypesPrinters tmpObject = new invoiceTypesPrinters();
                        var sEntity = entity.Set<invoiceTypesPrinters>();
                        DateTime datenow = cc.AddOffsetTodate(DateTime.Now);
                        if (newObject.invTypePrinterId == 0)
                        {
                            //check old rows to remove
                            var tmpprnt = entity.posPrinters.Where(p => p.printerId == newObject.printerId).FirstOrDefault();
                            int posId = (int)tmpprnt.posId;

                            var oldlist = entity.invoiceTypesPrinters.Where(p => p.posPrinters.posId == posId
                             && p.invoiceTypeId == newObject.invoiceTypeId && p.invTypePrinterId != newObject.invTypePrinterId).ToList();
                            entity.invoiceTypesPrinters.RemoveRange(oldlist);
                            entity.SaveChanges();
                            //
                            newObject.createDate = datenow;
                            newObject.updateDate = datenow;
                            newObject.updateUserId = newObject.createUserId;
                            tmpObject = sEntity.Add(newObject);
                            entity.SaveChanges();
                            message = tmpObject.invTypePrinterId.ToString();
                        }
                        else
                        {

                            tmpObject = entity.invoiceTypesPrinters.Where(p => p.invTypePrinterId == newObject.invTypePrinterId).FirstOrDefault();
                            //check old rows to remove
                            var tmpprnt = entity.posPrinters.Where(p => p.printerId == newObject.printerId).FirstOrDefault();
                            int posId = (int)tmpprnt.posId;

                            var oldlist = entity.invoiceTypesPrinters.Where(p => p.posPrinters.posId == posId
                             && p.invoiceTypeId == newObject.invoiceTypeId && p.invTypePrinterId != newObject.invTypePrinterId).ToList();
                            entity.invoiceTypesPrinters.RemoveRange(oldlist);
                            entity.SaveChanges();
                            //
                            tmpObject.invTypePrinterId = newObject.invTypePrinterId;
                            tmpObject.printerId = newObject.printerId;
                            tmpObject.invoiceTypeId = newObject.invoiceTypeId;
                            tmpObject.sizeId = newObject.sizeId;
                            tmpObject.notes = newObject.notes;
                            tmpObject.copyCount = newObject.copyCount;

                            tmpObject.updateDate = datenow;

                            tmpObject.updateUserId = newObject.updateUserId;

                            entity.SaveChanges();
                            message = tmpObject.invTypePrinterId.ToString();
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
                int invTypePrinterId = 0;
                int userId = 0;
                bool final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        invTypePrinterId = int.Parse(c.Value);
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
                            invoiceTypesPrinters Obj = entity.invoiceTypesPrinters.Find(invTypePrinterId);
                            entity.invoiceTypesPrinters.Remove(Obj);
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
        [Route("GetByPrinterId")]
        public string GetByPrinterId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int printerId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        printerId = int.Parse(c.Value);
                    }
                }

                try
                {
                    List<invoiceTypesPrintersModel> itemList = GetByPrinterId(printerId);
                    return TokenManager.GenerateToken(itemList);
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        public List<invoiceTypesPrintersModel> GetByPrinterId(int printerId)
        {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemList = (from ip in entity.invoiceTypesPrinters
                                    join pr in entity.posPrinters on ip.printerId equals pr.printerId
                                    //join p in entity.pos on m.posId equals p.posId
                                    join invType in entity.InvoiceTypes on ip.invoiceTypeId equals invType.invoiceTypeId

                                    join paper in entity.paperSize on pr.sizeId equals paper.sizeId
                                    where ip.printerId == printerId
                                    select new invoiceTypesPrintersModel
                                    {
                                        invTypePrinterId = ip.invTypePrinterId,
                                        invoiceTypeId = invType.invoiceTypeId,
                                        invoiceTypeName = invType.notes,
                                        printerId = ip.printerId,
                                        //   notes = itunit.notes,
                                        //createDate = ip.createDate,
                                        //updateDate = ip.updateDate,
                                        //updateUserId = ip.updateUserId,
                                        //createUserId = ip.createUserId,

                                        // unitName = un.name,
                                        sizeId = paper.sizeId,
                                        sizeName = paper.paperSize1,
                                        printerName = pr.name,


                                        // type= item.type,
                                    }).ToList();
                    return  itemList;
                }           
        }
        [HttpPost]
        [Route("GetByPosForPrint")]  
        public string GetByPosForPrint(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int PosId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        PosId = int.Parse(c.Value);
                    }
                }
                try
                {

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemList = (from ip in entity.invoiceTypesPrinters
                                    join pr in entity.posPrinters on ip.printerId equals pr.printerId
                                    //join p in entity.pos on m.posId equals p.posId
                                    join invType in entity.InvoiceTypes on ip.invoiceTypeId equals invType.invoiceTypeId

                                    join paper in entity.paperSize on pr.sizeId equals paper.sizeId
                                    where pr.posId == PosId
                                    select new invoiceTypesPrintersModel
                                    {
                                        invTypePrinterId = ip.invTypePrinterId,
                                        invoiceTypeId = ip.invoiceTypeId,
                                        invoiceTypeName = invType.notes,
                                        invoiceType=invType.invoiceType,//
                                        printerId = ip.printerId,
                                        printerName = pr.name, //                                    
                                        sizeId = paper.sizeId,
                                        sizeName = paper.paperSize1,
                                        sizeValue = paper.sizeValue, //
                                        printerSysName= pr.printerName,
                                    }).ToList();
                    return TokenManager.GenerateToken(itemList);
                }

                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken(ex.ToString());
                }
            }
        }

        [HttpPost]
        [Route("updateListByPrinterId")]
        public string updateListByPrinterId(string token)
        {

            string message = "";
            int printerId = 0;
            int userId = 0;
            token = TokenManager.readToken(HttpContext.Current.Request);
            if (TokenManager.GetPrincipal(token) == null) //invalid authorization
            {
                return TokenManager.GenerateToken("-7");
            }
            else
            {
                int res = 0;
                string Object = "";
                List<int> newObject = new List<int>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<int>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "printerId")
                    {
                        printerId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                        List<invoiceTypesPrinters> newList = new List<invoiceTypesPrinters>();
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var printerobj = entity.posPrinters.Where(p => p.printerId == printerId).FirstOrDefault();
                            var iuoffer = entity.invoiceTypesPrinters.Where(p => p.printerId == printerId).ToList();

                            if (iuoffer.Count() > 0)
                            {
                                entity.invoiceTypesPrinters.RemoveRange(iuoffer);
                            }

                            if (newObject.Count() > 0)
                            {
                                invoiceTypesPrinters itemsprinterTmp = new invoiceTypesPrinters();
                                DateTime datenow = cc.AddOffsetTodate(DateTime.Now);
                                var iuofferold = entity.invoiceTypesPrinters.Where(p => newObject.Contains((int)p.invoiceTypeId) && printerobj.posId == p.posPrinters.posId);
                                if (iuofferold.Count() > 0)
                                { //delete other itemunit that belong to another printer in same pos
                                    entity.invoiceTypesPrinters.RemoveRange(iuofferold);
                                }
                                entity.SaveChanges();
                                foreach (int newitofrow in newObject)
                                {
                                    itemsprinterTmp = new invoiceTypesPrinters();
                                   

                                    itemsprinterTmp.printerId = printerId;
                                    itemsprinterTmp.createDate = datenow;
                                    itemsprinterTmp.updateDate = datenow;
                                    itemsprinterTmp.createUserId = userId;
                                    itemsprinterTmp.updateUserId = userId;
                                    itemsprinterTmp.invoiceTypeId = newitofrow;
                                    newList.Add(itemsprinterTmp);
                                }

                            }
                            entity.invoiceTypesPrinters.AddRange(newList);
                            res = entity.SaveChanges();
                            // return res;
                            return TokenManager.GenerateToken(res.ToString());
                        }

                    }
                    catch (Exception ex)
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

    }
}