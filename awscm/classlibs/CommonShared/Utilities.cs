using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AWSCM.CommonShared
{
   public static class Utilities
   {
      public static readonly string AssemblyHomeDirectory =  new FileInfo( Assembly.GetExecutingAssembly().Location )?.DirectoryName;
      private static readonly string dataDirectory =  DataDirectory;
      private const string USE_LAST_KEY = @"uselast";

      public static bool IsUseLast( string value )
      {
         return value.Equals( USE_LAST_KEY, StringComparison.OrdinalIgnoreCase );
      }
      
      public static void DeletePrevConsoleLine()
      {
         if ( Console.CursorTop == 0 )
            return;
         Console.SetCursorPosition( 0, Console.CursorTop - 1 );
         Console.Write( new string( ' ', Console.WindowWidth ) );
         Console.SetCursorPosition( 0, Console.CursorTop - 1 );
      }
      public static void DoReadPassword( ref string password )
      {
         ConsoleKey key;
         do
         {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if ( key == ConsoleKey.Backspace && password.Length > 0 )
            {
               Console.Write( "\b \b" );
               password = password[0..^1];
            }
            else if ( !char.IsControl( keyInfo.KeyChar ) )
            {
               Console.Write( "*" );
               password += keyInfo.KeyChar;
            }
         } while ( key != ConsoleKey.Enter );
      }

      public static string DataDirectory
      {
         get
         {
            return GetValueOfEnvVar( Constants.AWSCM_DATA_PATH_ENV_KEY );
         }
         set
         {
            if ( !IfNull( value ) )
               SetValueOfEnvVar( Constants.AWSCM_DATA_PATH_ENV_KEY, value );
            else
               SetValueOfEnvVar( Constants.AWSCM_DATA_PATH_ENV_KEY, null );
         }
      }

      internal static bool IfNull( string value )
      {
         return string.IsNullOrEmpty( value ) || value.Equals( Constants.NULL_VALUE, StringComparison.OrdinalIgnoreCase );
      }

      public static string GetValueOfEnvVar( string name )
      {
         var value = string.Empty;
         try
         {

            value = Environment.GetEnvironmentVariable( name, EnvironmentVariableTarget.Machine );
            if ( string.IsNullOrEmpty( value ) ) // check process
               value = Environment.GetEnvironmentVariable( name, EnvironmentVariableTarget.Process );
            if ( string.IsNullOrEmpty( value ) ) // check User
               value = Environment.GetEnvironmentVariable( name, EnvironmentVariableTarget.User );
            if ( !string.IsNullOrEmpty( value ) ) //decrypt
            {
               if ( Symmetric.TryDecrypt( null, value, out string plainText, out _ ) )
                  value = plainText;
            }
         }
         catch
         {
            //swallo
         }
         return value == null ? string.Empty : value;
      }

      public static void SetValueOfEnvVar( string name, string value )
      {
         if ( !string.IsNullOrEmpty( value ) ) //decrypt
         {
            if ( Symmetric.TryEncrypt( null, value, out string cipherText, out _ ) )
               value = cipherText;
         }
         Environment.SetEnvironmentVariable( name, value, EnvironmentVariableTarget.Machine );
         Environment.SetEnvironmentVariable( name, value, EnvironmentVariableTarget.Process );
         Environment.SetEnvironmentVariable( name, value, EnvironmentVariableTarget.User );
      }

      public static string ValidBatchFilePath( string filePath )
      {
         if ( !Path.IsPathFullyQualified( filePath ) )
         {
            if ( !string.IsNullOrEmpty( dataDirectory ) )
               filePath = Path.Combine( Path.Combine( dataDirectory, Constants.AWSCM_BATCH_FILE_FOLDER ), filePath );
            else
               filePath = Path.Combine( Path.Combine( AssemblyHomeDirectory, Constants.AWSCM_BATCH_FILE_FOLDER ), filePath );
            Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );
         }
         return filePath;
      }

      public static string ValidKeyPairFilePath( string filePath )
      {
         if ( !Path.IsPathFullyQualified( filePath ) )
         {
            if ( !string.IsNullOrEmpty( dataDirectory ) )
               filePath = Path.Combine( Path.Combine( dataDirectory, Constants.AWSCM_KEYPAIR_FILE_FOLDER ), filePath );
            else
               filePath = Path.Combine( Path.Combine( AssemblyHomeDirectory, Constants.AWSCM_KEYPAIR_FILE_FOLDER ), filePath );
            Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );
         }
         return filePath;
      }
      public static string ValidTemplateFilePath( string filePath )
      {
         if ( !Path.IsPathFullyQualified( filePath ) )
         {
            if ( !string.IsNullOrEmpty( dataDirectory ) )
               filePath = Path.Combine( Path.Combine( dataDirectory, Constants.AWSCM_TEMPLATE_FILE_FOLDER ), filePath );
            else
               filePath = Path.Combine( Path.Combine( AssemblyHomeDirectory, Constants.AWSCM_TEMPLATE_FILE_FOLDER ), filePath );
            Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );
         }
         return filePath;
      }

      public static string ValidParameterFilePath( string filePath )
      {
         if ( !Path.IsPathFullyQualified( filePath ) )
         {
            if ( !string.IsNullOrEmpty( dataDirectory ) )
               filePath = Path.Combine( Path.Combine( dataDirectory, Constants.AWSCM_PARAMS_FILE_FOLDER ), filePath );
            else
               filePath = Path.Combine( Path.Combine( AssemblyHomeDirectory, Constants.AWSCM_PARAMS_FILE_FOLDER ), filePath );
            Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );
         }
         return filePath;
      }

      public static string ValidScriptFilePath( string filePath )
      {
         if ( !Path.IsPathFullyQualified( filePath ) )
         {
            if ( !string.IsNullOrEmpty( dataDirectory ) )
               filePath = Path.Combine( Path.Combine( dataDirectory, Constants.AWSCM_SCRIPT_FILE_FOLDER ), filePath );
            else
               filePath = Path.Combine( Path.Combine( AssemblyHomeDirectory, Constants.AWSCM_SCRIPT_FILE_FOLDER ), filePath );
            Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );
         }
         return filePath;
      }

      public static string ValidLogFilePath( string filePath )
      {
         if ( !Path.IsPathFullyQualified( filePath ) )
         {
            if ( !string.IsNullOrEmpty( dataDirectory ) )
               filePath = Path.Combine( Path.Combine( dataDirectory, Constants.AWSCM_LOG_FILE_FOLDER ), filePath );
            else
               filePath = Path.Combine( Path.Combine( AssemblyHomeDirectory, Constants.AWSCM_LOG_FILE_FOLDER ), filePath );
            Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );
         }
         return filePath;
      }
      public static bool GetEmbededResourceAsString( string filename, ref string result, ref string errorMessage )
      {
         var success = false;
         result = string.Empty;
         errorMessage = string.Empty;
         try
         {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            if ( assembly != null )
            {
               var name = assembly.GetName().Name + "." + filename;
               var info = assembly.GetManifestResourceInfo( name );
               if ( info != null )
                  using ( Stream stream = assembly.GetManifestResourceStream( name ) )
                  {
                     using ( StreamReader sr = new StreamReader( stream ) )
                     {
                        result = sr.ReadToEnd();
                        success = true;
                     }
                  }
               else
               {
                  errorMessage = "GetEmbededResourceAsString -> GetManifestResourceInfo not available for '" + name + "'";
               }
            }
         }
         catch ( Exception e )
         {
            errorMessage = ExceptionUtils.ParseException( e );
         }
         return success;
      }
   }
}
