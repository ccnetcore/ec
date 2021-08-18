using CC.ElectronicCommerce.AuthenticationCenter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.AuthenticationCenter.Utility
{
    public interface ICustomJWTService
    {
        string GetToken(User user);
    }
}
