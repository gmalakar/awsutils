using System;
using System.Collections.Generic;
using System.Text;

namespace AWSCM.Common.Extensions
{
   public static class StringExtensions
   {
      public static string AddString( this string str, string strtoadd, string separator = ", " )
      {
         if ( !string.IsNullOrEmpty( strtoadd ) )
         {
            if ( string.IsNullOrEmpty( str ) )
               str = strtoadd;
            else
               str = $"{str}{separator}{strtoadd}";
         }
         return str;
      }
   }
}
