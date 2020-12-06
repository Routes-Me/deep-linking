using System;
using System.Collections.Generic;

namespace DeepLinking.Models.DBModels
{
    public partial class LinkLogs
    {
        public int LinkLogId { get; set; }
        public int? PromotionId { get; set; }
        public string ClientBrowser { get; set; }
        public string ClientOs { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
