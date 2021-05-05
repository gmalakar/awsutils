using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.Json;
using System.Linq;
using AWSCM.CommonShared;

namespace AWSCM.CEInterface
{
   public static class Utilities
   {
      #region public methods
      public static bool CanTryLogin => !string.IsNullOrEmpty( localUserName ) && !string.IsNullOrEmpty( localPassword );

      public static bool TryLogin( string userName, string password, out string response )
      {
         if ( string.IsNullOrEmpty( userName ) )
            userName = localUserName;

         if ( string.IsNullOrEmpty( password ) )
            password = localPassword;

         var loginInfo = new Dictionary<string, string>();
         loginInfo.Add( @"username", userName );
         loginInfo.Add( @"password", password );
         var jsonString = JsonSerializer.Serialize( loginInfo );

         var res = RESTClient.InvokeApi( GetFullApiPath( LOGIN_API ), HttpMethod.Post, GetJsonContent( jsonString )  ).Result;
         response = res.Response;
         if ( res.Success )
         {
            SetUserName( userName );
            SetUserPassword( password );
            CommonShared.FileBasedCache.AddOrUpdate( CE_ACCOUNT_INFO, response );
            CommonShared.FileBasedCache.AddOrUpdate( CE_SESSION_COOKIES, res.Cookies );
         }
         return res.Success;
      }

      public static bool TryLogout( out string response )
      {
         SetSession();
         var res = RESTClient.InvokeApi( GetFullApiPath( lOGOUT_API ), HttpMethod.Post, null, authHeader, authCookies  ).Result;
         response = res.Response;
         CommonShared.FileBasedCache.TryRemove( CE_ACCOUNT_INFO );
         CommonShared.FileBasedCache.TryRemove( CE_SESSION_COOKIES );
         return res.Success;
      }

      public static bool TryLogin( out string response )
      {
         var success = false;
         if ( CanTryLogin )
            success = TryLogin( localUserName, localPassword, out response );
         else
            response = "Credentials are not available in cache!.";
         return success;
      }

      public static bool TryGetInstalationToken( string projectName, out string instalationToken )
      {
         var projectInstallationKey = GetProjectInstallationKey( projectName );
         var success = false;
         instalationToken = string.Empty;
         if ( !FileBasedCache.TryGet( projectInstallationKey, out instalationToken, out _ ) )
         {
            //get projects
            success = TryGetProject( projectName, out JsonElement project );
            if ( success )
            {
               JsonElement token = new JsonElement();
               success = project.ValueKind != JsonValueKind.Undefined && project.TryGetProperty( AGENT_INS_TOKEN, out token );
               if ( success )
               {
                  instalationToken = token.GetString();
                  CommonShared.FileBasedCache.AddOrUpdate( projectInstallationKey, instalationToken );
               }
            }
         }
         else
            success = true;

         return success;
      }

      public static bool TryCreateProject( string projectName, out string project )
      {
         //get projects
         project = string.Empty;
         var success = TryCreateProject( out JsonElement project2 );
         if ( success )
            project = StringUtils.ConvertToJSONIndented( project2 );
         return success;
      }

      public static bool TryGetProject( string projectName, out string project )
      {
         //get projects
         project = string.Empty;
         var success = TryGetProject( projectName, out JsonElement project2 );
         if ( success )
            project = StringUtils.ConvertToJSONIndented( project2 );
         return success;

      }

      public static bool TryGetCloud( string cloudName, out string cloud )
      {
         //get projects
         cloud = string.Empty;
         var success = TryGetCloud( cloudName, out JsonElement cloud2 );
         if ( success )
            cloud = StringUtils.ConvertToJSONIndented( cloud2 );
         return success;

      }

      public static bool TryGetCreds( string cloudName, string publickey, out string cred )
      {
         //get projects
         cred = string.Empty;
         var success = TryGetCreds( cloudName, publickey, out JsonElement cred2 );
         if ( success )
            cred = StringUtils.ConvertToJSONIndented( cred2 );
         return success;

      }

