using DeepLinking.Abstraction;
using DeepLinking.Helper;
using DeepLinking.Models;
using DeepLinking.Models.DBModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Obfuscation;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DeepLinking.Repository
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly AppSettings _appSettings;
        private readonly linkserviceContext _context;
        private readonly Dependencies _dependencies;

        public AnalyticsRepository(IOptions<AppSettings> appSettings, linkserviceContext context, IOptions<Dependencies> dependencies)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _dependencies = dependencies.Value;
        }
        public void InsertAnalytics()
        {
            try
            {
                string advertisementId = string.Empty;
                DateTime? lastCouponDate = null;
                var client = new RestClient(_appSettings.Host + _dependencies.GetAnalyticsUrl + "links");
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = response.Content;
                    var analyticsData = JsonConvert.DeserializeObject<GetAnalyticsResponse>(result);
                    if (analyticsData != null)
                        lastCouponDate = analyticsData.CreatedAt;
                }

                if (lastCouponDate != null)
                {
                    List<PromotionAnalytics> promotionAnalyticsList = new List<PromotionAnalytics>();
                    var redemptions = _context.LinkLogs.Where(x => x.CreatedAt > lastCouponDate).ToList();
                    if (redemptions != null && redemptions.Count > 0)
                    {
                        foreach (var group in redemptions.GroupBy(x => x.PromotionId))
                        {
                            var items = group.FirstOrDefault();
                            PromotionAnalytics promotionAnalytics = new PromotionAnalytics();
                            promotionAnalytics.PromotionId = ObfuscationClass.EncodeId(items.PromotionId.GetValueOrDefault(), _appSettings.Prime).ToString();
                            var clientItem = new RestClient(_appSettings.Host + _dependencies.GetPromotions + ObfuscationClass.EncodeId(items.PromotionId.GetValueOrDefault(), _appSettings.Prime).ToString());
                            var requestItem = new RestRequest(Method.GET);
                            IRestResponse responseItem = clientItem.Execute(requestItem);
                            if (responseItem.StatusCode == HttpStatusCode.OK)
                            {
                                var result = responseItem.Content;
                                var promotionData = JsonConvert.DeserializeObject<PromotionsGetResponse>(result);
                                if (promotionData != null)
                                {
                                    var itemData = promotionData.data.FirstOrDefault();
                                    advertisementId = itemData.AdvertisementId;
                                }
                            }
                            promotionAnalytics.AdvertismentId = advertisementId;
                            promotionAnalytics.CreatedAt = DateTime.Now;
                            promotionAnalytics.Count = group.Key;
                            promotionAnalytics.Type = "links";
                            promotionAnalyticsList.Add(promotionAnalytics);
                        }
                    }

                    if (promotionAnalyticsList != null && promotionAnalyticsList.Count > 0)
                    {
                        AnalyticsModel analyticsModel = new AnalyticsModel()
                        {
                            analytics = promotionAnalyticsList
                        };

                        var postClient = new RestClient(_appSettings.Host + _dependencies.PostAnalyticsUrl);
                        var postRequest = new RestRequest(Method.POST);
                        string jsonToSend = JsonConvert.SerializeObject(analyticsModel);
                        postRequest.AddParameter("application/json; charset=utf-8", jsonToSend, ParameterType.RequestBody);
                        postRequest.RequestFormat = DataFormat.Json;
                        IRestResponse institutionResponse = postClient.Execute(postRequest);
                        if (institutionResponse.StatusCode != HttpStatusCode.Created)
                        {

                        }
                    }
                }
                else
                {
                    List<PromotionAnalytics> promotionAnalyticsList = new List<PromotionAnalytics>();
                    var redemptions = _context.LinkLogs.ToList();
                    if (redemptions != null)
                    {
                        foreach (var group in redemptions.GroupBy(x => x.PromotionId))
                        {
                            var items = group.FirstOrDefault();
                            PromotionAnalytics promotionAnalytics = new PromotionAnalytics();
                            promotionAnalytics.PromotionId = ObfuscationClass.EncodeId(items.PromotionId.GetValueOrDefault(), _appSettings.Prime).ToString();

                            var clientItem = new RestClient(_appSettings.Host + _dependencies.GetPromotions + ObfuscationClass.EncodeId(items.PromotionId.GetValueOrDefault(), _appSettings.Prime).ToString());
                            var requestItem = new RestRequest(Method.GET);
                            IRestResponse responseItem = clientItem.Execute(requestItem);
                            if (responseItem.StatusCode == HttpStatusCode.OK)
                            {
                                var result = responseItem.Content;
                                var promotionData = JsonConvert.DeserializeObject<PromotionsGetResponse>(result);
                                if (promotionData != null)
                                {
                                    var itemData = promotionData.data.FirstOrDefault();
                                    advertisementId = itemData.AdvertisementId;
                                }
                            }
                            promotionAnalytics.AdvertismentId = advertisementId;
                            promotionAnalytics.CreatedAt = DateTime.Now;
                            promotionAnalytics.Count = group.Key;
                            promotionAnalytics.Type = "links";
                            promotionAnalyticsList.Add(promotionAnalytics);
                        }
                    }

                    if (promotionAnalyticsList != null && promotionAnalyticsList.Count > 0)
                    {
                        AnalyticsModel analyticsModel = new AnalyticsModel()
                        {
                            analytics = promotionAnalyticsList
                        };

                        var postClient = new RestClient(_appSettings.Host + _dependencies.PostAnalyticsUrl);
                        var postRequest = new RestRequest(Method.POST);
                        string jsonToSend = JsonConvert.SerializeObject(analyticsModel);
                        postRequest.AddParameter("application/json; charset=utf-8", jsonToSend, ParameterType.RequestBody);
                        postRequest.RequestFormat = DataFormat.Json;
                        IRestResponse institutionResponse = postClient.Execute(postRequest);
                        if (institutionResponse.StatusCode != HttpStatusCode.Created)
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
