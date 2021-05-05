#region history
//*****************************************************************************
// General.cs:
//
// History:
// 08/06/20 - Goutam Malakar 
//*****************************************************************************
#endregion history
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;

namespace AWSCM.AWSConfigManager.Utilities
{
   public class Describe
   {
      private Arguments parameters;
      private string filters = string.Empty;
      private string type = string.Empty;
      private static List<string> notValidFilters = new List<string>(){ "s", "service", "t", "type", "a", "action", "u", "debug" };
      private static Dictionary<string, string > specificFilterMapping = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
      {
         {@"instanceid", @"instance-id"}
         ,{@"vpcid", @"vpc-id"}
         ,{@"groupid", @"group-id"}
         ,{@"groupname", @"group-name"}
         ,{@"internetgatewayid", @"internet-gateway-id"}
         ,{@"subnetid", @"subnet-id"}
         ,{@"imageid", @"image-id"}
         ,{@"keyname", @"key-name"}
         ,{@"resources", @"resource-id"}
         ,{@"key", @"key"}
      };

      public Describe( Arguments args )
      {
         parameters = args;
         type = Common.GetType( this.parameters );
         filters = parameters.GetArgumentValue( @"filters", false );
         this.DoDescribe();
      }

      private void AddToFilter( string param, string filtername = null )
      {
         if ( parameters.HasParam( param ) )
         {
            if ( string.IsNullOrEmpty( filtername ) )
               filtername = param;
            filters = $" Name={ filtername }, Values={parameters.GetArgumentValue( param ).Replace( '|', ',' )}";
         }
      }

      private void MakeFilters()
      {
         foreach ( DictionaryEntry param in parameters.Parameters )
         {
            var key = param.Key.ToString().ToLower();
            if ( param.Value != null && !notValidFilters.Contains( key ) )
            {
               var value = param.Value.ToString();
               if ( !string.IsNullOrEmpty( value ) )
               {
                  var altermetKey = string.Empty;
                  if ( !specificFilterMapping.TryGetValue( key, out altermetKey ) )
                     altermetKey = key;
                  filters = $"{filters}{AWSInterface.Utilities.CreateFilterString( altermetKey, value )}";
               }
            }
         }
      }

      private void DoDescribe()
      {
         var vpcid = string.Empty;
         switch ( type )
         {
            case @"insprofile":
            case @"instanceprofile":
               var profileName = parameters.GetArgumentValue( @"profilename", false );
               Common.WriteMessage( AWSInterface.Utilities.DescribeInsProfile( profileName ) );
               break;
            case @"role":
               var rolename = parameters.GetArgumentValue( @"rolename", false );
               Common.WriteMessage( AWSInterface.Utilities.DescribeIAMRoles( rolename ) );
               break;
            case @"policy":
               var polname = parameters.GetArgumentValue( @"policyname", false );
               Common.WriteMessage( AWSInterface.Utilities.DescribeIAMPoliies( polname ) );
               break;
            case @"ssminsinfo":
               Common.WriteMessage( AWSInterface.Utilities.DescribeInstanceInfo() );
               break;
            default:
               MakeFilters();
               //AddToFilter( @"instanceid", @"instance-id" );
               //AddToFilter( @"vpcid", @"vpc-id" );
               //AddToFilter( @"groupid", @"group-id" );
               //AddToFilter( @"groupname", @"group-name" );
               //AddToFilter( @"internetgatewayid", @"internet-gateway-id" );
               //AddToFilter( @"subnetid", @"subnet-id" );
               //AddToFilter( @"imageid", @"image-id" );
               //AddToFilter( @"keyname", @"key-name" );
               //AddToFilter( @"resources", @"resource-id" );
               //AddToFilter( @"key", @"key" );
               Common.WriteMessage( AWSInterface.Utilities.Describe( type, filters ) );
               break;
         }
      }

   }
}
