namespace OnceMi.Framework.Model.Common
{
    public class OrderRule
    {
        private string orderBy = "asc";

        public OrderRule()
        {

        }

        public OrderRule(string filed)
        {
            Filed = filed;
            OrderBy = "asc";
        }

        public OrderRule(string filed, string orderBy)
        {
            Filed = filed;
            OrderBy = orderBy;
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
