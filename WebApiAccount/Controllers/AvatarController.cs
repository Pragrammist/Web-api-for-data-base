using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserAccountsDataBaseWebApi;
using WebApiAccount.Services;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;

namespace WebApiAccount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AvatarsController : DataController<Avatar, AvatarStore, AvatarsAgent>
    {
        
        public AvatarsController(AvatarsAgent avatars) : base(avatars)
        {            
            
        }
        
    }
}
