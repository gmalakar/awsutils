using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;

namespace AWSCM.AWSConfigManager.Utilities
{
   static internal class Common
   {
      static internal bool IsBatchArguments  = false;
      internal const string  SERVICE = @"service";
      internal const string  ACTION = @"action";
      internal const string  TYPE = @"type";
      internal const string  SERVICE2 = @"s";
      internal const string  ACTION2 = @"a";
      internal const string  MODE = @"mode";
      internal const string  TYPE2 = @"t";
      internal const string PARAM_PREFIX = @"filepath:";
      internal const string EC2_ASSOC_TARGET_KEY = @"InstanceIds";

      internal static Dictionary<string, List<string>> GetRunParameters( string parameters )
      {
         if ( parameters.StartsWith( PARAM_PREFIX, StringComparison.OrdinalIgnoreCase ) ) //get from file
            parameters = File.ReadAllText( CommonShared.Utilities.ValidParameterFilePath( parameters.Remove( 0, PARAM_PREFIX.Length ) ) );
         if ( !string.IsNullOrEmpty( parameters ) )
            return CommonShared.StringUtils.ConvertFromJSON<Dictionary<string, List<string>>>( parameters );
         else
            return null;
      }

      internal static string GetUserData( string userData )
      {
         if ( userData.StartsWith( PARAM_PREFIX, StringComparison.OrdinalIgnoreCase ) ) //get from file
            userData = Convert.ToBase64String( File.ReadAllBytesAsync( CommonShared.Utilities.ValidScriptFilePath( userData.Remove( 0, PARAM_PREFIX.Length ) ) ).Result );

         return userData;
      }

      internal static string GetJsonString( string parameter )
      {
         if ( parameter.StartsWith( PARAM_PREFIX, StringComparison.OrdinalIgnoreCase ) ) //get from file
            parameter = File.ReadAllTextAsync( CommonShared.Utilities.ValidParameterFilePath( parameter.Remove( 0, PARAM_PREFIX.Length ) ) ).Result;
         return parameter;
      }
      
      static internal void ThrowError( string error )
      {
         Environment.ExitCode = 1;
         throw new Exception( error );
      }

      [Conditional( "DEBUG" )]
      static internal void DebugWriteMessage( string message ) => WriteMessage( message );

      static internal void WriteMessage( string message )
      {
         if ( !string.IsNullOrEmpty( message ) )
            try
            {
               Console.WriteLine( string.Format( "{0}", message ) );
            }
            catch
            {
               try
               {
                  System.Diagnostics.Debug.WriteLine( string.Format( "{0}", message ) );
               }
               catch
               {
                  ;
               }
            }
      }

      static internal bool ConvertPropToBool( string prop )
      {
         return prop == "1" || prop.ToLower() == "y" || prop.ToLower() == "yes" || prop.ToLower() == "true" ? true : false;
      }
      static internal bool IsEqual( string str1, string str2, bool ignoreCase = true )
      {
         var equal = false;
         if ( ignoreCase )
            equal = string.Equals( str1, str2, StringComparison.OrdinalIgnoreCase );
         else
            equal = string.Equals( str1, str2 );
         return equal;
      }

      static internal string GetService( Arguments parameters )
      {
         var service = string.Empty;
         if ( parameters.HasParam( Common.SERVICE ) )
            service = parameters.GetArgumentValue( Common.SERVICE ).ToLowerInvariant();
         else if ( parameters.HasParam( Common.SERVICE2 ) )
            service = parameters.GetArgumentValue( Common.SERVICE2 ).ToLowerInvariant();
         return service.ToLowerInvariant();
      }
      static internal string GetAction( Arguments parameters )
      {
         var action = string.Empty;
         if ( parameters.HasParam( Common.ACTION ) )
            action = parameters.GetArgumentValue( Common.ACTION ).ToLowerInvariant();
         else if ( parameters.HasParam( Common.ACTION2 ) )
            action = parameters.GetArgumentValue( Common.ACTION2 ).ToLowerInvariant();
         return action.ToLowerInvariant();
      }
      
      static internal string GetType( Arguments parameters )
      {
         var type = string.Empty;
         if ( parameters.HasParam( Common.TYPE ) )
            type = parameters.GetArgumentValue( Common.TYPE ).ToLowerInvariant();
         else if ( parameters.HasParam( Common.TYPE2 ) )
            type = parameters.GetArgumentValue( Common.TYPE2 ).ToLowerInvariant();
         return type.ToLowerInvariant();
      }
      
      static internal void Describe( Arguments args )
      {
         Describe desc;
         try
         {
            desc = new Describe( args );
         }
         catch ( Exception ex )
         {
            Common.WriteMessage( "Error: " + ex.Message );
         }
         finally
         {
            desc = null;
         }
      }
      
      static internal void General( Arguments args )
      {
         General gen;
         try
         {
            gen = new General( args );
         }
         catch ( Exception ex )
         {
            Common.WriteMessage( "Error: " + ex.Message );
         }
         finally
         {
            gen = null;
         }
      }

      static internal void EC2Instance( Arguments args )
      {
         EC2Instance ec2;
         try
         {
            ec2 = new EC2Instance( args );
         }
         catch ( Exception ex )
         {
            Common.WriteMessage( "Error: " + ex.Message );
         }
         finally
         {
            ec2 = null;
         }
      }

      static internal void CloudEndure( Arguments args )
      {
         CloudEndure ce;
         try
         {
            ce = new CloudEndure( args );
         }
         catch ( Exception ex )
         {
            Common.WriteMessage( "Error: " + ex.Message );
         }
         finally
         {
            ce = null;
         }
      }

      static internal void IAMInstance( Arguments args )
      {
         IAMInstance iam;
         try
         {
            iam = new IAMInstance( args );
         }
         catch ( Exception ex )
         {
            Common.WriteMessage( "Error: " + ex.Message );
         }
         finally
         {
            iam = null;
         }
      }

      static internal void SSMInstance( Arguments args )
      {
         SSMInstance ssm;
         try
         {
            ssm = new SSMInstance( args );
         }
         catch ( Exception ex )
         {
            Common.WriteMessage( "Error: " + ex.Message );
         }
         finally
         {
            ssm = null;
         }
      }

      static internal void ThrowLastCreatedError( string header, string header2 )
      {
         Common.ThrowError( $"There is no last created { header } in cache. Please create a { header2 } or pass an existing valid { header }." );
      }

      static internal string NoLastCreatedMessage( string header )
      {
         return $"There is no last created { header } in cache.";
      }
   }
}
