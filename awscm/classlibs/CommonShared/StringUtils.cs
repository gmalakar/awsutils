using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Json;
using System.Linq;
using System.Text.Json;

namespace AWSCM.CommonShared
{
   public static class StringUtils
   {
      public static void AddString( ref string str, string strtoadd, string separator = ", " )
      {
         if ( !string.IsNullOrEmpty( strtoadd ) )
         {
            if ( string.IsNullOrEmpty( str ) )
               str = strtoadd;
            else
               str = $"{str}{separator}{strtoadd}";
         }
      }

      public static string AddString( string str, string strtoadd, string separator = ", " )
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

      public static List<string> ValueAsList( string values, string splitter = "|" )
      {
         return values.Split( splitter ).ToList();
      }

      public static string ConvertToJSONIndented( object objectToConvert, string name = null )
      {
         if ( objectToConvert is JsonElement && ( (JsonElement)objectToConvert ).ValueKind == JsonValueKind.Undefined )
            objectToConvert = JsonDocument.Parse( @"[]" ).RootElement;
         return JsonSerializer.Serialize( objectToConvert, WriteIndentedOption );
      }

      public static string ConvertToJSON( object objectToConvert )
      {
         if ( objectToConvert is JsonElement && ( (JsonElement)objectToConvert ).ValueKind == JsonValueKind.Undefined )
            objectToConvert = JsonDocument.Parse( @"[]" ).RootElement;
         return JsonSerializer.Serialize( objectToConvert );
      }

      public static T ConvertFromJSON<T>( string jsonString )
      {
         return (T)JsonSerializer.Deserialize<T>( jsonString );
      }

      public static string MaskString( string stringToMask, int maskFormLength = 4, int maskLength = -1, char maskChar = '*' )
      {
         if ( maskLength <= 0 )
            maskLength = stringToMask.Length;
         if ( maskLength <= maskFormLength )
            maskLength = 16;

         return stringToMask.Substring( maskLength - 4 ).PadLeft( maskLength, maskChar );
      }

      public static string NoneIfNull( string str )
      {
         if ( string.IsNullOrEmpty( str ) )
            str = @"None";
         return str;
      }

      public static List<NameValues> ConvertToNameValues( string nameValuesParameters )
      {
         var split1 = nameValuesParameters.Split( StringUtils.FilterDelemitters, StringSplitOptions.RemoveEmptyEntries );
         var nameValuesList = new List<string>();
         foreach ( var s in split1 )
            if ( !string.IsNullOrEmpty( s ) && !CommonShared.StringUtils.FilterIgnores.Contains( s ) )
            {
               nameValuesList.Add( s.TrimStart( CommonShared.StringUtils.FilterTrim ).TrimEnd( CommonShared.StringUtils.FilterTrim ) );
               //var p = pair.TrimStart( trim ).TrimEnd( trim );
               //json = $"{json}{p}{Environment.NewLine}";
            }
         var listNameValue = new List<NameValues>();
         for ( int i = 0; i < nameValuesList.Count; i += 2 )
         {
            listNameValue.Add( new NameValues() { Name = nameValuesList[i], Values = nameValuesList[i + 1].Split( CommonShared.StringUtils.FilterValusSplitter ).ToList<string>() } );
         }
         return listNameValue;
      }

      public static bool IfNull( string value )
      {
         return string.IsNullOrEmpty( value ) || value.Equals( Constants.NULL_VALUE, StringComparison.OrdinalIgnoreCase );
      }

      public static String[] FilterDelemitters = new String[] { "Name=", "Values=", "name=", "values=" };
      public static String[] FilterIgnores = new String[] { "\"", " " };
      public static Char[] FilterTrim = new Char[] { '"', '\'', ' ', ',' };
      public static Char[] FilterValusSplitter = new Char[] { ',', '|' };
      
      private static readonly JsonSerializerOptions WriteIndentedOption = new JsonSerializerOptions(){ WriteIndented = true };
   }
}
