using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pomelo.DevOps.Models.ViewModels
{
    public class PostPkceRequest
    {
        public string Verifier { get; set; }

        public string ClientId { get; set; }

        public string GrantType { get; set; }


    }
}
