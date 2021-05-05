#region history
//*****************************************************************************
// Arguments.cs:
//
// History:
// 08/06/20 - Goutam Malakar 
//*****************************************************************************
#endregion history
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AWSCM.AWSConfigManager.Utilities
{
   //from https://ideone.com/t9zusx
   static class ArgumentsUtils
   {
      private static Regex qoutesRegex = new Regex("(?<!\\\\)\\\"", RegexOptions.Compiled);

      private static IEnumerable<string> Split( this string str, Func<char, bool> controller )
      {
         int nextPiece = 0;

         for ( int c = 0; c < str.Length; c++ )
         {
            if ( controller( str[c] ) )
            {
               yield return str.Substring( nextPiece, c - nextPiece );
               nextPiece = c + 1;
            }
         }

         yield return str.Substring( nextPiece );
      }


      private static IEnumerable<string> SplitCommandLine( string commandLine )
      {
         bool inQuotes = false;
         bool isEscaping = false;

         return commandLine.Split( c =>
         {
            if ( c == '\\' && !isEscaping )
            { isEscaping = true; return false; }

            if ( c == '\"' && !isEscaping )
               inQuotes = !inQuotes;

            isEscaping = false;

            return !inQuotes && Char.IsWhiteSpace( c )/*c == ' '*/;
         } )
             .Select( arg => arg.Trim() )
             .Select( arg => qoutesRegex.Replace( arg, "" ).Replace( "\\\"", "\"" ) )
             .Where( arg => !string.IsNullOrEmpty( arg ) );
      }

      public static string[] ToCommandLineArgs( string commandLine )
      {
         return SplitCommandLine( commandLine ).ToArray();
      }
   }

   /// <summary>

   /// Arguments class

   /// </summary>

   public class Arguments
   {
      // Variables

      internal StringDictionary Parameters;
      private string blank=" ";
      private string dq="\"";
      private string sq="'";
      private bool debugging = false;
      const  string BATCH = @"BATCH";
      const  string BATCHFILE = @"BATCHFILE";
      string BATCHSPLITTER = @"(?=-U:)";

      // Constructor

      internal Arguments( string[] Args, bool debug = false )
      {
         this.debugging = debug;
         Parameters = new StringDictionary( );
         Regex Spliter = new Regex( @"^-{1,2}|^/|=|:",
             RegexOptions.IgnoreCase | RegexOptions.Compiled );

         Regex Remover = new Regex( @"^['""]?(.*?)['""]?$",
             RegexOptions.IgnoreCase | RegexOptions.Compiled );

         string Parameter = null;
         string[] Parts;

         // Valid parameters forms:

         // {-,/,--}param{ ,=,:}((",')value(",'))

         // Examples: 

         // -param1 value1 --param2 /param3:"Test-:-work" 

         //   /param4=happy -param5 '--=nice=--'

         //foreach ( string Txt in Args )
         for ( var ix = 0; ix < Args.Length; ix++ )
         {
            var Txt = Args[ix];
            // Look for new parameters (-,/ or --) and a

            // possible enclosed value (=,:)

            Parts = Spliter.Split( Txt, 3 );

            switch ( Parts.Length )
            {
               // Found a value (for the last parameter 

               // found (space separator))

               case 1:
                  if ( Parameter != null )
                  {
                     if ( !Parameters.ContainsKey( Parameter.ToUpper() ) )
                     {
                        Parts[0] = Remover.Replace( Parts[0], "$1" );
                        AddParam( Parameter, Parts[0] );
                        //Parameters.Add( Parameter, Parts [ 0 ] );
                     }
                     Parameter = null;
                  }
                  // else Error: no parameter waiting for a value (skipped)

                  break;

               // Found just a parameter

               case 2:
                  // The last parameter is still waiting. 

                  // With no value, set it to true.

                  if ( Parameter != null )
                  {
                     //if ( !Parameters.ContainsKey( Parameter ) )
                     //   Parameters.Add( Parameter, "true" );
                     AddParam( Parameter, "true" );
                  }
                  Parameter = Parts[1];
                  break;

               // Parameter with enclosed value

               case 3:
                  // The last parameter is still waiting. 

                  // With no value, set it to true.

                  if ( Parameter != null )
                  {
                     //if ( !Parameters.ContainsKey( Parameter ) )
                     //   Parameters.Add( Parameter, "true" );
                     AddParam( Parameter, "true" );
                  }

                  Parameter = Parts[1];

                  // Remove possible enclosing characters (",')

                  if ( !Parameters.ContainsKey( Parameter.ToUpper() ) )
                  {
                     var part2 = Parts[2];
                     if ( this.debugging )
                     {
                        bool withinDQ = part2.StartsWith( dq );
                        bool withinSQ = part2.StartsWith( sq );
                        if ( withinDQ || withinSQ ) //find the end
                        {
                           for ( var i = ++ix; i < Args.Length; i++ )
                           {
                              var partToAdd = Args[i];
                              part2 = string.Format( "{0} {1}", part2, string.IsNullOrEmpty( partToAdd ) ? this.blank : partToAdd );
                              if ( withinDQ && partToAdd.EndsWith( this.dq ) )
                                 break;
                              if ( withinSQ && partToAdd.EndsWith( this.sq ) )
                                 break;
                              ix = i + 1;
                           }
                        }
                     }
                     Parts[2] = Remover.Replace( part2, "$1" );
                     //Parameters.Add( Parameter, Parts [ 2 ] );
                     AddParam( Parameter, Parts[2] );
                  }

                  Parameter = null;
                  break;
            }
         }
         // In case a parameter is still waiting

         if ( Parameter != null )
         {
            //if ( !Parameters.ContainsKey( Parameter ) )
            //   Parameters.Add( Parameter, "true" );
            AddParam( Parameter, "true" );
         }
      }

      internal string GetArgumentValue( string parameterName, bool exception = true )
      {
         string value = this[parameterName];
         if ( value == null && exception )
         {
            Common.ThrowError( "Missing value for " + parameterName.ToUpper() );
         }
         return value == null ? "" : value;
      }

      internal List<string> GetArgumentValueAsList( string parameterName, string splitter = "|", bool exception = true )
      {
         string value = this[parameterName];
         if ( value == null && exception )
         {
            Common.ThrowError( "Missing value for " + parameterName.ToUpper() );
         }
         return value.Split( splitter ).ToList();
      }

      internal bool GetArgumentValueAsBoolean( string parameterName, bool defaultValue = false )
      {
         var boolValue = defaultValue;
         var value = GetArgumentValue( parameterName, false ).ToLower();
         if ( !string.IsNullOrEmpty( value ) )
            boolValue = value == "true" || value == "1" || value == "y" || value == "yes";
         return boolValue;
      }

      internal int GetArgumentValueAsInteger( string parameterName, int defaultValue = 0 )
      {
         var intValue = defaultValue;
         var value = GetArgumentValue( parameterName, false ).ToLower();
         if ( !string.IsNullOrEmpty( value ) )
         {
            if ( !int.TryParse( value, out intValue ) )
               intValue = defaultValue;
         }
         return intValue;
      }

      internal bool IsParamWithNoValue( string parameterName )
      {
         return this.Parameters.ContainsKey( parameterName.ToUpper() ) && this[parameterName] == "true";
      }

      internal bool HasParam( string parameterName )
      {
         return this.Parameters.ContainsKey( parameterName.ToUpper() );
      }

      internal bool TryGetArgumentValue( string parameterName, out string value )
      {
         var success = false;
         value = this[parameterName];
         if ( value != null )
         {
            value = value.Trim();
            success = true;
         }
         return success;
      }

      internal void AddParam( string param, string value )
      {
         param = param.ToUpper();//case insensitive param
         if ( !Parameters.ContainsKey( param ) )
         {
            Parameters.Add( param, value );
            if ( this.debugging )
               Common.WriteMessage( string.Format( "{0} - {1}", param, value ) );
         }
      }

      internal void PrintAllParams()
      {
         foreach ( DictionaryEntry param in Parameters )
         {
            Common.WriteMessage( string.Format( "Param: {0} - Value: {1}", param.Key, param.Value ) );
         }
      }

      internal bool TryGetBatchArgs( out IList<string[]> values, out string batchFileName )
      {
         var success = false;
         var text = string.Empty;
         values = null;
         batchFileName = string.Empty;
         if ( TryGetArgumentValue( BATCH, out string args ) )
         {
            success = true;
            //check if string is within []
            text = args.TrimStart( '[' ).TrimEnd( ']' );
         }
         else if ( TryGetArgumentValue( BATCHFILE, out batchFileName ) )
         {
            batchFileName = CommonShared.Utilities.ValidBatchFilePath( batchFileName );
            try
            {
               text = File.ReadAllText( batchFileName, Encoding.UTF8 );
               success = true;
            }
            catch ( Exception e )
            {
               Common.WriteMessage( string.Format( "Error {0} - Source {1} - Stack {2}", e.Message, e.Source, e.StackTrace ) );
            }
            success = true;
         }
         if ( success )
         {
            if ( !string.IsNullOrEmpty( text ) )
            {
               values = new List<string[]>();
               foreach ( var arg in Regex.Split( text, BATCHSPLITTER, RegexOptions.IgnoreCase ) )
               {
                  var carg = ArgumentsUtils.ToCommandLineArgs( arg.TrimEnd( Environment.NewLine.ToCharArray() ) );
                  if ( carg != null && carg.Length > 0 ) //eleminate null or blank array
                     values.Add( ArgumentsUtils.ToCommandLineArgs( arg.TrimEnd( Environment.NewLine.ToCharArray() ) ) );
               }
            }
            else
            {
               success = false;
               Common.WriteMessage( "No batch arguments available!" );
            }
         }
         return success;
      }
      // Retrieve a parameter value if it exists 

      // (overriding C# indexer property)

      internal string this[string Param]
      {
         get
         {
            return ( Parameters[Param.ToUpper()] );
         }
      }
   }
}