      public static bool TryGetRegions( string cloudName, string regionName, out string region )
      {
         //get projects
         region = string.Empty;
         var success = TryGetRegions( cloudName, regionName, out JsonElement region2 );
         if ( success )
            region = StringUtils.ConvertToJSONIndented( region2 );
         return success;
      }

      public static bool TryGetReplicationConfigs( string prohectName, out string replConfigs )
      {
         replConfigs = string.Empty;
         var success = TryGetReplicationConfigs( prohectName, out JsonElement replConfigs2 );
         if ( success )
            replConfigs = StringUtils.ConvertToJSONIndented( replConfigs2 );
         return success;
      }

      public static string GetMaskedUserId()
      {
         var userName = CommonShared.Utilities.GetValueOfEnvVar( CE_USERID_ENV_KEY );
         if ( userName.Length > 0 )
         {
            var splituserid = userName.Split( '@');
            if ( splituserid.Count() > 1 )
               userName = $"{splituserid[0].Substring( 0, 1 )}********{splituserid[0].Substring( splituserid[0].Length - 1, 1 )}@{splituserid[1]} ";
            else
               userName = CommonShared.StringUtils.MaskString( userName, 1, 16 );
         }
         return userName;
      }

      public static bool HasCrrentPassword { get { return !string.IsNullOrEmpty( localPassword ); } }

      public static bool TryGetAllProjects( out string response )
      {
         //get projects
         response = string.Empty;
         var success = TryGetAllProjects(  out JsonElement project2 );
         if ( success )
            response = StringUtils.ConvertToJSONIndented( project2 );
         return success;
      }

      public static bool TryGetAllClouds( out string response )
      {
         //get projects
         response = string.Empty;
         var success = TryGetAllClouds(  out JsonElement clouds );
         if ( success )
            response = StringUtils.ConvertToJSONIndented( clouds );
         return success;
      }

      public static bool TryGetAllCreds( out string response )
      {
         //get projects
         response = string.Empty;
         var success = TryGetAllCreds(  out JsonElement creds );
         if ( success )
            response = StringUtils.ConvertToJSONIndented( creds );
         return success;
      }

      #endregion public methods

      #region private methods
      private static bool TryGetAuthToken( out string authToken, out string authSession )
      {
         var succes = false;
         authToken = null;
         authSession = null;
         if ( TryGetCookies( out Dictionary<string, string> cookies ) )
         {
            if ( cookies != null && cookies.TryGetValue( AUTH_TOKEN, out authToken ) )
               succes = true;
            else
               throw new Exception( $"Error: Invalid token, Please configure to set credentials. Reason: {AUTH_TOKEN} is not available." );
            if ( cookies != null && cookies.TryGetValue( AUTH_SESSION, out authSession ) )
               succes = true;
            else
               throw new Exception( $"Error: Invalid session, Please configure to set credentials. Reason: {AUTH_SESSION} is not available." );
         }
         return succes;
      }

      private static bool TryGetCookies( out Dictionary<string, string> cookies )
      {
         var succes = true;
         if ( !FileBasedCache.TryGet( CE_SESSION_COOKIES, out cookies, out string message ) )
         {
            if ( !TryLogin( out message ) )
               throw new Exception( $"Error: Invalid session, Please configure to set credentials. Reason: {message} " );
         }
         return succes;
      }

      private static void SetAuthHeaders()
      {
         if ( !authHeaderSet && TryGetAuthToken( out string authToken, out string authSession ) )
         {
            authHeader.Clear();
            authHeader.TryAdd( AUTH_HEADER, authToken.TrimStart( '\"' ).TrimEnd( '\"' ) );
            authCookies.Clear();
            authCookies.TryAdd( AUTH_SESSION, authSession.TrimStart( '\"' ).TrimEnd( '\"' ) );
            authHeaderSet = true;
         }
      }

