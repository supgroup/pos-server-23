using Newtonsoft.Json;
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
using System.Data.Entity.Core.Objects;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/coupons")]
    public class couponsController : ApiController
    {
        CountriesController cc = new CountriesController();
        // GET api/<controller> get all coupons
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
                    var couponsList = entity.coupons

                   .Select(c => new CouponModel
                   {
                       cId = c.cId,
                       name = c.name,
                       code = c.code,
                       isActive = c.isActive,
                       discountType = c.discountType,
                       discountValue = c.discountValue,
                       startDate = c.startDate,
                       endDate = c.endDate,
                       notes = c.notes,
                       quantity = c.quantity,
                       remainQ = c.remainQ,
                       invMin = c.invMin,
                       invMax = c.invMax,
                       createDate = c.createDate,
                       updateDate = c.updateDate,
                       createUserId = c.createUserId,
                       updateUserId = c.updateUserId,
                       barcode = c.barcode,
                       details=c.details,
                   })
                   .ToList();

                    // can delet or not
                    if (couponsList.Count > 0)
                    {
                        foreach (CouponModel couponitem in couponsList)
                        {
                            canDelete = false;
                            if (couponitem.isActive == 1)
                            {
                                int cId = (int)couponitem.cId;
                                var copinv = entity.couponsInvoices.Where(x => x.couponId == cId).Select(x => new { x.id }).FirstOrDefault();

                                if ((copinv is null))
                                    canDelete = true;
                            }
                            couponitem.canDelete = canDelete;
                        }
                    }


                    return TokenManager.GenerateToken(couponsList);
                }
            }
        }
        [HttpPost]
        [Route("GetEffictive")]
        public string GetEffictive(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);

            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                DateTime datenow = DateTime.Now;
                datenow = cc.AddOffsetTodate(datenow);
                using (incposdbEntities entity = new incposdbEntities())
                {
                    /*
                     * var couponsList = entity.coupons.Where(x => (x.remainQ > 0 || x.quantity == 0) &&( (x.startDate <= DateTime.Now && x.endDate >= DateTime.Now)||x.endDate ==null )&& x.isActive == 1)
                     * */

                   // var couponsList = entity.coupons.Where(x => (x.remainQ > 0 || x.quantity == 0) && (EntityFunctions.TruncateTime(x.startDate) <= datenow.Date || x.startDate == null ) && (EntityFunctions.TruncateTime(x.endDate) >= datenow.Date || x.endDate == null)  && x.isActive == 1)
                        var couponsList = entity.coupons
                   .Select(c => new CouponModel
                   {
                       cId = c.cId,
                       name = c.name,
                       code = c.code,
                       isActive = c.isActive,
                       discountType = c.discountType,
                       discountValue = c.discountValue,
                       startDate = c.startDate,
                       endDate = c.endDate,
                       notes = c.notes,
                       quantity = c.quantity,
                       remainQ = c.remainQ,
                       invMin = c.invMin,
                       invMax = c.invMax,
                       createDate = c.createDate,
                       updateDate = c.updateDate,
                       createUserId = c.createUserId,
                       updateUserId = c.updateUserId,
                       details = c.details,
                       barcode = c.barcode,
                   })
                   .ToList();

                    couponsList = couponsList.Where(x => (x.remainQ > 0 || x.quantity == 0) && (x.startDate == null ? true : (x.startDate.Value.Date <= datenow.Date)) && (x.endDate == null ? true : (x.endDate.Value.Date >= datenow.Date)) && x.isActive == 1).ToList();
                    return TokenManager.GenerateToken(couponsList);
                }
            }
        }
        // GET api/<controller>  Get Coupon By ID 
        [HttpPost]
        [Route("GetCouponByID")]
        public string GetcouponByID(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int cId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        cId = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var coupon = entity.coupons
                   .Where(c => c.cId == cId)
                   .Select(c => new
                   {
                       c.cId,
                       c.name,
                       c.code,
                       c.isActive,
                       c.discountType,
                       c.discountValue,
                       c.startDate,
                       c.endDate,
                       c.notes,
                       c.quantity,
                       c.remainQ,
                       c.invMin,
                       c.invMax,
                       c.createDate,
                       c.updateDate,
                       c.createUserId,
                       c.updateUserId,
                       c.barcode,
                     c.details,
                   })
                   .FirstOrDefault();

                    return TokenManager.GenerateToken(coupon);
                }
            }
        }
        // GET api/<controller>  Get Coupon By Code 
        [HttpPost]
        [Route("GetCouponByCode")]
        public string GetCouponByCode(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string code = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        code = c.Value;
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var coupon = entity.coupons
                   .Where(c => c.code == code)
                   .Select(c => new
                   {
                       c.cId,
                       c.name,
                       c.code,
                       c.isActive,
                       c.discountType,
                       c.discountValue,
                       c.startDate,
                       c.endDate,
                       c.notes,
                       c.quantity,
                       c.remainQ,
                       c.invMin,
                       c.invMax,
                       c.createDate,
                       c.updateDate,
                       c.createUserId,
                       c.updateUserId,
                       c.barcode,
                     c.details,
                   })
                   .FirstOrDefault();

                    return TokenManager.GenerateToken(coupon);
                }
            }
        }
        // GET api/<controller>  Get Coupon By Barcode
        [HttpPost]
        [Route("GetCouponByBarcode")]
        public string GetcouponByBarcode(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string barcode = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        barcode = c.Value;
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var coupon = entity.coupons
                   .Where(c => c.barcode == barcode)
                   .Select(c => new
                   {
                       c.cId,
                       c.name,
                       c.code,
                       c.isActive,
                       c.discountType,
                       c.discountValue,
                       c.startDate,
                       c.endDate,
                       c.notes,
                       c.quantity,
                       c.remainQ,
                       c.invMin,
                       c.invMax,
                       c.createDate,
                       c.updateDate,
                       c.createUserId,
                       c.updateUserId,
                       c.barcode,
                    c.details,
                   })
                   .FirstOrDefault();

                    return TokenManager.GenerateToken(coupon);
                }
            }
        }
        // GET api/<controller>  Get Coupon By code
        [HttpPost]
        [Route("IsExistcode")]
        public string IsExistcode(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string code = "";
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        code = c.Value;
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var coupon = entity.coupons
                   .Where(c => c.code == code)
                   .Select(c => new
                   {
                       c.cId,
                       c.name,
                       c.code,

                       c.barcode,
                   })
                   .FirstOrDefault();

                    return TokenManager.GenerateToken(coupon);
                }
            }
        }
        // GET api/<controller>  Get Coupon By is active
        [HttpPost]
        [Route("GetByisActive")]
        public string GetByisActive(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int isActive = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        isActive = int.Parse(c.Value);
                    }
                }
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var coupon = entity.coupons
                   .Where(c => c.isActive == isActive)
                   .Select(c => new
                   {
                       c.cId,
                       c.name,
                       c.code,
                       c.isActive,
                       c.discountType,
                       c.discountValue,
                       c.startDate,
                       c.endDate,
                       c.notes,
                       c.quantity,
                       c.remainQ,
                       c.invMin,
                       c.invMax,
                       c.createDate,
                       c.updateDate,
                       c.createUserId,
                       c.updateUserId,
                      c.details,
                   })
                   .ToList();

                    return TokenManager.GenerateToken(coupon);
                }
            }
        }
        // add or update coupon 
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
                string couponObject = "";
                coupons Object = null;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemObject")
                    {
                        couponObject = c.Value.Replace("\\", string.Empty);
                        couponObject = couponObject.Trim('"');
                        Object = JsonConvert.DeserializeObject<coupons>(couponObject, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                        break;
                    }
                }

                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        coupons tmpcoupon = new coupons();
                        var couponEntity = entity.Set<coupons>();
                        if (Object.cId == 0)
                        {

                            Object.createDate = cc.AddOffsetTodate(DateTime.Now);
                            Object.updateDate = cc.AddOffsetTodate(DateTime.Now);
                            Object.updateUserId = Object.createUserId;
                            couponEntity.Add(Object);
                            tmpcoupon = couponEntity.Add(Object);
                            entity.SaveChanges();
                            message = tmpcoupon.cId.ToString();
                            return TokenManager.GenerateToken(message);
                        }
                        else
                        {

                            tmpcoupon = entity.coupons.Where(p => p.cId == Object.cId).FirstOrDefault();
                            tmpcoupon.name = Object.name;
                            tmpcoupon.code = Object.code;
                            tmpcoupon.isActive = Object.isActive;
                            tmpcoupon.discountType = Object.discountType;
                            tmpcoupon.discountValue = Object.discountValue;
                            tmpcoupon.startDate = Object.startDate;
                            tmpcoupon.endDate = Object.endDate;
                            tmpcoupon.notes = Object.notes;
                            tmpcoupon.quantity = Object.quantity;
                            tmpcoupon.remainQ = Object.remainQ;
                            tmpcoupon.invMin = Object.invMin;
                            tmpcoupon.invMax = Object.invMax;
                            tmpcoupon.details = Object.details;

                            tmpcoupon.updateDate = cc.AddOffsetTodate(DateTime.Now);// server current date;

                            tmpcoupon.updateUserId = Object.updateUserId;
                            tmpcoupon.barcode = Object.barcode;
                            entity.SaveChanges();
                            message = tmpcoupon.cId.ToString();
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
                int couponId = 0;
                int userId = 0;
                Boolean final = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "itemId")
                    {
                        couponId = int.Parse(c.Value);
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
                            coupons couponObj = entity.coupons.Find(couponId);

                            entity.coupons.Remove(couponObj);
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
                            coupons coupObj = entity.coupons.Find(couponId);

                            coupObj.isActive = 0;
                            coupObj.updateUserId = userId;
                            coupObj.updateDate = cc.AddOffsetTodate(DateTime.Now);
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