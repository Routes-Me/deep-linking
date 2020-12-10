using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeepLinking.Models
{
    public class Links
    {
        public string LinkId { get; set; }
        public string PromotionId { get; set; }
        public string Web { get; set; }
        public string Ios { get; set; }
        public string Android { get; set; }
    }

    public class Promotion
    {
        public string PromotionId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AdvertisementId { get; set; }
        public string InstitutionId { get; set; }
        public string Type { get; set; }
    }
}
