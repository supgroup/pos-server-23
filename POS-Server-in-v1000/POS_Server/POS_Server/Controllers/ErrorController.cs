﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
using System.Web;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/errorcontroller")]
    public class ErrorController : ApiController
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
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var List = (from S in entity.error
                                select new ErrorModel()
                                {
                                    errorId = S.errorId,
                                    num = S.num,
                                    msg = S.msg,
                                    stackTrace = S.stackTrace,
                                    targetSite = S.targetSite,
                                    posId = S.posId,
                                    branchId = S.branchId,
                                    createDate = S.createDate,
                                    createUserId = S.createUserId,

                                    programNamePos = S.programNamePos,
                                    versionNamePos = S.versionNamePos,
                                    programNameServer = S.programNameServer,
                                    versionNameServer = S.versionNameServer,
                                    source = S.source,
                                    method = S.method,




                                }).ToList();
                    return TokenManager.GenerateToken(List);
                }
            }
        }
        [HttpPost]
        [Route("GetByPos")]
        public string GetByPos(string token)
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
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var List = (from S in entity.error
                                where S.posId == posId
                                select new ErrorModel()
                                {
                                    errorId = S.errorId,
                                    num = S.num,
                                    msg = S.msg,
                                    stackTrace = S.stackTrace,
                                    targetSite = S.targetSite,
                                    posId = S.posId,
                                    branchId = S.branchId,
                                    createDate = S.createDate,
                                    createUserId = S.createUserId,
                                    programNamePos = S.programNamePos,
                                    versionNamePos = S.versionNamePos,
                                    programNameServer = S.programNameServer,
                                    versionNameServer = S.versionNameServer,
                                    source = S.source,
                                    method = S.method,
                                }).ToList();
                    return TokenManager.GenerateToken(List);
                }
            }
        }
        // GET api/<controller>
        [HttpPost]
        [Route("GetByID")]
        public string GetByID(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int errorId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        errorId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var row = entity.error
                   .Where(u => u.errorId == errorId)
                   .Select(S => new
                   {
                       S.errorId,

                       S.num,
                       S.msg,
                       S.stackTrace,
                       S.targetSite,
                       S.posId,
                       S.branchId,
                       S.createDate,
                       S.createUserId,
                       S.programNamePos,
                       S.versionNamePos,
                       S.programNameServer,
                       S.versionNameServer,
                      S.source,
                       S.method,


                   })
                   .FirstOrDefault();

                    return TokenManager.GenerateToken(row);
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
                string Object = "";
                error newObject = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        Object = c.Value.Replace("\\", string.Empty);
                        Object = Object.Trim('"');
                        newObject = JsonConvert.DeserializeObject<error>(Object, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }

                if (newObject.createUserId == 0 || newObject.createUserId == null)
                {
                    Nullable<int> id = null;
                    newObject.createUserId = id;
                }
                if (newObject.posId == 0 || newObject.posId == null)
                {
                    Nullable<int> posId = null;
                    newObject.posId = posId;
                }
                if (newObject.branchId == 0 || newObject.branchId == null)
                {
                    Nullable<int> branchId = null;
                    newObject.branchId = branchId;
                }
                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var locationEntity = entity.Set<error>();
                        if (newObject.errorId == 0)
                        {
                            newObject.createDate = cc.AddOffsetTodate(DateTime.Now);
                            var serverdetail = entity.ProgramDetails.FirstOrDefault();
                            //var posseting = entity.posSetting.Where(p => p.posId == newObject.posId).FirstOrDefault();
                            //if (posseting == null)
                            //{
                            //    newObject.programNamePos = "";
                            //    newObject.versionNamePos = "";
                            //}
                            //else
                            //{

                            //}
                            newObject.programNameServer = serverdetail.programName;
                            newObject.versionNameServer = serverdetail.versionName;
                            locationEntity.Add(newObject);
                            entity.SaveChanges();
                            message = newObject.errorId.ToString();
                            return TokenManager.GenerateToken(message);
                        }
                        else
                        {
                            var tmpObject = entity.error.Where(p => p.errorId == newObject.errorId).FirstOrDefault();
                            var serverdetail = entity.ProgramDetails.FirstOrDefault();
                            //var posseting = entity.posSetting.Where(p => p.posId == newObject.posId).FirstOrDefault();
                            //if (posseting == null)
                            //{
                            //    tmpObject.programNamePos ="";
                            //    tmpObject.versionNamePos = "";
                            //}
                            //else
                            //{
                            tmpObject.programNamePos = newObject.programNamePos;
                            tmpObject.versionNamePos = newObject.versionNamePos;
                            //}
                            tmpObject.programNameServer = serverdetail.programName;
                            tmpObject.versionNameServer = serverdetail.versionName;
                            // newObject.createDate = cc.AddOffsetTodate(DateTime.Now);

                            tmpObject.num = newObject.num;
                            tmpObject.msg = newObject.msg;
                            tmpObject.stackTrace = newObject.stackTrace;
                            tmpObject.targetSite = newObject.targetSite;
                            tmpObject.posId = newObject.posId;
                            tmpObject.branchId = newObject.branchId;

                            tmpObject.source = newObject.source;
                            tmpObject.method = newObject.method;

                            tmpObject.createUserId = newObject.createUserId;
                            entity.SaveChanges();

                            message = tmpObject.errorId.ToString();
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
                try
                {
                    int errorId = 0;
                    IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                    foreach (Claim c in claims)
                    {
                        if (c.Type == "itemId")
                        {
                            errorId = int.Parse(c.Value);
                        }
                    }
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        error objectDelete = entity.error.Find(errorId);

                        entity.error.Remove(objectDelete);
                        message = entity.SaveChanges().ToString();

                        return TokenManager.GenerateToken(message);

                    }
                }
                catch
                {
                    message = "-1";
                    return TokenManager.GenerateToken(message);

                }


            }
        }
    }
}