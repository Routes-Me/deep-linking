using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeepLinking.Models
{
    public class Response
    {
        public bool status { get; set; }
        public string message { get; set; }
        public int statusCode { get; set; }
    }

    public class LinkResponse : Response
    {
        public List<Links> data { get; set; }
    }

    public class GetAnalyticsResponse : Response
    {
        public DateTime? CreatedAt { get; set; }
    }

    public class PromotionsGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<PromotionsModel> data { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JObject included { get; set; }
    }
}