      private static void SetSession()
      {
         SetAuthHeaders();
         CheckMe( out bool loggedAgain );
         if ( loggedAgain )
         {
            authHeaderSet = false;
            SetAuthHeaders();
         }
      }

      private static void CheckMe( out bool loggedAgain )
      {
         var sessionExists = false;
         loggedAgain = false;
         var res = RESTClient.InvokeApi( GetFullApiPath( ME_API ), HttpMethod.Get, null, authHeader, authCookies ).Result;
         if ( res.Success )
         {
            try
            {
               sessionExists = JsonDocument.Parse( res.Response ).RootElement.GetProperty( @"status" ).ToString().Equals( @"CONFIRMED", StringComparison.OrdinalIgnoreCase );
            }
            catch
            {
               //pass
            }
         }
         if ( !sessionExists )
         {
            sessionExists = TryLogin( out string response );
            if ( !sessionExists )
               throw new Exception( response );
            else
               loggedAgain = true;
         }
      }

      private static bool TryGetProjectID( string projectName, out string ID )
      {
         ID = string.Empty;
         var success = TryGetProject( projectName, out JsonElement project );
         if ( success )
            ID = project.GetProperty( @"id" ).GetString();
         return success;
      }

      private static bool TryGetProject( string projectName, out JsonElement project )
      {
         //get projects
         var success = FileBasedCache.TryGet<JsonElement>( projectName.ToUpperInvariant(), out project, out _ );
         if ( !success )
         {
            success = TryGetAllProjects( out JsonElement allProjects );
            if ( success )
            {
               project = allProjects.EnumerateArray()
                  .FirstOrDefault( i => i.GetProperty( @"name" ).GetString().Equals( projectName, StringComparison.OrdinalIgnoreCase ) );

               success = project.ValueKind != JsonValueKind.Undefined && project.TryGetProperty( @"name", out _ );
               if ( success )
                  CommonShared.FileBasedCache.AddOrUpdate<JsonElement>( projectName.ToUpperInvariant(), project );
            }
         }
         return success;
      }

      private static bool TryGetCloudID( string cloudName, out string id )
      {
         id = string.Empty;
         var success = TryGetCloud( cloudName, out JsonElement cloud );
         if ( success )
            id = cloud.GetProperty( @"id" ).GetString();
         return success;
      }

      private static bool TryGetCloud( string cloudName, out JsonElement cloud )
      {
         //get projects
         cloud = default( JsonElement );
         var success = TryGetAllClouds( out JsonElement allClouds );

         if ( success )
         {
            cloud = allClouds.EnumerateArray()
               .FirstOrDefault( i => i.GetProperty( @"name" ).GetString().Equals( cloudName, StringComparison.OrdinalIgnoreCase ) );

            success =  cloud.ValueKind != JsonValueKind.Undefined && cloud.TryGetProperty( @"name", out _ );
         }

         return success;
      }

      private static bool TryGetCredsID( string cloudName, string publickey, out string ID, out string publicKey, out string accountIdentifier )
      {
         ID = string.Empty;
         publicKey = publickey;
         accountIdentifier = string.Empty;

         var success = TryGetCreds( cloudName, publickey, out JsonElement creds );
         if ( success )
         {
            JsonElement id = new JsonElement();
            success = creds.ValueKind != JsonValueKind.Undefined && creds.TryGetProperty( @"id", out id );
            if ( success )
            {
               ID = id.ToString();
               publicKey = creds.GetProperty( @"publicKey" ).ToString();
               accountIdentifier = creds.GetProperty( @"accountIdentifier" ).ToString();
            }
         }

         return success;
      }

