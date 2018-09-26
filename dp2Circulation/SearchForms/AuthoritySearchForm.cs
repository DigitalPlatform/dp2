using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Circulation
{
    public class AuthoritySearchForm : BiblioSearchForm
    {
        public AuthoritySearchForm() : base ()
        {
            this.DbType = "authority";
        }
    }
}
