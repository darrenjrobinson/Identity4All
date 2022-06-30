using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspNetCoreVerifiableCredentials.Pages
{
    public class IssuerVCModel : PageModel
    {

        public void OnGet([FromForm] Microsoft.AspNetCore.Http.IFormFile image)
        {
        }
    }
}
