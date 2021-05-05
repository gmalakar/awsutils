using System;
using System.Collections.Generic;
using System.Text;
using AWSCM.CommonShared;

namespace AWSCM.AWSInterface
{
   internal static class AWSCacheManager
   {

      public static bool TryGetKeyMaterial( string keyName, out string value, out string message )
      {
         return CommonShared.FileBasedCache.TryGet( $"{keyName}{ Constants.KPM_KEY}", out value, out message );
      }

      public static void SaveKeyMaterialToCache( string keyName, string value )
      {
         CommonShared.FileBasedCache.AddOrUpdate( $"{keyName}{Constants.KPM_KEY}", value );
      }

      internal static class Constants
      {
         //EC2 related
         public const string LAST_CREATED_VPC_ID_KEY = @"lcvpcid";
         public const string LAST_CREATED_IG_ID_KEY = @"lcigid";
         public const string LAST_CREATED_RT_ID_KEY = @"lcrtid";
         public const string LAST_CREATED_SG_ID_KEY = @"lcsgid";
         public const string LAST_CREATED_SN_ID_KEY = @"lcsnid";
         public const string LAST_CREATED_KP_ID_KEY = @"lckpid";
         public const string LAST_CREATED_KP_NAME_KEY = @"lckpnid";
         public const string LAST_LAUNCHED_INATANCES_KEY = @"llec2ids";
         public const string KPM_KEY = @"-#!((&%#1";

         //IAM related
         public const string LAST_CREATED_ACCESSED_ROLE_NAME_KEY = @"lcrn";
         public const string LAST_CREATED_POLICY_NAME_KEY = @"lcp";
         public const string LAST_CREATED_ACCESSED_PROFILE_KEY = @"lcinspf";
      }

   }
}