      private static bool TryGetCreds( string cloudName, string publickey, out JsonElement creds )
      {
         //get projects
         creds = default( JsonElement );
         var success = TryGetCloudID( cloudName, out string cloudId );
         if ( success )
         {
            success = TryGetAllCreds( out JsonElement allCreds );

            if ( success )
            {
               creds = allCreds.EnumerateArray()
                  .FirstOrDefault( i => i.GetProperty( @"cloud" ).GetString().Equals( cloudId, StringComparison.OrdinalIgnoreCase )
                  && ( string.IsNullOrEmpty( publickey ) || i.GetProperty( @"publicKey" ).GetString().Equals( publickey, StringComparison.OrdinalIgnoreCase ) )
                  );

               success = creds.ValueKind != JsonValueKind.Undefined && creds.TryGetProperty( @"cloud", out _ );
            }
         }

         return success;
      }

      private static bool TryGetRegions( string cloudName, string regionName, out JsonElement regions )
      {
         //get projects
         regions = new JsonElement();
         var success = TryGetCredsID( cloudName, null, out string id, out _, out _ );
         if ( success )
         {
            var key = $"{id}-{ALL_REGIONS_KEY}";
            success = FileBasedCache.TryGet<JsonElement>( key, out regions, out _ );
            if ( !success )
            {
               SetSession();
               var res = RESTClient.InvokeApi( GetFullApiPath( GetRegionAPI( id ) ), HttpMethod.Get, null, authHeader, authCookies ).Result;
               var response = res.Response;
               success = res.Success;
               if ( res.Success )
               {
                  var regions2 = JsonDocument.Parse( response ).RootElement.GetProperty( @"items" );

                  var regions3 = new List< Dictionary<string, string>>();

                  foreach ( var item in regions2.EnumerateArray() )
                  {
                     var newItem = new Dictionary<string, string>();
                     var name = item.GetProperty( "name" ).ToString();
                     newItem.Add( @"name", name );
                     newItem.Add( @"cloud", cloudName );
                     newItem.Add( @"id", item.GetProperty( "id" ).ToString() );
                     regions3.Add( newItem );
                  }

                  regions = StringUtils.ConvertFromJSON<JsonElement>( StringUtils.ConvertToJSON( regions3 ) );

                  CommonShared.FileBasedCache.AddOrUpdate<JsonElement>( key, regions );
               }
               else
                  throw new Exception( response );
            }

            if ( success ) //filter 
            {
               if ( !string.IsNullOrEmpty( regionName ) ) //filter 
               {
                  var filtered = new List<JsonElement>();
                  foreach ( var region in regions.EnumerateArray().Where( i => i.GetProperty( "name" ).ToString().StartsWith( regionName, StringComparison.OrdinalIgnoreCase ) ) )
                     filtered.Add( region );
                  regions = StringUtils.ConvertFromJSON<JsonElement>( StringUtils.ConvertToJSON( filtered ) );
               }
            }
         }
         return success;
      }

      private static bool TryGetReplicationConfigs( string projectName, out JsonElement replconfigs )
      {
         replconfigs = new JsonElement();
         var success = TryGetProjectID( projectName, out string id );
         if ( success )
         {
            var key = $"{id}-{ALL_REPL_CONFIGS_KEY}";
            success = FileBasedCache.TryGet<JsonElement>( key, out replconfigs, out _ );
            if ( !success )
            {
               SetSession();
               var res = RESTClient.InvokeApi( GetFullApiPath( GetReplAPI( id ) ), HttpMethod.Get, null, authHeader, authCookies ).Result;
               var response = res.Response;
               success = res.Success;
               if ( res.Success )
               {
                  replconfigs = JsonDocument.Parse( response ).RootElement.GetProperty( @"items" );

                  CommonShared.FileBasedCache.AddOrUpdate<JsonElement>( key, replconfigs );
               }
               else
                  throw new Exception( response );
            }
         }
         return success;
      }

      private static bool TryGetAllProjects( out JsonElement allProjects )
      {
         var success = FileBasedCache.TryGet<JsonElement>( ALL_PROJECTS_KEY, out allProjects, out _ );
         if ( !success )
         {
            SetSession();
            var res = RESTClient.InvokeApi( GetFullApiPath( PROJECTS_API ), HttpMethod.Get, null, authHeader, authCookies ).Result;
            var response = res.Response;
            success = res.Success;
            if ( res.Success )
            {
               allProjects = JsonDocument.Parse( response ).RootElement.GetProperty( @"items" );
               CommonShared.FileBasedCache.AddOrUpdate<JsonElement>( ALL_PROJECTS_KEY, allProjects );
            }
            else
               throw new Exception( response );
         }
         return success;
      }

