#region history
//*****************************************************************************
// Common.cs:
//
// History:
// 08/06/20 - Goutam Malakar 
//*****************************************************************************
#endregion history

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.Util;
using AWSCM.CommonShared;

namespace AWSCM.AWSInterface
{
   internal class Common
   {
      static Common()
      {
         SetConfig();
      }

      public static string DefaultServiceUrl
      {
         get
         {
            return @"https://s3.amazonaws.com";
         }
      }

      public static RegionEndpoint DefaultRegionEndpoint { get; private set; } = RegionEndpoint.USEast2;
      
      public static string DefaultProfile { get; private set; } = DEF_PROF;
      
      public static string DefaultSharedIniFileCredentials { get; private set; } = DEF_SHARED_CREDENTIALS_FILE;

      public static string DefaultEC2ServiceUrl
      {
         get
         {
            return @"https://ec2.us-east-1.amazonaws.com";
         }
      }

      public static string DefaultLambdaServiceUrl
      {
         get
         {
            return @"https://lambda.us-east-1.amazonaws.com";
         }
      }

      public static string ParseException( Exception e, bool detail = false, string from = null )
      {
         return ExceptionUtils.ParseException( e, detail, from );
      }
      private string SetEC2DocumentAttributes( bool test = false )
      {
         var status = string.Empty;
         var documentJSON = "{}";
         this.ec2IdentityDocumentProps.Clear();

         try
         {
            this.ec2IdentityDocumentProps = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>( documentJSON = EC2InstanceMetadata.IdentityDocument );
         }
         catch ( Exception ex )
         {
            if ( test )
            {
               status = $"{status}{Environment.NewLine}Attributes Exception => {ex.Message}";
               if ( ex.InnerException != null )
                  status = $"{status}{Environment.NewLine}Attributes Inner Exception => {ex.InnerException.Message}";
            }
         }

         if ( test )
         {
            status = $"{status}{Environment.NewLine}EC2 Attributes";
            foreach ( var prop in ec2IdentityDocumentProps )
            {
               var val = string.Empty;
               if ( prop.Value != null )
                  val = prop.Value.ToString();
               status = $"{status}{Environment.NewLine}{prop.Key} : {val}";
            }
         }
         return status;
      }

      private static void RefreshConfig()
      {
         var region = CommonShared.Utilities.GetValueOfEnvVar( AWS_DEFAULT_REGION_ENV_KEY ).ToLowerInvariant();
         if ( !string.IsNullOrEmpty( region ) )
         {
            try
            {
               DefaultRegionEndpoint = RegionEndpoint.GetBySystemName( region );
            }
            catch
            {

            }
         }
         var profile = CommonShared.Utilities.GetValueOfEnvVar( AWS_PROFILE_ENV_KEY ).ToLowerInvariant().ToLowerInvariant();
         if ( !string.IsNullOrEmpty( profile ) && !profile.Equals( DEF_PROF ) )
         {
            try
            {
               DefaultProfile = profile;
            }
            catch
            {

            }
         }
         var sharedcredfile = CommonShared.Utilities.GetValueOfEnvVar( AWS_SHARED_CREDENTIALS_FILE_ENV_KEY ).ToLowerInvariant().ToLowerInvariant();
         if ( !string.IsNullOrEmpty( sharedcredfile ) && !sharedcredfile.Equals( DEF_SHARED_CREDENTIALS_FILE ) )
         {
            try
            {
               DefaultSharedIniFileCredentials = sharedcredfile;
            }
            catch
            {

            }
         }
      }

      private static void SetConfig()
      {
         RefreshConfig();
      }

      internal static bool TryGetCredentials( out AWSCredentials awsCredentials )
      {
         awsCredentials = null;
         var useSharedFile = !Common.DefaultSharedIniFileCredentials.Equals( Common.DEF_SHARED_CREDENTIALS_FILE ) || !Common.DefaultProfile.Equals( Common.DEF_PROF );
         try
         {
            if ( useSharedFile )
            {
               SharedCredentialsFile sharedFile;
               if ( !Common.DefaultSharedIniFileCredentials.Equals( Common.DEF_SHARED_CREDENTIALS_FILE ) )
                  sharedFile = new SharedCredentialsFile( Common.DefaultSharedIniFileCredentials );
               else
                  sharedFile = new SharedCredentialsFile();
               if ( sharedFile.TryGetProfile( Common.DefaultProfile, out CredentialProfile credProfile ) )
                  AWSCredentialsFactory.TryGetAWSCredentials( credProfile, sharedFile, out awsCredentials );
            }
            else //basic credentials
            {
               var awsAccessKeyId = CommonShared.Utilities.GetValueOfEnvVar( Common.AWS_ACCESS_KEY_ID_ENV_KEY );
               var awsSecretAccessKey = CommonShared.Utilities.GetValueOfEnvVar( Common.AWS_SECRET_ACCESS_KEY_ENV_KEY );
               if ( !string.IsNullOrEmpty( awsAccessKeyId ) && !string.IsNullOrEmpty( awsSecretAccessKey ) )
                  awsCredentials = new BasicAWSCredentials( awsAccessKeyId, awsSecretAccessKey );
            }
         }
         catch
         {

         }

         return awsCredentials != null;
      }

      internal static string ConvertToJSON( object objectToConvert, string name = "" )
      {
         return StringUtils.ConvertToJSONIndented( objectToConvert, name );
      }

      //internal static readonly Common CommonInstance = new Common();
      private Dictionary<string,Object> ec2IdentityDocumentProps = new Dictionary<string, Object>();
      internal const string     AWS_ACCESS_KEY_ID_ENV_KEY = @"AWS_ACCESS_KEY_ID";
      internal const string     AWS_GET_CRED_FROM_FILE= @"AWS_GET_CRED_FROM_FILE";
      internal const string     AWS_SECRET_ACCESS_KEY_ENV_KEY = @"AWS_SECRET_ACCESS_KEY";
      internal const string     AWS_SESSION_TOKEN_ENV_KEY = @"AWS_SESSION_TOKEN";
      internal const string     AWS_EC2_INSTANCE_ID_ENV_KEY = @"MG_AWS_EC2_INSTANCE_ID";
      internal const string     AWS_CURRENT_EC2_INSTANCE_ID = @"AWS_CURRENT_EC2_INSTANCE_ID";
      internal const string     AWS_DEFAULT_REGION_ENV_KEY = @"AWS_DEFAULT_REGION";
      internal const string     AWS_SHARED_CREDENTIALS_FILE_ENV_KEY = @"AWS_SHARED_CREDENTIALS_FILE";
      internal const string     AWS_PROFILE_ENV_KEY = @"AWS_PROFILE";
      internal const string     AWS_CONFIG_FILE_ENV_KEY = @"AWS_CONFIG_FILE";
      internal const string     DEF_PROF = @"default";
      internal const string     DEF_SHARED_CREDENTIALS_FILE = @"~/.aws/credentials";
      internal static readonly char[] ResourceDelimeters = { '|', ' ' };
      internal static readonly char[] ListDelimeters = { '|' };
      internal static readonly char ResourceDelimeter = '|';
      internal const string     DEFAULT = @"default";
   }
}