using System;
using System.Collections.Generic;
using System.Text;

namespace AWSCM.CommonShared
{
   public static class ExceptionUtils
   {
      public static string ParseException( Exception e, bool detail = false, string from = null )
      {
         var message = (from == null) ? string.Empty: $"{from} -> ";
         if ( e != null )
         {
            StringUtils.AddString( ref message, $"Message: {e.Message}", Environment.NewLine );
            if ( detail )
            {
               StringUtils.AddString( ref message, $"Source: {e.Source}", Environment.NewLine );
               StringUtils.AddString( ref message, $"StackTrace: {e.StackTrace}", Environment.NewLine );
            }
            if ( e.InnerException != null )
               message = $"{message} {ParseException( e.InnerException, detail, detail ? @"InnerException" : string.Empty )}";
         }
         return message;
      }
   }
}
