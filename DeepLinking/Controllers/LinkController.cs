using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DeepLinking.Helper;
using DeepLinking.Models;
using DeepLinking.Models.DBModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Obfuscation;
using RestSharp;
using UAParser;

namespace DeepLinking.Controllers
{
    public class LinkController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly Dependencies _dependencies;
        private readonly linkserviceContext _context;
        public LinkController(IOptions<AppSettings> appSettings, IOptions<Dependencies> dependencies, linkserviceContext linkserviceContext)
        {
            _appSettings = appSettings.Value;
            _dependencies = dependencies.Value;
            _context = linkserviceContext;
        }

        [Obsolete]
        public IActionResult Index(string id)
        {
            List<Links> promotions = new List<Links>();
            try
            {
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                string uaString = Convert.ToString(userAgent[0]);
                var uaParser = Parser.GetDefault();
                ClientInfo clientInfo = uaParser.Parse(uaString);
                if (!string.IsNullOrEmpty(id))
                {
                    var client = new RestClient(_appSettings.Host + _dependencies.PromotionsUrl + id);
                    var request = new RestRequest(Method.GET);
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = response.Content;
                        var promotionData = JsonConvert.DeserializeObject<LinkResponse>(result);
                        promotions.AddRange(promotionData.data);
                    }
                    if (promotions.Count > 0)
                    {
                        foreach (var item in promotions)
                        {
                            LinkLogsData(clientInfo, item.PromotionId);
                            var webLink = promotions.Where(x => x.PromotionId == id).Select(x => x.Web).FirstOrDefault();
                            if (clientInfo.OS.Family.ToLower() == "windows")
                            {
                                if (string.IsNullOrEmpty(item.Web))
                                {
                                    return Redirect(_appSettings.RoutesAppUrl + id);
                                }
                                else
                                {
                                    return Redirect(item.Web);
                                }
                            }
                            else if (clientInfo.OS.Family.ToLower() == "ios" || clientInfo.OS.Family.ToLower() == "iPhone")
                            {
                                if (string.IsNullOrEmpty(item.Ios))
                                {
                                    if (string.IsNullOrEmpty(webLink))
                                        return Redirect(_appSettings.RoutesAppUrl + id);
                                    else
                                        return Redirect(webLink);

                                }
                                else
                                {
                                    return Redirect(item.Ios);
                                }
                            }
                            else if (clientInfo.OS.Family.ToLower() == "android")
                            {
                                if (string.IsNullOrEmpty(item.Android))
                                {
                                    if (string.IsNullOrEmpty(webLink))
                                        return Redirect(_appSettings.RoutesAppUrl + id);
                                    else
                                        return Redirect(webLink);
                                }
                                else
                                {
                                    return Redirect(item.Android);
                                }
                            }
                        }
                    }
                    else
                    {
                        LinkLogsData(clientInfo, id);
                        return Redirect(_appSettings.RoutesAppUrl + id);
                    }
                }
                else
                {
                    LinkLogsData(clientInfo, id);
                    return Redirect(_appSettings.RoutesAppUrl + id);
                }
                LinkLogsData(clientInfo, id);
                return Redirect(_appSettings.RoutesAppUrl + id);
            }
            catch (Exception)
            {
                return Redirect(_appSettings.RoutesAppUrl + id);
            }
        }

        [Obsolete]
        public void LinkLogsData(ClientInfo clientInfo, string promotionId)
        {
            int? promotionIdDecode = ObfuscationClass.DecodeId(Convert.ToInt32(promotionId), _appSettings.PrimeInverse);
            LinkLogs linkLogs = new LinkLogs();
            linkLogs.PromotionId = promotionIdDecode;
            linkLogs.ClientBrowser = clientInfo.UserAgent.Family.ToLower();
            linkLogs.ClientOs = clientInfo.OS.Family.ToLower();
            linkLogs.CreatedAt = DateTime.Now;
            _context.LinkLogs.Add(linkLogs);
            _context.SaveChanges();
        }
    }
}