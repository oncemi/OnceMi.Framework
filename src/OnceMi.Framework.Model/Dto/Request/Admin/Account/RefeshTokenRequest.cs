using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class RefeshTokenRequest : IRequest
    {
        /// <summary>
        /// RefeshToken
        /// </summary>
        [Required(ErrorMessage = "RefeshToken不能为空")]
        public string Token { get; set; }
    }
}
