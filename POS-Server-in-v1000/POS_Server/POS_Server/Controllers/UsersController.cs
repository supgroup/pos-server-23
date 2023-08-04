using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS_Server.Classes;
using POS_Server.Models;
using POS_Server.Models.VM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web;
using System.Threading.Tasks;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/Users")]
    public class UsersController : ApiController
    {
        CountriesController cc = new CountriesController();
        //get active users
        [HttpPost]
        [Route("GetActive")]
        public string GetActive(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            //if (strP != "0") //invalid authorization
            //{
            //    return TokenManager.GenerateToken(strP);
            //}
            //else
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var usersList = entity.users.Where(u => u.isActive == 1 && u.userId != 1)
                    .Select(u => new UserModel
                    {
                        userId = u.userId,
                        username = u.username,
                        password = u.password,
                        name = u.name,
                        lastname = u.lastname,
                        fullName = u.name + " " + u.lastname,
                        job = u.job,
                        workHours = u.workHours,
                        createDate = u.createDate,
                        updateDate = u.updateDate,
                        createUserId = u.createUserId,
                        updateUserId = u.updateUserId,
                        phone = u.phone,
                        mobile = u.mobile,
                        email = u.email,
                        notes = u.notes,
                        address = u.address,
                        isActive = u.isActive,
                        isOnline = u.isOnline,
                        image = u.image,
                        balance = u.balance,
                        balanceType = u.balanceType,
                        isAdmin = u.isAdmin,
                        groupId = u.groupId,
                        groupName = entity.groups.Where(g => g.groupId == u.groupId).FirstOrDefault().name,
                        hasCommission = u.hasCommission,
                        commissionValue = u.commissionValue,
                        commissionRatio = u.commissionRatio,

                    })
                    .ToList();

                    /*
                     hasCommission
commissionValue
commissionRatio

                     * */
                    return TokenManager.GenerateToken(usersList);
                }
            }
        }

        [HttpPost]
        [Route("GetActiveForAccount")]
        public string GetActiveForAccount(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string payType = "";
            Boolean canDelete = false;
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
                    if (c.Type == "payType")
                    {
                        payType = c.Value;
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var usersList = entity.users.Where(u => u.userId != 1 && (u.isActive == 1 ||
                                                 (u.isActive == 0 && payType == "p" && u.balanceType == 0) ||
                                                 (u.isActive == 0 && payType == "d" && u.balanceType == 1)))
                    .Select(u => new UserModel
                    {
                        userId = u.userId,
                        username = u.username,
                        password = u.password,
                        name = u.name,
                        lastname = u.lastname,
                        fullName = u.name + " " + u.lastname,
                        job = u.job,
                        workHours = u.workHours,
                        createDate = u.createDate,
                        updateDate = u.updateDate,
                        createUserId = u.createUserId,
                        updateUserId = u.updateUserId,
                        phone = u.phone,
                        mobile = u.mobile,
                        email = u.email,
                        notes = u.notes,
                        address = u.address,
                        isActive = u.isActive,
                        isOnline = u.isOnline,
                        image = u.image,
                        balance = u.balance,
                        balanceType = u.balanceType,
                        isAdmin = u.isAdmin,
                        groupId = u.groupId,
                        groupName = entity.groups.Where(g => g.groupId == u.groupId).FirstOrDefault().name,
                        hasCommission = u.hasCommission,
                        commissionValue = u.commissionValue,
                        commissionRatio = u.commissionRatio,
                    })
                    .ToList();

                    return TokenManager.GenerateToken(usersList);
                }
            }
        }

        [HttpPost]
        [Route("Getloginuser")]
        public async Task<string> Getloginuser(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            List<UserModel> usersList = new List<UserModel>();
            UserModel user = new UserModel();
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string userName = "";
                string password = "";
                int posId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "userName")
                    {
                        userName = c.Value;
                    }
                    else if (c.Type == "password")
                    {
                        password = c.Value;
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                }

                UserModel emptyuser = new UserModel();

                emptyuser.createDate = cc.AddOffsetTodate(DateTime.Now);
                emptyuser.updateDate = cc.AddOffsetTodate(DateTime.Now);
                //emptyuser.username = userName;
                emptyuser.createUserId = 0;
                emptyuser.updateUserId = 0;
                emptyuser.userId = 0;
                emptyuser.isActive = 0;
                emptyuser.isOnline = 0;
                emptyuser.canDelete = false;
                emptyuser.balance = 0;
                emptyuser.balanceType = 0;
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        usersList = entity.users.Where(u => u.isActive == 1 && u.username == userName)
                        .Select(u => new UserModel
                        {
                            userId = u.userId,
                            username = u.username,
                            password = u.password,
                            name = u.name,
                            lastname = u.lastname,
                            fullName = u.name + " " + u.lastname,
                            job = u.job,
                            workHours = u.workHours,
                            createDate = u.createDate,
                            updateDate = u.updateDate,
                            createUserId = u.createUserId,
                            updateUserId = u.updateUserId,
                            phone = u.phone,
                            mobile = u.mobile,
                            email = u.email,
                            notes = u.notes,
                            address = u.address,
                            isActive = u.isActive,
                            isOnline = u.isOnline,
                            image = u.image,
                            balance = u.balance,
                            balanceType = u.balanceType,
                            isAdmin = u.isAdmin,
                            groupId = u.groupId,
                            groupName = entity.groups.Where(g => g.groupId == u.groupId).FirstOrDefault().name,
                            hasCommission = u.hasCommission,
                            commissionValue = u.commissionValue,
                            commissionRatio = u.commissionRatio,
                        })
                        .ToList();

                        if (usersList == null || usersList.Count <= 0)
                        {
                            user = emptyuser;
                            // rong user
                            return TokenManager.GenerateToken(user);
                        }
                        else
                        {
                            user = usersList.Where(i => i.username == userName).FirstOrDefault();
                            if (user.password.Equals(password))
                            {
                                #region check if user can login and set other user logOut
                                user.canLogin = await CanLogIn(user.userId, posId);
                                if (user.canLogin == 1 || (user.username == "Support@Increase" && user.isAdmin == true) || (user.isAdmin == true))
                                {

                                    //make user online
                                    var us = entity.users.Find(user.userId);
                                    us.isOnline = 1;

                                    UsersLogsController ulc = new UsersLogsController();
                                    ulc.checkOtherUser(user.userId);
                                    ulc.SignOutOld(user.userId);
                                    //create lognin record
                                    usersLogs usersLogs = new usersLogs()
                                    {
                                        posId = posId,
                                        userId = user.userId,
                                        sInDate = cc.AddOffsetTodate(DateTime.Now),

                                    };
                                    entity.usersLogs.Add(usersLogs);
                                    entity.SaveChanges();
                                    user.userLogInID = usersLogs.logId;
                                    var pos = entity.pos.Find(posId);
                                    user.branchId = pos.branchId;
                                }
                                #endregion
                                // correct username and pasword
                                return TokenManager.GenerateToken(user);
                            }
                            else
                            {
                                // rong pass return just username
                                user = emptyuser;
                                user.username = userName;
                                return TokenManager.GenerateToken(user);

                            }
                        }
                    }

                }
                catch(Exception ex)
                {
                  //  emptyuser.notes = ex.ToString();
                    return TokenManager.GenerateToken(emptyuser);
                }
            }
        }

        //GetAll
        // return all users active and inactive
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
                    var usersList = entity.users
                    .Select(u => new UserModel()
                    {
                        userId = u.userId,
                        username = u.username,
                        password = u.password,
                        name = u.name,
                        lastname = u.lastname,
                        fullName = u.name + " " + u.lastname,
                        job = u.job,
                        workHours = u.workHours,
                        createDate = u.createDate,
                        updateDate = u.updateDate,
                        createUserId = u.createUserId,
                        updateUserId = u.updateUserId,
                        phone = u.phone,
                        mobile = u.mobile,
                        email = u.email,
                        notes = u.notes,
                        address = u.address,
                        isActive = u.isActive,
                        isOnline = u.isOnline,
                        image = u.image,
                        balance = u.balance,
                        balanceType = u.balanceType,
                        isAdmin = u.isAdmin,
                        groupId = u.groupId,
                        groupName = entity.groups.Where(g => g.groupId == u.groupId).FirstOrDefault().name,
                        hasCommission = u.hasCommission,
                        commissionValue = u.commissionValue,
                        commissionRatio = u.commissionRatio,
                    })
                    .ToList();

                    if (usersList.Count > 0)
                    {
                        for (int i = 0; i < usersList.Count; i++)
                        {
                            canDelete = false;
                            if (usersList[i].isActive == 1)
                            {
                                int userId = (int)usersList[i].userId;
                                var usersPos = entity.posUsers.Where(x => x.userId == userId).Select(b => new { b.posUserId }).FirstOrDefault();
                                if (usersPos is null)
                                    canDelete = true;
                            }

                            usersList[i].canDelete = canDelete;
                        }
                    }
                    return TokenManager.GenerateToken(usersList.Where(u => u.userId != 1));
                }
            }
        }

        //
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
                    var usersList = entity.users
                    .Select(u => new UserModel()
                    {
                        userId = u.userId,
                        username = u.username,
                        //  password = u.password,
                        name = u.name,
                        lastname = u.lastname,
                        fullName = u.name + " " + u.lastname,
                        job = u.job,
                        workHours = u.workHours,
                        createDate = u.createDate,
                        updateDate = u.updateDate,
                        createUserId = u.createUserId,
                        updateUserId = u.updateUserId,
                        phone = u.phone,
                        mobile = u.mobile,
                        email = u.email,
                        notes = u.notes,
                        address = u.address,
                        isActive = u.isActive,
                        isOnline = u.isOnline,
                        image = u.image,
                        balance = u.balance,
                        balanceType = u.balanceType,
                        isAdmin = u.isAdmin,
                        groupId = u.groupId,
                        groupName = entity.groups.Where(g => g.groupId == u.groupId).FirstOrDefault().name,
                        hasCommission = u.hasCommission,
                        commissionValue = u.commissionValue,
                        commissionRatio = u.commissionRatio,
                    })
                    .ToList();

                    return TokenManager.GenerateToken(usersList);
                }
            }
        }

        [HttpPost]
        [Route("GetUserSettings")]
        public string GetUserSettings(string token)
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
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    string result = "{";

                    //get all settings

                    var settingsCls = entity.setting.ToList();
                    var settingsValues = entity.setValues.ToList();
                    UserSettings usersetModel = new UserSettings();
                    #region get user language 
                    var set = settingsCls.Where(l => l.name == "language").FirstOrDefault();

                    var lang = (from c in entity.setValues.Where(x => x.settingId == set.settingId)
                                join us in entity.userSetValues.Where(x => x.userId == userId) on c.valId equals us.valId
                                select new
                                {
                                    c.valId,
                                    c.value,
                                    c.isDefault,
                                    c.isSystem,
                                    c.notes,
                                    c.settingId,

                                }).FirstOrDefault();

                    string langVal = "";
                    if (lang == null)
                        langVal = "en";
                    else
                        langVal = lang.value;
                    
                    result += "userLang:'" + langVal + "'";
                    usersetModel.userLang = langVal;
                    #endregion

                    #region menuOpen
                    set = settingsCls.Where(l => l.name == "menuIsOpen").FirstOrDefault();
                    var menu = (from c in entity.setValues.Where(x => x.settingId == set.settingId)
                                join us in entity.userSetValues.Where(x => x.userId == userId) on c.valId equals us.valId
                                select new
                                {
                                    c.valId,
                                    c.value,
                                    c.isDefault,
                                    c.isSystem,
                                    c.notes,
                                    c.settingId,

                                }).FirstOrDefault();

                    string menuVal = "";
                    if (menu == null)
                        menuVal = "close";
                    else
                        menuVal = menu.value;

                    result += ",UserMenu:'" + menuVal + "'";
                    usersetModel.UserMenu = menuVal;
                    #endregion
                    #region invoiceSlice
                    var slice = entity.userSetValues.Where(x => x.userId == userId && x.note.StartsWith("invoice_slice")).FirstOrDefault();

                    string sliceVal = "0";
                    if (slice != null)
                        sliceVal = slice.Value;

                    result += ",invoiceSlice:'" + sliceVal + "'";
                    usersetModel.invoiceSlice = int.Parse(sliceVal);
                    #endregion
                    #region user path
                    string firstPath = "";
                    int? firstId = null;
                    int? secondId = null;
                    string secondPath = "";
                    set = settingsCls.Where(l => l.name == "user_path").FirstOrDefault();
                    var setPath = settingsValues.Where(x => x.settingId == set.settingId).ToList();
                    if (setPath.Count > 0)
                    {
                        firstId = setPath[0].valId;
                        secondId = setPath[1].valId;
                        var first = entity.userSetValues.Where(x => x.userId == userId && x.valId == firstId).ToList();
                        var second = entity.userSetValues.Where(x => x.userId == userId && x.valId == secondId).ToList();
                        if (first.Count > 0 && second.Count > 0)
                        {
                            firstPath = first.FirstOrDefault().note;
                            secondPath = second.FirstOrDefault().note;
                        }
                    }

                    result += ",firstPathId:" + firstId + ",firstPath:'" + firstPath + "',secondPathId:" + secondId + ",secondPath:'" + secondPath + "'";
                    usersetModel.firstPathId = firstId;
                    usersetModel.firstPath = firstPath;
                    usersetModel.secondPathId = secondId;
                    usersetModel.secondPath = secondPath;
                    #endregion
                    #region user Last message
                    adminMessagesController ac = new adminMessagesController();
                    var message = ac.GetLastMessageByUserId(userId, posId);
                    if (message != null)
                    {
                        result += ",messageContent:'" + message.msgContent + "'";
                        result += ",messageTitle:'" + message.title + "'";
                        //
                        usersetModel.messageContent = message.msgContent;
                        usersetModel.messageTitle = message.title;
                    }
                    else
                    {
                        result += ",messageContent:''";
                        result += ",messageTitle:''";
                        usersetModel.messageContent = "";
                        usersetModel.messageTitle = "";
                    }
                    #endregion

                    #region default system info
                    List<char> charsToRemove = new List<char>() { '@', '_', ',', '.', '-' };

                    //company name
                    set = settingsCls.Where(s => s.name == "com_name").FirstOrDefault();
                    int settingId = set.settingId;
                    var setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    string val = "";

                    if (setVal != null)
                        val = setVal.value;
                    result += ",companyName:'" + val + "'";
                    usersetModel.companyName = val;

                    //company address
                    set = settingsCls.Where(s => s.name == "com_address").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                        val = setVal.value;
                    result += ",Address:'" + val + "'";
                    usersetModel.Address = val;
                    //company email
                    set = settingsCls.Where(s => s.name == "com_email").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                        val = setVal.value;
                    result += ",Email:'" + val + "'";
                    usersetModel.Email = val;
                    //get company mobile
                    set = settingsCls.Where(s => s.name == "com_mobile").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                    {
                        charsToRemove.ForEach(x => setVal.value = setVal.value.Replace(x.ToString(), String.Empty));
                        val = setVal.value;
                    }
                    result += ",Mobile:'" + val + "'";
                    usersetModel.Mobile = val;
                    //get company phone
                    set = settingsCls.Where(s => s.name == "com_phone").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                    {
                        charsToRemove.ForEach(x => setVal.value = setVal.value.Replace(x.ToString(), String.Empty));
                        val = setVal.value;
                    }
                    result += ",Phone:'" + val + "'";
                    usersetModel.Phone = val;
                    //get company fax
                    set = settingsCls.Where(s => s.name == "com_fax").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                    {
                        charsToRemove.ForEach(x => setVal.value = setVal.value.Replace(x.ToString(), String.Empty));
                        val = setVal.value;
                    }
                    result += ",Fax:'" + val + "'";
                    usersetModel.Fax = val;
                    //get company logo
                    set = settingsCls.Where(s => s.name == "com_logo").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                        val = setVal.value;
                    result += ",logoImage:'" + val + "'";
                    usersetModel.logoImage = val;
                    #endregion
                    #region social
                    set = settingsCls.Where(s => s.name == "com_website").FirstOrDefault();
                     settingId = set.settingId;
                      setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                   val = "";

                    if (setVal != null)
                        val = setVal.value;
                   // result += ",com_website:'" + val + "'";
                    usersetModel.com_website = val;
                    //
                    set = settingsCls.Where(s => s.name == "com_social").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                        val = setVal.value;
                    // result += ",com_website:'" + val + "'";
                    usersetModel.com_social = val;
                    //
                    set = settingsCls.Where(s => s.name == "com_social_icon").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                        val = setVal.value;
                    // result += ",com_website:'" + val + "'";
                    usersetModel.com_social_icon = val;
                    #endregion
                    #region tax
                    var oneSet = settingsCls.Where(s => s.name == "invoiceTax_bool").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "false";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",invoiceTax_bool:'" + val + "'";
                    usersetModel.invoiceTax_bool = bool.Parse(val);
                    oneSet = settingsCls.Where(s => s.name == "invoiceTax_decimal").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "0";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",invoiceTax_decimal:" + val;
                    usersetModel.invoiceTax_decimal = decimal.Parse(val);
                    oneSet = settingsCls.Where(s => s.name == "itemsTax_bool").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "false";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",itemsTax_bool:'" + val + "'";
                    usersetModel.itemsTax_bool = bool.Parse(val);

                    oneSet = settingsCls.Where(s => s.name == "itemsTax_decimal").FirstOrDefault();
                    setVal = settingsValues.Where(i => i.settingId == oneSet.settingId).FirstOrDefault();
                    val = "0";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",itemsTax_decimal:'" + val + "'";
                    usersetModel.itemsTax_decimal = decimal.Parse(val);
 
                    #endregion
                    #region get print settings
                    var printList = entity.setValues.ToList().Where(x => x.notes == "print")
                            .Select(X => new
                            {
                                X.valId,
                                X.value,
                                X.isDefault,
                                X.isSystem,
                                X.settingId,
                                X.notes,
                                name = entity.setting.ToList().Where(s => s.settingId == X.settingId).FirstOrDefault().name,

                            })
                            .ToList();


                    var psetVal = printList.Where(X => X.name == "sale_copy_count").FirstOrDefault();
                    val = "0";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",sale_copy_count:'" + val + "'";
                    usersetModel.sale_copy_count = val;
                    psetVal = printList.Where(X => X.name == "pur_copy_count").FirstOrDefault();
                    val = "0";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",pur_copy_count:'" + val + "'";
                    usersetModel.pur_copy_count = val;
                    psetVal = printList.Where(X => X.name == "print_on_save_sale").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",print_on_save_sale:'" + val + "'";
                    usersetModel.print_on_save_sale = val;
                    psetVal = printList.Where(X => X.name == "print_on_save_pur").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",print_on_save_pur:'" + val + "'";
                    usersetModel.print_on_save_pur = val;
                    psetVal = printList.Where(X => X.name == "email_on_save_sale").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",email_on_save_sale:'" + val + "'";
                    usersetModel.email_on_save_sale = val;
                    psetVal = printList.Where(X => X.name == "email_on_save_pur").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",email_on_save_pur:'" + val + "'";
                    usersetModel.email_on_save_pur = val;
                    psetVal = printList.Where(X => X.name == "rep_copy_count").FirstOrDefault();
                    val = "0";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",rep_print_count:'" + val + "'";
                    usersetModel.rep_print_count = val;
                    psetVal = printList.Where(X => X.name == "Allow_print_inv_count").FirstOrDefault();
                    val = "0";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",Allow_print_inv_count:'" + val + "'";
                    usersetModel.Allow_print_inv_count = val;
                    psetVal = printList.Where(X => X.name == "show_header").FirstOrDefault();
                    val = "1";
                    if (psetVal != null)
                    {
                        val = psetVal.value;
                        if (val == null || val == "")
                        {
                            val = "1";
                        }
                    }
                    result += ",show_header:'" + val + "'";
                    usersetModel.show_header = val;
                    psetVal = printList.Where(X => X.name == "itemtax_note").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",itemtax_note:'" + val + "'";
                    usersetModel.itemtax_note = val;
                    psetVal = printList.Where(X => X.name == "sales_invoice_note").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",sales_invoice_note:'" + val + "'";
                    usersetModel.sales_invoice_note = val;
                    psetVal = printList.Where(X => X.name == "print_on_save_directentry").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",print_on_save_directentry:'" + val + "'";
                    usersetModel.print_on_save_directentry = val;
                    psetVal = printList.Where(X => X.name == "directentry_copy_count").FirstOrDefault();
                    val = "0";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",directentry_copy_count:'" + val + "'";
                    usersetModel.directentry_copy_count = val;

                    //report language
                    oneSet = settingsCls.Where(s => s.name == "report_lang").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId && i.isDefault == 1).FirstOrDefault().value;

                    if (val.Equals(""))
                        val = "en";
                    result += ",Reportlang:'" + val + "'";
                    usersetModel.Reportlang = val;
                    #endregion


                    #region accuracy - date form
                    oneSet = settingsCls.Where(s => s.name == "accuracy").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId && i.isDefault == 1).FirstOrDefault();
                    val = "0";
                    if (setVal != null)
                    {
                        val = setVal.value;
                        if (val.Equals(""))
                            val = "0";
                    }
                    result += ",accuracy:'" + val + "'";
                    usersetModel.accuracy = val;
                    //date form
                    oneSet = settingsCls.Where(s => s.name == "dateForm").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId && i.isDefault == 1).FirstOrDefault();
                    val = "";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",dateFormat:'" + val + "'";
                    usersetModel.dateFormat = val;
                    //currency info
                    var regions = entity.countriesCodes.Where(x => x.isDefault == 1).FirstOrDefault();
                    if (regions == null)
                    {
                        result += ",Currency:''" + ",CurrencyId:,countryId:";
                        usersetModel.Currency = "";
                        usersetModel.CurrencyId = 0;
                        usersetModel.countryId = 0;
                    }                      
                    else
                    {
                        result += ",Currency:'" + regions.currency + "'" + ",CurrencyId:" + regions.currencyId + ",countryId:" + regions.countryId;
                        usersetModel.Currency = regions.currency;
                        usersetModel.CurrencyId = regions.currencyId;
                        usersetModel.countryId = regions.countryId;
                    }                    
                    #endregion


                    #region storage cost
                    oneSet = settingsCls.Where(s => s.name == "storage_cost").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault().value;

                    if (val == "" || val == null)
                        val = "0";
                    result += ",StorageCost:" + val;
                    usersetModel.StorageCost = decimal.Parse(val);
                    #endregion
                    #region activationSite
                    oneSet = settingsCls.Where(s => s.name == "active_site").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault().value;


                    result += ",activationSite:'" + val + "'";
                    usersetModel.activationSite = val;

                    #endregion
                    #region invoice_lang
                    oneSet = settingsCls.Where(s => s.name == "invoice_lang").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault().value;
                    result += ",invoice_lang:'" + val + "'";
                    usersetModel.invoice_lang = val;
                    #endregion
                    #region com_name_ar
                    oneSet = settingsCls.Where(s => s.name == "com_name_ar").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault().value;
                    result += ",com_name_ar:'" + val + "'";
                    usersetModel.com_name_ar = val;
                    #endregion
                    #region com_address_ar
                    oneSet = settingsCls.Where(s => s.name == "com_address_ar").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault().value;
                    result += ",com_address_ar:'" + val + "'";
                    usersetModel.com_address_ar = val;
                    #endregion
                    #region Properties
                    //canSkipProperties
                    oneSet = settingsCls.Where(s => s.name == "canSkipProperties").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "false";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",canSkipProperties:'" + val + "'";
                    usersetModel.canSkipProperties = bool.Parse(val);
                    //canSkipSerialsNum
                    oneSet = settingsCls.Where(s => s.name == "canSkipSerialsNum").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "false";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",canSkipSerialsNum:'" + val + "'";
                    usersetModel.canSkipSerialsNum = bool.Parse(val);
                    #endregion

                    #region returnPeriod
                    oneSet = settingsCls.Where(s => s.name == "returnPeriod").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault().value;

                    if (val == "" || val == null)
                        val = "0";
                    result += ",returnPeriod:" + val;
                    usersetModel.returnPeriod = int.Parse(val);
                    #endregion
                    #region freeDelivery
                    oneSet = settingsCls.Where(s => s.name == "freeDelivery").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "false";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",freeDelivery:'" + val + "'";
                    usersetModel.freeDelivery = bool.Parse(val);
                    #endregion
                    result += "}";
                    return TokenManager.GenerateToken(usersetModel);
                    // return TokenManager.GenerateToken(result);
                }
            }
        }

        [HttpPost]
        [Route("GetSettings")]
        public string GetSettings(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);

            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                UserSettings usersetModel = new UserSettings();
                usersetModel = GetSettings();
                return TokenManager.GenerateToken(usersetModel);
            }
        }

        public UserSettings GetSettings( )
        {
            
                using (incposdbEntities entity = new incposdbEntities())
                {
                    string result = "{";

                    //get all settings

                    var settingsCls = entity.setting.ToList();
                    var settingsValues = entity.setValues.ToList();
                    UserSettings usersetModel = new UserSettings();
                   

                
             
             
                   

                    #region default system info
                    List<char> charsToRemove = new List<char>() { '@', '_', ',', '.', '-' };

                    //company name
                  var  set = settingsCls.Where(s => s.name == "com_name").FirstOrDefault();
                    int settingId = set.settingId;
                    var setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    string val = "";

                    if (setVal != null)
                        val = setVal.value;
                    result += ",companyName:'" + val + "'";
                    usersetModel.companyName = val;

                    //company address
                    set = settingsCls.Where(s => s.name == "com_address").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                        val = setVal.value;
                    result += ",Address:'" + val + "'";
                    usersetModel.Address = val;
                    //company email
                    set = settingsCls.Where(s => s.name == "com_email").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                        val = setVal.value;
                    result += ",Email:'" + val + "'";
                    usersetModel.Email = val;
                    //get company mobile
                    set = settingsCls.Where(s => s.name == "com_mobile").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                    {
                        charsToRemove.ForEach(x => setVal.value = setVal.value.Replace(x.ToString(), String.Empty));
                        val = setVal.value;
                    }
                    result += ",Mobile:'" + val + "'";
                    usersetModel.Mobile = val;
                    //get company phone
                    set = settingsCls.Where(s => s.name == "com_phone").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                    {
                        charsToRemove.ForEach(x => setVal.value = setVal.value.Replace(x.ToString(), String.Empty));
                        val = setVal.value;
                    }
                    result += ",Phone:'" + val + "'";
                    usersetModel.Phone = val;
                    //get company fax
                    set = settingsCls.Where(s => s.name == "com_fax").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                    {
                        charsToRemove.ForEach(x => setVal.value = setVal.value.Replace(x.ToString(), String.Empty));
                        val = setVal.value;
                    }
                    result += ",Fax:'" + val + "'";
                    usersetModel.Fax = val;
                    //get company logo
                    set = settingsCls.Where(s => s.name == "com_logo").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                        val = setVal.value;
                    result += ",logoImage:'" + val + "'";
                    usersetModel.logoImage = val;
                    #endregion
                    #region social
                    set = settingsCls.Where(s => s.name == "com_website").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                        val = setVal.value;
                    // result += ",com_website:'" + val + "'";
                    usersetModel.com_website = val;
                    //
                    set = settingsCls.Where(s => s.name == "com_social").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                        val = setVal.value;
                    // result += ",com_website:'" + val + "'";
                    usersetModel.com_social = val;
                    //
                    set = settingsCls.Where(s => s.name == "com_social_icon").FirstOrDefault();
                    settingId = set.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "";

                    if (setVal != null)
                        val = setVal.value;
                    // result += ",com_website:'" + val + "'";
                    usersetModel.com_social_icon = val;
                    #endregion
                    #region 
                    var oneSet = settingsCls.Where(s => s.name == "invoiceTax_bool").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "false";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",invoiceTax_bool:'" + val + "'";
                    usersetModel.invoiceTax_bool = bool.Parse(val);
                    oneSet = settingsCls.Where(s => s.name == "invoiceTax_decimal").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "0";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",invoiceTax_decimal:" + val;
                    usersetModel.invoiceTax_decimal = decimal.Parse(val);
                    oneSet = settingsCls.Where(s => s.name == "itemsTax_bool").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "false";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",itemsTax_bool:'" + val + "'";
                    usersetModel.itemsTax_bool = bool.Parse(val);

                    #endregion
                    #region get print settings
                    var printList = entity.setValues.ToList().Where(x => x.notes == "print")
                            .Select(X => new
                            {
                                X.valId,
                                X.value,
                                X.isDefault,
                                X.isSystem,
                                X.settingId,
                                X.notes,
                                name = entity.setting.ToList().Where(s => s.settingId == X.settingId).FirstOrDefault().name,

                            })
                            .ToList();


                    var psetVal = printList.Where(X => X.name == "sale_copy_count").FirstOrDefault();
                    val = "0";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",sale_copy_count:'" + val + "'";
                    usersetModel.sale_copy_count = val;
                    psetVal = printList.Where(X => X.name == "pur_copy_count").FirstOrDefault();
                    val = "0";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",pur_copy_count:'" + val + "'";
                    usersetModel.pur_copy_count = val;
                    psetVal = printList.Where(X => X.name == "print_on_save_sale").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",print_on_save_sale:'" + val + "'";
                    usersetModel.print_on_save_sale = val;
                    psetVal = printList.Where(X => X.name == "print_on_save_pur").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",print_on_save_pur:'" + val + "'";
                    usersetModel.print_on_save_pur = val;
                    psetVal = printList.Where(X => X.name == "email_on_save_sale").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",email_on_save_sale:'" + val + "'";
                    usersetModel.email_on_save_sale = val;
                    psetVal = printList.Where(X => X.name == "email_on_save_pur").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",email_on_save_pur:'" + val + "'";
                    usersetModel.email_on_save_pur = val;
                    psetVal = printList.Where(X => X.name == "rep_copy_count").FirstOrDefault();
                    val = "0";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",rep_print_count:'" + val + "'";
                    usersetModel.rep_print_count = val;
                    psetVal = printList.Where(X => X.name == "Allow_print_inv_count").FirstOrDefault();
                    val = "0";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",Allow_print_inv_count:'" + val + "'";
                    usersetModel.Allow_print_inv_count = val;
                    psetVal = printList.Where(X => X.name == "show_header").FirstOrDefault();
                    val = "1";
                    if (psetVal != null)
                    {
                        val = psetVal.value;
                        if (val == null || val == "")
                        {
                            val = "1";
                        }
                    }
                    result += ",show_header:'" + val + "'";
                    usersetModel.show_header = val;
                    psetVal = printList.Where(X => X.name == "itemtax_note").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",itemtax_note:'" + val + "'";
                    usersetModel.itemtax_note = val;
                    psetVal = printList.Where(X => X.name == "sales_invoice_note").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",sales_invoice_note:'" + val + "'";
                    usersetModel.sales_invoice_note = val;
                    psetVal = printList.Where(X => X.name == "print_on_save_directentry").FirstOrDefault();
                    val = "";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",print_on_save_directentry:'" + val + "'";
                    usersetModel.print_on_save_directentry = val;
                    psetVal = printList.Where(X => X.name == "directentry_copy_count").FirstOrDefault();
                    val = "0";
                    if (psetVal != null)
                        val = psetVal.value;
                    result += ",directentry_copy_count:'" + val + "'";
                    usersetModel.directentry_copy_count = val;

                    //report language
                    oneSet = settingsCls.Where(s => s.name == "report_lang").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId && i.isDefault == 1).FirstOrDefault().value;

                    if (val.Equals(""))
                        val = "en";
                    result += ",Reportlang:'" + val + "'";
                    usersetModel.Reportlang = val;
                    #endregion


                    #region accuracy - date form
                    oneSet = settingsCls.Where(s => s.name == "accuracy").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId && i.isDefault == 1).FirstOrDefault();
                    val = "0";
                    if (setVal != null)
                    {
                        val = setVal.value;
                        if (val.Equals(""))
                            val = "0";
                    }
                    result += ",accuracy:'" + val + "'";
                    usersetModel.accuracy = val;
                    //date form
                    oneSet = settingsCls.Where(s => s.name == "dateForm").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId && i.isDefault == 1).FirstOrDefault();
                    val = "";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",dateFormat:'" + val + "'";
                    usersetModel.dateFormat = val;
                    //currency info
                    var regions = entity.countriesCodes.Where(x => x.isDefault == 1).FirstOrDefault();
                    if (regions == null)
                    {
                        result += ",Currency:''" + ",CurrencyId:,countryId:";
                        usersetModel.Currency = "";
                        usersetModel.CurrencyId = 0;
                        usersetModel.countryId = 0;
                    }
                    else
                    {
                        result += ",Currency:'" + regions.currency + "'" + ",CurrencyId:" + regions.currencyId + ",countryId:" + regions.countryId;
                        usersetModel.Currency = regions.currency;
                        usersetModel.CurrencyId = regions.currencyId;
                        usersetModel.countryId = regions.countryId;
                    }
                    #endregion


                    #region storage cost
                    oneSet = settingsCls.Where(s => s.name == "storage_cost").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault().value;

                    if (val == "" || val == null)
                        val = "0";
                    result += ",StorageCost:" + val;
                    usersetModel.StorageCost = decimal.Parse(val);
                    #endregion
                    #region activationSite
                    oneSet = settingsCls.Where(s => s.name == "active_site").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault().value;


                    result += ",activationSite:'" + val + "'";
                    usersetModel.activationSite = val;

                    #endregion
                    #region invoice_lang
                    oneSet = settingsCls.Where(s => s.name == "invoice_lang").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault().value;
                    result += ",invoice_lang:'" + val + "'";
                    usersetModel.invoice_lang = val;
                    #endregion
                    #region com_name_ar
                    oneSet = settingsCls.Where(s => s.name == "com_name_ar").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault().value;
                    result += ",com_name_ar:'" + val + "'";
                    usersetModel.com_name_ar = val;
                    #endregion
                    #region com_address_ar
                    oneSet = settingsCls.Where(s => s.name == "com_address_ar").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault().value;
                    result += ",com_address_ar:'" + val + "'";
                    usersetModel.com_address_ar = val;
                    #endregion
                    #region Properties
                    //canSkipProperties
                    oneSet = settingsCls.Where(s => s.name == "canSkipProperties").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "false";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",canSkipProperties:'" + val + "'";
                    usersetModel.canSkipProperties = bool.Parse(val);
                    //canSkipSerialsNum
                    oneSet = settingsCls.Where(s => s.name == "canSkipSerialsNum").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "false";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",canSkipSerialsNum:'" + val + "'";
                    usersetModel.canSkipSerialsNum = bool.Parse(val);
                    #endregion

                    #region returnPeriod
                    oneSet = settingsCls.Where(s => s.name == "returnPeriod").FirstOrDefault();
                    settingId = oneSet.settingId;
                    val = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault().value;

                    if (val == "" || val == null)
                        val = "0";
                    result += ",returnPeriod:" + val;
                    usersetModel.returnPeriod = int.Parse(val);
                    #endregion
                    #region freeDelivery
                    oneSet = settingsCls.Where(s => s.name == "freeDelivery").FirstOrDefault();
                    settingId = oneSet.settingId;
                    setVal = settingsValues.Where(i => i.settingId == settingId).FirstOrDefault();
                    val = "false";
                    if (setVal != null)
                        val = setVal.value;
                    result += ",freeDelivery:'" + val + "'";
                    usersetModel.freeDelivery = bool.Parse(val);
                    #endregion
                    result += "}";
                    return usersetModel;
                    // return TokenManager.GenerateToken(result);
                }
        
        }
        // GET api/<controller>
        [HttpPost]
        [Route("GetUserByID")]
        public string GetUserByID(string token)
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
                var user = GetUserByID(userId);
                return TokenManager.GenerateToken(user);

            }
        }

        public UserModel GetUserByID(int userId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var user = entity.users
               .Where(u => u.userId == userId)
               .Select(u => new UserModel()
               {
                  userId = u.userId,
                  username = u.username,
                   password = u.password,
                   name=  u.name,
                   lastname = u.lastname,
                   job = u.job,
                   workHours= u.workHours,
                   createDate=  u.createDate,
                   updateDate = u.updateDate,
                   createUserId =  u.createUserId,
                   updateUserId = u.updateUserId,
                   phone = u.phone,
                   mobile = u.mobile,
                   email = u.email,
                   notes = u.notes,
                   address = u.address,
                   isOnline = u.isOnline,
                   image = u.image,
                   isActive = u.isActive,
                   balance = u.balance,
                   balanceType = u.balanceType,
                   isAdmin = u.isAdmin,
                   fullName = u.name + " " + u.lastname,
                   groupId = u.groupId,
                   groupName = entity.groups.Where(g => g.groupId == u.groupId).FirstOrDefault().name,
                   hasCommission = u.hasCommission,
                   commissionValue =  u.commissionValue,
                   commissionRatio = u.commissionRatio,
               })
               .FirstOrDefault();
                return user;
            }
        }
        [HttpPost]
        [Route("editUserBalance")]
        public string editUserBalance(string token)
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
                decimal amount = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
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
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var user = entity.users.Find(userId);

                        if (user.balanceType == 0)
                        {
                            if (amount > user.balance)
                            {
                                amount -= (decimal)user.balance;
                                user.balance = amount;
                                user.balanceType = 1;
                            }
                            else
                                user.balance -= amount;
                        }
                        else
                        {
                            user.balance += amount;
                        }

                        entity.SaveChanges();
                    }
                    return TokenManager.GenerateToken("1");

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }


        [HttpPost]
        [Route("GetSalesMan")]
        public string GetSalesMan(string token)
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
                string deliveryPermission = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "branchId")
                    {
                        branchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "deliveryPermission")
                    {
                        deliveryPermission = c.Value;
                    }
                }
                List<UserModel> users = new List<UserModel>();
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var usersList = (from u in entity.users.Where(us => us.isActive == 1 && us.userId != 1)
                                     join bu in entity.branchesUsers on u.userId equals bu.userId
                                     where bu.branchId == branchId
                                     select new UserModel
                                     {
                                         userId = u.userId,
                                         username = u.username,
                                         name = u.name,
                                         lastname = u.lastname,
                                         fullName = u.name + " " + u.lastname,
                                         mobile = u.mobile,
                                         balance = u.balance,
                                         balanceType = u.balanceType,
                                         isAdmin = u.isAdmin,
                                         groupId = u.groupId,
                                         groupName = entity.groups.Where(g => g.groupId == u.groupId).FirstOrDefault().name,
                                         hasCommission = u.hasCommission,
                                         commissionValue = u.commissionValue,
                                         commissionRatio = u.commissionRatio,
                                     }).ToList();

                    foreach (UserModel user in usersList)
                    {
                        var groupObjects = (from GO in entity.groupObject
                                            where GO.showOb == 1 && GO.objects.name.Contains(deliveryPermission)
                                            join U in entity.users on GO.groupId equals U.groupId
                                            where U.userId == user.userId
                                            select new
                                            {
                                                //group object
                                                GO.id,
                                                GO.showOb,

                                            }).FirstOrDefault();

                        if (groupObjects != null)
                            users.Add(user);
                    }
                    return TokenManager.GenerateToken(users);
                }
            }
        }


        // add or update unit
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
                string userObject = "";
                users userObj = null;
                users newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        userObject = c.Value.Replace("\\", string.Empty);
                        userObject = userObject.Trim('"');
                        newObject = JsonConvert.DeserializeObject<users>(userObject, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
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
                        var userEntity = entity.Set<users>();
                        //var catEntity = entity.Set<categoryuser>();
                        if (newObject.userId == 0)
                        {
                            newObject.isAdmin = false;

                            ProgramInfo programInfo = new ProgramInfo();
                            int userMaxCount = programInfo.getUserCount();
                            int usersCount = entity.users.Count();
                            if (usersCount >= userMaxCount && userMaxCount != -1)
                            {
                                message = "-1";
                                return TokenManager.GenerateToken(message);
                            }
                            else
                            {
                                newObject.createDate = cc.AddOffsetTodate(DateTime.Now);
                                newObject.updateDate = cc.AddOffsetTodate(DateTime.Now);
                                newObject.updateUserId = newObject.createUserId;
                                newObject.balance = 0;
                                newObject.balanceType = 0;
                                userObj = userEntity.Add(newObject);
                                // get all categories
                                //var categories = entity.categories.Where(x => x.isActive == 1).Select(x => x.categoryId).ToList();
                                //int sequence = 0;
                                //for (int i = 0; i < categories.Count; i++)
                                //{
                                //    sequence++;
                                //    int categoryId = categories[i];
                                //    categoryuser cu = new categoryuser()
                                //    {
                                //        categoryId = categoryId,
                                //        userId = userObj.userId,
                                //        sequence = sequence,
                                //        createDate = cc.AddOffsetTodate(DateTime.Now),
                                //        updateDate = cc.AddOffsetTodate(DateTime.Now),
                                //        createUserId = newObject.createUserId,
                                //        updateUserId = newObject.updateUserId,
                                //    };
                                //    catEntity.Add(cu);
                                //}
                                entity.SaveChanges().ToString();
                                message = userObj.userId.ToString();
                                return TokenManager.GenerateToken(message);

                            }
                        }
                        else
                        {
                            userObj = entity.users.Where(p => p.userId == newObject.userId).FirstOrDefault();
                            userObj.name = newObject.name;
                            userObj.username = newObject.username;
                            userObj.password = newObject.password;
                            userObj.name = newObject.name;
                            userObj.lastname = newObject.lastname;
                            userObj.job = newObject.job;
                            userObj.workHours = newObject.workHours;
                            userObj.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            userObj.updateUserId = newObject.updateUserId;
                            userObj.phone = newObject.phone;
                            userObj.mobile = newObject.mobile;
                            userObj.email = newObject.email;
                            userObj.notes = newObject.notes;
                            userObj.address = newObject.address;
                            userObj.isActive = newObject.isActive;
                            userObj.balance = newObject.balance;
                            userObj.balanceType = newObject.balanceType;
                            userObj.isOnline = newObject.isOnline;
                            userObj.groupId = newObject.groupId;
                            userObj.hasCommission = newObject.hasCommission;
                            userObj.commissionValue = newObject.commissionValue;
                            userObj.commissionRatio = newObject.commissionRatio;



                            entity.SaveChanges().ToString();
                            message = userObj.userId.ToString();
                            return TokenManager.GenerateToken(message);

                        }
                    }
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                    // return TokenManager.GenerateToken(ex.ToString());
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
                int delUserId = 0;
                int userId = 0;
                Boolean final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "delUserId")
                    {
                        delUserId = int.Parse(c.Value);
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
                         
                            users usersDelete = entity.users.Find(delUserId);
                            entity.users.Remove(usersDelete);
                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);

                        }
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }
                else
                {
                    try
                    {
                        using (incposdbEntities entity = new incposdbEntities())
                        {
                            users userDelete = entity.users.Find(delUserId);

                            userDelete.isActive = 0;
                            userDelete.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            userDelete.updateUserId = userId;
                            message = entity.SaveChanges().ToString();
                            return TokenManager.GenerateToken(message);

                        }
                    }
                    catch
                    {
                        return TokenManager.GenerateToken("0");
                    }
                }

            }

        }
        [Route("PostUserImage")]
        public IHttpActionResult PostUserImage()
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

                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png", ".bmp", ".jpeg", ".tiff", ".jfif" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();

                        if (!AllowedFileExtensions.Contains(extension))
                        {

                            var message = string.Format("Please Upload image of type .jpg,.gif,.png, .jfif, .bmp , .jpeg ,.tiff");
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
                            var pathCheck = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\user"), imageWithNoExt);
                            var files = Directory.GetFiles(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\user"), imageWithNoExt + ".*");
                            if (files.Length > 0)
                            {
                                File.Delete(files[0]);
                            }

                            //Userimage myfolder name where i want to save my image
                            var filePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\user"), imageName);
                            postedFile.SaveAs(filePath);

                        }
                    }

                    var message1 = string.Format("Image Updated Successfully.");
                    return Ok(message1);
                }
                var res = string.Format("Please Upload a image.");

                return Ok(res);
            }
            catch (Exception ex)
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

        //    localFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\user"), imageName);

        //    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
        //    response.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
        //    response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
        //    response.Content.Headers.ContentDisposition.FileName = imageName;

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
                    localFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\images\\user"), imageName);

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
        [Route("UpdateImage")]
        public string UpdateImage(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            string message = "";
            var re = Request;
            var headers = re.Headers;
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string userObject = "";
                users userObj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        userObject = c.Value.Replace("\\", string.Empty);
                        userObject = userObject.Trim('"');
                        userObj = JsonConvert.DeserializeObject<users>(userObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }
                try
                {
                    users user;
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var userEntity = entity.Set<users>();
                        user = entity.users.Where(p => p.userId == userObj.userId).First();
                        user.image = userObj.image;
                        entity.SaveChanges();
                    }
                    message = user.userId.ToString();
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
        [Route("CanLogIn")]
        public async Task<string> CanLogIn(string token)
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
                int userId = 0;
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
                }
                List<UserModel> users = new List<UserModel>();
                try
                {
                    int can = await CanLogIn(userId, posId);
                    return TokenManager.GenerateToken(can.ToString());

                    //using (incposdbEntities entity = new incposdbEntities())
                    //{
                    //    var usersList = (from bu in entity.branchesUsers
                    //                     join B in entity.branches on bu.branchId equals B.branchId
                    //                     join P in entity.pos on B.branchId equals P.branchId
                    //                     // from u in entity.users.Where(us => us.isActive == 1 || us.userId == 1)

                    //                     where P.posId == posId && bu.userId == userId
                    //                     select new
                    //                     {
                    //                         bu.branchsUsersId,
                    //                         bu.branchId,
                    //                         bu.userId,
                    //                     }).ToList();
                    //    int can = 0;
                    //    if(usersList==null|| usersList.Count == 0)
                    //    {
                    //        can = 0;
                    //    }
                    //    else
                    //    {
                    //        can = 1;
                    //    }

                    //    return TokenManager.GenerateToken(can.ToString());
                    //}
                }

                catch
                {
                    return TokenManager.GenerateToken("0");
                }
            }
        }
        public async Task<int> CanLogIn(int userId, int posId)
        {
            using (incposdbEntities entity = new incposdbEntities())
            {
                var usersList = (from bu in entity.branchesUsers
                                 join B in entity.branches on bu.branchId equals B.branchId
                                 join P in entity.pos on B.branchId equals P.branchId
                                 // from u in entity.users.Where(us => us.isActive == 1 || us.userId == 1)

                                 where P.posId == posId && bu.userId == userId
                                 select new
                                 {
                                     bu.branchsUsersId,
                                     bu.branchId,
                                     bu.userId,
                                 }).ToList();
                int can = 0;
                if (usersList == null || usersList.Count == 0)
                {
                    can = 0;
                }
                else
                {
                    can = 1;
                }

                return can;
            }
        }
        [HttpPost]
        [Route("checkLoginAvalability")]
        public string checkLoginAvalability(string token)
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
                int posId = 0;
                string userName = "";
                string password = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "deviceCode")
                    {
                        deviceCode = c.Value;
                    }
                    else if (c.Type == "posId")
                    {
                        posId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userName")
                    {
                        userName = c.Value;
                    }
                    else if (c.Type == "password")
                    {
                        password = c.Value;
                    }
                }
                int res = checkLoginAvalability(posId, deviceCode, userName, password);
                return TokenManager.GenerateToken(res.ToString());

            }
        }
        public int checkLoginAvalability(int posId, string deviceCode, string userName, string password)
        {
            // 1 :  can login-
            //  0 : error 
            // -1 : package is expired 
            // -2 : device code is not correct 
            // -3 : serial is not active 
            // -4 : customer server code is wrong
            // -5 : login date is before last login date

            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    //check support user
                    if (userName == "Support@Increase")
                    {
                        var suppUser = entity.users.Where(u => u.isActive == 1 && u.username == userName && u.password == password && u.isAdmin == true).FirstOrDefault();
                        if (suppUser != null)
                            return 1;
                    }
                    //compair login date with last login date for this user
                    var user = entity.users.Where(x => x.username == userName && x.password == password && x.isActive == 1).FirstOrDefault();
                    if (user != null)
                    {
                        var logs = entity.usersLogs.Where(x => x.userId == user.userId).OrderByDescending(x => x.sInDate).FirstOrDefault();
                        if (logs != null && logs.sInDate > cc.AddOffsetTodate(DateTime.Now))
                            return -5;
                    }
                    ActivateController ac = new ActivateController();
                    int active = ac.CheckPeriod();
                    if (active == 0)
                        return -1;
                    else
                    {
                        var tmpObject = entity.posSetting.Where(x => x.posId == posId).FirstOrDefault();
                        if (tmpObject != null)
                        {
                            // check customer code
                            if (tmpObject.posDeviceCode != deviceCode)
                            {
                                return -2;
                            }
                            //check customer server code
                            ProgramDetailsController pc = new ProgramDetailsController();
                            var programD = pc.getCustomerServerCode();
                            if (programD == null || programD.customerServerCode != ac.ServerID())
                            {
                                return -4;
                            }
                        }
                        // check serial && package avalilability
                        var serial = entity.posSetting.Where(x => x.posId == posId && x.posSerials.isActive == true).FirstOrDefault();
                        var programDetails = entity.ProgramDetails.Where(x => x.isActive == true).FirstOrDefault();
                        if (serial == null || programDetails == null)
                            return -3;
                    }

                    return 1;
                }
            }
            catch
            {

                return 0;

            }

        }

        [HttpPost]
        [Route("updateOnline")]
        public string updateOnline(string token)
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
                int userId = 0;
                users userObj = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var userEntity = entity.Set<users>();

                        if (userId > 0)
                        {
                            userObj = entity.users.Where(p => p.userId == userId).FirstOrDefault();
                            userObj.isOnline = 0;
                            entity.SaveChanges().ToString();
                            message = userObj.userId.ToString();
                            return TokenManager.GenerateToken(message);
                        }
                        else
                        {
                            return TokenManager.GenerateToken("0");
                        }
                    }
                }
                catch
                {
                    message = "0";
                    return TokenManager.GenerateToken(message);
                    // return TokenManager.GenerateToken(ex.ToString());
                }
            }
        }
    }
}

