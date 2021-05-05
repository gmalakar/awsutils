#region history
//*****************************************************************************
// General.cs:
//
// History:
// 08/06/20 - Goutam Malakar 
//*****************************************************************************
#endregion history
using System;
using System.Security;
using System.Threading;

namespace AWSCM.AWSConfigManager.Utilities
{
   public class General
   {
      private Arguments parameters;
      public General( Arguments args )
      {
         parameters = args;
         switch ( Common.GetService( parameters ) )
         {
            case @"clearcache":
            case @"cc":
               this.DoClearCache();
               break;
            case @"config":
               this.DoConfigs();
               break;
            case @"configure":
               this.DoConfigure();
               break;       
            case @"configurece":
               this.DoSetConfigureCE();
               break;
            case @"crawl":
               this.DoCrawl();
               break;
            case @"sleep":
               this.DoSleep();
               break;
            case @"cipher":
               this.DoCipher();
               break;
            default:
               Common.ThrowError( "Invalid service!" );
               break;
         }
      }

      private void DoConfigs()
      {
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"set":
               this.DoSetConfigs();
               break;
            case @"clear":
               this.DoClearConfigs();
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }
      }

      private void DoConfigure()
      {
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"set":
               this.DoSetConfigs();
               break;
            case @"clear":
               this.DoClearConfigs();
               break;
            default:
               this.DoSetConfigure();
               break;
         }
      }

      private void DoSetConfigure()
      {
         Console.WriteLine( $"AWS Access Key ID [{ CommonShared.StringUtils.NoneIfNull( AWSInterface.Utilities.GetMaskedAccessKey() ) }]:" );
         var key = Console.ReadLine();
         CommonShared.Utilities.DeletePrevConsoleLine();
         if ( !string.IsNullOrEmpty( key ) )
            AWSInterface.Utilities.SetKey( key );

         Console.WriteLine( $"AWS Secret Access Key [{ CommonShared.StringUtils.NoneIfNull( AWSInterface.Utilities.GetMaskedSecretKey() ) }]:" );
         var secret = Console.ReadLine();
         CommonShared.Utilities.DeletePrevConsoleLine();
         if ( !string.IsNullOrEmpty( secret ) )
            AWSInterface.Utilities.SetSecretKey( secret );

         Console.WriteLine( $"Default region name [{ CommonShared.StringUtils.NoneIfNull( AWSInterface.Utilities.GetDefaultRegion() ) }]:" );
         var region = Console.ReadLine();

         if ( !string.IsNullOrEmpty( region ) )
            AWSInterface.Utilities.SetRegion( region );

         Console.WriteLine( $"Default Data Directory [{ CommonShared.StringUtils.NoneIfNull( AWSInterface.Utilities.GetDataDirectory() ) }]:" );
         var dd = Console.ReadLine();

         if ( !string.IsNullOrEmpty( dd ) )
            AWSInterface.Utilities.SetDataPath( dd );
      }

      private void DoSetConfigureCE()
      {
         var currentUserId = CommonShared.StringUtils.NoneIfNull( CEInterface.Utilities.GetMaskedUserId() );
         Console.WriteLine( $"Cloud Endure Current UserName [{ currentUserId }]:" );
         var username = Console.ReadLine();
         var currentPwd = CEInterface.Utilities.HasCrrentPassword ? @" *******************" : @"None";

         Console.WriteLine( $"Cloud Endure Current Password [{ currentPwd } ]:" );
         var pwd = string.Empty;

         CommonShared.Utilities.DoReadPassword( ref pwd );

         if ( ( string.IsNullOrEmpty( username ) && string.IsNullOrEmpty( currentUserId ) )
            || ( !CEInterface.Utilities.HasCrrentPassword && string.IsNullOrEmpty( pwd ) ) )
            Console.WriteLine( $"Please enter both CloudEndure UserID and Password to reconfigure" );
         else
         {
            if ( !CEInterface.Utilities.TryLogin( username, pwd, out string response ) )
               Console.WriteLine( response );
            else
               Console.WriteLine( @"Successfully logged in and created CloudeEndure session!" );
         }
      }

      private void DoSetConfigs()
      {
         if ( parameters.HasParam( @"credpath" ) )
         {
            AWSInterface.Utilities.SetSharedCredentialProfilePath( parameters.GetArgumentValue( @"credpath" ) );
            Common.WriteMessage( @"Credential path is set!" );
         }
         if ( parameters.HasParam( @"region" ) )
         {
            var region =  parameters.GetArgumentValue( @"region" );
            AWSInterface.Utilities.SetRegion( region );
            Common.WriteMessage( $"Region is set to [{ region }]" );
         }
         if ( parameters.HasParam( @"awsacceskey" ) )
         {
            AWSInterface.Utilities.SetKey( parameters.GetArgumentValue( @"awsacceskey" ) );
            Common.WriteMessage( $"AwsAccesKey is set." );
         }
         if ( parameters.HasParam( @"awssecret" ) )
         {
            AWSInterface.Utilities.SetSecretKey( parameters.GetArgumentValue( @"awssecret" ) );
            Common.WriteMessage( $"AwsSecret is set." );
         }
         if ( parameters.HasParam( @"awssessiontoken" ) )
         {
            AWSInterface.Utilities.SetToken( parameters.GetArgumentValue( @"awssessiontoken" ) );
            Common.WriteMessage( $"AwsSessionToken is set." );
         }
         if ( parameters.HasParam( @"profile" ) )
         {
            var profile =  parameters.GetArgumentValue( @"profile" );
            AWSInterface.Utilities.SetProfile( profile );
            Common.WriteMessage( $"Profile is set to [{ profile }]." );
         }
         if ( parameters.HasParam( @"configpath" ) )
         {
            var configpath =  parameters.GetArgumentValue( @"configpath" );
            AWSInterface.Utilities.SetConfigPath( configpath );
            Common.WriteMessage( $"ConfigPath is set to [{configpath}]." );
         }
         if ( parameters.HasParam( @"datapath" ) )
         {
            var datapath =  parameters.GetArgumentValue( @"datapath" );
            AWSInterface.Utilities.SetDataPath( datapath );
            Common.WriteMessage( $"DataPath is set to [{datapath}]." );
         }
      }

      private void DoClearConfigs()
      {
         AWSInterface.Utilities.SetSharedCredentialProfilePath( null );
         AWSInterface.Utilities.SetRegion( null );
         AWSInterface.Utilities.SetKey( null );
         AWSInterface.Utilities.SetSecretKey( null );
         AWSInterface.Utilities.SetToken( null );
         AWSInterface.Utilities.SetProfile( null );
         AWSInterface.Utilities.SetConfigPath( null );
         AWSInterface.Utilities.SetDataPath( null );
         Common.WriteMessage( @"Cleared configs successfully!" );
      }

      //private void DoDescribe()
      //{
      //   var vpcid = string.Empty;
      //   switch ( Common.GetType( this.parameters ) )
      //   {
      //      case @"insprofile":
      //      case @"instanceprofile":
      //         var profileName = parameters.GetArgumentValue( @"profilename", false );
      //         Common.WriteMessage( AWSInterface.Utilities.DescribeInsProfile( profileName ) );
      //         break;
      //      case @"role":
      //         var rolename = parameters.GetArgumentValue( @"rolename", false );
      //         Common.WriteMessage( AWSInterface.Utilities.DescribeIAMRoles( rolename ) );
      //         break;
      //      case @"policy":
      //         var polname = parameters.GetArgumentValue( @"policyname", false );
      //         Common.WriteMessage( AWSInterface.Utilities.DescribeIAMPoliies( polname ) );
      //         break;
      //      case @"ec2":
      //         var id = parameters.GetArgumentValue( @"instanceid", false );
      //         Common.WriteMessage( AWSInterface.Utilities.DescribeInstances( id ) );
      //         break;
      //      case @"sg":
      //         vpcid = parameters.GetArgumentValue( @"vpcid", false );
      //         var gid = parameters.GetArgumentValue( @"groupid", false );
      //         var gname = parameters.GetArgumentValue( @"groupname", false );
      //         Common.WriteMessage( AWSInterface.Utilities.DescribeSecurityGroups( vpcid, gid, gname ) );
      //         break;
      //      case @"ig":
      //         vpcid = parameters.GetArgumentValue( @"vpcid", false );
      //         var igid = parameters.GetArgumentValue( @"internetgatewayid", false );
      //         Common.WriteMessage( AWSInterface.Utilities.DescribeIG( igid, vpcid ) );
      //         break;
      //      case @"vpc":
      //         vpcid = parameters.GetArgumentValue( @"vpcid", false );
      //         Common.WriteMessage( AWSInterface.Utilities.DescribeVpcs( vpcid ) );
      //         break;
      //      case @"sn":
      //      case @"subnet":
      //         var snid = parameters.GetArgumentValue( @"subnetid", false );
      //         vpcid = parameters.GetArgumentValue( @"vpcid", false );
      //         Common.WriteMessage( AWSInterface.Utilities.DescribeSubnets( snid, vpcid ) );
      //         break;
      //      case @"ami":
      //      case @"image":
      //         var imageid = parameters.GetArgumentValue( @"id", false );
      //         Common.WriteMessage( AWSInterface.Utilities.DescribeImages( imageid ) );
      //         break;
      //      case @"kp":
      //         var keyname = parameters.GetArgumentValue( @"keyname", false );
      //         Common.WriteMessage( AWSInterface.Utilities.DescribeKeyPairs( keyname ) );
      //         break;
      //      case @"tags":
      //         var resources = parameters.GetArgumentValue( @"resources", false );
      //         var key = parameters.GetArgumentValue( @"key", false );
      //         Common.WriteMessage( AWSInterface.Utilities.DescribeTags( resources, key ) );
      //         break;
      //      case @"ssminsinfo":
      //         Common.WriteMessage( AWSInterface.Utilities.DescribeInstanceInfo() );
      //         break;
      //      default:
      //         Common.WriteMessage( AWSInterface.Utilities.DescribeReservations() );
      //         break;
      //   }
      //}

      private void DoCrawl()
      {
         Common.WriteMessage( Crawl( parameters.GetArgumentValue( @"url" ) ) );
      }
      private void DoSleep()
      {
         var sleepTime = parameters.GetArgumentValueAsInteger( @"time", 0 );
         if ( sleepTime > 0 )
         {
            Common.WriteMessage( $"Sleeping thread for [{ sleepTime } Seconds].." );
            Thread.Sleep( sleepTime * 1000 );
         }
      }

      private void DoCipher()
      {
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"encrypt":
               var plaintext = parameters.GetArgumentValue( @"plaintext" );
               var key = parameters.GetArgumentValue( @"key", false );
               Common.WriteMessage( CommonShared.Symmetric.EncryptString( key, plaintext ) );
               break;
            case @"decrypt":
               var ciphertext = parameters.GetArgumentValue( @"ciphertext" );
               var key2 = parameters.GetArgumentValue( @"key", false );
               Common.WriteMessage( CommonShared.Symmetric.DecryptString( key2, ciphertext ) );
               break;
            default:
               Common.WriteMessage( "Invalid action!" );
               break;
         }
      }

      private void DoClearCache()
      {
         CommonShared.FileBasedCache.ClearCache();
         Common.WriteMessage( @"Cleared cache successfully!" );
      }
      public static string Crawl( string url )
      {
         var response = CommonShared.RESTClient.InvokeApi( url ).Result;
         return response.Response;
      }

   }
}
