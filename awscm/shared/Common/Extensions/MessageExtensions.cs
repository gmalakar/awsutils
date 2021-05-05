using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace AWSCM.Common.Extensions
{
   public static class MessageExtensions
   {
      public static string StatusMessage( this HttpStatusCode statusCode, [CallerMemberName] string methodName = "" ) =>
            statusCode == HttpStatusCode.OK
                ? $"Operation completed successfully. Source: [{ methodName }]"
                : $"Operation completed as expected. Source: [{ methodName }], HTTP Status Code of { (int)statusCode } ({ statusCode }).";
   }
}
