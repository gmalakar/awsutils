using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Amazon.EC2.Model;

namespace AWSCM.Common.Others
{
   public static class Filters
   {


      public static List<Filter> ConvertToFilter( string filters )
      {
         var split1 = filters.Split( FilterDelemitters, StringSplitOptions.RemoveEmptyEntries );
         var nameValuesList = new List<string>();
         foreach ( var s in split1 )
            if ( !string.IsNullOrEmpty( s ) && !FilterIgnores.Contains( s ) )
               nameValuesList.Add( s.TrimStart( FilterTrim ).TrimEnd( FilterTrim ) );
         var listFilte = new List<Filter>();
         for ( int i = 0; i < nameValuesList.Count; i += 2 )
         {
            listFilte.Add( new Filter() { Name = nameValuesList[i], Values = nameValuesList[i + 1].Split( FilterValuesSplitter ).ToList<string>() } );
         }
         return listFilte;
      }

      public static void ParseFunctionInput( string input, out string operation, out string filters, out string others )
      {
         filters = string.Empty;
         others = string.Empty;
         var split1 = input.Split( InputSplitter, StringSplitOptions.RemoveEmptyEntries );
         operation = split1[0];
         if ( split1.Count() > 1 )
            filters = split1[1];
         if ( split1.Count() > 2 )
            others = split1[2];
      }


      public static String[] FilterDelemitters = new String[] { "Name=", "Values=", "name=", "values=" };
      public static String[] FilterIgnores = new String[] { "\"", " " };
      public static Char[] FilterTrim = new Char[] { '"', '\'', ' ', ',' };
      public static Char[] FilterValuesSplitter = new Char[] { ',' };
      public static Char[] InputSplitter = new Char[] { '|' };
   }
}
