using System;
using System.Collections.Generic;
using System.Text;

namespace AWSCM.Common.Extensions
{
   public static class ExceptionExtensions
   {
      public static string ParseException( this Exception exception, bool detail = false, string from = null )
      {
         var message = (from == null) ? string.Empty: $"{from} -> ";
         if ( exception != null )
         {
            message = message.AddString( $"Message: {exception.Message}", Environment.NewLine );

            if ( detail )
            {
               message = message.AddString( $"Source: {exception.Source}", Environment.NewLine );
               message = message.AddString( $"StackTrace: {exception.StackTrace}", Environment.NewLine );
            }
            if ( exception.InnerException != null )
               message = $"{message} {ParseException( exception.InnerException, detail, detail ? @"InnerException" : string.Empty )}";
         }
         return message;
      }
   }
}