      private static bool TryGetAllClouds( out JsonElement allClouds )
      {
         var success = FileBasedCache.TryGet<JsonElement>( ALL_CLOUDS_KEY, out allClouds, out _ );
         if ( !success )
         {
            SetSession();
            var res = RESTClient.InvokeApi( GetFullApiPath( CLOUDS_API ), HttpMethod.Get, null, authHeader, authCookies ).Result;
            var response = res.Response;
            success = res.Success;
            if ( res.Success )
            {
               allClouds = JsonDocument.Parse( response ).RootElement.GetProperty( @"items" );
               CommonShared.FileBasedCache.AddOrUpdate<JsonElement>( ALL_CLOUDS_KEY, allClouds );
            }
            else
               throw new Exception( response );
         }
         return success;
      }

      private static bool TryGetAllCreds( out JsonElement allCreds )
      {
         var success = FileBasedCache.TryGet<JsonElement>( ALL_CREDS_KEY, out allCreds, out _ );
         if ( !success )
         {
            SetSession();
            var res = RESTClient.InvokeApi( GetFullApiPath( CREDS_API ), HttpMethod.Get, null, authHeader, authCookies ).Result;
            var response = res.Response;
            success = res.Success;
            if ( res.Success )
            {
               allCreds = JsonDocument.Parse( response ).RootElement.GetProperty( @"items" );
               CommonShared.FileBasedCache.AddOrUpdate<JsonElement>( ALL_CREDS_KEY, allCreds );
            }
            else
               throw new Exception( response );
         }
         return success;
      }

      private static bool TryCreateProject( out JsonElement allProjects )
      {

         SetSession();
         ;

         var res = RESTClient.InvokeApi( GetFullApiPath( PROJECTS_API ), HttpMethod.Post,GetJsonContent(File.ReadAllText( CommonShared.Utilities.ValidParameterFilePath("createceproject.json") )) , authHeader, authCookies ).Result;
         var response = res.Response;
         var success = res.Success;
         if ( res.Success )
         {
            allProjects = JsonDocument.Parse( response ).RootElement.GetProperty( @"items" );
            CommonShared.FileBasedCache.AddOrUpdate<JsonElement>( ALL_PROJECTS_KEY, allProjects );
         }
         else
            throw new Exception( response );

         return success;
      }

      private static ByteArrayContent GetJsonContent( string json )
      {
         var content = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(json));
         content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( "application/json" );
         return content;
      }

      //static internal string GetApiTemplate( string templateName )
      //{
      //   return File.ReadAllText( GetAPITemplatePathFromResources( templateName ) );
      //}


      //static string GetAPIPathFromResources( string apiname )
      //{
      //   return Path.Combine( CERootUrl, resources.Resources.ResourceManager.GetString( $"{apiname}{PATH_POSTFIX}" ) );
      //}

      private static string GetFullApiPath( string apiname )
      {
         return Path.Combine( CERootUrl, apiname );
      }

      private static string GetAPITemplatePathFromResources( string apiname )
      {
         var path = Path.Combine( CETemplateFolder, resources.Resources.ResourceManager.GetString( $"{apiname}{TEMPLATE_POSTFIX}" ) );
         return path;
      }

      private static string GetProjectInstallationKey( string projectName )
      {
         return $"{projectName}-agent-installation-token";
      }

      private static void SetUserName( string userName )
      {
         if ( !CommonShared.StringUtils.IfNull( userName ) )
            CommonShared.Utilities.SetValueOfEnvVar( CE_USERID_ENV_KEY, userName );
         else
            CommonShared.Utilities.SetValueOfEnvVar( CE_USERID_ENV_KEY, null );
         localUserName = userName;
      }

