using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManualEmailer3
{
    public enum SmtpAuthMethod
    {
        [Description("No authentication")]
        NoAuthentication,

        [Description("Username and password")]
        BasicAuthentication,

        [Description("Current user")]
        DefaultAuthentication
    }
}
