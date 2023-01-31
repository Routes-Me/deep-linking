using DeepLinking.Abstraction;
using DeepLinking.Helper;
using DeepLinking.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DeepLinking.Repository
{
    public class QueuedHostedService : BackgroundService
    {

        private readonly ChannelReader<LinkLogs> _channel;
        private readonly AppSettings _appSettings;
        private readonly Dependencies _dependencies;

        public QueuedHostedService(IOptions<AppSettings> appSettings, IOptions<Dependencies> dependencies, ChannelReader<LinkLogs> channel)
        {
            _appSettings = appSettings.Value;
            _dependencies = dependencies.Value;
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await foreach (var item in _channel.ReadAllAsync(cancellationToken))
            {
                try
                {
                    LinkLogs linkLogs = new LinkLogs();
                    linkLogs.LinkLogId = item.LinkLogId;
                    linkLogs.PromotionId = item.PromotionId;
                    linkLogs.AdvertisementId = item.AdvertisementId;
                    linkLogs.InstitutionId = item.InstitutionId;
                    linkLogs.ClientBrowser = item.ClientBrowser;
                    linkLogs.ClientOs = item.ClientOs;
                    linkLogs.CreatedAt = item.CreatedAt;
                    linkLogs.DeviceId = item.DeviceId;
                    linkLogs.Latitude = item.Latitude;
                    linkLogs.Longitude = item.Longitude;
                    var postClient = new RestClient(_appSettings.Host + _dependencies.LinklogsUrl);
                    var postRequest = new RestRequest(Method.POST);
                    string jsonToSend = JsonConvert.SerializeObject(linkLogs);
                    postRequest.AddParameter("application/json; charset=utf-8", jsonToSend, ParameterType.RequestBody);
                    postRequest.RequestFormat = DataFormat.Json;
                    IRestResponse institutionResponse = postClient.Execute(postRequest);
                    if (institutionResponse.StatusCode != HttpStatusCode.Created)
                    {

                    }
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}
