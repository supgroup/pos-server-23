using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Models
{
    public class CategoryEcommerceModel
    {
        public int categoryId { get; set; }
        public string categoryCode { get; set; }
        public string name { get; set; }
        public string details { get; set; }
        public string image { get; set; }
        public Nullable<short> isActive { get; set; }
        public Nullable<decimal> taxes { get; set; }
        public Nullable<byte> fixedTax { get; set; }
        public Nullable<int> parentId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public string notes { get; set; }
        public Boolean canDelete { get; set; }
        public Nullable<int> sequence { get; set; }
        public Nullable<int> id { get; set; }
        public List<CategoryEcommerceModel> childCategories { get; set; }
        public List<ItemEcommerceModel> items { get; set; }
    }
    public class ItemEcommerceModel 
    {
        public int? itemId { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string details { get; set; }
        public string type { get; set; }
        public string image { get; set; }
        public Nullable<decimal> taxes { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Nullable<int> min { get; set; }
        public Nullable<int> max { get; set; }
        public Nullable<int> categoryId { get; set; }
        public string categoryName { get; set; }
        public Nullable<int> parentId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
       
        public Nullable<int> minUnitId { get; set; }
        public Nullable<int> maxUnitId { get; set; }
        public Nullable<int> itemCount { get; set; }
      
       
       
        public Nullable<int> isNew { get; set; }
        public Nullable<int> isOffer { get; set; }
        // unit item
        public string parentName { get; set; }
      
        public string minUnitName { get; set; }
        public string maxUnitName { get; set; }

        public List<ItemUnitEcommerceModel> ItemUnitList { get; set; }
        public List<PropertyModel> Properties { get; set; }


    }
    public class PosEcommerceModel
    {
        public int purchasesCount { get; set; }
        public int salesCount { get; set; }
        public int vendorsCount { get; set; }
        public int customersCount { get; set; }
        public int onLineUsersCount { get; set; }
        public decimal balance { get; set; }
        public int branchId { get; set; }
    }
    public class SettingEcommerceModel
    {
        public int valId { get; set; }
        public string value { get; set; }
        public Nullable<int> isDefault { get; set; }
        public Nullable<int> isSystem { get; set; }
        public string notes { get; set; }
        public Nullable<int> settingId { get; set; }
        //setting
        public string settingName { get; set; }
        public string settingNotes { get; set; }


    }
    public class ItemUnitEcommerceModel
    {
        #region parameters
        //offer
        public string discountType { get; set; }
        public Nullable<decimal> discountValue { get; set; }
        public Nullable<decimal> priceTax { get; set; }
 

        public string offerName { get; set; }
        public Nullable<int> offerId { get; set; }
        public Nullable<decimal> desPrice { get; set; }

        //
        public int itemUnitId { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<int> unitId { get; set; }
        public Nullable<int> unitValue { get; set; }
        public Nullable<short> defaultSale { get; set; }

        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> basicPrice { get; set; }
        public Nullable<decimal> cost { get; set; }
        public string barcode { get; set; }
        public string mainUnit { get; set; }
        public string smallUnit { get; set; }
        public Nullable<int> subUnitId { get; set; }
        public string itemName { get; set; }
        public string itemCode { get; set; }
        public string unitName { get; set; }
        public Nullable<int> storageCostId { get; set; }

        public Nullable<byte> isActive { get; set; }
        public Nullable<decimal> taxes { get; set; }


        #endregion

    }
    public class CustomerEcommerceModel
    {
        public int agentId { get; set; }

        public string name { get; set; }
        public string code { get; set; }
        public string company { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string image { get; set; }
        public string type { get; set; }
        public string accType { get; set; }
        public Nullable<decimal> balance { get; set; }
        public Nullable<byte> balanceType { get; set; }

        public string notes { get; set; }
        public Nullable<byte> isActive { get; set; }
        public string fax { get; set; }
        public Nullable<decimal> maxDeserve { get; set; }
        public bool isLimited { get; set; }
        public string payType { get; set; }
        public string lastName { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public Nullable<int> countryId { get; set; }
        public Nullable<int> cityId { get; set; }
        public string language { get; set; }
        public Nullable<bool> isShopCustomer { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public string countryName { get; set; }
        public string cityName { get; set; }

    }

    public class CountryModel
    {

     
        public int countryId { get; set; }
        public string code { get; set; }
        public string currency { get; set; }
        public string name { get; set; }
        public byte isDefault { get; set; }
        public int currencyId { get; set; }
        public List<CityModel> citiesList { get; set; }
        public string timeZoneName { get; set; }
        public string timeZoneOffset { get; set; }
    }

    public class CityModel
    {


        public int cityId { get; set; }
        public string cityCode { get; set; }
        public Nullable<int> countryId { get; set; }
    }
    public  class siteContentsModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string contents { get; set; }
        public string lang { get; set; }
        public string category { get; set; }
    }
    public  class siteContactModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string subject { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> subjectId { get; set; }
        public bool isActive { get; set; }
        public Nullable<int> updateUserId { get; set; }
    }
    public class siteSubjectModel
    {
        public int subjectId { get; set; }
        public string name { get; set; }
        public bool isActive { get; set; }
        public string value { get; set; }
        public string lang { get; set; }
    }
}