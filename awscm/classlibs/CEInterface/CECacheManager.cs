using System;
using System.Collections.Generic;
using System.Text;
using AWSCM.CommonShared;

namespace AWSCM.CEInterface
{
   internal static class CECacheManager
   {
      internal static class Constants
      {
         //CE related
         public const string LAST_SET_CREATED_PROJECT_ID_KEY = @"ce-lsetpid";
         public const string LAST_SET_CREATED_CLOUD_ID_KEY = @"ce-lsetcid";
         public const string LAST_SET_CREATED_REGION_ID_KEY = @"ce-lsetrid";
         public const string LAST_SET_CREATED_CREDS_ID_KEY = @"ce-lsetcrid";
         public const string LAST_USE_LAST_KEY = @"uselast";
      }
   }
}
