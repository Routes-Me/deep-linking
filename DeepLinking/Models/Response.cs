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
        public Pagination pagination { get; set; }
        public List<Links> data { get; set; }
        public PromotionsInclude included { get; set; }
    }

    public class PromotionsInclude
    {
        public List<Promotion> promotions { get; set; }
    }

    public class GetAnalyticsResponse : Response
    {
        public DateTime? CreatedAt { get; set; }
    }

}
