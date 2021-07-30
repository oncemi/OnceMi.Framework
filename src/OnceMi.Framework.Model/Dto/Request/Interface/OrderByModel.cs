using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class OrderByModel
    {
        private string orderBy = "asc";

        public OrderByModel()
        {

        }

        public OrderByModel(string filed)
        {
            this.Filed = filed;
            this.OrderBy = "asc";
        }

        public OrderByModel(string filed,string orderBy)
        {
            this.Filed = filed;
            this.OrderBy = orderBy;
        }


        public string Filed { get; set; }

        public string OrderBy
        {
            get
            {
                return orderBy;
            }
            set
            {
                switch (value.ToLower())
                {
                    default:
                    case "asc":
                        orderBy = "asc";
                        break;
                    case "desc":
                        orderBy = "desc";
                        break;
                }
            }
        }
    }
}