      private static void SetUserPassword( string password )
      {
         if ( !CommonShared.StringUtils.IfNull( password ) )
            CommonShared.Utilities.SetValueOfEnvVar( CE_PWD_ENV_KEY, password );
         else
            CommonShared.Utilities.SetValueOfEnvVar( CE_PWD_ENV_KEY, null );
         localPassword = password;
      }

      private static string GetRegionAPI( string credentialId )
      {
         return string.Format( REGION_API, credentialId );
      }

      private static string GetReplAPI( string projectId )
      {
         return string.Format( REPL_API, projectId );
      }
      
      #endregion private methods

      #region constant

      const string LOGIN_API_NAME = @"CE_LOGIN";
      const string PATH_POSTFIX = @"_API";
      const string TEMPLATE_POSTFIX = @"_TEMPLATE";
      const string CE_SESSION_COOKIES = @"ce_session_cookies";
      const string CE_ACCOUNT_INFO = @"ce_account_info";
      const string AUTH_HEADER = @"X-XSRF-TOKEN";
      const string PROJECTS_API = @"projects";
      const string CLOUDS_API = @"clouds";
      const string ME_API = @"me";
      const string CREDS_API = @"cloudCredentials";
      const string REGION_API = @"cloudCredentials/{0}/regions";
      const string REPL_API = @"projects/{0}/replicationConfigurations";
      const string LOGIN_API = @"login";
      const string lOGOUT_API = @"logout";
      const string AUTH_TOKEN = @"XSRF-TOKEN";
      const string AUTH_SESSION = @"session";
      const string ALL_PROJECTS_KEY = @"ce-allprojects";
      const string ALL_CLOUDS_KEY = @"ce-allclouds";
      const string ALL_REGIONS_KEY = @"ce-allregions";
      const string ALL_REPL_CONFIGS_KEY = @"ce-allreplconfigs";
      const string ALL_CREDS_KEY = @"ce-allcreds";
      const string AGENT_INS_TOKEN = @"agentInstallationToken";
      const string CE_USERID_ENV_KEY = @"CE_USERID";
      const string CE_PWD_ENV_KEY= @"CE_PWD";

      #endregion constant

      #region other properties
      private static readonly string CERootUrl = CEInterface.resources.Resources.CE_ROOT_PATH;
      private static readonly string CERootPath = CommonShared.Utilities.AssemblyHomeDirectory;

      private static string CETemplateFolder { get; set; } = Path.Combine( CERootPath, @"templates" );
      //public static string CETemplateExt { get; set; } = @".json";

      private static string localUserName = CommonShared.Utilities.GetValueOfEnvVar( CE_USERID_ENV_KEY );
      private static string localPassword = CommonShared.Utilities.GetValueOfEnvVar( CE_PWD_ENV_KEY );
      private static bool authHeaderSet = false;

      private static Dictionary<string, string> authHeader = new Dictionary<string, string>();
      private static Dictionary<string, string> authCookies = new Dictionary<string, string>();
      #endregion other properties

      #region LAST

      public static string LastSetCreatedProjectID
      {
         get { return CommonShared.CacheManager.Get( CECacheManager.Constants.LAST_SET_CREATED_PROJECT_ID_KEY ); }
      }


      public static string LastSetCreatedCloudID
      {
         get { return CommonShared.CacheManager.Get( CECacheManager.Constants.LAST_SET_CREATED_CLOUD_ID_KEY ); }
      }

      public static string LastSetCreatedCredentialsID
      {
         get { return CommonShared.CacheManager.Get( CECacheManager.Constants.LAST_SET_CREATED_CREDS_ID_KEY ); }
      }

      public static string LastSetCreatedRegionID
      {
         get { return CommonShared.CacheManager.Get( CECacheManager.Constants.LAST_SET_CREATED_REGION_ID_KEY ); }
      }

      #endregion
   }
}
