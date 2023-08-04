using LinqKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using POS_Server.Models.VM;
using System.Security.Claims;
using System.Web;
using Newtonsoft.Json.Converters;
using POS_Server.Classes;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/Cashtransfer")]
    public class CashTransferController : ApiController
    {
        CountriesController cc = new CountriesController();

        List<string> hiddenCashes = new List<string>() { "inv", "deliver", "commissionAgent", "commissionCard" };
        [HttpPost]
        [Route("GetBytypeandSide")]
        public string GetBytypeAndSide(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string type = "";
                string side = "";

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                    else if (c.Type == "side")
                    {
                        side = c.Value;
                    }


                }

                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
                                                            join b in entity.banks on C.bankId equals b.bankId into jb
                                                            join a in entity.agents on C.agentId equals a.agentId into ja
                                                            join p in entity.pos on C.posId equals p.posId into jp
                                                            join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                                            join u in entity.users on C.userId equals u.userId into ju
                                                            join uc in entity.users on C.createUserId equals uc.userId into juc
                                                            join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                                            join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                                            join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
                                                            from jbb in jb.DefaultIfEmpty()
                                                            from jaa in ja.DefaultIfEmpty()
                                                            from jpp in jp.DefaultIfEmpty()
                                                            from juu in ju.DefaultIfEmpty()
                                                            from jpcc in jpcr.DefaultIfEmpty()
                                                            from jucc in juc.DefaultIfEmpty()
                                                            from jcrd in jcr.DefaultIfEmpty()
                                                            from jbbo in jbo.DefaultIfEmpty()
                                                            from jssh in jsh.DefaultIfEmpty()
                                                            select new CashTransferModel()
                                                            {
                                                                cashTransId = C.cashTransId,
                                                                transType = C.transType,
                                                                posId = C.posId,
                                                                userId = C.userId,
                                                                agentId = C.agentId,
                                                                invId = C.invId,
                                                                transNum = C.transNum,
                                                                createDate = C.createDate,
                                                                updateDate = C.updateDate,
                                                                cash = C.cash,
                                                                updateUserId = C.updateUserId,
                                                                createUserId = C.createUserId,
                                                                notes = C.notes,
                                                                posIdCreator = C.posIdCreator,
                                                                isConfirm = C.isConfirm,
                                                                cashTransIdSource = C.cashTransIdSource,
                                                                side = C.side,

                                                                docName = C.docName,
                                                                docNum = C.docNum,
                                                                docImage = C.docImage,
                                                                bankId = C.bankId,
                                                                bankName = jbb.name,
                                                                agentName = jaa.name,
                                                                usersName = juu.name,// side =u

                                                                posName = jpp.name,
                                                                posCreatorName = jpcc.name,
                                                                processType = C.processType,
                                                                cardId = C.cardId,
                                                                bondId = C.bondId,
                                                                usersLName = juu.lastname,// side =u
                                                                createUserName = jucc.name,
                                                                createUserLName = jucc.lastname,
                                                                createUserJob = jucc.job,
                                                                cardName = jcrd.name,
                                                                bondDeserveDate = jbbo.deserveDate,
                                                                bondIsRecieved = jbbo.isRecieved,
                                                                shippingCompanyId = C.shippingCompanyId,
                                                                shippingCompanyName = jssh.name,
                                                                commissionValue = C.commissionValue,
                                                                commissionRatio = C.commissionRatio,

                                                            }).Where(C => ((type == "all") ? true : C.transType == type) && (C.processType != "balance")
                && ((side == "all") ? true : C.side == side) && !(C.agentId == null && C.userId == null && C.shippingCompanyId == null)).ToList();

                        if (cachlist.Count > 0 && side == "p")
                        {
                            CashTransferModel tempitem = null;
                            foreach (CashTransferModel cashtItem in cachlist)
                            {
                                tempitem = this.Getpostransmodel(cashtItem.cashTransId)
                                    .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
                                cashtItem.cashTrans2Id = tempitem.cashTransId;
                                cashtItem.pos2Id = tempitem.posId;
                                cashtItem.pos2Name = tempitem.posName;
                                cashtItem.isConfirm2 = tempitem.isConfirm;
                                // cashtItem.posCreatorName = tempitem.posName;


                            }

                        }


                        return TokenManager.GenerateToken(cachlist);


                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        [HttpPost]
        [Route("GetCashBond")]
        public string GetCashBond(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string type = "";
                string side = "";

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                    else if (c.Type == "side")
                    {
                        side = c.Value;
                    }


                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
                                                            join b in entity.banks on C.bankId equals b.bankId into jb
                                                            join a in entity.agents on C.agentId equals a.agentId into ja
                                                            join p in entity.pos on C.posId equals p.posId into jp
                                                            join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                                            join u in entity.users on C.userId equals u.userId into ju
                                                            join uc in entity.users on C.createUserId equals uc.userId into juc
                                                            join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                                            join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                                            join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
                                                            join INV in entity.invoices on C.invId equals INV.invoiceId into jINV
                                                            from jbb in jb.DefaultIfEmpty()
                                                            from jaa in ja.DefaultIfEmpty()
                                                            from jpp in jp.DefaultIfEmpty()
                                                            from juu in ju.DefaultIfEmpty()
                                                            from jpcc in jpcr.DefaultIfEmpty()
                                                            from jucc in juc.DefaultIfEmpty()
                                                            from jcrd in jcr.DefaultIfEmpty()
                                                            from jbbo in jbo.DefaultIfEmpty()
                                                            from jssh in jsh.DefaultIfEmpty()
                                                            from INVOIC in jINV.DefaultIfEmpty()
                                                            select new CashTransferModel()
                                                            {
                                                                cashTransId = C.cashTransId,
                                                                transType = C.transType,
                                                                posId = C.posId,
                                                                userId = C.userId,
                                                                agentId = C.agentId,
                                                                invId = C.invId,
                                                                transNum = C.transNum,
                                                                createDate = C.createDate,
                                                                updateDate = C.updateDate,
                                                                cash = C.cash,
                                                                updateUserId = C.updateUserId,
                                                                createUserId = C.createUserId,
                                                                notes = C.notes,
                                                                posIdCreator = C.posIdCreator,
                                                                isConfirm = C.isConfirm,
                                                                cashTransIdSource = C.cashTransIdSource,
                                                                side = C.side,

                                                                docName = C.docName,
                                                                docNum = C.docNum,
                                                                docImage = C.docImage,
                                                                bankId = C.bankId,
                                                                bankName = jbb.name,
                                                                agentName = jaa.name,
                                                                usersName = juu.name,// side =u

                                                                posName = jpp.name,
                                                                posCreatorName = jpcc.name,
                                                                processType = C.processType,
                                                                cardId = C.cardId,
                                                                bondId = C.bondId,
                                                                usersLName = juu.lastname,// side =u
                                                                createUserName = jucc.name,
                                                                createUserLName = jucc.lastname,
                                                                createUserJob = jucc.job,
                                                                cardName = jcrd.name,
                                                                bondDeserveDate = jbbo.deserveDate,
                                                                bondIsRecieved = jbbo.isRecieved,
                                                                shippingCompanyId = C.shippingCompanyId,
                                                                shippingCompanyName = jssh.name,
                                                                commissionValue = C.commissionValue,
                                                                commissionRatio = C.commissionRatio,
                                                                invNumber = INVOIC.invNumber,
                                                                invType = INVOIC.invType,
                                                                isInvPurpose = C.isInvPurpose,
                                                                purpose = C.purpose,
                                                                otherSide = C.otherSide,
                                                            }).Where(C => ((type == "all") ? true : C.transType == type) && ((side == "all") ? true : C.side == side) && C.processType!="statement").ToList();



                        return TokenManager.GenerateToken(cachlist);


                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }
        [Route("getNotPaidDeliverCashes")]
        [HttpPost]
        public string getNotPaidDeliverCashes(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int shippingComId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "shippingComId")
                    {
                        shippingComId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    List<CashTransferModel> res = new List<CashTransferModel>();
                    var cashes = (from b in entity.cashTransfer.Where(x => x.shippingCompanyId == shippingComId
                                                                    && x.processType.Trim() == "deliver" && x.deserved > 0)
                                  select new CashTransferModel()
                                  {
                                      cashTransId = b.cashTransId,
                                      invId = b.invId,
                                      agentId = b.agentId,
                                      cash = b.cash,
                                      deserved = b.deserved,
                                      paid = b.paid,
                                      notes = b.notes,
                                      createUserId = b.createUserId,
                                      updateDate = b.updateDate,
                                      updateUserId = b.updateUserId,
                                      shippingCompanyId = b.shippingCompanyId,
                                      transNum = b.transNum,


                                  }).ToList();

                    foreach (var cash in cashes)
                    {
                        var statusObj = entity.invoiceStatus.Where(x => x.invoiceId == cash.invId && x.status == "Done").FirstOrDefault();
                        if (statusObj != null)
                        {
                            res.Add(cash);
                        }
                    }
                    return TokenManager.GenerateToken(res);
                }
            }
        }
        [HttpPost]
        [Route("GetCashTransfer")]
        public string GetCashTransfer(string token)
        {
            //string type, string side

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string type = "";
                string side = "";

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                    else if (c.Type == "side")
                    {
                        side = c.Value;
                    }


                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
                                                            select new CashTransferModel()
                                                            {
                                                                cashTransId = C.cashTransId,
                                                                transType = C.transType,
                                                                posId = C.posId,
                                                                userId = C.userId,
                                                                agentId = C.agentId,
                                                                invId = C.invId,
                                                                transNum = C.transNum,
                                                                createDate = C.createDate,
                                                                updateDate = C.updateDate,
                                                                cash = C.cash,
                                                                updateUserId = C.updateUserId,
                                                                createUserId = C.createUserId,
                                                                notes = C.notes,
                                                                posIdCreator = C.posIdCreator,
                                                                isConfirm = C.isConfirm,
                                                                cashTransIdSource = C.cashTransIdSource,
                                                                side = C.side,

                                                                docName = C.docName,
                                                                docNum = C.docNum,
                                                                docImage = C.docImage,
                                                                bankId = C.bankId,
                                                                processType = C.processType,
                                                                cardId = C.cardId,
                                                                bondId = C.bondId,
                                                                commissionValue = C.commissionValue,
                                                                commissionRatio = C.commissionRatio,

                                                            }).Where(C => ((type == "all") ? true : C.transType == type) && ((side == "all") ? true : C.side == side)).ToList();

                        return TokenManager.GenerateToken(cachlist);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        [HttpPost]
        [Route("GetPayedByInvId")]
        public string GetPayedByInvId(string token)
        {
            //  string token

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                int invId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invId")
                    {
                        invId = int.Parse(c.Value);
                    }


                }

                try
                {
                    var cachtranslist = GetPayedByInvId(invId);
                    return TokenManager.GenerateToken(cachtranslist);
                    //using (incposdbEntities entity = new incposdbEntities())
                    //{
                    //    List<CashTransferModel> cachtrans = new List<CashTransferModel>();
                    //    cachtrans = (from C in entity.cashTransfer
                    //                 join b in entity.cards on C.cardId equals b.cardId into jb

                    //                 from Card in jb.DefaultIfEmpty()


                    //                 select new CashTransferModel()
                    //                 {
                    //                     cashTransId = C.cashTransId,
                    //                     transType = C.transType,

                    //                     invId = C.invId,


                    //                     cash = C.cash,


                    //                     cardName = Card.name,
                    //                     processType = C.processType,
                    //                     cardId = C.cardId,


                    //                 }).Where(C => C.invId == invId && (C.processType == "card" || C.processType == "cash")).ToList();

                    //    int i = 0;
                    //    var cachtranslist = cachtrans.GroupBy(x => x.cardId).Select(x => new {
                    //        processType = x.FirstOrDefault().processType,

                    //        cash = x.Sum(c => c.cash),
                    //        cardId = x.FirstOrDefault().cardId,
                    //        cardName = x.FirstOrDefault().processType == "card" ? x.FirstOrDefault().cardName : "cash",
                    //        sequenc = x.FirstOrDefault().processType == "cash" ? 0 : ++i,
                    //    }).OrderBy(c => c.cardId).ToList();


                    //    return TokenManager.GenerateToken(cachtranslist);
                    //}
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }



            }

        }

        [NonAction]
        public List<PayedInvclass> GetPayedByInvId(int invId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                InvoicesController ic = new InvoicesController();
                var invoice = entity.invoices.Where(x => x.invoiceId == invId)
                          .Select(x => new InvoiceModel()
                          {
                              invoiceId = x.invoiceId,
                              invoiceMainId = x.invoiceMainId,
                              invType = x.invType,
                          }).FirstOrDefault();

                if (invoice.invType.Equals("s") || invoice.invType.Equals("p"))
                {
                    while (invoice != null)
                    {
                        invId = invoice.invoiceId;

                        if (invoice.invoiceMainId != null)
                            invoice = ic.GetParentInv((int)invoice.invoiceId);
                        else
                            break;
                    }
                }
                List<CashTransferModel> cachtrans = new List<CashTransferModel>();
                cachtrans = (from C in entity.cashTransfer
                             join b in entity.cards on C.cardId equals b.cardId into jb

                             from Card in jb.DefaultIfEmpty()
                             select new CashTransferModel()
                             {
                                 cashTransId = C.cashTransId,
                                 transType = C.transType,
                                 invId = C.invId,
                                 cash = C.cash,
                                 docNum = C.docNum,
                                 //    cardName = Card.name,
                                 processType = C.processType,
                                 cardId = C.cardId,
                                 commissionRatio = C.commissionRatio,
                                 commissionValue = C.commissionValue,
                                 cardName = C.processType == "card" ? Card.name : "cash",
                                 hasProcessNum = Card.hasProcessNum,
                             }).Where(C => C.invId == invId && (C.processType == "card" || C.processType == "cash")).ToList();

                int i = 0;
                List<PayedInvclass> Payedlist = new List<PayedInvclass>();
                // cachtranslist.Add
                //cash
                List<PayedInvclass> cachprocess = cachtrans.GroupBy(x => x.cardId).Select(x => new PayedInvclass
                {
                    processType = x.FirstOrDefault().processType,

                    cash = x.Sum(c => c.cash),
                    cardId = x.FirstOrDefault().cardId == null ? 0 : x.FirstOrDefault().cardId,
                    cardName = x.FirstOrDefault().cardName,
                    sequenc = x.FirstOrDefault().processType == "cash" ? 0 : ++i,
                    commissionRatio = x.FirstOrDefault().commissionRatio,
                    commissionValue = x.FirstOrDefault().commissionValue,
                    docNum = x.FirstOrDefault().docNum,
                }).OrderBy(c => c.cardId).ToList().Where(x => x.processType == "cash").ToList();
                //all card
                List<PayedInvclass> cardprocess = cachtrans.Select(x => new PayedInvclass
                {
                    processType = x.processType,
                    cash = x.cash,
                    cardId = x.cardId,
                    cardName = x.cardName,
                    sequenc = x.processType == "cash" ? 0 : ++i,
                    commissionRatio = x.commissionRatio,
                    commissionValue = x.commissionValue,
                    docNum = x.docNum,
                }).OrderBy(c => c.cardId).ToList().Where(x => x.processType == "card").ToList();
                //card has process num -no group
                List<PayedInvclass> cardhasprocessnum = cardprocess.Where(x => x.processType == "card" && (x.docNum != "" && x.docNum != null)).ToList();
                // card has No Process num - group
                List<PayedInvclass> cardhasNoprocessnum = cardprocess.Where(x => x.processType == "card" && (x.docNum == "" || x.docNum == null)).ToList();
                //group
                List<PayedInvclass> NoprocessnumGroup = cardhasNoprocessnum.GroupBy(x => x.cardId).Select(x => new PayedInvclass
                {
                    processType = x.FirstOrDefault().processType,
                    cash = x.Sum(c => c.cash),
                    cardId = x.FirstOrDefault().cardId,
                    cardName = x.FirstOrDefault().cardName,
                    sequenc = x.FirstOrDefault().sequenc,
                    commissionRatio = x.FirstOrDefault().commissionRatio,
                    commissionValue = x.FirstOrDefault().commissionValue,
                    docNum = x.FirstOrDefault().docNum,
                }).OrderBy(c => c.cardId).ToList();
                //add cash row
                if (cachprocess.Count() > 0)
                {
                    Payedlist.Add(cachprocess.FirstOrDefault());
                }
                //add card with process num
                Payedlist.AddRange(cardhasprocessnum);
                //add card no process num
                Payedlist.AddRange(NoprocessnumGroup);

                return Payedlist;
            }
        }

        [HttpPost]
        [Route("GetBytypeAndSideForPos")]
        public string GetBytypeAndSideForPos(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string type = "";
                string side = "";

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                    else if (c.Type == "side")
                    {
                        side = c.Value;
                    }


                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
                                                            join b in entity.banks on C.bankId equals b.bankId into jb
                                                            join a in entity.agents on C.agentId equals a.agentId into ja
                                                            join p in entity.pos on C.posId equals p.posId into jp
                                                            join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                                            join u in entity.users on C.userId equals u.userId into ju
                                                            join uc in entity.users on C.createUserId equals uc.userId into juc
                                                            join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                                            join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                                            join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
                                                            from jbb in jb.DefaultIfEmpty()
                                                            from jaa in ja.DefaultIfEmpty()
                                                            from jpp in jp.DefaultIfEmpty()
                                                            from juu in ju.DefaultIfEmpty()
                                                            from jpcc in jpcr.DefaultIfEmpty()
                                                            from jucc in juc.DefaultIfEmpty()
                                                            from jcrd in jcr.DefaultIfEmpty()
                                                            from jbbo in jbo.DefaultIfEmpty()
                                                            from jssh in jsh.DefaultIfEmpty()
                                                            select new CashTransferModel()
                                                            {
                                                                cashTransId = C.cashTransId,
                                                                transType = C.transType,
                                                                posId = C.posId,
                                                                userId = C.userId,
                                                                agentId = C.agentId,
                                                                invId = C.invId,
                                                                transNum = C.transNum,
                                                                createDate = C.createDate,
                                                                updateDate = C.updateDate,
                                                                cash = C.cash,
                                                                updateUserId = C.updateUserId,
                                                                createUserId = C.createUserId,
                                                                notes = C.notes,
                                                                posIdCreator = C.posIdCreator,
                                                                isConfirm = C.isConfirm,
                                                                cashTransIdSource = C.cashTransIdSource,
                                                                side = C.side,

                                                                docName = C.docName,
                                                                docNum = C.docNum,
                                                                docImage = C.docImage,
                                                                bankId = C.bankId,
                                                                bankName = jbb.name,
                                                                agentName = jaa.name,
                                                                usersName = juu.name,// side =u

                                                                posName = jpp.name,
                                                                posCreatorName = jpcc.name,
                                                                processType = C.processType,
                                                                cardId = C.cardId,
                                                                bondId = C.bondId,
                                                                usersLName = juu.lastname,// side =u
                                                                createUserName = jucc.name,
                                                                createUserLName = jucc.lastname,
                                                                createUserJob = jucc.job,
                                                                cardName = jcrd.name,
                                                                bondDeserveDate = jbbo.deserveDate,
                                                                bondIsRecieved = jbbo.isRecieved,
                                                                shippingCompanyId = C.shippingCompanyId,
                                                                shippingCompanyName = jssh.name,
                                                                commissionValue = C.commissionValue,
                                                                commissionRatio = C.commissionRatio,

                                                            }).Where(C => ((type == "all") ? true : C.transType == type) && (C.processType != "balance")
                && ((side == "all") ? true : C.side == side)).ToList();

                        if (cachlist.Count > 0 && side == "p")
                        {
                            CashTransferModel tempitem = null;
                            foreach (CashTransferModel cashtItem in cachlist)
                            {
                                tempitem = this.Getpostransmodel(cashtItem.cashTransId)
                                    .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
                                cashtItem.cashTrans2Id = tempitem.cashTransId;
                                cashtItem.pos2Id = tempitem.posId;
                                cashtItem.pos2Name = tempitem.posName;
                                cashtItem.isConfirm2 = tempitem.isConfirm;

                            }

                        }


                        return TokenManager.GenerateToken(cachlist);


                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }

            //var re = Request;
            //var headers = re.Headers;
            //string token = "";

            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}



            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid)
            //{
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
            //                                            join b in entity.banks on C.bankId equals b.bankId into jb
            //                                            join a in entity.agents on C.agentId equals a.agentId into ja
            //                                            join p in entity.pos on C.posId equals p.posId into jp
            //                                            join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
            //                                            join u in entity.users on C.userId equals u.userId into ju
            //                                            join uc in entity.users on C.createUserId equals uc.userId into juc
            //                                            join cr in entity.cards on C.cardId equals cr.cardId into jcr
            //                                            join bo in entity.bondes on C.bondId equals bo.bondId into jbo
            //                                            join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
            //                                            from jbb in jb.DefaultIfEmpty()
            //                                            from jaa in ja.DefaultIfEmpty()
            //                                            from jpp in jp.DefaultIfEmpty()
            //                                            from juu in ju.DefaultIfEmpty()
            //                                            from jpcc in jpcr.DefaultIfEmpty()
            //                                            from jucc in juc.DefaultIfEmpty()
            //                                            from jcrd in jcr.DefaultIfEmpty()
            //                                            from jbbo in jbo.DefaultIfEmpty()
            //                                            from jssh in jsh.DefaultIfEmpty()
            //                                            select new CashTransferModel()
            //                                            {
            //                                                cashTransId = C.cashTransId,
            //                                                transType = C.transType,
            //                                                posId = C.posId,
            //                                                userId = C.userId,
            //                                                agentId = C.agentId,
            //                                                invId = C.invId,
            //                                                transNum = C.transNum,
            //                                                createDate = C.createDate,
            //                                                updateDate = C.updateDate,
            //                                                cash = C.cash,
            //                                                updateUserId = C.updateUserId,
            //                                                createUserId = C.createUserId,
            //                                                notes = C.notes,
            //                                                posIdCreator = C.posIdCreator,
            //                                                isConfirm = C.isConfirm,
            //                                                cashTransIdSource = C.cashTransIdSource,
            //                                                side = C.side,

            //                                                docName = C.docName,
            //                                                docNum = C.docNum,
            //                                                docImage = C.docImage,
            //                                                bankId = C.bankId,
            //                                                bankName = jbb.name,
            //                                                agentName = jaa.name,
            //                                                usersName = juu.name,// side =u

            //                                                posName = jpp.name,
            //                                                posCreatorName = jpcc.name,
            //                                                processType = C.processType,
            //                                                cardId = C.cardId,
            //                                                bondId = C.bondId,
            //                                                usersLName = juu.lastname,// side =u
            //                                                createUserName = jucc.name,
            //                                                createUserLName = jucc.lastname,
            //                                                createUserJob = jucc.job,
            //                                                cardName = jcrd.name,
            //                                                bondDeserveDate = jbbo.deserveDate,
            //                                                bondIsRecieved = jbbo.isRecieved,
            //                                                shippingCompanyId = C.shippingCompanyId,
            //                                                shippingCompanyName = jssh.name

            //                                            }).Where(C => ((type == "all") ? true : C.transType == type)
            //&& ((side == "all") ? true : C.side == side)).ToList();

            //        if (cachlist.Count > 0 && side == "p")
            //        {
            //            CashTransferModel tempitem = null;
            //            foreach (CashTransferModel cashtItem in cachlist)
            //            {
            //                tempitem = this.Getpostransmodel(cashtItem.cashTransId)
            //                    .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
            //                cashtItem.cashTrans2Id = tempitem.cashTransId;
            //                cashtItem.pos2Id = tempitem.posId;
            //                cashtItem.pos2Name = tempitem.posName;
            //                cashtItem.isConfirm2 = tempitem.isConfirm;

            //            }

            //        }




            //        if (cachlist == null)
            //            return NotFound();
            //        else
            //            return Ok(cachlist);

            //    }
            //}
            //else
            //    return NotFound();
        }

        //
        [HttpPost]
        [Route("GetCashTransferForPosById")]
        public string GetCashTransferForPosById(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string type = "";
                string side = "";
                int posId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                    else if (c.Type == "side")
                    {
                        side = c.Value;
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }

                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
                                                            join b in entity.banks on C.bankId equals b.bankId into jb
                                                            join a in entity.agents on C.agentId equals a.agentId into ja
                                                            join p in entity.pos on C.posId equals p.posId into jp
                                                            join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                                            join u in entity.users on C.userId equals u.userId into ju
                                                            join uc in entity.users on C.createUserId equals uc.userId into juc
                                                            join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                                            join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                                            join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
                                                            from jbb in jb.DefaultIfEmpty()
                                                            from jaa in ja.DefaultIfEmpty()
                                                            from jpp in jp.DefaultIfEmpty()
                                                            from juu in ju.DefaultIfEmpty()
                                                            from jpcc in jpcr.DefaultIfEmpty()
                                                            from jucc in juc.DefaultIfEmpty()
                                                            from jcrd in jcr.DefaultIfEmpty()
                                                            from jbbo in jbo.DefaultIfEmpty()
                                                            from jssh in jsh.DefaultIfEmpty()
                                                            select new CashTransferModel()
                                                            {
                                                                cashTransId = C.cashTransId,
                                                                transType = C.transType,
                                                                posId = C.posId,
                                                                userId = C.userId,
                                                                agentId = C.agentId,
                                                                invId = C.invId,
                                                                transNum = C.transNum,
                                                                createDate = C.createDate,
                                                                updateDate = C.updateDate,
                                                                cash = C.cash,
                                                                updateUserId = C.updateUserId,
                                                                createUserId = C.createUserId,
                                                                notes = C.notes,
                                                                posIdCreator = C.posIdCreator,
                                                                isConfirm = C.isConfirm,
                                                                cashTransIdSource = C.cashTransIdSource,
                                                                side = C.side,

                                                                docName = C.docName,
                                                                docNum = C.docNum,
                                                                docImage = C.docImage,
                                                                bankId = C.bankId,
                                                                bankName = jbb.name,
                                                                agentName = jaa.name,
                                                                usersName = juu.name,// side =u

                                                                posName = jpp.name,
                                                                posCreatorName = jpcc.name,
                                                                processType = C.processType,
                                                                cardId = C.cardId,
                                                                bondId = C.bondId,
                                                                usersLName = juu.lastname,// side =u
                                                                createUserName = jucc.name,
                                                                createUserLName = jucc.lastname,
                                                                createUserJob = jucc.job,
                                                                cardName = jcrd.name,
                                                                bondDeserveDate = jbbo.deserveDate,
                                                                bondIsRecieved = jbbo.isRecieved,
                                                                shippingCompanyId = C.shippingCompanyId,
                                                                shippingCompanyName = jssh.name,
                                                                commissionValue = C.commissionValue,
                                                                commissionRatio = C.commissionRatio,

                                                            }).Where(C => ((type == "all") ? true : C.transType == type) && (C.processType != "balance")
                && ((side == "all") ? true : C.side == side)).ToList();

                        if (cachlist.Count > 0 && side == "p")
                        {
                            CashTransferModel tempitem = null;
                            foreach (CashTransferModel cashtItem in cachlist)
                            {
                                tempitem = this.Getpostransmodel(cashtItem.cashTransId)
                                    .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
                                cashtItem.cashTrans2Id = tempitem.cashTransId;
                                cashtItem.pos2Id = tempitem.posId;
                                cashtItem.pos2Name = tempitem.posName;
                                cashtItem.isConfirm2 = tempitem.isConfirm;

                            }

                        }
                        cachlist = cachlist.Where(C => (C.posId == posId || C.pos2Id == posId || C.posIdCreator == posId)).ToList();

                        return TokenManager.GenerateToken(cachlist);


                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }
        //
        // get by bondId
        [HttpPost]
        [Route("GetNotConfirmdByPosId")]
        public string GetNotConfirmdByPosId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string type = "";
                string side = "";
                int posId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                    else if (c.Type == "side")
                    {
                        side = c.Value;
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }

                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {


                    List<CashTransferModel> cachlist = new List<CashTransferModel>();
                    cachlist = GetCashTransferForPosById(posId, type, side);
                    cachlist = cachlist.Where(C => (C.posId == posId && C.isConfirm == 0)).ToList();
                    //|| ( C.posIdCreator == posId &&(C.posId!=posId && C.pos2Id !=posId) &&(C.isConfirm == 0 || C.isConfirm2 == 0) )
                    return TokenManager.GenerateToken(cachlist);
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        [NonAction]
        public int GetCountNotConfirmdByPosId(int posId,string side, string type)
        {

            try
            {
                List<CashTransferModel> cachlist = new List<CashTransferModel>();
                cachlist = GetCashTransferForPosById(posId, type, side);
                cachlist = cachlist.Where(C => (C.posId == posId && C.isConfirm == 0)).ToList();
                return cachlist.Count;
            }
            catch
            {
                return 0;
            }

        }

        [NonAction]
        public List<CashTransferModel> GetCashTransferForPosById(int posId, string type, string side)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                List<CashTransferModel> cachlist = (from C in entity.cashTransfer
                                                    join b in entity.banks on C.bankId equals b.bankId into jb
                                                    join a in entity.agents on C.agentId equals a.agentId into ja
                                                    join p in entity.pos on C.posId equals p.posId into jp
                                                    join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                                    join u in entity.users on C.userId equals u.userId into ju
                                                    join uc in entity.users on C.createUserId equals uc.userId into juc
                                                    join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                                    join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                                    join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
                                                    from jbb in jb.DefaultIfEmpty()
                                                    from jaa in ja.DefaultIfEmpty()
                                                    from jpp in jp.DefaultIfEmpty()
                                                    from juu in ju.DefaultIfEmpty()
                                                    from jpcc in jpcr.DefaultIfEmpty()
                                                    from jucc in juc.DefaultIfEmpty()
                                                    from jcrd in jcr.DefaultIfEmpty()
                                                    from jbbo in jbo.DefaultIfEmpty()
                                                    from jssh in jsh.DefaultIfEmpty()
                                                    select new CashTransferModel()
                                                    {
                                                        cashTransId = C.cashTransId,
                                                        transType = C.transType,
                                                        posId = C.posId,
                                                        userId = C.userId,
                                                        agentId = C.agentId,
                                                        invId = C.invId,
                                                        transNum = C.transNum,
                                                        createDate = C.createDate,
                                                        updateDate = C.updateDate,
                                                        cash = C.cash,
                                                        updateUserId = C.updateUserId,
                                                        createUserId = C.createUserId,
                                                        notes = C.notes,
                                                        posIdCreator = C.posIdCreator,
                                                        isConfirm = C.isConfirm,
                                                        cashTransIdSource = C.cashTransIdSource,
                                                        side = C.side,

                                                        docName = C.docName,
                                                        docNum = C.docNum,
                                                        docImage = C.docImage,
                                                        bankId = C.bankId,
                                                        bankName = jbb.name,
                                                        agentName = jaa.name,
                                                        usersName = juu.name,// side =u

                                                        posName = jpp.name,
                                                        posCreatorName = jpcc.name,
                                                        processType = C.processType,
                                                        cardId = C.cardId,
                                                        bondId = C.bondId,
                                                        usersLName = juu.lastname,// side =u
                                                        createUserName = jucc.name,
                                                        createUserLName = jucc.lastname,
                                                        createUserJob = jucc.job,
                                                        cardName = jcrd.name,
                                                        bondDeserveDate = jbbo.deserveDate,
                                                        bondIsRecieved = jbbo.isRecieved,
                                                        shippingCompanyId = C.shippingCompanyId,
                                                        shippingCompanyName = jssh.name,
                                                        commissionValue = C.commissionValue,
                                                        commissionRatio = C.commissionRatio,

                                                    }).Where(C => ((type == "all") ? true : C.transType == type) && (C.processType != "balance")
        && ((side == "all") ? true : C.side == side)).ToList();

                if (cachlist.Count > 0 && side == "p")
                {
                    CashTransferModel tempitem = null;
                    foreach (CashTransferModel cashtItem in cachlist)
                    {
                        tempitem = this.Getpostransmodel(cashtItem.cashTransId)
                            .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
                        if (tempitem != null)
                        {
                            cashtItem.cashTrans2Id = tempitem.cashTransId;
                            cashtItem.pos2Id = tempitem.posId;
                            cashtItem.pos2Name = tempitem.posName;
                            cashtItem.isConfirm2 = tempitem.isConfirm;
                        }
                    }

                }
                cachlist = cachlist.Where(C => C.posId == posId || C.pos2Id == posId || C.posIdCreator == posId).ToList();

                return cachlist;


            }
        }
        [HttpPost]
        [Route("GetBybondId")]
        public string GetBybondId(string token)
        {
            //int bondId string token

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int bondId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "bondId")
                    {
                        bondId = int.Parse(c.Value);
                    }



                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
                                                            where C.bondId == bondId
                                                            select new CashTransferModel()
                                                            {
                                                                cashTransId = C.cashTransId,
                                                                transType = C.transType,
                                                                posId = C.posId,
                                                                userId = C.userId,
                                                                agentId = C.agentId,
                                                                invId = C.invId,
                                                                transNum = C.transNum,
                                                                createDate = C.createDate,
                                                                updateDate = C.updateDate,
                                                                cash = C.cash,
                                                                updateUserId = C.updateUserId,
                                                                createUserId = C.createUserId,
                                                                notes = C.notes,
                                                                posIdCreator = C.posIdCreator,
                                                                isConfirm = C.isConfirm,
                                                                cashTransIdSource = C.cashTransIdSource,
                                                                side = C.side,

                                                                docName = C.docName,
                                                                docNum = C.docNum,
                                                                docImage = C.docImage,
                                                                bankId = C.bankId,

                                                                processType = C.processType,
                                                                cardId = C.cardId,
                                                                bondId = C.bondId,
                                                                shippingCompanyId = C.shippingCompanyId,
                                                                commissionValue = C.commissionValue,
                                                                commissionRatio = C.commissionRatio,
                                                            }).ToList();






                        return TokenManager.GenerateToken(cachlist);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }

            //var re = Request;
            //var headers = re.Headers;
            //string token = "";


            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}



            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid)
            //{
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {

            //        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
            //                                            where C.bondId==bondId
            //                                            select new CashTransferModel()
            //                                            {
            //                                                cashTransId = C.cashTransId,
            //                                                transType = C.transType,
            //                                                posId = C.posId,
            //                                                userId = C.userId,
            //                                                agentId = C.agentId,
            //                                                invId = C.invId,
            //                                                transNum = C.transNum,
            //                                                createDate = C.createDate,
            //                                                updateDate = C.updateDate,
            //                                                cash = C.cash,
            //                                                updateUserId = C.updateUserId,
            //                                                createUserId = C.createUserId,
            //                                                notes = C.notes,
            //                                                posIdCreator = C.posIdCreator,
            //                                                isConfirm = C.isConfirm,
            //                                                cashTransIdSource = C.cashTransIdSource,
            //                                                side = C.side,

            //                                                docName = C.docName,
            //                                                docNum = C.docNum,
            //                                                docImage = C.docImage,
            //                                                bankId = C.bankId,

            //                                                processType = C.processType,
            //                                                cardId = C.cardId,
            //                                                bondId = C.bondId,
            //                                                shippingCompanyId = C.shippingCompanyId,
            //                                            }).ToList();





            //        if (cachlist == null)
            //            return NotFound();
            //        else
            //            return Ok(cachlist);

            //    }
            //}
            //else
            //    return NotFound();
        }



        [HttpPost]
        [Route("GetByID")]
        public string GetByID(string token)
        {
            //string type, string side string token

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int cTId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "cashTransferId")
                    {
                        cTId = int.Parse(c.Value);
                    }



                }

                try
                {
                    cashTransfer item = new cashTransfer();
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        List<cashTransfer> cacht = entity.cashTransfer.ToList();
                        item = cacht.Where(C => C.cashTransId == cTId)

            .Select(C => new cashTransfer
            {
                cashTransId = C.cashTransId,
                transType = C.transType,
                posId = C.posId,
                userId = C.userId,
                agentId = C.agentId,
                invId = C.invId,
                transNum = C.transNum,
                createDate = C.createDate,
                updateDate = C.updateDate,
                cash = C.cash,
                updateUserId = C.updateUserId,
                createUserId = C.createUserId,
                notes = C.notes,
                posIdCreator = C.posIdCreator,
                isConfirm = C.isConfirm,
                cashTransIdSource = C.cashTransIdSource,
                side = C.side,

                docName = C.docName,
                docNum = C.docNum,
                docImage = C.docImage,
                bankId = C.bankId,
                processType = C.processType,
                cardId = C.cardId,
                bondId = C.bondId,
                shippingCompanyId = C.shippingCompanyId,
                commissionValue = C.commissionValue,
                commissionRatio = C.commissionRatio,

            }).FirstOrDefault();


                        return TokenManager.GenerateToken(item);

                    }



                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }
            //      var re = Request;
            //      var headers = re.Headers;
            //      string token = "";
            //      int cTId = 0;
            //      if (headers.Contains("APIKey"))
            //      {
            //          token = headers.GetValues("APIKey").First();
            //      }
            //      if (headers.Contains("cashTransId"))
            //      {
            //          cTId = Convert.ToInt32(headers.GetValues("cashTransId").First());
            //      }
            //      Validation validation = new Validation();
            //      bool valid = validation.CheckApiKey(token);

            //      if (valid)
            //      {
            //          using (incposdbEntities entity = new incposdbEntities())
            //          {

            //              var cacht = entity.cashTransfer

            //.Where(C => C.cashTransId == cTId)

            //  .Select(C => new CashTransferModel
            //  {
            //      cashTransId = C.cashTransId,
            //      transType = C.transType,
            //      posId = C.posId,
            //      userId = C.userId,
            //      agentId = C.agentId,
            //      invId = C.invId,
            //      transNum = C.transNum,
            //      createDate = C.createDate,
            //      updateDate = C.updateDate,
            //      cash = C.cash,
            //      updateUserId = C.updateUserId,
            //      createUserId = C.createUserId,
            //      notes = C.notes,
            //      posIdCreator = C.posIdCreator,
            //      isConfirm = C.isConfirm,
            //      cashTransIdSource = C.cashTransIdSource,
            //      side = C.side,

            //      docName = C.docName,
            //      docNum = C.docNum,
            //      docImage = C.docImage,
            //      bankId = C.bankId,
            //      processType = C.processType,
            //      cardId = C.cardId,
            //      bondId = C.bondId,
            //      shippingCompanyId=C.shippingCompanyId,

            //  }).FirstOrDefault();

            //              if (cacht == null)
            //                  return NotFound();
            //              else
            //                  return Ok(cacht);

            //          }
            //      }
            //      else
            //          return NotFound();
        }



        // add or update agent
        [HttpPost]
        [Route("Save")]
        public async Task<string> Save(string token)
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
                string Object = "";
                cashTransfer newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                        message = await addCashTransfer(newObject);
                        return TokenManager.GenerateToken(message);
                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                }
                message = "0";
                return TokenManager.GenerateToken(message);
            }

        }

        [HttpPost]
        [Route("SaveCashWithCommission")]
        public async Task<string> SaveCashWithCommission(string token)
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
                string Object = "";
                cashTransfer newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                        message = await addCashTransfer(newObject);
                        newObject.cashTransId = int.Parse(message);

                        if (newObject.processType == "card")
                            AddCardCommission(newObject);
                        return TokenManager.GenerateToken(message);
                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                }
                message = "0";
                return TokenManager.GenerateToken(message);
            }

        }

        [HttpPost]
        [Route("transferPosBalance")]
        public async Task<string> transferPosBalance(string token)
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
                #region params
                string Object = "";
                cashTransfer cash1 = null;
                cashTransfer cash2 = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "cash1")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cash1 = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "cash2")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cash2 = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                }
                #endregion
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        int posId = (int)cash1.posId;
                        var pos = entity.pos.Find(posId);

                        #region check pos balance
                        // not confirmed transfers
                        var cashes = GetCashTransferForPosById(posId, "all", "p");
                        var allTransfers = cashes.Where(s => s.isConfirm == 1
                                                    && s.posId == posId
                                                    && s.isConfirm2 == 0)
                            .Select(s => s.cash).Sum();

                        allTransfers += (decimal)cash1.cash;

                        if (pos.balance < allTransfers)
                            return TokenManager.GenerateToken("-3");
                        #endregion            

                        message = await addCashTransfer(cash1);
                        if (int.Parse(message) > 0)
                        {
                            cash2.cashTransIdSource = int.Parse(message);
                            await addCashTransfer(cash2);
                        }
                        return TokenManager.GenerateToken(pos.balance);
                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("-1");
                }
            }

        }
        [HttpPost]
        [Route("confirmBankTransfer")]
        public async Task<string> confirmBankTransfer(string token)
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
                string Object = "";
                cashTransfer newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                        decimal amount = (decimal)newObject.cash;
                        pos pos;

                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            pos = entity.pos.Find(newObject.posId);

                            if (newObject.transType.Equals("d"))
                            {
                                amount *= -1;

                                if (pos.balance < (decimal)newObject.cash)
                                    return TokenManager.GenerateToken("-3");


                            }

                            message = await addCashTransfer(newObject);

                            pos.balance += amount;
                            entity.SaveChanges();
                        }
                        return TokenManager.GenerateToken(pos.balance.ToString());
                    }
                    catch
                    {
                        message = "-1";
                        return TokenManager.GenerateToken(message);
                    }
                }
                message = "-1";
                return TokenManager.GenerateToken(message);
            }

        }

        [HttpPost]
        [Route("SaveWithBalanceCheck")]
        public async Task<string> SaveWithBalanceCheck(string token)
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
                string Object = "";
                cashTransfer newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                        using (var entity = new incposdbEntities())
                        {
                            var pos = entity.pos.Find(newObject.posId);

                            if (pos.balance < newObject.cash)
                            {
                                return TokenManager.GenerateToken("-3");
                            }
                        }
                        // newObject.transNum = await generateCashNumber(newObject.transNum);
                        message = await addCashTransfer(newObject);
                        return TokenManager.GenerateToken(message);
                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }
                }
                message = "0";
                return TokenManager.GenerateToken(message);
            }

        }
        [HttpPost]
        [Route("Confirm")]
        public async Task<string> Confirm(string token)
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
                string Object = "";
                cashTransfer newObject = null;
                CashTransferModel cashTransferModel = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        cashTransferModel = JsonConvert.DeserializeObject<CashTransferModel>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                        var allList = this.Getpostransmodel(newObject.cashTransId).ToList();
                        var delList = allList.Where(C => C.isConfirm == 1).ToList();
                        if (delList != null)
                        {
                            if (delList.Count == 2)
                            {
                                message = "-2";
                                return TokenManager.GenerateToken(message);


                            }
                            else
                            {
                                int res = await Confirm(newObject, cashTransferModel);
                                return TokenManager.GenerateToken(res.ToString());
                            }
                        }
                    }
                    catch
                    {
                        message = "-1";
                        return TokenManager.GenerateToken(message);
                    }
                }
                message = "0";
                return TokenManager.GenerateToken(message);
            }

        }

        [HttpPost]
        [Route("ConfirmAll")]
        public async Task<string> ConfirmAll(string token)
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
                string Object = "";
                cashTransfer newObject = null;
                CashTransferModel cashTransferModel = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        cashTransferModel = JsonConvert.DeserializeObject<CashTransferModel>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                        var allList = this.Getpostransmodel(newObject.cashTransId).ToList();
                        var delList = allList.Where(C => C.isConfirm == 1).ToList();
                        if (delList != null)
                        {
                            if (delList.Count == 2)
                            {
                                message = "-2";
                                return TokenManager.GenerateToken(message);


                            }
                            else
                            {

                                int res = await ConfirmAll(newObject, cashTransferModel);
                                return TokenManager.GenerateToken(res.ToString());
                            }
                        }
                    }
                    catch
                    {
                        message = "-1";
                        return TokenManager.GenerateToken(message);
                    }
                }
                message = "0";
                return TokenManager.GenerateToken(message);
            }

        }




        [HttpPost]
        [Route("ConfirmAndTrans")]
        public async Task<string> ConfirmAndTrans(string token)
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
                string Object = "";
                cashTransfer newObject = null;
                CashTransferModel cashTransferModel = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        cashTransferModel = JsonConvert.DeserializeObject<CashTransferModel>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                        var allList = this.Getpostransmodel(newObject.cashTransId).ToList();
                        var delList = allList.Where(C => C.isConfirm == 1).ToList();
                        if (delList != null)
                        {
                            if (delList.Count == 2)
                            {
                                message = "-2";
                                return TokenManager.GenerateToken(message);
                            }
                            else
                            {
                                if (cashTransferModel.isConfirm2 == 0)
                                {
                                    newObject.isConfirm = 1;
                                    cashTransferModel.isConfirm = 1;
                                    int res = await Confirm(newObject, cashTransferModel);
                                    //  await confirmOpr(row);
                                    string msg = "0";
                                    if (res > 0)
                                    {
                                        msg = "-22.2";

                                    }
                                    else
                                    {
                                        msg = res.ToString();
                                    }
                                    return TokenManager.GenerateToken(msg.ToString());
                                }
                                else
                                {
                                    //make trans
                                    string res = "0";
                                    if (cashTransferModel.transType == "d")
                                    {
                                        res = await MakeDeposit(cashTransferModel);
                                    }
                                    else
                                    {
                                        res = await MakePull(cashTransferModel);
                                    }

                                    return TokenManager.GenerateToken(res.ToString());
                                }
                            }
                        }
                    }
                    catch
                    {
                        message = "-1";
                        return TokenManager.GenerateToken(message);
                    }
                }

                message = "0";
                return TokenManager.GenerateToken(message);
            }
        }

        //ConfirmAllAndTrans
        [HttpPost]
        [Route("ConfirmAllAndTrans")]
        public async Task<string> ConfirmAllAndTrans(string token)
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
                string Object = "";
                cashTransfer newObject = null;
                CashTransferModel cashTransferModel = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        cashTransferModel = JsonConvert.DeserializeObject<CashTransferModel>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                        var allList = this.Getpostransmodel(newObject.cashTransId).ToList();
                        var delList = allList.Where(C => C.isConfirm == 1).ToList();
                        if (delList != null)
                        {
                            if (delList.Count == 2)
                            {
                                message = "-2";
                                return TokenManager.GenerateToken(message);
                            }
                            else
                            {


                                if (cashTransferModel.isConfirm == 0 && cashTransferModel.isConfirm2 == 0)
                                {
                                    // confirm 2 
                                    int confres = await SetConfirmById(cashTransferModel.cashTrans2Id);

                                    if (confres > 0)
                                    {
                                        cashTransferModel.isConfirm2 = 1;

                                    }

                                }
                                string res = "0";
                                if (cashTransferModel.transType == "d")
                                {
                                    res = await MakeDeposit(cashTransferModel);
                                }
                                else
                                {
                                    res = await MakePull(cashTransferModel);
                                }

                                if (cashTransferModel.isConfirm == 1 && cashTransferModel.isConfirm2 == 0)
                                {
                                    // confirm 2 
                                    int confres = await SetConfirmById(cashTransferModel.cashTrans2Id);

                                    if (confres > 0)
                                    {
                                        cashTransferModel.isConfirm2 = 1;

                                    }

                                }
                                return TokenManager.GenerateToken(res.ToString());

                                //if (cashTransferModel.isConfirm2 == 0)
                                //{
                                //    newObject.isConfirm = 1;
                                //    cashTransferModel.isConfirm = 1;
                                //    int res = await Confirm(newObject, cashTransferModel);
                                //    //  await confirmOpr(row);
                                //    string msg = "0";
                                //    if (res > 0)
                                //    {
                                //        msg = "-22.2";

                                //    }
                                //    else
                                //    {
                                //        msg = res.ToString();
                                //    }
                                //    return TokenManager.GenerateToken(msg.ToString());
                                //}
                                //else
                                //{
                                //make trans
                                //string res = "0";
                                //if (cashTransferModel.transType == "d")
                                //{
                                //    res = await MakeDeposit(cashTransferModel);
                                //}
                                //else
                                //{
                                //    res = await MakePull(cashTransferModel);
                                //}

                                //return TokenManager.GenerateToken(res.ToString());
                                //}
                            }
                        }
                    }
                    catch
                    {
                        message = "-1";
                        return TokenManager.GenerateToken(message);
                    }
                }

                message = "0";
                return TokenManager.GenerateToken(message);
            }
        }


        [HttpPost]
        [Route("MakeDeposit")]
        public async Task<string> MakeDeposit(string token)
        {
            string message = "-1";

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string Object = "";
                cashTransfer newObject = null;
                CashTransferModel cashTransferModel = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        cashTransferModel = JsonConvert.DeserializeObject<CashTransferModel>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                        var allList = this.Getpostransmodel(newObject.cashTransId).ToList();
                        var delList = allList.Where(C => C.isConfirm == 1).ToList();
                        if (delList != null)
                        {
                            if (delList.Count == 2)
                            {
                                message = "-2";
                                return TokenManager.GenerateToken(message);


                            }
                            else
                            {
                                pos pos;
                                using (incposdbEntities entity = new incposdbEntities())
                                {
                                    pos = entity.pos.Find(newObject.posId);
                                    var pos2 = entity.pos.Find(cashTransferModel.pos2Id);

                                    if (pos.balance < newObject.cash)
                                    {
                                        return TokenManager.GenerateToken("-3");
                                    }

                                    pos.balance -= newObject.cash;

                                    pos2.balance += newObject.cash;

                                    entity.SaveChanges();
                                }
                                newObject.isConfirm = 1;
                                decimal res = await Confirm(newObject, cashTransferModel);
                                if (res > 0)
                                {
                                    res = (decimal)pos.balance;
                                }
                                return TokenManager.GenerateToken(res.ToString());
                            }
                        }
                    }
                    catch
                    {
                        message = "-1";
                        return TokenManager.GenerateToken(message);
                    }
                }
                message = "-1";
                return TokenManager.GenerateToken(message);
            }

        }

        [NonAction]
        public async Task<string> MakeDeposit(CashTransferModel cashTransferModel)
        {
            string message = "-1";


            string Object = "";
            cashTransfer newObject = null;

            Object = JsonConvert.SerializeObject(cashTransferModel);
            newObject = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });


            if (newObject != null)
            {
                try
                {
                    var allList = this.Getpostransmodel(newObject.cashTransId).ToList();
                    var delList = allList.Where(C => C.isConfirm == 1).ToList();
                    if (delList != null)
                    {
                        if (delList.Count == 2)
                        {
                            message = "-2";
                            return message;


                        }
                        else
                        {
                            pos pos;
                            using (incposdbEntities entity = new incposdbEntities())
                            {
                                pos = entity.pos.Find(newObject.posId);
                                var pos2 = entity.pos.Find(cashTransferModel.pos2Id);

                                if (pos.balance < newObject.cash)
                                {
                                    return "-3";
                                }

                                pos.balance -= newObject.cash;

                                pos2.balance += newObject.cash;

                                entity.SaveChanges();
                            }
                            newObject.isConfirm = 1;
                            decimal res = await Confirm(newObject, cashTransferModel);
                            if (res > 0)
                            {
                                res = (decimal)pos.balance;
                            }
                            return res.ToString();
                        }
                    }
                }
                catch
                {
                    message = "-1";
                    return message;
                }
            }
            message = "-1";
            return message;


        }

        [HttpPost]
        [Route("MakePull")]
        public async Task<string> MakePull(string token)
        {
            string message = "-1";

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string Object = "";
                cashTransfer newObject = null;
                CashTransferModel cashTransferModel = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "Object")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        cashTransferModel = JsonConvert.DeserializeObject<CashTransferModel>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                if (newObject != null)
                {
                    try
                    {
                        var allList = this.Getpostransmodel(newObject.cashTransId).ToList();
                        var delList = allList.Where(C => C.isConfirm == 1).ToList();
                        if (delList != null)
                        {
                            if (delList.Count == 2)
                            {
                                message = "-2";
                                return TokenManager.GenerateToken(message);


                            }
                            else
                            {
                                pos pos;
                                using (incposdbEntities entity = new incposdbEntities())
                                {
                                    pos = entity.pos.Find(newObject.posId);
                                    var pos2 = entity.pos.Find(cashTransferModel.pos2Id);

                                    if (pos2.balance < newObject.cash)
                                    {
                                        return TokenManager.GenerateToken("-3");
                                    }

                                    pos2.balance -= newObject.cash;

                                    pos.balance += newObject.cash;

                                    entity.SaveChanges();
                                }
                                newObject.isConfirm = 1;
                                decimal res = await Confirm(newObject, cashTransferModel);
                                if (res > 0)
                                {
                                    res = (decimal)pos.balance;
                                    return TokenManager.GenerateToken(res.ToString());
                                }
                            }
                        }
                    }
                    catch
                    {
                        message = "-1";
                        return TokenManager.GenerateToken(message);
                    }
                }

                return TokenManager.GenerateToken(message);
            }

        }

        [NonAction]
        public async Task<string> MakePull(CashTransferModel cashTransferModel)
        {
            string message = "-1";
            string Object = "";
            cashTransfer newObject = null;

            Object = JsonConvert.SerializeObject(cashTransferModel);
            newObject = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

            if (newObject != null)
            {
                try
                {
                    var allList = this.Getpostransmodel(newObject.cashTransId).ToList();
                    var delList = allList.Where(C => C.isConfirm == 1).ToList();
                    if (delList != null)
                    {
                        if (delList.Count == 2)
                        {
                            message = "-2";
                            return message;
                        }
                        else
                        {
                            pos pos;
                            using (incposdbEntities entity = new incposdbEntities())
                            {
                                pos = entity.pos.Find(newObject.posId);
                                var pos2 = entity.pos.Find(cashTransferModel.pos2Id);

                                if (pos2.balance < newObject.cash)
                                {
                                    return "-3";
                                }

                                pos2.balance -= newObject.cash;

                                pos.balance += newObject.cash;

                                entity.SaveChanges();
                            }
                            newObject.isConfirm = 1;
                            decimal res = await Confirm(newObject, cashTransferModel);
                            if (res > 0)
                            {
                                res = (decimal)pos.balance;
                                return res.ToString();
                            }
                        }
                    }
                }
                catch
                {
                    message = "-1";
                    return message;
                }
            }
            return message;
        }


        [NonAction]
        private async Task<int> Confirm(cashTransfer newObject, CashTransferModel cashTransferModel)
        {
            try
            {
                await addCashTransfer(newObject);
                using (incposdbEntities entity = new incposdbEntities())
                {
                    //edit updatedate for cashTrans 2 
                    var cashTrans2 = entity.cashTransfer.Find(cashTransferModel.cashTrans2Id);
                    cashTrans2.updateDate = cc.AddOffsetTodate(DateTime.Now);
                    entity.SaveChanges();
                }
                return 1;
            }
            catch
            {
                return -1;
            }
        }
        [NonAction]
        private async Task<int> ConfirmAll(cashTransfer newObject, CashTransferModel cashTransferModel)
        {
            try
            {
                newObject.isConfirm = 1;
                await addCashTransfer(newObject);
                using (incposdbEntities entity = new incposdbEntities())
                {
                    //edit updatedate for cashTrans 2 
                    var cashTrans2 = entity.cashTransfer.Find(cashTransferModel.cashTrans2Id);
                    cashTrans2.updateDate = cc.AddOffsetTodate(DateTime.Now);
                    cashTrans2.isConfirm = 1;
                    entity.SaveChanges();
                }
                return 1;
            }
            catch
            {
                return -1;
            }
        }
        [NonAction]
        private async Task<int> SetConfirmById(int cashTransId)
        {
            try
            {

                using (incposdbEntities entity = new incposdbEntities())
                {
                    //edit updatedate for cashTrans 2 
                    var cashTrans = entity.cashTransfer.Find(cashTransId);
                    cashTrans.updateDate = cc.AddOffsetTodate(DateTime.Now);
                    cashTrans.isConfirm = 1;
                    entity.SaveChanges();
                }
                return 1;
            }
            catch
            {
                return -1;
            }
        }
        [NonAction]
        public async Task<string> addCashTransfer(cashTransfer newObject)
        {
            string message = "";
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

            if (newObject.agentId == 0 || newObject.agentId == null)
            {
                Nullable<int> id = null;
                newObject.agentId = id;
            }
            if (newObject.invId == 0 || newObject.invId == null)
            {
                Nullable<int> id = null;
                newObject.invId = id;
            }
            if (newObject.posIdCreator == 0 || newObject.posIdCreator == null)
            {
                Nullable<int> id = null;
                newObject.posIdCreator = id;
            }

            if (newObject.cashTransIdSource == 0 || newObject.cashTransIdSource == null)
            {
                Nullable<int> id = null;
                newObject.cashTransIdSource = id;
            }
            if (newObject.bankId == 0 || newObject.bankId == null)
            {
                Nullable<int> id = null;
                newObject.bankId = id;
            }

            cashTransfer cashtr;
            using (incposdbEntities entity = new incposdbEntities())
            {
                DateTime datenow = cc.AddOffsetTodate(DateTime.Now);
                var cEntity = entity.Set<cashTransfer>();
                if (newObject.cashTransId == 0)
                {
                    newObject.transNum = await generateCashNumber(newObject.transNum);
                    if (newObject.processType == "statement")
                    {
                        DateTime DT = new DateTime(datenow.Year, 1, 1, 0, 0, 0);
                        newObject.createDate = DT;
                        newObject.updateDate = DT;
                    }
                    else
                    {
                        newObject.createDate = datenow;
                        newObject.updateDate = datenow;
                    }

                    newObject.updateUserId = newObject.createUserId;
                    cashtr = cEntity.Add(newObject);
                }
                else
                {
                    cashtr = entity.cashTransfer.Where(p => p.cashTransId == newObject.cashTransId).First();
                    cashtr.transType = newObject.transType;
                    cashtr.posId = newObject.posId;
                    cashtr.userId = newObject.userId;
                    cashtr.agentId = newObject.agentId;
                    cashtr.invId = newObject.invId;
                    cashtr.transNum = newObject.transNum;
                    cashtr.createDate = newObject.createDate;
                    cashtr.updateDate = datenow;// server current date
                    cashtr.cash = newObject.cash;
                    cashtr.updateUserId = newObject.updateUserId;
                    // cashtr.createUserId = newObject. ;
                    cashtr.notes = newObject.notes;
                    cashtr.posIdCreator = newObject.posIdCreator;
                    cashtr.isConfirm = newObject.isConfirm;
                    cashtr.cashTransIdSource = newObject.cashTransIdSource;
                    cashtr.side = newObject.side;

                    cashtr.docName = newObject.docName;
                    cashtr.docNum = newObject.docNum;
                    cashtr.docImage = newObject.docImage;
                    cashtr.bankId = newObject.bankId;
                    // cashtr.updateDate = cc.AddOffsetTodate(DateTime.Now);// server current date
                    cashtr.processType = newObject.processType;
                    cashtr.cardId = newObject.cardId;
                    cashtr.bondId = newObject.bondId;
                    cashtr.shippingCompanyId = newObject.shippingCompanyId;
                    cashtr.commissionValue = newObject.commissionValue;
                    cashtr.commissionRatio = newObject.commissionRatio;

                }
                entity.SaveChanges();
            }
            message = cashtr.cashTransId.ToString();
            return message;
        }

        [HttpPost]
        [Route("GetCashTransferForPosByUserId")]
        public string GetCashTransferForPosByUserId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string type = "";
                string side = "";
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                    else if (c.Type == "side")
                    {
                        side = c.Value;
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                    BranchesController branchctrlr = new BranchesController();

                    List<BranchModel> bmList = new List<BranchModel>();
                    bmList = branchctrlr.BranchesByUserIdType(userId);
                    List<int> brIds = bmList.Select(S => S.branchId).ToList();

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
                                                            join b in entity.banks on C.bankId equals b.bankId into jb
                                                            join a in entity.agents on C.agentId equals a.agentId into ja
                                                            join p in entity.pos on C.posId equals p.posId into jp
                                                            join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                                            join u in entity.users on C.userId equals u.userId into ju
                                                            join uc in entity.users on C.createUserId equals uc.userId into juc
                                                            join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                                            join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                                            join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
                                                            from jbb in jb.DefaultIfEmpty()
                                                            from jaa in ja.DefaultIfEmpty()
                                                            from jpp in jp.DefaultIfEmpty()
                                                            from juu in ju.DefaultIfEmpty()
                                                            from jpcc in jpcr.DefaultIfEmpty()
                                                            from jucc in juc.DefaultIfEmpty()
                                                            from jcrd in jcr.DefaultIfEmpty()
                                                            from jbbo in jbo.DefaultIfEmpty()
                                                            from jssh in jsh.DefaultIfEmpty()
                                                            select new CashTransferModel()
                                                            {
                                                                cashTransId = C.cashTransId,
                                                                transType = C.transType,
                                                                posId = C.posId,
                                                                userId = C.userId,
                                                                agentId = C.agentId,
                                                                invId = C.invId,
                                                                transNum = C.transNum,
                                                                createDate = C.createDate,
                                                                updateDate = C.updateDate,
                                                                cash = C.cash,
                                                                updateUserId = C.updateUserId,
                                                                createUserId = C.createUserId,
                                                                notes = C.notes,
                                                                posIdCreator = C.posIdCreator,
                                                                isConfirm = C.isConfirm,
                                                                cashTransIdSource = C.cashTransIdSource,
                                                                side = C.side,

                                                                docName = C.docName,
                                                                docNum = C.docNum,
                                                                docImage = C.docImage,
                                                                bankId = C.bankId,
                                                                bankName = jbb.name,
                                                                agentName = jaa.name,
                                                                usersName = juu.name,// side =u

                                                                posName = jpp.name,
                                                                posCreatorName = jpcc.name,
                                                                processType = C.processType,
                                                                cardId = C.cardId,
                                                                bondId = C.bondId,
                                                                usersLName = juu.lastname,// side =u
                                                                createUserName = jucc.name,
                                                                createUserLName = jucc.lastname,
                                                                createUserJob = jucc.job,
                                                                cardName = jcrd.name,
                                                                bondDeserveDate = jbbo.deserveDate,
                                                                bondIsRecieved = jbbo.isRecieved,
                                                                shippingCompanyId = C.shippingCompanyId,
                                                                shippingCompanyName = jssh.name,
                                                                commissionValue = C.commissionValue,
                                                                commissionRatio = C.commissionRatio,
                                                                branchId = jpp.branchId,
                                                                branchName = jpp.branches.name,
                                                                branchCreatorId = jpcc.branchId,
                                                                branchCreatorname = jpcc.branches.name,

                                                            }).Where(C => ((type == "all") ? true : C.transType == type) && (C.processType != "balance")
                && ((side == "all") ? true : C.side == side)).ToList();

                        if (cachlist.Count > 0 && side == "p")
                        {
                            CashTransferModel tempitem = null;
                            foreach (CashTransferModel cashtItem in cachlist)
                            {
                                tempitem = this.Getpostransmodel(cashtItem.cashTransId)
                                    .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
                                cashtItem.cashTrans2Id = tempitem.cashTransId;
                                cashtItem.pos2Id = tempitem.posId;
                                cashtItem.pos2Name = tempitem.posName;
                                cashtItem.isConfirm2 = tempitem.isConfirm;
                                cashtItem.branch2Id = tempitem.branchId;
                                cashtItem.branch2Name = tempitem.branchName;
                            }

                        }
                        cachlist = cachlist.Where(C => (brIds.Contains((int)C.branchId) || brIds.Contains((int)C.branch2Id) || brIds.Contains((int)C.branchCreatorId))
                        && C.transType == "p"
                        && (C.isConfirm == 0 || C.isConfirm2 == 0)).ToList();

                        return TokenManager.GenerateToken(cachlist);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }
        ///
        [HttpPost]
        [Route("GetbySourcId")]
        public string GetbySourcId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int sourceId = 0;
                string side = "";

                string type = "all";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "sourceId")
                    {
                        sourceId = int.Parse(c.Value);
                    }
                    else if (c.Type == "side")
                    {
                        side = c.Value;
                    }


                }


                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var cachlist = (from C in entity.cashTransfer
                                        join b in entity.banks on C.bankId equals b.bankId into jb
                                        join a in entity.agents on C.agentId equals a.agentId into ja
                                        join p in entity.pos on C.posId equals p.posId into jp
                                        join u in entity.users on C.userId equals u.userId into ju
                                        from jbb in jb.DefaultIfEmpty()
                                        from jaa in ja.DefaultIfEmpty()
                                        from jpp in jp.DefaultIfEmpty()
                                        from juu in ju.DefaultIfEmpty()

                                        select new CashTransferModel()
                                        {
                                            cashTransId = C.cashTransId,
                                            transType = C.transType,
                                            posId = C.posId,
                                            userId = C.userId,
                                            agentId = C.agentId,
                                            invId = C.invId,
                                            transNum = C.transNum,
                                            createDate = C.createDate,
                                            updateDate = C.updateDate,
                                            cash = C.cash,
                                            updateUserId = C.updateUserId,
                                            createUserId = C.createUserId,
                                            notes = C.notes,
                                            posIdCreator = C.posIdCreator,
                                            isConfirm = C.isConfirm,
                                            cashTransIdSource = C.cashTransIdSource,
                                            side = C.side,

                                            docName = C.docName,
                                            docNum = C.docNum,
                                            docImage = C.docImage,
                                            bankId = C.bankId,
                                            bankName = jbb.name,
                                            agentName = jaa.name,
                                            usersName = juu.username,
                                            posName = jpp.name,
                                            processType = C.processType,
                                            cardId = C.cardId,
                                            bondId = C.bondId,
                                            shippingCompanyId = C.shippingCompanyId,
                                            commissionValue = C.commissionValue,
                                            commissionRatio = C.commissionRatio,
                                        }).Where(C => ((type == "all") ? true : C.transType == type)
                                    && ((side == "all") ? true : C.side == side) && (C.cashTransId == sourceId || C.cashTransIdSource == sourceId)).ToList();


                        // one row mean type=d
                        if (cachlist.Count == 1)
                        {
                            int? pullposcashtransid = cachlist.First().cashTransIdSource;

                            //
                            var cachadd = (from C in entity.cashTransfer
                                           join b in entity.banks on C.bankId equals b.bankId into jb
                                           join a in entity.agents on C.agentId equals a.agentId into ja
                                           join p in entity.pos on C.posId equals p.posId into jp
                                           join u in entity.users on C.userId equals u.userId into ju
                                           from jbb in jb.DefaultIfEmpty()
                                           from jaa in ja.DefaultIfEmpty()
                                           from jpp in jp.DefaultIfEmpty()
                                           from juu in ju.DefaultIfEmpty()

                                           select new CashTransferModel()
                                           {
                                               cashTransId = C.cashTransId,
                                               transType = C.transType,
                                               posId = C.posId,
                                               userId = C.userId,
                                               agentId = C.agentId,
                                               invId = C.invId,
                                               transNum = C.transNum,
                                               createDate = C.createDate,
                                               updateDate = C.updateDate,
                                               cash = C.cash,
                                               updateUserId = C.updateUserId,
                                               createUserId = C.createUserId,
                                               notes = C.notes,
                                               posIdCreator = C.posIdCreator,
                                               isConfirm = C.isConfirm,
                                               cashTransIdSource = C.cashTransIdSource,
                                               side = C.side,

                                               docName = C.docName,
                                               docNum = C.docNum,
                                               docImage = C.docImage,
                                               bankId = C.bankId,
                                               bankName = jbb.name,
                                               agentName = jaa.name,
                                               usersName = juu.username,
                                               posName = jpp.name,
                                               processType = C.processType,
                                               cardId = C.cardId,
                                               bondId = C.bondId,
                                               commissionValue = C.commissionValue,
                                               commissionRatio = C.commissionRatio,
                                           }).Where(C => ((type == "all") ? true : C.transType == type)
                   && ((side == "all") ? true : C.side == side) && (C.cashTransId == pullposcashtransid)).ToList();

                            //

                            if (cachadd.Count > 0)
                            {
                                cachlist.AddRange(cachadd);

                            }
                        }


                        return TokenManager.GenerateToken(cachlist);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


            //string type = "all";

            //var re = Request;
            //var headers = re.Headers;
            //string token = "";
            ///*
            //string type = "";
            //string side = "";
            //*/
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}



            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid)
            //{
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {

            //        var cachlist = (from C in entity.cashTransfer
            //                        join b in entity.banks on C.bankId equals b.bankId into jb
            //                        join a in entity.agents on C.agentId equals a.agentId into ja
            //                        join p in entity.pos on C.posId equals p.posId into jp
            //                        join u in entity.users on C.userId equals u.userId into ju
            //                        from jbb in jb.DefaultIfEmpty()
            //                        from jaa in ja.DefaultIfEmpty()
            //                        from jpp in jp.DefaultIfEmpty()
            //                        from juu in ju.DefaultIfEmpty()

            //                        select new CashTransferModel()
            //                        {
            //                            cashTransId = C.cashTransId,
            //                            transType = C.transType,
            //                            posId = C.posId,
            //                            userId = C.userId,
            //                            agentId = C.agentId,
            //                            invId = C.invId,
            //                            transNum = C.transNum,
            //                            createDate = C.createDate,
            //                            updateDate = C.updateDate,
            //                            cash = C.cash,
            //                            updateUserId = C.updateUserId,
            //                            createUserId = C.createUserId,
            //                            notes = C.notes,
            //                            posIdCreator = C.posIdCreator,
            //                            isConfirm = C.isConfirm,
            //                            cashTransIdSource = C.cashTransIdSource,
            //                            side = C.side,

            //                            docName = C.docName,
            //                            docNum = C.docNum,
            //                            docImage = C.docImage,
            //                            bankId = C.bankId,
            //                            bankName = jbb.name,
            //                            agentName = jaa.name,
            //                            usersName = juu.username,
            //                            posName = jpp.name,
            //                            processType = C.processType,
            //                            cardId = C.cardId,
            //                            bondId = C.bondId,
            //                            shippingCompanyId = C.shippingCompanyId,
            //                        }).Where(C => ((type == "all") ? true : C.transType == type)
            //                    && ((side == "all") ? true : C.side == side) && (C.cashTransId == sourceId || C.cashTransIdSource == sourceId)).ToList();


            //        // one row mean type=d
            //        if (cachlist.Count == 1)
            //        {
            //            int? pullposcashtransid = cachlist.First().cashTransIdSource;

            //            //
            //            var cachadd = (from C in entity.cashTransfer
            //                           join b in entity.banks on C.bankId equals b.bankId into jb
            //                           join a in entity.agents on C.agentId equals a.agentId into ja
            //                           join p in entity.pos on C.posId equals p.posId into jp
            //                           join u in entity.users on C.userId equals u.userId into ju
            //                           from jbb in jb.DefaultIfEmpty()
            //                           from jaa in ja.DefaultIfEmpty()
            //                           from jpp in jp.DefaultIfEmpty()
            //                           from juu in ju.DefaultIfEmpty()

            //                           select new CashTransferModel()
            //                           {
            //                               cashTransId = C.cashTransId,
            //                               transType = C.transType,
            //                               posId = C.posId,
            //                               userId = C.userId,
            //                               agentId = C.agentId,
            //                               invId = C.invId,
            //                               transNum = C.transNum,
            //                               createDate = C.createDate,
            //                               updateDate = C.updateDate,
            //                               cash = C.cash,
            //                               updateUserId = C.updateUserId,
            //                               createUserId = C.createUserId,
            //                               notes = C.notes,
            //                               posIdCreator = C.posIdCreator,
            //                               isConfirm = C.isConfirm,
            //                               cashTransIdSource = C.cashTransIdSource,
            //                               side = C.side,

            //                               docName = C.docName,
            //                               docNum = C.docNum,
            //                               docImage = C.docImage,
            //                               bankId = C.bankId,
            //                               bankName = jbb.name,
            //                               agentName = jaa.name,
            //                               usersName = juu.username,
            //                               posName = jpp.name,
            //                               processType = C.processType,
            //                               cardId = C.cardId,
            //                               bondId = C.bondId,
            //                           }).Where(C => ((type == "all") ? true : C.transType == type)
            //   && ((side == "all") ? true : C.side == side) && (C.cashTransId == pullposcashtransid)).ToList();

            //            //

            //            if (cachadd.Count > 0)
            //            {
            //                cachlist.AddRange(cachadd);

            //            }
            //        }

            //        if (cachlist == null)
            //            return NotFound();
            //        else
            //        {

            //            return Ok(cachlist);
            //        }
            //    }

            //}
            //else
            //    return NotFound();
        }

        [HttpPost]
        [Route("Delete")]
        public string Delete(string token)
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
                int cashTransId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "cashTransId")
                    {
                        cashTransId = int.Parse(c.Value);
                    }


                }

                List<CashTransferModel> delList = null;
                List<CashTransferModel> allList = null;
                cashTransfer cashobject = new cashTransfer();

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        allList = this.Getpostransmodel(cashTransId).ToList();
                        delList = allList.Where(C => C.isConfirm == 1).ToList();
                        if (delList != null)
                        {
                            if (delList.Count == 2)
                            {
                                message = "0";
                                return TokenManager.GenerateToken(message);


                            }
                            else
                            {

                                foreach (CashTransferModel ctitem in allList)
                                {
                                    cashobject = entity.cashTransfer.Where(C => C.cashTransId == ctitem.cashTransId).FirstOrDefault();
                                    entity.cashTransfer.Remove(cashobject);

                                }
                                int res = entity.SaveChanges();
                                if (res > 0)
                                {
                                    message = "1";
                                }

                            }

                        }

                        return TokenManager.GenerateToken(message);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }


            }

            //var re = Request;
            //var headers = re.Headers;
            //string token = "";
            //List<CashTransferModel> delList = null;
            //List<CashTransferModel> allList = null;
            //cashTransfer cashobject = new cashTransfer();
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);
            //if (valid)
            //{

            //    try
            //    {
            //        using (incposdbEntities entity = new incposdbEntities())
            //        {
            //            allList = this.Getpostransmodel(cashTransId).ToList();
            //            delList = allList.Where(C => C.isConfirm == 1).ToList();
            //            if (delList != null)
            //            {
            //                if (delList.Count == 2)
            //                {

            //                    return Ok("0");
            //                }
            //                else
            //                {

            //                    foreach (CashTransferModel ctitem in allList)
            //                    {
            //                        cashobject = entity.cashTransfer.Where(C => C.cashTransId == ctitem.cashTransId).FirstOrDefault();
            //                        entity.cashTransfer.Remove(cashobject);

            //                    }
            //                    entity.SaveChanges();


            //                }

            //            }






            //            return Ok("1");
            //        }
            //    }
            //    catch
            //    {
            //        return NotFound();
            //    }


            //}
            //else
            //    return NotFound();
        }

        [HttpPost]
        [Route("Cancle")]
        public string Cancle(string token)
        {
            string message = "-1";

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int cashTransId1 = 0;
                int cashTransId2 = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "cashTransId1")
                    {
                        cashTransId1 = int.Parse(c.Value);
                    }
                    else if (c.Type == "cashTransId2")
                    {
                        cashTransId2 = int.Parse(c.Value);
                    }
                }

                List<CashTransferModel> delList = null;
                List<CashTransferModel> allList = null;
                cashTransfer cashobject = new cashTransfer();

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        allList = this.Getpostransmodel(cashTransId1).ToList();
                        delList = allList.Where(C => C.isConfirm == 1).ToList();
                        if (delList != null)
                        {
                            if (delList.Count == 2)
                            {
                                message = "-2";
                                return TokenManager.GenerateToken(message);

                            }
                            else
                            {
                                var cash1 = entity.cashTransfer.Find(cashTransId1);
                                var cash2 = entity.cashTransfer.Find(cashTransId2);

                                cash1.isConfirm = 2;
                                cash2.isConfirm = 2;
                                entity.SaveChanges();

                                message = "1";


                            }

                        }

                        return TokenManager.GenerateToken(message);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("-1");
                }

            }

        }


        [HttpPost]
        [Route("MovePosCash")]
        public string MovePosCash(string token)
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
                int cashTransId = 0;
                int userIdD = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "cashTransId")
                    {
                        cashTransId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userIdD")
                    {
                        userIdD = int.Parse(c.Value);
                    }

                }


                List<CashTransferModel> tempList = null;
                List<CashTransferModel> tempList2 = null;
                List<CashTransferModel> allList = null;
                CashTransferModel cashobject = new CashTransferModel();
                CashTransferModel cashobject2 = new CashTransferModel();
                cashTransfer ctObject = new cashTransfer();
                cashTransfer ctObject2 = new cashTransfer();
                pos posobject = new pos();
                pos posobjectD = new pos();
                int? posidPull = 0;
                int? posidD = 0;
                decimal? cash = 0;

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        allList = Getpostransmodel(cashTransId).ToList();
                        if (allList.Count > 0)
                        {
                            //check if first pos is confirm
                            tempList = allList.Where(C => C.transType == "p" && C.isConfirm == 1).ToList();
                            //    tempList2= allList.Where(C => C.transType == "d" && C.isConfirm == 1).ToList();

                            if (tempList != null)
                            {
                                if (tempList.Count >= 1)
                                {
                                    cashobject = tempList.FirstOrDefault();
                                    cash = cashobject.cash;
                                    posidPull = cashobject.posId;
                                    posobject = entity.pos.Where(p => p.posId == posidPull).FirstOrDefault();
                                    if (cashobject.cash <= posobject.balance)
                                    {
                                        //in "d" set confirm to 1
                                        //get row of type d
                                        cashobject = allList.Where(C => C.transType == "d").FirstOrDefault();
                                        ctObject = entity.cashTransfer.Where(C => C.cashTransId == cashobject.cashTransId).FirstOrDefault();
                                        ctObject.isConfirm = 1;
                                        ctObject.updateUserId = userIdD;
                                        ctObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                        ctObject.userId = userIdD;

                                        //update date for other row
                                        cashobject2 = allList.Where(C => C.transType == "p").FirstOrDefault();
                                        ctObject2 = entity.cashTransfer.Where(C => C.cashTransId == cashobject2.cashTransId).FirstOrDefault();

                                        ctObject2.updateDate = ctObject.updateDate;


                                        // END in "d" set confirm to 1

                                        //START decreas balance from pull pos
                                        posidD = ctObject.posId;
                                        posobject = entity.pos.Where(p => p.posId == posidPull).FirstOrDefault();

                                        posobject.balance = posobject.balance - cash;
                                        posobject.updateUserId = userIdD;
                                        posobject.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                        // end
                                        //increase balance from d pos
                                        posobjectD = entity.pos.Where(p => p.posId == posidD).FirstOrDefault();
                                        if (posobjectD.balance == null)
                                        {
                                            posobjectD.balance = 0;
                                        }
                                        posobjectD.balance = posobjectD.balance + cash;
                                        posobjectD.updateUserId = userIdD;
                                        posobjectD.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                        entity.SaveChanges();
                                        // return Ok("transdone");
                                        return TokenManager.GenerateToken("1");
                                    }
                                    else
                                    {
                                        //  return Ok("nobalanceinpullpos");
                                        return TokenManager.GenerateToken("2");
                                    }


                                }
                                else
                                {
                                    //return Ok("pullposnotconfirmed");
                                    return TokenManager.GenerateToken("3");
                                }
                            }
                            else
                            {
                                //  return Ok("nopullidornotconfirmed");
                                return TokenManager.GenerateToken("4");
                            }
                        }
                        else
                        {
                            //  return Ok("idnotfound");
                            return TokenManager.GenerateToken("5");
                        }

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }



            }
            //var re = Request;
            //var headers = re.Headers;
            //string token = "";
            //List<CashTransferModel> tempList = null;
            //List<CashTransferModel> allList = null;
            //CashTransferModel cashobject = new CashTransferModel();
            //cashTransfer ctObject = new cashTransfer();

            //pos posobject = new pos();
            //pos posobjectD = new pos();
            //int? posidPull = 0;
            //int? posidD = 0;
            //decimal? cash = 0;

            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);
            //if (valid)
            //{

            //    try
            //    {
            //        using (incposdbEntities entity = new incposdbEntities())
            //        {
            //            allList = this.Getpostransmodel(cashTransId).ToList();
            //            if (allList.Count > 0)
            //            {
            //                //check if first pos is confirm
            //                tempList = allList.Where(C => C.transType == "p" && C.isConfirm == 1).ToList();

            //                if (tempList != null)
            //                {
            //                    if (tempList.Count >= 1)
            //                    {
            //                        cashobject = tempList.FirstOrDefault();
            //                        cash = cashobject.cash;
            //                        posidPull = cashobject.posId;
            //                        posobject = entity.pos.Where(p => p.posId == posidPull).FirstOrDefault();
            //                        if (cashobject.cash <= posobject.balance)
            //                        {
            //                            //in "d" set confirm to 1
            //                            //get row of type d
            //                            cashobject = allList.Where(C => C.transType == "d").FirstOrDefault();
            //                            ctObject = entity.cashTransfer.Where(C => C.cashTransId == cashobject.cashTransId).FirstOrDefault();
            //                            ctObject.isConfirm = 1;
            //                            ctObject.updateUserId = userIdD;
            //                            ctObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
            //                            ctObject.userId = userIdD;
            //                            // END in "d" set confirm to 1

            //                            //START decreas balance from pull pos
            //                            posidD = ctObject.posId;
            //                            posobject = entity.pos.Where(p => p.posId == posidPull).FirstOrDefault();

            //                            posobject.balance = posobject.balance - cash;
            //                            posobject.updateUserId = userIdD;
            //                            posobject.updateDate = cc.AddOffsetTodate(DateTime.Now);
            //                            // end
            //                            //increase balance from d pos
            //                            posobjectD = entity.pos.Where(p => p.posId == posidD).FirstOrDefault();
            //                            if (posobjectD.balance == null)
            //                            {
            //                                posobjectD.balance = 0;
            //                            }
            //                            posobjectD.balance = posobjectD.balance + cash;
            //                            posobjectD.updateUserId = userIdD;
            //                            posobjectD.updateDate = cc.AddOffsetTodate(DateTime.Now);
            //                            entity.SaveChanges();
            //                            return Ok("transdone");
            //                        }
            //                        else
            //                        {
            //                            return Ok("nobalanceinpullpos");
            //                        }


            //                    }
            //                    else
            //                    {
            //                        return Ok("pullposnotconfirmed");
            //                    }
            //                }
            //                else
            //                {
            //                    return Ok("nopullidornotconfirmed");
            //                }
            //            }
            //            else
            //            {
            //                return Ok("idnotfound");
            //            }

            //        }
            //    }
            //    catch
            //    {
            //        return NotFound();
            //    }


            //}
            //else
            //    return NotFound();
        }

        [NonAction]
        public List<CashTransferModel> Getpostransmodel(int cashTransId)
        {
            string side = "p";
            string type = "all";
            using (incposdbEntities entity = new incposdbEntities())
            {

                var cachlist = (from C in entity.cashTransfer
                                join b in entity.banks on C.bankId equals b.bankId into jb
                                join a in entity.agents on C.agentId equals a.agentId into ja
                                join p in entity.pos on C.posId equals p.posId into jp
                                join u in entity.users on C.userId equals u.userId into ju
                                from jbb in jb.DefaultIfEmpty()
                                from jaa in ja.DefaultIfEmpty()
                                from jpp in jp.DefaultIfEmpty()
                                from juu in ju.DefaultIfEmpty()

                                select new CashTransferModel()
                                {
                                    cashTransId = C.cashTransId,
                                    transType = C.transType,
                                    posId = C.posId,
                                    userId = C.userId,
                                    agentId = C.agentId,
                                    invId = C.invId,
                                    transNum = C.transNum,
                                    createDate = C.createDate,
                                    updateDate = C.updateDate,
                                    cash = C.cash,
                                    updateUserId = C.updateUserId,
                                    createUserId = C.createUserId,
                                    notes = C.notes,
                                    posIdCreator = C.posIdCreator,
                                    isConfirm = C.isConfirm,
                                    cashTransIdSource = C.cashTransIdSource,
                                    side = C.side,

                                    docName = C.docName,
                                    docNum = C.docNum,
                                    docImage = C.docImage,
                                    bankId = C.bankId,
                                    bankName = jbb.name,
                                    agentName = jaa.name,
                                    usersName = juu.username,
                                    posName = jpp.name,
                                    processType = C.processType,
                                    cardId = C.cardId,
                                    bondId = C.bondId,
                                    shippingCompanyId = C.shippingCompanyId,
                                    commissionValue = C.commissionValue,
                                    commissionRatio = C.commissionRatio,
                                }).Where(C => ((type == "all") ? true : C.transType == type)
        && ((side == "all") ? true : C.side == side) && (C.cashTransId == cashTransId || C.cashTransIdSource == cashTransId)).ToList();


                // one row mean type=d
                if (cachlist.Count == 1)
                {
                    int? pullposcashtransid = cachlist.First().cashTransIdSource;

                    //
                    var cachadd = (from C in entity.cashTransfer
                                   join b in entity.banks on C.bankId equals b.bankId into jb
                                   join a in entity.agents on C.agentId equals a.agentId into ja
                                   join p in entity.pos on C.posId equals p.posId into jp
                                   join u in entity.users on C.userId equals u.userId into ju
                                   from jbb in jb.DefaultIfEmpty()
                                   from jaa in ja.DefaultIfEmpty()
                                   from jpp in jp.DefaultIfEmpty()
                                   from juu in ju.DefaultIfEmpty()

                                   select new CashTransferModel()
                                   {
                                       cashTransId = C.cashTransId,
                                       transType = C.transType,
                                       posId = C.posId,
                                       userId = C.userId,
                                       agentId = C.agentId,
                                       invId = C.invId,
                                       transNum = C.transNum,
                                       createDate = C.createDate,
                                       updateDate = C.updateDate,
                                       cash = C.cash,
                                       updateUserId = C.updateUserId,
                                       createUserId = C.createUserId,
                                       notes = C.notes,
                                       posIdCreator = C.posIdCreator,
                                       isConfirm = C.isConfirm,
                                       cashTransIdSource = C.cashTransIdSource,
                                       side = C.side,

                                       docName = C.docName,
                                       docNum = C.docNum,
                                       docImage = C.docImage,
                                       bankId = C.bankId,
                                       bankName = jbb.name,
                                       agentName = jaa.name,
                                       usersName = juu.username,
                                       posName = jpp.name,
                                       processType = C.processType,
                                       cardId = C.cardId,
                                       bondId = C.bondId,
                                       shippingCompanyId = C.shippingCompanyId,
                                       commissionValue = C.commissionValue,
                                       commissionRatio = C.commissionRatio,
                                   }).Where(C => ((type == "all") ? true : C.transType == type)
                      && ((side == "all") ? true : C.side == side) && (C.cashTransId == pullposcashtransid)).ToList();

                    //

                    if (cachadd.Count > 0)
                    {
                        cachlist.AddRange(cachadd);

                    }

                }

                return cachlist;
            }
        }


        [HttpPost]
        [Route("GetByInvId")]
        public string GetByInvId(string token)

        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string type = "all";
                string side = "all";
                int invId = 0;


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invId")
                    {
                        invId = int.Parse(c.Value);
                    }


                }

                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        CashTransferModel cachtrans = new CashTransferModel();
                        cachtrans = (from C in entity.cashTransfer
                                     join b in entity.banks on C.bankId equals b.bankId into jb
                                     join a in entity.agents on C.agentId equals a.agentId into ja
                                     join p in entity.pos on C.posId equals p.posId into jp
                                     join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                     join u in entity.users on C.userId equals u.userId into ju
                                     from jbb in jb.DefaultIfEmpty()
                                     from jaa in ja.DefaultIfEmpty()
                                     from jpp in jp.DefaultIfEmpty()
                                     from juu in ju.DefaultIfEmpty()
                                     from jpcc in jpcr.DefaultIfEmpty()
                                     select new CashTransferModel()
                                     {
                                         cashTransId = C.cashTransId,
                                         transType = C.transType,
                                         posId = C.posId,
                                         userId = C.userId,
                                         agentId = C.agentId,
                                         invId = C.invId,
                                         transNum = C.transNum,
                                         createDate = C.createDate,
                                         updateDate = C.updateDate,
                                         cash = C.cash,
                                         updateUserId = C.updateUserId,
                                         createUserId = C.createUserId,
                                         notes = C.notes,
                                         posIdCreator = C.posIdCreator,
                                         isConfirm = C.isConfirm,
                                         cashTransIdSource = C.cashTransIdSource,
                                         side = C.side,

                                         docName = C.docName,
                                         docNum = C.docNum,
                                         docImage = C.docImage,
                                         bankId = C.bankId,
                                         bankName = jbb.name,
                                         agentName = jaa.name,
                                         usersName = juu.username,
                                         posName = jpp.name,
                                         posCreatorName = jpcc.name,
                                         processType = C.processType,
                                         cardId = C.cardId,
                                         bondId = C.bondId,
                                         shippingCompanyId = C.shippingCompanyId,
                                         commissionValue = C.commissionValue,
                                         commissionRatio = C.commissionRatio,
                                     }).Where(C => ((type == "all") ? true : C.transType == type)
                                                            && ((side == "all") ? true : C.side == side)
                                                            && C.invId == invId
                                                         && !hiddenCashes.Any(str => str.Contains(C.processType))
                                                            // && C.processType != "inv"
                                                            ).FirstOrDefault();



                        return TokenManager.GenerateToken(cachtrans);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }


            }
        }


        [HttpPost]
        [Route("GetListByInvId")]
        public string GetListByInvId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                int invId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invId")
                    {
                        invId = int.Parse(c.Value);
                    }
                }

                try
                {
                    InvoicesController ic = new InvoicesController();

                    int invoiceId = invId;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var invoice = entity.invoices.Where(x => x.invoiceId == invId)
                           .Select(x => new InvoiceModel()
                           {
                               invoiceId = x.invoiceId,
                               invoiceMainId = x.invoiceMainId,
                               invType = x.invType,
                           }).FirstOrDefault();

                        if (invoice.invType.Equals("s") || invoice.invType.Equals("p"))
                        {
                            while (invoice != null)
                            {
                                invoiceId = invoice.invoiceId;

                                if (invoice.invoiceMainId != null)
                                    invoice = ic.GetParentInv((int)invoice.invoiceId);
                                else
                                    break;
                            }
                        }
                        List<CashTransferModel> cachtrans = new List<CashTransferModel>();

                        cachtrans = (from C in entity.cashTransfer
                                     join b in entity.banks on C.bankId equals b.bankId into jb
                                     join a in entity.agents on C.agentId equals a.agentId into ja
                                     join p in entity.pos on C.posId equals p.posId into jp
                                     join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                     join u in entity.users on C.userId equals u.userId into ju
                                     from jbb in jb.DefaultIfEmpty()
                                     from jaa in ja.DefaultIfEmpty()
                                     from jpp in jp.DefaultIfEmpty()
                                     from juu in ju.DefaultIfEmpty()
                                     from jpcc in jpcr.DefaultIfEmpty()

                                     select new CashTransferModel()
                                     {
                                         cashTransId = C.cashTransId,
                                         transType = C.transType,
                                         posId = C.posId,
                                         userId = C.userId,
                                         agentId = C.agentId,
                                         invId = C.invId,
                                         transNum = C.transNum,
                                         createDate = C.createDate,
                                         updateDate = C.updateDate,
                                         cash = C.cash,
                                         updateUserId = C.updateUserId,
                                         createUserId = C.createUserId,
                                         notes = C.notes,
                                         posIdCreator = C.posIdCreator,
                                         isConfirm = C.isConfirm,
                                         cashTransIdSource = C.cashTransIdSource,
                                         side = C.side,

                                         docName = C.docName,
                                         docNum = C.docNum,
                                         docImage = C.docImage,
                                         bankId = C.bankId,
                                         bankName = jbb.name,
                                         agentName = jaa.name,
                                         usersName = juu.username,
                                         posName = jpp.name,
                                         posCreatorName = jpcc.name,
                                         processType = C.processType,
                                         cardId = C.cardId,
                                         cardName = entity.cards.Where(x => x.cardId == C.cardId).Select(x => x.name).FirstOrDefault(),
                                         bondId = C.bondId,
                                         shippingCompanyId = C.shippingCompanyId,
                                         commissionValue = C.commissionValue,
                                         commissionRatio = C.commissionRatio,
                                     }).Where(C => C.invId == invoiceId
                                       && C.processType != "inv" && C.processType != "deliver" && C.processType != "commissionAgent" && C.processType != "commissionCard"
                                     //&& !hiddenCashes.Any(str => str.Contains(C.processType))
                                     //&& C.processType != "inv"
                                     ).ToList();

                        int sequence = 1;
                        foreach(var row in cachtrans)
                        {
                            row.sequence = sequence;
                            sequence++;
                        }
                        return TokenManager.GenerateToken(cachtrans);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        [HttpPost]
        [Route("GetInvAndReturn")]
        public string GetInvAndReturn(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                int invId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invId")
                    {
                        invId = int.Parse(c.Value);
                    }
                }

                try
                {
                    InvoicesController ic = new InvoicesController();

                    int invoiceId = invId;
                    List<int> returnIds = new List<int>();

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var invoice = entity.invoices.Where(x => x.invoiceId == invId)
                           .Select(x => new InvoiceModel()
                           {
                               invoiceId = x.invoiceId,
                               invoiceMainId = x.invoiceMainId,
                               invType = x.invType,
                           }).FirstOrDefault();

                        if (invoice.invType.Equals("s") || invoice.invType.Equals("p"))
                        {
                            while (invoice != null)
                            {
                                invoiceId = invoice.invoiceId;


                                if (invoice != null)
                                {
                                    var returnInv = entity.invoices.Where(x => x.invoiceMainId == invoice.invoiceId && (x.invType == "sb" || x.invType == "pb" || x.invType == "pbw")).FirstOrDefault();
                                    if (returnInv != null)
                                        returnIds.Add(returnInv.invoiceId);
                                }
                                invoice = ic.GetChildInv((int)invoice.invoiceId, invoice.invType);
                            }
                        }
                        List<CashTransferModel> cachtrans = new List<CashTransferModel>();

                        cachtrans = (from C in entity.cashTransfer
                                     join b in entity.banks on C.bankId equals b.bankId into jb
                                     join a in entity.agents on C.agentId equals a.agentId into ja
                                     join p in entity.pos on C.posId equals p.posId into jp
                                     join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                     join u in entity.users on C.userId equals u.userId into ju
                                     from jbb in jb.DefaultIfEmpty()
                                     from jaa in ja.DefaultIfEmpty()
                                     from jpp in jp.DefaultIfEmpty()
                                     from juu in ju.DefaultIfEmpty()
                                     from jpcc in jpcr.DefaultIfEmpty()

                                     select new CashTransferModel()
                                     {
                                         cashTransId = C.cashTransId,
                                         transType = C.transType,
                                         posId = C.posId,
                                         userId = C.userId,
                                         agentId = C.agentId,
                                         invId = C.invId,
                                         transNum = C.transNum,
                                         createDate = C.createDate,
                                         updateDate = C.updateDate,
                                         cash = C.cash,
                                         updateUserId = C.updateUserId,
                                         createUserId = C.createUserId,
                                         notes = "payments",
                                         posIdCreator = C.posIdCreator,
                                         isConfirm = C.isConfirm,
                                         cashTransIdSource = C.cashTransIdSource,
                                         side = C.side,

                                         docName = C.docName,
                                         docNum = C.docNum,
                                         docImage = C.docImage,
                                         bankId = C.bankId,
                                         bankName = jbb.name,
                                         agentName = jaa.name,
                                         usersName = juu.username,
                                         posName = jpp.name,
                                         posCreatorName = jpcc.name,
                                         processType = C.processType,
                                         cardId = C.cardId,
                                         bondId = C.bondId,
                                         shippingCompanyId = C.shippingCompanyId,
                                         commissionValue = C.commissionValue,
                                         commissionRatio = C.commissionRatio,
                                     }).Where(C => C.invId == invId && !hiddenCashes.Any(str => str.Contains(C.processType))
                                     //&& C.processType != "inv"
                                     ).ToList();


                        var returnCachtrans = (from C in entity.cashTransfer
                                               join b in entity.banks on C.bankId equals b.bankId into jb
                                               join a in entity.agents on C.agentId equals a.agentId into ja
                                               join p in entity.pos on C.posId equals p.posId into jp
                                               join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                               join u in entity.users on C.userId equals u.userId into ju
                                               from jbb in jb.DefaultIfEmpty()
                                               from jaa in ja.DefaultIfEmpty()
                                               from jpp in jp.DefaultIfEmpty()
                                               from juu in ju.DefaultIfEmpty()
                                               from jpcc in jpcr.DefaultIfEmpty()

                                               select new CashTransferModel()
                                               {
                                                   cashTransId = C.cashTransId,
                                                   transType = C.transType,
                                                   posId = C.posId,
                                                   userId = C.userId,
                                                   agentId = C.agentId,
                                                   invId = C.invId,
                                                   transNum = C.transNum,
                                                   createDate = C.createDate,
                                                   updateDate = C.updateDate,
                                                   cash = -1 * C.cash,
                                                   updateUserId = C.updateUserId,
                                                   createUserId = C.createUserId,
                                                   notes = "return",
                                                   posIdCreator = C.posIdCreator,
                                                   isConfirm = C.isConfirm,
                                                   cashTransIdSource = C.cashTransIdSource,
                                                   side = C.side,

                                                   docName = C.docName,
                                                   docNum = C.docNum,
                                                   docImage = C.docImage,
                                                   bankId = C.bankId,
                                                   bankName = jbb.name,
                                                   agentName = jaa.name,
                                                   usersName = juu.username,
                                                   posName = jpp.name,
                                                   posCreatorName = jpcc.name,
                                                   processType = C.processType,
                                                   cardId = C.cardId,
                                                   bondId = C.bondId,
                                                   shippingCompanyId = C.shippingCompanyId,
                                                   commissionValue = C.commissionValue,
                                                   commissionRatio = C.commissionRatio,
                                               }).Where(C => returnIds.Contains((int)C.invId) && !hiddenCashes.Any(str => str.Contains(C.processType))
                                     //  && C.processType != "inv"
                                     ).ToList();

                        cachtrans.AddRange(returnCachtrans);

                        return TokenManager.GenerateToken(cachtrans);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }



            }
        }



        [HttpPost]
        [Route("GetCountByInvId")]
        public string GetCountByInvId(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int invId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "invoiceId")
                    {
                        invId = int.Parse(c.Value);
                    }
                }
                try
                {
                    int cachtrans = GetCountByInvId(invId);
                    return TokenManager.GenerateToken(cachtrans);
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        [NonAction]
        public int GetCountByInvId(int invId)
        {
            InvoicesController ic = new InvoicesController();
            int invoiceId = invId;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var invoice = entity.invoices.Where(x => x.invoiceId == invId)
                    .Select(x => new InvoiceModel()
                    {
                        invoiceId = x.invoiceId,
                        invoiceMainId = x.invoiceMainId,
                        invType = x.invType,
                    }).FirstOrDefault();
                if (invoice.invType.Equals("s") || invoice.invType.Equals("p"))
                {
                    while (invoice != null)
                    {
                        invoiceId = invoice.invoiceId;

                        if (invoice.invoiceMainId != null)
                            invoice = ic.GetParentInv((int)invoice.invoiceId);
                        else
                            break;
                    }
                }
                int cachtrans = entity.cashTransfer.Where(C => C.invId == invoiceId && C.processType != "balance" && !hiddenCashes.Any(str => str.Contains(C.processType))).ToList().Count();
                return cachtrans;
            }
        }

        [HttpPost]
        [Route("payByAmount")]
        public async Task<string> payByAmount(string token)
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
                #region params
                string Object = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                int agentId = 0;
                decimal amount = 0;
                string payType = "";
                cashTransfer cashTr = new cashTransfer();
                foreach (Claim c in claims)
                {
                    if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTr = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "agentId")
                    {
                        agentId = int.Parse(c.Value);
                    }
                    else if (c.Type == "amount")
                    {
                        amount = decimal.Parse(c.Value);
                    }
                    else if (c.Type == "payType")
                    {
                        payType = c.Value;
                    }

                }
                #endregion
                if (cashTr != null)
                {

                    try
                    {
                        List<string> typesList = new List<string>();

                        switch (payType)
                        {
                            case "pay"://get pw,pi,sb invoices

                                typesList.Add("pw");
                                typesList.Add("p");
                                typesList.Add("sb");
                                break;
                            case "feed": //get si, pb

                                typesList.Add("pb");
                                typesList.Add("s");
                                break;
                        }
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var invList = (from b in entity.invoices.Where(x => x.agentId == agentId && typesList.Contains(x.invType) && x.isActive == true
                                                                            && x.deserved > 0 &&
                                                                         ((x.shippingCompanyId == null && x.shipUserId == null && x.agentId != null) ||
                                                                          (x.shippingCompanyId != null && x.shipUserId != null && x.agentId != null)
                                                                          || x.shippingCompanyId != null && x.shipUserId == null && x.agentId != null && x.isPrePaid == 1))

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
                                               vendorInvNum = b.vendorInvNum,
                                               vendorInvDate = b.vendorInvDate,
                                               createUserId = b.createUserId,
                                               updateDate = b.updateDate,
                                               updateUserId = b.updateUserId,
                                               branchId = b.branchId,
                                               discountValue = b.discountValue,
                                               discountType = b.discountType,
                                               tax = b.tax,
                                               taxtype = b.taxtype,
                                               name = b.name,
                                               isApproved = b.isApproved,
                                               branchCreatorId = b.branchCreatorId,
                                               shippingCompanyId = b.shippingCompanyId,
                                               shipUserId = b.shipUserId,

                                           }).ToList().OrderBy(b => b.deservedDate).ToList();

                            invList = invList.Where(inv => inv.invoiceId == invList.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId).ToList();

                            List<InvoiceModel> res = new List<InvoiceModel>();
                            agents agent;
                            //get only with rc status
                            if (payType == "feed")
                            {
                                foreach (InvoiceModel inv in invList)
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

                            }
                            else
                            {
                                res.AddRange(invList);
                            }

                            if (res.ToList().Count > 0)
                            {
                                switch (payType)
                                {
                                    #region payments
                                    case "pay"://get pw,p,sb invoices

                                        foreach (InvoiceModel inv in res)
                                        {
                                            decimal paid = 0;
                                            agent = entity.agents.Find(agentId);
                                            decimal agentBalance = (decimal)agent.balance;
                                            var invObj = entity.invoices.Find(inv.invoiceId);
                                            cashTr.invId = inv.invoiceId;
                                            if (amount >= inv.deserved)
                                            {
                                                paid = (decimal)inv.deserved;
                                                invObj.paid += inv.deserved;
                                                invObj.deserved = 0;
                                                amount -= (decimal)inv.deserved;
                                            }
                                            else
                                            {
                                                paid = amount;
                                                invObj.paid = invObj.paid + amount;
                                                invObj.deserved -= amount;
                                                amount = 0;
                                            }
                                            cashTr.cash = paid;
                                            cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateUserId = cashTr.createUserId;
                                            cashTr.transNum = CashNumber(cashTr.transNum);
                                            entity.cashTransfer.Add(cashTr);

                                            // increase agent balance
                                            if (agent.balanceType == 0)
                                            {
                                                if (paid <= (decimal)agent.balance)
                                                {
                                                    agent.balance = agentBalance - paid;
                                                }
                                                else
                                                {
                                                    agent.balance = paid - agentBalance;
                                                    agent.balanceType = 1;
                                                }
                                            }
                                            else
                                            {
                                                agent.balance = agentBalance + paid;
                                            }

                                            entity.SaveChanges();

                                            if (amount == 0)
                                                break;
                                        }
                                        if (amount > 0) // save remain amount
                                        {
                                            agent = entity.agents.Find(agentId);
                                            decimal agentBalance = (decimal)agent.balance;
                                            cashTr.cash = amount;
                                            cashTr.invId = null;
                                            cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateUserId = cashTr.createUserId;
                                            cashTr.transNum = CashNumber(cashTr.transNum);
                                            entity.cashTransfer.Add(cashTr);

                                            // increase agent balance
                                            if (agent.balanceType == 0)
                                            {
                                                if (amount <= (decimal)agent.balance)
                                                {
                                                    agent.balance = agentBalance - amount;
                                                }
                                                else
                                                {
                                                    agent.balance = amount - agentBalance;
                                                    agent.balanceType = 1;
                                                }
                                            }
                                            else
                                            {
                                                agent.balance = agentBalance + amount;
                                            }
                                            entity.SaveChanges();
                                        }
                                        break;
                                    #endregion
                                    #region feed
                                    case "feed": //get s, pb
                                        foreach (InvoiceModel inv in res)
                                        {
                                            agent = entity.agents.Find(agentId);

                                            decimal paid = 0;
                                            var invObj = entity.invoices.Find(inv.invoiceId);
                                            cashTr.invId = inv.invoiceId;
                                            if (amount >= inv.deserved)
                                            {
                                                paid = (decimal)inv.deserved;
                                                invObj.paid = invObj.paid + inv.deserved;
                                                invObj.deserved = 0;
                                                amount -= (decimal)inv.deserved;
                                            }
                                            else
                                            {
                                                paid = amount;
                                                invObj.paid = invObj.paid + amount;
                                                invObj.deserved -= amount;
                                                amount = 0;
                                            }
                                            cashTr.cash = paid;
                                            cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateUserId = cashTr.createUserId;
                                            cashTr.transNum = CashNumber(cashTr.transNum);
                                            cashTr = entity.cashTransfer.Add(cashTr);
                                            entity.SaveChanges();
                                            // decrease agent balance
                                            if (agent.balanceType == 1)
                                            {
                                                if (paid <= (decimal)agent.balance)
                                                {
                                                    agent.balance -= paid;
                                                }
                                                else
                                                {
                                                    agent.balance = paid - agent.balance;
                                                    agent.balanceType = 0;
                                                }
                                            }
                                            else
                                            {
                                                agent.balance += paid;
                                            }

                                            entity.SaveChanges();

                                            if (cashTr.processType == "card")
                                                AddCardCommission(cashTr);

                                            if (amount == 0)
                                                break;
                                        }
                                        if (amount > 0) // save remain amount
                                        {
                                            agent = entity.agents.Find(agentId);

                                            cashTr.cash = amount;
                                            cashTr.invId = null;
                                            cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateUserId = cashTr.createUserId;
                                            cashTr.transNum = CashNumber(cashTr.transNum);
                                            entity.cashTransfer.Add(cashTr);

                                            // decrease agent balance
                                            if (agent.balanceType == 1)
                                            {
                                                if (amount <= (decimal)agent.balance)
                                                {
                                                    agent.balance = agent.balance - amount;
                                                }
                                                else
                                                {
                                                    agent.balance = amount - agent.balance;
                                                    agent.balanceType = 0;
                                                }
                                            }
                                            else
                                            {
                                                agent.balance += amount;
                                            }

                                            entity.SaveChanges();
                                        }
                                        break;
                                        #endregion
                                }
                                return TokenManager.GenerateToken("1");
                            }
                            else
                            {
                                if (amount > 0) // save remain amount
                                {
                                    switch (payType)
                                    {
                                        case "pay":
                                            agent = entity.agents.Find(agentId);
                                            decimal agentBalance = (decimal)agent.balance;
                                            cashTr.cash = amount;
                                            cashTr.invId = null;
                                            cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateUserId = cashTr.createUserId;
                                            cashTr.transNum = CashNumber(cashTr.transNum);
                                            entity.cashTransfer.Add(cashTr);

                                            // increase agent balance
                                            if (agent.balanceType == 0)
                                            {
                                                if (amount <= (decimal)agent.balance)
                                                {
                                                    agent.balance = agentBalance - amount;
                                                }
                                                else
                                                {
                                                    agent.balance = amount - agentBalance;
                                                    agent.balanceType = 1;
                                                }
                                            }
                                            else
                                            {
                                                agent.balance = agentBalance + amount;
                                            }
                                            entity.SaveChanges();
                                            break;
                                        case "feed":
                                            agent = entity.agents.Find(agentId);

                                            cashTr.cash = amount;
                                            cashTr.invId = null;
                                            cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateUserId = cashTr.createUserId;
                                            cashTr.transNum = CashNumber(cashTr.transNum);
                                            cashTr = entity.cashTransfer.Add(cashTr);

                                            // decrease agent balance
                                            if (agent.balanceType == 1)
                                            {
                                                if (amount <= (decimal)agent.balance)
                                                {
                                                    agent.balance = agent.balance - amount;
                                                }
                                                else
                                                {
                                                    agent.balance = amount - agent.balance;
                                                    agent.balanceType = 0;
                                                }
                                            }
                                            else
                                            {
                                                agent.balance += amount;
                                            }

                                            entity.SaveChanges();

                                            if (cashTr.processType == "card")
                                                AddCardCommission(cashTr);
                                            break;
                                    }
                                }
                                return TokenManager.GenerateToken("-1");
                            }
                        }

                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }


                }

                return TokenManager.GenerateToken("0");

            }

        }

        [NonAction]
        public async void AddDeliveryCash(int shippingCompanyId, int invoiceId, int userId, int posId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var company = entity.shippingCompanies.Find(shippingCompanyId);

                var cashTransfer = new cashTransfer()
                {
                    invId = invoiceId,
                    shippingCompanyId = shippingCompanyId,
                    posId = posId,
                    cash = company.RealDeliveryCost,
                    deserved = company.RealDeliveryCost,
                    paid = 0,
                    processType = "deliver",
                    transType = "d",
                    side = "shd",
                    transNum = await generateCashNumber("d" + "shd"),
                    isCommissionPaid = 0,
                    createUserId = userId,
                    updateUserId = userId,
                    createDate = cc.AddOffsetTodate(DateTime.Now),
                    updateDate = cc.AddOffsetTodate(DateTime.Now),
                };

                entity.cashTransfer.Add(cashTransfer);
                entity.SaveChanges();

                increaseShippingComBalance(shippingCompanyId, (decimal)company.RealDeliveryCost);
            }
        }


        [NonAction]
        public void increaseShippingComBalance(int shippingComId, decimal paid)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var com = entity.shippingCompanies.Find(shippingComId);
                if (com.balanceType == 1)
                {
                    if (paid <= (decimal)com.balance)
                    {
                        com.balance = com.balance - paid;
                    }
                    else
                    {
                        com.balance = paid - com.balance;
                        com.balanceType = 0;
                    }
                }
                else if (com.balanceType == 0)
                {
                    com.balance += paid;
                }
                entity.SaveChanges();
            }
        }
        [NonAction]
        public void decreaseShippingComBalance(int shippingComId, decimal paid)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var com = entity.shippingCompanies.Find(shippingComId);
                if (com.balanceType == 0)
                {
                    if (paid <= (decimal)com.balance)
                    {
                        com.balance = com.balance - paid;
                    }
                    else
                    {
                        com.balance = paid - com.balance;
                        com.balanceType = 1;
                    }
                }
                else if (com.balanceType == 1)
                {
                    com.balance += paid;
                }
                entity.SaveChanges();
            }
        }

        [NonAction]
        public async void AddAgentCommission(int userId, int invoiceId, decimal invoiceValue, int posId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var user = entity.users.Find(userId);
                if (user.hasCommission)
                {
                    decimal commissionValue = 0;
                    if (user.commissionValue > 0)
                        commissionValue += (decimal)user.commissionValue;
                    if (user.commissionRatio > 0)
                    {
                        Calculate calc = new Calculate();
                        commissionValue += calc.percentValue(invoiceValue, user.commissionRatio);
                    }

                    if (commissionValue > 0)
                    {
                        var cashTransfer = new cashTransfer()
                        {
                            invId = invoiceId,
                            userId = userId,
                            posId = posId,
                            cash = commissionValue,
                            deserved = commissionValue,
                            paid = 0,
                            processType = "commissionAgent",
                            transType = "d",
                            side = "u",
                            transNum = await generateCashNumber("d" + "u"),
                            isCommissionPaid = 0,
                            commissionRatio = user.commissionRatio,
                            commissionValue = user.commissionValue,
                            createUserId = userId,
                            updateUserId = userId,
                            createDate = cc.AddOffsetTodate(DateTime.Now),
                            updateDate = cc.AddOffsetTodate(DateTime.Now),
                        };

                        entity.cashTransfer.Add(cashTransfer);
                        entity.SaveChanges();

                        increaseUserBalance(userId, commissionValue);
                    }
                }
            }
        }

        [NonAction]
        public void increaseUserBalance(int userId, decimal paid)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var user = entity.users.Find(userId);
                if (user.balanceType == 1)
                {
                    if (paid <= (decimal)user.balance)
                    {
                        user.balance = user.balance - paid;
                    }
                    else
                    {
                        user.balance = paid - user.balance;
                        user.balanceType = 0;
                    }
                }
                else if (user.balanceType == 0)
                {
                    user.balance += paid;
                }
                entity.SaveChanges();
            }
        }
        [NonAction]
        public void decreaseUserBalance(int userId, decimal paid)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var user = entity.users.Find(userId);
                if (user.balanceType == 0)
                {
                    if (paid <= (decimal)user.balance)
                    {
                        user.balance = user.balance - paid;
                    }
                    else
                    {
                        user.balance = paid - user.balance;
                        user.balanceType = 1;
                    }
                }
                else if (user.balanceType == 1)
                {
                    user.balance += paid;
                }
                entity.SaveChanges();
            }
        }
        [NonAction]
        public async void AddCardCommission(cashTransfer basicCash)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var card = entity.cards.Find(basicCash.cardId);

                decimal commissionValue = 0;
                if (card.commissionValue > 0)
                    commissionValue += (decimal)card.commissionValue;
                if (card.commissionRatio > 0)
                {
                    Calculate calc = new Calculate();
                    commissionValue += calc.percentValue(basicCash.cash, card.commissionRatio);
                }

                if (commissionValue > 0)
                {
                    var cashTransfer = new cashTransfer()
                    {
                        invId = basicCash.invId,
                        cashTransIdSource = basicCash.cashTransId,
                        cardId = basicCash.cardId,
                        posId = basicCash.posId,
                        cash = commissionValue,
                        deserved = 0,
                        paid = commissionValue,
                        processType = "commissionCard",
                        transType = "d",
                        side = "card",
                        isCommissionPaid = 1,
                        commissionValue = card.commissionValue,
                        commissionRatio = card.commissionRatio,
                        transNum = await generateCashNumber("d" + "u"),
                        createUserId = basicCash.createUserId,
                        updateUserId = basicCash.createUserId,
                        createDate = cc.AddOffsetTodate(DateTime.Now),
                        updateDate = cc.AddOffsetTodate(DateTime.Now),
                    };

                    entity.cashTransfer.Add(cashTransfer);
                    entity.SaveChanges();

                    // increaseCardBalance((int)basicCash.cardId, commissionValue);
                }

            }
        }

        [NonAction]
        public void increaseCardBalance(int cardId, decimal paid)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var card = entity.cards.Find(cardId);
                if (card.balanceType == 1)
                {
                    if (paid <= (decimal)card.balance)
                    {
                        card.balance = card.balance - paid;
                    }
                    else
                    {
                        card.balance = paid - card.balance;
                        card.balanceType = 0;
                    }
                }
                else if (card.balanceType == 0)
                {
                    card.balance += paid;
                }
                entity.SaveChanges();
            }
        }
        [HttpPost]
        [Route("payUserByAmount")]
        public async Task<string> payUserByAmount(string token)
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
                string Object = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                decimal amount = 0;
                int userId = 0;

                string payType = "";
                cashTransfer cashTr = new cashTransfer();
                foreach (Claim c in claims)
                {
                    if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTr = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "amount")
                    {
                        amount = int.Parse(c.Value);
                    }
                    else if (c.Type == "payType")
                    {
                        payType = c.Value;
                    }

                }
                #endregion
                if (cashTr != null)
                {
                    try
                    {
                        List<string> typesList = new List<string>();

                        switch (payType)
                        {
                            case "feed": //get si, pb

                                typesList.Add("pb");
                                typesList.Add("s");
                                break;
                        }
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var invList = (from b in entity.invoices.Where(x => x.userId == userId && typesList.Contains(x.invType) && x.deserved > 0)
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
                                               vendorInvNum = b.vendorInvNum,
                                               vendorInvDate = b.vendorInvDate,
                                               createUserId = b.createUserId,
                                               updateDate = b.updateDate,
                                               updateUserId = b.updateUserId,
                                               branchId = b.branchId,
                                               discountValue = b.discountValue,
                                               discountType = b.discountType,
                                               tax = b.tax,
                                               taxtype = b.taxtype,
                                               name = b.name,
                                               isApproved = b.isApproved,
                                               branchCreatorId = b.branchCreatorId,
                                               shippingCompanyId = b.shippingCompanyId,
                                               shipUserId = b.shipUserId,
                                               userId = b.userId
                                           }).ToList().OrderBy(b => b.deservedDate).ToList();

                            invList = invList.Where(inv => inv.invoiceId == invList.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId).ToList();

                            users user;
                            if (invList.ToList().Count > 0)
                            {
                                switch (payType)
                                {
                                    #region payments

                                    #region feed
                                    case "feed": //get s, pb
                                        foreach (InvoiceModel inv in invList)
                                        {
                                            user = entity.users.Find(userId);

                                            decimal paid = 0;
                                            var invObj = entity.invoices.Find(inv.invoiceId);
                                            cashTr.invId = inv.invoiceId;
                                            if (amount >= inv.deserved)
                                            {
                                                paid = (decimal)inv.deserved;
                                                invObj.paid = invObj.paid + inv.deserved;
                                                invObj.deserved = 0;
                                                amount -= (decimal)inv.deserved;
                                            }
                                            else
                                            {
                                                paid = amount;
                                                invObj.paid = invObj.paid + amount;
                                                invObj.deserved -= amount;
                                                amount = 0;
                                            }
                                            cashTr.cash = paid;
                                            cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateUserId = cashTr.createUserId;
                                            cashTr.transNum = CashNumber(cashTr.transNum);
                                            entity.cashTransfer.Add(cashTr);


                                            // decrease user balance
                                            if (user.balanceType == 1)
                                            {
                                                if (paid <= (decimal)user.balance)
                                                {
                                                    user.balance -= paid;
                                                }
                                                else
                                                {
                                                    user.balance = paid - user.balance;
                                                    user.balanceType = 0;
                                                }
                                            }
                                            else
                                            {
                                                user.balance += paid;
                                            }


                                            entity.SaveChanges();

                                            if (amount == 0)
                                                break;
                                        }
                                        if (amount > 0) // save remain amount
                                        {
                                            user = entity.users.Find(userId);

                                            cashTr.cash = amount;
                                            cashTr.invId = null;
                                            cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateUserId = cashTr.createUserId;
                                            cashTr.transNum = CashNumber(cashTr.transNum);
                                            entity.cashTransfer.Add(cashTr);

                                            // decrease user balance
                                            if (user.balanceType == 1)
                                            {
                                                if (amount <= (decimal)user.balance)
                                                {
                                                    user.balance = user.balance - amount;
                                                }
                                                else
                                                {
                                                    user.balance = amount - user.balance;
                                                    user.balanceType = 0;
                                                }
                                            }
                                            else
                                            {
                                                user.balance += amount;
                                            }

                                            entity.SaveChanges();
                                        }
                                        break;
                                        #endregion
                                }
                                return TokenManager.GenerateToken("1");
                            }
                            else
                            {
                                if (amount > 0) // save remain amount
                                {
                                    switch (payType)
                                    {
                                        case "feed":
                                            user = entity.users.Find(userId);

                                            cashTr.cash = amount;
                                            cashTr.invId = null;
                                            cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateUserId = cashTr.createUserId;
                                            cashTr.transNum = CashNumber(cashTr.transNum);
                                            entity.cashTransfer.Add(cashTr);

                                            // decrease user balance
                                            if (user.balanceType == 1)
                                            {
                                                if (amount <= (decimal)user.balance)
                                                {
                                                    user.balance = user.balance - amount;
                                                }
                                                else
                                                {
                                                    user.balance = amount - user.balance;
                                                    user.balanceType = 0;
                                                }
                                            }
                                            else
                                            {
                                                user.balance += amount;
                                            }

                                            entity.SaveChanges();
                                            break;
                                    }
                                }
                                return TokenManager.GenerateToken("-1");
                            }
                        }
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        //

        /// </summary>
        /// <param name="shippingCompanyId"></param>
        /// <param name="amount"></param>
        /// <param name="payType">{feed}</param>
        /// <param name="cashTransfer"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        [Route("payShippingCompanyByAmount")]
        public async Task<string> payShippingCompanyByAmount(string token)
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
                string Object = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                decimal amount = 0;
                int shippingCompanyId = 0;

                string payType = "";
                cashTransfer cashTr = new cashTransfer();
                foreach (Claim c in claims)
                {
                    if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTr = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "shippingCompanyId")
                    {
                        shippingCompanyId = int.Parse(c.Value);
                    }
                    else if (c.Type == "amount")
                    {
                        amount = int.Parse(c.Value);
                    }
                    else if (c.Type == "payType")
                    {
                        payType = c.Value;
                    }

                }
                #endregion
                if (cashTr != null)
                {
                    try
                    {
                        List<string> typesList = new List<string>();
                        string cashIds = "";
                        switch (payType)
                        {

                            case "feed": //get si, pb

                                typesList.Add("pb");
                                typesList.Add("s");
                                break;
                        }
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            List<InvoiceModel> invList = new List<InvoiceModel>();
                            var res = (from b in entity.invoices.Where(x => x.shippingCompanyId == shippingCompanyId && typesList.Contains(x.invType) && x.deserved > 0 &&
                                                                                x.shippingCompanyId != null && x.shipUserId == null && x.agentId != null)

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
                                           vendorInvNum = b.vendorInvNum,
                                           vendorInvDate = b.vendorInvDate,
                                           createUserId = b.createUserId,
                                           updateDate = b.updateDate,
                                           updateUserId = b.updateUserId,
                                           branchId = b.branchId,
                                           discountValue = b.discountValue,
                                           discountType = b.discountType,
                                           tax = b.tax,
                                           taxtype = b.taxtype,
                                           name = b.name,
                                           isApproved = b.isApproved,
                                           branchCreatorId = b.branchCreatorId,
                                           shippingCompanyId = b.shippingCompanyId,
                                           shipUserId = b.shipUserId,
                                           userId = b.userId
                                       }).ToList().OrderBy(b => b.deservedDate).ToList();

                            res = res.Where(inv => inv.invoiceId == res.Where(i => i.invNumber == inv.invNumber).ToList().OrderBy(i => i.invoiceId).FirstOrDefault().invoiceId).ToList();

                            foreach (var inv in res)
                            {
                                var statusObj = entity.invoiceStatus.Where(x => x.invoiceId == inv.invoiceId && x.status == "Done").FirstOrDefault();

                                if (statusObj != null)
                                    invList.Add(inv);
                            }
                            shippingCompanies shippingCompany;
                            if (invList.ToList().Count > 0)
                            {
                                switch (payType)
                                {
                                    case "feed": //get s, pb
                                        foreach (InvoiceModel inv in invList)
                                        {
                                            shippingCompany = entity.shippingCompanies.Find(shippingCompanyId);

                                            decimal paid = 0;
                                            var invObj = entity.invoices.Find(inv.invoiceId);
                                            cashTr.invId = inv.invoiceId;
                                            if (amount >= inv.deserved)
                                            {
                                                paid = (decimal)inv.deserved;
                                                invObj.paid = invObj.paid + inv.deserved;
                                                invObj.deserved = 0;
                                                amount -= (decimal)inv.deserved;

                                                #region make invoice status as "Done"
                                                InvoiceStatusController isc = new InvoiceStatusController();
                                                var st = new invoiceStatus()
                                                {
                                                    status = "Done",
                                                    invoiceId = inv.invoiceId,
                                                    createUserId = cashTr.createUserId,
                                                };
                                                isc.Save(st);
                                                #endregion
                                            }
                                            else
                                            {
                                                paid = amount;
                                                invObj.paid = invObj.paid + amount;
                                                invObj.deserved -= amount;
                                                amount = 0;
                                            }
                                            cashTr.cash = paid;
                                            cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateUserId = cashTr.createUserId;
                                            cashTr.transNum = CashNumber(cashTr.transNum);
                                            cashTr = entity.cashTransfer.Add(cashTr);

                                            // decrease shipping balance
                                            if (shippingCompany.balanceType == 1)
                                            {
                                                if (paid <= (decimal)shippingCompany.balance)
                                                {
                                                    shippingCompany.balance -= paid;
                                                }
                                                else
                                                {
                                                    shippingCompany.balance = paid - shippingCompany.balance;
                                                    shippingCompany.balanceType = 0;
                                                }
                                            }
                                            else
                                            {
                                                shippingCompany.balance += paid;
                                            }

                                            entity.SaveChanges();

                                            //add card commission
                                            if (cashTr.processType.Trim() == "card")
                                                AddCardCommission(cashTr);

                                            if (amount == 0)
                                                break;
                                        }
                                        if (amount > 0) // save remain amount
                                        {
                                            shippingCompany = entity.shippingCompanies.Find(shippingCompanyId);

                                            cashTr.cash = amount;
                                            cashTr.invId = null;
                                            cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateUserId = cashTr.createUserId;
                                            cashTr.transNum = CashNumber(cashTr.transNum);
                                            cashTr = entity.cashTransfer.Add(cashTr);

                                            // decrease shipping balance
                                            if (shippingCompany.balanceType == 1)
                                            {
                                                if (amount <= (decimal)shippingCompany.balance)
                                                {
                                                    shippingCompany.balance = shippingCompany.balance - amount;
                                                }
                                                else
                                                {
                                                    shippingCompany.balance = amount - shippingCompany.balance;
                                                    shippingCompany.balanceType = 0;
                                                }
                                            }
                                            else
                                            {
                                                shippingCompany.balance += amount;
                                            }

                                            entity.SaveChanges();

                                            //add card commission
                                            if (cashTr.processType.Trim() == "card")
                                                AddCardCommission(cashTr);
                                        }
                                        break;
                                        #endregion
                                }
                                TokenManager.GenerateToken(cashIds.ToString());
                            }
                            else
                            {
                                if (amount > 0) // save remain amount
                                {
                                    switch (payType)
                                    {
                                        case "feed":
                                            shippingCompany = entity.shippingCompanies.Find(shippingCompanyId);

                                            cashTr.cash = amount;
                                            cashTr.invId = null;
                                            cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                            cashTr.updateUserId = cashTr.createUserId;
                                            cashTr.transNum = CashNumber(cashTr.transNum);
                                            entity.cashTransfer.Add(cashTr);

                                            // decrease shipping balance
                                            if (shippingCompany.balanceType == 1)
                                            {
                                                if (amount <= (decimal)shippingCompany.balance)
                                                {
                                                    shippingCompany.balance = shippingCompany.balance - amount;
                                                }
                                                else
                                                {
                                                    shippingCompany.balance = amount - shippingCompany.balance;
                                                    shippingCompany.balanceType = 0;
                                                }
                                            }
                                            else
                                            {
                                                shippingCompany.balance += amount;
                                            }

                                            entity.SaveChanges();

                                            //add card commission
                                            if (cashTr.processType.Trim() == "card")
                                                AddCardCommission(cashTr);
                                            break;
                                    }
                                }
                                TokenManager.GenerateToken("-1");
                            }
                        }


                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }
                return TokenManager.GenerateToken("0");
            }

        }

        [HttpPost]
        [Route("payListOfInvoices")]
        public async Task<string> payListOfInvoices(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string Object = "";
                string listObject = "";
                List<invoices> invoiceList = new List<invoices>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                int agentId = 0;

                string payType = "";
                cashTransfer cashTr = new cashTransfer();
                foreach (Claim c in claims)
                {
                    if (c.Type == "invoices")
                    {
                        listObject = c.Value.Replace("\\", string.Empty);
                        listObject = listObject.Trim('"');
                        invoiceList = JsonConvert.DeserializeObject<List<invoices>>(listObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTr = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }

                    else if (c.Type == "agentId")
                    {
                        agentId = int.Parse(c.Value);
                    }

                    else if (c.Type == "payType")
                    {
                        payType = c.Value;
                    }

                }
                if (cashTr != null)
                {
                    try
                    {

                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            agents agent = entity.agents.Find(agentId);

                            switch (payType)
                            {
                                case "pay"://get pw,p,sb invoices
                                    foreach (invoices inv in invoiceList)
                                    {
                                        decimal paid = 0;
                                        var invObj = entity.invoices.Find(inv.invoiceId);
                                        cashTr.invId = inv.invoiceId;

                                        paid = (decimal)inv.deserved;
                                        invObj.paid = invObj.paid + inv.deserved;
                                        invObj.deserved = 0;

                                        cashTr.cash = paid;
                                        cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                        cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                        cashTr.updateUserId = cashTr.createUserId;
                                        cashTr.transNum = CashNumber(cashTr.transNum);
                                        cashTr = entity.cashTransfer.Add(cashTr);


                                        // increase agent balance
                                        if (agent.balanceType == 0)
                                        {
                                            if (paid <= (decimal)agent.balance)
                                            {
                                                agent.balance = agent.balance - paid;
                                            }
                                            else
                                            {
                                                agent.balance = paid - agent.balance;
                                                agent.balanceType = 1;
                                            }
                                        }
                                        else if (agent.balanceType == 1)
                                        {
                                            agent.balance += paid;
                                        }
                                        entity.SaveChanges();
                                    }
                                    entity.SaveChanges();
                                    break;
                                case "feed": //get s, pb
                                    foreach (invoices inv in invoiceList)
                                    {
                                        decimal paid = 0;

                                        var invObj = entity.invoices.Find(inv.invoiceId);
                                        cashTr.invId = inv.invoiceId;

                                        paid = (decimal)inv.deserved;
                                        invObj.paid = invObj.paid + inv.deserved;
                                        invObj.deserved = 0;

                                        cashTr.cash = paid;
                                        cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                        cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                        cashTr.updateUserId = cashTr.createUserId;
                                        cashTr.transNum = CashNumber(cashTr.transNum);
                                        cashTr = entity.cashTransfer.Add(cashTr);

                                        // decrease agent balance
                                        if (agent.balanceType == 1)
                                        {
                                            if (paid <= (decimal)agent.balance)
                                            {
                                                agent.balance = agent.balance - paid;
                                            }
                                            else
                                            {
                                                agent.balance = paid - agent.balance;
                                                agent.balanceType = 0;
                                            }
                                        }
                                        else if (agent.balanceType == 0)
                                        {
                                            agent.balance += paid;
                                        }
                                        entity.SaveChanges();

                                        if (cashTr.processType == "card")
                                            AddCardCommission(cashTr);
                                    }
                                    entity.SaveChanges();
                                    break;
                            }
                            return TokenManager.GenerateToken("1");

                        }


                    }
                    catch
                    {
                        return TokenManager.GenerateToken("-2");
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }
            }


        }


        [HttpPost]
        [Route("payUserListOfInvoices")]
        public async Task<string> payUserListOfInvoices(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string cashIds = "";
                string Object = "";
                string listObject = "";
                List<invoices> invoiceList = new List<invoices>();
                // bondes newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                int userId = 0;

                string payType = "";
                cashTransfer cashTr = new cashTransfer();
                foreach (Claim c in claims)
                {
                    if (c.Type == "invoices")
                    {
                        listObject = c.Value.Replace("\\", string.Empty);
                        listObject = listObject.Trim('"');
                        invoiceList = JsonConvert.DeserializeObject<List<invoices>>(listObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTr = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }

                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                    else if (c.Type == "payType")
                    {
                        payType = c.Value;
                    }

                }
                if (cashTr != null)
                {


                    try
                    {

                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            users user = entity.users.Find(userId);

                            switch (payType)
                            {
                                case "feed": //get s, pb
                                    foreach (invoices inv in invoiceList)
                                    {
                                        decimal paid = 0;

                                        var invObj = entity.invoices.Find(inv.invoiceId);
                                        cashTr.invId = inv.invoiceId;

                                        paid = (decimal)inv.deserved;
                                        invObj.paid = invObj.paid + inv.deserved;
                                        invObj.deserved = 0;

                                        cashTr.cash = paid;
                                        cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                        cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                        cashTr.updateUserId = cashTr.createUserId;
                                        cashTr.transNum = CashNumber(cashTr.transNum);
                                        entity.cashTransfer.Add(cashTr);

                                        // decrease user balance
                                        if (user.balanceType == 1)
                                        {
                                            if (paid <= (decimal)user.balance)
                                            {
                                                user.balance = user.balance - paid;
                                            }
                                            else
                                            {
                                                user.balance = paid - user.balance;
                                                user.balanceType = 0;
                                            }
                                        }
                                        else if (user.balanceType == 0)
                                        {
                                            user.balance += paid;
                                        }

                                        entity.SaveChanges();
                                    }
                                    entity.SaveChanges();
                                    break;
                            }
                            //return Ok(cashIds);
                            return TokenManager.GenerateToken("1");

                        }



                    }
                    catch
                    {
                        return TokenManager.GenerateToken("-2");
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }
                //  return TokenManager.GenerateToken("0");
            }


        }


        [HttpPost]
        [Route("payShippingCompanyListOfInvoices")]
        public async Task<string> payShippingCompanyListOfInvoices(string token)
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
                string Object = "";
                string listObject = "";
                List<invoices> invoiceList = new List<invoices>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                int shippingCompanyId = 0;

                string payType = "";
                cashTransfer cashTr = new cashTransfer();
                foreach (Claim c in claims)
                {
                    if (c.Type == "invoices")
                    {
                        listObject = c.Value.Replace("\\", string.Empty);
                        listObject = listObject.Trim('"');
                        invoiceList = JsonConvert.DeserializeObject<List<invoices>>(listObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTr = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }

                    else if (c.Type == "shippingCompanyId")
                    {
                        shippingCompanyId = int.Parse(c.Value);
                    }

                    else if (c.Type == "payType")
                    {
                        payType = c.Value;
                    }

                }
                #endregion
                if (cashTr != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            shippingCompanies shippingCompany = entity.shippingCompanies.Find(shippingCompanyId);

                            switch (payType)
                            {
                                case "feed": //get s, pb
                                    foreach (invoices inv in invoiceList)
                                    {
                                        decimal paid = 0;
                                        var invObj = entity.invoices.Find(inv.invoiceId);
                                        cashTr.invId = inv.invoiceId;

                                        paid = (decimal)inv.deserved;
                                        invObj.paid = invObj.paid + inv.deserved;
                                        invObj.deserved = 0;

                                        cashTr.cash = paid;
                                        cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                        cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                        cashTr.updateUserId = cashTr.createUserId;
                                        cashTr.transNum = CashNumber(cashTr.transNum);
                                        cashTr = entity.cashTransfer.Add(cashTr);

                                        // decrease shippingCompany balance
                                        if (shippingCompany.balanceType == 1)
                                        {
                                            if (paid <= (decimal)shippingCompany.balance)
                                            {
                                                shippingCompany.balance = shippingCompany.balance - paid;
                                            }
                                            else
                                            {
                                                shippingCompany.balance = paid - shippingCompany.balance;
                                                shippingCompany.balanceType = 0;
                                            }
                                        }
                                        else if (shippingCompany.balanceType == 0)
                                        {
                                            shippingCompany.balance += paid;
                                        }

                                        #region make invoice status as "Done"
                                        InvoiceStatusController isc = new InvoiceStatusController();
                                        var st = new invoiceStatus()
                                        {
                                            status = "Done",
                                            invoiceId = inv.invoiceId,
                                            createUserId = cashTr.createUserId,
                                        };
                                        isc.Save(st);
                                        #endregion
                                        entity.SaveChanges();
                                        //add card commission
                                        if (cashTr.processType == "card")
                                            AddCardCommission(cashTr);
                                    }
                                    entity.SaveChanges();
                                    break;
                            }
                            return TokenManager.GenerateToken("1");

                        }

                    }
                    catch
                    {
                        return TokenManager.GenerateToken("-2");
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }
            }


        }
        [HttpPost]
        [Route("payDeliveryCostOfInvoices")]
        public async Task<string> payDeliveryCostOfInvoices(string token)
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
                string Object = "";
                List<cashTransfer> cashesList = new List<cashTransfer>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                int shippingCompanyId = 0;

                cashTransfer cashTr = new cashTransfer();
                foreach (Claim c in claims)
                {
                    if (c.Type == "invoices")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashesList = JsonConvert.DeserializeObject<List<cashTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTr = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }

                    else if (c.Type == "shippingCompanyId")
                    {
                        shippingCompanyId = int.Parse(c.Value);
                    }

                }
                #endregion
                if (cashTr != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            foreach (var cash in cashesList)
                            {
                                decimal paid = 0;
                                var deliverCash = entity.cashTransfer.Find(cash.cashTransId);
                                deliverCash.paid += deliverCash.deserved;
                                paid = (decimal)deliverCash.deserved;
                                deliverCash.deserved = 0;
                                deliverCash.isCommissionPaid = 1;


                                cashTr.cash = paid;
                                cashTr.cashTransIdSource = cash.cashTransId;
                                cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateUserId = cashTr.createUserId;
                                cashTr.transNum = CashNumber(cashTr.transNum);
                                entity.cashTransfer.Add(cashTr);


                                entity.SaveChanges();
                                decreaseShippingComBalance((int)cash.shippingCompanyId, paid);
                            }


                            return TokenManager.GenerateToken("1");

                        }

                    }
                    catch
                    {
                        return TokenManager.GenerateToken("-1");
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("-1");
                }
            }

        }

        [HttpPost]
        [Route("payDeliveryCostByAmount")]
        public async Task<string> payDeliveryCostByAmount(string token)
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
                string Object = "";
                decimal amount = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                int shippingCompanyId = 0;

                cashTransfer cashTr = new cashTransfer();
                foreach (Claim c in claims)
                {
                    if (c.Type == "amount")
                    {
                        amount = decimal.Parse(c.Value);

                    }
                    else if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTr = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }

                    else if (c.Type == "shippingCompanyId")
                    {
                        shippingCompanyId = int.Parse(c.Value);
                    }

                }
                #endregion
                if (cashTr != null)
                {
                    try
                    {
                        decimal basicAmount = amount;
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var cashesList = entity.cashTransfer.Where(x => x.shippingCompanyId == shippingCompanyId && x.processType.Trim() == "deliver" && x.deserved > 0).ToList();


                            foreach (var cash in cashesList)
                            {
                                var statusObj = entity.invoiceStatus.Where(x => x.invoiceId == cash.invId && x.status == "Done").FirstOrDefault();

                                if (statusObj != null)
                                {
                                    decimal paid = 0;
                                    if (amount >= cash.deserved)
                                    {
                                        paid = (decimal)cash.deserved;
                                        amount -= (decimal)cash.deserved;
                                        cash.paid += cash.deserved;
                                        cash.deserved = 0;
                                        cash.isCommissionPaid = 1;

                                    }
                                    else
                                    {
                                        paid = amount;
                                        cash.paid = cash.paid + amount;
                                        cash.deserved -= amount;
                                        amount = 0;
                                    }

                                    cashTr.cash = paid;
                                    cashTr.cashTransIdSource = cash.cashTransId;
                                    cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                    cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                    cashTr.updateUserId = cashTr.createUserId;
                                    cashTr.transNum = CashNumber(cashTr.transNum);
                                    entity.cashTransfer.Add(cashTr);

                                    //decreaseShippingComBalance(shippingCompanyId,paid);


                                    entity.SaveChanges();
                                }
                            }
                            if (amount > 0)
                            {
                                cashTr.cash = amount;
                                cashTr.cashTransIdSource = null;
                                cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateUserId = cashTr.createUserId;
                                cashTr.transNum = CashNumber(cashTr.transNum);
                                entity.cashTransfer.Add(cashTr);

                                entity.SaveChanges();
                            }
                            decreaseShippingComBalance(shippingCompanyId, basicAmount);

                            return TokenManager.GenerateToken("1");

                        }

                    }
                    catch
                    {
                        return TokenManager.GenerateToken("-1");
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("-1");
                }
            }

        }

        [HttpPost]
        [Route("payListShortageCashes")]
        public async Task<string> payListShortageCashes(string token)
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
                string Object = "";
                List<cashTransfer> cashesList = new List<cashTransfer>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);


                cashTransfer cashTr = new cashTransfer();
                foreach (Claim c in claims)
                {
                    if (c.Type == "cashesList")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashesList = JsonConvert.DeserializeObject<List<cashTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTr = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }

                }
                #endregion
                if (cashTr != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {

                            foreach (var cash in cashesList)
                            {
                                decimal paid = 0;
                                var deliverCash = entity.cashTransfer.Find(cash.cashTransId);
                                deliverCash.paid += deliverCash.deserved;
                                paid = (decimal)deliverCash.deserved;
                                deliverCash.deserved = 0;
                                deliverCash.isCommissionPaid = 1;


                                cashTr.cash = paid;
                                cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateUserId = cashTr.createUserId;
                                cashTr.transNum = CashNumber(cashTr.transNum);
                                cashTr = entity.cashTransfer.Add(cashTr);


                                entity.SaveChanges();
                                increaseUserBalance((int)deliverCash.userId, paid);

                                //add card commission
                                if (cashTr.processType == "card")
                                    AddCardCommission(cashTr);
                            }


                            return TokenManager.GenerateToken("1");

                        }

                    }
                    catch
                    {
                        return TokenManager.GenerateToken("-1");
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("-1");
                }
            }

        }

        [HttpPost]
        [Route("payShortageCashesByAmount")]
        public async Task<string> payShortageCashesByAmount(string token)
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
                string Object = "";
                int userId = 0;
                decimal amount = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);


                cashTransfer cashTr = new cashTransfer();
                foreach (Claim c in claims)
                {
                    if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "amount")
                    {
                        amount = decimal.Parse(c.Value);
                    }
                    else if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTr = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }

                }
                #endregion
                if (cashTr != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var cashesList = entity.cashTransfer.Where(x => x.userId == userId
                                                                  && (x.processType.Trim() == "destroy" || x.processType.Trim() == "shortage")
                                                                  && x.deserved > 0).ToList();

                            decimal basicAmount = amount;
                            foreach (var cash in cashesList)
                            {
                                decimal paid = 0;
                                if (amount >= cash.deserved)
                                {
                                    paid = (decimal)cash.deserved;
                                    amount -= (decimal)cash.deserved;
                                    cash.paid += cash.deserved;
                                    cash.deserved = 0;
                                    cash.isCommissionPaid = 1;

                                }
                                else
                                {
                                    paid = amount;
                                    cash.paid = cash.paid + amount;
                                    cash.deserved -= amount;
                                    amount = 0;
                                }
                                entity.SaveChanges();
                                cashTr.cash = paid;
                                cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateUserId = cashTr.createUserId;
                                cashTr.transNum = CashNumber(cashTr.transNum);
                                cashTr.cashTransIdSource = cash.cashTransId;
                                cashTr = entity.cashTransfer.Add(cashTr);

                                entity.SaveChanges();

                                //add card commission
                                if (cashTr.processType == "card")
                                    AddCardCommission(cashTr);

                                if (amount == 0)
                                    break;
                            }

                            if (amount > 0)
                            {
                                cashTr.cash = amount;
                                cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateUserId = cashTr.createUserId;
                                cashTr.transNum = CashNumber(cashTr.transNum);
                                cashTr.cashTransIdSource = null;
                                cashTr = entity.cashTransfer.Add(cashTr);
                                entity.SaveChanges();
                                //add card commission
                                if (cashTr.processType == "card")
                                    AddCardCommission(cashTr);
                            }

                            increaseUserBalance(userId, basicAmount);

                            return TokenManager.GenerateToken("1");

                        }

                    }
                    catch
                    {
                        return TokenManager.GenerateToken("-1");
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("-1");
                }
            }

        }

        [HttpPost]
        [Route("payListCommissionCashes")]
        public async Task<string> payListCommissionCashes(string token)
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
                string Object = "";
                List<cashTransfer> cashesList = new List<cashTransfer>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);


                cashTransfer cashTr = new cashTransfer();
                foreach (Claim c in claims)
                {
                    if (c.Type == "cashesList")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashesList = JsonConvert.DeserializeObject<List<cashTransfer>>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }
                    else if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTr = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }

                }
                #endregion
                if (cashTr != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {

                            foreach (var cash in cashesList)
                            {
                                decimal paid = 0;

                                var deliverCash = entity.cashTransfer.Find(cash.cashTransId);
                                deliverCash.paid += deliverCash.deserved;
                                paid = (decimal)deliverCash.deserved;
                                deliverCash.deserved = 0;
                                deliverCash.isCommissionPaid = 1;
                                entity.SaveChanges();

                                cashTr.cash = paid;
                                cashTr.cashTransIdSource = cash.cashTransId;
                                cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateUserId = cashTr.createUserId;
                                cashTr.transNum = CashNumber(cashTr.transNum);
                                entity.cashTransfer.Add(cashTr);
                                entity.SaveChanges();

                                //entity.SaveChanges();
                                decreaseUserBalance((int)deliverCash.userId, paid);
                            }


                            return TokenManager.GenerateToken("1");

                        }

                    }
                    catch
                    {
                        return TokenManager.GenerateToken("-1");
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("-1");
                }
            }

        }

        [HttpPost]
        [Route("payCommissionCashesByAmount")]
        public async Task<string> payCommissionCashesByAmount(string token)
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
                string Object = "";
                int userId = 0;
                decimal amount = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);


                cashTransfer cashTr = new cashTransfer();
                foreach (Claim c in claims)
                {
                    if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "amount")
                    {
                        amount = decimal.Parse(c.Value);
                    }
                    else if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTr = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }

                }
                #endregion
                if (cashTr != null)
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            var cashesList = entity.cashTransfer.Where(x => x.userId == userId
                                                                  && x.processType.Trim() == "commissionAgent"
                                                                  && x.deserved > 0).ToList();

                            decimal basicAmount = amount;
                            foreach (var cash in cashesList)
                            {
                                decimal paid = 0;
                                if (amount >= cash.deserved)
                                {
                                    paid = (decimal)cash.deserved;
                                    amount -= (decimal)cash.deserved;
                                    cash.paid += cash.deserved;
                                    cash.deserved = 0;
                                    cash.isCommissionPaid = 1;
                                }
                                else
                                {
                                    paid = amount;
                                    cash.paid = cash.paid + amount;
                                    cash.deserved -= amount;
                                    amount = 0;
                                }
                                entity.SaveChanges();
                                cashTr.cash = paid;
                                cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateUserId = cashTr.createUserId;
                                cashTr.transNum = CashNumber(cashTr.transNum);
                                cashTr.cashTransIdSource = cash.cashTransId;
                                entity.cashTransfer.Add(cashTr);

                                entity.SaveChanges();

                                //increaseUserBalance(userId,paid);
                                if (amount == 0)
                                    break;
                            }

                            if (amount > 0)
                            {
                                cashTr.cash = amount;
                                cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                cashTr.updateUserId = cashTr.createUserId;
                                cashTr.transNum = CashNumber(cashTr.transNum);
                                cashTr.cashTransIdSource = null;

                                entity.cashTransfer.Add(cashTr);
                                entity.SaveChanges();
                            }
                            decreaseUserBalance(userId, basicAmount);

                            return TokenManager.GenerateToken("1");

                        }

                    }
                    catch
                    {
                        return TokenManager.GenerateToken("-1");
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("-1");
                }
            }

        }


        [HttpPost]
        [Route("payOrderInvoice")]
        public async Task<string> payOrderInvoice(string token)
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
                string Object = "";
                List<invoices> invoiceList = new List<invoices>();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                int invoiceId = 0;
                int invStatusId = 0;
                string payType = "";
                decimal amount = 0;
                cashTransfer cashTr = new cashTransfer();
                foreach (Claim c in claims)
                {

                    if (c.Type == "cashTransfer")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        cashTr = JsonConvert.DeserializeObject<cashTransfer>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    }

                    else if (c.Type == "invoiceId")
                    {
                        invoiceId = int.Parse(c.Value);
                    }
                    else if (c.Type == "invStatusId")
                    {
                        invStatusId = int.Parse(c.Value);
                    }
                    else if (c.Type == "amount")
                    {
                        amount = decimal.Parse(c.Value);
                    }
                    else if (c.Type == "payType")
                    {
                        payType = c.Value;
                    }

                }
                if (cashTr != null)
                {


                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            invoices inv = entity.invoices.Find(invoiceId);
                            agents agent = entity.agents.Find(inv.agentId);
                            invoiceStatus invStatus = entity.invoiceStatus.Find(invStatusId);

                            //update invoice type
                            //invStatus.status = "tr";
                            invStatus.status = "Collected";
                            //add cashtransfer
                            cashTr.invId = inv.invoiceId;
                            cashTr.cash = amount;
                            cashTr.createDate = cc.AddOffsetTodate(DateTime.Now);
                            cashTr.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            cashTr.updateUserId = cashTr.createUserId;

                            cashTransfer ct;
                            //update agent
                            switch (payType)
                            {
                                case "0":// cash - card - cheque - doc

                                    //update invoice
                                    inv.paid += amount;
                                    inv.deserved -= amount;
                                    await addCashTransfer(cashTr);

                                    // decrease agent balance
                                    if (agent.balanceType == 0)
                                    {
                                        if (amount <= (decimal)agent.balance)
                                        {
                                            agent.balance -= amount;
                                        }
                                        else
                                        {
                                            agent.balance = amount - agent.balance;
                                            agent.balanceType = 1;
                                        }
                                    }
                                    else if (agent.balanceType == 1)
                                    {
                                        if (amount <= (decimal)agent.balance)
                                        {
                                            agent.balance -= amount;
                                        }
                                        else
                                        {
                                            agent.balance = amount - agent.balance;
                                            agent.balanceType = 0;
                                        }
                                    }
                                    break;
                                case "1":// balance
                                    decimal newBalance = 0;
                                    if (agent.balanceType == 0)
                                    {
                                        //cash
                                        cashTr.transType = "balance";
                                        if (amount <= (decimal)agent.balance)
                                        {
                                            //invoice
                                            inv.paid += amount;
                                            inv.deserved -= amount;
                                            //agent
                                            newBalance = (decimal)agent.balance - amount;
                                            agent.balance = newBalance;
                                        }
                                        else
                                        {
                                            //invoice
                                            inv.paid += agent.balance;
                                            inv.deserved -= agent.balance;
                                            //agent
                                            newBalance = (decimal)amount - (decimal)agent.balance;
                                            agent.balance = newBalance;
                                            agent.balanceType = 1;
                                            //cash
                                            cashTr.cash = newBalance;
                                        }

                                    }
                                    else if (agent.balanceType == 1)
                                    {
                                        newBalance = (decimal)agent.balance + amount;
                                        agent.balance = newBalance;
                                    }
                                    break;
                            }
                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);

                        }

                    }
                    catch
                    {
                        return TokenManager.GenerateToken("-2");
                    }
                }
                else
                {
                    return TokenManager.GenerateToken("0");
                }
            }


        }
        [HttpPost]
        [Route("GetLastNumOfCash")]
        public string GetLastNumOfCash(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string cashCode = "";

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);



                foreach (Claim c in claims)
                {
                    if (c.Type == "cashCode")
                    {
                        cashCode = c.Value;
                    }
                }
                try
                {

                    List<string> numberList;
                    int lastNum = 0;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        numberList = entity.cashTransfer.Where(b => b.transNum.Contains(cashCode + "-")).Select(b => b.transNum).ToList();

                        for (int i = 0; i < numberList.Count; i++)
                        {
                            string code = numberList[i];
                            string s = code.Substring(code.LastIndexOf("-") + 1);
                            numberList[i] = s;
                        }
                        if (numberList.Count > 0)
                        {
                            numberList.Sort();
                            lastNum = int.Parse(numberList[numberList.Count - 1]);
                        }
                    }

                    return TokenManager.GenerateToken(lastNum.ToString());
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }
        }

        [HttpPost]
        [Route("GetLastNumOfDocNum")]
        public string GetLastNumOfDocNum(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string docNum = "";

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);

                foreach (Claim c in claims)
                {
                    if (c.Type == "docNum")
                    {
                        docNum = c.Value;
                    }
                }

                try
                {

                    List<string> numberList;
                    int lastNum = 0;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        numberList = entity.cashTransfer.Where(b => b.docNum.Contains(docNum + "-")).Select(b => b.docNum).ToList();

                        for (int i = 0; i < numberList.Count; i++)
                        {
                            string code = numberList[i];
                            string s = code.Substring(code.LastIndexOf("-") + 1);
                            numberList[i] = s;
                        }
                        if (numberList.Count > 0)
                        {
                            numberList.Sort();
                            lastNum = int.Parse(numberList[numberList.Count - 1]);
                        }
                    }
                    return TokenManager.GenerateToken(lastNum.ToString());
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }
        }

        [HttpPost]
        [Route("getLastOpenTransNum")]
        public string getLastOpenTransNum(string token)
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

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                }
                try
                {
                    string numberList = "";
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        numberList = entity.cashTransfer.Where(b => b.posId == posId && b.transType == "o").ToList().OrderBy(b => b.cashTransId).LastOrDefault().transNum;
                    }
                    return TokenManager.GenerateToken(numberList);
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        [NonAction]
        public async Task<string> generateCashNumber(string cashCode)
        {
            #region check if last of code is num
            string num = cashCode.Substring(cashCode.LastIndexOf("-") + 1);

            if (!num.Equals(cashCode))
                return cashCode;

            #endregion

            List<string> numberList;
            int sequence = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {
                numberList = entity.cashTransfer.Where(b => b.transNum.Contains(cashCode + "-")).Select(b => b.transNum).ToList();

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
                    catch { }
                }
            }
            sequence++;

            string strSeq = sequence.ToString();
            if (sequence <= 999999)
                strSeq = sequence.ToString().PadLeft(6, '0');
            string transNum = cashCode + "-" + strSeq;
            return transNum;
        }
        [NonAction]
        public string CashNumber(string cashCode)
        {
            #region check if last of code is num
            string num = cashCode.Substring(cashCode.LastIndexOf("-") + 1);

            if (!num.Equals(cashCode))
                return cashCode;

            #endregion

            List<string> numberList;
            int sequence = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {
                numberList = entity.cashTransfer.Where(b => b.transNum.Contains(cashCode + "-")).Select(b => b.transNum).ToList();

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
                    catch { }
                }
            }
            sequence++;

            string strSeq = sequence.ToString();
            if (sequence <= 999999)
                strSeq = sequence.ToString().PadLeft(6, '0');
            string transNum = cashCode + "-" + strSeq;
            return transNum;
        }

        [NonAction]
        public int GetCountTransferForPosByUserId(int userId,string side,string type)
        {

                BranchesController branchctrlr = new BranchesController();

                List<BranchModel> bmList = new List<BranchModel>();
                bmList = branchctrlr.BranchesByUserIdType(userId);
                List<int> brIds = bmList.Select(S => S.branchId).ToList();

                using (incposdbEntities entity = new incposdbEntities())
                {
                    List<CashTransferModel> cachlist = (from C in entity.cashTransfer
                                                        join b in entity.banks on C.bankId equals b.bankId into jb
                                                        join a in entity.agents on C.agentId equals a.agentId into ja
                                                        join p in entity.pos on C.posId equals p.posId into jp
                                                        join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                                        join u in entity.users on C.userId equals u.userId into ju
                                                        join uc in entity.users on C.createUserId equals uc.userId into juc
                                                        join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                                        join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                                        join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
                                                        from jbb in jb.DefaultIfEmpty()
                                                        from jaa in ja.DefaultIfEmpty()
                                                        from jpp in jp.DefaultIfEmpty()
                                                        from juu in ju.DefaultIfEmpty()
                                                        from jpcc in jpcr.DefaultIfEmpty()
                                                        from jucc in juc.DefaultIfEmpty()
                                                        from jcrd in jcr.DefaultIfEmpty()
                                                        from jbbo in jbo.DefaultIfEmpty()
                                                        from jssh in jsh.DefaultIfEmpty()
                                                        select new CashTransferModel()
                                                        {
                                                            cashTransId = C.cashTransId,
                                                            transType = C.transType,
                                                            posId = C.posId,
                                                            userId = C.userId,
                                                            agentId = C.agentId,
                                                            invId = C.invId,
                                                            transNum = C.transNum,
                                                            createDate = C.createDate,
                                                            updateDate = C.updateDate,
                                                            cash = C.cash,
                                                            updateUserId = C.updateUserId,
                                                            createUserId = C.createUserId,
                                                            notes = C.notes,
                                                            posIdCreator = C.posIdCreator,
                                                            isConfirm = C.isConfirm,
                                                            cashTransIdSource = C.cashTransIdSource,
                                                            side = C.side,

                                                            docName = C.docName,
                                                            docNum = C.docNum,
                                                            docImage = C.docImage,
                                                            bankId = C.bankId,
                                                            bankName = jbb.name,
                                                            agentName = jaa.name,
                                                            usersName = juu.name,// side =u

                                                            posName = jpp.name,
                                                            posCreatorName = jpcc.name,
                                                            processType = C.processType,
                                                            cardId = C.cardId,
                                                            bondId = C.bondId,
                                                            usersLName = juu.lastname,// side =u
                                                            createUserName = jucc.name,
                                                            createUserLName = jucc.lastname,
                                                            createUserJob = jucc.job,
                                                            cardName = jcrd.name,
                                                            bondDeserveDate = jbbo.deserveDate,
                                                            bondIsRecieved = jbbo.isRecieved,
                                                            shippingCompanyId = C.shippingCompanyId,
                                                            shippingCompanyName = jssh.name,
                                                            commissionValue = C.commissionValue,
                                                            commissionRatio = C.commissionRatio,
                                                            branchId = jpp.branchId,
                                                            branchName = jpp.branches.name,
                                                            branchCreatorId = jpcc.branchId,
                                                            branchCreatorname = jpcc.branches.name,

                                                        }).Where(C => ((type == "all") ? true : C.transType == type) && (C.processType != "balance")
                                    && ((side == "all") ? true : C.side == side)).ToList();

                    if (cachlist.Count > 0 && side == "p")
                    {
                        CashTransferModel tempitem = null;
                        foreach (CashTransferModel cashtItem in cachlist)
                        {
                            tempitem = this.Getpostransmodel(cashtItem.cashTransId)
                                .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
                            cashtItem.cashTrans2Id = tempitem.cashTransId;
                            cashtItem.pos2Id = tempitem.posId;
                            cashtItem.pos2Name = tempitem.posName;
                            cashtItem.isConfirm2 = tempitem.isConfirm;
                            cashtItem.branch2Id = tempitem.branchId;
                            cashtItem.branch2Name = tempitem.branchName;
                        }

                    }
                    cachlist = cachlist.Where(C => (brIds.Contains((int)C.branchId) || brIds.Contains((int)C.branch2Id) || brIds.Contains((int)C.branchCreatorId))
                    && C.transType == "p"
                    && (C.isConfirm == 0 || C.isConfirm2 == 0)).ToList();

                return cachlist.Count;
                }

        }

        [Route("getNotPaidAgentCommission")]
        [HttpPost]
        public string getNotPaidAgentCommission(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var cashes = (from b in entity.cashTransfer.Where(x => x.userId == userId
                                                                    && x.processType == "commissionAgent" && x.deserved > 0)
                                  select new CashTransferModel()
                                  {
                                      cashTransId = b.cashTransId,
                                      invId = b.invId,
                                      agentId = b.agentId,
                                      cash = b.cash,
                                      deserved = b.deserved,
                                      paid = b.paid,
                                      notes = b.notes,
                                      createUserId = b.createUserId,
                                      updateDate = b.updateDate,
                                      updateUserId = b.updateUserId,
                                      shippingCompanyId = b.shippingCompanyId,
                                      transNum = b.transNum,


                                  }).ToList();

                    return TokenManager.GenerateToken(cashes);
                }
            }
        }

        [Route("getNotPaidShortage")]
        [HttpPost]
        public string getNotPaidShortage(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var cashes = (from b in entity.cashTransfer.Where(x => x.userId == userId
                                                                    && (x.processType.Trim() == "destroy" || x.processType.Trim() == "shortage")
                                                                    && x.deserved > 0)
                                  select new CashTransferModel()
                                  {
                                      cashTransId = b.cashTransId,
                                      invId = b.invId,
                                      agentId = b.agentId,
                                      cash = b.cash,
                                      deserved = b.deserved,
                                      paid = b.paid,
                                      notes = b.notes,
                                      createUserId = b.createUserId,
                                      updateDate = b.updateDate,
                                      updateUserId = b.updateUserId,
                                      shippingCompanyId = b.shippingCompanyId,
                                      transNum = b.transNum,


                                  }).ToList();

                    return TokenManager.GenerateToken(cashes);
                }
            }
        }
    }
}

