﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Classes;
using POS_Server.Models;
using POS_Server.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/Pos")]
    public class PosController : ApiController
    {
        CountriesController countryc = new CountriesController();

        [HttpGet]
        [Route("checkUri")]
        public string checkUri()
        {
            return "";
        }


        // GET api/<controller>
        [HttpPost]
        [Route("Get")]
        public string Get(string token)
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
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var posList = (from p in entity.pos
                                   join b in entity.branches on p.branchId equals b.branchId into lj
                                   from x in lj.DefaultIfEmpty()
                                   select new PosModel() {
                                       posId = p.posId,
                                       balance = p.balance != null ? p.balance : 0,
                                       branchId = p.branchId,
                                       code = p.code,
                                       name = p.name,
                                       branchName = x.name,
                                       createDate = p.createDate,
                                       updateDate = p.updateDate,
                                       createUserId = p.createUserId,
                                       updateUserId = p.updateUserId,
                                       isActive = p.isActive,
                                      balanceAll=p.balanceAll,
                                       note = p.note,
                                       branchCode = x.code,
                                       boxState = p.boxState,
                                       isAdminClose = p.isAdminClose,
                                   }).ToList();

                    if (posList.Count > 0)
                    {
                        for (int i = 0; i < posList.Count; i++)
                        {
                            canDelete = false;
                            if (posList[i].isActive == 1)
                            {
                                int posId = (int)posList[i].posId;
                                var cashTransferL = entity.cashTransfer.Where(x => x.posId == posId).Select(b => new { b.cashTransId }).FirstOrDefault();
                                var posUsersL = entity.posUsers.Where(x => x.posId == posId).Select(x => new { x.posUserId }).FirstOrDefault();
                               
                                if ((cashTransferL is null) && (posUsersL is null))
                                    canDelete = true;
                            }
                           
                            posList[i].canDelete = canDelete;
                        }
                    }
                    return TokenManager.GenerateToken(posList);
                }
            }
          
        }
        // GET api/<controller>
        [HttpPost]
        [Route("GetPosByID")]
        public string GetPosByID(string token)
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
                    if (c.Type == "itemId")
                    {
                        posId = int.Parse(c.Value);
                    }
                }
                var pos = GetPosByID(posId);
                return TokenManager.GenerateToken(pos);

                //using (incposdbEntities entity = new incposdbEntities())
                //{
                //    var pos = (from p in entity.pos where p.posId == posId
                //                join b in entity.branches on p.branchId equals b.branchId into lj
                //                from x in lj.DefaultIfEmpty()
                //                select new PosModel()
                //                {
                //                    posId = p.posId,
                //                    balance = p.balance != null ? p.balance : 0,
                //                    branchId = p.branchId,
                //                    code = p.code,
                //                    name = p.name,
                //                    branchName = x.name,
                //                    createDate = p.createDate,
                //                    updateDate = p.updateDate,
                //                    createUserId = p.createUserId,
                //                    updateUserId = p.updateUserId,
                //                    isActive = p.isActive,
                //                    balanceAll = p.balanceAll,
                //                    note = p.note,
                //                    branchCode = x.code,
                //                    isAdminClose = p.isAdminClose,
                //                    boxState = p.boxState,
                //                }).FirstOrDefault();
                //    return TokenManager.GenerateToken(pos);
                //}
            }
         }
        public PosModel GetPosByID(int posId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var pos = (from p in entity.pos
                           where p.posId == posId
                           join b in entity.branches on p.branchId equals b.branchId into lj
                           from x in lj.DefaultIfEmpty()
                           select new PosModel()
                           {
                               posId = p.posId,
                               balance = p.balance != null ? p.balance : 0,
                               branchId = p.branchId,
                               code = p.code,
                               name = p.name,
                               branchName = x.name,
                               createDate = p.createDate,
                               updateDate = p.updateDate,
                               createUserId = p.createUserId,
                               updateUserId = p.updateUserId,
                               isActive = p.isActive,
                               balanceAll = p.balanceAll,
                               note = p.note,
                               branchCode = x.code,
                               isAdminClose = p.isAdminClose,
                               boxState = p.boxState,
                           }).FirstOrDefault();
                return pos;
            }
        }
        // add or update pos
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
                string posObject = "";
                pos newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        posObject = c.Value.Replace("\\", string.Empty);
                        posObject = posObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<pos>(posObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
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
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        pos tmpPos = new pos();
                        var unitEntity = entity.Set<pos>();
                        if (newObject.posId == 0)
                        {
                            ProgramInfo programInfo = new ProgramInfo();
                            int posMaxCount = programInfo.getPosCount();
                            int posCount = entity.pos.Count();
                            if (posCount >= posMaxCount && posMaxCount != -1)
                            {
                                message = "-1";
                                return TokenManager.GenerateToken(message);
                            }
                            else
                            {
                                newObject.createDate = countryc.AddOffsetTodate(DateTime.Now);
                                newObject.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                                newObject.updateUserId = newObject.createUserId;
                                newObject.balance = 0;
                                newObject.balanceAll = 0;
                                newObject.boxState = "c";
                                newObject.isAdminClose = 0;
                                tmpPos = unitEntity.Add(newObject);
                                entity.SaveChanges();
                                message = tmpPos.posId.ToString();
                            }
                            return TokenManager.GenerateToken(message);
                        }
                        else
                        {
                            tmpPos = entity.pos.Where(p => p.posId == newObject.posId).FirstOrDefault();
                            tmpPos.name = newObject.name;
                            tmpPos.code = newObject.code;
                            tmpPos.branchId = newObject.branchId;
                            tmpPos.note = newObject.note;
                            tmpPos.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                            tmpPos.updateUserId = newObject.updateUserId;
                            tmpPos.isActive = newObject.isActive;
                            tmpPos.balance = newObject.balance;
                            tmpPos.balanceAll = newObject.balanceAll;
                            entity.SaveChanges();
                            message = tmpPos.posId.ToString();
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
        [Route("EditBalance")]
        public string EditBalance(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string result = "";
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int posId = 0;
                decimal balance = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                    else if (c.Type == "balance")
                        balance = decimal.Parse(c.Value);
                }

                using(var entity = new incposdbEntities())
                {
                    var pos = entity.pos.Find(posId);
                    pos.balance += balance;
                    entity.SaveChanges();

                    var posModel = entity.pos.Where(x => x.posId == posId)
                        .Select(x => new PosModel()
                        {
                            balance = x.balance,
                            posId = x.posId
                        }).FirstOrDefault();
                   return TokenManager.GenerateToken(posModel);
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
                int posId = 0;
                int userId = 0;
                Boolean final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        posId = int.Parse(c.Value);
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
                            pos posDelete = entity.pos.Find(posId);

                            entity.pos.Remove(posDelete);
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
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        pos posDelete = entity.pos.Find(posId);

                        posDelete.isActive = 0;
                        posDelete.updateUserId = userId;
                        posDelete.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                        message = entity.SaveChanges().ToString();
                        return TokenManager.GenerateToken(message);
                    }
                }
            }
        }
         [HttpPost]
        [Route("checkPreviousActivate")]
        public string checkPreviousActivate(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                string deviceCode = "";

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "deviceCode")
                    {
                        deviceCode = c.Value;
                    }
                   
                }

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var posSet = entity.posSetting.Where(x=> x.posDeviceCode == deviceCode).FirstOrDefault();

                    PosModel pos = new PosModel();
                    if (posSet != null)
                    {

                        int posId =(int) posSet.posId;
                        pos = GetPosByID(posId);
                    }

                    return TokenManager.GenerateToken(pos);
                }
            }
        }

        [HttpPost]
        [Route("updateBoxState")]
        public string updateBoxState(string token)
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
                int posId = 0;
                int userId = 0;
                string boxState = "";
                string cashObject = "";
                int isAdminClose = 0;
                cashTransfer cashTransfer = new cashTransfer() ;
                CashTransferController cc = new CashTransferController();
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
                    else if (c.Type == "isAdminClose")
                    {
                        isAdminClose = int.Parse(c.Value);
                    }
                    else if (c.Type == "state")
                    {
                        boxState = c.Value;
                    }
                    if (c.Type == "cashTransfer")
                    {
                        cashObject = c.Value.Replace("\\", string.Empty);
                        cashObject = cashObject.Trim('"');
                        cashTransfer = JsonConvert.DeserializeObject<cashTransfer>(cashObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }

                using (incposdbEntities entity = new incposdbEntities())
                {
                    pos pos = entity.pos.Find(posId);

                    pos.boxState = boxState;
                    pos.isAdminClose = (byte)isAdminClose;
                    pos.updateUserId = userId;
                    pos.updateDate = countryc.AddOffsetTodate(DateTime.Now);
                    message = entity.SaveChanges().ToString();

                    cc.addCashTransfer(cashTransfer);
                    return TokenManager.GenerateToken(message);
                }
            }
        }

        [HttpPost]
        [Route("setConfiguration")]
        public string setConfiguration(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            //if (strP != "0") //invalid authorization
            //{
            //    return TokenManager.GenerateToken(strP);
            //}
            //else
            {
                string setObject = "";
                string activationCode = "";
                string deviceCode = "";
                int countryId = 0;
                string userName = "";
                string password = "";
                string branchName = "";
                string branchCode = "";
                string branchMobile = "";
                string posName = "";
                List<setValuesModel> newObject = null;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "setValues")
                    {
                        setObject = c.Value.Replace("\\", string.Empty);
                        setObject = setObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<List<setValuesModel>>(setObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    }
                    else if (c.Type == "activationCode")
                    {
                        activationCode = c.Value;
                    }
                    else if (c.Type == "deviceCode")
                    {
                        deviceCode = c.Value;
                    }
                    else if (c.Type == "countryId")
                    {
                        countryId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userName")
                    {
                        userName = c.Value;
                    }
                    else if (c.Type == "password")
                    {
                        password = c.Value;
                    }
                    else if (c.Type == "branchMobile")
                    {
                        branchMobile = c.Value;
                    }
                    else if (c.Type == "branchName")
                    {
                        branchName = c.Value;
                    }
                    else if (c.Type == "branchCode")
                    {
                        branchCode = c.Value;
                    }
                    else if (c.Type == "posName")
                    {
                        posName = c.Value;
                    }
                }

                //try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        pos tmpPos = new pos();
                        var unitEntity = entity.Set<pos>();
                       
                        var validSerial = entity.posSerials.Where(x => x.posSerial == activationCode).FirstOrDefault();
                        if (validSerial != null) // activation code is correct
                        {
                            var serialExist = entity.posSetting.Where(x => x.posSerialId == validSerial.id).FirstOrDefault();

                            if (serialExist == null) // activation code is available
                            {
                                var pos = entity.pos.Find(1);
                                pos.name = posName;
                                entity.SaveChanges();
                                #region add pos settings record
                                var posSett = new posSetting()
                                {
                                    posId = pos.posId,
                                    posSerialId = validSerial.id,
                                    posDeviceCode = deviceCode,
                                    createDate = countryc.AddOffsetTodate(DateTime.Now),
                                    updateDate = countryc.AddOffsetTodate(DateTime.Now),
                                };
                                entity.posSetting.Add(posSett);
                                #endregion
                                #region region settings
                                List<countriesCodes> objectlist = entity.countriesCodes.Where(x => x.isDefault == 1).ToList();
                                if (objectlist.Count > 0)
                                {
                                    for (int i = 0; i < objectlist.Count; i++)
                                    {
                                        objectlist[i].isDefault = 0;

                                    }
                                }
                                // set is selected to isdefault=1
                                countriesCodes objectrow = entity.countriesCodes.Find(countryId);
                                if (objectrow != null)
                                    objectrow.isDefault = 1;

                                #endregion
                                #region update user
                                var user = entity.users.Find(2);
                                user.username = userName;
                                user.password = password;
                                #endregion
                                #region update branch
                                var branch = entity.branches.Find(2);
                                branch.name = branchName;
                                branch.code = branchCode;
                                branch.mobile = branchMobile;
                                #endregion
                                #region company info
                                foreach (setValuesModel v in newObject)
                                {
                                    var setId = entity.setting.Where(x => x.name == v.name).Select(x => x.settingId).Single();
                                    var setValue = entity.setValues.Where(x => x.settingId == setId).FirstOrDefault();
                                    setValue.value = v.value;                                   
                                }
                                #endregion
                                entity.SaveChanges();
                                return TokenManager.GenerateToken(pos.posId.ToString());

                            }
                            else
                                return TokenManager.GenerateToken("-3"); // activation code is unavailable
                        }
                        else
                            return TokenManager.GenerateToken("-2"); // serial is wrong
                    }
                }
                //catch(Exception ex)
                //{
                //    return TokenManager.GenerateToken(ex.ToString());
                //}
            }
        }
        [HttpPost]
        [Route("setPosConfiguration")]
        public string setPosConfiguration(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string activationCode = "";
                string deviceCode = "";
                int posId = 0;               

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "activationCode")
                    {
                        activationCode = c.Value;
                    }
                    else if (c.Type == "deviceCode")
                    {
                        deviceCode = c.Value;
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }  
                }

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        pos tmpPos = new pos();
                        var unitEntity = entity.Set<pos>();
                        var validSerial = entity.posSerials.Where(x => x.posSerial == activationCode).FirstOrDefault();
                        if (validSerial != null) // activation code is correct
                        {
                            var serialExist = entity.posSetting.Where(x => x.posSerialId == validSerial.id).FirstOrDefault();

                            if (serialExist == null ) // activation code is available or unlimited package
                            {
                                #region add pos settings record
                                var posSett = new posSetting()
                                {
                                    posId = posId,
                                    posSerialId = validSerial.id,
                                    posDeviceCode = deviceCode,
                                    createDate = countryc.AddOffsetTodate(DateTime.Now),
                                    updateDate = countryc.AddOffsetTodate(DateTime.Now),
                                    //programName = "",
                                    //versionName = "",
                                };
                                entity.posSetting.Add(posSett);
                                #endregion
                               
                                entity.SaveChanges();
                                return TokenManager.GenerateToken(posId.ToString());

                            }
                            else
                                return TokenManager.GenerateToken("-3"); // activation code is unavailable
                        }
                        else
                            return TokenManager.GenerateToken("-2"); // serial is wrong
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }

        [HttpPost]
        [Route("getInstallationNum")]
        public string getInstallationNum(string token)
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
                using (incposdbEntities entity = new incposdbEntities())
                {
                    try
                    {
                        var pos = entity.posSetting.FirstOrDefault();
                        if (pos == null)
                            return TokenManager.GenerateToken("1");
                        else
                            return TokenManager.GenerateToken("2");
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }
            }
        }
        [HttpPost]
        [Route("GetUnactivated")]
        public string GetUnactivated(string token)
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
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var posList = (from p in entity.pos where p.isActive == 1
                                   join b in entity.branches on p.branchId equals b.branchId into lj
                                   from x in lj.DefaultIfEmpty()
                                   where x.branchId == branchId 
                                   && !entity.posSetting.Any(m => m.posId == p.posId )
                                   select new PosModel()
                                   {
                                       posId = p.posId,
                                       name = p.name, 
                                   }).ToList();
   
                    return TokenManager.GenerateToken(posList);
                }
            }

        }

        [HttpPost]
        [Route("GetByUserId")]
        public string GetByUserId(string token)
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
                    if (c.Type == "itemId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                BranchesController branchctrlr = new BranchesController();
             
                List<BranchModel> bmList = new List<BranchModel>();
                bmList= branchctrlr.BranchesByUserIdType(userId);
                List<int> brIds = bmList.Select(S => S.branchId).ToList();

                using (incposdbEntities entity = new incposdbEntities())
                {
                    var pos = (from p in entity.pos
                               where brIds.Contains((int)p.branchId) && p.isActive==1


                               select new PosModel()
                               {
                                   posId = p.posId,
                                   balance = p.balance != null ? p.balance : 0,
                                   branchId = p.branchId,
                                   code = p.code,
                                   name = p.name,
                                //   branchName = x.name,
                                   createDate = p.createDate,
                                   updateDate = p.updateDate,
                                   createUserId = p.createUserId,
                                   updateUserId = p.updateUserId,
                                   isActive = p.isActive,
                                   balanceAll = p.balanceAll,
                                   note = p.note,
                                  // branchCode = x.code,
                                   isAdminClose = p.isAdminClose,
                                   boxState = p.boxState,
                                   branchName=p.branches.name,
                               }).ToList();
                    return TokenManager.GenerateToken(pos);

                }
              

                //using (incposdbEntities entity = new incposdbEntities())
                //{
                //    var pos = (from p in entity.pos where p.posId == posId
                //                join b in entity.branches on p.branchId equals b.branchId into lj
                //                from x in lj.DefaultIfEmpty()
                //                select new PosModel()
                //                {
                //                    posId = p.posId,
                //                    balance = p.balance != null ? p.balance : 0,
                //                    branchId = p.branchId,
                //                    code = p.code,
                //                    name = p.name,
                //                    branchName = x.name,
                //                    createDate = p.createDate,
                //                    updateDate = p.updateDate,
                //                    createUserId = p.createUserId,
                //                    updateUserId = p.updateUserId,
                //                    isActive = p.isActive,
                //                    balanceAll = p.balanceAll,
                //                    note = p.note,
                //                    branchCode = x.code,
                //                    isAdminClose = p.isAdminClose,
                //                    boxState = p.boxState,
                //                }).FirstOrDefault();
                //    return TokenManager.GenerateToken(pos);
                //}
            }
        }
    }
}