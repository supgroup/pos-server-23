using Newtonsoft.Json;
using POS_Server.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using POS_Server.Classes;
using POS_Server.Models.VM;
using System.Security.Claims;
using System.Web;
using Newtonsoft.Json.Converters;
using System.Web;
using LinqKit;
//using Nancy.Json;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/dash")]
    public class dashController : ApiController
    {
        CountriesController cc = new CountriesController();
        // get all dash board info
        [HttpPost]
        [Route("GetDashInfo")]
        public string GetDashInfo(string token)
        {

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                Calculate calc = new Calculate();


                int mainBranchId = 0;
                int userId = 0;

                string IUList = "";
                List<itemsUnits> newiuObj = new List<itemsUnits>();

                string result = "";
                string jsonStr = "";

                int year;
                int month;
                int days;
                DateTime currentdate = cc.AddOffsetTodate(DateTime.Now);
                year = currentdate.Year;
                month = currentdate.Month;
                days = calc.getdays(year, month);

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "IUList")
                    {
                        IUList = c.Value.Replace("\\", string.Empty);
                        IUList = IUList.Trim('"');
                        newiuObj = JsonConvert.DeserializeObject<List<itemsUnits>>(IUList, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }

                }
                try
                {
                    StatisticsController sts = new StatisticsController();
                    List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        result += "{";

                        #region Getdashsalpur
                        var invListm = (from I in entity.invoices

                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId) && (I.invType == "p" || I.invType == "pw" || I.invType == "s" || I.invType == "pbw" || I.invType == "pb" || I.invType == "sb"))

                                        select new
                                        {
                                            I.invoiceId,
                                            I.invType,
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                        }).ToList();

                        var list = invListm.GroupBy(g => g.branchCreatorId).Select(g => new
                        {
                            invType = g.FirstOrDefault().invType,
                            branchCreatorId = g.FirstOrDefault().branchCreatorId,
                            branchCreatorName = g.FirstOrDefault().branchCreatorName,
                            purCount = g.Where(i => (i.invType == "p" || i.invType == "pw")).Count(),
                            saleCount = g.Where(i => i.invType == "s").Count(),
                            purBackCount = g.Where(i => (i.invType == "pbw" || i.invType == "pb")).Count(),
                            saleBackCount = g.Where(i => i.invType == "sb").Count(),
                        }).ToList();

                        result += "ListSalPur:";
                        jsonStr = JsonConvert.SerializeObject(list);
                        result += jsonStr + ',';
                        #endregion


                        #region GetTotalPurSale



                        List<branches> brlist = new List<branches>();
                        List<TotalPurSale> totalinMonth = new List<TotalPurSale>();
                        List<TotalPurSale> totalinday = new List<TotalPurSale>();
                        List<TotalPurSale> listTotal = new List<TotalPurSale>();
                        TotalPurSale totalAllBranchRow = new TotalPurSale();
                        TotalPurSale totalRowtemp = new TotalPurSale();

                        brlist = entity.branches.ToList();
                        brlist = brlist.Where(x => (x.branchId != 1 && x.isActive == 1)).Select(b => new branches
                        {
                            branchId = b.branchId,
                            name = b.name,
                            isActive = b.isActive,
                        }).ToList();



                        for (int i = 1; i <= days; i++)
                        {
                            DateTime daydate = new DateTime(year, month, i);
                            totalinday = new List<TotalPurSale>();

                            totalinday = GetTotalPurSaleday(daydate, i, mainBranchId, userId);
                            totalinMonth.AddRange(totalinday);
                        }

                        for (int i = 1; i <= days; i++)
                        {
                            foreach (branches row in brlist)
                            {
                                totalAllBranchRow = new TotalPurSale();
                                totalAllBranchRow.branchCreatorId = row.branchId;
                                totalAllBranchRow.branchCreatorName = row.name;
                                totalAllBranchRow.day = i;
                                totalAllBranchRow.totalPur = 0;
                                totalAllBranchRow.totalSale = 0;
                                totalAllBranchRow.countPur = 0;
                                totalAllBranchRow.countSale = 0;
                                listTotal.Add(totalAllBranchRow);

                            }


                        }

                        foreach (TotalPurSale rowinv in listTotal)
                        {
                            totalRowtemp = new TotalPurSale();
                            totalRowtemp = totalinMonth.Where(b => (b.day == rowinv.day && b.branchCreatorId == rowinv.branchCreatorId)).FirstOrDefault();
                            if (totalRowtemp != null)
                            {
                                rowinv.totalPur = totalRowtemp.totalPur;
                                rowinv.totalSale = totalRowtemp.totalSale;
                                rowinv.countPur = totalRowtemp.countPur;
                                rowinv.countSale = totalRowtemp.countSale;
                            }

                        }

                        result += "MonthlySalPur:";
                        jsonStr = JsonConvert.SerializeObject(listTotal);
                        result += jsonStr + ',';
                        #endregion

                        #region DailySalPur

                        var invListmtmp = (from I in entity.invoices

                                           join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC

                                           from JBCC in JBC.DefaultIfEmpty()
                                           where (brIds.Contains(JBCC.branchId) && (I.invType == "p" || I.invType == "pw" || I.invType == "s" || I.invType == "pbw" || I.invType == "pb" || I.invType == "sb"))
                                           select new
                                           {
                                               I.invoiceId,
                                               I.invType,
                                               I.branchCreatorId,
                                               branchCreatorName = JBCC.name,
                                               I.updateDate,
                                               I.invDate,
                                           }).ToList();

                        var invListd = invListmtmp.Where(X => DateTime.Compare(
                           (DateTime)calc.changeDateformat(X.invDate, "yyyy-MM-dd")
                           , (DateTime)calc.changeDateformat(cc.AddOffsetTodate(DateTime.Now), "yyyy-MM-dd")) == 0).ToList();


                        var listDaily = invListd.GroupBy(g => g.branchCreatorId).Select(g => new
                        {
                            invType = g.FirstOrDefault().invType,
                            branchCreatorId = g.FirstOrDefault().branchCreatorId,
                            branchCreatorName = g.FirstOrDefault().branchCreatorName,
                            purCount = g.Where(i => (i.invType == "p" || i.invType == "pw")).Count(),
                            saleCount = g.Where(i => i.invType == "s").Count(),
                            purBackCount = g.Where(i => (i.invType == "pbw" || i.invType == "pb")).Count(),
                            saleBackCount = g.Where(i => i.invType == "sb").Count(),
                        }).ToList();

                        result += "DailySalPur:";
                        jsonStr = JsonConvert.SerializeObject(listDaily);
                        result += jsonStr + ',';

                        #endregion

                        #region agent count                      

                        //var invListy = (from A in entity.agents
                        //                select new
                        //                {
                        //                    A.type,
                        //                }).ToList();



                        //var agentList = invListy.GroupBy(g => g.type).Select(g => new
                        //{
                        //    type = g.FirstOrDefault().type,

                        //    vendorCount = g.Where(i => i.type == "v").Count(),
                        //    customerCount = g.Where(i => i.type == "c").Count(),
                        //    grp = 1,
                        //}).ToList().GroupBy(g => g.grp).Select(c => new
                        //{

                        //    vendorCount = c.Sum(d => d.vendorCount),
                        //    customerCount = c.Sum(d => d.customerCount),
                        //}).ToList();
                        List<AgentsCountbyBranch> agentBranchlist = new List<AgentsCountbyBranch>();
                        agentBranchlist = GetagentsCount(brIds);

                        result += "ListAgentCount:";
                        jsonStr = JsonConvert.SerializeObject(agentBranchlist);
                        result += jsonStr + ',';

                        #endregion

                        #region users list

                        var listPosinbranch = entity.pos.Select(s => new
                        {
                            s.branchId,
                            s.posId,
                            s.isActive,
                            s.branches.name,
                        }).ToList();
                        // get Active Pos count in every branch
                        var listposb = listPosinbranch.Where(x => x.isActive == 1).GroupBy(g => g.branchId).Select(g => new
                        {
                            posAll = g.Count(),
                            g.FirstOrDefault().branchId,
                            g.FirstOrDefault().name,

                        }).ToList();
                        List<UserOnlineCount> listU = new List<UserOnlineCount>();

                        foreach (var row in listposb)
                        {
                            UserOnlineCount newrow = new UserOnlineCount();
                            newrow.allPos = row.posAll;
                            newrow.branchId = (int)row.branchId;
                            newrow.branchName = row.name;
                            newrow.offlineUsers = row.posAll;
                            newrow.userOnlineCount = 0;

                            listU.Add(newrow);

                        }

                        var invListu = (from log in entity.usersLogs
                                        join p in entity.pos on log.posId equals p.posId
                                        join u in entity.users on log.userId equals u.userId

                                        where (log.sOutDate == null && log.users.isOnline == 1)

                                        select new
                                        {
                                            log.userId,
                                            p.branchId,
                                            branchName = p.branches.name,
                                            branchisActive = p.branches.isActive,

                                            log.posId,
                                            posName = p.name,
                                            posisActive = p.isActive,


                                        }).ToList();


                        List<userOnlineInfo> grouplist = invListu.GroupBy(g => new { g.branchId, g.userId }).Select(g => new userOnlineInfo
                        {


                            branchId = g.LastOrDefault().branchId,
                            branchName = g.LastOrDefault().branchName,
                            branchisActive = g.LastOrDefault().branchisActive,

                            posId = g.LastOrDefault().posId,
                            posName = g.LastOrDefault().posName,
                            posisActive = g.LastOrDefault().posisActive,

                            userId = g.LastOrDefault().userId,

                            //usernameAccount = g.LastOrDefault().usernameAccount,
                            //userName = g.LastOrDefault().userName,
                            //lastname = g.LastOrDefault().lastname,
                            //job = g.LastOrDefault().job,
                            //phone = g.LastOrDefault().phone,
                            //mobile = g.LastOrDefault().mobile,
                            //email = g.LastOrDefault().email,
                            //address = g.LastOrDefault().address,
                            //userisActive = g.LastOrDefault().userisActive,
                            //isOnline = g.LastOrDefault().isOnline,
                            //image = g.LastOrDefault().image,



                        }).ToList();

                        List<UserOnlineCount> grop = grouplist.GroupBy(g => g.branchId).Select(g => new UserOnlineCount
                        {
                            userOnlineCount = g.Count(),
                            allPos = listposb.Where(b => b.branchId == g.FirstOrDefault().branchId).FirstOrDefault().posAll,
                            offlineUsers = listposb.Where(b => b.branchId == g.FirstOrDefault().branchId).FirstOrDefault().posAll - g.Count(),//offline= all -online
                            branchId = (int)g.FirstOrDefault().branchId,
                            branchName = g.FirstOrDefault().branchName,

                        }).ToList();

                        listU = listU.Where(X => brIds.Contains(X.branchId)).ToList();
                        foreach (UserOnlineCount finalrow in listU)
                        {
                            UserOnlineCount temp = new UserOnlineCount();
                            temp = grop.Where(x => x.branchId == finalrow.branchId).FirstOrDefault();
                            if (temp != null)
                            {
                                finalrow.offlineUsers = temp.offlineUsers;
                                finalrow.userOnlineCount = temp.userOnlineCount;

                            }
                        }

                        result += "UsersList:";

                        jsonStr = JsonConvert.SerializeObject(listU);
                        result += jsonStr + ',';
                        #endregion

                        #region Branch online List
                        List<BranchOnlineCount> BList = new List<BranchOnlineCount>();

                        var allBranchesList = entity.branches.ToList();
                        int allBranches = allBranchesList
                            .Select(b => new
                            {
                                b.branchId,
                                b.isActive,
                            }).Where(b => (b.branchId != 1 && b.isActive == 1)).ToList().Count();
                        var invListb = (from log in entity.usersLogs
                                        join p in entity.pos on log.posId equals p.posId
                                        where (brIds.Contains((int)p.branchId) && (log.sOutDate == null && log.users.isOnline == 1))

                                        select new
                                        {
                                            log.userId,
                                            p.branchId,
                                            branchName = p.branches.name,

                                        }).ToList();


                        var Bgrouplist = invListb.GroupBy(g => new { g.branchId, g.userId }).Select(g => new
                        {
                            g.FirstOrDefault().userId,
                            g.FirstOrDefault().branchId,
                            g.FirstOrDefault().branchName,

                        }).ToList();
                        List<UserOnlineCount> Bgrop = Bgrouplist.GroupBy(g => g.branchId).Select(g => new UserOnlineCount
                        {

                            branchId = (int)g.FirstOrDefault().branchId,
                            branchName = g.FirstOrDefault().branchName,
                        }).ToList();
                        BranchOnlineCount brow = new BranchOnlineCount();
                        brow.branchAll = allBranches;
                        brow.branchOnline = grop.Count();
                        brow.branchOffline = brIds.Count() - grop.Count();
                        BList.Add(brow);

                        result += "ListBranchOnline:";
                        jsonStr = JsonConvert.SerializeObject(BList);
                        result += jsonStr + ',';

                        #endregion

                        #region best seller


                        var invListBest = (from IT in entity.itemsTransfer
                                           from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)
                                           from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
                                           join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                           join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                           join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                           from JBCC in JBC.DefaultIfEmpty()
                                           where (brIds.Contains(JBCC.branchId) && (I.invType == "s"))

                                           select new
                                           {

                                               itemName = ITEM.name,
                                               unitName = UNIT.name,
                                               itemsTransId = IT.itemsTransId,
                                               itemUnitId = IT.itemUnitId,

                                               itemId = IU.itemId,
                                               unitId = IU.unitId,
                                               quantity = IT.quantity,
                                               price = IT.price,
                                               I.invoiceId,
                                               I.invType,
                                               I.updateDate,
                                               I.branchCreatorId,
                                               branchCreatorName = JBCC.name,
                                               Totalrow = (IT.price * IT.quantity),

                                               ITupdateDate = IT.updateDate,
                                               I.invDate,

                                           }).ToList();

                        var invListm2 = invListBest.Where(X => DateTime.Compare(
                       (DateTime)calc.changeDateformat(X.invDate, "yyyy-MM")
                       , (DateTime)calc.changeDateformat(DateTime.Now, "yyyy-MM")) == 0).ToList();

                        var listBest = invListm2.GroupBy(g => new { g.branchCreatorId, g.itemUnitId })
                            .Select(g => new
                            {
                                itemName = g.FirstOrDefault().itemName,
                                unitName = g.FirstOrDefault().unitName,
                                itemUnitId = g.FirstOrDefault().itemUnitId,

                                itemId = g.FirstOrDefault().itemId,
                                unitId = g.FirstOrDefault().unitId,
                                quantity = g.Sum(s => s.quantity),

                                price = g.FirstOrDefault().price,

                                branchCreatorId = g.FirstOrDefault().branchCreatorId,
                                branchCreatorName = g.FirstOrDefault().branchCreatorName,
                                subTotal = g.Sum(s => s.Totalrow),

                            }).OrderByDescending(o => o.quantity).ToList().Take(10);

                        result += "ListBestSeller:";
                        jsonStr = JsonConvert.SerializeObject(listBest);
                        result += jsonStr + ',';

                        #endregion

                        #region IUStorage

                        List<IUStorage> IUnitlist = new List<IUStorage>();
                        List<int> iuIds = new List<int>();
                        foreach (itemsUnits row in newiuObj)
                        {

                            foreach (branches branchRow in brlist)
                            {
                                IUStorage newrow = new IUStorage();
                                newrow.itemUnitId = row.itemUnitId;
                                newrow.quantity = 0;
                                newrow.branchId = branchRow.branchId;
                                newrow.branchName = branchRow.name;
                                newrow.itemId = entity.itemsUnits.Find(row.itemUnitId).itemId;
                                newrow.unitId = entity.itemsUnits.Find(row.itemUnitId).unitId; ;
                                newrow.itemName = entity.itemsUnits.Find(row.itemUnitId).items.name;
                                newrow.unitName = entity.itemsUnits.Find(row.itemUnitId).units.name;

                                IUnitlist.Add(newrow);


                            }
                            iuIds.Add(row.itemUnitId);

                        }

                        var invListmtemp = (from L in entity.locations
                                                //  from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)


                                            join IUL in entity.itemsLocations on L.locationId equals IUL.locationId
                                            join IU in entity.itemsUnits on IUL.itemUnitId equals IU.itemUnitId

                                            //  join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                            //join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                            join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                            join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                            //   join S in entity.sections on L.sectionId equals S.sectionId into JS
                                            join B in entity.branches on L.branchId equals B.branchId into JB

                                            // join UPUSR in entity.users on IUL.updateUserId equals UPUSR.userId into JUPUS
                                            //  join U in entity.users on IUL.createUserId equals U.userId into JU

                                            from JBB in JB
                                            where (brIds.Contains(JBB.branchId) && iuIds.Contains(IU.itemUnitId))
                                            //  from JSS in JS.DefaultIfEmpty()
                                            // from JUU in JU.DefaultIfEmpty()
                                            // from JUPUSS in JUPUS.DefaultIfEmpty()
                                            select new
                                            {
                                                // item unit
                                                itemName = ITEM.name,
                                                unitName = UNIT.name,
                                                IU.itemUnitId,

                                                IU.itemId,
                                                IU.unitId,
                                                branchName = JBB.name,

                                                branchType = JBB.type,
                                                IUL.itemsLocId,
                                                IUL.locationId,
                                                IUL.quantity,
                                                L.branchId,



                                            }).ToList();

                        List<IUStorage> invListIU = invListmtemp.GroupBy(g => new { g.branchId, g.itemUnitId })
                              .Select(s => new IUStorage
                              {
                                  itemName = s.FirstOrDefault().itemName,
                                  unitName = s.FirstOrDefault().unitName,
                                  itemUnitId = s.FirstOrDefault().itemUnitId,

                                  itemId = s.FirstOrDefault().itemId,
                                  unitId = s.FirstOrDefault().unitId,
                                  branchName = s.FirstOrDefault().branchName,
                                  branchId = s.FirstOrDefault().branchId,
                                  quantity = s.Sum(q => q.quantity),

                              }).OrderByDescending(x => x.quantity).ToList();
                        // merge two list
                        foreach (IUStorage finalrow in IUnitlist)
                        {
                            IUStorage temp = new IUStorage();
                            temp = invListIU.Where(x => (x.branchId == finalrow.branchId && x.itemUnitId == finalrow.itemUnitId)).FirstOrDefault();
                            if (temp != null)
                            {
                                finalrow.quantity = temp.quantity;
                            }

                        }

                        result += "ListIUStorage:";
                        jsonStr = JsonConvert.SerializeObject(IUnitlist);
                        result += jsonStr + ',';
                        #endregion

                        #region  AmountMonthlySalPur

                        totalinMonth = new List<TotalPurSale>();
                        totalinday = new List<TotalPurSale>();
                        List<TotalPurSale> Monthlylist = new List<TotalPurSale>();
                        totalAllBranchRow = new TotalPurSale();
                        totalRowtemp = new TotalPurSale();


                        for (int i = 1; i <= days; i++)
                        {
                            DateTime daydate = new DateTime(year, month, i);
                            totalinday = new List<TotalPurSale>();

                            totalinday = GetTotalPurSaleday(daydate, i, mainBranchId, userId);
                            totalinMonth.AddRange(totalinday);
                        }

                        for (int i = 1; i <= days; i++)
                        {
                            foreach (branches row in brlist)
                            {
                                totalAllBranchRow = new TotalPurSale();
                                totalAllBranchRow.branchCreatorId = row.branchId;
                                totalAllBranchRow.branchCreatorName = row.name;
                                totalAllBranchRow.day = i;
                                totalAllBranchRow.totalPur = 0;
                                totalAllBranchRow.totalSale = 0;
                                totalAllBranchRow.countPur = 0;
                                totalAllBranchRow.countSale = 0;
                                Monthlylist.Add(totalAllBranchRow);

                            }


                        }

                        foreach (TotalPurSale rowinv in Monthlylist)
                        {
                            totalRowtemp = new TotalPurSale();
                            totalRowtemp = totalinMonth.Where(b => (b.day == rowinv.day && b.branchCreatorId == rowinv.branchCreatorId)).FirstOrDefault();
                            if (totalRowtemp != null)
                            {
                                rowinv.totalPur = totalRowtemp.totalPur;
                                rowinv.totalSale = totalRowtemp.totalSale;
                                rowinv.countPur = totalRowtemp.countPur;
                                rowinv.countSale = totalRowtemp.countSale;
                            }

                        }

                        result += "ListAllBestSeller:";
                        jsonStr = JsonConvert.SerializeObject(Monthlylist);
                        result += jsonStr + ',';
                        #endregion

                        #region GetuseronlineInfo
                        var invListOnline = (from log in entity.usersLogs
                                             join p in entity.pos on log.posId equals p.posId
                                             join u in entity.users on log.userId equals u.userId

                                             where (brIds.Contains((int)p.branchId) && (log.sOutDate == null && log.users.isOnline == 1))

                                             select new
                                             {
                                                 log.userId,
                                                 p.branchId,
                                                 branchName = p.branches.name,
                                                 branchisActive = p.branches.isActive,

                                                 log.posId,
                                                 posName = p.name,
                                                 posisActive = p.isActive,
                                                 //
                                                 usernameAccount = u.username,
                                                 userName = u.name,
                                                 lastname = u.lastname,

                                                 job = u.job,
                                                 phone = u.phone,
                                                 mobile = u.mobile,
                                                 email = u.email,
                                                 address = u.address,
                                                 userisActive = u.isActive,
                                                 isOnline = u.isOnline,

                                                 image = u.image,
                                                 updateDate = u.updateDate,
                                                 //

                                             }).ToList();

                        List<userOnlineInfo> listOnline = invListOnline.GroupBy(g => new { g.branchId, g.userId }).Select(g => new userOnlineInfo
                        {

                            branchId = g.FirstOrDefault().branchId,
                            branchName = g.LastOrDefault().branchName,
                            branchisActive = g.LastOrDefault().branchisActive,

                            posId = g.LastOrDefault().posId,
                            posName = g.LastOrDefault().posName,
                            posisActive = g.LastOrDefault().posisActive,

                            userId = g.LastOrDefault().userId,
                            usernameAccount = g.LastOrDefault().usernameAccount,
                            userName = g.LastOrDefault().userName,
                            lastname = g.LastOrDefault().lastname,
                            job = g.LastOrDefault().job,
                            phone = g.LastOrDefault().phone,
                            mobile = g.LastOrDefault().mobile,
                            email = g.LastOrDefault().email,
                            address = g.LastOrDefault().address,
                            userisActive = g.LastOrDefault().userisActive,
                            isOnline = g.LastOrDefault().isOnline,
                            image = g.LastOrDefault().image,
                            updateDate = g.LastOrDefault().updateDate,

                        }).ToList();

                        result += "listUserOnline:";
                        jsonStr = JsonConvert.SerializeObject(listOnline);
                        result += jsonStr + ',';

                        #endregion

                        #region GetbranchBalance
                        List<PosModel> branchBalance = new List<PosModel>();
                        branchBalance = GetbranchBalance(brIds);
                        result += "branchBalance:";
                        jsonStr = JsonConvert.SerializeObject(branchBalance);
                        result += jsonStr + ',';

                        #endregion
                        #region GetPosOnlineInfo
                        List<PosModel> PosOnlineInfo = new List<PosModel>();
                        PosOnlineInfo = GetPosOnlineInfo(brIds);
                        result += "PosOnlineInfo:";
                        jsonStr = JsonConvert.SerializeObject(PosOnlineInfo);
                        result += jsonStr + ',';

                        #endregion
                        #region GetPosOnlineCount
                        List<PosOnlineCount> posOnlineCount = new List<PosOnlineCount>();
                        posOnlineCount = GetPosOnlineCount(brIds);
                        result += "posOnlineCount:";
                        jsonStr = JsonConvert.SerializeObject(posOnlineCount);
                        result += jsonStr + ',';
                        #endregion
                        #region GetpaymentsToday
                        List<CardsSts> paymentsToday = new List<CardsSts>();
                        paymentsToday = GetpaymentsToday(brIds);
                        result += "paymentsToday:";
                        jsonStr = JsonConvert.SerializeObject(paymentsToday);
                        result += jsonStr + ',';

                        #endregion


                        result += "}";
                        return TokenManager.GenerateToken(result);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        [HttpPost]
        [Route("GetMainNotification")]
        public string GetMainNotification(string token)
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
                int posId = 0;
                string alertType = "";
                string cashTransferType = "";
                string cashSide = "";
                string status = "";
                string invType = "";


                string result = "{";


                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "alertType")
                    {
                        alertType = c.Value;
                    }
                    else if (c.Type == "cashTransferType")
                    {
                        cashTransferType = c.Value;
                    }
                    else if (c.Type == "cashSide")
                    {
                        cashSide = c.Value;
                    }
                    else if (c.Type == "status")
                    {
                        status = c.Value;
                    }
                    else if (c.Type == "invType")
                    {
                        invType = c.Value;
                    }

                }
                try
                {
                    notificationUserController nc = new notificationUserController();
                    int notCount = nc.GetNotUserCount(posId, userId, alertType);
                    result += "UserNotCount:" + notCount;

                    messagesPosController mc = new messagesPosController();
                    int messageCount = mc.GetNotReadCountByUserId(userId, posId);
                    result += ",UserMessageCount:" + messageCount;

                    InvoicesController ic = new InvoicesController();
                    int ordersCount = ic.getDeliverOrdersCount(invType, status, userId);
                    result += ",UseraitingOrderCount:" + ordersCount;


                    GroupObjectController gc = new GroupObjectController();
                    CashTransferController cc = new CashTransferController();
                    int cashesQuery =0;

                    if (gc.HasPermissionAction("setUserSetting_administrativePosTransfers", "one", userId))
                        cashesQuery = cc.GetCountTransferForPosByUserId(userId, cashSide, cashTransferType);
                    else
                        cashesQuery = cc.GetCountNotConfirmdByPosId(posId , cashSide, cashTransferType);

                    int posCachTransfers = cashesQuery;
                    result += ",CashTransferCount:" + posCachTransfers;


                    PosController pc = new PosController();
                    PosModel pos = pc.GetPosByID(posId);
                    result += ",PosBalance:" + pos.balance;
                    result += ",BoxState:'" + pos.boxState + "'";


                    result += "}";
                    // return result;
                    return TokenManager.GenerateToken(result);

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        // for Dashboard
        //  
        [HttpPost]
        [Route("Getdashsalpur")]
        public string Getdashsalpur(string token)
        {
            // public string Get(string token)

            // public ResponseVM GetPurinv(string token)

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                try
                {
                    StatisticsController sts = new StatisticsController();
                    List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var invListm = (from I in entity.invoices

                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC

                                        //pbw pb  sb
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId) && (I.invType == "p" || I.invType == "pw" || I.invType == "s" || I.invType == "pbw" || I.invType == "pb" || I.invType == "sb"))

                                        select new
                                        {
                                            I.invoiceId,
                                            // I.invNumber,
                                            //  I.agentId,
                                            //  I.posId,
                                            I.invType,
                                            //  I.total,
                                            //I.totalNet,


                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,

                                        }).ToList();
                        //   var group2invlist = invListm.GroupBy(g => new { g.invType, g.branchCreatorId }).Select(g => new

                        var list = invListm.GroupBy(g => g.branchCreatorId).Select(g => new
                        {
                            invType = g.FirstOrDefault().invType,
                            branchCreatorId = g.FirstOrDefault().branchCreatorId,
                            branchCreatorName = g.FirstOrDefault().branchCreatorName,
                            purCount = g.Where(i => (i.invType == "p" || i.invType == "pw")).Count(),
                            saleCount = g.Where(i => i.invType == "s").Count(),
                            purBackCount = g.Where(i => (i.invType == "pbw" || i.invType == "pb")).Count(),
                            saleBackCount = g.Where(i => i.invType == "sb").Count(),
                        }).ToList();
                        /*
                        .GroupBy(s =>  s.branchCreatorId ).Select(s => new
                        {
                            invType = s.FirstOrDefault().invType,
                            branchCreatorId = s.FirstOrDefault().branchCreatorId,
                            branchCreatorName = s.FirstOrDefault().branchCreatorName,
                            purCount = s.Where(i => (i.invType == "p" || i.invType == "pw")).Count(),

                            saleCount = s.Where(i => i.invType == "s").Count(),
                        }
                            ).ToList();
                            */

                        /*
                           var result = temp.GroupBy(s => new { s.updateUserId, s.cUserAccName }).Select(s => new
            {
                updateUserId = s.FirstOrDefault().updateUserId,
                cUserAccName = s.FirstOrDefault().cUserAccName,
                count = s.Count()
            });
                         * */
                        return TokenManager.GenerateToken(list);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        [HttpPost]
        [Route("GetAgentCount")]
        public string GetAgentCount(string token)
        {
            // public string Get(string token)

            // public ResponseVM GetPurinv(string token)




            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                //int mainBranchId = 0;
                //int userId = 0;

                //IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                //foreach (Claim c in claims)
                //{
                //    if (c.Type == "mainBranchId")
                //    {
                //        mainBranchId = int.Parse(c.Value);
                //    }
                //    else if (c.Type == "userId")
                //    {
                //        userId = int.Parse(c.Value);
                //    }

                //}
                try
                {
                    //StatisticsController sts = new StatisticsController();
                    //List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var invListm = (from A in entity.agents
                                        select new
                                        {
                                            //  A.agentId,                                     
                                            A.type,
                                        }).ToList();


                        //   var group2invlist = invListm.GroupBy(g => new { g.invType, g.branchCreatorId }).Select(g => new

                        var list = invListm.GroupBy(g => g.type).Select(g => new
                        {
                            type = g.FirstOrDefault().type,

                            vendorCount = g.Where(i => i.type == "v").Count(),
                            customerCount = g.Where(i => i.type == "c").Count(),
                            grp = 1,
                        }).ToList().GroupBy(g => g.grp).Select(c => new
                        {

                            vendorCount = c.Sum(d => d.vendorCount),
                            customerCount = c.Sum(d => d.customerCount),
                        }).ToList();

                        //g.FirstOrDefault().type=="v"

                        return TokenManager.GenerateToken(list);

                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }



            }



        }


        //عدد المستخدمين المتصلين والغير متصلين  حاليا في كل فرع 
        [HttpPost]
        [Route("Getuseronline")]
        public string Getuseronline(string token)
        {
            // public string Get(string token)

            // public ResponseVM GetPurinv(string token)




            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                try
                {
                    StatisticsController sts = new StatisticsController();
                    List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        //  int allUsers = entity.users.ToList().Count();
                        /*
                        // get user count in every branch
                        var listUsersinbranch = entity.branchesUsers.Select(s => new
                        {
                            s.branchId,
                            s.userId
                        }).ToList();

                        var listub = listUsersinbranch.GroupBy(g => g.branchId).Select(g => new
                        {
                            usersAll = g.Count(),
                            g.FirstOrDefault().branchId,

                        }).ToList();
                        */

                        var listPosinbranch = entity.pos.Select(s => new
                        {
                            s.branchId,
                            s.posId,
                            s.isActive,
                            s.branches.name,
                        }).ToList();
                        // get Active Pos count in every branch
                        var listposb = listPosinbranch.Where(x => x.isActive == 1).GroupBy(g => g.branchId).Select(g => new
                        {
                            posAll = g.Count(),
                            g.FirstOrDefault().branchId,
                            g.FirstOrDefault().name,

                        }).ToList();
                        List<UserOnlineCount> list = new List<UserOnlineCount>();

                        foreach (var row in listposb)
                        {
                            UserOnlineCount newrow = new UserOnlineCount();
                            newrow.allPos = row.posAll;
                            newrow.branchId = (int)row.branchId;
                            newrow.branchName = row.name;
                            newrow.offlineUsers = row.posAll;
                            newrow.userOnlineCount = 0;

                            list.Add(newrow);

                        }

                        var invListm = (from log in entity.usersLogs
                                        join p in entity.pos on log.posId equals p.posId
                                        join u in entity.users on log.userId equals u.userId

                                        where (log.sOutDate == null && log.users.isOnline == 1)

                                        select new
                                        {
                                            log.userId,
                                            p.branchId,
                                            branchName = p.branches.name,
                                            branchisActive = p.branches.isActive,

                                            log.posId,
                                            posName = p.name,
                                            posisActive = p.isActive,
                                            //
                                            //usernameAccount = u.username,
                                            //userName = u.name,
                                            //lastname = u.lastname,

                                            //job = u.job,
                                            //phone = u.phone,
                                            //mobile = u.mobile,
                                            //email = u.email,
                                            //address = u.address,
                                            //userisActive = u.isActive,
                                            //isOnline = u.isOnline,

                                            //image = u.image,

                                            //

                                        }).ToList();


                        //   var group2invlist = invListm.GroupBy(g => new { g.invType, g.branchCreatorId }).Select(g => new

                        List<userOnlineInfo> grouplist = invListm.GroupBy(g => new { g.branchId, g.userId }).Select(g => new userOnlineInfo
                        {


                            branchId = g.LastOrDefault().branchId,
                            branchName = g.LastOrDefault().branchName,
                            branchisActive = g.LastOrDefault().branchisActive,

                            posId = g.LastOrDefault().posId,
                            posName = g.LastOrDefault().posName,
                            posisActive = g.LastOrDefault().posisActive,

                            userId = g.LastOrDefault().userId,

                            //usernameAccount = g.LastOrDefault().usernameAccount,
                            //userName = g.LastOrDefault().userName,
                            //lastname = g.LastOrDefault().lastname,
                            //job = g.LastOrDefault().job,
                            //phone = g.LastOrDefault().phone,
                            //mobile = g.LastOrDefault().mobile,
                            //email = g.LastOrDefault().email,
                            //address = g.LastOrDefault().address,
                            //userisActive = g.LastOrDefault().userisActive,
                            //isOnline = g.LastOrDefault().isOnline,
                            //image = g.LastOrDefault().image,



                        }).ToList();

                        List<UserOnlineCount> grop = grouplist.GroupBy(g => g.branchId).Select(g => new UserOnlineCount
                        {
                            userOnlineCount = g.Count(),
                            allPos = listposb.Where(b => b.branchId == g.FirstOrDefault().branchId).FirstOrDefault().posAll,
                            offlineUsers = listposb.Where(b => b.branchId == g.FirstOrDefault().branchId).FirstOrDefault().posAll - g.Count(),//offline= all -online
                            branchId = (int)g.FirstOrDefault().branchId,
                            branchName = g.FirstOrDefault().branchName,
                            //  userOnlinelist = grouplist.Where(b => b.branchId == g.FirstOrDefault().branchId).ToList(),
                            //   userOnlinelist = grouplist.ToList(),
                        }).ToList();
                        list = list.Where(X => brIds.Contains(X.branchId)).ToList();
                        foreach (UserOnlineCount finalrow in list)
                        {
                            UserOnlineCount temp = new UserOnlineCount();
                            temp = grop.Where(x => x.branchId == finalrow.branchId).FirstOrDefault();
                            if (temp != null)
                            {
                                finalrow.offlineUsers = temp.offlineUsers;
                                finalrow.userOnlineCount = temp.userOnlineCount;

                            }
                        }



                        return TokenManager.GenerateToken(list);

                    }


                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }




            }

        }


        [HttpPost]
        [Route("GetuseronlineInfo")]
        public string GetuseronlineInfo(string token)
        {
            // public string Get(string token)

            // public ResponseVM GetPurinv(string token)




            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                try
                {
                    StatisticsController sts = new StatisticsController();
                    List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var invListm = (from log in entity.usersLogs
                                        join p in entity.pos on log.posId equals p.posId
                                        join u in entity.users on log.userId equals u.userId

                                        where (brIds.Contains((int)p.branchId) && (log.sOutDate == null && log.users.isOnline == 1))

                                        select new
                                        {
                                            log.userId,
                                            p.branchId,
                                            branchName = p.branches.name,
                                            branchisActive = p.branches.isActive,

                                            log.posId,
                                            posName = p.name,
                                            posisActive = p.isActive,
                                            //
                                            usernameAccount = u.username,
                                            userName = u.name,
                                            lastname = u.lastname,

                                            job = u.job,
                                            phone = u.phone,
                                            mobile = u.mobile,
                                            email = u.email,
                                            address = u.address,
                                            userisActive = u.isActive,
                                            isOnline = u.isOnline,

                                            image = u.image,

                                            //

                                        }).ToList();

                        List<userOnlineInfo> list = invListm.GroupBy(g => new { g.branchId, g.userId }).Select(g => new userOnlineInfo
                        {

                            branchId = g.FirstOrDefault().branchId,
                            branchName = g.LastOrDefault().branchName,
                            branchisActive = g.LastOrDefault().branchisActive,

                            posId = g.LastOrDefault().posId,
                            posName = g.LastOrDefault().posName,
                            posisActive = g.LastOrDefault().posisActive,

                            userId = g.LastOrDefault().userId,
                            usernameAccount = g.LastOrDefault().usernameAccount,
                            userName = g.LastOrDefault().userName,
                            lastname = g.LastOrDefault().lastname,
                            job = g.LastOrDefault().job,
                            phone = g.LastOrDefault().phone,
                            mobile = g.LastOrDefault().mobile,
                            email = g.LastOrDefault().email,
                            address = g.LastOrDefault().address,
                            userisActive = g.LastOrDefault().userisActive,
                            isOnline = g.LastOrDefault().isOnline,
                            image = g.LastOrDefault().image,
                        }).ToList();

                        return TokenManager.GenerateToken(list);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }



            }


        }


        [HttpPost]
        [Route("GetBrachonline")]
        public string GetBrachonline(string token)
        {

            // public string Get(string token)

            // public ResponseVM GetPurinv(string token)




            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                try
                {
                    List<BranchOnlineCount> list = new List<BranchOnlineCount>();
                    StatisticsController sts = new StatisticsController();
                    List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        // int allBranches = entity.branches.ToList().Count();
                        var allBranchesList = entity.branches.ToList();
                        int allBranches = allBranchesList
                            .Select(b => new
                            {
                                b.branchId,
                                b.isActive,
                            }).Where(b => (b.branchId != 1 && b.isActive == 1)).ToList().Count();
                        var invListm = (from log in entity.usersLogs
                                        join p in entity.pos on log.posId equals p.posId
                                        where (brIds.Contains((int)p.branchId) && (log.sOutDate == null && log.users.isOnline == 1))

                                        select new
                                        {
                                            log.userId,
                                            p.branchId,
                                            branchName = p.branches.name,

                                        }).ToList();


                        var grouplist = invListm.GroupBy(g => new { g.branchId, g.userId }).Select(g => new
                        {
                            g.FirstOrDefault().userId,
                            g.FirstOrDefault().branchId,
                            g.FirstOrDefault().branchName,

                        }).ToList();
                        List<UserOnlineCount> grop = grouplist.GroupBy(g => g.branchId).Select(g => new UserOnlineCount
                        {

                            branchId = (int)g.FirstOrDefault().branchId,
                            branchName = g.FirstOrDefault().branchName,
                        }).ToList();
                        BranchOnlineCount brow = new BranchOnlineCount();
                        brow.branchAll = allBranches;
                        brow.branchOnline = grop.Count();
                        //brow.branchOffline = allBranches - grop.Count();
                        brow.branchOffline = brIds.Count() - grop.Count();
                        list.Add(brow);

                        return TokenManager.GenerateToken(list);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }


            }


        }

        //عدد الفواتير في اليوم الحالي 
        [HttpPost]
        [Route("GetdashsalpurDay")]
        public string GetdashsalpurDay(string token)
        {
            // public string Get(string token)

            // public ResponseVM GetPurinv(string token)

            int mainBranchId = 0;
            int userId = 0;



            token = TokenManager.readToken(HttpContext.Current.Request);
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
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }

                Calculate calc = new Calculate();
                try
                {
                    StatisticsController sts = new StatisticsController();
                    List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var invListmtmp = (from I in entity.invoices

                                           join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC

                                           //pbw pb  sb
                                           from JBCC in JBC.DefaultIfEmpty()
                                           where (brIds.Contains(JBCC.branchId) && (I.invType == "p" || I.invType == "pw" || I.invType == "s" || I.invType == "pbw" || I.invType == "pb" || I.invType == "sb"))
                                           //   && I.updateDate==DateTime.Now())
                                           select new
                                           {
                                               I.invoiceId,
                                               // I.invNumber,
                                               //  I.agentId,
                                               //  I.posId,
                                               I.invType,
                                               //  I.total,
                                               //I.totalNet,


                                               //
                                               I.branchCreatorId,
                                               branchCreatorName = JBCC.name,
                                               I.updateDate,
                                               I.invDate,
                                           }).ToList();
                        var invListm = invListmtmp.Where(X => DateTime.Compare(
                           (DateTime)calc.changeDateformat(X.invDate, "yyyy-MM-dd")
                           , (DateTime)calc.changeDateformat(DateTime.Now, "yyyy-MM-dd")) == 0).ToList();


                        var list = invListm.GroupBy(g => g.branchCreatorId).Select(g => new
                        {
                            invType = g.FirstOrDefault().invType,
                            branchCreatorId = g.FirstOrDefault().branchCreatorId,
                            branchCreatorName = g.FirstOrDefault().branchCreatorName,
                            purCount = g.Where(i => (i.invType == "p" || i.invType == "pw")).Count(),
                            saleCount = g.Where(i => i.invType == "s").Count(),
                            purBackCount = g.Where(i => (i.invType == "pbw" || i.invType == "pb")).Count(),
                            saleBackCount = g.Where(i => i.invType == "sb").Count(),
                        }).ToList();


                        return TokenManager.GenerateToken(list);

                    }


                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }



            }


        }

        //
        // فواتير المبيعات مع العناصر
        [HttpPost]
        [Route("Getbestseller")]
        public string Getbestseller(string token)
        {
            // public string Get(string token)

            // public ResponseVM GetPurinv(string token)




            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                Calculate calc = new Calculate();
                StatisticsController sts = new StatisticsController();
                List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var invListmtmp = (from IT in entity.itemsTransfer
                                           from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                           from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
                                               // join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                               //   join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                           join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                           join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                           //    join B in entity.branches on I.branchId equals B.branchId into JB
                                           join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC

                                           from JBCC in JBC.DefaultIfEmpty()
                                           where (brIds.Contains(JBCC.branchId) && (I.invType == "s"))

                                           select new
                                           {

                                               itemName = ITEM.name,
                                               unitName = UNIT.name,
                                               itemsTransId = IT.itemsTransId,
                                               itemUnitId = IT.itemUnitId,

                                               itemId = IU.itemId,
                                               unitId = IU.unitId,
                                               quantity = IT.quantity,
                                               price = IT.price,
                                               I.invoiceId,
                                               I.invType,
                                               I.updateDate,
                                               I.branchCreatorId,
                                               branchCreatorName = JBCC.name,
                                               Totalrow = (IT.price * IT.quantity),

                                               //   ITcreateDate = IT.createDate,
                                               ITupdateDate = IT.updateDate,
                                               I.invDate,

                                           }).ToList();

                        var invListm2 = invListmtmp.Where(X => DateTime.Compare(
                       (DateTime)calc.changeDateformat(X.invDate, "yyyy-MM")
                       , (DateTime)calc.changeDateformat(DateTime.Now, "yyyy-MM")) == 0).ToList();

                        var list = invListm2.GroupBy(g => new { g.branchCreatorId, g.itemUnitId })
                            .Select(g => new
                            {
                                itemName = g.FirstOrDefault().itemName,
                                unitName = g.FirstOrDefault().unitName,
                                // itemsTransId = IT.itemsTransId,
                                itemUnitId = g.FirstOrDefault().itemUnitId,

                                itemId = g.FirstOrDefault().itemId,
                                unitId = g.FirstOrDefault().unitId,
                                quantity = g.Sum(s => s.quantity),

                                price = g.FirstOrDefault().price,
                                //  I.invoiceId,
                                //  I.invType,
                                //    I.updateDate,
                                branchCreatorId = g.FirstOrDefault().branchCreatorId,
                                branchCreatorName = g.FirstOrDefault().branchCreatorName,
                                subTotal = g.Sum(s => s.Totalrow),

                            }).OrderByDescending(o => o.quantity).ToList().Take(10);

                        //.Take(3)



                        return TokenManager.GenerateToken(list);

                    }


                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }



            }



        }

        // كمية العناصر في الفروع

        //  [HttpPost]
        [HttpPost]
        [Route("GetIUStorage")]
        public string GetIUStorage(string token)
        {
            // public ResponseVM GetPurinv(string token)string IUList

            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string IUList = "";
                int mainBranchId = 0;
                int userId = 0;
                List<itemsUnits> newiuObj = new List<itemsUnits>();

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "IUList")
                    {
                        IUList = c.Value.Replace("\\", string.Empty);
                        IUList = IUList.Trim('"');
                        newiuObj = JsonConvert.DeserializeObject<List<itemsUnits>>(IUList, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }

                    else if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }




                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                    StatisticsController sts = new StatisticsController();
                    List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
                    List<int> iuIds = new List<int>();
                    List<IUStorage> list = new List<IUStorage>();
                    List<branches> brlist = new List<branches>();
                    using (incposdbEntities entity1 = new incposdbEntities())
                    {
                        // get branches
                        brlist = entity1.branches.ToList();
                        brlist = brlist.Where(x => (x.branchId != 1 && x.isActive == 1)).Select(b => new branches
                        {
                            branchId = b.branchId,
                            name = b.name,
                            isActive = b.isActive,
                        }).ToList();
                        // prepare new list with all branches and all iu.

                        foreach (itemsUnits row in newiuObj)
                        {

                            foreach (branches branchRow in brlist)
                            {
                                IUStorage newrow = new IUStorage();
                                newrow.itemUnitId = row.itemUnitId;
                                newrow.quantity = 0;
                                newrow.branchId = branchRow.branchId;
                                newrow.branchName = branchRow.name;
                                newrow.itemId = entity1.itemsUnits.Find(row.itemUnitId).itemId;
                                newrow.unitId = entity1.itemsUnits.Find(row.itemUnitId).unitId; ;
                                newrow.itemName = entity1.itemsUnits.Find(row.itemUnitId).items.name;
                                newrow.unitName = entity1.itemsUnits.Find(row.itemUnitId).units.name;

                                list.Add(newrow);


                            }

                            //newrow. = 0;
                            iuIds.Add(row.itemUnitId);

                        }

                    }



                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        // storageCost storageCostsr = new storageCost();

                        var invListmtemp = (from L in entity.locations
                                                //  from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)


                                            join IUL in entity.itemsLocations on L.locationId equals IUL.locationId
                                            join IU in entity.itemsUnits on IUL.itemUnitId equals IU.itemUnitId

                                            //  join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                            //join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                            join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                            join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                            //   join S in entity.sections on L.sectionId equals S.sectionId into JS
                                            join B in entity.branches on L.branchId equals B.branchId into JB

                                            // join UPUSR in entity.users on IUL.updateUserId equals UPUSR.userId into JUPUS
                                            //  join U in entity.users on IUL.createUserId equals U.userId into JU

                                            from JBB in JB
                                            where (brIds.Contains(JBB.branchId) && iuIds.Contains(IU.itemUnitId))
                                            //  from JSS in JS.DefaultIfEmpty()
                                            // from JUU in JU.DefaultIfEmpty()
                                            // from JUPUSS in JUPUS.DefaultIfEmpty()
                                            select new
                                            {
                                                // item unit
                                                itemName = ITEM.name,
                                                unitName = UNIT.name,
                                                IU.itemUnitId,

                                                IU.itemId,
                                                IU.unitId,
                                                branchName = JBB.name,

                                                branchType = JBB.type,
                                                IUL.itemsLocId,
                                                IUL.locationId,
                                                IUL.quantity,
                                                L.branchId,



                                            }).ToList();

                        List<IUStorage> invListm = invListmtemp.GroupBy(g => new { g.branchId, g.itemUnitId })
                              .Select(s => new IUStorage
                              {
                                  itemName = s.FirstOrDefault().itemName,
                                  unitName = s.FirstOrDefault().unitName,
                                  itemUnitId = s.FirstOrDefault().itemUnitId,

                                  itemId = s.FirstOrDefault().itemId,
                                  unitId = s.FirstOrDefault().unitId,
                                  branchName = s.FirstOrDefault().branchName,
                                  branchId = s.FirstOrDefault().branchId,
                                  quantity = s.Sum(q => q.quantity),

                              }).OrderByDescending(x => x.quantity).ToList();
                        // merge two list
                        foreach (IUStorage finalrow in list)
                        {
                            IUStorage temp = new IUStorage();
                            temp = invListm.Where(x => (x.branchId == finalrow.branchId && x.itemUnitId == finalrow.itemUnitId)).FirstOrDefault();
                            if (temp != null)
                            {
                                finalrow.quantity = temp.quantity;
                            }

                        }

                        return TokenManager.GenerateToken(list.OrderByDescending(x => x.quantity));
                        //  return new ResponseVM { Status = "Success", Message = TokenManager.GenerateToken(list.OrderByDescending(x => x.quantity)) };


                    }


                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }



        }

        // مجموع مبالغ المشتريات والمبيعات اليومي خلال الشهر الحالي لكل فرع
        [HttpPost]
        [Route("GetTotalPurSale")]
        public string GetTotalPurSale(string token)
        {
            // public string Get(string token)

            // public ResponseVM GetPurinv(string token)




            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                Calculate calc = new Calculate();
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                try
                {
                    int year;
                    int month;
                    int days;
                    //StatisticsController sts = new StatisticsController();
                    //List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
                    List<branches> brlist = new List<branches>();
                    List<TotalPurSale> totalinMonth = new List<TotalPurSale>();
                    List<TotalPurSale> totalinday = new List<TotalPurSale>();
                    List<TotalPurSale> list = new List<TotalPurSale>();
                    TotalPurSale totalAllBranchRow = new TotalPurSale();
                    TotalPurSale totalRowtemp = new TotalPurSale();
                    DateTime currentdate = cc.AddOffsetTodate(DateTime.Now);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        brlist = entity.branches.ToList();
                        brlist = brlist.Where(x => (x.branchId != 1 && x.isActive == 1)).Select(b => new branches
                        {
                            branchId = b.branchId,
                            name = b.name,
                            isActive = b.isActive,
                        }).ToList();

                    }

                    year = currentdate.Year;
                    month = currentdate.Month;
                    days = calc.getdays(year, month);
                    for (int i = 1; i <= days; i++)
                    {
                        DateTime daydate = new DateTime(year, month, i);
                        totalinday = new List<TotalPurSale>();

                        totalinday = GetTotalPurSaleday(daydate, i, mainBranchId, userId);
                        totalinMonth.AddRange(totalinday);
                    }

                    for (int i = 1; i <= days; i++)
                    {
                        foreach (branches row in brlist)
                        {
                            totalAllBranchRow = new TotalPurSale();
                            totalAllBranchRow.branchCreatorId = row.branchId;
                            totalAllBranchRow.branchCreatorName = row.name;
                            totalAllBranchRow.day = i;
                            totalAllBranchRow.totalPur = 0;
                            totalAllBranchRow.totalSale = 0;
                            totalAllBranchRow.countPur = 0;
                            totalAllBranchRow.countSale = 0;
                            list.Add(totalAllBranchRow);

                        }


                    }

                    foreach (TotalPurSale rowinv in list)
                    {
                        totalRowtemp = new TotalPurSale();
                        totalRowtemp = totalinMonth.Where(b => (b.day == rowinv.day && b.branchCreatorId == rowinv.branchCreatorId)).FirstOrDefault();
                        if (totalRowtemp != null)
                        {
                            rowinv.totalPur = totalRowtemp.totalPur;
                            rowinv.totalSale = totalRowtemp.totalSale;
                            rowinv.countPur = totalRowtemp.countPur;
                            rowinv.countSale = totalRowtemp.countSale;
                        }

                    }


                    return TokenManager.GenerateToken(list);


                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }


        }

        //in day

        public List<TotalPurSale> GetTotalPurSaleday(DateTime dayDate, int number, int mainBranchId, int userId)
        {
            Calculate calc = new Calculate();
            TotalPurSale totalrow = new TotalPurSale();
            List<TotalPurSale> totalList = new List<TotalPurSale>();
            StatisticsController sts = new StatisticsController();
            List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
            using (incposdbEntities entity = new incposdbEntities())
            {
                var invListm1 = (from I in entity.invoices

                                 join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC

                                 //pbw pb  sb
                                 from JBCC in JBC.DefaultIfEmpty()
                                     // where (I.invType == "p" || I.invType == "pw" || I.invType == "s" || I.invType == "pbw" || I.invType == "pb" || I.invType == "sb")
                                 where (brIds.Contains(JBCC.branchId) && (I.invType == "p" || I.invType == "pw" || I.invType == "s") && JBCC.branchId != 1 && JBCC.isActive == 1)

                                 select new
                                 {
                                     I.invoiceId,
                                     // I.invNumber,
                                     //  I.agentId,
                                     //  I.posId,
                                     I.invType,
                                     //  I.total,
                                     I.totalNet,
                                     I.updateDate,
                                     I.invDate,
                                     //
                                     I.branchCreatorId,
                                     branchCreatorName = JBCC.name,

                                 }).ToList();
                var invListm = invListm1.Where(X => DateTime.Compare(
   (DateTime)calc.changeDateformat(X.invDate, "yyyy-MM-dd")
   , (DateTime)calc.changeDateformat(dayDate, "yyyy-MM-dd")) == 0).ToList();

                totalList = invListm.GroupBy(g => g.branchCreatorId).Select(g => new TotalPurSale
                {
                    //  invType = g.FirstOrDefault().invType,
                    branchCreatorId = g.FirstOrDefault().branchCreatorId,
                    branchCreatorName = g.FirstOrDefault().branchCreatorName,
                    totalPur = g.Where(i => (i.invType == "p" || i.invType == "pw")).Sum(s => s.totalNet),
                    totalSale = g.Where(i => i.invType == "s").Sum(s => s.totalNet),
                    countPur = g.Where(i => (i.invType == "p" || i.invType == "pw")).Count(),
                    countSale = g.Where(i => i.invType == "s").Count(),
                    //  purBackCount = g.Where(i => (i.invType == "pbw" || i.invType == "pb")).Count(),
                    // saleBackCount = g.Where(i => i.invType == "sb").Count(),
                    day = number,
                }).ToList();



                return totalList;
            }


        }

        public List<BranchModel> GetAllbranches()
        {
            List<BranchModel> brlist = new List<BranchModel>();
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {

                    var brlist1 = entity.branches.ToList();
                    brlist = brlist1.Where(x => (x.branchId != 1 && x.isActive == 1)).Select(b => new BranchModel
                    {
                        branchId = b.branchId,
                        name = b.name,
                        isActive = b.isActive,
                        type = b.type,
                    }).ToList();

                }
                return brlist;
            }
            catch
            {
                return brlist;
            }
        }

        ////مجموع الكاش لكل فرع
        //[HttpPost]
        //[Route("GetbranchBalance")]
        //public string GetbranchBalance(string token)
        //{
        //    token = TokenManager.readToken(HttpContext.Current.Request);

        //    var strP = TokenManager.GetPrincipal(token);
        //    if (strP != "0") //invalid authorization
        //    {
        //        return TokenManager.GenerateToken(strP);

        //    }
        //    else
        //    {
        //        int mainBranchId = 0;
        //        int userId = 0;
        //        IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
        //        foreach (Claim c in claims)
        //        {
        //            if (c.Type == "mainBranchId")
        //            {
        //                mainBranchId = int.Parse(c.Value);
        //            }
        //            else if (c.Type == "userId")
        //            {
        //                userId = int.Parse(c.Value);
        //            }

        //        }
        //        StatisticsController sts = new StatisticsController();
        //        List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
        //        using (incposdbEntities entity = new incposdbEntities())
        //        {
        //            var posList = (from p in entity.pos
        //                           join b in entity.branches on p.branchId equals b.branchId  

        //                           where brIds.Contains((int)p.branchId) && b.branchId!=1
        //                           select new PosModel()
        //                           {
        //                               posId = p.posId,
        //                               balance = p.balance != null ? p.balance : 0,
        //                               branchId = p.branchId,
        //                               //code = p.code,
        //                               //name = p.name,
        //                               branchName = b.name,

        //                               isActive = p.isActive,
        //                               balanceAll = p.balanceAll,
        //                               //note = p.note,
        //                               branchCode = b.code,
        //                               //boxState = p.boxState,
        //                               //isAdminClose = p.isAdminClose,
        //                           }).ToList();



        //            List<PosModel> branchlist = posList.GroupBy(b => b.branchId).Select(b => new PosModel
        //            {
        //                balance = b.Sum(p=>p.balance),
        //                branchId = b.FirstOrDefault().branchId,
        //                //code = p.code,
        //                //name = p.name,
        //                branchName = b.FirstOrDefault().branchName,
        //                branchCode = b.FirstOrDefault().branchCode,
        //            }).ToList();
        //            return TokenManager.GenerateToken(branchlist);
        //        }
        //    }

        //}

        //مجموع الكاش لكل فرع

        public List<PosModel> GetbranchBalance(List<int> brIds)
        {
            List<PosModel> branchlist = new List<PosModel>();
            try
            {
                //StatisticsController sts = new StatisticsController();

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var posList = (from p in entity.pos
                                   join b in entity.branches on p.branchId equals b.branchId

                                   where brIds.Contains((int)p.branchId) && b.branchId != 1
                                   select new PosModel()
                                   {
                                       posId = p.posId,
                                       balance = p.balance != null ? p.balance : 0,
                                       branchId = p.branchId,
                                       //code = p.code,
                                       //name = p.name,
                                       branchName = b.name,

                                       isActive = p.isActive,
                                       balanceAll = p.balanceAll,
                                       //note = p.note,
                                       branchCode = b.code,
                                       //boxState = p.boxState,
                                       //isAdminClose = p.isAdminClose,
                                   }).ToList();



                    branchlist = posList.GroupBy(b => b.branchId).Select(b => new PosModel
                    {
                        balance = b.Sum(p => p.balance),
                        branchId = b.FirstOrDefault().branchId,
                        //code = p.code,
                        //name = p.name,
                        branchName = b.FirstOrDefault().branchName,
                        branchCode = b.FirstOrDefault().branchCode,
                    }).ToList();

                }

                return branchlist;
            }
            catch
            {
                branchlist = new List<PosModel>();
                return branchlist;
            }

        }
        ////نقط البيع المتصلة حاليا
        //[HttpPost]
        //[Route("GetPosonlineInfo")]
        //public string GetPosonlineInfo(string token)
        //{
        //    // public string Get(string token)

        //    // public ResponseVM GetPurinv(string token)




        //    token = TokenManager.readToken(HttpContext.Current.Request);
        //    var strP = TokenManager.GetPrincipal(token);
        //    if (strP != "0") //invalid authorization
        //    {
        //        return TokenManager.GenerateToken(strP);
        //    }
        //    else
        //    {
        //        int mainBranchId = 0;
        //        int userId = 0;

        //        IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
        //        foreach (Claim c in claims)
        //        {
        //            if (c.Type == "mainBranchId")
        //            {
        //                mainBranchId = int.Parse(c.Value);
        //            }
        //            else if (c.Type == "userId")
        //            {
        //                userId = int.Parse(c.Value);
        //            }

        //        }
        //        try
        //        {
        //            StatisticsController sts = new StatisticsController();
        //            List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
        //            using (incposdbEntities entity = new incposdbEntities())
        //            {


        //                var invListm = (from log in entity.usersLogs
        //                                join p in entity.pos on log.posId equals p.posId
        //                               // join u in entity.users on log.userId equals u.userId

        //                                where (brIds.Contains((int)p.branchId) && (log.sOutDate == null ) && p.isActive==1 && p.branchId!=1)

        //                                select new
        //                                {
        //                                    log.userId,
        //                                    p.branchId,
        //                                    branchName = p.branches.name,
        //                                    branchisActive = p.branches.isActive,

        //                                    p.posId,
        //                                    posName = p.name,
        //                                    isActive = p.isActive,
        //                                    //
        //                                    //usernameAccount = u.username,
        //                                    //userName = u.name,
        //                                    //lastname = u.lastname,

        //                                    //job = u.job,
        //                                    //phone = u.phone,
        //                                    //mobile = u.mobile,
        //                                    //email = u.email,
        //                                    //address = u.address,
        //                                    //userisActive = u.isActive,
        //                                    //isOnline = u.isOnline,

        //                                    //image = u.image,

        //                                    //

        //                                }).ToList();
        //               invListm = invListm.Where(p => p.branchisActive == 1).ToList();
        //                List<PosModel> list = invListm.GroupBy(g => new { g.posId }).Select(g => new PosModel
        //                {

        //                    branchId = g.FirstOrDefault().branchId,
        //                    branchName = g.LastOrDefault().branchName,
        //                    //branchisActive = g.LastOrDefault().branchisActive,

        //                    posId = g.LastOrDefault().posId,
        //                    name = g.LastOrDefault().posName,
        //                    isActive = g.LastOrDefault().isActive,

        //                    //userId = g.LastOrDefault().userId,
        //                    //usernameAccount = g.LastOrDefault().usernameAccount,
        //                    //userName = g.LastOrDefault().userName,
        //                    //lastname = g.LastOrDefault().lastname,
        //                    //job = g.LastOrDefault().job,
        //                    //phone = g.LastOrDefault().phone,
        //                    //mobile = g.LastOrDefault().mobile,
        //                    //email = g.LastOrDefault().email,
        //                    //address = g.LastOrDefault().address,
        //                    //userisActive = g.LastOrDefault().userisActive,
        //                    //isOnline = g.LastOrDefault().isOnline,
        //                    //image = g.LastOrDefault().image,
        //                }).ToList();

        //                return TokenManager.GenerateToken(list);

        //            }

        //        }
        //        catch
        //        {
        //            return TokenManager.GenerateToken("0");
        //        }



        //    }


        //}

        //نقط البيع المتصلة حاليا

        public List<PosModel> GetPosOnlineInfo(List<int> brIds)
        {


            List<PosModel> list = new List<PosModel>();


            try
            {
                //StatisticsController sts = new StatisticsController();
                //List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
                using (incposdbEntities entity = new incposdbEntities())
                {


                    var invListm = (from log in entity.usersLogs
                                    join p in entity.pos on log.posId equals p.posId
                                    // join u in entity.users on log.userId equals u.userId

                                    where (brIds.Contains((int)p.branchId) && (log.sOutDate == null) && p.isActive == 1 && p.branchId != 1)

                                    select new
                                    {
                                        log.userId,
                                        p.branchId,
                                        branchName = p.branches.name,
                                        branchisActive = p.branches.isActive,

                                        p.posId,
                                        posName = p.name,
                                        isActive = p.isActive,


                                    }).ToList();
                    invListm = invListm.Where(p => p.branchisActive == 1).ToList();
                    list = invListm.GroupBy(g => new { g.posId }).Select(g => new PosModel
                    {

                        branchId = g.FirstOrDefault().branchId,
                        branchName = g.LastOrDefault().branchName,
                        //branchisActive = g.LastOrDefault().branchisActive,

                        posId = g.LastOrDefault().posId,
                        name = g.LastOrDefault().posName,
                        isActive = g.LastOrDefault().isActive,

                    }).ToList();

                    return list;

                }

            }
            catch
            {
                list = new List<PosModel>();
                return list;
            }
        }

        //نقط البيع المتصلة حاليا لكل فرع عدد 
        //[HttpPost]
        //[Route("GetPosonlineCount")]
        //public string GetPosonlineCount(string token)
        //{
        //    // public string Get(string token)

        //    // public ResponseVM GetPurinv(string token)




        //    token = TokenManager.readToken(HttpContext.Current.Request);
        //    var strP = TokenManager.GetPrincipal(token);
        //    if (strP != "0") //invalid authorization
        //    {
        //        return TokenManager.GenerateToken(strP);
        //    }
        //    else
        //    {
        //        int mainBranchId = 0;
        //        int userId = 0;

        //        IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
        //        foreach (Claim c in claims)
        //        {
        //            if (c.Type == "mainBranchId")
        //            {
        //                mainBranchId = int.Parse(c.Value);
        //            }
        //            else if (c.Type == "userId")
        //            {
        //                userId = int.Parse(c.Value);
        //            }

        //        }
        //        try
        //        {
        //            StatisticsController sts = new StatisticsController();
        //            List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
        //            using (incposdbEntities entity = new incposdbEntities())
        //            {


        //                var invListm = (from log in entity.usersLogs
        //                                join p in entity.pos on log.posId equals p.posId
        //                                // join u in entity.users on log.userId equals u.userId

        //                                where (brIds.Contains((int)p.branchId) && (log.sOutDate == null) && p.isActive==1 && p.branchId!=1)

        //                                select new
        //                                {
        //                                    log.userId,
        //                                    p.branchId,
        //                                    branchName = p.branches.name,
        //                                    branchisActive = p.branches.isActive,

        //                                    p.posId,
        //                                    posName = p.name,
        //                                    isActive = p.isActive,


        //                                    //

        //                                }).ToList();
        //                invListm = invListm.Where(p => p.branchisActive == 1).ToList();
        //                List<PosModel> list = invListm.GroupBy(g => new { g.posId }).Select(g => new PosModel
        //                {

        //                    branchId = g.FirstOrDefault().branchId,
        //                    branchName = g.LastOrDefault().branchName,
        //                    //branchisActive = g.LastOrDefault().branchisActive,

        //                    posId = g.LastOrDefault().posId,
        //                    name = g.LastOrDefault().posName,
        //                    isActive = g.LastOrDefault().isActive,


        //                }).ToList();

        //                List<PosOnlineCount> onlinecountList= list.GroupBy(g => new { g.branchId }).Select(g => new PosOnlineCount
        //                {

        //                    branchId =(int) g.FirstOrDefault().branchId,
        //                    branchName = g.LastOrDefault().branchName,
        //                    //branchisActive = g.LastOrDefault().branchisActive,
        //                    posOnlineCount=g.Count(),


        //                }).ToList();


        //                var allbranch = (from b in entity.branches
        //                                join p in entity.pos on b.branchId equals p.branchId
        //                                 // join u in entity.users on log.userId equals u.userId

        //                                 where (brIds.Contains((int)p.branchId) && b.isActive == 1 && p.isActive == 1 && p.branchId != 1)

        //                                select new PosModel
        //                                {

        //                               branchId= b.branchId,
        //                                    branchName = p.branches.name,
        //                                    // branchisActive = p.branches.isActive,

        //                                    posId=  p.posId,


        //                                }).ToList();

        //                List<PosOnlineCount> allbranchinfo = allbranch.GroupBy(g => new { g.branchId }).Select(g => new PosOnlineCount
        //                {

        //                    branchId = (int)g.FirstOrDefault().branchId,
        //                    branchName = g.LastOrDefault().branchName,
        //                    //branchisActive = g.LastOrDefault().branchisActive,
        //                    allPos = g.Count(),
        //                    posOnlineCount= onlinecountList.Where(x=>x.branchId== (int)g.FirstOrDefault().branchId).FirstOrDefault().posOnlineCount,


        //                }).ToList();
        //                foreach (PosOnlineCount posrow in allbranchinfo)
        //                {
        //                    posrow.offlinePos = posrow.allPos - posrow.posOnlineCount;
        //                }
        //                return TokenManager.GenerateToken(allbranchinfo);

        //            }

        //        }
        //        catch
        //        {
        //            return TokenManager.GenerateToken("0");
        //        }



        //    }


        //}

        //نقط البيع المتصلة حاليا لكل فرع عدد 

        public List<PosOnlineCount> GetPosOnlineCount(List<int> brIds)
        {
            List<PosOnlineCount> allbranchinfo = new List<PosOnlineCount>();
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var invListm = (from log in entity.usersLogs
                                    join p in entity.pos on log.posId equals p.posId
                                    // join u in entity.users on log.userId equals u.userId

                                    where (brIds.Contains((int)p.branchId) && (log.sOutDate == null) && p.isActive == 1 && p.branchId != 1)

                                    select new
                                    {
                                        log.userId,
                                        p.branchId,
                                        branchName = p.branches.name,
                                        branchisActive = p.branches.isActive,

                                        p.posId,
                                        posName = p.name,
                                        isActive = p.isActive,


                                    }).ToList();
                    invListm = invListm.Where(p => p.branchisActive == 1).ToList();
                    List<PosModel> list = invListm.GroupBy(g => new { g.posId }).Select(g => new PosModel
                    {

                        branchId = g.FirstOrDefault().branchId,
                        branchName = g.LastOrDefault().branchName,
                        //branchisActive = g.LastOrDefault().branchisActive,

                        posId = g.LastOrDefault().posId,
                        name = g.LastOrDefault().posName,
                        isActive = g.LastOrDefault().isActive,


                    }).ToList();

                    List<PosOnlineCount> onlinecountList = list.GroupBy(g => new { g.branchId }).Select(g => new PosOnlineCount
                    {

                        branchId = (int)g.FirstOrDefault().branchId,
                        branchName = g.LastOrDefault().branchName,
                        //branchisActive = g.LastOrDefault().branchisActive,
                        posOnlineCount = g.Count(),
                    }).ToList();
                    var allbranch = (from b in entity.branches
                                     join p in entity.pos on b.branchId equals p.branchId
                                     // join u in entity.users on log.userId equals u.userId

                                     where (brIds.Contains((int)p.branchId) && b.isActive == 1 && p.isActive == 1 && p.branchId != 1)

                                     select new PosModel
                                     {

                                         branchId = b.branchId,
                                         branchName = p.branches.name,
                                         // branchisActive = p.branches.isActive,
                                         posId = p.posId,
                                     }).ToList();

                    allbranchinfo = allbranch.GroupBy(g => new { g.branchId }).Select(g => new PosOnlineCount
                    {

                        branchId = (int)g.FirstOrDefault().branchId,
                        branchName = g.LastOrDefault().branchName,
                        //branchisActive = g.LastOrDefault().branchisActive,
                        allPos = g.Count(),
                        //   posOnlineCount = onlinecountList.Where(x => x.branchId == (int)g.FirstOrDefault().branchId).FirstOrDefault().posOnlineCount,


                    }).ToList();
                    foreach (PosOnlineCount posrow in allbranchinfo)
                    {
                        var posOnlineCount = onlinecountList.Where(x => x.branchId == (int)posrow.branchId).ToList();
                        if (posOnlineCount != null)
                        {
                            if (posOnlineCount.Count() > 0)
                            {
                                posrow.posOnlineCount = posOnlineCount.FirstOrDefault().posOnlineCount;
                            }
                            else
                            {
                                posrow.posOnlineCount = 0;
                            }

                        }
                        else
                        {
                            posrow.posOnlineCount = 0;
                        }



                        posrow.offlinePos = posrow.allPos - posrow.posOnlineCount;
                    }


                }
                return allbranchinfo;

            }
            catch
            {
                allbranchinfo = new List<PosOnlineCount>();
                return allbranchinfo;
            }
        }

        // محصلة طرق الدفع بدءا من بداية اليوم الحالي لكل فرع
        //[HttpPost]
        //[Route("GetpaymentsToday")]
        //public string GetpaymentsToday(string token)
        //{
        //    token = TokenManager.readToken(HttpContext.Current.Request);
        //    var strP = TokenManager.GetPrincipal(token);
        //    if (strP != "0") //invalid authorization
        //    {
        //        return TokenManager.GenerateToken(strP);
        //    }
        //    else
        //    {
        //        int mainBranchId = 0;
        //        int userId = 0;

        //        IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
        //        foreach (Claim c in claims)
        //        {
        //            if (c.Type == "mainBranchId")
        //            {
        //                mainBranchId = int.Parse(c.Value);
        //            }
        //            else if (c.Type == "userId")
        //            {
        //                userId = int.Parse(c.Value);
        //            }


        //        }

        //        // DateTime cmpdate = DateTime.Now.AddDays(newdays);
        //        try
        //        {
        //            DateTime datenow = cc.AddOffsetTodate(DateTime.Now).Date;
        //            List<OpenClosOperatinModel> cachlist = new List<OpenClosOperatinModel>();
        //            using (incposdbEntities entity = new incposdbEntities())
        //            {
        //                List<cashTransfer> allcashlist = entity.cashTransfer.ToList();

        //                //var closlist = allcashlist.Where(X => X.posId == posId && X.transType == "c").ToList();
        //                //var openlist = allcashlist.Where(X => X.posId == posId && X.transType == "o").ToList();
        //                cashTransfer closrow = new cashTransfer();
        //                //if (openlist == null || openlist.Count() == 0)
        //                //{

        //                //    return TokenManager.GenerateToken(cachlist);
        //                //}
        //                //else
        //                //{
        //                //if (closlist == null || closlist.Count() == 0)
        //                //{
        //                //    closrow = new cashTransfer();
        //                //}
        //                //else
        //                //{
        //                //    closrow = closlist.OrderBy(X => X.updateDate).LastOrDefault();
        //                //}

        //                //cashTransfer openrow = openlist.OrderBy(X => X.updateDate).LastOrDefault();

        //                cachlist = (from C in entity.cashTransfer
        //                            join b in entity.banks on C.bankId equals b.bankId into jb
        //                            join a in entity.agents on C.agentId equals a.agentId into ja
        //                            join p in entity.pos on C.posId equals p.posId into jp
        //                            join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
        //                            join u in entity.users on C.userId equals u.userId into ju
        //                            //   join uc in entity.users on C.createUserId equals uc.userId into juc
        //                            join uu in entity.users on C.createUserId equals uu.userId into jup

        //                            join cr in entity.cards on C.cardId equals cr.cardId into jcr
        //                            join bo in entity.bondes on C.bondId equals bo.bondId into jbo
        //                            join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
        //                            join i in entity.invoices on C.invId equals i.invoiceId into ji


        //                            from jbb in jb.DefaultIfEmpty()
        //                            from jaa in ja.DefaultIfEmpty()
        //                            from jpp in jp.DefaultIfEmpty()
        //                            from juu in ju.DefaultIfEmpty()
        //                            from jpcc in jpcr.DefaultIfEmpty()
        //                                // from jucc in juc.DefaultIfEmpty()
        //                            from jupdateusr in jup.DefaultIfEmpty()

        //                            from jcrd in jcr.DefaultIfEmpty()
        //                            from jbbo in jbo.DefaultIfEmpty()
        //                            from jssh in jsh.DefaultIfEmpty()
        //                            from I in ji.DefaultIfEmpty()
        //                            where C.updateDate >= datenow
        //                            select new OpenClosOperatinModel()
        //                            {
        //                                cashTransId = C.cashTransId,
        //                                transType = C.transType,
        //                                posId = C.posId,
        //                                userId = C.userId,
        //                                agentId = C.agentId,
        //                                invId = C.invId,
        //                                transNum = C.transNum,
        //                                createDate = C.createDate,
        //                                updateDate = C.updateDate,
        //                                cash = C.cash,
        //                                updateUserId = C.updateUserId,
        //                                createUserId = C.createUserId,
        //                                notes = C.notes,
        //                                posIdCreator = C.posIdCreator,
        //                                isConfirm = C.isConfirm,
        //                                cashTransIdSource = C.cashTransIdSource,
        //                                side = C.side,

        //                                docName = C.docName,
        //                                docNum = C.docNum,
        //                                docImage = C.docImage,
        //                                bankId = C.bankId,
        //                                bankName = jbb.name,
        //                                agentName = jaa.name,
        //                                usersName = juu.name,// side =u

        //                                posName = jpp.name,
        //                                posCreatorName = jpcc.name,
        //                                processType = C.processType,
        //                                cardId = C.cardId,
        //                                bondId = C.bondId,
        //                                usersLName = juu.lastname,// side =u
        //                                                          //createUserName = jucc.name,
        //                                                          //createUserLName = jucc.lastname,
        //                                                          //createUserJob = jucc.job,
        //                                cardName = jcrd.name,
        //                                bondDeserveDate = jbbo.deserveDate,
        //                                bondIsRecieved = jbbo.isRecieved,
        //                                shippingCompanyId = C.shippingCompanyId,
        //                                shippingCompanyName = jssh.name,
        //                                branchCreatorId = jpcc.branchId,
        //                                branchCreatorname = jpcc.branches.name,
        //                                branchId = jpp.branchId,
        //                                branchName = jpp.branches.name,
        //                                branch2Id = 0,
        //                                branch2Name = "",
        //                                updateUserAcc = jupdateusr.username,
        //                                invNumber = I.invNumber,
        //                                invType = I.invType,

        //                            }).Where(C =>
        //                          (C.transType != "o" && C.transType != "c"
        //                            && C.processType != "balance" && C.processType != "box" &&
        //                            C.processType != "inv" && C.processType != "cheque" && C.processType != "doc"
        //                            && (C.side == "bn" ? C.isConfirm == 1 : true)
        //                           )
        //                                                    ).OrderBy(X => X.updateDate).ToList();


        //                BranchesController branchCntrlr = new BranchesController();
        //                StatisticsController stsc = new StatisticsController();
        //                if (cachlist.Count > 0)
        //                {
        //                    branches branchmodel = new branches();

        //                    CashTransferModel tempitem = null;
        //                    foreach (OpenClosOperatinModel cashtItem in cachlist)
        //                    {
        //                        if (cashtItem.side == "p")
        //                        {
        //                            tempitem = stsc.Getpostransmodel(cashtItem.cashTransId)
        //                                .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
        //                            cashtItem.cashTrans2Id = tempitem.cashTransId;
        //                            cashtItem.pos2Id = tempitem.posId;
        //                            cashtItem.pos2Name = tempitem.posName;
        //                            cashtItem.isConfirm2 = tempitem.isConfirm;

        //                            branchmodel = branchCntrlr.GetBranchByPosId(cashtItem.pos2Id);
        //                            cashtItem.branch2Id = branchmodel.branchId;
        //                            cashtItem.branch2Name = branchmodel.name;
        //                        }

        //                    }

        //                }
        //                cachlist = cachlist.Where(X => X.side == "p" ? (X.isConfirm == 1 && X.isConfirm2 == 1) : true).ToList();

        //                List<CardsSts> cardlist = new List<CardsSts>();

        //                cardlist = calctotalCards(cachlist, brIds);


        //                return TokenManager.GenerateToken(cachlist);


        //                // }

        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //            return TokenManager.GenerateToken("0");
        //        }

        //    }

        //}

        // محصلة طرق الدفع بدءا من بداية اليوم الحالي لكل فرع

        //public List<CardsSts> GetpaymentsToday(List<int> brIds)
        //{
        //    List<CardsSts> cardlist = new List<CardsSts>();
        //    try
        //    {
        //        DateTime datenow = cc.AddOffsetTodate(DateTime.Now).Date;
        //        List<OpenClosOperatinModel> cachlist = new List<OpenClosOperatinModel>();
        //        using (incposdbEntities entity = new incposdbEntities())
        //        {
        //            List<cashTransfer> allcashlist = entity.cashTransfer.ToList();

        //            cashTransfer closrow = new cashTransfer();

        //            cachlist = (from C in entity.cashTransfer
        //                        join b in entity.banks on C.bankId equals b.bankId into jb
        //                        join a in entity.agents on C.agentId equals a.agentId into ja
        //                        join p in entity.pos on C.posId equals p.posId into jp//
        //                        join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
        //                        join u in entity.users on C.userId equals u.userId into ju
        //                        //   join uc in entity.users on C.createUserId equals uc.userId into juc
        //                        join uu in entity.users on C.createUserId equals uu.userId into jup

        //                        join cr in entity.cards on C.cardId equals cr.cardId into jcr
        //                        join bo in entity.bondes on C.bondId equals bo.bondId into jbo
        //                        join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
        //                        join i in entity.invoices on C.invId equals i.invoiceId into ji


        //                        from jbb in jb.DefaultIfEmpty()
        //                        from jaa in ja.DefaultIfEmpty()
        //                        from jpp in jp.DefaultIfEmpty()
        //                        from juu in ju.DefaultIfEmpty()
        //                        from jpcc in jpcr.DefaultIfEmpty()
        //                            // from jucc in juc.DefaultIfEmpty()
        //                        from jupdateusr in jup.DefaultIfEmpty()

        //                        from jcrd in jcr.DefaultIfEmpty()
        //                        from jbbo in jbo.DefaultIfEmpty()
        //                        from jssh in jsh.DefaultIfEmpty()
        //                        from I in ji.DefaultIfEmpty()
        //                        where C.updateDate >= datenow
        //                        select new OpenClosOperatinModel()
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
        //                            usersName = juu.name,// side =u

        //                            posName = jpp.name,
        //                            posCreatorName = jpcc.name,
        //                            processType = C.processType,
        //                            cardId = C.cardId,
        //                            bondId = C.bondId,
        //                            usersLName = juu.lastname,// side =u
        //                                                      //createUserName = jucc.name,
        //                                                      //createUserLName = jucc.lastname,
        //                                                      //createUserJob = jucc.job,
        //                            cardName = jcrd.name,
        //                            bondDeserveDate = jbbo.deserveDate,
        //                            bondIsRecieved = jbbo.isRecieved,
        //                            shippingCompanyId = C.shippingCompanyId,
        //                            shippingCompanyName = jssh.name,
        //                            branchCreatorId = jpcc.branchId,
        //                            branchCreatorname = jpcc.branches.name,
        //                         branchId = jpp.branchId,
        //                          //  branchId = C.pos.branchId,
        //                            branchName = jpp.branches.name,
        //                            branch2Id = 0,
        //                            branch2Name = "",
        //                            updateUserAcc = jupdateusr.username,
        //                            invNumber = I.invNumber,
        //                            invType = I.invType,

        //                        })
        //                        .Where(C =>
        //                      (C.transType != "o" && C.transType != "c"
        //                        && C.processType != "balance" && C.processType != "box" &&
        //                         (C.processType != "inv" && C.processType != "distroy" && C.processType != "destroy" && C.processType != "shortage"
        //                                    && C.processType != "deliver" && C.processType != "commissionAgent"
        //                                    && C.processType != "commissionCard") &&
        //                        C.processType != "inv" && C.processType != "cheque" && C.processType != "doc"
        //                        && (C.side == "bn" ? C.isConfirm == 1 : true)
        //                       ))
        //                        .OrderBy(X => X.updateDate).ToList();
        //            /*
        //             *C.transType != "o" && C.transType != "c"
        //                                && C.processType != "balance" && C.processType != "box" &&
        //                                (C.processType != "inv" && C.processType != "distroy" && C.processType != "destroy" && C.processType != "shortage"
        //                                    && C.processType != "deliver" && C.processType != "commissionAgent"
        //                                    && C.processType != "commissionCard") && C.processType != "cheque" && C.processType != "doc"
        //             * */

        //            BranchesController branchCntrlr = new BranchesController();
        //            StatisticsController stsc = new StatisticsController();
        //            if (cachlist.Count > 0)
        //            {
        //                branches branchmodel = new branches();

        //                CashTransferModel tempitem = null;
        //                foreach (OpenClosOperatinModel cashtItem in cachlist)
        //                {
        //                    if (cashtItem.side == "p")
        //                    {
        //                        tempitem = stsc.Getpostransmodel(cashtItem.cashTransId)
        //                            .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
        //                        cashtItem.cashTrans2Id = tempitem.cashTransId;
        //                        cashtItem.pos2Id = tempitem.posId;
        //                        cashtItem.pos2Name = tempitem.posName;
        //                        cashtItem.isConfirm2 = tempitem.isConfirm;

        //                        branchmodel = branchCntrlr.GetBranchByPosId(cashtItem.pos2Id);
        //                        cashtItem.branch2Id = branchmodel.branchId;
        //                        cashtItem.branch2Name = branchmodel.name;
        //                    }

        //                }

        //            }
        //            cachlist = cachlist.Where(X => X.side == "p" ? (X.isConfirm == 1 && X.isConfirm2 == 1) : true).ToList();
        //            cardlist = calctotalCards(cachlist, brIds);
        //        }
        //        return cardlist;
        //    }
        //    catch (Exception ex)
        //    {
        //        cardlist = new List<CardsSts>();
        //        return cardlist;
        //    }
        //}
        // طرق الدفع من اخر عملية فتح وحتى الان
        public List<CardsSts> GetpaymentsToday(List<int> brIds)
        {
            List<CardsSts> cardlist = new List<CardsSts>();
            try
            {
                // DateTime datenow = cc.AddOffsetTodate(DateTime.Now).Date;
                cardlist = calcFinalCards(brIds);
                return cardlist;
            }
            catch (Exception ex)
            {
                cardlist = new List<CardsSts>();
                return cardlist;
            }
        }
        public List<CardsSts> fillCashquery(List<OpenClosOperatinModel> Boxquery, BranchModel branchrow, PosModel posrow)
        {
            List<CardsSts> tmpcard = new List<CardsSts>();
            List<CardsSts> cardtransList = new List<CardsSts>();

            // Boxquery = await stsModel.GetTransfromOpen(posId);
            tmpcard = calctotalCards(Boxquery, branchrow, posrow);
            cardtransList = new List<CardsSts>();
            // open cash
            CardsSts cardcashrow = new CardsSts();
            cardcashrow = BoxOpenCashCalc(Boxquery.ToList(), branchrow, posrow);

            //add cash row
            cardtransList.Add(cardcashrow);
            // cash
            cardcashrow = new CardsSts();
            cardcashrow = BoxCashCalc(Boxquery.ToList(), branchrow, posrow);
            //   cardcashrow.name = MainWindow.resourcemanager.GetString("trCash");
            //add cash row
            cardtransList.Add(cardcashrow);
            //add card list
            cardtransList.AddRange(tmpcard);
            return cardtransList;
        }
        public List<CardsSts> calcFinalCards(List<int> brIds)
        {
            StatisticsController stscntrlr = new StatisticsController();
            List<OpenClosOperatinModel> cashQuery = new List<OpenClosOperatinModel>();
            List<CardsSts> allcardList = new List<CardsSts>();
            List<CardsSts> finalcardList = new List<CardsSts>();
            //  CardsSts cardcashrow = new CardsSts();
            List<CardsSts> cardlist = new List<CardsSts>();
            // cardlist = getallCards();
            List<BranchModel> allbranch = getAllBranches(brIds);
            List<PosModel> posList = new List<PosModel>();
            foreach (BranchModel branchrow in allbranch)
            {

                cashQuery = new List<OpenClosOperatinModel>();
                posList = new List<PosModel>();
                posList = GetPosByBranchId(branchrow.branchId);
                foreach (PosModel posrow in posList)
                {
                    cardlist = new List<CardsSts>();
                    cashQuery = new List<OpenClosOperatinModel>();
                    cashQuery = stscntrlr.createTransfromOpen(posrow.posId);
                    cardlist = fillCashquery(cashQuery, branchrow, posrow);
                    allcardList.AddRange(cardlist);

                    //   cardcashrow = BoxCashCalc(Query );
                }

            }
            finalcardList = allcardList.GroupBy(g => new { g.branchId, g.cardId }).Select(g => new CardsSts
            {
                branchId = g.FirstOrDefault().branchId,
                branchName = g.FirstOrDefault().branchName,
                cardId = g.FirstOrDefault().cardId,
                name = g.FirstOrDefault().name,
                total = g.Sum(x => x.total),
                posId = g.FirstOrDefault().posId,                
            }).ToList();
            return finalcardList;

        }

        public List<CardsSts> calctotalCards(List<OpenClosOperatinModel> Query, BranchModel branchrow, PosModel posrow)
        {
            List<CardsSts> cardlist = new List<CardsSts>();
            cardlist = getallCards();
            List<CardsSts> cardtransList = new List<CardsSts>();
            foreach (CardsSts card in cardlist)
            {
                CardsSts tempcard = new CardsSts();

                tempcard.cardId = card.cardId;
                tempcard.name = card.name;
                tempcard.hasProcessNum = card.hasProcessNum;
                tempcard.image = card.image;
                tempcard.isActive = card.isActive;
                tempcard.total = 0;
                tempcard.name = card.name;
                cardtransList.Add(tempcard);
            }
            //card sum
            foreach (CardsSts card in cardtransList)
            {
                decimal pay = 0;
                decimal deposit = 0;
                pay = (decimal)Query.Where(x => x.processType == "card" && x.cardId == card.cardId && x.transType == "p").Sum(x => x.cash);
                deposit = (decimal)Query.Where(x => x.processType == "card" && x.cardId == card.cardId && x.transType == "d").Sum(x => x.cash);
                card.total = deposit - pay;

                //card.total = (decimal)queryrOps.Where(x => x.processType == "card" && x.cardId == card.cardId).Sum(x => x.cash);
                //converter
                card.total = card.total;
                card.branchId = branchrow.branchId;
                card.branchName = branchrow.name;
                card.posId = posrow.posId;
            }
            return cardtransList;
        }
        //public List<CardsSts> calctotalCards(List<OpenClosOperatinModel> Query, List<int> brIds)
        //{
        //    List<CardsSts> cardtransList = new List<CardsSts>();
        //    CardsSts cardcashrow = new CardsSts();
        //    List<CardsSts> cardlist = new List<CardsSts>();
        //    cardlist = getallCards();
        //    List<BranchModel> allbranch = getAllBranches(brIds);
        //    foreach (BranchModel branchrow in allbranch)
        //    {
        //        // cash sum
        //        cardcashrow=  new CardsSts();
        //        cardcashrow = BoxCashCalc(Query, branchrow.branchId);

        //        cardcashrow.branchName = branchrow.name;
        //        cardcashrow.branchId = branchrow.branchId;
        //        //add cash row
        //        cardtransList.Add(cardcashrow);
        //        //card total

        //        foreach (CardsSts card in cardlist)
        //        {

        //            cardcashrow = new CardsSts();
        //            decimal pay = 0;
        //            decimal deposit = 0;
        //            pay = (decimal)Query.Where(x => x.processType == "card" && x.cardId == card.cardId && x.transType == "p" && x.branchId == branchrow.branchId).Sum(x => x.cash);
        //            deposit = (decimal)Query.Where(x => x.processType == "card" && x.cardId == card.cardId && x.transType == "d" && x.branchId == branchrow.branchId).Sum(x => x.cash);
        //           // card.total = deposit - pay;
        //            //  cardcashrow = card;
        //            cardcashrow.name = card.name;
        //            cardcashrow.cardId = card.cardId;
        //            cardcashrow.total = deposit - pay;
        //            cardcashrow.branchId = branchrow.branchId;
        //            cardcashrow.branchName = branchrow.name;
        //            cardtransList.Add(cardcashrow);

        //        }
        //        //add card list
        //        //  cardtransList.AddRange(cardlist);

        //    }

        //    return cardtransList;

        //}

        public List<CardsSts> getallCards()
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var cardsList = entity.cards

               .Select(c => new CardsSts()
               {
                   cardId = c.cardId,
                   name = c.name,
                   isActive = c.isActive,
               })
               .ToList();


                return cardsList;
            }
        }
        public CardsSts BoxCashCalc(List<OpenClosOperatinModel> Query, BranchModel branchrow, PosModel posrow)
        {
            CardsSts cardcashrow = new CardsSts();
            // cash
            decimal sumCash = 0;
            decimal pay = 0;
            decimal deposit = 0;
            pay = (decimal)Query.Where(x => (x.processType == "cash" && x.transType == "p") || (x.side == "p" && x.transType == "d") || (x.side == "bn" && x.transType == "d")).Sum(x => x.cash);
            deposit = (decimal)Query.Where(x => (x.processType == "cash" && x.transType == "d") || (x.side == "p" && x.transType == "p") || (x.side == "bn" && x.transType == "p")).Sum(x => x.cash);

            sumCash = deposit - pay;
            cardcashrow.name = "cash";
            cardcashrow.total = sumCash;
            cardcashrow.cardId = -1;

            cardcashrow.branchId = branchrow.branchId;
            cardcashrow.branchName = branchrow.name;
            cardcashrow.posId = posrow.posId;
            return cardcashrow;
        }
        public static CardsSts BoxOpenCashCalc(List<OpenClosOperatinModel> Query, BranchModel branchrow, PosModel posrow)
        {
            CardsSts cardcashrow = new CardsSts();
            OpenClosOperatinModel openrow = Query.Where(x => x.transType == "o" && x.processType == "box").FirstOrDefault();

            if (openrow == null)
            {
                cardcashrow.name = "obox";
                cardcashrow.total = 0;
                cardcashrow.cardId = -2;
                cardcashrow.branchId = branchrow.branchId;
                cardcashrow.branchName = branchrow.name;
                cardcashrow.posId = posrow.posId;
            }
            else
            {
                cardcashrow.name = "obox";
                cardcashrow.total = openrow.cash;
                cardcashrow.cardId = -2;
                cardcashrow.branchId = branchrow.branchId;
                cardcashrow.branchName = branchrow.name;
                cardcashrow.posId = posrow.posId;
            }
            return cardcashrow;
        }


        //public CardsSts BoxCashCalc(List<OpenClosOperatinModel> Query, int branchId)
        //{
        //    CardsSts cardcashrow = new CardsSts();
        //    // cash
        //    decimal sumCash = 0;
        //    decimal pay = 0;
        //    decimal deposit = 0;
        //    pay = (decimal)Query.Where(x => x.branchId == branchId && ((x.processType == "cash" && x.transType == "p") || (x.side == "p" && x.transType == "d") || (x.side == "bn" && x.transType == "d"))).Sum(x => x.cash);
        //    deposit = (decimal)Query.Where(x => x.branchId == branchId && ((x.processType == "cash" && x.transType == "d") || (x.side == "p" && x.transType == "p") || (x.side == "bn" && x.transType == "p"))).Sum(x => x.cash);
        //    sumCash = deposit - pay;
        //    cardcashrow.name = "cash";
        //    cardcashrow.total = sumCash;

        //    return cardcashrow;
        //}
        public List<BranchModel> getAllBranches(List<int> brIds)
        {
            List<BranchModel> allbranch = new List<BranchModel>();
            //StatisticsController sts = new StatisticsController();
            //List<int> brIds = sts.AllowedBranchsId(mainBranchId, userId);
            using (incposdbEntities entity = new incposdbEntities())
            {
                allbranch = (from b in entity.branches

                                 // join u in entity.users on log.userId equals u.userId

                             where (brIds.Contains((int)b.branchId) && b.branchId != 1)

                             select new BranchModel
                             {

                                 branchId = b.branchId,
                                 name = b.name,
                                 // branchisActive = p.branches.isActive,

                                 // posId = p.posId,
                             }).ToList();
            }
            return allbranch;
        }
        public List<PosModel> GetPosByBranchId(int branchId)
        {
            List<PosModel> poslist = new List<PosModel>();
            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    poslist = (from p in entity.pos
                               where p.branchId == branchId
                               select new PosModel()
                               {
                                   posId = p.posId,
                                   balance = p.balance != null ? p.balance : 0,
                                   branchId = p.branchId,
                                   code = p.code,
                                   name = p.name,

                                   //createDate = p.createDate,
                                   //updateDate = p.updateDate,
                                   //createUserId = p.createUserId,
                                   //updateUserId = p.updateUserId,
                                   isActive = p.isActive,
                                   balanceAll = p.balanceAll,
                                   note = p.note,
                                   isAdminClose = p.isAdminClose,
                                   boxState = p.boxState,
                               }).ToList();
                    return poslist;
                }
            }
            catch
            {
                poslist = new List<PosModel>();
                return poslist;
            }

        }

        public List<AgentsCountbyBranch> GetagentsCount(  List<int> branchIds)
        {
            List<BranchModel> allowedBranch = new List<BranchModel>();
            List<AgentsCountbyBranch> agentBranchlist = new List<AgentsCountbyBranch>();
            AgentsCountbyBranch AgentsCount = new AgentsCountbyBranch();
            List<InvoiceModel> InvLIst = new List<InvoiceModel>();
            try
            {
                var searchPredicate = PredicateBuilder.New<invoices>();
                searchPredicate = PredicateBuilder.New<invoices>();
                searchPredicate = searchPredicate.And(x => x.isActive == true && (x.invType == "p" || x.invType == "pw"|| x.invType == "s") && x.agentId != null);
           
                //   searchPredicate = searchPredicate.And(x => x.agentId != null);

                //if (branchId != 0)
                //    searchPredicate = searchPredicate.And(x => x.branchId == branchId);
                //else if (userId != 2)
                //{
                //    searchPredicate = searchPredicate.And(x => branchIds.Contains((int)x.branchId));
                //}
                searchPredicate = searchPredicate.And(x => branchIds.Contains((int)x.branchId));
                //  dashBoardModel.vendorsCount = entity.invoices.Where(searchPredicate).Select(x => x.agentId).ToList().Distinct().Count();

                using (incposdbEntities entity = new incposdbEntities())
                {
                    InvLIst = entity.invoices.Where(searchPredicate).Select(x => new InvoiceModel {
                        agentId =x.agentId,
                        invType =x.invType,
                        branchId =x.branchId,
                    }).ToList();
                }
                allowedBranch = getAllBranches(branchIds);
                List<InvoiceModel> tmpVinvlist = new List<InvoiceModel>();
                List<InvoiceModel> tmpCinvlist = new List<InvoiceModel>();
                List<InvoiceModel> groupVendorinvlist = new List<InvoiceModel>();
                List<InvoiceModel> groupCustomerinvlist = new List<InvoiceModel>();
                tmpVinvlist = InvLIst.Where(x => (x.invType == "p" || x.invType == "pw")  ).ToList();
                tmpCinvlist = InvLIst.Where(x => (x.invType == "s")).ToList();
                groupVendorinvlist = tmpVinvlist.GroupBy(g => new { g.branchId, g.agentId }).Select(g => new InvoiceModel
                {
                    agentId = g.FirstOrDefault().agentId,
                    branchId = g.FirstOrDefault().branchId,
                }).ToList();
                groupCustomerinvlist = tmpCinvlist.GroupBy(g => new { g.branchId, g.agentId }).Select(g => new InvoiceModel
                {
                    agentId = g.FirstOrDefault().agentId,
                    branchId = g.FirstOrDefault().branchId,
                }).ToList();
                //all branches

                int  CountAllVendorinvlist= tmpVinvlist.GroupBy(g => new { g.agentId }).Select(g => new InvoiceModel
                {
                    agentId = g.FirstOrDefault().agentId,
                    branchId = g.FirstOrDefault().branchId,
                }).ToList().Count();
                int CountAllCustomerinvlist = tmpCinvlist.GroupBy(g => new { g.agentId }).Select(g => new InvoiceModel
                {
                    agentId = g.FirstOrDefault().agentId,
                    branchId = g.FirstOrDefault().branchId,
                }).ToList().Count();
                //add row
                AgentsCount.branchId = 0;
                AgentsCount.branchName ="all";
                AgentsCount.vendorsCount = CountAllVendorinvlist;
                AgentsCount.customersCount = CountAllCustomerinvlist;
                agentBranchlist.Add(AgentsCount);
                foreach (BranchModel branchRow in allowedBranch)
                {
                    AgentsCount = new AgentsCountbyBranch();
                    AgentsCount.branchId = branchRow.branchId;
                    AgentsCount.branchName = branchRow.name;
                    AgentsCount.vendorsCount = groupVendorinvlist.Where(x => x.branchId == branchRow.branchId).ToList().Count();
                    AgentsCount.customersCount = groupCustomerinvlist.Where(x => x.branchId == branchRow.branchId).ToList().Count();
                    agentBranchlist.Add(AgentsCount);
                }
                return agentBranchlist;
            }
            catch
            {
                agentBranchlist = new List<AgentsCountbyBranch>();
                return agentBranchlist;
            }

        }
       


    }
}