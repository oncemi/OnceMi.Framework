namespace OnceMi.Framework.Model.Dto.Request.Admin.Config
{
    class WebSiteInfoModel
    {
        public string Name { get; set; }

        public string IndexUrl { get; set; }

        /// <summary>
        /// ICP备案
        /// </summary>
        public string ICP { get; set; }

        /// <summary>
        /// ICP管局网站地址
        /// </summary>
        public string ICPUrl { get; set; }

        /// <summary>
        /// 公安部备案
        /// </summary>
        public string MPS { get; set; }

    }
}
