using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Amazon.EC2.Model;

namespace AWSCM.AWSInterface
{
   public static class Utilities
   {

      #region parameter conversion

      public static List<Filter> ConvertToFilter( string filters )
      {
         var split1 = filters.Split( CommonShared.StringUtils.FilterDelemitters, StringSplitOptions.RemoveEmptyEntries );
         var nameValuesList = new List<string>();
         foreach ( var s in split1 )
            if ( !string.IsNullOrEmpty( s ) && !CommonShared.StringUtils.FilterIgnores.Contains( s ) )
            {
               nameValuesList.Add( s.TrimStart( CommonShared.StringUtils.FilterTrim ).TrimEnd( CommonShared.StringUtils.FilterTrim ) );
               //var p = pair.TrimStart( trim ).TrimEnd( trim );
               //json = $"{json}{p}{Environment.NewLine}";
            }
         var listFilte = new List<Filter>();
         for ( int i = 0; i < nameValuesList.Count; i += 2 )
         {
            listFilte.Add( new Filter() { Name = nameValuesList[i], Values = nameValuesList[i + 1].Split( CommonShared.StringUtils.FilterValusSplitter ).ToList<string>() } );
         }
         return listFilte;
      }

      #endregion

      #region  CRED AND OTHERS
      public static void SetSharedCredentialProfilePath( string path )
      {
         if ( !CommonShared.StringUtils.IfNull( path ) )
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_SHARED_CREDENTIALS_FILE_ENV_KEY, path );
         else
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_SHARED_CREDENTIALS_FILE_ENV_KEY, null );
      }

      public static void SetRegion( string region )
      {
         if ( !CommonShared.StringUtils.IfNull( region ) )
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_DEFAULT_REGION_ENV_KEY, region );
         else
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_DEFAULT_REGION_ENV_KEY, null );
      }

      public static void SetKey( string key )
      {
         if ( !CommonShared.StringUtils.IfNull( key ) )
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_ACCESS_KEY_ID_ENV_KEY, key );
         else
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_ACCESS_KEY_ID_ENV_KEY, null );
      }

      private static string GetMasked( string envVariable )
      {
         var str = CommonShared.Utilities.GetValueOfEnvVar( envVariable );
         if ( str.Length > 0 )
            str = CommonShared.StringUtils.MaskString( str );
         return str;
      }

      public static string GetMaskedAccessKey()
      {
         return GetMasked( Common.AWS_ACCESS_KEY_ID_ENV_KEY );
      }

      public static string GetMaskedSecretKey()
      {
         return GetMasked( Common.AWS_SECRET_ACCESS_KEY_ENV_KEY );
      }

      public static string GetDefaultRegion()
      {
         return CommonShared.Utilities.GetValueOfEnvVar( Common.AWS_DEFAULT_REGION_ENV_KEY );
      }

      public static string GetDataDirectory()
      {
         return CommonShared.Utilities.DataDirectory;
      }
      
      public static void SetSecretKey( string key )
      {
         if ( !CommonShared.StringUtils.IfNull( key ) )
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_SECRET_ACCESS_KEY_ENV_KEY, key );
         else
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_SECRET_ACCESS_KEY_ENV_KEY, null );
      }

      public static void SetToken( string key )
      {
         if ( !CommonShared.StringUtils.IfNull( key ) )
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_SESSION_TOKEN_ENV_KEY, key );
         else
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_SESSION_TOKEN_ENV_KEY, null );
      }

      public static void SetProfile( string key )
      {
         if ( !CommonShared.StringUtils.IfNull( key ) )
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_PROFILE_ENV_KEY, key );
         else
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_PROFILE_ENV_KEY, null );
      }

      public static void SetConfigPath( string key )
      {
         if ( !CommonShared.StringUtils.IfNull( key ) )
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_CONFIG_FILE_ENV_KEY, key );
         else
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_CONFIG_FILE_ENV_KEY, null );
      }
      public static void SetDataPath( string dataPath )
      {
         CommonShared.Utilities.DataDirectory = dataPath;
      }

      #endregion CRED AND OTHERS

      #region SSM

      public static string DescribeInstanceInfo()
      {
         return SSM.DescribeInstanceInfo();
      }

      public static string DescribeAutomationExecution( string executionIds = null, string documentNamePrefix = null )
      {
         return SSM.DescribeAutomationExecution( executionIds, documentNamePrefix );
      }
      
      public static bool TryCreateAssociation( out string message, string associationName, string targetKey, List<string> targetValues, string documentName, Dictionary<string, List<string>> parameters = null )
      {
         return SSM.TryCreateAssociation( out message, associationName, targetKey, targetValues, documentName, parameters );
      }


      public static bool TryCreateCommandDocument( out string message, string name )
      {
         return SSM.TryCreateCommandDocument( out message, name );
      }

      public static bool TryRunCommands(
         out string message,
         out string commandId,
         out string output,
         string documentName,
         List<string> instanceIDs,
         Dictionary<string, List<string>> parameters,
         string comment = null,
         string outputS3BucketName = null,
         string outputS3KeyPrefix = null,
         string outputS3Region = null,
         bool commandOutput = false )
      {
         return SSM.TryRunCommands( out message, out commandId, out output, documentName, instanceIDs, parameters, comment, outputS3BucketName, outputS3KeyPrefix, outputS3Region, commandOutput );
      }

      public static bool TryStartAutomationExecution( out string message, out string executionId, string documentName, Dictionary<string, List<string>> parameters = null, string targetKey = null, List<string> targetValues = null )
      {
         return SSM.TryStartAutomationExecution( out message, out executionId, documentName, parameters, targetKey, targetValues );
      }

      public static bool TryGetCommandOutputs( out string message, out string output, string commandId, string instanceId = null, bool reursive = false )
      {
         return SSM.TryGetCommandOutputs( out message, out output, commandId, instanceId, reursive );
      }
      #endregion

      #region IAM
      public static string DescribeIAMPoliies( string policyName = null )
      {
         return IAM.DescribePolicies( policyName );
      }
      public static string DescribeIAMRoles( string rolename = null )
      {
         return IAM.DescribeRoles( rolename );
      }
      public static string DescribeInsProfile( string profileName = null )
      {
         return IAM.DescribeInsProfile( profileName );
      }
      public static bool TryCreateIAMRole( out string message, string roleName, string assumePolicy, string description = null )
      {
         return IAM.TryCreateRole( out message, roleName, assumePolicy, description );
      }

      public static bool TryDeleteRole( out string message, string roleName )
      {
         return IAM.TryDeleteRole( out message, roleName );
      }
      
      public static bool TryAddPoliciesToRole( out string message, string roleName, List<string> policyNames )
      {
         return IAM.TryAddPoliciesToRole( out message, roleName, policyNames );
      }

      public static bool TryCreateInstanceProfile( out string message, string profileName, string roleNameToAdd = null )
      {
         return IAM.TryCreateInstanceProfile( out message, profileName, roleNameToAdd );
      }

      public static bool TryDeleteInstanceProfile( out string message, string profileName )
      {
         return IAM.TryDeleteInstanceProfile( out message, profileName );
      }
      public static bool TryAddRolesToInstanceProfile( out string message, string profileName, List<string> roleNames )
      {
         return IAM.TryAddRolesToInstanceProfile( out message, profileName, roleNames );
      }

      #endregion

      #region VPC
      //public static string DescribeVpcs( string vpcid = null )
      //{
      //   return EC2.DescribeVpcs( vpcid );
      //}

      public static bool TryCreateVPC( out string message, out string vpcid, string ipv4CIDR, string enableDNSSupportFlag = null, string enableDNSHostNameFlag = null, string name = null, string instanceTenancy = null )
      {
         return EC2.TryCreateVPC( out message, out vpcid, ipv4CIDR, enableDNSSupportFlag, enableDNSHostNameFlag, name, instanceTenancy );
      }

      public static bool TryDeleteVPC( out string message, string vpcid, bool delDependents = false )
      {
         return EC2.TryDeleleVPC( vpcid, out message, delDependents );
      }
      #endregion

      #region SUBNET
      //public static string DescribeSubnets( string subnetid = null, string vpcid = null )
      //{
      //   return EC2.DescribeSubnets( subnetid, vpcid );
      //}

      public static bool TryCreateSubnet( out string message, out string subnetid, string cidrBlock, string vpcid, string name = null )
      {
         return EC2.TryCreateSubnet( out message, out subnetid, cidrBlock, vpcid, name );
      }

      public static bool TryDeleteSubnet( out string message, string subnetid )
      {
         return EC2.TryDeleleSubnet( subnetid, out message );
      }
      #endregion

      #region SG
      //public static string DescribeSecurityGroups( string vpcid = null, string groupid = null, string groupname = null )
      //{
      //   return EC2.DescribeSecurityGroups( vpcid, groupid, groupname );
      //}

      public static bool TryCreateSG( out string message, out string groupid, string groupname, string description, string vpcid = null )
      {
         return EC2.TryCreateSG( out message, out groupid, groupname, description, vpcid );
      }

      public static bool TryDeleteSG( out string message, string groupname = null, string groupid = null )
      {
         return EC2.TryDeleleSG( out message, groupname, groupid );
      }
      #endregion

      #region IG

      //public static string DescribeIG( string internetgatewayids = null, string vpcids = null )
      //{
      //   return EC2.DescribeIG( internetgatewayids, vpcids );
      //}

      public static bool TryCreateAndAtachInternetGateway( out string message, out string internetgatewayid, string vpcid )
      {
         return EC2.TryCreateAndAtachInternetGateway( out message, out internetgatewayid, vpcid );
      }
      
      public static bool TryDeleteInternetGateway( out string message, string internetgatewayid )
      {
         return EC2.TryDeleteInternetGateway( out message, internetgatewayid );
      }
      
      #endregion

      #region IP
      public static bool TryCreateIPRangePermission( out string message, string groupId, bool egress = false, string cidrIp = null, string ipProtocol = null, int fromPort = 0, int toPort = 0 )
      {
         return EC2.TryCreateIPRangePermission( out message, groupId, egress, cidrIp, ipProtocol, fromPort, toPort );
      }

      #endregion

      #region EC2 Instance
      public static bool CheckInatance( string instanceId, out string error )
      {
         return EC2.TryCheckIfInstanceExists( instanceId, out error );
      }

      public static bool TrySetCurrentEC2InatanceID( string instanceId, out string error )
      {
         var success = EC2.TryCheckIfInstanceExists(instanceId, out error );
         if ( success )
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_CURRENT_EC2_INSTANCE_ID, instanceId );
         else
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_CURRENT_EC2_INSTANCE_ID, null );
         return success;
      }

      public static string Describe( string type, string filters = null )
      {
         return EC2.Describe( type, filters );
      }

      //public static string DescribeReservations()
      //{
      //   return EC2.DescribeReservations();
      //}

      //public static string DescribeInstances( string instanceID )
      //{
      //   if ( string.IsNullOrEmpty( instanceID ) )
      //      return EC2.DescribeInstances();
      //   else
      //      return EC2.DescribeInstance( instanceID );
      //}

      public static string ListInstanceIDs()
      {
         return EC2.DescribeInstanceIDs();
      }

      public static void TerminateInstances( out string message, string instanceIds )
      {
         EC2.TerminateInstances( out message, instanceIds );
      }
      public static bool TryAssociateIamInstanceProfile( out string message, string instanceId, string profileName )
      {
         return EC2.TryAssociateIamInstanceProfile( out message, instanceId, profileName );
      }

      public static bool TryLaunchInatances(
         out string message,
         out string instances,
         string amiID,
         string keyName,
         List<string> sg,
         string iamRole = null,
         string subnetid = null,
         string instanceType = null,
         int deviceIndex = 0,
         bool associatepublicip = false,
         int minCount = 1,
         int maxCount = 1,
         string userData = null,
         string jsonTagsString = null )
      {
         List<Tag> tags = null;
         if ( !string.IsNullOrEmpty( jsonTagsString ) )
            tags = BuildTags( jsonTagsString );
         return EC2.TryLaunchInatances( out message, out instances, amiID, keyName, sg, iamRole, subnetid, instanceType, deviceIndex, associatepublicip, minCount, maxCount, userData, tags );
      }

      #endregion

      #region key pair
      //public static string DescribeKeyPairs( string keyname )
      //{
      //   return EC2.DescribeKeyPairs( keyname );
      //}

      public static bool TryCreateKeyPair( out string message, string keyName, string saveKeyToFilePath )
      {
         return EC2.TryCreateKeyPair( out message, keyName, saveKeyToFilePath );
      }

      public static bool TryCreateRouteTableForVPC( out string message, out string routeTableid, string vpcid )
      {
         return EC2.TryCreateRouteTableForVPC( out message, out routeTableid, vpcid );
      }

      public static bool TryCreateRoute( out string message, string routeTableid, string gaterwayid, string targetCidrBlock )
      {
         return EC2.TryCreateRoute( out message, routeTableid, gaterwayid, targetCidrBlock );
      }
      
      public static bool TryDeleteKeyPair( out string message, string keyName )
      {
         return EC2.TryDeleteKeyPair( keyName, out message );
      }
      public static bool TryDeleteRouteTable( out string message, string routeTableId )
      {
         return EC2.TryDeleteRouteTable( out message, routeTableId );
      }

      public static bool TryDeleteRoute( out string message, string routeTableId, string targetCidrBlock )
      {
         return EC2.TryDeleteRoute( out message, routeTableId, targetCidrBlock );
      }

      #endregion

      #region Win Password
      public static bool TryGetWinPassword( out string message, out string password, string instanceId, string privatekeyData )
      {
         return EC2.TryGetWinPassword( out message, out password, instanceId, privatekeyData );
      }

      public static bool TryGetWinPasswordUsingKeyFile( out string message, out string password, string instanceId, string keyFilePath )
      {
         var success = false;
         password = string.Empty;
         password = string.Empty;
         try
         {
            success = EC2.TryGetWinPassword( out message, out password, instanceId, File.ReadAllText( keyFilePath ) );
         }
         catch ( Exception ex )
         {
            message = Common.ParseException( ex );
         }
         return success;
      }
      #endregion Win Password

      #region Image
      //public static string DescribeImages( string imageid = null )
      //{
      //   return EC2.DescribeImages( imageid );
      //}

      #endregion

      #region TAGS
      public static bool TryGetTagValue( string name, out string value, out string error )
      {
         return EC2.TryEC2TagValueByTagName( name, out value, out error );
      }

      public static bool TrySetTag( string resources, string name, string value, out string error )
      {
         return EC2.TrySetEC2TagValueByTagName( resources, name, value, out error );
      }

      public static bool TrySetTags( string resources, string tagAsJsonString, out string error )
      {
         return EC2.TrySetTags( resources, BuildTags( tagAsJsonString ), out error );
      }

      public static List<Tag> BuildTags( string tagsJsonString )
      {
         return JsonSerializer.Deserialize<List<Tag>>( tagsJsonString );
      }

      public static Tag CreateTag( string key, string value )
      {
         return new Tag { Key = key, Value = value };
      }

      public static string CreateFilterString( string name, string value )
      {
         return $" Name={ name }, Values={value.Replace( '|', ',' )}";
      }

      public static void AddToFilterString( ref string filters, string name, string value )
      {
         filters = $"{filters}{CreateFilterString( name, value )}";
      }

      //public static string DescribeTags( string resources = null, string key = null )
      //{
      //   return EC2.DescribeTags( resources, key );
      //}
      #endregion

      #region LAST

      public static string LastCreatedVpcID
      {
         get { return  CommonShared.CacheManager.Get(  AWSCacheManager.Constants.LAST_CREATED_VPC_ID_KEY ); }
      }


      public static string LastCreatedIAMRole
      {
         get { return  CommonShared.CacheManager.Get(  AWSCacheManager.Constants.LAST_CREATED_ACCESSED_ROLE_NAME_KEY ); }
      }

      public static string LastCreatedInstanceProfile
      {
         get { return  CommonShared.CacheManager.Get(  AWSCacheManager.Constants.LAST_CREATED_ACCESSED_PROFILE_KEY ); }
      }

      public static string LastCreatedSecurityGroupID
      {
         get { return  CommonShared.CacheManager.Get(  AWSCacheManager.Constants.LAST_CREATED_SG_ID_KEY ); }
      }

      public static string LastCreatedSubnetID
      {
         get { return  CommonShared.CacheManager.Get(  AWSCacheManager.Constants.LAST_CREATED_SN_ID_KEY ); }
      }

      public static string LastCreatedRouteTableID
      {
         get { return  CommonShared.CacheManager.Get(  AWSCacheManager.Constants.LAST_CREATED_RT_ID_KEY ); }
      }
      
      public static string LastCreatedGatewayID
      {
         get { return  CommonShared.CacheManager.Get(  AWSCacheManager.Constants.LAST_CREATED_IG_ID_KEY ); }
      }
      
      public static string LastCreatedKeyPairName
      {
         get { return  CommonShared.CacheManager.Get(  AWSCacheManager.Constants.LAST_CREATED_KP_NAME_KEY ); }
      }

      public static string LastCreatedKeyPairID
      {
         get { return  CommonShared.CacheManager.Get(  AWSCacheManager.Constants.LAST_CREATED_KP_ID_KEY ); }
      }

      public static string LastLaunchedEC2Instance
      {
         get { return  CommonShared.CacheManager.Get(  AWSCacheManager.Constants.LAST_LAUNCHED_INATANCES_KEY ); }
      }

      #endregion
   }
}
