using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DeepLinking.Abstraction;
using DeepLinking.Helper;
using DeepLinking.Models;
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
        private readonly ChannelWriter<LinkLogs> _channel;

        public LinkController(IOptions<AppSettings> appSettings, IOptions<Dependencies> dependencies, ChannelWriter<LinkLogs> channel)
        {
            _appSettings = appSettings.Value;
            _dependencies = dependencies.Value;
            _channel = channel;
        }

        [Obsolete]
        public async Task<IActionResult> Index(string id)
        {
            List<Links> links = new List<Links>();
            List<Promotion> promotions = new List<Promotion>();
            string advertisementId = string.Empty, institutionId = string.Empty;
            try
            {
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                string uaString = Convert.ToString(userAgent[0]);
                var uaParser = Parser.GetDefault();
                ClientInfo clientInfo = uaParser.Parse(uaString);
                if (!string.IsNullOrEmpty(id))
                {
                    var client = new RestClient(_appSettings.Host + _dependencies.PromotionsUrl.Replace("|Id|", id));
                    var request = new RestRequest(Method.GET);
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = response.Content;
                        var linksData = JsonConvert.DeserializeObject<LinkResponse>(result);
                        links.AddRange(linksData.data);
                        advertisementId = linksData.included.promotions.Where(x => x.PromotionId == id).Select(x => x.AdvertisementId).FirstOrDefault();
                        institutionId = linksData.included.promotions.Where(x => x.PromotionId == id).Select(x => x.InstitutionId).FirstOrDefault();
                    }
                    if (links.Count > 0)
                    {
                        foreach (var item in links)
                        {
                            await LinkLogsDataAsync(clientInfo, id, advertisementId, institutionId);
                            var webLink = links.Where(x => x.PromotionId == id).Select(x => x.Web).FirstOrDefault();
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
                        await LinkLogsDataAsync(clientInfo, id, advertisementId, institutionId);
                        return Redirect(_appSettings.RoutesAppUrl + id);
                    }
                }
                else
                {
                    await LinkLogsDataAsync(clientInfo, id, advertisementId, institutionId);
                    return Redirect(_appSettings.RoutesAppUrl + id);
                }
                await LinkLogsDataAsync(clientInfo, id, advertisementId, institutionId);
                return Redirect(_appSettings.RoutesAppUrl + id);
            }
            catch (Exception)
            {
                return Redirect(_appSettings.RoutesAppUrl + id);
            }
        }

        [Obsolete]
        public async Task LinkLogsDataAsync(ClientInfo clientInfo, string promotionId, string advertisementId, string institutionId)
        {
            LinkLogs linkLogs = new LinkLogs();
            linkLogs.PromotionId = promotionId;
            linkLogs.AdvertisementId = advertisementId;
            linkLogs.InstitutionId = institutionId;
            linkLogs.ClientBrowser = clientInfo.UserAgent.Family.ToLower();
            linkLogs.ClientOs = clientInfo.OS.Family.ToLower();
            linkLogs.CreatedAt = DateTime.Now;
            await _channel.WriteAsync(linkLogs);
        }
    }
}