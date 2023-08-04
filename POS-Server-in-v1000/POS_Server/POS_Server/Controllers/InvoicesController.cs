using LinqKit;
using Newtonsoft.Json;
using POS_Server.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity.Core.Objects;
using POS_Server.Models.VM;
using System.Security.Claims;
using Newtonsoft.Json.Converters;
using System.Web;
using POS_Server.Classes;
using System.Threading.Tasks;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/Invoices")]
    public class InvoicesController : ApiController
    {
        List<string> salesType = new List<string>() { "sd", "sbd", "s", "sb" };
        CountriesController countryc = new CountriesController();
      
 
        [HttpPost]
        [Route("GetByInvoiceId")]
        public async Task<string> GetByInvoiceId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
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
             
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var invoice = (from b in entity.invoices.Where(x => x.invoiceId == invoiceId)
                                        join l in entity.branches on b.branchId equals l.branchId into lj
                                        from x in lj.DefaultIfEmpty()
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            mainInvNumber = entity.invoices.Where(m => m.invoiceId == b.invoiceMainId).FirstOrDefault().invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            taxValue = b.taxValue,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchName = x.name,
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            userId = b.userId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            cashReturn = b.cashReturn,
                                            realShippingCost = b.realShippingCost,
                                            shippingCost = b.shippingCost,
                                            isOrginal = b.isOrginal,
                                            printedcount = b.printedcount,
                                            isPrePaid = b.isPrePaid,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,
                                            performed = (entity.invoices.Where(y => y.invoiceMainId == b.invoiceId).FirstOrDefault() == null) ? false : true,

                                        }).FirstOrDefault();

                  
                    if (invoice != null)
                    {
                        ItemsTransferController ic = new ItemsTransferController();
                        CashTransferController cc = new CashTransferController();

                        invoice.invoiceItems = await ic.Get(invoice.invoiceId);
                        invoice.itemsCount = invoice.invoiceItems.Count;
                        invoice.cachTrans = cc.GetPayedByInvId(invoiceId);
                        invoice.invoiceTaxes = GetInvoiceTaxes(invoiceId);
                        #region can return
                        var returnPeriodSet = entity.setValues.Where(x => x.setting.name == "returnPeriod").Select(x => x.value).SingleOrDefault();

                        int returnPeriod = 0;
                        if (returnPeriodSet != null)
                            returnPeriod = int.Parse(returnPeriodSet);

                        invoice.canReturn = false;
                        if (returnPeriod != 0)
                        {
                            DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-returnPeriod).ToShortDateString());
                            if (invoice.updateDate >= dt)
                                invoice.canReturn = true;
                        }
                        #endregion

                        #region get child invoice
                            //var invoice = invoice[i];

                            if (invoice.invType.Equals("s") || invoice.invType.Equals("p") || invoice.invType.Equals("pw"))
                            {
                                InvoiceModel childInvoice = new InvoiceModel();
                                while (childInvoice != null)
                                {
                                    childInvoice = GetChildInv(invoiceId, invoice.invType);
                                    if (childInvoice != null)
                                    {
                                        invoice.ChildInvoice = childInvoice;
                                        invoiceId = childInvoice.invoiceId;
                                    }

                                }
                                if (invoice.ChildInvoice != null)
                                    invoice.ChildInvoice.invoiceItems = await ic.Get(invoice.ChildInvoice.invoiceId);
                            }
                            #endregion
                    }

                    return TokenManager.GenerateToken(invoice);
                }
            }
        }

        [HttpPost]
        [Route("GetFullInvoice")]
        public async Task<string> GetFullInvoice(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
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

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var invoice = (from b in entity.invoices.Where(x => x.invoiceId == invoiceId)
                                   join l in entity.branches on b.branchId equals l.branchId into lj
                                   from x in lj.DefaultIfEmpty()
                                   select new InvoiceModel()
                                   {
                                       invoiceId = b.invoiceId,
                                       invNumber = b.invNumber,
                                       mainInvNumber = entity.invoices.Where(m => m.invoiceId == b.invoiceMainId).FirstOrDefault().invNumber,
                                       agentId = b.agentId,
                                       invType = b.invType,
                                       total = b.total,
                                       totalNet = b.totalNet,
                                       paid = b.paid,
                                       deserved = b.deserved,
                                       deservedDate = b.deservedDate,
                                       invDate = b.invDate,
                                       invoiceMainId = b.invoiceMainId,
                                       invCase = b.invCase,
                                       invTime = b.invTime,
                                       notes = b.notes,
                                       itemtax_note = b.itemtax_note,
                                       sales_invoice_note = b.sales_invoice_note,

                                       vendorInvNum = b.vendorInvNum,
                                       vendorInvDate = b.vendorInvDate,
                                       createUserId = b.createUserId,
                                       updateDate = b.updateDate,
                                       updateUserId = b.updateUserId,
                                       branchId = b.branchId,
                                       DBDiscountValue = b.discountValue,
                                       discountType = b.discountType,
                                       tax = b.tax,
                                       taxtype = b.taxtype,
                                       taxValue = b.taxValue,
                                       VATValue = b.VATValue,
                                       name = b.name,
                                       isApproved = b.isApproved,
                                       branchName = x.name,
                                       branchCreatorId = b.branchCreatorId,
                                       shippingCompanyId = b.shippingCompanyId,
                                       shipUserId = b.shipUserId,
                                       userId = b.userId,
                                       manualDiscountType = b.manualDiscountType,
                                       manualDiscountValue = b.manualDiscountValue,
                                       cashReturn = b.cashReturn,
                                       realShippingCost = b.realShippingCost,
                                       shippingCost = b.shippingCost,
                                       isOrginal = b.isOrginal,
                                       printedcount = b.printedcount,
                                       isPrePaid = b.isPrePaid,
                                       sliceId = b.sliceId,
                                       sliceName = b.sliceName,
                                       isFreeShip = b.isFreeShip,
                                       performed = (entity.invoices.Where(y => y.invoiceMainId == b.invoiceId).FirstOrDefault() == null) ? false : true,

                                   }).FirstOrDefault();


                    if (invoice != null)
                    {
                        ItemsTransferController ic = new ItemsTransferController();
                        AgentController ac = new AgentController();
                        CashTransferController cc = new CashTransferController();
                        ShippingCompaniesController sc = new ShippingCompaniesController();
                        UsersController uc = new UsersController();
                        BranchesController bc = new BranchesController();

                        invoice.invoiceItems = await ic.Get(invoice.invoiceId);
                        invoice.itemsCount = invoice.invoiceItems.Count;

                        if(invoice.agentId != null)
                            invoice.Agent = ac.GetAgentByID((int)invoice.agentId);

                          if(invoice.shippingCompanyId != null)
                            invoice.ShippingCompany = sc.GetByID((int)invoice.shippingCompanyId);

                        if (invoice.shipUserId != null)
                            invoice.DeliveryMan = uc.GetUserByID((int)invoice.shipUserId);

                        if (invoice.branchCreatorId != null)
                            invoice.FromBranch = bc.GetBranchByID((int)invoice.branchCreatorId);

                        if (invoice.branchId != null)
                            invoice.ToBranch = bc.GetBranchByID((int)invoice.branchId);
                        invoice.cachTrans = cc.GetPayedByInvId(invoiceId);
                        invoice.invoiceTaxes = GetInvoiceTaxes(invoiceId);
                        #region can return
                        var returnPeriodSet = entity.setValues.Where(x => x.setting.name == "returnPeriod").Select(x => x.value).SingleOrDefault();

                        int returnPeriod = 0;
                        if (returnPeriodSet != null)
                            returnPeriod = int.Parse(returnPeriodSet);

                        invoice.canReturn = false;
                        if (returnPeriod != 0)
                        {
                            DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-returnPeriod).ToShortDateString());
                            if (invoice.updateDate >= dt)
                                invoice.canReturn = true;
                        }
                        #endregion

                        #region get child invoice
                        //var invoice = invoice[i];

                        if (invoice.invType.Equals("s") || invoice.invType.Equals("p") || invoice.invType.Equals("pw"))
                        {
                            InvoiceModel childInvoice = new InvoiceModel();
                            while (childInvoice != null)
                            {
                                childInvoice = GetChildInv(invoiceId, invoice.invType);
                                if (childInvoice != null)
                                {
                                    invoice.ChildInvoice = childInvoice;
                                    invoiceId = childInvoice.invoiceId;
                                }

                            }
                            if (invoice.ChildInvoice != null)
                                invoice.ChildInvoice.invoiceItems = await ic.Get(invoice.ChildInvoice.invoiceId);
                        }
                        #endregion
                    }
                  
                    return TokenManager.GenerateToken(invoice);
                }
            }
        }
        [HttpPost]
        [Route("getgeneratedInvoice")]
        public string getgeneratedInvoice(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainInvoiceId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        mainInvoiceId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var banksList = entity.invoices.Where(b => b.invoiceMainId == mainInvoiceId).Select(b => new
                    {
                        b.invoiceId,
                        b.invNumber,
                        b.agentId,
                        b.invType,
                        b.total,
                        b.totalNet,
                        b.paid,
                        b.deserved,
                        b.deservedDate,
                        b.invDate,
                        b.invoiceMainId,
                        b.invCase,
                        b.invTime,
                        b.notes,
                        b.itemtax_note,
                        b.sales_invoice_note,

                        b.vendorInvNum,
                        b.vendorInvDate,
                        b.createUserId,
                        b.updateDate,
                        b.updateUserId,
                        b.branchId,
                        b.discountType,
                        b.discountValue,
                        b.tax,
                        b.taxtype,
                        b.VATValue,
                        b.name,
                        b.isApproved,
                        b.branchCreatorId,
                        b.shippingCompanyId,
                        b.shipUserId,
                        b.userId,
                        b.manualDiscountType,

                        b.manualDiscountValue,
                        b.realShippingCost,
                        b.shippingCost,
                        b.sliceId,
                        b.sliceName,
                        b.isFreeShip,
                    })
                    .FirstOrDefault();

                    return TokenManager.GenerateToken(banksList);
                }
            }
        }

        [HttpPost]
        [Route("getById")]
        public string GetById(string token)
        {
            var strP = TokenManager.GetPrincipal(token);
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
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var banksList = entity.invoices.Where(b => b.invoiceId == invoiceId).Select(b => new
                    {
                        b.invoiceId,
                        b.invNumber,
                        b.agentId,
                        b.invType,
                        b.total,
                        b.totalNet,
                        b.paid,
                        b.deserved,
                        b.deservedDate,
                        b.invDate,
                        b.invoiceMainId,
                        b.invCase,
                        b.invTime,
                        b.notes,
                        b.itemtax_note,
                        b.sales_invoice_note,

                        b.vendorInvNum,
                        b.vendorInvDate,
                        b.createUserId,
                        b.updateDate,
                        b.updateUserId,
                        b.branchId,
                        b.discountType,
                        b.discountValue,
                        b.tax,
                        b.taxtype,
                        b.taxValue,
                        b.VATValue,
                        b.name,
                        b.isApproved,
                        b.branchCreatorId,
                        b.shippingCompanyId,
                        b.shipUserId,
                        b.userId,
                        b.cashReturn,
                        b.manualDiscountType,

                        b.manualDiscountValue,
                        b.realShippingCost,
                        b.isFreeShip,
                        b.shippingCost,
                        b.sliceId,
                        b.sliceName,
                    })
                    .FirstOrDefault();

                    return TokenManager.GenerateToken(banksList);
                }
            }
        }
        [HttpPost]
        [Route("GetByInvNum")]
        public string GetByInvNum(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string invNum = "";
                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invNum")
                    {
                        invNum = c.Value;
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    if (branchId == 0)
                    {
                        var banksList = (from b in entity.invoices.Where(b => b.invNumber == invNum)
                                         join l in entity.branches on b.branchId equals l.branchId into lj
                                         from x in lj.DefaultIfEmpty()
                                         select new InvoiceModel()
                                         {
                                             invoiceId = b.invoiceId,
                                             invNumber = b.invNumber,
                                             agentId = b.agentId,
                                             invType = b.invType,
                                             total = b.total,
                                             totalNet = b.totalNet,
                                             paid = b.paid,
                                             deserved = b.deserved,
                                             deservedDate = b.deservedDate,
                                             invDate = b.invDate,
                                             invoiceMainId = b.invoiceMainId,
                                             invCase = b.invCase,
                                             invTime = b.invTime,
                                             notes = b.notes,
                                             itemtax_note = b.itemtax_note,
                                             sales_invoice_note = b.sales_invoice_note,


                                             vendorInvNum = b.vendorInvNum,
                                             vendorInvDate = b.vendorInvDate,
                                             createUserId = b.createUserId,
                                             updateDate = b.updateDate,
                                             updateUserId = b.updateUserId,
                                             branchId = b.branchId,
                                             DBDiscountValue = b.discountValue,
                                             discountType = b.discountType,
                                             tax = b.tax,
                                             taxtype = b.taxtype,
                                             VATValue = b.VATValue,
                                             name = b.name,
                                             isApproved = b.isApproved,
                                             branchName = x.name,
                                             branchCreatorId = b.branchCreatorId,
                                             shippingCompanyId = b.shippingCompanyId,
                                             shipUserId = b.shipUserId,
                                             userId = b.userId,
                                             manualDiscountType = b.manualDiscountType,
                                             manualDiscountValue = b.manualDiscountValue,
                                             realShippingCost = b.realShippingCost,
                                             shippingCost = b.shippingCost,
                                             sliceId = b.sliceId,
                                             sliceName = b.sliceName,
                                             isFreeShip = b.isFreeShip,

                                         })

                               .FirstOrDefault();
                        return TokenManager.GenerateToken(banksList);
                    }
                    else
                    {
                        var banksList = (from b in entity.invoices.Where(b => b.invNumber == invNum && b.branchId == branchId)
                                         join l in entity.branches on b.branchId equals l.branchId into lj
                                         from x in lj.DefaultIfEmpty()
                                         select new InvoiceModel()
                                         {
                                             invoiceId = b.invoiceId,
                                             invNumber = b.invNumber,
                                             agentId = b.agentId,
                                             invType = b.invType,
                                             total = b.total,
                                             totalNet = b.totalNet,
                                             paid = b.paid,
                                             deserved = b.deserved,
                                             deservedDate = b.deservedDate,
                                             invDate = b.invDate,
                                             invoiceMainId = b.invoiceMainId,
                                             invCase = b.invCase,
                                             invTime = b.invTime,
                                             notes = b.notes,
                                             itemtax_note = b.itemtax_note,
                                             sales_invoice_note = b.sales_invoice_note,

                                             vendorInvNum = b.vendorInvNum,
                                             vendorInvDate = b.vendorInvDate,
                                             createUserId = b.createUserId,
                                             updateDate = b.updateDate,
                                             updateUserId = b.updateUserId,
                                             branchId = b.branchId,
                                             DBDiscountValue = b.discountValue,
                                             discountType = b.discountType,
                                             tax = b.tax,
                                             taxtype = b.taxtype,
                                             VATValue = b.VATValue,
                                             name = b.name,
                                             isApproved = b.isApproved,
                                             branchName = x.name,
                                             branchCreatorId = b.branchCreatorId,
                                             shippingCompanyId = b.shippingCompanyId,
                                             shipUserId = b.shipUserId,
                                             userId = b.userId,
                                             manualDiscountType = b.manualDiscountType,
                                             manualDiscountValue = b.manualDiscountValue,
                                             realShippingCost = b.realShippingCost,
                                             shippingCost = b.shippingCost,
                                             sliceId = b.sliceId,
                                             sliceName = b.sliceName,
                                             isFreeShip = b.isFreeShip,

                                         })

                               .FirstOrDefault();
                        return TokenManager.GenerateToken(banksList);
                    }
                }
            }
        }
        [HttpPost]
        [Route("GetInvoicesByBarcodeAndUser")]
        public string GetInvoicesByBarcodeAndUser(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                BranchesController bc = new BranchesController();
                string invNum = "";
                int branchId = 0;
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invNum")
                    {
                        invNum = c.Value;
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    #region get inv type from invNum
                    int codeindex = invNum.IndexOf("-");
                    string prefix = "";
                    if (codeindex >= 0)
                        prefix = invNum.Substring(0, codeindex);
                    prefix = prefix.ToLower();

                    int returnPeriod = 0;
                    if (prefix == "si")
                    {
                        var returnPeriodSet = entity.setValues.Where(x => x.setting.name == "returnPeriod").Select(x => x.value).SingleOrDefault();

                        if (returnPeriodSet != null)
                            returnPeriod = int.Parse(returnPeriodSet);
                    }
                    #endregion
                    //get user branches permission
                    var branches = bc.BrListByBranchandUser(branchId, userId);
                    List<int> branchesIds = new List<int>();
                    for (int i = 0; i < branches.Count; i++)
                        branchesIds.Add(branches[i].branchId);

                    var invoicesList = (from b in entity.invoices.Where(b => b.invNumber == invNum
                                   && b.isActive == true
                                   && branchesIds.Contains((int)b.branchId))
                                   join l in entity.branches on b.branchId equals l.branchId into lj
                                   from x in lj.DefaultIfEmpty()
                                   select new InvoiceModel()
                                   {
                                       invoiceId = b.invoiceId,
                                       invNumber = b.invNumber,
                                       agentId = b.agentId,
                                       invType = b.invType,
                                       total = b.total,
                                       totalNet = b.totalNet,
                                       paid = b.paid,
                                       deserved = b.deserved,
                                       deservedDate = b.deservedDate,
                                       invDate = b.invDate,
                                       invoiceMainId = b.invoiceMainId,
                                       invCase = b.invCase,
                                       invTime = b.invTime,
                                       notes = b.notes,
                                       itemtax_note = b.itemtax_note,
                                       sales_invoice_note = b.sales_invoice_note,

                                       vendorInvNum = b.vendorInvNum,
                                       vendorInvDate = b.vendorInvDate,
                                       createUserId = b.createUserId,
                                       updateDate = b.updateDate,
                                       updateUserId = b.updateUserId,
                                       branchId = b.branchId,
                                       DBDiscountValue = b.discountValue,
                                       discountType = b.discountType,
                                       tax = b.tax,
                                       taxtype = b.taxtype,
                                       taxValue=b.taxValue,
                                       VATValue = b.VATValue,
                                       name = b.name,
                                       isApproved = b.isApproved,
                                       branchName = x.name,
                                       branchCreatorId = b.branchCreatorId,
                                       shippingCompanyId = b.shippingCompanyId,
                                       shipUserId = b.shipUserId,
                                       userId = b.userId,
                                       manualDiscountType = b.manualDiscountType,
                                       manualDiscountValue = b.manualDiscountValue,
                                       realShippingCost = b.realShippingCost,
                                       shippingCost = b.shippingCost,
                                       sliceId = b.sliceId,
                                       sliceName = b.sliceName,
                                       isFreeShip = b.isFreeShip,

                                   }).ToList();

                    var invoice =  invoicesList.Where(inv => inv.invoiceId == invoicesList.Where(i => i.invNumber == inv.invNumber).ToList().OrderByDescending(i => i.invoiceId).FirstOrDefault().invoiceId).FirstOrDefault();

                    #region can return
                    if (prefix =="si")
                    {                   
                        invoice.canReturn = false;
                        if (returnPeriod != 0)
                        {
                            DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-returnPeriod).ToShortDateString());
                            if (invoice.updateDate >= dt)
                                invoice.canReturn = true;
                        }
                       
                    }
                    #endregion
                    return TokenManager.GenerateToken(invoice);
                }
            }
        }
        [HttpPost]
        [Route("getInvoiceByNumAndUser")]
        public string getInvoiceByNumAndUser(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                BranchesController bc = new BranchesController();
                string vendorInvNum = "";
                string invType = "";
                int branchId = 0;
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invNum")
                    {
                        vendorInvNum = c.Value;
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "invType")
                    {
                        invType = c.Value;
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    #region get inv type from invNum
                    int returnPeriod = 0;
                    if (invType == "s")
                    {
                        var returnPeriodSet = entity.setValues.Where(x => x.setting.name == "returnPeriod").Select(x => x.value).SingleOrDefault();

                        if (returnPeriodSet != null)
                            returnPeriod = int.Parse(returnPeriodSet);
                    }
                    #endregion
                    //get user branches permission
                    var branches = bc.BrListByBranchandUser(branchId, userId);
                    List<int> branchesIds = new List<int>();
                    for (int i = 0; i < branches.Count; i++)
                        branchesIds.Add(branches[i].branchId);

                    var invoicesList = (from b in entity.invoices.Where(b => b.vendorInvNum == vendorInvNum 
                                   && b.isActive == true
                                   && branchesIds.Contains((int)b.branchId) && b.invType == invType)
                                   join l in entity.branches on b.branchId equals l.branchId into lj
                                   from x in lj.DefaultIfEmpty()
                                   select new InvoiceModel()
                                   {
                                       invoiceId = b.invoiceId,
                                       invNumber = b.invNumber,
                                       agentId = b.agentId,
                                       invType = b.invType,
                                       total = b.total,
                                       totalNet = b.totalNet,
                                       paid = b.paid,
                                       deserved = b.deserved,
                                       deservedDate = b.deservedDate,
                                       invDate = b.invDate,
                                       invoiceMainId = b.invoiceMainId,
                                       invCase = b.invCase,
                                       invTime = b.invTime,
                                       notes = b.notes,
                                       itemtax_note = b.itemtax_note,
                                       sales_invoice_note = b.sales_invoice_note,

                                       vendorInvNum = b.vendorInvNum,
                                       vendorInvDate = b.vendorInvDate,
                                       createUserId = b.createUserId,
                                       updateDate = b.updateDate,
                                       updateUserId = b.updateUserId,
                                       branchId = b.branchId,
                                       DBDiscountValue = b.discountValue,
                                       discountType = b.discountType,
                                       tax = b.tax,
                                       taxtype = b.taxtype,
                                       taxValue = b.taxValue,
                                       VATValue = b.VATValue,
                                       name = b.name,
                                       isApproved = b.isApproved,
                                       branchName = x.name,
                                       branchCreatorId = b.branchCreatorId,
                                       shippingCompanyId = b.shippingCompanyId,
                                       shipUserId = b.shipUserId,
                                       userId = b.userId,
                                       manualDiscountType = b.manualDiscountType,
                                       manualDiscountValue = b.manualDiscountValue,
                                       realShippingCost = b.realShippingCost,
                                       shippingCost = b.shippingCost,
                                       sliceId = b.sliceId,
                                       sliceName = b.sliceName,
                                       isFreeShip = b.isFreeShip,

                                   }).ToList();

                    var invoice = invoicesList.Where(inv => inv.invoiceId == invoicesList.Where(i => i.invNumber == inv.invNumber).ToList().OrderByDescending(i => i.invoiceId).FirstOrDefault().invoiceId).FirstOrDefault();
                    #region can return
                    if (invType == "s")
                    {
                        invoice.canReturn = false;
                        if (returnPeriod != 0)
                        {
                            DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-returnPeriod).ToShortDateString());
                            if (invoice.updateDate >= dt)
                                invoice.canReturn = true;
                        }

                    }
                    #endregion
                    return TokenManager.GenerateToken(invoice);
                }
            }
        }
        [HttpPost]
        [Route("GetByInvoiceType")]
        public async Task<string> GetByInvoiceType(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                ItemsTransferController ic = new ItemsTransferController();
                CashTransferController cc = new CashTransferController();
                string invType = "";
                int branchId = 0;
                List<string> invTypeL = new List<string>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }



                using (incposdbEntities entity = new incposdbEntities())
                {
                    if (branchId == 0)
                    {
                        var invoicesList = (from b in entity.invoices.Where(x => invTypeL.Contains(x.invType))
                                            join l in entity.branches on b.branchId equals l.branchId into lj
                                            from x in lj.DefaultIfEmpty()
                                            select new InvoiceModel()
                                            {
                                                invoiceId = b.invoiceId,
                                                invNumber = b.invNumber,
                                                agentId = b.agentId,
                                                invType = b.invType,
                                                total = b.total,
                                                totalNet = b.totalNet,
                                                paid = b.paid,
                                                deserved = b.deserved,
                                                deservedDate = b.deservedDate,
                                                invDate = b.invDate,
                                                invoiceMainId = b.invoiceMainId,
                                                invCase = b.invCase,
                                                invTime = b.invTime,
                                                notes = b.notes,
                                                itemtax_note = b.itemtax_note,
                                                sales_invoice_note = b.sales_invoice_note,

                                                vendorInvNum = b.vendorInvNum,
                                                vendorInvDate = b.vendorInvDate,
                                                createUserId = b.createUserId,
                                                updateDate = b.updateDate,
                                                updateUserId = b.updateUserId,
                                                branchId = b.branchId,
                                                DBDiscountValue = b.discountValue,
                                                discountType = b.discountType,
                                                tax = b.tax,
                                                taxtype = b.taxtype,
                                                taxValue = b.taxValue,
                                                VATValue = b.VATValue,
                                                name = b.name,
                                                isApproved = b.isApproved,
                                                branchName = x.name,
                                                branchCreatorId = b.branchCreatorId,
                                                shippingCompanyId = b.shippingCompanyId,
                                                shipUserId = b.shipUserId,
                                                userId = b.userId,
                                                manualDiscountType = b.manualDiscountType,
                                                manualDiscountValue = b.manualDiscountValue,
                                                cashReturn = b.cashReturn,
                                                realShippingCost = b.realShippingCost,
                                                shippingCost = b.shippingCost,
                                                isOrginal = b.isOrginal,
                                                printedcount = b.printedcount,
                                                sliceId = b.sliceId,
                                                sliceName = b.sliceName,
                                                isFreeShip = b.isFreeShip,

                                            })
                        .ToList();
                        if (invoicesList != null)
                        {
                            for (int i = 0; i < invoicesList.Count; i++)
                            {
                                int invoiceId = invoicesList[i].invoiceId;
                                invoicesList[i].invoiceItems = await ic.Get(invoiceId);
                                invoicesList[i].itemsCount = invoicesList[i].invoiceItems.Count;
                                invoicesList[i].cachTrans = cc.GetPayedByInvId(invoiceId);
                            }
                        }

                        return TokenManager.GenerateToken(invoicesList);
                    }
                    else
                    {
                        var invoicesList = (from b in entity.invoices.Where(x => invTypeL.Contains(x.invType) && x.branchId == branchId)
                                            join l in entity.branches on b.branchId equals l.branchId into lj
                                            from x in lj.DefaultIfEmpty()
                                            select new InvoiceModel()
                                            {
                                                invoiceId = b.invoiceId,
                                                invNumber = b.invNumber,
                                                agentId = b.agentId,
                                                invType = b.invType,
                                                total = b.total,
                                                totalNet = b.totalNet,
                                                paid = b.paid,
                                                deserved = b.deserved,
                                                deservedDate = b.deservedDate,
                                                invDate = b.invDate,
                                                invoiceMainId = b.invoiceMainId,
                                                invCase = b.invCase,
                                                invTime = b.invTime,
                                                notes = b.notes,
                                                itemtax_note = b.itemtax_note,
                                                sales_invoice_note = b.sales_invoice_note,

                                                vendorInvNum = b.vendorInvNum,
                                                vendorInvDate = b.vendorInvDate,
                                                createUserId = b.createUserId,
                                                updateDate = b.updateDate,
                                                updateUserId = b.updateUserId,
                                                branchId = b.branchId,
                                                DBDiscountValue = b.discountValue,
                                                discountType = b.discountType,
                                                tax = b.tax,
                                                taxtype = b.taxtype,
                                                VATValue = b.VATValue,
                                                name = b.name,
                                                isApproved = b.isApproved,
                                                branchName = x.name,
                                                branchCreatorId = b.branchCreatorId,
                                                shippingCompanyId = b.shippingCompanyId,
                                                shipUserId = b.shipUserId,
                                                userId = b.userId,
                                                manualDiscountType = b.manualDiscountType,
                                                manualDiscountValue = b.manualDiscountValue,
                                                realShippingCost = b.realShippingCost,
                                                shippingCost = b.shippingCost,
                                                isOrginal = b.isOrginal,
                                                printedcount = b.printedcount,
                                                sliceId = b.sliceId,
                                                sliceName = b.sliceName,
                                                isFreeShip = b.isFreeShip,

                                            })
                        .ToList();
                        if (invoicesList != null)
                        {
                            for (int i = 0; i < invoicesList.Count; i++)
                            {
                                int invoiceId = invoicesList[i].invoiceId;
                                invoicesList[i].invoiceItems = await ic.Get(invoiceId);
                                invoicesList[i].itemsCount = invoicesList[i].invoiceItems.Count;
                                invoicesList[i].cachTrans = cc.GetPayedByInvId(invoiceId);
                            }
                        }

                        return TokenManager.GenerateToken(invoicesList);
                    }
                }
            }
        }
        [HttpPost]
        [Route("GetItemUnitOrders")]
        public async Task<string> GetItemUnitOrders(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                ItemsTransferController ic = new ItemsTransferController();
                CashTransferController cc = new CashTransferController();
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



                using (incposdbEntities entity = new incposdbEntities())
                {

                    var invoicesList = (from b in entity.invoices
                                        .Where(x =>(( x.invType == "po" &&  !entity.invoices.Any(y => y.invoiceMainId == x.invoiceId))|| (x.invType == "exw"))
                                                    && x.isActive == true && x.branchCreatorId == branchId)
                                        join l in entity.itemsTransfer.Where(i => i.itemUnitId == itemUnitId) on b.invoiceId equals l.invoiceId 
                                        select new ItemTransferModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            quantity = l.quantity,
                                            invType = b.invType,
                                               
                                        })
                    .ToList();

                    return TokenManager.GenerateToken(invoicesList);
                   
                }
            }
        }
        [HttpPost]
        [Route("GetInvoicesByCreator")]
        public async Task<string> GetInvoicesByCreator(string token)
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
                CashTransferController cc = new CashTransferController();
                ItemsTransferController ic = new ItemsTransferController();
                string invType = "";
                int createUserId = 0;
                int duration = 0;
                List<string> invTypeL = new List<string>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "createUserId")
                    {
                        createUserId = int.Parse(c.Value);
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                }
                #endregion

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var searchPredicate = PredicateBuilder.New<invoices>();

                    if (duration > 0)
                    {
                        DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-duration).ToShortDateString());
                        searchPredicate = searchPredicate.And(inv => inv.updateDate >= dt);
                    }
                    searchPredicate = searchPredicate.And(inv => invTypeL.Contains(inv.invType));
                    searchPredicate = searchPredicate.And(inv => inv.createUserId == createUserId);
                    searchPredicate = searchPredicate.And(inv => inv.isActive == true);

                    var invoicesList = (from b in entity.invoices.Where(searchPredicate)
                                        join l in entity.branches on b.branchId equals l.branchId into lj
                                        from x in lj.DefaultIfEmpty()
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            mainInvNumber = entity.invoices.Where(m => m.invoiceId == b.invoiceMainId).FirstOrDefault().invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            taxValue = b.taxValue,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchName = x.name,
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            userId = b.userId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            cashReturn = b.cashReturn,
                                            realShippingCost = b.realShippingCost,
                                            shippingCost = b.shippingCost,
                                            isOrginal = b.isOrginal,
                                            printedcount = b.printedcount,
                                            isPrePaid = b.isPrePaid,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,

                                        }).ToList();

                    invoicesList = invoicesList.Where(inv => inv.invoiceId == invoicesList.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId).ToList();

                    if (invoicesList != null)
                    {
                        for (int i = 0; i < invoicesList.Count; i++)
                        {
                            int invoiceId = invoicesList[i].invoiceId;
                            invoicesList[i].invoiceItems = await ic.Get(invoiceId);
                            invoicesList[i].itemsCount = invoicesList[i].invoiceItems.Count;
                            invoicesList[i].cachTrans = cc.GetPayedByInvId(invoiceId);

                            #region get child invoice
                            var invoice = invoicesList[i];

                            if (invoice.invType.Equals("p"))
                            {
                                InvoiceModel childInvoice = new InvoiceModel();
                                while (childInvoice != null)
                                {
                                    childInvoice = GetChildInv(invoiceId, invoice.invType);
                                    if (childInvoice != null)
                                    {
                                        invoicesList[i].ChildInvoice = childInvoice;
                                        invoiceId = childInvoice.invoiceId;
                                    }

                                }
                                if (invoicesList[i].ChildInvoice != null)
                                    invoicesList[i].ChildInvoice.invoiceItems = await ic.Get(invoicesList[i].ChildInvoice.invoiceId);
                            }
                            #endregion
                        }
                    }

                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }
        [HttpPost]
        [Route("GetInvoicesForAdmin")]
        public async Task<string> GetInvoicesForAdmin(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                ItemsTransferController ic = new ItemsTransferController();
                CashTransferController cc = new CashTransferController();
                string invType = "";
                int duration = 0;
                List<string> invTypeL = new List<string>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                }

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var searchPredicate = PredicateBuilder.New<invoices>();

                    if (duration > 0)
                    {
                        DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-duration).ToShortDateString());
                        searchPredicate = searchPredicate.And(inv => inv.updateDate >= dt);
                    }
                    searchPredicate = searchPredicate.And(inv => invTypeL.Contains(inv.invType));
                    searchPredicate = searchPredicate.And(inv => inv.isActive == true);

                    var invoicesList = (from b in entity.invoices.Where(searchPredicate)
                                        join l in entity.branches on b.branchId equals l.branchId into lj
                                        from x in lj.DefaultIfEmpty()
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            taxValue = b.taxValue,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchName = x.name,
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            userId = b.userId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            cashReturn = b.cashReturn,
                                            realShippingCost = b.realShippingCost,
                                            shippingCost = b.shippingCost,
                                            isOrginal = b.isOrginal,
                                            printedcount = b.printedcount,
                                            isPrePaid = b.isPrePaid,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,

                                        })
                    .ToList();

                    if (invoicesList != null)
                    {
                        for (int i = 0; i < invoicesList.Count; i++)
                        {
                            int invoiceId = invoicesList[i].invoiceId;

                            invoicesList[i].invoiceItems = await ic.Get(invoiceId);
                            invoicesList[i].itemsCount = invoicesList[i].invoiceItems.Count;
                            invoicesList[i].cachTrans = cc.GetPayedByInvId(invoiceId);
                            invoicesList[i].invoiceTaxes = GetInvoiceTaxes(invoiceId);
                        }
                    }

                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }
        [HttpPost]
        [Route("GetSalesInvoices")]
        public async Task<string> GetSalesInvoices(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                ItemsTransferController ic = new ItemsTransferController();
                CashTransferController cc = new CashTransferController();
                string invType = "";
                int duration = 0;
                int userId = 0;
                bool isAdmin = false;
                List<string> invTypeL = new List<string>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "isAdmin")
                    {
                        isAdmin = bool.Parse(c.Value);
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var returnPeriodSet = entity.setValues.Where(x => x.setting.name == "returnPeriod").Select(x => x.value).SingleOrDefault();

                    int returnPeriod = 0;
                    if (returnPeriodSet != null)
                        returnPeriod = int.Parse(returnPeriodSet);

                    var searchPredicate = PredicateBuilder.New<invoices>();

                    if (duration > 0)
                    {
                        DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-duration).ToShortDateString());
                        searchPredicate = searchPredicate.And(inv => inv.updateDate >= dt);
                    }
                    if (isAdmin == false)
                        searchPredicate = searchPredicate.And(inv => inv.createUserId == userId);

                    searchPredicate = searchPredicate.And(inv => invTypeL.Contains(inv.invType));
                    searchPredicate = searchPredicate.And(inv => inv.isActive == true);

                    var invoicesList = (from b in entity.invoices.Where(searchPredicate)
                                        join l in entity.branches on b.branchId equals l.branchId into lj
                                        from x in lj.DefaultIfEmpty()
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            mainInvNumber = entity.invoices.Where( m => m.invoiceId == b.invoiceMainId).FirstOrDefault().invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            taxValue = b.taxValue,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchName = x.name,
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            userId = b.userId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            cashReturn = b.cashReturn,
                                            realShippingCost = b.realShippingCost,
                                            shippingCost = b.shippingCost,
                                            isOrginal = b.isOrginal,
                                            printedcount = b.printedcount,
                                            isPrePaid = b.isPrePaid,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,

                                        })
                    .ToList();

                    invoicesList = invoicesList.Where(inv => inv.invoiceId == invoicesList.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId).ToList();

                    if (invoicesList != null)
                    {
                        for (int i = 0; i < invoicesList.Count; i++)
                        {
                            int invoiceId = invoicesList[i].invoiceId;

                            invoicesList[i].invoiceItems = await ic.Get(invoiceId);
                            invoicesList[i].itemsCount = invoicesList[i].invoiceItems.Count;
                            invoicesList[i].cachTrans = cc.GetPayedByInvId(invoiceId);
                            invoicesList[i].invoiceTaxes = GetInvoiceTaxes(invoiceId);

                            #region can return
                           
                            invoicesList[i].canReturn = false;
                            if (returnPeriod != 0)
                            {
                                DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-returnPeriod).ToShortDateString());
                                if(invoicesList[i].updateDate >= dt)
                                    invoicesList[i].canReturn = true;
                            }
                            #endregion
                            #region get child invoice
                            var invoice = invoicesList[i];
                            
                            if (invoice.invType.Equals("s"))
                            {
                                InvoiceModel childInvoice = new InvoiceModel();
                                while (childInvoice != null)
                                {
                                    childInvoice = GetChildInv(invoiceId, invoice.invType);
                                    if (childInvoice != null)
                                    {
                                        invoicesList[i].ChildInvoice = childInvoice;
                                        invoiceId = childInvoice.invoiceId;
                                    }

                                }
                                if (invoicesList[i].ChildInvoice != null)
                                    invoicesList[i].ChildInvoice.invoiceItems = await ic.Get(invoicesList[i].ChildInvoice.invoiceId);
                            }
                            #endregion
                        }
                    }

                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }

        public List<InvoiceTaxesModel> GetInvoiceTaxes(int invoiceId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var invoiceTaxes = entity.invoiceTaxes.Where(x => x.invoiceId == invoiceId)
                    .Select(x => new InvoiceTaxesModel() {
                        invoiceId = x.invoiceId,
                        notes = x.notes,
                        rate = x.rate,
                        taxId = x.taxId,
                        taxType = x.taxType,
                        taxValue = x.taxValue,
                        name = entity.taxes.Where(m => m.taxId == x.taxId).Select(m => m.name).FirstOrDefault(),
                        nameAr = entity.taxes.Where(m => m.taxId == x.taxId).Select(m => m.nameAr).FirstOrDefault(),
                    }).ToList();

                return invoiceTaxes;
            }
        }
        public InvoiceModel GetParentInv(int invoiceId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var invoice = (from b in entity.invoices.Where(x => x.invoiceId == entity.invoices.Where(m => m.invoiceId == invoiceId).Select(m => m.invoiceMainId).FirstOrDefault())
                               join l in entity.branches on b.branchId equals l.branchId into lj
                               from x in lj.DefaultIfEmpty()
                               where b.isActive == true
                               select new InvoiceModel()
                               {
                                   invoiceId = b.invoiceId,
                                   invNumber = b.invNumber,
                                   agentId = b.agentId,
                                   invType = b.invType,
                                   total = b.total,
                                   totalNet = b.totalNet,
                                   paid = b.paid,
                                   deserved = b.deserved,
                                   deservedDate = b.deservedDate,
                                   invDate = b.invDate,
                                   invoiceMainId = b.invoiceMainId,
                                   invCase = b.invCase,
                                   invTime = b.invTime,
                                   notes = b.notes,
                                   itemtax_note = b.itemtax_note,
                                   sales_invoice_note = b.sales_invoice_note,

                                   vendorInvNum = b.vendorInvNum,
                                   vendorInvDate = b.vendorInvDate,
                                   createUserId = b.createUserId,
                                   updateDate = b.updateDate,
                                   updateUserId = b.updateUserId,
                                   branchId = b.branchId,
                                   DBDiscountValue = b.discountValue,
                                   discountType = b.discountType,
                                   tax = b.tax,
                                   taxtype = b.taxtype,
                                   VATValue = b.VATValue,
                                   name = b.name,
                                   isApproved = b.isApproved,
                                   branchName = x.name,
                                   branchCreatorId = b.branchCreatorId,
                                   shippingCompanyId = b.shippingCompanyId,
                                   shipUserId = b.shipUserId,
                                   userId = b.userId,
                                   manualDiscountType = b.manualDiscountType,
                                   manualDiscountValue = b.manualDiscountValue,
                                   cashReturn = b.cashReturn,
                                   realShippingCost = b.realShippingCost,
                                   shippingCost = b.shippingCost,
                                   isOrginal = b.isOrginal,
                                   printedcount = b.printedcount,
                                   isPrePaid = b.isPrePaid,
                                   isArchived = true,
                                   sliceId = b.sliceId,
                                   sliceName = b.sliceName,
                                   isFreeShip = b.isFreeShip,

                               }).FirstOrDefault();
                return invoice;
            }
        }
        public InvoiceModel GetChildInv(int invoiceId, string type)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var invoice = (from b in entity.invoices.Where(x => x.invoiceMainId == invoiceId && x.invType == type)
                               join l in entity.branches on b.branchId equals l.branchId into lj
                               from x in lj.DefaultIfEmpty()
                               where b.isActive == true
                               select new InvoiceModel()
                               {
                                   invoiceId = b.invoiceId,
                                   invNumber = b.invNumber,
                                   agentId = b.agentId,
                                   invType = b.invType,
                                   total = b.total,
                                   totalNet = b.totalNet,
                                   paid = b.paid,
                                   deserved = b.deserved,
                                   deservedDate = b.deservedDate,
                                   invDate = b.invDate,
                                   invoiceMainId = b.invoiceMainId,
                                   invCase = b.invCase,
                                   invTime = b.invTime,
                                   notes = b.notes,
                                   itemtax_note = b.itemtax_note,
                                   sales_invoice_note = b.sales_invoice_note,

                                   vendorInvNum = b.vendorInvNum,
                                   vendorInvDate = b.vendorInvDate,
                                   createUserId = b.createUserId,
                                   updateDate = b.updateDate,
                                   updateUserId = b.updateUserId,
                                   branchId = b.branchId,
                                   DBDiscountValue = b.discountValue,
                                   discountType = b.discountType,
                                   tax = b.tax,
                                   taxtype = b.taxtype,
                                   VATValue = b.VATValue,
                                   name = b.name,
                                   isApproved = b.isApproved,
                                   branchName = x.name,
                                   branchCreatorId = b.branchCreatorId,
                                   shippingCompanyId = b.shippingCompanyId,
                                   shipUserId = b.shipUserId,
                                   userId = b.userId,
                                   manualDiscountType = b.manualDiscountType,
                                   manualDiscountValue = b.manualDiscountValue,
                                   cashReturn = b.cashReturn,
                                   realShippingCost = b.realShippingCost,
                                   shippingCost = b.shippingCost,
                                   isOrginal = b.isOrginal,
                                   printedcount = b.printedcount,
                                   isPrePaid = b.isPrePaid,
                                   isArchived = true,
                                   sliceId = b.sliceId,
                                   sliceName = b.sliceName,
                                   isFreeShip = b.isFreeShip,

                               }).FirstOrDefault();

                return invoice;
            }
        }
         [HttpPost]
        [Route("GetInvoiceArchive")]
        public async Task<string> GetInvoiceArchive(string token)
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
                int invoiceId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                   if (c.Type == "invoiceId")
                    {
                        invoiceId = int.Parse(c.Value);
                    }
                }
                #endregion
                using (incposdbEntities entity = new incposdbEntities())
                {
                    ItemsTransferController ic = new ItemsTransferController();
                    CashTransferController cc = new CashTransferController();
                    List<InvoiceModel> invoicesList = new List<InvoiceModel>();

                    var inv = entity.invoices.Find(invoiceId);
                    InvoiceModel retInv = new InvoiceModel();
                    if (inv.invType.Equals("s") || inv.invType.Equals("p") || inv.invType.Equals("bw"))// get returns count
                    {
                        while (retInv != null)
                        {
                            retInv = (from b in entity.invoices.Where(x => x.isActive == true
                                                                && x.invoiceMainId == invoiceId
                                                                && (x.invType == "sb" || x.invType == "pb" || x.invType == "pbw"))
                                      join x in entity.branches on b.branchId equals x.branchId
                                      select new InvoiceModel()
                                      {
                                          invoiceId = b.invoiceId,
                                          invNumber = b.invNumber,
                                          agentId = b.agentId,
                                          invType = b.invType,
                                          total = b.total,
                                          totalNet = b.totalNet,
                                          paid = b.paid,
                                          deserved = b.deserved,
                                          deservedDate = b.deservedDate,
                                          invDate = b.invDate,
                                          invoiceMainId = b.invoiceMainId,
                                          invCase = b.invCase,
                                          invTime = b.invTime,
                                          notes = b.notes,
                                          itemtax_note = b.itemtax_note,
                                          sales_invoice_note = b.sales_invoice_note,

                                          vendorInvNum = b.vendorInvNum,
                                          vendorInvDate = b.vendorInvDate,
                                          createUserId = b.createUserId,
                                          updateDate = b.updateDate,
                                          updateUserId = b.updateUserId,
                                          branchId = b.branchId,
                                          DBDiscountValue = b.discountValue,
                                          discountType = b.discountType,
                                          tax = b.tax,
                                          taxtype = b.taxtype,
                                          taxValue = b.taxValue,
                                          VATValue = b.VATValue,
                                          name = b.name,
                                          isApproved = b.isApproved,
                                          branchName = x.name,
                                          branchCreatorId = b.branchCreatorId,
                                          shippingCompanyId = b.shippingCompanyId,
                                          shipUserId = b.shipUserId,
                                          userId = b.userId,
                                          manualDiscountType = b.manualDiscountType,
                                          manualDiscountValue = b.manualDiscountValue,
                                          cashReturn = b.cashReturn,
                                          realShippingCost = b.realShippingCost,
                                          shippingCost = b.shippingCost,
                                          isOrginal = b.isOrginal,
                                          printedcount = b.printedcount,
                                          isPrePaid = b.isPrePaid,
                                          isArchived = true,
                                          sliceId = b.sliceId,
                                          sliceName = b.sliceName,
                                          isFreeShip = b.isFreeShip,

                                      }).FirstOrDefault();

                            if (retInv != null)
                            {
                                invoicesList.Add(retInv);
                                invoiceId = entity.invoices.Where(x => x.isActive == true
                                                                && (x.invType == "s" || x.invType == "p")
                                                                && x.invoiceMainId == invoiceId).Select(x => x.invoiceId).FirstOrDefault();
                            }
                        }
                    }
                    else
                    {
                        var invoice = GetParentInv(invoiceId);

                        while (invoice != null)
                        {
                            invoicesList = new List<InvoiceModel>();
                           
                            invoicesList.Add(invoice);
                            invoice = GetParentInv(invoice.invoiceId);
                        }
                    }

                   foreach (var inv1 in invoicesList)
                    {
                        inv1.invoiceItems = await ic.Get(inv1.invoiceId);
                        inv1.itemsCount = inv1.invoiceItems.Count;
                        #region get child invoice
                        InvoiceModel childInvoice = new InvoiceModel();
                        invoiceId = inv1.invoiceId;
                        while (childInvoice != null)
                        {
                            childInvoice = GetChildInv(invoiceId, inv1.invType);
                            if (childInvoice != null)
                            {
                                inv1.ChildInvoice = childInvoice;
                                invoiceId = childInvoice.invoiceId;
                            }

                        }
                        if (inv1.ChildInvoice != null)
                            inv1.ChildInvoice.invoiceItems = await ic.Get(inv1.ChildInvoice.invoiceId);

                        #endregion
                    }
                    invoicesList = invoicesList.OrderBy(x => x.invoiceId).ToList();
                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }

        private int GetInvoiceArchiveCount(int invoiceId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                ItemsTransferController ic = new ItemsTransferController();
                CashTransferController cc = new CashTransferController();
                List<InvoiceModel> invoicesList = new List<InvoiceModel>();

               

                var inv = entity.invoices.Find(invoiceId);
                InvoiceModel retInv = new InvoiceModel();
                if(inv.invType.Equals("s") || inv.invType.Equals("p") || inv.invType.Equals("bw"))// get returns count
                {
                    while (retInv != null)
                    {
                        retInv = (from b in entity.invoices.Where(x => x.isActive == true
                                                            && x.invoiceMainId == invoiceId
                                                            && (x.invType == "sb" || x.invType == "pb" || x.invType == "pbw"))
                            join x in entity.branches on b.branchId equals x.branchId
                                  select  new InvoiceModel()
                                  {
                                      invoiceId = b.invoiceId,
                                      invNumber = b.invNumber,
                                      agentId = b.agentId,
                                  }).FirstOrDefault();

                        if (retInv != null)
                        {
                            invoicesList.Add(retInv);
                            invoiceId = entity.invoices.Where(x => x.isActive == true
                                                            && (x.invType == "s" || x.invType == "p")
                                                            && x.invoiceMainId == invoiceId).Select(x => x.invoiceId).FirstOrDefault();
                        }
                    }
                }
                else
                {
                    var invoice = GetParentInv(invoiceId);

                    while (invoice != null)
                    {
                        invoicesList = new List<InvoiceModel>();
                        invoicesList.Add(invoice);
                        invoice = GetParentInv(invoice.invoiceId);
                    }
                }
                return invoicesList.Count();
            }
        }
        [HttpPost]
        [Route("GetCountInvoicesForAdmin")]
        public string GetCountInvoicesForAdmin(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string invType = "";
                int duration = 0;
                List<string> invTypeL = new List<string>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                }

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var searchPredicate = PredicateBuilder.New<invoices>();

                    if (duration > 0)
                    {
                        DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-duration).ToShortDateString());
                        searchPredicate = searchPredicate.And(inv => inv.updateDate >= dt);
                    }
                    searchPredicate = searchPredicate.And(inv => invTypeL.Contains(inv.invType));
                    searchPredicate = searchPredicate.And(inv => inv.isActive == true);

                    var invoicesCount = (from b in entity.invoices.Where(searchPredicate)
                                         join l in entity.branches on b.branchId equals l.branchId into lj
                                         from x in lj.DefaultIfEmpty()
                                         select new InvoiceModel()
                                         {
                                             invoiceId = b.invoiceId,
                                         })
                    .ToList().Count;
                    return TokenManager.GenerateToken(invoicesCount);
                }
            }
        }
        private int GetCountInvoicesForAdmin(List<string> invTypeL, int duration)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var searchPredicate = PredicateBuilder.New<invoices>();

                if (duration > 0)
                {
                    DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-duration).ToShortDateString());
                    searchPredicate = searchPredicate.And(inv => inv.updateDate >= dt);
                }
                searchPredicate = searchPredicate.And(inv => invTypeL.Contains(inv.invType));
                searchPredicate = searchPredicate.And(inv => inv.isActive == true);

                var invoicesCount = (from b in entity.invoices.Where(searchPredicate)
                                     join l in entity.branches on b.branchId equals l.branchId into lj
                                     from x in lj.DefaultIfEmpty()
                                     select new InvoiceModel()
                                     {
                                         invoiceId = b.invoiceId,
                                         invNumber = b.invNumber,
                                     })
                .ToList().Where(inv => inv.invoiceId == entity.invoices.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId).ToList().Count;

                return invoicesCount;
            }
        }
        [HttpPost]
        [Route("GetCountByCreator")]
        public string GetCountByCreator(string token)
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
                string invType = "";
                int createUserId = 0;
                int duration = 0;
                List<string> invTypeL = new List<string>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "createUserId")
                    {
                        createUserId = int.Parse(c.Value);
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                }
                #endregion

                int invoicesCount = GetCountByCreator(invTypeL, duration, createUserId);

                return TokenManager.GenerateToken(invoicesCount);

            }
        }

        [HttpPost]
        [Route("getSalesNot")]
        public string getSalesNot(string token)
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

                int createUserId = 0;
                int branchId = 0;
                int duration = 0;
                string result = "{";
                List<string> invTypeL;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "createUserId")
                    {
                        createUserId = int.Parse(c.Value);
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
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
                        int invCount = 0;

                        #region get sales count
                        invTypeL = new List<string>() { "s", "sb" };

                        var user = entity.users.Find(createUserId);
                        if (user.isAdmin == true)
                            invCount = GetCountInvoicesForAdmin(invTypeL, duration);
                        else
                            invCount = GetCountByCreator(invTypeL, duration, createUserId);

                        result += "InvoiceCount:" + invCount;
                        #endregion

                        #region get waiting sales orders count
                        invTypeL = new List<string>() { "or" };
                        invCount = GetCountUnHandeledOrders(invTypeL, 0, branchId, 0, 0);
                        result += ",SalesWaitingOrdersCount:" + invCount;

                        #endregion

                        #region get waiting sales quotations count
                        invTypeL = new List<string>() { "q" };
                        invCount = GetCountUnHandeledOrders(invTypeL, branchId, 0, 0, 0);
                        result += ",SalesQuotationCount:" + invCount;

                        #endregion
                        result += "}";
                    }
                    return TokenManager.GenerateToken(result);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        [HttpPost]
        [Route("getPurNot")]
        public async Task<string> getPurNot(string token)
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

                int createUserId = 0;
                int branchId = 0;
                int duration = 0;
                string result = "{";
                List<string> invTypeL;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "createUserId")
                    {
                        createUserId = int.Parse(c.Value);
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }
                #endregion
                using (incposdbEntities entity = new incposdbEntities())
                {
                    int invCount = 0;

                    #region get purchase count
                    invTypeL = new List<string>() { "p", "pw", "pb", "pbw" };

                    invCount = GetCountByCreator(invTypeL, duration, createUserId);

                    result += "InvoiceCount:" + invCount;
                    #endregion

                    #region get waiting purchase orders count
                    invTypeL = new List<string>() { "po" };
                    invCount = GetCountUnHandeledOrders(invTypeL, 0, branchId, 0, 0);
                    result += ",OrdersCount:" + invCount;

                    #endregion

                    #region get lack notification
                    ItemsLocationsController ilc = new ItemsLocationsController();
                    string str = await ilc.isThereLackNoPackage(branchId);

                    result += ",isThereLack:'" + str + "'";

                    #endregion
                    result += "}";
                }
                return TokenManager.GenerateToken(result);

            }
        }

        [HttpPost]
        [Route("getInvoicePaymentArchiveCount")]
        public async Task<string> getInvoicePaymentArchiveCount(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                CashTransferController cc = new CashTransferController();
                string result = "{";

                #region params
                int invoiceId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invoiceId")
                    {
                        invoiceId = int.Parse(c.Value);
                    }
                }
                #endregion
                using (incposdbEntities entity = new incposdbEntities())
                {
                    int PaymentsCount = 0;

                    #region get payments count

                    PaymentsCount = cc.GetCountByInvId(invoiceId);

                    result += "PaymentsCount:" + PaymentsCount;
                    #endregion

                    #region get invoice archive count
                   int invCount = GetInvoiceArchiveCount(invoiceId);
                    result += ",InvoiceCount:" + invCount;

                    #endregion

                    result += "}";
                }
                return TokenManager.GenerateToken(result);

            }
        }



        [HttpPost]
        [Route("getTransNot")]
        public async Task<string> getTransNot(string token)
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
                int duration = 0;
                string result = "";
                List<string> invTypeL;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }
                #endregion
                using (incposdbEntities entity = new incposdbEntities())
                {
                    int invCount = 0;
                    result = "{";

                    #region waiting orders count
                    invTypeL = new List<string>() { "exw" };

                    invCount = GetCountBranchInvoices(0, branchId, duration, invTypeL);

                    result += "WaitngExportCount:" + invCount;
                    #endregion

                    #region get  orders count
                    invTypeL = new List<string>() { "im", "ex" };
                    invCount = GetCountBranchInvoices(0, branchId, duration, invTypeL);
                    result += ",OrdersCount:" + invCount;

                    #endregion

                    #region get lack notification
                    ItemsLocationsController ilc = new ItemsLocationsController();
                    string str = await ilc.isThereLack(branchId);

                    result += ",isThereLack:'" + str + "'";

                    #endregion
                    result += "}";
                }
                return TokenManager.GenerateToken(result);

            }
        }

        [HttpPost]
        [Route("getPurOrderNot")]
        public async Task<string> getPurOrderNot(string token)
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

                int createUserId = 0;
                int branchId = 0;
                int duration = 0;
                string result = "{";
                List<string> invTypeL;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "createUserId")
                    {
                        createUserId = int.Parse(c.Value);
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }
                #endregion
                using (incposdbEntities entity = new incposdbEntities())
                {
                    int invCount = 0;

                    #region get purchase count
                    invTypeL = new List<string>() { "po" };

                    invCount = GetCountByCreator(invTypeL, duration, createUserId);

                    result += "InvoiceCount:" + invCount;
                    #endregion

                    #region get lack notification
                    ItemsLocationsController ilc = new ItemsLocationsController();
                    string str = await ilc.isThereLackNoPackage(branchId);
                    result += ",isThereLack:'" + str + "'";

                    #endregion
                    result += "}";
                }
                return TokenManager.GenerateToken(result);

            }
        }

        private int GetCountByCreator(List<string> invTypeL, int duration, int createUserId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var searchPredicate = PredicateBuilder.New<invoices>();

                if (duration > 0)
                {
                    DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-duration).ToShortDateString());
                    searchPredicate = searchPredicate.And(inv => inv.updateDate >= dt);
                }
                searchPredicate = searchPredicate.And(inv => invTypeL.Contains(inv.invType));
                searchPredicate = searchPredicate.And(inv => inv.createUserId == createUserId);
                searchPredicate = searchPredicate.And(inv => inv.isActive == true);

                var invoicesCount = (from b in entity.invoices.Where(searchPredicate)
                                     join l in entity.branches on b.branchId equals l.branchId into lj
                                     from x in lj.DefaultIfEmpty()
                                     select new InvoiceModel()
                                     {
                                         invoiceId = b.invoiceId,
                                         invNumber = b.invNumber,
                                         agentId = b.agentId,
                                         invType = b.invType,
                                         total = b.total,
                                         totalNet = b.totalNet,
                                         paid = b.paid,
                                         deserved = b.deserved,
                                         deservedDate = b.deservedDate,
                                         invDate = b.invDate,
                                         invoiceMainId = b.invoiceMainId,
                                         invCase = b.invCase,
                                         invTime = b.invTime,
                                         notes = b.notes,
                                         itemtax_note = b.itemtax_note,
                                         sales_invoice_note = b.sales_invoice_note,

                                     })
                .ToList().Where(inv => inv.invoiceId == entity.invoices.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId).ToList().Count;
                return invoicesCount;
            }
        }
        [HttpPost]
        [Route("getBranchInvoices")]
        public async Task<string> getBranchInvoices(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                ItemsTransferController ic = new ItemsTransferController();
                CashTransferController cc = new CashTransferController();
                string invType = "";
                int branchCreatorId = 0;
                int branchId = 0;
                int duration = 0;
                List<string> invTypeL = new List<string>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "branchCreatorId")
                    {
                        branchCreatorId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                }

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var searchPredicate = PredicateBuilder.New<invoices>();
                    if (branchCreatorId != 0)
                        searchPredicate = searchPredicate.And(inv => inv.branchCreatorId == branchCreatorId && inv.isActive == true && invTypeL.Contains(inv.invType));
                    if (branchId != 0)
                        searchPredicate = searchPredicate.Or(inv => inv.branchId == branchId && inv.isActive == true && invTypeL.Contains(inv.invType));

                    if (duration > 0)
                    {
                        DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-duration).ToShortDateString());
                        searchPredicate = searchPredicate.And(inv => inv.updateDate >= dt);
                    }

                    var invoicesList = (from b in entity.invoices.Where(searchPredicate)
                                        join l in entity.branches on b.branchId equals l.branchId into lj
                                        from x in lj.DefaultIfEmpty()
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            taxValue = b.taxValue,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchName = x.name,
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            userId = b.userId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            cashReturn = b.cashReturn,
                                            shippingCost = b.shippingCost,
                                            realShippingCost = b.realShippingCost,
                                            isOrginal = b.isOrginal,
                                            printedcount = b.printedcount,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,

                                        })
                                    .ToList().Where(inv => inv.invoiceId == entity.invoices.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId).ToList();
                    
                    if (invoicesList != null)
                    {
                        for (int i = 0; i < invoicesList.Count; i++)
                        {
                            int invoiceId = invoicesList[i].invoiceId;
                            invoicesList[i].invoiceItems = await ic.Get(invoiceId);
                            invoicesList[i].itemsCount = invoicesList[i].invoiceItems.Count;
                            invoicesList[i].cachTrans = cc.GetPayedByInvId(invoiceId);
                        }
                    }

                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }
        [HttpPost]
        [Route("getExportImportInvoices")]
        public async Task<string> getExportImportInvoices(string token)
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
                ItemsTransferController ic = new ItemsTransferController();
                string invType = "";
                int branchId = 0;
                int duration = 0;
                List<string> invTypeL = new List<string>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                }
                #endregion
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var searchPredicate = PredicateBuilder.New<invoices>();

                    if (branchId != 0)
                        searchPredicate = searchPredicate.Or(inv => inv.branchId == branchId && inv.isActive == true && invTypeL.Contains(inv.invType));

                    if (duration > 0)
                    {
                        DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-duration).ToShortDateString());
                        searchPredicate = searchPredicate.And(inv => inv.updateDate >= dt);
                    }

                    var invoicesList = (from b in entity.invoices.Where(searchPredicate)
                                        join l in entity.branches on b.branchId equals l.branchId into lj
                                        from x in lj.DefaultIfEmpty()
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.invoiceMainId != null ? (from i in entity.invoices.Where(m => m.invoiceId == b.invoiceMainId)
                                                                               select i.invType).FirstOrDefault(): 
                                                                               (from i in entity.invoices.Where(m => m.invoiceMainId == b.invoiceId)
                                                                               select i.invType).FirstOrDefault() ,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchCreatorName = b.invoiceMainId != null ? (from i in entity.invoices.Where(m => m.invoiceId == b.invoiceMainId)
                                                                                           join b in entity.branches on i.branchId equals b.branchId
                                                                                           select b.name).FirstOrDefault()
                                                                                 : (from i in entity.invoices.Where(m => m.invoiceMainId == b.invoiceId)
                                                                                    join b in entity.branches on i.branchId equals b.branchId
                                                                                    select b.name).FirstOrDefault(),
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            userId = b.userId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            cashReturn = b.cashReturn,
                                            shippingCost = b.shippingCost,
                                            realShippingCost = b.realShippingCost,
                                            isOrginal = b.isOrginal,
                                            printedcount = b.printedcount,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,

                                        })
                    .ToList();
                    if (invoicesList != null)
                    {
                        for (int i = 0; i < invoicesList.Count; i++)
                        {
                            int invoiceId = invoicesList[i].invoiceId;
                            invoicesList[i].invoiceItems = await ic.Get(invoiceId);
                            invoicesList[i].itemsCount = invoicesList[i].invoiceItems.Count;
                        }
                    }

                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }
        [HttpPost]
        [Route("getExportInvoices")]
        public async Task<string> getExportInvoices(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                ItemsTransferController ic = new ItemsTransferController();
                string invType = "";
                int branchId = 0;
                List<string> invTypeL = new List<string>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var searchPredicate = PredicateBuilder.New<invoices>();
                    if (branchId != 0)
                        searchPredicate = searchPredicate.Or(inv => inv.branchId == branchId && inv.isActive == true && invTypeL.Contains(inv.invType));

                    var invoicesList = (from b in entity.invoices.Where(searchPredicate)
                                        join l in entity.branches on b.branchId equals l.branchId into lj
                                        from x in lj.DefaultIfEmpty()
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchCreatorName = entity.invoices.Where(m => m.invoiceId == b.invoiceMainId).Select(m => m.branches.name).FirstOrDefault(),
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            userId = b.userId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            cashReturn = b.cashReturn,
                                            shippingCost = b.shippingCost,
                                            realShippingCost = b.realShippingCost,
                                            isOrginal = b.isOrginal,
                                            printedcount = b.printedcount,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                        }).ToList();
                    if (invoicesList != null)
                    {
                        for (int i = 0; i < invoicesList.Count; i++)
                        {
                            int invoiceId = invoicesList[i].invoiceId;
                            invoicesList[i].invoiceItems = await ic.Get(invoiceId);
                            invoicesList[i].itemsCount = invoicesList[i].invoiceItems.Count;

                        }
                    }

                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }

        [HttpPost]
        [Route("getInvoicesToReturn")]
        public async Task<string> getInvoicesToReturn(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                ItemsTransferController ic = new ItemsTransferController();
                CashTransferController cc = new CashTransferController();
                string invType = "";
                int userId = 0;
                List<string> invTypeL = new List<string>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var branches = (from S in entity.branchesUsers
                                    join B in entity.branches on S.branchId equals B.branchId into JB
                                    join U in entity.users on S.userId equals U.userId into JU
                                    from JBB in JB.DefaultIfEmpty()
                                    from JUU in JU.DefaultIfEmpty()
                                    where S.userId == userId
                                    select JBB.branchId).ToList();

                    var searchPredicate = PredicateBuilder.New<invoices>();
                    searchPredicate = searchPredicate.Or(inv => branches.Contains((int)inv.branchId) && inv.isActive == true && invTypeL.Contains(inv.invType));

                    var invoicesList = (from b in entity.invoices.Where(searchPredicate)
                                        join l in entity.branches on b.branchId equals l.branchId into lj
                                        from x in lj.DefaultIfEmpty()
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            branchId = b.branchId,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            taxValue = b.taxValue,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchName = x.name,
                                            branchCreatorId = b.branchCreatorId,
                                            userId = b.userId,
                                            cashReturn = b.cashReturn,
                                            shippingCost = b.shippingCost,
                                            realShippingCost = b.realShippingCost,
                                            isOrginal = b.isOrginal,
                                            printedcount = b.printedcount,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,

                                        })
                    .ToList();
                    if (invoicesList != null)
                    {
                        for (int i = 0; i < invoicesList.Count; i++)
                        {
                            int invoiceId = invoicesList[i].invoiceId;
                            invoicesList[i].invoiceItems = await ic.Get(invoiceId);
                            invoicesList[i].itemsCount = invoicesList[i].invoiceItems.Count;
                            invoicesList[i].cachTrans = cc.GetPayedByInvId(invoiceId);
                            invoicesList[i].invoiceTaxes = GetInvoiceTaxes(invoiceId);
                        }
                    }
                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }
        [HttpPost]
        [Route("getUnHandeldOrders")]
        public async Task<string> getUnHandeldOrders(string token)
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
                string invType = "";
                int branchCreatorId = 0;
                int branchId = 0;
                int duration = 0;
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                    }
                    else if (c.Type == "branchCreatorId")
                    {
                        branchCreatorId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                #endregion
                var invoicesList = await getUnhandeledOrdersList(invType, branchCreatorId, branchId, duration, userId);

                return TokenManager.GenerateToken(invoicesList);
            }
        }
        [HttpPost]
        [Route("getWaitingOrders")]
        public async Task<string> getWaitingOrders(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string invType = "";
                int branchCreatorId = 0;
                int branchId = 0;
                int duration = 0;
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                    }
                    else if (c.Type == "branchCreatorId")
                    {
                        branchCreatorId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                var invoicesList = await getWaitingOrdersList(invType, branchId);
                return TokenManager.GenerateToken(invoicesList);
            }
        }

        [HttpPost]
        [Route("GetCountUnHandeledOrders")]
        public string GetCountUnHandeledOrders(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string invType = "";
                int branchCreatorId = 0;
                int branchId = 0;
                int userId = 0;
                int duration = 0;
                List<string> invTypeL = new List<string>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "branchCreatorId")
                    {
                        branchCreatorId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }

                var invoicesCount = GetCountUnHandeledOrders(invTypeL, branchCreatorId, branchId, duration, userId);
                return TokenManager.GenerateToken(invoicesCount);
            }
        }

        private int GetCountUnHandeledOrders(List<string> invTypeL, int branchCreatorId, int branchId, int duration = 0, int userId = 0)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var searchPredicate = PredicateBuilder.New<invoices>();
                searchPredicate = searchPredicate.And(inv => inv.isActive == true && invTypeL.Contains(inv.invType));
                if (branchCreatorId != 0)
                    searchPredicate = searchPredicate.And(inv => inv.branchCreatorId == branchCreatorId && inv.isActive == true && invTypeL.Contains(inv.invType));

                if (branchId != 0)
                    searchPredicate = searchPredicate.And(inv => inv.branchId == branchId);
                if (duration > 0)
                {
                    DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-duration).ToShortDateString());
                    searchPredicate = searchPredicate.And(inv => inv.updateDate >= dt);
                }
                if (userId != 0)
                    searchPredicate = searchPredicate.And(inv => inv.createUserId == userId);

                var invoicesCount = (from b in entity.invoices.Where(searchPredicate)
                                     join l in entity.branches on b.branchId equals l.branchId into lj
                                     from x in lj.DefaultIfEmpty()
                                     where !entity.invoices.Any(y => y.invoiceMainId == b.invoiceId)
                                     select new InvoiceModel()
                                     {
                                         invoiceId = b.invoiceId,
                                         invNumber = b.invNumber,

                                     })
                .ToList().Count;
                return invoicesCount;
            }
        }
        [HttpPost]
        [Route("GetCountBranchInvoices")]
        public string GetCountBranchInvoices(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters definition
                string invType = "";
                int branchCreatorId = 0;
                int branchId = 0;
                int duration = 0;
                List<string> invTypeL = new List<string>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "branchCreatorId")
                    {
                        branchCreatorId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "duration")
                    {
                        duration = int.Parse(c.Value);
                    }
                }
                #endregion
                var invoicesCount = GetCountBranchInvoices(branchCreatorId, branchId, duration, invTypeL);
                return TokenManager.GenerateToken(invoicesCount);

               
            }
        }

        private int GetCountBranchInvoices(int branchCreatorId, int branchId, int duration, List<string> invTypeL)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var searchPredicate = PredicateBuilder.New<invoices>();
                if (branchCreatorId != 0)
                    searchPredicate = searchPredicate.And(inv => inv.branchCreatorId == branchCreatorId && inv.isActive == true && invTypeL.Contains(inv.invType));
                // searchPredicate = searchPredicate.And(inv => invTypeL.Contains(inv.invType));
                if (branchId != 0)
                    searchPredicate = searchPredicate.Or(inv => inv.branchId == branchId && inv.isActive == true && invTypeL.Contains(inv.invType));

                if (duration > 0)
                {
                    DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-duration).ToShortDateString());
                    searchPredicate = searchPredicate.And(inv => inv.updateDate >= dt);
                }

                var invoicesCount = (from b in entity.invoices.Where(searchPredicate)
                                     join l in entity.branches on b.branchId equals l.branchId into lj
                                     from x in lj.DefaultIfEmpty()
                                     select new InvoiceModel()
                                     {
                                         invoiceId = b.invoiceId,
                                         invNumber = b.invNumber,
   
                                     })
                .ToList().Count;

                return invoicesCount;
            }
        }

        [HttpPost]
        [Route("getDeliverOrders")]
        public async Task<string> getDeliverOrders(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                ItemsTransferController ic = new ItemsTransferController();
                CashTransferController cc = new CashTransferController();
                string invType = "";
                string status = "";
                int shipUserId = 0;
                List<string> invTypeL = new List<string>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "status")
                    {
                        status = c.Value;
                    }
                    else if (c.Type == "userId")
                    {
                        shipUserId = int.Parse(c.Value);
                    }
                }

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var invoicesList = (from b in entity.invoices.Where(x => invTypeL.Contains(x.invType) && x.shipUserId == shipUserId && x.isActive == true)
                                        join s in entity.invoiceStatus on b.invoiceId equals s.invoiceId
                                        where (s.status == status && s.invStatusId == entity.invoiceStatus.Where(x => x.invoiceId == b.invoiceId).Max(x => x.invStatusId))
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            agentId = b.agentId,
                                            agentName = b.agents.name,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            taxValue = b.taxValue,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            userId = b.userId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            shippingCost = b.shippingCost,
                                            realShippingCost = b.realShippingCost,
                                            isOrginal = b.isOrginal,
                                            printedcount = b.printedcount,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,

                                        })
                    .ToList();
                    if (invoicesList != null)
                    {
                        for (int i = 0; i < invoicesList.Count; i++)
                        {
                            int invoiceId = invoicesList[i].invoiceId;
                            invoicesList[i].invoiceItems = await ic.Get(invoiceId);
                            invoicesList[i].itemsCount = invoicesList[i].invoiceItems.Count;
                            invoicesList[i].cachTrans = cc.GetPayedByInvId(invoiceId);
                        }
                    }

                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }
        [HttpPost]
        [Route("getDeliverOrdersCount")]
        public string getDeliverOrdersCount(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string invType = "";
                string status = "";
                int shipUserId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;

                    }
                    else if (c.Type == "status")
                    {
                        status = c.Value;
                    }
                    else if (c.Type == "userId")
                    {
                        shipUserId = int.Parse(c.Value);
                    }
                }

                int invoicesCount = getDeliverOrdersCount(invType, status, shipUserId);
                return TokenManager.GenerateToken(invoicesCount);
               
            }
        }

        public int getDeliverOrdersCount(string invType, string status, int shipUserId)
        {
            List<string> invTypeL = new List<string>();
            string[] invTypeArray = invType.Split(',');
            foreach (string s in invTypeArray)
                invTypeL.Add(s.Trim());
            using (incposdbEntities entity = new incposdbEntities())
            {
                var invoicesCount = (from b in entity.invoices.Where(x => invTypeL.Contains(x.invType) && x.shipUserId == shipUserId && x.isActive == true)
                                     join s in entity.invoiceStatus on b.invoiceId equals s.invoiceId
                                     where (s.status == status && s.invStatusId == entity.invoiceStatus.Where(x => x.invoiceId == b.invoiceId).Max(x => x.invStatusId))
                                     select new InvoiceModel()
                                     {
                                         invoiceId = b.invoiceId,
                                         invNumber = b.invNumber,
   
                                     })
                .ToList().Count;

                return invoicesCount;
            }
        }
        [HttpPost]
        [Route("getOrdersForPay")]
        public string getOrdersForPay(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int branchId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }
                List<string> statusL = new List<string>();
                //statusL.Add("tr");
                //statusL.Add("rc");
                //statusL.Add("InTheWay");
                statusL.Add("Delivered");
                statusL.Add("Done");
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var invoicesList = (from b in entity.invoices.Where(x => x.invType == "s" && x.branchCreatorId == branchId && x.shipUserId != null && x.isActive == true)
                                        join s in entity.invoiceStatus on b.invoiceId equals s.invoiceId
                                        join u in entity.users on b.shipUserId equals u.userId into lj
                                        from y in lj.DefaultIfEmpty()
                                        where (statusL.Contains(s.status) && s.invStatusId == entity.invoiceStatus.Where(x => x.invoiceId == b.invoiceId).Max(x => x.invStatusId))
                                        select new InvoiceModel()
                                        {
                                            invStatusId = s.invStatusId,
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            agentName = b.agents.name + " " + b.agents.lastName,
                                            shipUserName = y.name + " " + y.lastname,
                                            status = s.status,
                                            userId = b.userId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            shippingCost = b.shippingCost,
                                            realShippingCost = b.realShippingCost,
                                            payStatus = b.deserved == 0 ? "payed" : (b.deserved == b.totalNet ? "unpayed" : "partpayed"),
                                            branchCreatorName = entity.branches.Where(X => X.branchId == b.branchCreatorId).FirstOrDefault().name,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,

                                        })
                    .ToList();
                    if (invoicesList != null)
                    {
                        for (int i = 0; i < invoicesList.Count; i++)
                        {
                            int invoiceId = invoicesList[i].invoiceId;
                            int itemCount = entity.itemsTransfer.Where(x => x.invoiceId == invoiceId).Select(x => x.itemsTransId).ToList().Count;
                            invoicesList[i].itemsCount = itemCount;
                        }
                    }

                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }

        [HttpPost]
        [Route("GetOrdersWithDelivery")]
        public string GetOrdersWithDelivery(string token)
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
                int branchId = 0;
                string statusStr = "";
                List<string> statusL = new List<string>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "status")
                    {
                        statusStr = c.Value;
                        string[] statusArray = statusStr.Split(',');
                        foreach (string s in statusArray)
                            statusL.Add(s.Trim());
                    }
                }
                #endregion

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var invoicesList = (from b in entity.invoices.Where(x => x.invType == "s" && x.branchCreatorId == branchId && x.shippingCompanyId != null && x.isActive == true)
                                        join s in entity.invoiceStatus on b.invoiceId equals s.invoiceId
                                        join u in entity.users on b.shipUserId equals u.userId into lj
                                        from y in lj.DefaultIfEmpty()
                                        where (s.invStatusId == entity.invoiceStatus.Where(x => x.invoiceId == b.invoiceId).Max(x => x.invStatusId))
                                        select new InvoiceModel()
                                        {
                                            invStatusId = s.invStatusId,
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            agentName = b.agents.name,
                                            shipUserName = y.name + " " + y.lastname,
                                            shipCompanyName = b.shippingCompanies.name,
                                            status = s.status,
                                            userId = b.userId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            shippingCost = b.shippingCost,
                                            realShippingCost = b.realShippingCost,
                                            payStatus = b.deserved == 0 ? "payed" : (b.deserved == b.totalNet ? "unpayed" : "partpayed"),
                                            branchCreatorName = entity.branches.Where(X => X.branchId == b.branchCreatorId).FirstOrDefault().name,
                                            agentAddress = b.agents.address,
                                            agentMobile = b.agents.mobile,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,

                                        })
                    .ToList();

                    if (statusStr != "")
                        invoicesList = invoicesList.Where(s => statusL.Contains(s.status)).ToList();

                    if (invoicesList != null)
                    {
                        for (int i = 0; i < invoicesList.Count; i++)
                        {
                            int invoiceId = invoicesList[i].invoiceId;
                            int itemCount = entity.itemsTransfer.Where(x => x.invoiceId == invoiceId).Select(x => x.itemsTransId).ToList().Count;
                            invoicesList[i].itemsCount = itemCount;
                        }
                    }

                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }
        [HttpPost]
        [Route("GetDailyDestructive")]
        public string GetDailyDestructive(string token)
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
                int branchId = 0;
                int userId = 0;
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
                }
                #endregion

                using (incposdbEntities entity = new incposdbEntities())
                {
                    DateTime dt = Convert.ToDateTime(DateTime.Today.ToShortDateString());

                    var invoicesList = (from  I in entity.invoices.Where(x => x.branchCreatorId == branchId
                                       && x.invType == "d" && x.isActive == true && x.createUserId == userId
                                       && x.invDate >= dt)
                     join IT in entity.itemsTransfer on I.invoiceId equals IT.invoiceId
                     from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)

                     join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                     join U in entity.users on I.createUserId equals U.userId into JU
                     join du in entity.users on I.userId equals du.userId into Dusr
 
                     from JUU in JU.DefaultIfEmpty()

                     from duu in Dusr.DefaultIfEmpty()
                     from JBCC in JBC.DefaultIfEmpty()

                     select new ItemTransferInvoice
                     {
                         causeDestroy = IT.inventoryItemLocation.fallCause != null ? IT.inventoryItemLocation.fallCause : IT.cause,
                         userdestroy = duu.username,

                         itemName = IU.items.name,
                         unitName = IU.units.name,
                         itemUnitId = IT.itemUnitId,

                         itemId = IU.itemId,
                         unitId = IU.unitId,
                         quantity = IT.quantity,

                         invoiceId = I.invoiceId,
                         invNumber = I.invNumber,
                         total = I.totalNet,

                         IupdateDate = I.updateDate,

                         branchName = JBCC.name,
                         branchId = I.branchCreatorId,

                     }).ToList();

                   
                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }

        [HttpPost]
        [Route("GetDailyShortage")]
        public string GetDailyShortage(string token)
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
                int branchId = 0;
                int userId = 0;
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
                }
                #endregion

                using (incposdbEntities entity = new incposdbEntities())
                {
                    DateTime dt = Convert.ToDateTime(DateTime.Today.ToShortDateString());

                    var invoicesList = (from  I in entity.invoices.Where(x => x.branchCreatorId == branchId
                                       && x.invType == "sh" && x.isActive == true && x.createUserId == userId
                                       && x.invDate >= dt)
                     join IT in entity.itemsTransfer on I.invoiceId equals IT.invoiceId
                     from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)

                     join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                     join U in entity.users on I.createUserId equals U.userId into JU
                     join du in entity.users on I.userId equals du.userId into Dusr
 
                     from JUU in JU.DefaultIfEmpty()

                     from duu in Dusr.DefaultIfEmpty()
                     from JBCC in JBC.DefaultIfEmpty()

                     select new ItemTransferInvoice
                     {

                         causeDestroy = IT.inventoryItemLocation.fallCause != null ? IT.inventoryItemLocation.fallCause : IT.cause,
                         userdestroy = duu.username,

                         itemName = IU.items.name,
                         unitName = IU.units.name,
                         itemUnitId = IT.itemUnitId,

                         itemId = IU.itemId,
                         unitId = IU.unitId,
                         quantity = IT.quantity,

                         invoiceId = I.invoiceId,
                         invNumber = I.invNumber,
                         total = I.totalNet,

                         IupdateDate = I.updateDate,

                         branchName = JBCC.name,
                         branchId = I.branchCreatorId,

                     }).ToList();

                   
                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }

        [HttpPost]
        [Route("EditInvoiceDelivery")]
        public string EditInvoiceDelivery(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "1";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region params
                int invoiceId = 0;
                int? shipUserId = 0;
                int shippingCompanyId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invoiceId")
                    {
                        invoiceId = int.Parse(c.Value);
                    }
                    else if (c.Type == "shipUserId")
                    {
                        try
                        {
                            shipUserId = int.Parse(c.Value);
                        }
                        catch
                        {
                            shipUserId = null;
                        }
                    }
                    else if (c.Type == "shippingCompanyId")
                    {
                        shippingCompanyId = int.Parse(c.Value);
                    }

                }
                #endregion
                try
                {
                    CashTransferController ctc = new CashTransferController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                       
                        var inv = entity.invoices.Find(invoiceId);
                        #region update shipping company
                        if (inv.shippingCompanyId != shippingCompanyId)
                        {
                            var company = entity.shippingCompanies.Find(shippingCompanyId);
                            var cashTransfer = entity.cashTransfer.Where(x => x.invId == invoiceId && x.processType == "deliver").FirstOrDefault();

                            decimal previousComCash = inv.realShippingCost;
                            inv.shippingCost = (decimal)company.deliveryCost;
                            inv.realShippingCost = (decimal)company.RealDeliveryCost;
                            if(cashTransfer != null)
                            {
                                cashTransfer.cash = company.RealDeliveryCost;
                                cashTransfer.shippingCompanyId = shippingCompanyId;
                               

                                //decrease previous company balance
                                ctc.decreaseShippingComBalance((int)inv.shippingCompanyId, previousComCash);
                                //increase new shipping company balance
                                ctc.increaseShippingComBalance(shippingCompanyId, (decimal)company.RealDeliveryCost);
                            }
                        }
                        #endregion

                        #region edit shipping info
                        inv.shipUserId = shipUserId;
                        inv.shippingCompanyId = shippingCompanyId;
                        entity.SaveChanges();
                        #endregion
                    }
                }
                catch
                {
                    message = "0";
                }
                return TokenManager.GenerateToken(message);
            }
        }

        [HttpPost]
        [Route("getAgentInvoices")]
        public string getAgentInvoices(string token)
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
                int agentId = 0;
                string type = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "agentId")
                    {
                        agentId = int.Parse(c.Value);
                    }
                    else if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                }
                #endregion
                List<string> typesList = new List<string>();
                if (type.Equals("feed"))
                {
                    typesList.Add("pb");
                    typesList.Add("s");
                }
                else if (type.Equals("pay"))
                {
                    typesList.Add("p");
                    typesList.Add("sb");
                    typesList.Add("pw");
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var invoicesList = (from b in entity.invoices.Where(x => x.agentId == agentId && typesList.Contains(x.invType)
                                                 && x.deserved > 0  &&
                                           ((x.shippingCompanyId == null && x.shipUserId == null ) ||
                                          (x.shippingCompanyId != null && x.shipUserId == null && x.isPrePaid == 1 ) ||
                                           (x.shippingCompanyId != null && x.shipUserId != null ) ))
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            realShippingCost = b.realShippingCost,
                                            shippingCost = b.shippingCost,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,

                                        }).ToList();

                   // invoicesList = invoicesList.Where(inv => (inv.invoiceId == invoicesList.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId && inv.invoiceMainId == null && (inv.invType == "s" || inv.invType == "p" ))).ToList();
                    //invoicesList = invoicesList.Where(inv => (inv.invoiceId == invoicesList.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId && (inv.invType == "s" || inv.invType == "p"))
                    //                || (inv.invType != "s" && inv.invType !="p") ).ToList();

                    invoicesList = invoicesList.Where(inv => inv.invoiceMainId == null
                    || (inv.invoiceMainId != null 
                                && entity.invoices.Where(x => x.invoiceId == inv.invoiceMainId && x.invType != "s" && x.invType != "p").FirstOrDefault() != null))
                        .ToList();
                    //get only with Done status
                    if (type == "feed")
                    {
                        List<InvoiceModel> res = new List<InvoiceModel>();
                        foreach (InvoiceModel inv in invoicesList)
                        {
                            int invoiceId = inv.invoiceId;

                            var statusObj = entity.invoiceStatus.Where(x => x.invoiceId == invoiceId && x.status == "Done").FirstOrDefault();

                            if (statusObj != null)
                            {
                                int itemCount = entity.itemsTransfer.Where(x => x.invoiceId == invoiceId).Select(x => x.itemsTransId).ToList().Count;
                                inv.itemsCount = itemCount;
                                res.Add(inv);
                            }
                        }
                        return TokenManager.GenerateToken(res);
                    }
                    else
                    {
                        for (int i = 0; i < invoicesList.Count; i++)
                        {
                            int invoiceId = invoicesList[i].invoiceId;
                            int itemCount = entity.itemsTransfer.Where(x => x.invoiceId == invoiceId).Select(x => x.itemsTransId).ToList().Count;
                            invoicesList[i].itemsCount = itemCount;
                        }
                        return TokenManager.GenerateToken(invoicesList);
                    }

                }
            }
        }
        [HttpPost]
        [Route("getNotPaidAgentInvoices")]
        public string getNotPaidAgentInvoices(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int agentId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        agentId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var invoicesList = (from b in entity.invoices.Where(x => x.agentId == agentId && x.deserved > 0)
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            realShippingCost = b.realShippingCost,
                                            shippingCost = b.shippingCost,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,

                                        }).ToList();

                    invoicesList = invoicesList.Where(inv => inv.invoiceId == invoicesList.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId).ToList();

                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }
       
        [HttpPost]
        [Route("getShipCompanyInvoices")]
        public string getShipCompanyInvoices(string token)
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
                int shippingCompanyId = 0;
                string type = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "shippingCompanyId")
                    {
                        shippingCompanyId = int.Parse(c.Value);
                    }
                    else if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                }
                #endregion
                List<string> typesList = new List<string>();
                if (type.Equals("feed"))
                {
                    typesList.Add("pb");
                    typesList.Add("s");
                }
                else if (type.Equals("pay"))
                {
                    typesList.Add("p");
                    typesList.Add("sb");
                    typesList.Add("pw");
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    List<InvoiceModel> res = new List<InvoiceModel>();
                    var invoicesList = (from b in entity.invoices.Where(x => x.shippingCompanyId == shippingCompanyId && typesList.Contains(x.invType)
                                      && x.deserved > 0  &&
                                         x.shippingCompanyId != null && x.shipUserId == null && x.agentId != null && x.isPrePaid == 0)
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            shippingCost = b.shippingCost,
                                            realShippingCost = b.realShippingCost,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,

                                        }).ToList();

                    invoicesList = invoicesList.Where(inv => inv.invoiceId == invoicesList.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId).ToList();

                    if (invoicesList != null)
                    {
                        for (int i = 0; i < invoicesList.Count; i++)
                        {
                            int invoiceId = invoicesList[i].invoiceId;
                            var statusObj = entity.invoiceStatus.Where(x => x.invoiceId == invoiceId && x.status == "Done").FirstOrDefault();

                            if (statusObj != null)
                            {
                                
                                int itemCount = entity.itemsTransfer.Where(x => x.invoiceId == invoiceId).Select(x => x.itemsTransId).ToList().Count;
                                invoicesList[i].itemsCount = itemCount;
                                res.Add(invoicesList[i]);
                            }
                        }
                    }

                    return TokenManager.GenerateToken(res);
                }
            }
        }
        [HttpPost]
        [Route("getUserInvoices")]
        public string getUserInvoices(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region
                int branchId = 0;
                int userId = 0;
                string type = "";
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
                    else if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                }
                #endregion
                List<string> typesList = new List<string>();
                if (type.Equals("feed"))
                {
                    typesList.Add("pb");
                    typesList.Add("s");
                }
                else if (type.Equals("pay"))
                {
                    typesList.Add("p");
                    typesList.Add("sb");
                    typesList.Add("pw");
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var invoicesList = (from b in entity.invoices.Where(x => x.userId == userId && typesList.Contains(x.invType) &&
                                                                              x.deserved > 0 && x.branchCreatorId == branchId)
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchCreatorId = b.branchCreatorId,
                                            userId = b.userId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            realShippingCost = b.realShippingCost,
                                            shippingCost = b.shippingCost,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,

                                        }).ToList();

                    invoicesList = invoicesList.Where(inv => inv.invoiceId == invoicesList.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId).ToList();

                    if (invoicesList != null)
                    {
                        for (int i = 0; i < invoicesList.Count; i++)
                        {
                            int invoiceId = invoicesList[i].invoiceId;
                            int itemCount = entity.itemsTransfer.Where(x => x.invoiceId == invoiceId).Select(x => x.itemsTransId).ToList().Count;
                            invoicesList[i].itemsCount = itemCount;
                        }
                    }

                    return TokenManager.GenerateToken(invoicesList);
                }
            }
        }
        [HttpPost]
        [Route("GetOrderByType")]
        public async Task<string> GetOrderByType(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                ItemsTransferController ic = new ItemsTransferController();
                string invType = "";
                int branchId = 0;
                List<string> invTypeL = new List<string>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                        string[] invTypeArray = invType.Split(',');
                        foreach (string s in invTypeArray)
                            invTypeL.Add(s.Trim());
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }

                using (incposdbEntities entity = new incposdbEntities())
                {
                    if (branchId == 0)
                    {
                        var invoicesList = (from b in entity.invoices.Where(x => invTypeL.Contains(x.invType) && x.invoiceMainId == null)
                                            join l in entity.branches on b.branchId equals l.branchId into lj
                                            from x in lj.DefaultIfEmpty()
                                            select new InvoiceModel()
                                            {
                                                invoiceId = b.invoiceId,
                                                invNumber = b.invNumber,
                                                agentId = b.agentId,
                                                invType = b.invType,
                                                total = b.total,
                                                totalNet = b.totalNet,
                                                paid = b.paid,
                                                deserved = b.deserved,
                                                deservedDate = b.deservedDate,
                                                invDate = b.invDate,
                                                invoiceMainId = b.invoiceMainId,
                                                invCase = b.invCase,
                                                invTime = b.invTime,
                                                notes = b.notes,
                                                itemtax_note = b.itemtax_note,
                                                sales_invoice_note = b.sales_invoice_note,

                                                vendorInvNum = b.vendorInvNum,
                                                vendorInvDate = b.vendorInvDate,
                                                createUserId = b.createUserId,
                                                updateDate = b.updateDate,
                                                updateUserId = b.updateUserId,
                                                branchId = b.branchId,
                                                DBDiscountValue = b.discountValue,
                                                discountType = b.discountType,
                                                tax = b.tax,
                                                taxtype = b.taxtype,
                                                taxValue = b.taxValue,
                                                VATValue = b.VATValue,
                                                name = b.name,
                                                isApproved = b.isApproved,
                                                branchName = x.name,
                                                branchCreatorId = b.branchCreatorId,
                                                shippingCompanyId = b.shippingCompanyId,
                                                shipUserId = b.shipUserId,
                                                userId = b.userId,
                                                manualDiscountType = b.manualDiscountType,
                                                manualDiscountValue = b.manualDiscountValue,
                                                cashReturn = b.cashReturn,
                                                shippingCost = b.shippingCost,
                                                realShippingCost = b.realShippingCost,
                                                isOrginal = b.isOrginal,
                                                printedcount = b.printedcount,
                                                sliceId = b.sliceId,
                                                sliceName = b.sliceName,
                                                isFreeShip = b.isFreeShip,

                                            })
                        .ToList();
                        if (invoicesList != null)
                        {
                            for (int i = 0; i < invoicesList.Count; i++)
                            {
                                int invoiceId = invoicesList[i].invoiceId;
                                invoicesList[i].invoiceItems = await ic.Get(invoiceId);
                                invoicesList[i].itemsCount = invoicesList[i].invoiceItems.Count;
                            }
                        }

                        return TokenManager.GenerateToken(invoicesList);
                    }
                    else
                    {
                        var invoicesList = (from b in entity.invoices.Where(x => invTypeL.Contains(x.invType) && x.branchId == branchId && x.invoiceMainId == null)
                                            join l in entity.branches on b.branchId equals l.branchId into lj
                                            from x in lj.DefaultIfEmpty()
                                            select new InvoiceModel()
                                            {
                                                invoiceId = b.invoiceId,
                                                invNumber = b.invNumber,
                                                agentId = b.agentId,
                                                invType = b.invType,
                                                total = b.total,
                                                totalNet = b.totalNet,
                                                paid = b.paid,
                                                deserved = b.deserved,
                                                deservedDate = b.deservedDate,
                                                invDate = b.invDate,
                                                invoiceMainId = b.invoiceMainId,
                                                invCase = b.invCase,
                                                invTime = b.invTime,
                                                notes = b.notes,
                                                itemtax_note = b.itemtax_note,
                                                sales_invoice_note = b.sales_invoice_note,

                                                vendorInvNum = b.vendorInvNum,
                                                vendorInvDate = b.vendorInvDate,
                                                createUserId = b.createUserId,
                                                updateDate = b.updateDate,
                                                updateUserId = b.updateUserId,
                                                branchId = b.branchId,
                                                DBDiscountValue = b.discountValue,
                                                discountType = b.discountType,
                                                tax = b.tax,
                                                taxtype = b.taxtype,
                                                VATValue = b.VATValue,
                                                name = b.name,
                                                isApproved = b.isApproved,
                                                branchName = x.name,
                                                branchCreatorId = b.branchCreatorId,
                                                shippingCompanyId = b.shippingCompanyId,
                                                shipUserId = b.shipUserId,
                                                userId = b.userId,
                                                manualDiscountType = b.manualDiscountType,
                                                manualDiscountValue = b.manualDiscountValue,
                                                shippingCost = b.shippingCost,
                                                realShippingCost = b.realShippingCost,
                                                sliceId = b.sliceId,
                                                sliceName = b.sliceName,
                                                isFreeShip = b.isFreeShip,

                                            })
                        .ToList();
                        if (invoicesList != null)
                        {
                            for (int i = 0; i < invoicesList.Count; i++)
                            {
                                int invoiceId = invoicesList[i].invoiceId;
                                int itemCount = entity.itemsTransfer.Where(x => x.invoiceId == invoiceId).Select(x => x.itemsTransId).ToList().Count;
                                invoicesList[i].itemsCount = itemCount;
                            }
                        }

                        return TokenManager.GenerateToken(invoicesList);
                    }
                }
            }
        }

        [HttpPost]
        [Route("getInvoicesByAgentAndType")]
        public string getInvoicesByAgentAndType(string token)
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
                int agentId = 0;
                string type = "";
                string duration = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "agentId")
                    {
                        agentId = int.Parse(c.Value);
                    }
                    else if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                    else if (c.Type == "duration")
                    {
                        duration = c.Value;
                    }
                }
                #endregion
                using (incposdbEntities entity = new incposdbEntities())
                {
                    Calculate calc = new Calculate();
                    DateTime startDate = DateTime.Now.Date;

                    var searchPredicate = PredicateBuilder.New<invoices>();
                    searchPredicate = searchPredicate.And(x => x.agentId == agentId && x.invType == type && x.isActive == true);

                    if(duration != "")
                    {
                        if (duration.Trim() == "daily")
                            searchPredicate = searchPredicate.And(x => x.updateDate >= startDate);

                        else if (duration.Trim() == "weekly")
                        {
                            var startOfWeek = calc.StartOfWeek(DateTime.Now.Date, DayOfWeek.Sunday);
                            var endOfWeek = startOfWeek.AddDays(7);
                            searchPredicate = searchPredicate.And(x => x.updateDate >= startOfWeek && x.updateDate < endOfWeek);
                        }
                        else if (duration.Trim() == "monthly")
                        {
                            var startOfMonth = calc.StartOfMonth(startDate);
                            var endOfMonth = calc.EndOfMonth(startDate);
                            searchPredicate = searchPredicate.And(x => x.updateDate >= startOfMonth && x.updateDate <= endOfMonth);
                        }
                        else if (duration.Trim() == "yearly")
                        {
                            var startOfYear = calc.StartOfYear(DateTime.Now.Year);
                            var endOfYear = calc.EndOfYear(DateTime.Now.Year);
                            searchPredicate = searchPredicate.And(x => x.updateDate >= startOfYear && x.updateDate <= endOfYear);
                        }
                    }
                    var invoicesList = (from b in entity.invoices.Where(searchPredicate)
                                        select new InvoiceModel()
                                        {
                                            invoiceId = b.invoiceId,
                                            invNumber = b.invNumber,
                                            agentId = b.agentId,
                                            invType = b.invType,
                                            total = b.total,
                                            totalNet = b.totalNet,
                                            paid = b.paid,
                                            deserved = b.deserved,
                                            deservedDate = b.deservedDate,
                                            invDate = b.invDate,
                                            invoiceMainId = b.invoiceMainId,
                                            invCase = b.invCase,
                                            invTime = b.invTime,
                                            notes = b.notes,
                                            itemtax_note = b.itemtax_note,
                                            sales_invoice_note = b.sales_invoice_note,

                                            vendorInvNum = b.vendorInvNum,
                                            vendorInvDate = b.vendorInvDate,
                                            createUserId = b.createUserId,
                                            updateDate = b.updateDate,
                                            updateUserId = b.updateUserId,
                                            branchId = b.branchId,
                                            DBDiscountValue = b.discountValue,
                                            discountType = b.discountType,
                                            tax = b.tax,
                                            taxtype = b.taxtype,
                                            VATValue = b.VATValue,
                                            name = b.name,
                                            isApproved = b.isApproved,
                                            branchCreatorId = b.branchCreatorId,
                                            shippingCompanyId = b.shippingCompanyId,
                                            shipUserId = b.shipUserId,
                                            manualDiscountType = b.manualDiscountType,
                                            manualDiscountValue = b.manualDiscountValue,
                                            realShippingCost = b.realShippingCost,
                                            shippingCost = b.shippingCost,
                                            sliceId = b.sliceId,
                                            sliceName = b.sliceName,
                                            isFreeShip = b.isFreeShip,

                                        }).ToList();

                    // invoicesList = invoicesList.Where(inv => inv.invoiceMainId == null
                    //|| (inv.invoiceMainId != null
                    //            && entity.invoices.Where(x => x.invoiceId == inv.invoiceMainId && x.invType != "s" && x.invType != "p").FirstOrDefault() != null))
                    //    .ToList();

                    invoicesList = invoicesList.Where(inv => inv.invoiceId == invoicesList.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId).ToList();

                    for (int i = 0; i < invoicesList.Count; i++)
                    {
                        int invoiceId = invoicesList[i].invoiceId;
                        int itemCount = entity.itemsTransfer.Where(x => x.invoiceId == invoiceId).Select(x => x.itemsTransId).ToList().Count;
                        invoicesList[i].itemsCount = itemCount;
                    }
                    return TokenManager.GenerateToken(invoicesList);

                }
            }
        }
    
        // for report
        [HttpPost]
        [Route("GetinvCountBydate")]
        public string GetinvCountBydate(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string invType = "";
                string branchType = "";
                DateTime startDate = countryc.AddOffsetTodate(DateTime.Now);
                DateTime endDate = countryc.AddOffsetTodate(DateTime.Now);
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invType")
                    {
                        invType = c.Value;
                    }
                    else if (c.Type == "branchType")
                    {
                        branchType = c.Value;
                    }
                    else if (c.Type == "startDate")
                    {
                        startDate = DateTime.Parse(c.Value);
                    }
                    else if (c.Type == "endDate")
                    {
                        endDate = DateTime.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var invListm = (from I in entity.invoices
                                    join B in entity.branches on I.branchId equals B.branchId into JB
                                    from JBB in JB.DefaultIfEmpty()
                                    where //(invtype == "all" ? true : I.invType == invtype)  &&
                                      (branchType == "all" ? true : JBB.type == branchType)
                                    && System.DateTime.Compare((DateTime)startDate, (DateTime)I.invDate) <= 0
                                    && System.DateTime.Compare((DateTime)endDate, (DateTime)I.invDate) >= 0
                                    // I.invType == invtype
                                    //     && branchType == "all" ? true : JBB.type == branchType

                                    //  && startDate <= I.invDate && endDate >= I.invDate
                                    // &&  System.DateTime.Compare((DateTime)startDate,  I.invDate) <= 0 && System.DateTime.Compare((DateTime)endDate, I.invDate) >= 0
                                    group new { I, JBB } by (I.branchId) into g
                                    select new
                                    {
                                        branchId = g.Key,
                                        name = g.Select(t => t.JBB.name).FirstOrDefault(),


                                        countP = g.Where(t => t.I.invType == "p").Count(),
                                        countS = g.Where(t => t.I.invType == "s").Count(),
                                        totalS = g.Where(t => t.I.invType == "s").Sum(S => S.I.total),
                                        totalNetS = g.Where(t => t.I.invType == "s").Sum(S => S.I.totalNet),
                                        totalP = g.Where(t => t.I.invType == "p").Sum(S => S.I.total),
                                        totalNetP = g.Where(t => t.I.invType == "p").Sum(S => S.I.totalNet),
                                        paid = g.Sum(S => S.I.paid),
                                        deserved = g.Sum(S => S.I.deserved),
                                        discountValue = g.Sum(S => (S.I.discountType == "1" ? S.I.discountValue : (S.I.discountType == "2" ? (S.I.discountValue / 100) : 0))
                                         + ((S.I.manualDiscountType == "1" || S.I.discountType == null) ? S.I.manualDiscountValue : (S.I.manualDiscountType == "2" ? ((S.I.manualDiscountValue / 100) * S.I.total) : 0))
                                            ),
                                    }).ToList();

                    return TokenManager.GenerateToken(invListm);
                }

            }
        }
        // add or update bank
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
                string invoiceObject = "";
                invoices newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        invoiceObject = c.Value.Replace("\\", string.Empty);
                        invoiceObject = invoiceObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<invoices>(invoiceObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                        break;
                    }
                }
                try
                {
                    invoices tmpInvoice;
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var invoiceEntity = entity.Set<invoices>();
                        if (newObject.invoiceMainId == 0)
                            newObject.invoiceMainId = null;

                        if (newObject.invoiceId == 0)
                        {
                            newObject.invDate = countryc.AddOffsetTodate(DateTime.Now);
                            newObject.invTime = DateTime.Now.TimeOfDay.Add(countryc.offsetTime());
                            newObject.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                            newObject.updateUserId = newObject.createUserId;
                            newObject.isActive = true;
                            newObject.isOrginal = true;
                            tmpInvoice = invoiceEntity.Add(newObject);
                            entity.SaveChanges();
                            message = tmpInvoice.invoiceId.ToString();

                            return TokenManager.GenerateToken(message);
                        }
                        else
                        {
                            tmpInvoice = entity.invoices.Where(p => p.invoiceId == newObject.invoiceId).FirstOrDefault();
                            tmpInvoice.invNumber = newObject.invNumber;
                            tmpInvoice.agentId = newObject.agentId;
                            tmpInvoice.invType = newObject.invType;
                            tmpInvoice.total = newObject.total;
                            tmpInvoice.totalNet = newObject.totalNet;
                            tmpInvoice.paid = newObject.paid;
                            tmpInvoice.deserved = newObject.deserved;
                            tmpInvoice.deservedDate = newObject.deservedDate;
                            tmpInvoice.invoiceMainId = newObject.invoiceMainId;
                            tmpInvoice.invCase = newObject.invCase;
                            tmpInvoice.notes = newObject.notes;
                            tmpInvoice.itemtax_note = newObject.itemtax_note;
                            tmpInvoice.sales_invoice_note = newObject.sales_invoice_note;
                            tmpInvoice.vendorInvNum = newObject.vendorInvNum;
                            tmpInvoice.vendorInvDate = newObject.vendorInvDate;
                            tmpInvoice.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                            tmpInvoice.updateUserId = newObject.updateUserId;
                            tmpInvoice.branchId = newObject.branchId;
                            tmpInvoice.discountType = newObject.discountType;
                            tmpInvoice.discountValue = newObject.discountValue;
                            tmpInvoice.tax = newObject.tax;
                            tmpInvoice.taxtype = newObject.taxtype;
                            tmpInvoice.name = newObject.name;
                            tmpInvoice.isApproved = newObject.isApproved;
                            tmpInvoice.branchCreatorId = newObject.branchCreatorId;
                            tmpInvoice.shippingCompanyId = newObject.shippingCompanyId;
                            tmpInvoice.shipUserId = newObject.shipUserId;
                            tmpInvoice.userId = newObject.userId;
                            tmpInvoice.manualDiscountType = newObject.manualDiscountType;
                            tmpInvoice.manualDiscountValue = newObject.manualDiscountValue;
                            tmpInvoice.cashReturn = newObject.cashReturn;
                            tmpInvoice.shippingCost = newObject.shippingCost;
                            tmpInvoice.realShippingCost = newObject.realShippingCost;
                            tmpInvoice.sliceId = newObject.sliceId;
                            tmpInvoice.sliceName = newObject.sliceName;
                            tmpInvoice.isFreeShip = newObject.isFreeShip;
                            tmpInvoice.VATValue = newObject.VATValue;
                            entity.SaveChanges();
                            message = tmpInvoice.invoiceId.ToString();
                            return TokenManager.GenerateToken(message);
                        }
                    }
                }

                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }
            }
        }
        [HttpPost]
        [Route("savePurchaseBounce")]
        public async Task<string> savePurchaseBounce(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string Object = "";
                int posId = 0;
                int branchId = 0;
                invoices newObject = null;
                NotificationUserModel notificationUser = null;
                notification notification = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> billDetails = new List<ItemTransferModel>();
                List<cashTransfer> listPayments = new List<cashTransfer>();
                cashTransfer PosCashTransfer = new cashTransfer();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Object = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        billDetails = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "listPayments")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        listPayments = JsonConvert.DeserializeObject<List<cashTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "posCashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        PosCashTransfer = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }

                    else if (c.Type == "notification")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        notificationUser = JsonConvert.DeserializeObject<NotificationUserModel>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        notification = JsonConvert.DeserializeObject<notification>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }

                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    ItemsTransferController it = new ItemsTransferController();
                    ItemsUnitsController iuc = new ItemsUnitsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        #region caculate available amount in basic invoice
                        //get last sales invoice   
                        var lastMainInvId = entity.invoices.Where(x => x.invNumber == entity.invoices.Where(i => i.invoiceId == newObject.invoiceMainId).FirstOrDefault().invNumber).Max(x => x.invoiceId);

                        //sales invoice items
                        var mainInvoiceItems = await it.Get(lastMainInvId);

                        var returnedItems = billDetails.Select(x => x.itemId).Distinct().ToList();
                        foreach (var item in returnedItems)
                        {
                            var returnedItemUnits = billDetails.Where(x => x.itemId == item).Select(x => new { x.itemUnitId, x.quantity }).ToList();
                            var saledItemUnits = mainInvoiceItems.Where(x => x.itemId == item).ToList();
                            int returnedQuantity = 0;
                            int saledQuantity = 0;

                            foreach (var itemUnit in returnedItemUnits)
                            {
                                int multiplyFactor = iuc.multiplyFactorWithSmallestUnit((int)item, (int)itemUnit.itemUnitId);
                                returnedQuantity += multiplyFactor * (int)itemUnit.quantity;
                            }

                            foreach (var itemUnit in saledItemUnits)
                            {
                                int multiplyFactor = iuc.multiplyFactorWithSmallestUnit((int)item, (int)itemUnit.itemUnitId);
                                saledQuantity += multiplyFactor * (int)itemUnit.quantity;
                            }
                            if (returnedQuantity > saledQuantity)
                            {
                                message = "-9";
                                result += "Result:" + message;
                                result += "}";
                                return TokenManager.GenerateToken(result);
                            }
                        }

                        #endregion

                        #region check properties in store
                        bool propCheck = it.ReturnedPropAmountsAvailable(billDetails, branchId);

                        if (!propCheck)
                        {
                            message = "-10";
                            result += "Result:" + message;

                            result += "}";

                            return TokenManager.GenerateToken(result);
                        }
                        #endregion
                        #region check items quantity in store
                        ItemsLocationsController itc = new ItemsLocationsController();
                        string res = itc.checkItemsAmounts(billDetails, branchId);

                        if (!res.Equals(""))
                        {
                            message = "-3";
                            result += "Result:" + message;

                            res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                            result += ",Message:'" + res + "'";
                            result += "}";

                            return TokenManager.GenerateToken(result);
                        }
                        #endregion

                        #region check customer balance
                        //if (newObject.agentId != null)
                        //{
                        //    int agentId = (int)newObject.agentId;
                        //    agents agent = entity.agents.Where(b => b.agentId == agentId).FirstOrDefault();

                        //    foreach (var pay in listPayments)
                        //    {

                        //        if (pay.processType == "balance" &&
                        //            (newObject.shippingCompanyId == null || (newObject.shippingCompanyId != null && newObject.shipUserId != null))
                        //            && newObject.agentId != null)
                        //        {

                        //            if (!(
                        //                (agent.isLimited == true && agent.maxDeserve == 0) ||
                        //            (agent.isLimited == true && agent.balanceType == 0 && agent.maxDeserve >= newObject.totalNet - newObject.paid - agent.balance) ||
                        //            (agent.isLimited == true && agent.balanceType == 1 && agent.maxDeserve >= newObject.totalNet - newObject.paid + agent.balance) ||
                        //             (agent.isLimited == false && agent.balanceType == 0 && (decimal)agent.balance >= newObject.totalNet - newObject.paid)
                        //             ))

                        //            {

                        //                message = "-4";
                        //                result += "Result:" + message;
                        //                result += "}";
                        //                return TokenManager.GenerateToken(result);

                        //            }

                        //        }
                        //    }
                        //}
                        #endregion


                        newObject = await SaveInvoice(newObject);
                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;
                        if (!invoiceId.Equals(0))
                        {
                            #region save updated purchase invoice
                            var purInvoice = entity.invoices.Where(x => x.invoiceId == lastMainInvId).FirstOrDefault();

                            var newPurchaseInv = new invoices();
                            #region new sales invoice object
                            newPurchaseInv.invNumber = purInvoice.invNumber;
                            newPurchaseInv.invoiceMainId = lastMainInvId;
                            newPurchaseInv.agentId = purInvoice.agentId;
                            newPurchaseInv.invType = purInvoice.invType;
                            newPurchaseInv.total = purInvoice.total;
                            newPurchaseInv.totalNet = purInvoice.totalNet;
                            newPurchaseInv.paid = purInvoice.paid;
                            newPurchaseInv.deserved = purInvoice.deserved;
                            newPurchaseInv.deservedDate = purInvoice.deservedDate;
                            newPurchaseInv.invCase = purInvoice.invCase;
                            newPurchaseInv.notes = purInvoice.notes;
                            newPurchaseInv.vendorInvNum = purInvoice.vendorInvNum;
                            newPurchaseInv.vendorInvDate = purInvoice.vendorInvDate;
                            newPurchaseInv.invTime = purInvoice.invTime;
                            newPurchaseInv.invDate = purInvoice.invDate;
                            newPurchaseInv.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                            newPurchaseInv.createUserId = purInvoice.createUserId;
                            newPurchaseInv.updateUserId = purInvoice.updateUserId;
                            newPurchaseInv.discountType = purInvoice.discountType;
                            newPurchaseInv.discountValue = purInvoice.discountValue;
                            newPurchaseInv.tax = purInvoice.tax;
                            newPurchaseInv.taxtype = purInvoice.taxtype;
                            newPurchaseInv.taxValue = purInvoice.taxValue;
                            newPurchaseInv.name = purInvoice.name;
                            newPurchaseInv.isApproved = purInvoice.isApproved;
                            newPurchaseInv.branchCreatorId = purInvoice.branchCreatorId;
                            newPurchaseInv.branchId = purInvoice.branchId;
                            newPurchaseInv.posId = purInvoice.posId;
                            newPurchaseInv.shippingCompanyId = purInvoice.shippingCompanyId;
                            newPurchaseInv.shipUserId = purInvoice.shipUserId;
                            newPurchaseInv.userId = purInvoice.userId;
                            newPurchaseInv.manualDiscountType = purInvoice.manualDiscountType;
                            newPurchaseInv.manualDiscountValue = purInvoice.manualDiscountValue;
                            newPurchaseInv.cashReturn = purInvoice.cashReturn;
                            newPurchaseInv.shippingCost = purInvoice.shippingCost;
                            newPurchaseInv.realShippingCost = purInvoice.realShippingCost;
                            newPurchaseInv.isPrePaid = purInvoice.isPrePaid;
                            newPurchaseInv.isActive = true;
                            newPurchaseInv.sales_invoice_note = purInvoice.sales_invoice_note;
                            newPurchaseInv.itemtax_note = purInvoice.itemtax_note;
                            newPurchaseInv.sliceId = purInvoice.sliceId;
                            newPurchaseInv.sliceName = purInvoice.sliceName;
                            #endregion
                            newPurchaseInv = entity.invoices.Add(newPurchaseInv);
                            entity.SaveChanges();

                            #endregion

                            #region save return invoice items
                            it.saveReturnPurItems(transferObject, billDetails, invoiceId, newPurchaseInv.invoiceId,(int)newObject.branchId, mainInvoiceItems);

                            #endregion

                            #region save pos cash transfer
                            CashTransferController cc = new CashTransferController();

                            PosCashTransfer.invId = invoiceId;

                            await cc.addCashTransfer(PosCashTransfer);
                            #endregion

                            #region save payments
                            decimal paid = 0;
                            decimal deserved = 0;

                            foreach (var item in listPayments)
                            {
                                await ConfiguredSalesCashTrans(newObject, item, posId);

                                if (item.processType != "balance")
                                {
                                    paid += (decimal)item.cash;
                                    deserved += (decimal)item.cash;
                                }
                            }
                            var inv = entity.invoices.Find(invoiceId);
                            inv.paid += paid;
                            inv.deserved -= deserved;
                            entity.SaveChanges();

                            foreach (var item in listPayments)
                            {
                                if (item.processType == "balance")
                                {
                                    var basicInvId = entity.invoices.Where(x => x.invNumber == entity.invoices.Where(i => i.invoiceId == newObject.invoiceMainId).FirstOrDefault().invNumber).Min(x => x.invoiceId);
                                    var basicInv = entity.invoices.Find(basicInvId);
                                    var returnInv = entity.invoices.Find(invoiceId);

                                    decimal salesPaid = 0;
                                    if (basicInv.deserved >= item.cash)
                                        salesPaid = (decimal)item.cash;
                                    else
                                    {
                                        salesPaid = (decimal)basicInv.deserved;
                                        //decrease agent balance
                                        var agent = entity.agents.Find(newObject.agentId);
                                        decimal newBalance = 0;
                                        if (agent.balanceType == 0)
                                        {
                                            if (salesPaid <= (decimal)agent.balance)
                                            {
                                                newBalance = (decimal)agent.balance - (decimal)item.cash;
                                                agent.balance = newBalance;
                                            }
                                            else
                                            {
                                                newBalance = (decimal)item.cash - (decimal)agent.balance;
                                                agent.balance = newBalance;
                                                agent.balanceType = 1;
                                            }


                                        }
                                        else if (agent.balanceType == 1)
                                        {
                                            newBalance = (decimal)agent.balance + (decimal)item.cash;
                                            agent.balance = newBalance;
                                        }
                                    }

                                    basicInv.deserved -= salesPaid;
                                    basicInv.paid += salesPaid;

                                    returnInv.deserved -= salesPaid;
                                    returnInv.paid += salesPaid;
                                    entity.SaveChanges();

                                    break;
                                }
                            }
                            #endregion
                            #region save notification
                            NotificationController nc = new NotificationController();
                            notification.updateUserId = notification.createUserId;

                            nc.save(notification, notificationUser.objectName, notificationUser.prefix, (int)notificationUser.branchId);
                            #endregion
                        }
                    }
                }

                catch
                {
                    message = "0";
                }
                result += "Result:" + message;
                string temp = System.Web.Helpers.Json.Encode(newObject.invNumber).Substring(1, System.Web.Helpers.Json.Encode(newObject.invNumber).Length - 2);
                result += ",Message:'" + temp + "'";

                #region get sales draft count
                List<string> invoiceType = new List<string>() { "pd ", "pbd" };
                int draftCount = getDraftCount((int)newObject.updateUserId, invoiceType);

                result += ",PurchaseDraftCount:" + draftCount;
                #endregion

                #region return pos Balance
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var pos = entity.pos.Find(posId);
                    result += ",PosBalance:" + pos.balance;
                }
                #endregion

                result += "}";
                return TokenManager.GenerateToken(result);

            }
        }
        [HttpPost]
        [Route("savePurchaseDraft")]
        public async Task<string> savePurchaseDraft(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
            
          
                string Object = "";
                int posId = 0;
                invoices newObject = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> transferObjectModel = new List<ItemTransferModel>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        //Object = c.Value.Replace("\\", string.Empty);
                        //Object = Object.Trim('"');
                        Object = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        transferObjectModel = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }

                }
             
                #endregion
              
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        if (newObject.invoiceMainId == 0)
                            newObject.invoiceMainId = null;

                        newObject = await SaveInvoice(newObject);

                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;
                        if (!invoiceId.Equals(0))
                        {
                            //save items transfer
                            ItemsTransferController it = new ItemsTransferController();
                            it.saveWithSerials(transferObject, transferObjectModel, invoiceId,(int)newObject.branchId, false, false, 0);
                        }
                    }
                }

                catch(Exception ex)
                {
                       // return TokenManager.GenerateToken(ex.ToString());
                         message = "0";
                }
                result += "Result:" + message;
                string temp = System.Web.Helpers.Json.Encode(newObject.invNumber).Substring(1, System.Web.Helpers.Json.Encode(newObject.invNumber).Length - 2);
                result += ",Message:'" + temp + "'";
                result += ",InvTime:'" + newObject.invTime + "'";
                result += ",UpdateDate:'" + DateTime.Parse(newObject.updateDate.ToString()).ToString() + "'";

                #region get purchase draft count
                List<string> invoiceType = new List<string>() { "pd", "pbd" };
                int draftCount = 0;
                if (!invoiceType.Contains(newObject.invType))
                {
                    invoiceType = new List<string>() { "pod", "pos" };

                    if (!invoiceType.Contains(newObject.invType))
                        invoiceType = new List<string>() { "isd" };
                }

                draftCount = getDraftCount((int)newObject.createUserId, invoiceType);

                result += ",PurchaseDraftCount:" + draftCount;
                #endregion

                #region return pos Balance
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var pos = entity.pos.Find(posId);
                    result += ",PosBalance:" + pos.balance;
                }
                #endregion

                result += "}";
                return TokenManager.GenerateToken(result);
                    //               
            }
        }

        [HttpPost]
        [Route("saveWithItems")]
        public async Task<string> saveWithItems(string token)
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
                #region parameters
                string Object = "";
                int posId = 0;
                invoices newObject = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        //Object = c.Value.Replace("\\", string.Empty);
                        //Object = Object.Trim('"');
                        Object = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }

                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        if (newObject.invoiceMainId == 0)
                            newObject.invoiceMainId = null;



                        newObject = await SaveInvoice(newObject);

                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;
                        if (!invoiceId.Equals(0))
                        {
                            //save items transfer
                            ItemsTransferController it = new ItemsTransferController();
                            it.save(transferObject, invoiceId);
                        }
                    }
                }

                catch
                {
                    message = "0";
                }

                return TokenManager.GenerateToken(message);

            }
        }
        [HttpPost]
        [Route("SaveImportOrder")]
        public async Task<string> SaveImportOrder(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string Object = "";
                bool final = false;
                invoices newObject = null;
                invoices sentExportInvoice = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> itemsTransfer = new List<ItemTransferModel>();
                NotificationUserModel notUser = new NotificationUserModel();
                notification not = new notification();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<invoices>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "exportInvoice")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        sentExportInvoice = JsonConvert.DeserializeObject<invoices>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        itemsTransfer = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "not")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        notUser = JsonConvert.DeserializeObject<NotificationUserModel>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        not = JsonConvert.DeserializeObject<notification>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "final")
                        final = bool.Parse(c.Value);

                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        //save import invoice
                        newObject = await AddImportInvoice(newObject, sentExportInvoice, transferObject,itemsTransfer, notUser, not, final);

                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;

                    }
                }

                catch
                {
                    message = "0";
                }
                result += "Result:" + message;
                string temp = System.Web.Helpers.Json.Encode(newObject.invNumber).Substring(1, System.Web.Helpers.Json.Encode(newObject.invNumber).Length - 2);
                result += ",Message:'" + temp + "'";

                #region get sales draft count
                List<string> invoiceType = new List<string>() { "imd ", "exd" };
                int draftCount = getDraftCount((int)newObject.updateUserId, invoiceType);
                result += ",ImExpDraftCount:" + draftCount;
                #endregion


                result += "}";
                return TokenManager.GenerateToken(result);

            }
        }
        [HttpPost]
        [Route("GenerateExport")]
        public async Task<string> GenerateExport(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string Object = "";
                string notObject = "";
                int branchId = 0;
                int toBranch = 0;
                int userId = 0;
                bool final = false;
                invoices newObject = null;
                invoices sentExportInvoice = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> billDetails = new List<ItemTransferModel>();
                List<itemsLocations> itemsLoc = new List<itemsLocations>();
                NotificationUserModel notUser = new NotificationUserModel();
                notification not = new notification();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<invoices>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "exportInvoice")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        sentExportInvoice = JsonConvert.DeserializeObject<invoices>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        billDetails = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "not")
                    {
                        notObject = c.Value.Replace("\\", string.Empty);
                        notObject = notObject.Trim('"');
                        notUser = JsonConvert.DeserializeObject<NotificationUserModel>(notObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        not = JsonConvert.DeserializeObject<notification>(notObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "ItemsLoc")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        itemsLoc = JsonConvert.DeserializeObject<List<itemsLocations>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "final")
                    {
                        final = bool.Parse(c.Value);
                    }

                }
                #endregion
                try
                {
                    ItemsTransferController it = new ItemsTransferController();

                    #region check items quantity in store
                    if (final == true)
                    {
                        ItemsLocationsController itc = new ItemsLocationsController();
                        toBranch = (int)notUser.branchId;

                        string res = itc.checkItemsAmounts(billDetails, branchId);

                        if (!res.Equals(""))
                        {
                            message = "-3";
                            result += "Result:" + message;

                            res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                            result += ",Message:'" + res + "'";
                            result += "}";

                            return TokenManager.GenerateToken(result);
                        }
                    }
                    #endregion

                    #region check properties in store
                    bool propCheck = it.PropertiesAmountsAvailable(billDetails, branchId);

                    if (!propCheck)
                    {
                        message = "-10";
                        result += "Result:" + message;

                        result += "}";

                        return TokenManager.GenerateToken(result);
                    }
                    #endregion

                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        //save import invoice
                        newObject = await AddImportInvoice(newObject, sentExportInvoice, transferObject,billDetails,null,null,final);

                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;

                        //reciept invoice
                        if (final == true)
                        {
                            ItemsLocationsController ilc = new ItemsLocationsController();
                            //ilc.receiptOrder(itemsLoc, billDetails, toBranch, userId, notUser.objectName, notObject);
                            ilc.transferQuantity(itemsLoc, billDetails, toBranch, userId, notUser.objectName, notObject);
                        }
                    }
                }

                catch
                {
                    message = "0";
                }
                result += "Result:" + message;
                string temp = System.Web.Helpers.Json.Encode(newObject.invNumber).Substring(1, System.Web.Helpers.Json.Encode(newObject.invNumber).Length - 2);
                result += ",Message:'" + temp + "'";

                #region get sales draft count
                List<string> invoiceType = new List<string>() { "imd ", "exd" };
                int draftCount = getDraftCount((int)newObject.updateUserId, invoiceType);
                result += ",ImExpDraftCount:" + draftCount;
                #endregion


                result += "}";
                return TokenManager.GenerateToken(result);

            }
        }

        [HttpPost]
        [Route("AcceptWaitingImport")]
        public async Task<string> AcceptWaitingImport(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string Object = "";
                string notObject = "";
                int branchId = 0;
                int toBranch = 0;
                int userId = 0;
                invoices newObject = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> billDetails = new List<ItemTransferModel>();
                List<itemsLocations> itemsLoc = new List<itemsLocations>();
                NotificationUserModel notUser = new NotificationUserModel();
                notification not = new notification();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        //Object = c.Value.Replace("\\", string.Empty);
                        //Object = Object.Trim('"');
                        Object = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }

                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        billDetails = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "ItemsLoc")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        itemsLoc = JsonConvert.DeserializeObject<List<itemsLocations>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "not")
                    {
                        notObject = c.Value.Replace("\\", string.Empty);
                        notObject = notObject.Trim('"');
                        notUser = JsonConvert.DeserializeObject<NotificationUserModel>(notObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }

                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    ItemsTransferController it = new ItemsTransferController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        #region check items quantity in store
                        ItemsLocationsController itc = new ItemsLocationsController();
                        toBranch = (int)notUser.branchId;

                        string res = itc.checkItemsAmounts(billDetails, branchId);

                        if (!res.Equals(""))
                        {
                            message = "-3";
                            result += "Result:" + message;

                            res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                            result += ",Message:'" + res + "'";
                            result += "}";

                            return TokenManager.GenerateToken(result);
                        }
                        #endregion

                        #region check properties in store
                        bool propCheck = it.PropertiesAmountsAvailable(billDetails, branchId);

                        if (!propCheck)
                        {
                            message = "-10";
                            result += "Result:" + message;

                            result += "}";

                            return TokenManager.GenerateToken(result);
                        }
                        #endregion
                        //edit export invoice
                        newObject = await SaveInvoice(newObject);

                        message = newObject.invoiceId.ToString();

                        int invoiceId = newObject.invoiceId;

                        if (!invoiceId.Equals(0))
                        {                          
                            int exportBranchId = (int)entity.invoices.Find(newObject.invoiceMainId).branchId;
                            int importBranchId = (int)newObject.branchId;
                            it.saveImExItems(transferObject,billDetails, newObject.invoiceId, (int)newObject.invoiceMainId,exportBranchId,importBranchId);

                            //reciept invoice
                            ItemsLocationsController ilc = new ItemsLocationsController();
                           // ilc.receiptOrder(itemsLoc, billDetails, toBranch, userId, notUser.objectName, notObject);
                            ilc.transferQuantity(itemsLoc, billDetails, toBranch, userId, notUser.objectName, notObject);

                        }
                    }
                }

                catch
                {
                    message = "0";
                }
                result += "Result:" + message;
                string temp = System.Web.Helpers.Json.Encode(newObject.invNumber).Substring(1, System.Web.Helpers.Json.Encode(newObject.invNumber).Length - 2);
                result += ",Message:'" + temp + "'";


                result += "}";
                return TokenManager.GenerateToken(result);

            }
        }

        [HttpPost]
        [Route("savePurchaseInvoice")]
        public async Task<string> savePurchaseInvoice(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string invoiceObject = "";
                string amountNotStr = "";
                string waitNotStr = "";
                string Object = "";
                int posId = 0;

                invoices newObject = null;
                NotificationUserModel amountNot = null;
                NotificationUserModel waitNotUser = null;
                notification waitNot = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> transferObjectModel = new List<ItemTransferModel>();
                cashTransfer PosCashTransfer = null;
                List<cashTransfer> listPayments = new List<cashTransfer>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        //invoiceObject = c.Value.Replace("\\", string.Empty);
                        //invoiceObject = invoiceObject.Trim('"');
                        invoiceObject = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(invoiceObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        transferObjectModel = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "amountNot")
                    {
                        amountNotStr = c.Value.Replace("\\", string.Empty);
                        amountNotStr = amountNotStr.Trim('"');
                        amountNot = JsonConvert.DeserializeObject<NotificationUserModel>(amountNotStr, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "waitNot")
                    {
                        waitNotStr = c.Value.Replace("\\", string.Empty);
                        waitNotStr = waitNotStr.Trim('"');
                        waitNotUser = JsonConvert.DeserializeObject<NotificationUserModel>(waitNotStr, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        waitNot = JsonConvert.DeserializeObject<notification>(waitNotStr, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "PosCashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        PosCashTransfer = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "listPayments")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        listPayments = JsonConvert.DeserializeObject<List<cashTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }

                }
                #endregion

               try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        #region check pos balance
                        var pos = entity.pos.Find(posId);
                        foreach (var c in listPayments)
                        {
                            if (c.processType == "cash" && pos.balance < c.cash)
                            {
                                message = "-2";
                                result += "Result:" + message;
                                result += "}";
                                return TokenManager.GenerateToken(result);

                            }
                        }
                        #endregion


                        newObject = await SaveInvoice(newObject);
                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;
                        if (!invoiceId.Equals(0))
                        {
                            //save items transfer
                            ItemsTransferController it = new ItemsTransferController();
                            it.saveWithSerials(transferObject, transferObjectModel, invoiceId, (int)newObject.branchId, false, false);
                           // it.save(transferObject, invoiceId);

                            #region enter items to store and notification

                            if (newObject.branchCreatorId.Equals(newObject.branchId))
                            {
                                ItemsLocationsController ilc = new ItemsLocationsController();
                                await ilc.recieptItems((int)newObject.branchId, transferObjectModel, (int)newObject.updateUserId, amountNot.objectName, amountNotStr);
                                saveAvgPrice(transferObject);
                            }
                            else
                            {
                                NotificationController nc = new NotificationController();
                                nc.save(waitNot, waitNotUser.objectName, waitNotUser.prefix, (int)waitNotUser.branchId);
                            }
                            #endregion

                            #region save payments
                            #region save pos cash transfer
                            CashTransferController cc = new CashTransferController();

                            PosCashTransfer.invId = invoiceId;
                            //PosCashTransfer.transNum = await cc.generateCashNumber(PosCashTransfer.transNum);

                            await cc.addCashTransfer(PosCashTransfer);
                            #endregion
                            var inv = entity.invoices.Find(invoiceId);

                            foreach (var item in listPayments)
                            {
                                item.invId = invoiceId;
                                await savePurchaseCash(newObject, item, posId);
                            }

                            #endregion
                        }


                    }
                }

                catch
                {
                    message = "0";
                }
                result += "Result:" + message;
                string temp = System.Web.Helpers.Json.Encode(newObject.invNumber).Substring(1, System.Web.Helpers.Json.Encode(newObject.invNumber).Length - 2);
                result += ",Message:'" + temp + "'";
                result += ",InvTime:'" + newObject.invTime + "'";
                result += ",UpdateDate:'" + DateTime.Parse(newObject.updateDate.ToString()).ToString() + "'";
                #region get purchase draft count
                List<string> invoiceType = new List<string>() { "pd ", "pbd" };
                int draftCount = getDraftCount((int)newObject.updateUserId, invoiceType);

                result += ",PurchaseDraftCount:" + draftCount;
                #endregion

                #region return pos Balance
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var pos = entity.pos.Find(posId);
                    result += ",PosBalance:" + pos.balance;
                }
                #endregion
                result += "}";
                return TokenManager.GenerateToken(result);

            }
        }

        [HttpPost]
        [Route("saveDirectEntry")]
        public async Task<string> saveDirectEntry(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string invoiceObject = "";
                string amountNotStr = "";
                string Object = "";
                int posId = 0;

                invoices newObject = null;
                NotificationUserModel amountNot = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> billDetails = new List<ItemTransferModel>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        //invoiceObject = c.Value.Replace("\\", string.Empty);
                        //invoiceObject = invoiceObject.Trim('"');
                        invoiceObject = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(invoiceObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        billDetails = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "amountNot")
                    {
                        amountNotStr = c.Value.Replace("\\", string.Empty);
                        amountNotStr = amountNotStr.Trim('"');
                        amountNot = JsonConvert.DeserializeObject<NotificationUserModel>(amountNotStr, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }

                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        newObject = await SaveInvoice(newObject);
                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;
                        if (!invoiceId.Equals(0))
                        {
                            //save items transfer
                            ItemsTransferController it = new ItemsTransferController();
                            it.saveWithSerials(transferObject,billDetails, invoiceId,(int)newObject.branchId,false,false);

                            #region enter items to store and notification


                            ItemsLocationsController ilc = new ItemsLocationsController();
                            await ilc.recieptItems((int)newObject.branchId, billDetails, (int)newObject.updateUserId, amountNot.objectName, amountNotStr);
                            saveAvgPrice(transferObject);

                            #endregion

                        }


                    }
                }

                catch
                {
                    message = "0";
                }
                result += "Result:" + message;
                string temp = System.Web.Helpers.Json.Encode(newObject.invNumber).Substring(1, System.Web.Helpers.Json.Encode(newObject.invNumber).Length - 2);
                result += ",Message:'" + temp + "'";
                result += ",InvTime:'" + newObject.invTime + "'";
                result += ",UpdateDate:'" + DateTime.Parse(newObject.updateDate.ToString()).ToString() + "'";
                #region get purchase draft count
                List<string> invoiceType = new List<string>() { "isd" };
                int draftCount = getDraftCount((int)newObject.updateUserId, invoiceType);

                result += ",DirectStorageDraftCount:" + draftCount;
                #endregion

                result += "}";
                return TokenManager.GenerateToken(result);

            }
        }

        [HttpPost]
        [Route("recieptWaitingPurchase")]
        public async Task<string> recieptWaitingPurchase(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string invoiceObject = "";
                string amountNotStr = "";
                string Object = "";
                int branchId = 0;

                invoices newObject = null;
                NotificationUserModel amountNot = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> transferObjectModel = new List<ItemTransferModel>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        //invoiceObject = c.Value.Replace("\\", string.Empty);
                        //invoiceObject = invoiceObject.Trim('"');
                        invoiceObject = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(invoiceObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        transferObjectModel = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "amountNot")
                    {
                        amountNotStr = c.Value.Replace("\\", string.Empty);
                        amountNotStr = amountNotStr.Trim('"');
                        amountNot = JsonConvert.DeserializeObject<NotificationUserModel>(amountNotStr, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }

                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }

                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        newObject = await SaveInvoice(newObject);
                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;
                        if (!invoiceId.Equals(0))
                        {

                            #region enter items to store and notification


                            ItemsLocationsController ilc = new ItemsLocationsController();
                            await ilc.recieptItems(branchId, transferObjectModel, (int)newObject.updateUserId, amountNot.objectName, amountNotStr);
                            saveAvgPrice(transferObject);

                            #endregion

                        }


                    }
                }

                catch
                {
                    message = "0";
                }
                result += "Result:" + message;
                string temp = System.Web.Helpers.Json.Encode(newObject.invNumber).Substring(1, System.Web.Helpers.Json.Encode(newObject.invNumber).Length - 2);
                result += ",Message:'" + temp + "'";
                result += ",InvTime:'" + newObject.invTime + "'";
                result += ",UpdateDate:'" + DateTime.Parse(newObject.updateDate.ToString()).ToString() + "'";

                #region get direct storage draft count
                List<string> invoiceType = new List<string>() { "isd" };
                int draftCount = getDraftCount((int)newObject.updateUserId, invoiceType);

                result += ",PurchaseDraftCount:" + draftCount;
                #endregion


                result += "}";
                return TokenManager.GenerateToken(result);

            }
        }

        [HttpPost]
        [Route("returnPurInvoice")]
        public async Task<string> returnPurInvoice(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string invoiceObject = "";
                string amountNotStr = "";
                string Object = "";
                int branchId = 0;
                int posId = 0;

                invoices newObject = null;
                invoiceStatus invoiceStatus = null;
                NotificationUserModel amountNot = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> billDetails = new List<ItemTransferModel>();
                List<itemsLocations> readyItemsLoc = new List<itemsLocations>();
                cashTransfer PosCashTransfer = null;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        //invoiceObject = c.Value.Replace("\\", string.Empty);
                        //invoiceObject = invoiceObject.Trim('"');
                        invoiceObject = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(invoiceObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        billDetails = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "invoiceStatus")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        invoiceStatus = JsonConvert.DeserializeObject<invoiceStatus>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "amountNot")
                    {
                        amountNotStr = c.Value.Replace("\\", string.Empty);
                        amountNotStr = amountNotStr.Trim('"');
                        amountNot = JsonConvert.DeserializeObject<NotificationUserModel>(amountNotStr, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "PosCashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        PosCashTransfer = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "readyItemsLoc")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        readyItemsLoc = JsonConvert.DeserializeObject<List<itemsLocations>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }

                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }

                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        #region check items quantity in store
                        ItemsLocationsController itc = new ItemsLocationsController();
                        string res = itc.checkItemsAmounts(billDetails, branchId);

                        if (!res.Equals(""))
                        {
                            message = "-3";
                            result += "Result:" + message;

                            res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                            result += ",Message:'" + res + "'";
                            result += "}";

                            return TokenManager.GenerateToken(result);
                        }
                        #endregion

                        newObject = await SaveInvoice(newObject);
                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;
                        if (!invoiceId.Equals(0))
                        {
                            //save items transfer
                            //ItemsTransferController it = new ItemsTransferController();
                            //it.save(transferObject, invoiceId);

                            #region decrease amount 
                            foreach (var row in billDetails)
                            {
                                var quantity = row.quantity;
                                var unitLocations = readyItemsLoc.Where(x => x.itemUnitId == row.itemUnitId).ToList();

                                foreach (itemsLocations item in unitLocations)
                                {
                                    itemsLocations itemL = new itemsLocations();

                                    itemL = entity.itemsLocations.Find(item.itemsLocId);
                                    if (quantity < itemL.quantity)
                                    {
                                        itemL.quantity -= quantity;
                                        quantity = 0;
                                    }
                                    else
                                    {
                                        quantity -= itemL.quantity;
                                        itemL.quantity = 0;
                                    }
                                    itemL.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                                    itemL.updateUserId = newObject.updateUserId;
                                    entity.SaveChanges();

                                    if (quantity == 0)
                                        break;
                                }
                              
                            }
                            #endregion

                            //#region save pos cash transfer
                            //CashTransferController cc = new CashTransferController();

                            //PosCashTransfer.invId = invoiceId;
                            ////PosCashTransfer.transNum = await cc.generateCashNumber(PosCashTransfer.transNum);

                            //await cc.addCashTransfer(PosCashTransfer);
                            //#endregion

                            //#region save payments
                            //if (newObject.agentId != null)
                            //{
                            //    cashTransfer cashTrasnfer = new cashTransfer();
                            //    cashTrasnfer.cash = newObject.totalNet;
                            //    cashTrasnfer.processType = "balance";
                            //    await recordConfiguredAgentCash(newObject, "pb", cashTrasnfer, posId);
                            //}
                            //#endregion
                        }


                    }
                }

                catch
                {
                    message = "0";
                }
                result += "Result:" + message;
                string temp = System.Web.Helpers.Json.Encode(newObject.invNumber).Substring(1, System.Web.Helpers.Json.Encode(newObject.invNumber).Length - 2);
                result += ",Message:'" + temp + "'";
                result += ",InvTime:'" + newObject.invTime + "'";
                result += ",UpdateDate:'" + DateTime.Parse(newObject.updateDate.ToString()).ToString() + "'";
                #region get sales draft count
                result += ",PurchaseDraftCount:";

                List<string> invoiceType = new List<string>() { "isd" };
                int draftCount = getDraftCount((int)newObject.updateUserId, invoiceType);

                result += draftCount;
                #endregion
                result += "}";
                return TokenManager.GenerateToken(result);

            }
        }
        private async Task savePurchaseCash(invoices inv, cashTransfer cashTransfer, int posId)
        {
            CashTransferController cc = new CashTransferController();

            using (incposdbEntities entity = new incposdbEntities())
            {
                var invoice = entity.invoices.Find(inv.invoiceId);
                switch (cashTransfer.processType)
                {
                    case "cash":// cash: update pos balance  
                        var pos = entity.pos.Find(posId);
                        if (pos.balance > 0)
                        {
                            if (pos.balance >= cashTransfer.cash)
                            {
                                pos.balance -= cashTransfer.cash;
                                invoice.paid = cashTransfer.cash;
                                invoice.deserved -= cashTransfer.cash;
                            }
                            else
                            {
                                invoice.paid = pos.balance;
                                cashTransfer.cash = pos.balance;
                                invoice.deserved -= pos.balance;
                                pos.balance = 0;
                            }
                            entity.SaveChanges();
                            await cc.addCashTransfer(cashTransfer); //add cash transfer  
                        }
                        break;
                    case "balance":// balance: update customer balance
                        await recordConfiguredAgentCash(invoice, "pi", cashTransfer, posId);

                        break;
                    case "card": // card  
                        await cc.addCashTransfer(cashTransfer); //add cash transfer 
                        invoice.paid += cashTransfer.cash;
                        invoice.deserved -= cashTransfer.cash;
                        entity.SaveChanges();
                        break;
                }
            }
        }
        [HttpPost]
        [Route("saveSalesWithItems")]
        public async Task<string> saveSalesWithItems(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string Object = "";

                invoices newObject = null;
                InvoiceModel invoiceModel = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> billDetails = new List<ItemTransferModel>();
                List<couponsInvoices> couponsInvoices = new List<couponsInvoices>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Object = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                        invoiceModel = JsonConvert.DeserializeObject<InvoiceModel>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        billDetails = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "invoiceCoupons")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        couponsInvoices = JsonConvert.DeserializeObject<List<couponsInvoices>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        #region validate coupons
                        string res = "";
                        foreach (var coupon in couponsInvoices)
                        {
                            var c = entity.coupons.Find(coupon.couponId);
                            res = c.name;

                            DateTime datenow = DateTime.Now;
                            datenow = countryc.AddOffsetTodate(datenow);
                            #region check coupon effictive
                            if ((c.startDate > datenow && c.startDate != null) || (c.endDate < datenow && c.endDate != null) || c.isActive != 1)
                            {
                                message = "-8";
                                result += "Result:" + message;

                                res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                result += ",Message:'" + res + "'";
                                result += "}";

                                return TokenManager.GenerateToken(result);
                            }
                            #endregion
                            #region check coupon remain
                            if (c.remainQ <= 0 && !c.quantity.Equals(0))
                            {
                                message = "-7";
                                result += "Result:" + message;

                                res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                result += ",Message:'" + res + "'";
                                result += "}";

                                return TokenManager.GenerateToken(result);
                            }
                            #endregion
                            #region check invMax - invMin for coupon
                            if (c.invMax != 0 || c.invMin != 0)
                            {
                                if (newObject.total < c.invMin)
                                {
                                    message = "-5";
                                    result += "Result:" + message;

                                    res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                    result += ",Message:'" + res + "'";
                                    result += "}";

                                    return TokenManager.GenerateToken(result);
                                }
                                else if (newObject.total > c.invMax)
                                {
                                    message = "-6";
                                    result += "Result:" + message;

                                    res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                    result += ",Message:'" + res + "'";
                                    result += "}";

                                    return TokenManager.GenerateToken(result);
                                }
                            }
                            else if (c.invMax == 0)
                            {
                                if (newObject.total < c.invMin)
                                {
                                    message = "-5";
                                    result += "Result:" + message;

                                    res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                    result += ",Message:'" + res + "'";
                                    result += "}";

                                    return TokenManager.GenerateToken(result);
                                }
                            }
                            #endregion
                        }

                        #endregion

                        newObject = await SaveInvoice(newObject);
                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;
                        if (!invoiceId.Equals(0))
                        {
                            //save items transfer
                            ItemsTransferController it = new ItemsTransferController();
                            it.saveWithSerials(transferObject, billDetails, invoiceId, (int)newObject.branchId,false);

                            #region invoice taxes
                            if (invoiceModel.invoiceTaxes != null)
                                SaveInvoiceTaxes(invoiceModel.invoiceTaxes, invoiceId);
                            #endregion

                            #region coupons
                            couponsInvoicesController cc = new couponsInvoicesController();
                            cc.Save(couponsInvoices, newObject.invType, invoiceId);
                            #endregion
                        }

                    }
                }

                catch
                {
                    message = "0";
                }
                result += "Result:" + message;

                string temp = System.Web.Helpers.Json.Encode(newObject.invNumber).Substring(1, System.Web.Helpers.Json.Encode(newObject.invNumber).Length - 2);
                result += ",Message:'" + temp + "'";
                result += ",InvTime:'" + newObject.invTime + "'";
                result += ",UpdateDate:'" + DateTime.Parse(newObject.updateDate.ToString()).ToString() + "'";

                #region get sales draft count
                int draftCount = 0;
                List<string> invoiceType = null;
                result += ",SalesDraftCount:";
                if (salesType.Contains(newObject.invType))
                {
                    invoiceType = new List<string>() { "sd ", "sbd" };
                }
                else
                    invoiceType = new List<string>() { "qd", "qs" };

                draftCount = getDraftCount((int)newObject.updateUserId, invoiceType);

                result += draftCount;
                #endregion

                result += "}";
                return TokenManager.GenerateToken(result);

            }
        }
        [HttpPost]
        [Route("SaveSalesInvoice")]
        public async Task<string> SaveSalesInvoice(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string invoiceObject = "";
                string amountNotStr = "";
                string Object = "";
                int branchId = 0;
                int posId = 0;

                invoices newObject = null;
                InvoiceModel invoiceModel = null;
                invoiceStatus invoiceStatus = null;
                NotificationUserModel amountNot = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> billDetails = new List<ItemTransferModel>();
                cashTransfer PosCashTransfer = null;
                List<couponsInvoices> couponsInvoices = new List<couponsInvoices>();
                List<cashTransfer> listPayments = new List<cashTransfer>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        invoiceObject = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(invoiceObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                        invoiceModel = JsonConvert.DeserializeObject<InvoiceModel>(invoiceObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value;
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        billDetails = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "invoiceStatus")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        invoiceStatus = JsonConvert.DeserializeObject<invoiceStatus>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "amountNot")
                    {
                        amountNotStr = c.Value.Replace("\\", string.Empty);
                        amountNotStr = amountNotStr.Trim('"');
                        amountNot = JsonConvert.DeserializeObject<NotificationUserModel>(amountNotStr, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "PosCashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        PosCashTransfer = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "invoiceCoupons")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        couponsInvoices = JsonConvert.DeserializeObject<List<couponsInvoices>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "listPayments")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        listPayments = JsonConvert.DeserializeObject<List<cashTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }

                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    ItemsTransferController it = new ItemsTransferController();
                    CashTransferController ctc = new CashTransferController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        #region check upgrade
                        if (newObject.invoiceId == 0)
                        {

                            if (newObject.invType == "s")
                            {
                                ProgramInfo programInfo = new ProgramInfo();
                                int invMaxCount = programInfo.getSaleinvCount();
                                int salesInvCount = pc.getSalesInvCountInMonth();
                                if (salesInvCount >= invMaxCount && invMaxCount != -1)
                                {
                                    message = "-1";
                                    result += "Result:" + message;
                                    result += "}";
                                    return TokenManager.GenerateToken(result);
                                }
                            }
                        }
                        #endregion

                        #region check items quantity in store
                        ItemsLocationsController itc = new ItemsLocationsController();
                        string res = itc.checkItemsAmounts(billDetails, branchId);

                        if (!res.Equals(""))
                        {
                            message = "-3";
                            result += "Result:" + message;

                            res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                            result += ",Message:'" + res + "'";
                            result += "}";

                            return TokenManager.GenerateToken(result);
                        }
                        #endregion

                        #region check properties in store
                        bool propCheck =  it.PropertiesAmountsAvailable(billDetails, branchId);

                        if (!propCheck)
                        {
                            message = "-10";
                            result += "Result:" + message;

                            result += "}";

                            return TokenManager.GenerateToken(result);
                        }
                        #endregion

                        #region validate coupons

                        foreach (var coupon in couponsInvoices)
                        {
                            var c = entity.coupons.Find(coupon.couponId);
                            res = c.name;

                            DateTime datenow = DateTime.Now;
                            datenow = countryc.AddOffsetTodate(datenow);
                            #region check coupon effictive
                            if ((c.startDate == null ? false : c.startDate.Value.Date > datenow.Date) || (c.endDate == null ? false : c.endDate.Value.Date < datenow.Date) || c.isActive != 1)
                            {
                                message = "-8";
                                result += "Result:" + message;

                                res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                result += ",Message:'" + res + "'";
                                result += "}";

                                return TokenManager.GenerateToken(result);
                            }
                            #endregion
                            #region check coupon remain
                            if (c.remainQ <= 0 && !c.quantity.Equals(0))
                            {
                                message = "-7";
                                result += "Result:" + message;

                                res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                result += ",Message:'" + res + "'";
                                result += "}";

                                return TokenManager.GenerateToken(result);
                            }
                            #endregion
                            #region check invMax - invMin for coupon
                            if (c.invMax != 0 || c.invMin != 0)
                            {
                                if (newObject.total < c.invMin && c.invMin != 0)
                                {
                                    message = "-5";
                                    result += "Result:" + message;

                                    res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                    result += ",Message:'" + res + "'";
                                    result += "}";

                                    return TokenManager.GenerateToken(result);
                                }
                                else if (newObject.total > c.invMax && c.invMax != 0)
                                {
                                    message = "-6";
                                    result += "Result:" + message;

                                    res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                    result += ",Message:'" + res + "'";
                                    result += "}";

                                    return TokenManager.GenerateToken(result);
                                }
                            }
                            else if (c.invMax == 0)
                            {
                                if (newObject.total < c.invMin)
                                {
                                    message = "-5";
                                    result += "Result:" + message;

                                    res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                    result += ",Message:'" + res + "'";
                                    result += "}";

                                    return TokenManager.GenerateToken(result);
                                }
                            }
                            #endregion
                        }

                        #endregion

                        #region check customer balance
                        if (newObject.agentId != null)
                        {
                            int agentId = (int)newObject.agentId;
                            agents agent = entity.agents.Where(b => b.agentId == agentId).FirstOrDefault();

                            foreach (var pay in listPayments)
                            {

                                if (pay.processType == "balance" &&
                                    (newObject.shippingCompanyId == null || (newObject.shippingCompanyId != null && newObject.shipUserId != null))
                                    && newObject.agentId != null)
                                {

                                    if (!(
                                        (agent.isLimited == true && agent.maxDeserve == 0) ||
                                    (agent.isLimited == true && agent.balanceType == 0 && agent.maxDeserve >= newObject.totalNet - newObject.paid - agent.balance) ||
                                    (agent.isLimited == true && agent.balanceType == 1 && agent.maxDeserve >= newObject.totalNet - newObject.paid + agent.balance) ||
                                     (agent.isLimited == false && agent.balanceType == 0 && (decimal)agent.balance >= newObject.totalNet - newObject.paid)
                                     ))

                                    {

                                        message = "-4";
                                        result += "Result:" + message;
                                        result += "}";
                                        return TokenManager.GenerateToken(result);

                                    }

                                }
                            }
                        }
                        #endregion

                        newObject = await SaveInvoice(newObject);
                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;
                        if (!invoiceId.Equals(0))
                        {
                            //save items transfer                          
                           it.saveWithSerials(transferObject, billDetails, invoiceId, (int)newObject.branchId,true);

                            #region invoice taxes
                            if (invoiceModel.invoiceTaxes != null)
                                SaveInvoiceTaxes(invoiceModel.invoiceTaxes, invoiceId);
                            #endregion

                            #region coupons
                            couponsInvoicesController ic = new couponsInvoicesController();
                            ic.Save(couponsInvoices, newObject.invType, invoiceId);
                            #endregion

                            #region decrease amount                         
                            itc.decraseAmounts(transferObject, amountNotStr, amountNot.objectName, newObject.invoiceMainId, branchId, (int)newObject.updateUserId);
                            #endregion

                            #region add invoice status st.status = status; //UnderProcessing - Ready - Done

                            InvoiceStatusController isc = new InvoiceStatusController();
                            invoiceStatus.invoiceId = invoiceId;
                            invoiceStatus.status = "UnderProcessing";
                            isc.Save(invoiceStatus);

                            invoiceStatus.status = "Ready";
                            isc.Save(invoiceStatus);

                            if (newObject.shippingCompanyId == null)
                            {
                                invoiceStatus.status = "Done";
                                isc.Save(invoiceStatus);
                            }

                            #endregion

                            #region save payments

                            #region save pos cash transfer
                            CashTransferController cc = new CashTransferController();

                            if (newObject.shippingCompanyId != null && newObject.shipUserId == null && newObject.isPrePaid == 0)
                                PosCashTransfer.side = "sh";


                            PosCashTransfer.invId = invoiceId;

                            await cc.addCashTransfer(PosCashTransfer);
                            #endregion
                            decimal paid = 0;
                            decimal deserved = 0;

                            foreach (var item in listPayments)
                            {
                                await ConfiguredSalesCashTrans(newObject, item, posId);

                                if (item.processType != "balance")
                                {
                                    paid += (decimal)item.cash;
                                    deserved += (decimal)item.cash;
                                }
                            }
                            var inv = entity.invoices.Find(invoiceId);
                            inv.paid += paid;
                            inv.deserved -= deserved;
                            entity.SaveChanges();

                            #region add delivery cash if shipping company is external
                            if(newObject.shippingCompanyId != null && newObject.shipUserId == null)
                            {
                                ctc.AddDeliveryCash((int)newObject.shippingCompanyId,invoiceId,(int) newObject.updateUserId, (int)newObject.posId);
                            }
                            #endregion

                            #region add agent commission to create user
                            ctc.AddAgentCommission((int) newObject.createUserId,invoiceId,(decimal) newObject.totalNet,(int) newObject.posId);
                            #endregion

                            #endregion
                        }


                    }
                }

                catch
                {
                    message = "0";
                }
                result += "Result:" + message;
                string temp = System.Web.Helpers.Json.Encode(newObject.invNumber).Substring(1, System.Web.Helpers.Json.Encode(newObject.invNumber).Length - 2);
                result += ",Message:'" + temp + "'";
                result += ",InvTime:'" + newObject.invTime + "'";
                result += ",UpdateDate:'" + DateTime.Parse(newObject.updateDate.ToString()).ToString() + "'";
                #region get sales draft count
                result += ",SalesDraftCount:";

                List<string> invoiceType = new List<string>() { "sd ", "sbd" };
                int draftCount = getDraftCount((int)newObject.updateUserId, invoiceType);

                result += draftCount;
                #endregion

                #region return pos Balance
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var pos = entity.pos.Find(posId);
                    result += ",PosBalance:" + pos.balance;
                }
                #endregion
                result += "}";
                return TokenManager.GenerateToken(result);

            }
        }

        [HttpPost]
        [Route("saveOrderPayments")]
        public async Task<string> saveOrderPayments(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string invoiceObject = "";
                string Object = "";

                int branchId = 0;
                int posId = 0;

                invoices newObject = null;
                invoiceStatus invoiceStatus = null;
                List<cashTransfer> listPayments = new List<cashTransfer>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        invoiceObject = c.Value.Replace("\\", string.Empty);
                        invoiceObject = invoiceObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<invoices>(invoiceObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "invoiceStatus")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        invoiceStatus = JsonConvert.DeserializeObject<invoiceStatus>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "listPayments")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        listPayments = JsonConvert.DeserializeObject<List<cashTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }

                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        int invoiceId = newObject.invoiceId;
                        message = invoiceId.ToString();
                        #region add invoice status 

                        InvoiceStatusController isc = new InvoiceStatusController();
                        invoiceStatus.invoiceId = invoiceId;
                        isc.Save(invoiceStatus);

                        #endregion

                        #region save payments
                        //var inv = entity.invoices.Find(invoiceId);
                        decimal paid = 0;
                        decimal deserved = 0;
                        foreach (var item in listPayments)
                        {
                            await OrderPaymentCashTrans(newObject, item, posId);
                            // yasin code
                            if (item.processType != "balance")
                            {
                                paid += (decimal)item.cash;
                                deserved += (decimal)item.cash;
                            }
                        }
                        var inv = entity.invoices.Find(invoiceId);
                        inv.paid += paid;
                        inv.deserved -= deserved;

                        entity.SaveChanges();

                        #endregion


                    }
                }

                catch
                {
                    message = "0";
                }
                result += "Result:" + message;
                string temp = System.Web.Helpers.Json.Encode(newObject.invNumber).Substring(1, System.Web.Helpers.Json.Encode(newObject.invNumber).Length - 2);
                result += ",Message:'" + temp + "'";
                result += ",InvTime:'" + newObject.invTime + "'";
                result += ",UpdateDate:'" + DateTime.Parse(newObject.updateDate.ToString()).ToString() + "'";

                #region return pos Balance
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var pos = entity.pos.Find(posId);
                    result += ",PosBalance:" + pos.balance;
                }
                #endregion
                result += "}";
                return TokenManager.GenerateToken(result);

            }
        }

        [HttpPost]
        [Route("recordCompanyCashTransfer")]
        public async Task<string> recordCompanyCashTransfer(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "1";

            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string Object = "";
                int invoiceId = 0;

                cashTransfer cashTransfer = new cashTransfer();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTransfer = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "invoiceId")
                    {
                        invoiceId = int.Parse(c.Value);
                    }

                }
                #endregion
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        CashTransferController cc = new CashTransferController();
                        cashTransfer.transNum = await cc.generateCashNumber(cashTransfer.transNum);

                        var invoice = entity.invoices.Find(invoiceId);
                        int companyId = (int)invoice.shippingCompanyId;
                        var company = entity.shippingCompanies.Find(companyId);

                        decimal newBalance = 0;
                        if (company.balanceType == 0)
                        {
                            if (invoice.totalNet <= (decimal)company.balance)
                            {
                                invoice.paid = invoice.totalNet;
                                invoice.deserved = 0;
                                newBalance = (decimal)company.balance - (decimal)invoice.totalNet;
                                company.balance = newBalance;
                            }
                            else
                            {
                                invoice.paid = (decimal)company.balance;
                                invoice.deserved = invoice.totalNet - (decimal)company.balance;
                                newBalance = (decimal)invoice.totalNet - company.balance;
                                company.balance = newBalance;
                                company.balanceType = 1;
                            }

                            cashTransfer.cash = invoice.paid;
                            cashTransfer.transType = "d"; //deposit
                            if (invoice.paid > 0)
                            {
                                await cc.addCashTransfer(cashTransfer);
                            }
                        }
                        else if (company.balanceType == 1)
                        {
                            newBalance = (decimal)company.balance + (decimal)invoice.totalNet;
                            company.balance = newBalance;
                        }
                        entity.SaveChanges();

                    }
                }

                catch
                {
                    message = "0";
                }

                return TokenManager.GenerateToken(message);

            }
        }

        [HttpPost]
        [Route("recordConfiguredAgentCash")]
        public async Task<string> recordConfiguredAgentCash(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "1";

            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string Object = "";
                string invType = "";
                int invoiceId = 0;
                int posId = 0;

                cashTransfer cashTransfer = new cashTransfer();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTransfer = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "invoiceId")
                    {
                        invoiceId = int.Parse(c.Value);
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                    else if (c.Type == "invType")
                    {
                        invType = c.Value;
                    }

                }
                #endregion
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invoice = entity.invoices.Find(invoiceId);
                        recordConfiguredAgentCash(invoice, invType, cashTransfer, posId);


                    }
                }

                catch
                {
                    message = "0";
                }

                return TokenManager.GenerateToken(message);

            }
        }
        private int getDraftCount(int createUserId, List<string> invoiceType)
        {

            int duration = 2;
            int draftCount = GetCountByCreator(invoiceType, duration, createUserId);
            return draftCount;
        }
        private async Task<cashTransfer> ConfiguredSalesCashTrans(invoices invoice, cashTransfer cashTransfer, int posId)
        {
            CashTransferController cc = new CashTransferController();
            cashTransfer.createUserId = invoice.updateUserId;
            switch (cashTransfer.processType)
            {
                case "cash":// cash: update pos balance  
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var pos = entity.pos.Find(posId);
                        if (pos.balance == null)
                            pos.balance = 0;
                        pos.balance += cashTransfer.cash;
                        entity.SaveChanges();
                    }

                    cashTransfer.transType = "d"; //deposit
                    cashTransfer.posId = posId;
                    cashTransfer.agentId = invoice.agentId;
                    cashTransfer.invId = invoice.invoiceId;
                    cashTransfer.transNum = "dc";
                    cashTransfer.side = "c"; // customer                    
                    cashTransfer.createUserId = invoice.updateUserId;
                    await cc.addCashTransfer(cashTransfer);
                    break;
                case "balance":// balance: update customer balance
                    if (invoice.shippingCompanyId != null && invoice.shipUserId == null && invoice.isPrePaid == 0)
                    {
                        cashTransfer = await recordComSpecificPaidCash(invoice, cashTransfer, posId);
                        //if (cashTransfer.cash > 0)
                        // {
                        //  await cc.addCashTransfer(cashTransfer); //add cash transfer
                        //}
                    }
                    else
                    {
                        await recordConfiguredAgentCash(invoice, "si", cashTransfer, posId);
                    }
                    break;
                case "card": // card
                    cashTransfer.transType = "d"; //deposit
                    cashTransfer.posId = posId;
                    cashTransfer.agentId = invoice.agentId;
                    cashTransfer.invId = invoice.invoiceId;
                    cashTransfer.transNum = "dc";
                    cashTransfer.side = "c"; // customer
                    cashTransfer.createUserId = invoice.updateUserId;
                    await cc.addCashTransfer(cashTransfer); //add cash transfer

                    cc.AddCardCommission(cashTransfer);
                    break;
            }

            return cashTransfer;
        }
        private async Task<cashTransfer> OrderPaymentCashTrans(invoices invoice, cashTransfer cashTransfer, int posId)
        {
            CashTransferController cc = new CashTransferController();
            cashTransfer.createUserId = invoice.updateUserId;
            switch (cashTransfer.processType)
            {
                case "cash":// cash: update pos balance  
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var pos = entity.pos.Find(posId);
                        if (pos.balance == null)
                            pos.balance = 0;
                        //pos.balance += invoice.totalNet;
                        pos.balance += cashTransfer.cash;
                        entity.SaveChanges();
                    }

                    cashTransfer.transType = "d"; //deposit
                    cashTransfer.posId = posId;
                    cashTransfer.agentId = invoice.agentId;
                    cashTransfer.invId = invoice.invoiceId;
                    cashTransfer.transNum = "dc";
                    cashTransfer.side = "c"; // customer                    
                    cashTransfer.createUserId = invoice.updateUserId;
                    await cc.addCashTransfer(cashTransfer);
                    break;
                case "balance":// balance: update customer balance

                    await recordConfiguredAgentCash(invoice, "si", cashTransfer, posId);

                    break;
                case "card": // card
                    cashTransfer.transType = "d"; //deposit
                    cashTransfer.posId = posId;
                    cashTransfer.agentId = invoice.agentId;
                    cashTransfer.invId = invoice.invoiceId;
                    cashTransfer.transNum = "dc";
                    cashTransfer.side = "c"; // customer
                    cashTransfer.createUserId = invoice.updateUserId;
                    await cc.addCashTransfer(cashTransfer); //add cash transfer

                    cc.AddCardCommission(cashTransfer);
                    break;
            }

            return cashTransfer;
        }
        public async Task<cashTransfer> recordComSpecificPaidCash(invoices invoice, cashTransfer cashTrasnfer, int posId)
        {
            CashTransferController cc = new CashTransferController();
            shippingCompanies company;
            using (incposdbEntities entity = new incposdbEntities())
            {
                company = entity.shippingCompanies.Find(invoice.shippingCompanyId);
                //var inv = entity.invoices.Find(invoice.invoiceId);


                decimal newBalance = 0;

                cashTrasnfer.posId = posId;
                cashTrasnfer.shippingCompanyId = invoice.shippingCompanyId;
                cashTrasnfer.invId = invoice.invoiceId;
                cashTrasnfer.createUserId = invoice.createUserId;
                cashTrasnfer.transType = "d"; //deposit
                cashTrasnfer.side = "sh"; // vendor
                cashTrasnfer.transNum = "dsh";

                if (company.balanceType == 0)
                {
                    if (cashTrasnfer.cash <= (decimal)company.balance)
                    {
                        newBalance = (decimal)company.balance - (decimal)cashTrasnfer.cash;
                        company.balance = newBalance;

                        // yasin code
                        //inv.paid += cashTrasnfer.cash;
                       // inv.deserved -= cashTrasnfer.cash;
                        /////
                    }
                    else
                    {
                        //inv.paid += (decimal)company.balance;
                       // inv.deserved -= (decimal)company.balance;
                        ///////
                        newBalance = (decimal)cashTrasnfer.cash - company.balance;
                        company.balance = newBalance;
                        company.balanceType = 1;
                    }
                    cashTrasnfer.transType = "d"; //deposit

                }
                else if (company.balanceType == 1)
                {
                    newBalance = (decimal)company.balance + (decimal)cashTrasnfer.cash;
                    company.balance = newBalance;
                }
                entity.SaveChanges();
            }
            return cashTrasnfer;
        }

        public async Task<invoices> recordConfiguredAgentCash(invoices invoice, string invType, cashTransfer cashTransfer, int posId)
        {
            CashTransferController cc = new CashTransferController();
            decimal newBalance = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {
                var agent = entity.agents.Find(invoice.agentId);
                var inv = entity.invoices.Find(invoice.invoiceId);

                #region agent Cash transfer
                cashTransfer.posId = posId;
                cashTransfer.agentId = invoice.agentId;
                cashTransfer.invId = invoice.invoiceId;
                if (cashTransfer.createUserId == null)
                    cashTransfer.createUserId = invoice.createUserId;
                #endregion
                switch (invType)
                {
                    #region purchase
                    case "pi"://purchase invoice
                    case "sb"://sale bounce
                        cashTransfer.transType = "p";
                        if (invType.Equals("pi"))
                        {
                            cashTransfer.side = "v"; // vendor
                            cashTransfer.transNum = await cc.generateCashNumber("pv");

                        }
                        else
                        {
                            cashTransfer.side = "c"; // customer     
                            cashTransfer.transNum = await cc.generateCashNumber("pc");

                        }
                        if (agent.balanceType == 1)
                        {
                            if (cashTransfer.cash <= (decimal)agent.balance)
                            {

                                newBalance = (decimal)agent.balance - (decimal)cashTransfer.cash;
                                agent.balance = newBalance;

                                // yasin code
                               // inv.paid += cashTransfer.cash;
                                //inv.deserved -= cashTransfer.cash;
                                ////
                                entity.SaveChanges();
                                ///
                            }
                            else
                            {
                                // yasin code
                               // inv.paid += (decimal)agent.balance;
                               // inv.deserved -= (decimal)agent.balance;
                                //////
                                ///
                                newBalance = (decimal)cashTransfer.cash - (decimal)agent.balance;
                                agent.balance = newBalance;
                                agent.balanceType = 0;
                                entity.SaveChanges();

                            }
                            cashTransfer.transType = "p"; //pull

                            if (cashTransfer.processType != "balance")
                                await cc.addCashTransfer(cashTransfer); //add agent cash transfer

                        }
                        else if (agent.balanceType == 0)
                        {
                            newBalance = (decimal)agent.balance + (decimal)cashTransfer.cash;
                            agent.balance = newBalance;
                            entity.SaveChanges();
                        }

                        break;
                    #endregion
                    #region purchase bounce
                    case "pb"://purchase bounce invoice
                    case "si"://sale invoice
                        cashTransfer.transType = "d";

                        if (invType.Equals("pb"))
                        {
                            cashTransfer.side = "v"; // vendor
                            cashTransfer.transNum = await cc.generateCashNumber("dv");
                        }
                        else
                        {
                            cashTransfer.side = "c"; // customer
                            cashTransfer.transNum = await cc.generateCashNumber("dc");

                        }
                        if (agent.balanceType == 0)
                        {
                            if (cashTransfer.cash <= (decimal)agent.balance)
                            {
                                newBalance = (decimal)agent.balance - (decimal)cashTransfer.cash;
                                agent.balance = newBalance;

                                // yasin code
                                //inv.paid += cashTransfer.cash;
                               // inv.deserved -= cashTransfer.cash;
                                entity.SaveChanges();
                            }
                            else
                            {

                                //inv.paid += (decimal)agent.balance;
                               // inv.deserved -= (decimal)agent.balance;

                                //////
                                newBalance = (decimal)cashTransfer.cash - (decimal)agent.balance;
                                agent.balance = newBalance;
                                agent.balanceType = 1;
                                entity.SaveChanges();

                            }
                            cashTransfer.transType = "d"; //deposit

                            if (cashTransfer.cash > 0 && cashTransfer.processType != "balance")
                            {
                                await cc.addCashTransfer(cashTransfer); //add cash transfer     
                            }
                        }
                        else if (agent.balanceType == 1)
                        {
                            newBalance = (decimal)agent.balance + (decimal)cashTransfer.cash;
                            agent.balance = newBalance;
                            entity.SaveChanges();
                        }


                        break;
                        #endregion
                }
            }

            return invoice;
        }
        [HttpPost]
        [Route("saveSalesBounce")]
        public async Task<string> saveSalesBounce(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string invoiceObject = "";
                invoices newObject = null;

                string Object = "";
                string notificationStr = "";
                int branchId = 0;
                int posId = 0;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> transferObjectModel = new List<ItemTransferModel>();
                List<cashTransfer> paymentsList = new List<cashTransfer>();
                cashTransfer PosCashTransfer = null;
                NotificationUserModel notificationUser = null;
                List<couponsInvoices> couponsInvoices = new List<couponsInvoices>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        invoiceObject = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(invoiceObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        transferObjectModel = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        paymentsList = JsonConvert.DeserializeObject<List<cashTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "notification")
                    {
                        notificationStr = c.Value.Replace("\\", string.Empty);
                        notificationStr = notificationStr.Trim('"');
                        notificationUser = JsonConvert.DeserializeObject<NotificationUserModel>(notificationStr, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "PosCashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        PosCashTransfer = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "invoiceCoupons")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        couponsInvoices = JsonConvert.DeserializeObject<List<couponsInvoices>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }

                }
                #endregion
                pos pos = new pos();
               try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    ItemsUnitsController iuc = new ItemsUnitsController();
                    ItemsTransferController it = new ItemsTransferController();

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        #region check pos balance
                        pos = entity.pos.Find(posId);
                        var cashT = paymentsList.Where(x => x.processType == "cash").FirstOrDefault();

                        if (cashT != null && pos.balance < cashT.cash)
                        {
                            message = "-2";
                            result += "Result:" + message;
                            result += "}";
                            return TokenManager.GenerateToken(result);

                        }
                        #endregion

                        #region caculate available amount in basic invoice
                        //get last sales invoice   
                        var lastMainInvId = entity.invoices.Where(x => x.invNumber == entity.invoices.Where(i => i.invoiceId == newObject.invoiceMainId).FirstOrDefault().invNumber).Max(x => x.invoiceId);

                        //sales invoice items
                        var mainInvoiceItems = await it.Get(lastMainInvId);

                        var returnedItems = transferObjectModel.Where(x => x.itemType != "sr").Select(x => x.itemId).Distinct().ToList();
                        foreach (var item in returnedItems)
                        {
                            var returnedItemUnits = transferObjectModel.Where(x => x.itemId == item).Select(x => new { x.itemUnitId, x.quantity }).ToList();
                            var saledItemUnits = mainInvoiceItems.Where(x => x.itemId == item).ToList();
                            int returnedQuantity = 0;
                            int saledQuantity = 0;

                            foreach (var itemUnit in returnedItemUnits)
                            {
                                int multiplyFactor = iuc.multiplyFactorWithSmallestUnit((int)item, (int)itemUnit.itemUnitId);
                                returnedQuantity += multiplyFactor * (int)itemUnit.quantity;
                            }
                            
                            foreach (var itemUnit in saledItemUnits)
                            {
                                int multiplyFactor = iuc.multiplyFactorWithSmallestUnit((int)item, (int)itemUnit.itemUnitId);
                                saledQuantity += multiplyFactor * (int)itemUnit.quantity;
                            }
                            if (returnedQuantity > saledQuantity)
                            {
                                message = "-9";
                                result += "Result:" + message;
                                result += "}";
                                return TokenManager.GenerateToken(result);
                            }
                        }

                        #endregion
                        newObject.invoiceMainId = lastMainInvId;
                        newObject = await SaveInvoice(newObject);
                        int invoiceId = newObject.invoiceId;

                        if (!invoiceId.Equals(0))
                        {
                            try
                            {
                                #region save updated sales invoice
                                var salesInvoice = entity.invoices.Where(x => x.invoiceId == lastMainInvId).FirstOrDefault();

                                var newSalesInv = new invoices();
                                #region new sales invoice object
                                newSalesInv.invNumber = salesInvoice.invNumber;
                                newSalesInv.invoiceMainId = lastMainInvId;
                                newSalesInv.agentId = salesInvoice.agentId;
                                newSalesInv.invType = salesInvoice.invType;
                                newSalesInv.total = salesInvoice.total;
                                newSalesInv.totalNet = salesInvoice.totalNet;
                                newSalesInv.paid = salesInvoice.paid;
                                newSalesInv.deserved = salesInvoice.deserved;
                                newSalesInv.deservedDate = salesInvoice.deservedDate;
                                newSalesInv.invCase = salesInvoice.invCase;
                                newSalesInv.notes = salesInvoice.notes;
                                newSalesInv.vendorInvNum = salesInvoice.vendorInvNum;
                                newSalesInv.vendorInvDate = salesInvoice.vendorInvDate;
                                newSalesInv.invTime = salesInvoice.invTime;
                                newSalesInv.invDate = salesInvoice.invDate;
                                newSalesInv.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                                newSalesInv.createUserId = salesInvoice.createUserId;
                                newSalesInv.updateUserId = salesInvoice.updateUserId;
                                newSalesInv.discountType = salesInvoice.discountType;
                                newSalesInv.discountValue = salesInvoice.discountValue;
                                newSalesInv.tax = salesInvoice.tax;
                                newSalesInv.taxtype = salesInvoice.taxtype;
                                newSalesInv.taxValue = salesInvoice.taxValue;
                                newSalesInv.name = salesInvoice.name;
                                newSalesInv.isApproved = salesInvoice.isApproved;
                                newSalesInv.branchCreatorId = salesInvoice.branchCreatorId;
                                newSalesInv.branchId = salesInvoice.branchId;
                                newSalesInv.posId = salesInvoice.posId;
                                newSalesInv.shippingCompanyId = salesInvoice.shippingCompanyId;
                                newSalesInv.shipUserId = salesInvoice.shipUserId;
                                newSalesInv.userId = salesInvoice.userId;
                                newSalesInv.manualDiscountType = salesInvoice.manualDiscountType;
                                newSalesInv.manualDiscountValue = salesInvoice.manualDiscountValue;
                                newSalesInv.cashReturn = salesInvoice.cashReturn;
                                newSalesInv.shippingCost = salesInvoice.shippingCost;
                                newSalesInv.realShippingCost = salesInvoice.realShippingCost;
                                newSalesInv.isPrePaid = salesInvoice.isPrePaid;
                                newSalesInv.isActive = true;
                                newSalesInv.sales_invoice_note = salesInvoice.sales_invoice_note;
                                newSalesInv.itemtax_note = salesInvoice.itemtax_note;
                                newSalesInv.sliceId = salesInvoice.sliceId;
                                newSalesInv.sliceName = salesInvoice.sliceName;
                                #endregion
                                newSalesInv = entity.invoices.Add(newSalesInv);
                                entity.SaveChanges();

                                #region copy status to new sales invoice
                                var invoicStatus = entity.invoiceStatus.Where(x => x.invoiceId == lastMainInvId).ToList();
                                foreach(var s in invoicStatus)
                                {
                                    var status = new invoiceStatus()
                                    {
                                        invoiceId = newSalesInv.invoiceId,
                                        createDate = s.createDate,
                                        createUserId = s.createUserId,
                                        isActive = s.isActive,
                                        notes = s.notes,
                                        status = s.status,
                                        updateDate = s.updateDate,
                                        updateUserId = s.updateUserId,                                       
                                    };

                                    entity.invoiceStatus.Add(status);
                                    entity.SaveChanges();

                                }
                                #endregion
                                #region copy invoice taxes to new sales invoice
                                var invoicTaxes = entity.invoiceTaxes.Where(x => x.invoiceId == lastMainInvId).ToList();
                                foreach (var s in invoicTaxes)
                                {
                                    var tax = new invoiceTaxes()
                                    {
                                        invoiceId = newSalesInv.invoiceId,
                                        rate = s.rate,
                                        taxId = s.taxId,
                                        taxType = s.taxType,
                                        taxValue = s.taxValue,
                                        notes = s.notes,
                                    };

                                    entity.invoiceTaxes.Add(tax);
                                    entity.SaveChanges();

                                }
                                #endregion
                                #endregion
                                #region save return invoice items
                                int sliceId = 0;
                                if (salesInvoice.sliceId != null)
                                    sliceId = (int)salesInvoice.sliceId;
                                it.saveReturnItems(transferObject, transferObjectModel, invoiceId, newSalesInv.invoiceId, mainInvoiceItems,(int)salesInvoice.branchId,sliceId);

                                #endregion

                                #region coupons
                                couponsInvoicesController ic = new couponsInvoicesController();
                                ic.Save(couponsInvoices, newObject.invType, invoiceId);
                                ic.Save(couponsInvoices, "sd", newSalesInv.invoiceId);
                                #endregion

                                #region save payments
                                CashTransferController cc = new CashTransferController();
                                foreach (var cashTransfer in paymentsList)
                                {
                                    cashTransfer.invId = invoiceId;

                                    int cashRes = 0;

                                    cashTransfer.transType = "p"; //pull
                                    cashTransfer.posId = posId;
                                    cashTransfer.agentId = newObject.agentId;
                                    cashTransfer.side = "c"; // customer
                                    cashTransfer.createUserId = newObject.updateUserId;
                                    cashTransfer.transNum = "pc";
                                    var inv = entity.invoices.Find(newObject.invoiceId);
                                    switch (cashTransfer.processType)
                                    {

                                        case "cash":
                                            cashRes = int.Parse(await cc.addCashTransfer(cashTransfer));

                                            if (cashRes > 0)
                                            {
                                                pos.balance -= cashTransfer.cash;
                                                
                                                inv.paid += cashTransfer.cash;
                                                inv.deserved -= cashTransfer.cash;
                                                entity.SaveChanges();
                                            }
                                            break;
                                        case "balance":
                                            if (newObject.agentId != null)
                                            {
                                                decimal newBalance = 0;
                                                var agent = entity.agents.Where(x => x.agentId == newObject.agentId).FirstOrDefault();
                                                if (agent.balanceType == 1)
                                                {
                                                    if (cashTransfer.cash <= (decimal)agent.balance)
                                                    {
                                                       // inv.paid += cashTransfer.cash;
                                                        //inv.deserved -= cashTransfer.cash;
                                                        newBalance = (decimal)agent.balance - (decimal)cashTransfer.cash;
                                                        agent.balance = newBalance;
                                                    }
                                                    else
                                                    {
                                                        //inv.paid += (decimal)agent.balance;
                                                        //inv.deserved -= cashTransfer.cash - (decimal)agent.balance;
                                                        newBalance = (decimal)cashTransfer.cash - (decimal)agent.balance;
                                                        agent.balance = newBalance;
                                                        agent.balanceType = 0;
                                                    }


                                                }
                                                else if (agent.balanceType == 0)
                                                {
                                                    newBalance = (decimal)agent.balance + (decimal)cashTransfer.cash;
                                                    agent.balance = newBalance;
                                                }

                                                cashRes = int.Parse(await cc.addCashTransfer(cashTransfer));

                                                var basicInvId = entity.invoices.Where(x => x.invNumber == entity.invoices.Where(i => i.invoiceId == newObject.invoiceMainId).FirstOrDefault().invNumber).Min(x => x.invoiceId);
                                                var basicInv = entity.invoices.Find(basicInvId);
                                                var returnInv = entity.invoices.Find(invoiceId);

                                                decimal salesPaid = 0;
                                                if (basicInv.deserved >= cashTransfer.cash)
                                                    salesPaid = (decimal)cashTransfer.cash;
                                                else
                                                {
                                                    salesPaid = (decimal)basicInv.deserved;
                                                    //increase agent balance
                                                    if (agent.balanceType == 1)
                                                    {
                                                        if (salesPaid <= (decimal)agent.balance)
                                                        { 
                                                            newBalance = (decimal)agent.balance - (decimal)cashTransfer.cash;
                                                            agent.balance = newBalance;
                                                        }
                                                        else
                                                        {
                                                            newBalance = (decimal)cashTransfer.cash - (decimal)agent.balance;
                                                            agent.balance = newBalance;
                                                            agent.balanceType = 0;
                                                        }


                                                    }
                                                    else if (agent.balanceType == 0)
                                                    {
                                                        newBalance = (decimal)agent.balance + (decimal)cashTransfer.cash;
                                                        agent.balance = newBalance;
                                                    }
                                                }

                                                basicInv.deserved -= salesPaid;
                                                basicInv.paid += salesPaid;

                                                returnInv.deserved -= salesPaid;
                                                returnInv.paid += salesPaid;
                                                entity.SaveChanges();
                                            }
                                            break;
                                    }
                                }
                                #endregion

                                #region save pos cash transfer //process type = inv
                                PosCashTransfer.invId = invoiceId;
                                await cc.addCashTransfer(PosCashTransfer);
                                #endregion

                                #region recieptItems
                                ItemsLocationsController itc = new ItemsLocationsController();
                                itc.recieptItems(branchId, transferObjectModel, (int)newObject.updateUserId, notificationUser.objectName, notificationStr);
                                #endregion
                                message = invoiceId.ToString();

                            }
                            catch
                            {
                                message = "0";
                            }
                        }

                    }
                }

                catch
                {
                    message = "0";

                }
                result += "Result:" + message;

                #region get sales draft count
                result += ",SalesDraftCount:";

                List<string> invoiceType = new List<string>() { "sd ", "sbd" };
                int draftCount = getDraftCount((int)newObject.updateUserId, invoiceType);
                result += draftCount;
                #endregion

                #region return pos Balance
                result += ",PosBalance:" + pos.balance;
                #endregion
                result += "}";
                return TokenManager.GenerateToken(result);

            }
        }


        [HttpPost]
        [Route("saveSalesOrder")]
        public async Task<string> saveSalesOrder(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string invoiceObject = "";
                string Object = "";
                string amountNotStr = "";
                int branchId = 0;
                int posId = 0;
                invoices newObject = null;
                InvoiceModel invoiceModel = null;

                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> itemsTranfsersModel = new List<ItemTransferModel>();
                NotificationUserModel notificationUser = null;
                NotificationUserModel amountNot = null;
                invoiceStatus invoiceStatus = null;
                cashTransfer PosCashTransfer = null;
                List<couponsInvoices> couponsInvoices = new List<couponsInvoices>();
                List<cashTransfer> listPayments = new List<cashTransfer>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        invoiceObject = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(invoiceObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                        invoiceModel = JsonConvert.DeserializeObject<InvoiceModel>(invoiceObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        //Object = c.Value.Replace("\\", string.Empty);
                        //Object = Object.Trim('"');
                        Object = c.Value;
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        itemsTranfsersModel = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "notification")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        notificationUser = JsonConvert.DeserializeObject<NotificationUserModel>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "amountNot")
                    {
                        amountNotStr = c.Value.Replace("\\", string.Empty);
                        amountNotStr = amountNotStr.Trim('"');
                        amountNot = JsonConvert.DeserializeObject<NotificationUserModel>(amountNotStr, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "invoiceStatus")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        invoiceStatus = JsonConvert.DeserializeObject<invoiceStatus>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "PosCashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        PosCashTransfer = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "invoiceCoupons")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        couponsInvoices = JsonConvert.DeserializeObject<List<couponsInvoices>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "listPayments")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        listPayments = JsonConvert.DeserializeObject<List<cashTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }

                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    ItemsTransferController it = new ItemsTransferController();
                    CashTransferController ctc = new CashTransferController();

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        if (newObject.invoiceId == 0 && newObject.invType == "s")
                        {
                            #region check upgrade

                            ProgramInfo programInfo = new ProgramInfo();
                            int invMaxCount = programInfo.getSaleinvCount();
                            int salesInvCount = pc.getSalesInvCountInMonth();
                            if (salesInvCount >= invMaxCount && invMaxCount != -1)
                            {
                                result += "{";
                                message = "-1";
                                result += "Result:" + message;
                                result += "}";
                                return TokenManager.GenerateToken(result);
                            }
                            #endregion
                        }

                        #region check properties in store
                        bool propCheck = it.PropertiesAmountsAvailable(itemsTranfsersModel, branchId);

                        if (!propCheck)
                        {
                            message = "-10";
                            result += "Result:" + message;

                            result += "}";

                            return TokenManager.GenerateToken(result);
                        }
                        #endregion

                        #region validate coupons
                        string res = "";
                        foreach (var coupon in couponsInvoices)
                        {
                            var c = entity.coupons.Find(coupon.couponId);
                            res = c.name;

                            DateTime datenow = DateTime.Now;
                            datenow = countryc.AddOffsetTodate(datenow);
                            #region check coupon effictive
                            if ((c.startDate == null ? false : c.startDate.Value.Date > datenow.Date) || (c.endDate == null ? false : c.endDate.Value.Date < datenow.Date) || c.isActive != 1)
                            {
                                message = "-8";
                                result += "Result:" + message;

                                res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                result += ",Message:'" + res + "'";
                                result += "}";

                                return TokenManager.GenerateToken(result);
                            }
                            #endregion
                            #region check coupon remain
                            if (c.remainQ <= 0 && !c.quantity.Equals(0))
                            {
                                message = "-7";
                                result += "Result:" + message;

                                res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                result += ",Message:'" + res + "'";
                                result += "}";

                                return TokenManager.GenerateToken(result);
                            }
                            #endregion
                            #region check invMax - invMin for coupon
                            if (c.invMax != 0 || c.invMin != 0)
                            {
                                if (newObject.total < c.invMin && c.invMin != 0)
                                {
                                    message = "-5";
                                    result += "Result:" + message;

                                    res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                    result += ",Message:'" + res + "'";
                                    result += "}";

                                    return TokenManager.GenerateToken(result);
                                }
                                else if (newObject.total > c.invMax && c.invMax != 0)
                                {
                                    message = "-6";
                                    result += "Result:" + message;

                                    res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                    result += ",Message:'" + res + "'";
                                    result += "}";

                                    return TokenManager.GenerateToken(result);
                                }
                            }
                            else if (c.invMax == 0)
                            {
                                if (newObject.total < c.invMin)
                                {
                                    message = "-5";
                                    result += "Result:" + message;

                                    res = System.Web.Helpers.Json.Encode(res).Substring(1, System.Web.Helpers.Json.Encode(res).Length - 2);
                                    result += ",Message:'" + res + "'";
                                    result += "}";

                                    return TokenManager.GenerateToken(result);
                                }
                            }
                            #endregion
                        }

                        #endregion

                        #region check customer balance
                        if (newObject.agentId != null)
                        {
                            int agentId = (int)newObject.agentId;
                            agents agent = entity.agents.Where(b => b.agentId == agentId).FirstOrDefault();

                            foreach (var pay in listPayments)
                            {

                                if (pay.processType == "balance" &&
                                    (newObject.shippingCompanyId == null || (newObject.shippingCompanyId != null && newObject.shipUserId != null))
                                    && newObject.agentId != null)
                                {

                                    if (!(
                                        (agent.isLimited == true && agent.maxDeserve == 0) ||
                                    (agent.isLimited == true && agent.balanceType == 0 && agent.maxDeserve >= newObject.totalNet - newObject.paid - agent.balance) ||
                                    (agent.isLimited == true && agent.balanceType == 1 && agent.maxDeserve >= newObject.totalNet - newObject.paid + agent.balance) ||
                                    (agent.isLimited == false && agent.balanceType == 0 && (decimal)agent.balance >= newObject.totalNet - newObject.paid)
                                    ))
                                    {
                                        message = "-4";
                                        result += "Result:" + message;
                                        result += "}";
                                        return TokenManager.GenerateToken(result);
                                    }

                                }
                            }
                        }
                        #endregion
                        newObject = await SaveInvoice(newObject);
                        int invoiceId = newObject.invoiceId;
                        message = invoiceId.ToString();

                        if (!invoiceId.Equals(0))
                        {
                            try
                            {
                                #region save items transfer                             
                                it.saveWithSerials(transferObject, itemsTranfsersModel, invoiceId, (int)newObject.branchId, true);
                                #endregion

                                #region invoice taxes
                                if (invoiceModel.invoiceTaxes != null)
                                    SaveInvoiceTaxes(invoiceModel.invoiceTaxes,invoiceId);
                                #endregion

                                #region coupons
                                couponsInvoicesController ic = new couponsInvoicesController();
                                ic.Save(couponsInvoices, newObject.invType, invoiceId);
                                #endregion

                                #region decrease amount
                                ItemsLocationsController itc = new ItemsLocationsController();
                                itc.decraseAmounts(transferObject, amountNotStr, amountNot.objectName, newObject.invoiceMainId, branchId, (int)newObject.updateUserId);
                                #endregion

                                #region add notification 
                                NotificationController nc = new NotificationController();
                                notification notification = new notification()
                                {
                                    createDate = countryc.AddOffsetTodate(DateTime.Now),
                                    createUserId = notificationUser.createUserId,
                                    updateUserId = notificationUser.createUserId,
                                    title = notificationUser.title,
                                    ncontent = notificationUser.ncontent,
                                    msgType = notificationUser.msgType,
                                };
                                nc.save(notification, notificationUser.objectName, notificationUser.prefix, (int)notificationUser.branchId, (int)notificationUser.recieveId);
                                #endregion

                                #region invoice status
                                InvoiceStatusController isc = new InvoiceStatusController();
                                invoiceStatus.invoiceId = invoiceId;
                                isc.Save(invoiceStatus);

                                if (newObject.shippingCompanyId == null) //new
                                {
                                    invoiceStatus.status = "Done";
                                    isc.Save(invoiceStatus);
                                }
                                #endregion

                                #region save pos cash transfer
                                CashTransferController cc = new CashTransferController();

                                PosCashTransfer.invId = invoiceId;
                                // PosCashTransfer.transNum = await cc.generateCashNumber(PosCashTransfer.transNum);

                                await cc.addCashTransfer(PosCashTransfer);
                                #endregion

                                #region save payments
                                decimal paid = 0;
                                decimal deserved = 0;
                                //if (newObject.shippingCompanyId == null || (newObject.shippingCompanyId != null && newObject.shipUserId == null))
                                {
                                    foreach (var item in listPayments)
                                    {
                                        await ConfiguredSalesCashTrans(newObject, item, posId);
                                        if (item.processType != "balance")
                                        {
                                            paid += (decimal)item.cash;
                                            deserved += (decimal)item.cash;
                                        }
                                    }
                                    var inv = entity.invoices.Find(invoiceId);
                                    inv.paid += paid;
                                    inv.deserved -= deserved;
                                    entity.SaveChanges();

                                    #region add delivery cash if shipping company is external
                                    if (newObject.shippingCompanyId != null && newObject.shipUserId == null)
                                    {
                                        ctc.AddDeliveryCash((int)newObject.shippingCompanyId, invoiceId, (int)newObject.updateUserId, (int)newObject.posId);
                                    }
                                    #endregion

                                    #region add agent commission to create user
                                    ctc.AddAgentCommission((int)newObject.createUserId, invoiceId, (decimal)newObject.totalNet, (int)newObject.posId);
                                    #endregion
                                    

                                }
                                #endregion
                            }
                            catch
                            {
                                message = "0";

                            }
                        }


                    }
                }

                catch
                {
                    message = "0";
                }

                result = "{";
                result += "Result:" + message;
                result += ",Message:'" + newObject.invNumber + "'";
                result += ",InvTime:'" + newObject.invTime + "'";
                result += ",UpdateDate:'" + DateTime.Parse(newObject.updateDate.ToString()).ToString() + "'";

                #region return pos Balance
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var pos = entity.pos.Find(posId);
                    result += ",PosBalance:" + pos.balance;
                }
                #endregion
                result += "}";
                return TokenManager.GenerateToken(result);
            }
        }

        private void SaveInvoiceTaxes(List<InvoiceTaxesModel> invoiceTaxes, int invoiceId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                //remove invoice taxes
               var taxes = entity.invoiceTaxes.Where(x => x.invoiceId == invoiceId).ToList();
                entity.invoiceTaxes.RemoveRange(taxes);
                entity.SaveChanges();

                foreach (var row in invoiceTaxes)
                {
                    var tax = new invoiceTaxes()
                    {
                        invoiceId = invoiceId,
                        notes = row.notes,
                        rate = row.rate,
                        taxType = row.taxType,
                        taxId = row.taxId,
                        taxValue = row.taxValue,
                    };
                    entity.invoiceTaxes.Add(tax);
                }
                entity.SaveChanges();
            }
        }
        [HttpPost]
        [Route("preparingOrder")]
        public async Task<string> preparingOrder(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            string result = "{";
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region parameters
                string Object = "";
                int previouseBranch = 0;
                int branchId = 0;
                int userId = 0;
                invoices newObject = null;
                InvoiceModel invoiceModel = null;
                invoiceStatus invoiceStatus = null;
                NotificationUserModel notificationUser = null;
                notification notification = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<couponsInvoices> couponsInvoices = new List<couponsInvoices>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        //Object = c.Value.Replace("\\", string.Empty);
                        //Object = Object.Trim('"');
                        Object = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                        invoiceModel = JsonConvert.DeserializeObject<InvoiceModel>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "invoiceCoupons")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        couponsInvoices = JsonConvert.DeserializeObject<List<couponsInvoices>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "invoiceStatus")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        invoiceStatus = JsonConvert.DeserializeObject<invoiceStatus>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "notification")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        notificationUser = JsonConvert.DeserializeObject<NotificationUserModel>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        notification = JsonConvert.DeserializeObject<notification>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "previouseBranch")
                    {
                        previouseBranch = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        bool unreserveItems = false;

                        if (newObject.invoiceId != 0)
                        {
                            unreserveItems = true;
                        }

                        newObject = await SaveInvoice(newObject);
                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;
                        if (!invoiceId.Equals(0))
                        {
                            //save items transfer
                            ItemsTransferController it = new ItemsTransferController();
                            it.save(transferObject, invoiceId);

                            #region invoice taxes
                            if (invoiceModel.invoiceTaxes != null)
                                SaveInvoiceTaxes(invoiceModel.invoiceTaxes, invoiceId);
                            #endregion
                            #region coupons
                            couponsInvoicesController cc = new couponsInvoicesController();
                            cc.Save(couponsInvoices, newObject.invType, invoiceId);
                            #endregion

                            #region invoice status
                            InvoiceStatusController isc = new InvoiceStatusController();
                            invoiceStatus.invoiceId = invoiceId;
                            isc.Save(invoiceStatus);
                            #endregion

                            #region add notification 
                            NotificationController nc = new NotificationController();
                            notification.updateUserId = notification.createUserId;

                            nc.save(notification, notificationUser.objectName, notificationUser.prefix, (int)notificationUser.branchId);
                            #endregion

                            #region reserve items
                            if (newObject.invType == "or" || newObject.invType == "ors")
                            {
                                ItemsLocationsController ilc = new ItemsLocationsController();
                                if (unreserveItems)
                                {
                                    ilc.unlockItems(transferObject, previouseBranch);
                                }
                                ilc.reserveItems(transferObject, invoiceId, (int)newObject.branchId, userId);
                            }
                            #endregion
                        }

                    }
                }

                catch
                {
                    message = "0";
                }
                result += "Result:" + message;
                result += ",Message:'" + newObject.invNumber + "'";
                result += ",InvTime:'" + newObject.invTime + "'";
                result += ",UpdateDate:'" + DateTime.Parse(newObject.updateDate.ToString()).ToString() + "'";

                #region get order draft count
                result += ",SalesDraftCount:";

                List<string> invoiceType = new List<string>() { "ord ", "ors" };
                int invCount = getDraftCount((int)newObject.updateUserId, invoiceType);
                result += invCount;
                #endregion

                #region get waiting orders count
                invoiceType = new List<string>() { "or" };
                invCount = GetCountUnHandeledOrders(invoiceType, branchId, 0, 0, userId);
                result += ",SalesWaitingOrdersCount:" + invCount;

                #endregion
                result += "}";
                return TokenManager.GenerateToken(result);

            }
        }
        private async Task<invoices> SaveInvoice(invoices newObject)
        {
            string message = "";
            invoices tmpInvoice;
            #region generate InvNumber

            int branchId = (int)newObject.branchCreatorId;
            string invNumber = await generateInvNumber(newObject.invNumber, branchId);

            #endregion
            using (incposdbEntities entity = new incposdbEntities())
            {
                var invoiceEntity = entity.Set<invoices>();

                if (newObject.invoiceMainId == 0)
                    newObject.invoiceMainId = null;
                if (newObject.invoiceId == 0)
                {

                    newObject.invDate = countryc.AddOffsetTodate(DateTime.Now);
                    newObject.invTime = DateTime.Now.TimeOfDay.Add(countryc.offsetTime());
                    newObject.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                    newObject.updateUserId = newObject.createUserId;
                    newObject.isActive = true;
                    newObject.isOrginal = true;
                    newObject.invNumber = invNumber;

                    tmpInvoice = invoiceEntity.Add(newObject);
                    entity.SaveChanges();
                    entity.Dispose();
                    message = tmpInvoice.invoiceId.ToString();
                }
                else
                {
                    tmpInvoice = entity.invoices.Where(p => p.invoiceId == newObject.invoiceId).FirstOrDefault();
                    tmpInvoice.invNumber = invNumber;
                    tmpInvoice.agentId = newObject.agentId;
                    tmpInvoice.invType = newObject.invType;
                    tmpInvoice.total = newObject.total;
                    tmpInvoice.totalNet = newObject.totalNet;
                    tmpInvoice.paid = newObject.paid;
                    tmpInvoice.deserved = newObject.deserved;
                    tmpInvoice.deservedDate = newObject.deservedDate;
                    tmpInvoice.invoiceMainId = newObject.invoiceMainId;
                    tmpInvoice.invCase = newObject.invCase;
                    tmpInvoice.notes = newObject.notes;
                    tmpInvoice.itemtax_note = newObject.itemtax_note;
                    tmpInvoice.sales_invoice_note = newObject.sales_invoice_note;
                    tmpInvoice.vendorInvNum = newObject.vendorInvNum;
                    tmpInvoice.vendorInvDate = newObject.vendorInvDate;
                    tmpInvoice.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                    tmpInvoice.updateUserId = newObject.updateUserId;
                    tmpInvoice.branchId = newObject.branchId;
                    tmpInvoice.discountType = newObject.discountType;
                    tmpInvoice.discountValue = newObject.discountValue;
                    tmpInvoice.tax = newObject.tax;
                    tmpInvoice.taxtype = newObject.taxtype;
                    tmpInvoice.taxValue = newObject.taxValue;
                    tmpInvoice.name = newObject.name;
                    tmpInvoice.isApproved = newObject.isApproved;
                    tmpInvoice.branchCreatorId = newObject.branchCreatorId;
                    tmpInvoice.shippingCompanyId = newObject.shippingCompanyId;
                    tmpInvoice.shipUserId = newObject.shipUserId;
                    tmpInvoice.userId = newObject.userId;
                    tmpInvoice.manualDiscountType = newObject.manualDiscountType;
                    tmpInvoice.manualDiscountValue = newObject.manualDiscountValue;
                    tmpInvoice.cashReturn = newObject.cashReturn;
                    tmpInvoice.shippingCost = newObject.shippingCost;
                    tmpInvoice.realShippingCost = newObject.realShippingCost;
                    tmpInvoice.isPrePaid = newObject.isPrePaid;
                    tmpInvoice.sliceId = newObject.sliceId;
                    tmpInvoice.sliceName = newObject.sliceName;
                    tmpInvoice.isFreeShip = newObject.isFreeShip;
                    tmpInvoice.VATValue = newObject.VATValue;
                    entity.SaveChanges();
                    message = tmpInvoice.invoiceId.ToString();
                }

            }
            return tmpInvoice;
        }

        private async Task<invoices> AddImportInvoice(invoices newObject, invoices sentExportInvoice, 
                                            List<itemsTransfer> transferObject, List<ItemTransferModel> itemsTransfer,
                                            NotificationUserModel notUser = null, notification not = null,
                                            bool final = true)
        {
            string message = "";
            invoices tmpInvoice;
            invoices exportInvoice = new invoices();
            string exportInvNumber = "";
            using (incposdbEntities entity = new incposdbEntities())
            {
                var invoiceEntity = entity.Set<invoices>();
                #region generate InvNumber

                int branchId = (int)newObject.branchCreatorId;
                string invNumber = await generateInvNumber(newObject.invNumber, branchId);

                #endregion

                #region export invoice

                if (newObject.invoiceId != 0)
                {
                    exportInvoice = entity.invoices.Where(x => x.invoiceMainId == newObject.invoiceId).FirstOrDefault();
                    exportInvoice.branchId = sentExportInvoice.branchId;
                    exportInvoice.updateUserId = sentExportInvoice.updateUserId;
                    exportInvoice.invType = sentExportInvoice.invType;
                }
                else
                {
                    exportInvoice = sentExportInvoice;
                    exportInvNumber = await generateInvNumber(exportInvoice.invNumber, branchId);
                }
                #endregion

                #region save import invoice
                if (newObject.invoiceMainId == 0)
                    newObject.invoiceMainId = null;
                if (newObject.invoiceId == 0)
                {
                    newObject.invDate = countryc.AddOffsetTodate(DateTime.Now);
                    newObject.invTime = DateTime.Now.TimeOfDay.Add(countryc.offsetTime());
                    newObject.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                    newObject.updateUserId = newObject.createUserId;
                    newObject.isActive = true;
                    newObject.isOrginal = true;
                    newObject.invNumber = invNumber;

                    tmpInvoice = invoiceEntity.Add(newObject);
                    entity.SaveChanges();

                    message = tmpInvoice.invoiceId.ToString();
                }
                else
                {
                    tmpInvoice = entity.invoices.Where(p => p.invoiceId == newObject.invoiceId).FirstOrDefault();
                    tmpInvoice.invNumber = invNumber;
                    tmpInvoice.agentId = newObject.agentId;
                    tmpInvoice.invType = newObject.invType;
                    tmpInvoice.total = newObject.total;
                    tmpInvoice.totalNet = newObject.totalNet;
                    tmpInvoice.paid = newObject.paid;
                    tmpInvoice.deserved = newObject.deserved;
                    tmpInvoice.deservedDate = newObject.deservedDate;
                    tmpInvoice.invoiceMainId = newObject.invoiceMainId;
                    tmpInvoice.invCase = newObject.invCase;
                    tmpInvoice.notes = newObject.notes;
                    tmpInvoice.itemtax_note = newObject.itemtax_note;
                    tmpInvoice.sales_invoice_note = newObject.sales_invoice_note;

                    tmpInvoice.vendorInvNum = newObject.vendorInvNum;
                    tmpInvoice.vendorInvDate = newObject.vendorInvDate;
                    tmpInvoice.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                    tmpInvoice.updateUserId = newObject.updateUserId;
                    tmpInvoice.branchId = newObject.branchId;
                    tmpInvoice.discountType = newObject.discountType;
                    tmpInvoice.discountValue = newObject.discountValue;
                    tmpInvoice.tax = newObject.tax;
                    tmpInvoice.taxtype = newObject.taxtype;
                    tmpInvoice.name = newObject.name;
                    tmpInvoice.isApproved = newObject.isApproved;
                    tmpInvoice.branchCreatorId = newObject.branchCreatorId;
                    tmpInvoice.shippingCompanyId = newObject.shippingCompanyId;
                    tmpInvoice.shipUserId = newObject.shipUserId;
                    tmpInvoice.userId = newObject.userId;
                    tmpInvoice.manualDiscountType = newObject.manualDiscountType;
                    tmpInvoice.manualDiscountValue = newObject.manualDiscountValue;
                    tmpInvoice.cashReturn = newObject.cashReturn;
                    tmpInvoice.shippingCost = newObject.shippingCost;
                    tmpInvoice.realShippingCost = newObject.realShippingCost;
                    tmpInvoice.sliceId = newObject.sliceId;
                    tmpInvoice.sliceName = newObject.sliceName;
                    entity.SaveChanges();
                    message = tmpInvoice.invoiceId.ToString();
                }
                #endregion

            }
            if (!tmpInvoice.Equals(0))
            {
                using (incposdbEntities entity1 = new incposdbEntities())
                {
                    exportInvoice.invoiceMainId = tmpInvoice.invoiceId;

                    #region save export invoice
                    if (exportInvoice.invoiceId == 0)
                    {

                        exportInvoice.invDate = countryc.AddOffsetTodate(DateTime.Now);
                        exportInvoice.invTime = DateTime.Now.TimeOfDay.Add(countryc.offsetTime());
                        exportInvoice.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                        exportInvoice.updateUserId = exportInvoice.createUserId;
                        exportInvoice.isActive = true;
                        exportInvoice.isOrginal = true;
                        exportInvoice.invNumber = exportInvNumber;

                        exportInvoice = entity1.invoices.Add(exportInvoice);
                        entity1.SaveChanges();

                    }
                    else
                    {
                        var exportInvoiceTmp = entity1.invoices.Where(p => p.invoiceId == exportInvoice.invoiceId).FirstOrDefault();
                        exportInvoiceTmp.agentId = exportInvoice.agentId;
                        exportInvoiceTmp.invType = exportInvoice.invType;
                        exportInvoiceTmp.total = exportInvoice.total;
                        exportInvoiceTmp.totalNet = exportInvoice.totalNet;
                        exportInvoiceTmp.paid = exportInvoice.paid;
                        exportInvoiceTmp.deserved = exportInvoice.deserved;
                        exportInvoiceTmp.deservedDate = exportInvoice.deservedDate;
                        exportInvoiceTmp.invoiceMainId = exportInvoice.invoiceMainId;
                        exportInvoiceTmp.invCase = exportInvoice.invCase;
                        exportInvoiceTmp.notes = exportInvoice.notes;
                        exportInvoiceTmp.vendorInvNum = exportInvoice.vendorInvNum;
                        exportInvoiceTmp.vendorInvDate = exportInvoice.vendorInvDate;
                        exportInvoiceTmp.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                        exportInvoiceTmp.updateUserId = exportInvoice.updateUserId;
                        exportInvoiceTmp.branchId = exportInvoice.branchId;
                        exportInvoiceTmp.discountType = exportInvoice.discountType;
                        exportInvoiceTmp.discountValue = exportInvoice.discountValue;
                        exportInvoiceTmp.tax = exportInvoice.tax;
                        exportInvoiceTmp.taxtype = exportInvoice.taxtype;
                        exportInvoiceTmp.name = exportInvoice.name;
                        exportInvoiceTmp.isApproved = exportInvoice.isApproved;
                        exportInvoiceTmp.branchCreatorId = exportInvoice.branchCreatorId;
                        exportInvoiceTmp.shippingCompanyId = exportInvoice.shippingCompanyId;
                        exportInvoiceTmp.shipUserId = exportInvoice.shipUserId;
                        exportInvoiceTmp.userId = exportInvoice.userId;
                        exportInvoiceTmp.manualDiscountType = exportInvoice.manualDiscountType;
                        exportInvoiceTmp.manualDiscountValue = exportInvoice.manualDiscountValue;
                        exportInvoiceTmp.cashReturn = exportInvoice.cashReturn;
                        exportInvoiceTmp.shippingCost = exportInvoice.shippingCost;
                        exportInvoiceTmp.realShippingCost = exportInvoice.realShippingCost;
                        exportInvoiceTmp.sliceId = exportInvoice.sliceId;
                        exportInvoiceTmp.sliceName = exportInvoice.sliceName;
                        entity1.SaveChanges();

                    }
                    #endregion
                }
                //save items transfer to import and export invoice
                ItemsTransferController it = new ItemsTransferController();
                if(newObject.invType.Equals("exd") || newObject.invType.Equals("ex"))
                it.saveImExItems(transferObject, itemsTransfer, exportInvoice.invoiceId,tmpInvoice.invoiceId ,(int)newObject.branchId,(int)exportInvoice.branchId,final);
                else
                it.saveImExItems(transferObject, itemsTransfer, tmpInvoice.invoiceId, exportInvoice.invoiceId,(int)newObject.branchId,(int)exportInvoice.branchId,final);

                //send notification
                if (not != null && final == true)
                {
                    NotificationController nc = new NotificationController();
                    nc.save(not, notUser.objectName, notUser.prefix, (int)notUser.branchId);
                }
            }

            return tmpInvoice;
        }
        public async Task<string> generateInvNumber(string invoiceCode, int branchId)
        {
            #region check if last of code is num
            var num = invoiceCode.Substring(invoiceCode.LastIndexOf("-") + 1);

            if (!num.Equals(invoiceCode))
                return invoiceCode;
            //try {
            //    int tmp = int.Parse(num);
            //    return invoiceCode;
            //}
            //catch { }
            #endregion
            int sequence = 0;
            string branchCode = "";

            using (incposdbEntities entity = new incposdbEntities())
            {
                var branch = entity.branches.Find(branchId);

                branchCode = branch.code;

                var numberList = entity.invoices.Where(b => b.invNumber.Contains(invoiceCode + "-") && b.branchCreatorId == branchId).Select(b => b.invNumber).ToList();
                for (int i = 0; i < numberList.Count; i++)
                {
                    string code = numberList[i];
                    string s = code.Substring(code.LastIndexOf("-") + 1);

                    numberList[i] = s;
                }
                if (numberList.Count > 0)
                {
                    numberList.Sort();
                    try
                    {
                        sequence = int.Parse(numberList[numberList.Count - 1]);
                    }
                    catch
                    { sequence = 0; }
                }
            }
            sequence++;

            string strSeq = sequence.ToString();
            if (sequence <= 999999)
                strSeq = sequence.ToString().PadLeft(6, '0');
            string invoiceNum = invoiceCode + "-" + branchCode + "-" + strSeq;
            return invoiceNum;
        }


        [HttpPost]
        [Route("updateprintstat")]
        public string updateprintstat(string token)
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
                int countstep = 0;
                bool isOrginal = false;
                bool updateOrginalstate = false;

                string invoiceObject = "";

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "id")
                    {
                        id = int.Parse(c.Value);
                    }
                    else if (c.Type == "countstep")
                    {
                        countstep = int.Parse(c.Value);
                    }
                    else if (c.Type == "isOrginal")
                    {
                        isOrginal = bool.Parse(c.Value);
                    }
                    else if (c.Type == "updateOrginalstate")
                    {
                        updateOrginalstate = bool.Parse(c.Value);
                    }
                }

                try
                {

                    invoices tmpInvoice;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        if (id == 0)
                        {

                            return TokenManager.GenerateToken("0");

                        }
                        else
                        {

                            tmpInvoice = entity.invoices.Where(p => p.invoiceId == id).FirstOrDefault();
                            int res = tmpInvoice.printedcount + countstep;
                            if (res < 0)
                            {
                                res = 0;
                            }
                            tmpInvoice.printedcount = res;
                            if (updateOrginalstate)
                            {
                                tmpInvoice.isOrginal = isOrginal;
                            }


                            entity.SaveChanges();
                            message = tmpInvoice.invoiceId.ToString();
                            return TokenManager.GenerateToken(message);
                        }
                    }
                }

                catch (Exception ex)
                {
                    message = "0";
                    return TokenManager.GenerateToken(ex.ToString());
                }
            }
        }


        [HttpPost]
        [Route("delete")]
        public string delete(string token)
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
                try
                {
                    int invoiceId = 0;
                    int userId = 0;
                    IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                    foreach (Claim c in claims)
                    {
                        if (c.Type == "itemId")
                        {
                            invoiceId = int.Parse(c.Value);
                        }
                        else if (c.Type == "userId")
                        {
                            userId = int.Parse(c.Value);
                        }
                    }
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var inv = entity.invoices.Find(invoiceId);
                        inv.isActive = false;
                        message = entity.SaveChanges().ToString();


                        #region get orders draft count
                        int draftCount = 0;
                        List<string> invoiceType = null;

                        string result = "{";
                        result += "Result:1";
                        result += ",OrdersCount:";

                        invoiceType = new List<string>() { "ord ", "ors" };

                        draftCount = getDraftCount((int)userId, invoiceType);

                        result += draftCount;
                        #endregion

                        #region get quotations draft count
                        invoiceType = new List<string>() { "qd", "qs" };
                        draftCount = getDraftCount((int)userId, invoiceType);
                        result += ",SalesQuotationCount:" + draftCount;
                        result += "}";
                        #endregion
                        return TokenManager.GenerateToken(result);
                    }
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                }
            }
        }
        [HttpPost]
        [Route("deleteMovment")]
        public string deleteMovment(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                try
                {
                    int invoiceId = 0;
                    int userId = 0;
                    IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                    foreach (Claim c in claims)
                    {
                        if (c.Type == "itemId")
                        {
                            invoiceId = int.Parse(c.Value);
                        }
                        else if (c.Type == "userId")
                        {
                            userId = int.Parse(c.Value);
                        }
                    }
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var inv = entity.invoices.Find(invoiceId);
                        inv.isActive = false;

                        invoices inv2 = new invoices();
                        if (inv.invoiceMainId != null)
                        {
                            inv2 = entity.invoices.Find(inv.invoiceMainId);                            
                        }
                        else
                        {
                            inv2 = entity.invoices.Where(x => x.invoiceMainId == invoiceId).FirstOrDefault();
                        }
                        if (inv2 != null)
                            inv2.isActive = false;

                        entity.SaveChanges();
                        string result = "{Result:1}";

                        return TokenManager.GenerateToken(result);
                    }
                }
                catch
                {
                    string result = "{Result:0}";
                    return TokenManager.GenerateToken(result);
                }
            }
        }
        [HttpPost]
        [Route("deleteOrder")]
        public string deleteOrder(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            ItemsLocationsController ilc = new ItemsLocationsController();
            string message = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                try
                {
                    int invoiceId = 0;
                    int userId = 0;
                    IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                    foreach (Claim c in claims)
                    {
                        if (c.Type == "itemId")
                        {
                            invoiceId = int.Parse(c.Value);
                        }
                        else if (c.Type == "userId")
                        {
                            userId = int.Parse(c.Value);
                        }
                    }

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        // desactive invoice
                        var inv = entity.invoices.Find(invoiceId);
                        inv.isActive = false;
                        entity.SaveChanges();

                        // unlockItems
                        var itemsLocations = entity.itemsLocations.Where(x => x.invoiceId == invoiceId).ToList();
                        foreach (itemsLocations il in itemsLocations)
                        {
                            var itemLoc = (from b in entity.itemsLocations
                                           where b.invoiceId == null && b.itemUnitId == il.itemUnitId && b.locationId == il.locationId
                                           && b.startDate == il.startDate && b.endDate == il.endDate
                                           select new ItemLocationModel
                                           {
                                               itemsLocId = b.itemsLocId,
                                           }).FirstOrDefault();
                            var orderItem = entity.itemsLocations.Find(il.itemsLocId);
                            if (orderItem.quantity == il.quantity)
                                entity.itemsLocations.Remove(orderItem);
                            else
                                orderItem.quantity -= il.quantity;

                            if (itemLoc == null)
                            {
                                var loc = new itemsLocations()
                                {
                                    locationId = il.locationId,
                                    quantity = il.quantity,
                                    createDate = countryc.AddOffsetTodate(DateTime.Now),
                                    updateDate = countryc.AddOffsetTodate(DateTime.Now),
                                    createUserId = il.createUserId,
                                    updateUserId = il.createUserId,
                                    startDate = il.startDate,
                                    endDate = il.endDate,
                                    itemUnitId = il.itemUnitId,
                                    note = il.note,
                                };
                                entity.itemsLocations.Add(loc);
                            }
                            else
                            {
                                var loc = entity.itemsLocations.Find(itemLoc.itemsLocId);
                                loc.quantity += il.quantity;
                                loc.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                                loc.updateUserId = il.updateUserId;

                            }
                            entity.SaveChanges();
                        }
                        message = "1";
                        string result = "{Result:" + message;
                        result += ",OrdersCount:";

                        List<string> invoiceType = new List<string>() { "ord ", "ors" };

                        int draftCount = getDraftCount((int)userId, invoiceType);

                        result += draftCount;
                        result += "}";
                        return TokenManager.GenerateToken(result);
                    }
                }
                catch
                {
                    message = "{}";
                    return TokenManager.GenerateToken(message);
                }
            }
        }

        

        public decimal saveAvgPrice(List<itemsTransfer> newObject)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var set = entity.setting.Where(x => x.name == "item_cost").FirstOrDefault();
                string invoiceNum = "0";
                if (set != null)
                    invoiceNum = entity.setValues.Where(x => x.settingId == (int)set.settingId).Select(x => x.value).Single();
                foreach (itemsTransfer item in newObject)
                {
                    var itemId = entity.itemsUnits.Where(x => x.itemUnitId == (int)item.itemUnitId).Select(x => x.itemId).Single();

                    decimal price = GetAvgPrice((int)itemId, int.Parse(invoiceNum));

                    var itemO = entity.items.Find(itemId);
                    itemO.avgPurchasePrice = price;

                }
                entity.SaveChanges();
                return 0;
            }
        }
        private decimal GetAvgPrice(int itemId, int numInvoice)
        {
            decimal price = 0;
            int totalNum = 0;
            decimal smallUnitPrice = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemUnits = entity.itemsUnits.Where(i => i.itemId == itemId).Select(i => i.itemUnitId).ToList();
                List<int> invoicesIds = new List<int>();
                if (numInvoice == 0)
                {
                    invoicesIds = (from p in entity.invoices
                                   where p.isActive == true && (p.invType == "p"|| p.invType == "is")
                                   select p).Select(x => x.invoiceId).ToList();
                }
                else
                {
                    var invoices = (from p in entity.invoices
                                    where p.isActive == true && (p.invType == "p" || p.invType == "is")
                                    orderby p.invDate descending
                                    select p).Take(numInvoice);
                    invoicesIds = invoices.Select(x => x.invoiceId).ToList();
                }

                price += getLastPrice(itemUnits, invoicesIds);

                totalNum = getItemUnitLastNum(itemUnits, invoicesIds);

                if (totalNum != 0)
                    smallUnitPrice = price / totalNum;
                return smallUnitPrice;

            }
        }

        private int getUpperUnitValue(int itemUnitId, int basicItemUnitId)
        {
            int unitValue = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {

                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (upperUnit == null)
                    return 1;
                if (upperUnit.itemUnitId == basicItemUnitId)
                    return (int)upperUnit.unitValue;
                else
                    unitValue *= getUpperUnitValue(upperUnit.itemUnitId, basicItemUnitId);
                return unitValue;
            }
        }
        private decimal getItemUnitSumPrice(List<int> itemUnits)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var sumPrice = (from b in entity.invoices
                                where b.invType == "p"
                                join s in entity.itemsTransfer.Where(x => itemUnits.Contains((int)x.itemUnitId)) on b.invoiceId equals s.invoiceId
                                select s.quantity * s.price).Sum();

                if (sumPrice != null)
                    return (decimal)sumPrice;
                else
                    return 0;
            }
        }
        private decimal getLastPrice(List<int> itemUnits, List<int> invoiceIds)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var sumPrice = (from s in entity.itemsTransfer.Where(x => itemUnits.Contains((int)x.itemUnitId) && invoiceIds.Contains((int)x.invoiceId))
                                select s.quantity * s.price).Sum();

                if (sumPrice != null)
                    return (decimal)sumPrice;
                else
                    return 0;
            }
        }

        private int getItemUnitLastNum(List<int> itemUnits, List<int> invoiceIds)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {

                var smallestUnit = entity.itemsUnits.Where(iu => itemUnits.Contains((int)iu.itemUnitId) && iu.unitId == iu.subUnitId).FirstOrDefault();

                if (smallestUnit == null)
                {
                    smallestUnit = entity.itemsUnits.Where(u => !entity.itemsUnits.Any(y => u.subUnitId == y.unitId && itemUnits.Contains((int)u.itemUnitId))).FirstOrDefault();
                }
                var lst = entity.itemsTransfer.Where(x => x.itemUnitId == smallestUnit.itemUnitId && invoiceIds.Contains((int)x.invoiceId))
                           .Select(t => new ItemLocationModel
                           {
                               quantity = t.quantity,
                           }).ToList();
                long sumNum = 0;
                if (lst.Count > 0)
                    sumNum = (long)lst.Sum(x => x.quantity);


                if (sumNum == null)
                    sumNum = 0;

                var unit = entity.itemsUnits.Where(x => x.itemUnitId == smallestUnit.itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (upperUnit != null && upperUnit.itemUnitId != smallestUnit.itemUnitId)
                    sumNum += (int)upperUnit.unitValue * getLastNum(upperUnit.itemUnitId, invoiceIds);

                try
                {
                    return (int)sumNum;
                }
                catch
                {
                    return 0;
                }
            }
        }
        private long getLastNum(int itemUnitId, List<int> invoiceIds)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {

                var lst = entity.itemsTransfer.Where(x => x.itemUnitId == itemUnitId && invoiceIds.Contains((int)x.invoiceId))
                           .Select(t => new ItemLocationModel
                           {
                               quantity = t.quantity,
                           }).ToList();
                long sumNum = 0;
                if (lst.Count > 0)
                    sumNum = (long)lst.Sum(x => x.quantity);
                if (sumNum == null)
                    sumNum = 0;

                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (upperUnit != null)
                    sumNum += (int)upperUnit.unitValue * getLastNum(upperUnit.itemUnitId, invoiceIds);

                if (sumNum != null) return (long)sumNum;
                else
                    return 0;
            }
        }
        private long getItemUnitNum(int itemUnitId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var sumNum = (from b in entity.invoices
                              where b.invType.Contains("p")
                              join s in entity.itemsTransfer.Where(x => x.itemUnitId == itemUnitId) on b.invoiceId equals s.invoiceId
                              select s.quantity).Sum();

                if (sumNum == null)
                    sumNum = 0;

                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (upperUnit != null)
                    sumNum += (int)upperUnit.unitValue * getItemUnitNum(upperUnit.itemUnitId);

                if (sumNum != null) return (long)sumNum;
                else
                    return 0;
            }
        }
       
        private int getItemUnitTotalNum(List<int> itemUnits)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {

                var smallestUnitId = (from iu in entity.itemsUnits
                                      where (itemUnits.Contains((int)iu.itemUnitId) && iu.unitId == iu.subUnitId)
                                      select iu.itemUnitId).FirstOrDefault();

                if (smallestUnitId == null || smallestUnitId == 0)
                {
                    smallestUnitId = (from u in entity.itemsUnits
                                      where !entity.itemsUnits.Any(y => u.subUnitId == y.unitId)
                                      where (itemUnits.Contains((int)u.itemUnitId))
                                      select u.itemUnitId).FirstOrDefault();
                }
                var sumNum = (from b in entity.invoices
                              where b.invType == "p"
                              join s in entity.itemsTransfer.Where(x => x.itemUnitId == smallestUnitId) on b.invoiceId equals s.invoiceId
                              select s.quantity).Sum();

                if (sumNum == null)
                    sumNum = 0;

                var unit = entity.itemsUnits.Where(x => x.itemUnitId == smallestUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (upperUnit != null && upperUnit.itemUnitId != smallestUnitId)
                    sumNum += (int)upperUnit.unitValue * getItemUnitNum(upperUnit.itemUnitId);

                if (sumNum != null)
                    return (int)sumNum;
                else
                    return 0;
            }
        }
        public async Task<List<InvoiceModel>> getUnhandeledOrdersList(string invType, int branchCreatorId, int branchId, int duration = 0, int userId = 0)
        {
            ItemsTransferController ic = new ItemsTransferController();
            string[] invTypeArray = invType.Split(',');
            List<string> invTypeL = new List<string>();
            foreach (string s in invTypeArray)
                invTypeL.Add(s.Trim());

            using (incposdbEntities entity = new incposdbEntities())
            {
                var searchPredicate = PredicateBuilder.New<invoices>();
                searchPredicate = searchPredicate.And(inv => inv.isActive == true && invTypeL.Contains(inv.invType));
                if (duration > 0)
                {
                    DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-duration).ToShortDateString());
                    searchPredicate = searchPredicate.And(inv => inv.updateDate >= dt);
                }
                if (branchCreatorId != 0)
                    searchPredicate = searchPredicate.And(inv => inv.branchCreatorId == branchCreatorId && inv.isActive == true && invTypeL.Contains(inv.invType));

                if (branchId != 0)
                    searchPredicate = searchPredicate.And(inv => inv.branchId == branchId);
                if (userId != 0)
                    searchPredicate = searchPredicate.And(inv => inv.createUserId == userId);
                var invoicesList = (from b in entity.invoices.Where(searchPredicate)
                                    join u in entity.users on b.createUserId equals u.userId into uj
                                    from us in uj.DefaultIfEmpty()
                                    join l in entity.branches on b.branchId equals l.branchId into lj
                                    from x in lj.DefaultIfEmpty()
                                    join y in entity.branches on b.branchCreatorId equals y.branchId into yj
                                    from z in yj.DefaultIfEmpty()
                                    join a in entity.agents on b.agentId equals a.agentId into aj
                                    from ag in aj.DefaultIfEmpty()
                                    where !entity.invoices.Any(y => y.invoiceMainId == b.invoiceId)
                                    select new InvoiceModel()
                                    {
                                        invoiceId = b.invoiceId,
                                        invNumber = b.invNumber,
                                        agentId = b.agentId,
                                        notes = b.notes,
                                        agentName = ag.name,
                                        invType = b.invType,
                                        tax = b.tax,
                                        taxtype = b.taxtype,
                                        taxValue = b.taxValue,
                                        VATValue = b.VATValue,
                                        name = b.name,
                                        branchName = x.name,
                                        branchCreatorName = z.name,
                                        createrUserName = us.name + " " + us.lastname,
                                        totalNet = b.totalNet,
                                        total = b.total,
                                        discountType = b.discountType,
                                        DBDiscountValue = b.discountValue,
                                        manualDiscountType = b.manualDiscountType,
                                        manualDiscountValue = b.manualDiscountValue,
                                        realShippingCost = b.realShippingCost,
                                        shippingCost = b.shippingCost,
                                        updateUserId = b.updateUserId,
                                        isApproved = b.isApproved,
                                        branchId = b.branchId,
                                        branchCreatorId = b.branchCreatorId,
                                        invDate = b.invDate,
                                        isOrginal = b.isOrginal,
                                        printedcount = b.printedcount,
                                        isPrePaid = b.isPrePaid,
                                        updateDate = b.updateDate,
                                        sliceId = b.sliceId,
                                        sliceName = b.sliceName,
                                        isFreeShip = b.isFreeShip,

                                    }).OrderBy(x => x.invDate)
                .ToList();
                if (invoicesList != null)
                {
                    for (int i = 0; i < invoicesList.Count(); i++)
                    {

                        int invoiceId = invoicesList[i].invoiceId;
                        invoicesList[i].invoiceItems = await ic.Get(invoiceId);
                        invoicesList[i].itemsCount = invoicesList[i].invoiceItems.Count;
                        invoicesList[i].invoiceTaxes = GetInvoiceTaxes(invoiceId);

                    }
                }
                return invoicesList;
            }
        }
        public async Task<List<InvoiceModel>> getWaitingOrdersList(string invType, int branchId)
        {
            ItemsTransferController ic = new ItemsTransferController();
            string[] invTypeArray = invType.Split(',');
            List<string> invTypeL = new List<string>();
            foreach (string s in invTypeArray)
                invTypeL.Add(s.Trim());

            using (incposdbEntities entity = new incposdbEntities())
            {
                var searchPredicate = PredicateBuilder.New<invoices>();
                searchPredicate = searchPredicate.And(inv => inv.isActive == true && invTypeL.Contains(inv.invType));

                if (branchId != 0)
                    searchPredicate = searchPredicate.And(inv => inv.branchId == branchId);

                var invoicesList = (from b in entity.invoices.Where(searchPredicate)
                                    join u in entity.users on b.createUserId equals u.userId into uj
                                    from us in uj.DefaultIfEmpty()
                                    join l in entity.branches on b.branchId equals l.branchId into lj
                                    from x in lj.DefaultIfEmpty()
                                    join y in entity.branches on b.branchCreatorId equals y.branchId into yj
                                    from z in yj.DefaultIfEmpty()
                                    join a in entity.agents on b.agentId equals a.agentId into aj
                                    from ag in aj.DefaultIfEmpty()
                                    where !entity.invoices.Any(y => y.invoiceMainId == b.invoiceId)
                                    select new InvoiceModel()
                                    {
                                        invoiceId = b.invoiceId,
                                        invNumber = b.invNumber,
                                        agentId = b.agentId,
                                        notes = b.notes,
                                        agentName = ag.name,
                                        invType = b.invType,
                                        tax = b.tax,
                                        taxtype = b.taxtype,
                                        taxValue = b.taxValue,
                                        VATValue = b.VATValue,
                                        name = b.name,
                                        branchName = z.name,
                                        branchCreatorName = z.name,
                                        createrUserName = us.name + " " + us.lastname,
                                        totalNet = b.totalNet,
                                        total = b.total,
                                        discountType = b.discountType,
                                        DBDiscountValue = b.discountValue,
                                        manualDiscountType = b.manualDiscountType,
                                        manualDiscountValue = b.manualDiscountValue,
                                        realShippingCost = b.realShippingCost,
                                        shippingCost = b.shippingCost,
                                        updateUserId = b.updateUserId,
                                        isApproved = b.isApproved,
                                        branchId = b.branchId,
                                        shippingCompanyId = b.shippingCompanyId,
                                        shipUserId = b.shipUserId,
                                        isOrginal = b.isOrginal,
                                        printedcount = b.printedcount,
                                        branchCreatorId = b.branchCreatorId,
                                        updateDate = b.updateDate,
                                        isPrePaid = b.isPrePaid,
                                        sliceId = b.sliceId,
                                        sliceName = b.sliceName,
                                        isFreeShip = b.isFreeShip,

                                    })
                .ToList();
                if (invoicesList != null)
                {
                    for (int i = 0; i < invoicesList.Count(); i++)
                    {
                        int invoiceId = invoicesList[i].invoiceId;
                        invoicesList[i].invoiceItems = await ic.Get(invoiceId);
                        invoicesList[i].itemsCount = invoicesList[i].invoiceItems.Count;
                        invoicesList[i].invoiceTaxes = GetInvoiceTaxes(invoiceId);
                    }
                }
                return invoicesList;
            }
        }

        [HttpPost]
        [Route("checkOrderRedeaniss")]
        public string checkOrderRedeaniss(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int invoiceId = 0;
                string message = "1";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invoiceId")
                    {
                        invoiceId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var itemList = entity.itemsTransfer
                            .Where(x => x.invoiceId == invoiceId && x.itemsUnits.items.type != "sr").ToList();
                    foreach (itemsTransfer tr in itemList)
                    {
                        var lockedQuantity = entity.itemsLocations
                            .Where(x => x.invoiceId == invoiceId && x.itemUnitId == tr.itemUnitId && x.itemsUnits.items.type != "sr")
                            .Select(x => x.quantity).Sum();

                        if (lockedQuantity == null || lockedQuantity < tr.quantity)
                        {
                            message = "0";
                            break;
                        }
                    }
                }
                return TokenManager.GenerateToken(message);
            }
        }
        public decimal AvgItemPurPrice(int itemUnitId, int itemId)
        {

            decimal price = 0;
            int totalNum = 0;
            decimal smallUnitPrice = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemUnits = (from i in entity.itemsUnits where (i.itemId == itemId) select (i.itemUnitId)).ToList();

                price += getItemUnitSumPrice(itemUnits);

                totalNum = getItemUnitTotalNum(itemUnits);

                if (totalNum != 0)
                    smallUnitPrice = price / totalNum;

                var smallestUnitId = (from iu in entity.itemsUnits
                                      where (itemUnits.Contains((int)iu.itemUnitId) && iu.unitId == iu.subUnitId)
                                      select iu.itemUnitId).FirstOrDefault();

                if (smallestUnitId == null || smallestUnitId == 0)
                {
                    smallestUnitId = (from u in entity.itemsUnits
                                      where !entity.itemsUnits.Any(y => u.subUnitId == y.unitId)
                                      where (itemUnits.Contains((int)u.itemUnitId))
                                      select u.itemUnitId).FirstOrDefault();
                }
                if (itemUnitId == smallestUnitId || smallestUnitId == null || smallestUnitId == 0)
                    return smallUnitPrice;
                else
                {
                    smallUnitPrice = smallUnitPrice * getUpperUnitValue(smallestUnitId, itemUnitId);
                    return smallUnitPrice;
                }
            }


        }


        [HttpPost]
        [Route("distroyItem")]
        public async Task<string> distroyItem(string token)
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
                #region parameters
                string Object = "";
                string notificationObj = "";
                invoices newObject = null;
                inventoryItemLocation itemLocationInv = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                NotificationUserModel notificationUser = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        //Object = c.Value.Replace("\\", string.Empty);
                        //Object = Object.Trim('"');
                        Object = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }

                    else if (c.Type == "itemLocationInv")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        itemLocationInv = JsonConvert.DeserializeObject<inventoryItemLocation>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "not")
                    {
                        notificationObj = c.Value.Replace("\\", string.Empty);
                        notificationObj = notificationObj.Trim('"');
                        notificationUser = JsonConvert.DeserializeObject<NotificationUserModel>(notificationObj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        newObject = await SaveInvoice(newObject);

                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;
                        if (!invoiceId.Equals(0))
                        {
                            //save items transfer
                            ItemsTransferController it = new ItemsTransferController();
                            it.save(transferObject, invoiceId);

                            #region destroy item
                            InventoryItemLocationController iic = new InventoryItemLocationController();
                            iic.destroyItem(itemLocationInv);
                            #endregion

                            #region decrease item quantity
                            ItemsLocationsController ilc = new ItemsLocationsController();
                            ilc.decreaseItemLocationQuantity((int)itemLocationInv.itemLocationId, (int)itemLocationInv.amountDestroyed, (int)newObject.createUserId, notificationUser.objectName, notificationObj);
                            #endregion
                            #region record cash transfer
                            if (newObject.userId != null)
                            {
                                var paid = depositFromUserBalance((int)newObject.userId, newObject.invoiceId);
                                //if (paid > 0)
                                {
                                    CashTransferController cc = new CashTransferController();

                                    cashTransfer cashTrasnfer = new cashTransfer();
                                    cashTrasnfer.cash = newObject.total;
                                    cashTrasnfer.paid = 0;
                                    cashTrasnfer.deserved = newObject.total;
                                    cashTrasnfer.posId = newObject.posId;
                                    cashTrasnfer.userId = (int)newObject.userId;
                                    cashTrasnfer.invId = newObject.invoiceId;
                                    cashTrasnfer.createUserId = newObject.createUserId;
                                    cashTrasnfer.processType = "destroy";
                                    cashTrasnfer.isCommissionPaid = 0;
                                    cashTrasnfer.side = "u"; // user
                                    cashTrasnfer.transType = "p"; //deposit
                                    cashTrasnfer.transNum = "pu";
                                    await cc.addCashTransfer(cashTrasnfer);
                                }
                            }
                            #endregion
                        }
                    }
                }

                catch
                {
                    message = "0";
                }

                return TokenManager.GenerateToken(message);

            }
        }

        [HttpPost]
        [Route("shortageItem")]
        public async Task<string> shortageItem(string token)
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
                #region parameters
                string Object = "";
                string notificationObj = "";
                invoices newObject = null;
                inventoryItemLocation itemLocationInv = null;
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                NotificationUserModel notificationUser = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        //Object = c.Value.Replace("\\", string.Empty);
                        //Object = Object.Trim('"');
                        Object = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }

                    else if (c.Type == "itemLocationInv")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        itemLocationInv = JsonConvert.DeserializeObject<inventoryItemLocation>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "not")
                    {
                        notificationObj = c.Value.Replace("\\", string.Empty);
                        notificationObj = notificationObj.Trim('"');
                        notificationUser = JsonConvert.DeserializeObject<NotificationUserModel>(notificationObj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        newObject = await SaveInvoice(newObject);

                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;
                        if (!invoiceId.Equals(0))
                        {
                            //save items transfer
                            ItemsTransferController it = new ItemsTransferController();
                            it.save(transferObject, invoiceId);

                            #region shortage item
                            InventoryItemLocationController iic = new InventoryItemLocationController();
                            iic.fallItem(itemLocationInv);
                            #endregion
                            #region decrease item quantity
                            ItemsLocationsController ilc = new ItemsLocationsController();
                            ilc.decreaseItemLocationQuantity((int)itemLocationInv.itemLocationId, (int)itemLocationInv.amount, (int)newObject.createUserId, notificationUser.objectName, notificationObj);
                            #endregion
                            #region record cash transfer
                            if (newObject.userId != null)
                            {
                                var paid = depositFromUserBalance((int)newObject.userId, newObject.invoiceId);
                                //if (paid > 0)
                                {
                                    CashTransferController cc = new CashTransferController();

                                    cashTransfer cashTrasnfer = new cashTransfer();
                                    cashTrasnfer.cash = newObject.total;
                                    cashTrasnfer.paid = 0;
                                    cashTrasnfer.deserved = newObject.total ;
                                    cashTrasnfer.posId = newObject.posId;
                                    cashTrasnfer.userId = (int)newObject.userId;
                                    cashTrasnfer.invId = newObject.invoiceId;
                                    cashTrasnfer.createUserId = newObject.createUserId;
                                    cashTrasnfer.processType = "shortage";
                                    cashTrasnfer.isCommissionPaid = 0;
                                    cashTrasnfer.side = "u"; // user
                                    cashTrasnfer.transType = "p"; //deposit
                                    cashTrasnfer.transNum = "pu";
                                    await cc.addCashTransfer(cashTrasnfer);
                                }
                            }
                            #endregion
                        }
                    }
                }

                catch
                {
                    message = "0";
                }

                return TokenManager.GenerateToken(message);

            }
        }

        [HttpPost]
        [Route("manualDistroyItem")]
        public async Task<string> manualDistroyItem(string token)
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
                #region parameters
                string Object = "";
                string notificationObj = "";
                invoices newObject = null;
                List<itemsLocations> itemsLoc = new List<itemsLocations>();
                List<itemsTransfer> transferObject = new List<itemsTransfer>();
                List<ItemTransferModel> billDetails = new List<ItemTransferModel>();
                NotificationUserModel notificationUser = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        //Object = c.Value.Replace("\\", string.Empty);
                        //Object = Object.Trim('"');
                        Object = c.Value;
                        newObject = JsonConvert.DeserializeObject<invoices>(Object, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
                    }
                    else if (c.Type == "itemTransferObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        transferObject = JsonConvert.DeserializeObject<List<itemsTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        billDetails = JsonConvert.DeserializeObject<List<ItemTransferModel>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }

                    else if (c.Type == "itemsLoc")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        itemsLoc = JsonConvert.DeserializeObject<List<itemsLocations>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "not")
                    {
                        notificationObj = c.Value.Replace("\\", string.Empty);
                        notificationObj = notificationObj.Trim('"');
                        notificationUser = JsonConvert.DeserializeObject<NotificationUserModel>(notificationObj, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                }
                #endregion
                try
                {
                    ProgramDetailsController pc = new ProgramDetailsController();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        #region check items quantity in store
                        ItemsLocationsController itc = new ItemsLocationsController();
                        foreach (var itemLoc in itemsLoc)
                        {
                            int itemLocId = itemLoc.itemsLocId;
                            int quantity = (int)itemLoc.quantity;
                            string res = itc.checkLocationAmounts(itemLocId, quantity);
                            if (!res.Equals(""))
                            {
                                return TokenManager.GenerateToken("-3");
                            }
                        }


                        #endregion
                        newObject = await SaveInvoice(newObject);

                        message = newObject.invoiceId.ToString();
                        int invoiceId = newObject.invoiceId;
                        if (!invoiceId.Equals(0))
                        {
                            //save items transfer
                            ItemsTransferController it = new ItemsTransferController();
                            it.save(transferObject, invoiceId);

                            #region decrease item quantity

                            foreach (var itemLoc in itemsLoc)
                            {
                                int itemLocId = itemLoc.itemsLocId;
                                int quantity = (int)itemLoc.quantity;
                                itc.decreaseItemLocationQuantity((int)itemLocId, (int)quantity, (int)newObject.createUserId, notificationUser.objectName, notificationObj);
                            }
                            #endregion
                            #region record cash transfer
                            if (newObject.userId != null)
                            {
                                var paid = depositFromUserBalance((int)newObject.userId, newObject.invoiceId);
                               //if (paid > 0)
                                {
                                    CashTransferController cc = new CashTransferController();

                                    cashTransfer cashTrasnfer = new cashTransfer();
                                    cashTrasnfer.cash = newObject.total;
                                    cashTrasnfer.paid = 0;
                                    cashTrasnfer.deserved = newObject.total;
                                    cashTrasnfer.posId = newObject.posId;
                                    cashTrasnfer.userId = (int)newObject.userId;
                                    cashTrasnfer.invId = newObject.invoiceId;
                                    cashTrasnfer.createUserId = newObject.createUserId;
                                    cashTrasnfer.processType = "destroy";
                                    cashTrasnfer.side = "u"; // user
                                    cashTrasnfer.transType = "p"; //deposit
                                    cashTrasnfer.transNum = "pu";
                                    cashTrasnfer.isCommissionPaid = 0;

                                    await cc.addCashTransfer(cashTrasnfer);
                                }
                            }
                            #endregion
                        }
                    }
                }

                catch
                {
                    message = "0";
                }

                return TokenManager.GenerateToken(message);

            }
        }

        public decimal depositFromUserBalance(int userId, int invoiceId)
        {
            decimal paid = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {
                var user = entity.users.Find(userId);
                var invoice = entity.invoices.Find(invoiceId);

                if (user.balanceType == 0)
                {
                    if (invoice.totalNet <= (decimal)user.balance)
                    {
                        invoice.paid = invoice.totalNet;
                        invoice.deserved = 0;
                        user.balance -= (decimal)invoice.totalNet;
                    }
                    else
                    {
                        invoice.paid = (decimal)user.balance;
                        invoice.deserved = invoice.totalNet - (decimal)user.balance;
                        decimal newBalance = (decimal)invoice.totalNet - (decimal)user.balance;
                        user.balance = newBalance;
                        user.balanceType = 1;
                    }

                    paid = (decimal)invoice.paid;

                    entity.SaveChanges();
                }
                else if (user.balanceType == 1)
                {
                    decimal newBalance = (decimal)user.balance + (decimal)invoice.totalNet;
                    user.balance = newBalance;
                    entity.SaveChanges();
                }
                return paid;
            }
        }
    }
}