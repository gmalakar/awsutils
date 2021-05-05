#region history
//*****************************************************************************
// ec2.cs:
//
// History:
// 08/06/20 - Goutam Malakar 
//*****************************************************************************
#endregion history

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.Json;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.IdentityManagement.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace AWSCM.AWSInterface
{

   internal class EC2
   {
      private EC2() { }

      #region internal static

      #region internal Describe static
      internal static string Describe( string type, string filters = null )
      {
         var desc = string.Empty;
         switch ( type.ToLower() )
         {
            case "instance":
            case "ins":
               desc = Common.ConvertToJSON( EC2Instance.DescribeLocal<Instance>( filters ) );
               break;
            case "reservation":
               desc = Common.ConvertToJSON( EC2Instance.DescribeLocal<Reservation>( filters ) );
               break;
            case "sg":
            case "securitygroup":
               desc = Common.ConvertToJSON( EC2Instance.DescribeLocal<SecurityGroup>( filters ) );
               break;
            case "sn":
            case "subnet":
               desc = Common.ConvertToJSON( EC2Instance.DescribeLocal<Subnet>( filters ) );
               break;
            case "ig":
            case "internetgateway":
               desc = Common.ConvertToJSON( EC2Instance.DescribeLocal<InternetGateway>( filters ) );
               break;
            case "kp":
            case "keypair":
               desc = Common.ConvertToJSON( EC2Instance.DescribeLocal<KeyPair>( filters ) );
               break;
            case "image":
               desc = Common.ConvertToJSON( EC2Instance.DescribeLocal<Image>( filters ) );
               break;
            case "tag":
            case "tagdesc":
            case "tagdescription":
               desc = Common.ConvertToJSON( EC2Instance.DescribeLocal<TagDescription>( filters ) );
               break;
            case "vpc":
               desc = Common.ConvertToJSON( EC2Instance.DescribeLocal<Vpc>( filters ) );
               break;
            case "iaminstanceprofileassociation":
               desc = Common.ConvertToJSON( EC2Instance.DescribeLocal<IamInstanceProfileAssociation>( filters ) );
               break;
            case "routetable":
               desc = Common.ConvertToJSON( EC2Instance.DescribeLocal<RouteTable>( filters ) );
               break;
            default:
               desc = $"No description is available for this invalid type: [{ type }]";
               break;
         }
         return desc;
      }

      internal static string DescribeInstanceIDs()
      {
         return Common.ConvertToJSON( EC2Instance.DescribeLocal<Instance>().Select( i => i.InstanceId ).ToList() );
      }

      #endregion internal Describe static

      #region internal TryCreate/TrySet static
      internal static bool TryEC2TagValueByTagName( string tagName, out string tagValue, out string error )
      {
         error = string.Empty;
         tagValue = string.Empty;
         var success = false;
         try
         {
            tagValue = EC2Instance.TagValueByName( tagName );
            success = true;
         }
         catch ( Exception e )
         {
            success = false;
            error = Common.ParseException( e );
         }
         return success;
      }

      internal static bool TrySetEC2TagValueByTagName( string resouces, string tagName, string tagValue, out string error )
      {
         error = string.Empty;
         var success = false;
         try
         {
            success = EC2Instance.TrySetTagsLocal( out error, resouces, new List<Amazon.EC2.Model.Tag> { new Amazon.EC2.Model.Tag { Key = tagName, Value = tagValue } } );
         }
         catch ( Exception e )
         {
            success = false;
            error = Common.ParseException( e );
         }
         return success;
      }
      internal static bool TrySetTags( string resources, List<Amazon.EC2.Model.Tag> tags, out string error )
      {
         error = string.Empty;
         var success = false;
         try
         {
            EC2Instance.TrySetTagsLocal( out error, resources, tags );
            success = true;
         }
         catch ( Exception e )
         {
            success = false;
            error = Common.ParseException( e );
         }
         return success;
      }

      internal static bool TryCreateVPC( out string message, out string vpcid, string ipv4CIDR, string enableDNSSupportFlag = null, string enableDNSHostNameFlag = null, string name = null, string instanceTenancy = null )
      {
         return EC2Instance.TryCreateVpc( out message, out vpcid, ipv4CIDR, enableDNSSupportFlag, enableDNSHostNameFlag, name, instanceTenancy );
      }

      internal static bool TryCreateSubnet( out string message, out string subnetid, string cidrBlock, string vpcid, string name = null )
      {
         return EC2Instance.TryCreateSN( out message, out subnetid, cidrBlock, vpcid, name );
      }

      internal static bool TryCreateSG( out string message, out string groupid, string groupname, string description, string vpcid = null )
      {
         return EC2Instance.TryCreateSG2( out message, out groupid, groupname, description, vpcid );
      }

      internal static bool TryCreateAndAtachInternetGateway( out string message, out string internetgatewayid, string vpcid )
      {
         return EC2Instance.TryCreateAndAtachIG( out message, out internetgatewayid, vpcid );
      }
      internal static bool TryCreateIPRangePermission( out string message, string groupId, bool egress = false, string cidrIp = null, string ipProtocol = null, int fromPort = 0, int toPort = 0 )
      {
         return EC2Instance.TryCreateIPRP( out message, groupId, egress, cidrIp, ipProtocol, fromPort, toPort );
      }

      internal static bool TryCreateKeyPair( out string message, string keyName, string saveKeyToFilePath )
      {
         return EC2Instance.TryCreateKP( out message, keyName, saveKeyToFilePath );
      }

      internal static bool TryCreateRouteTableForVPC( out string message, out string routeTableid, string vpcid )
      {
         return EC2Instance.TryCreateRTForVPC( out message, out routeTableid, vpcid );
      }

      internal static bool TryCreateRoute( out string message, string routeTableid, string gaterwayid, string targetCidrBlock )
      {
         return EC2Instance.TryCreateR( out message, routeTableid, gaterwayid, targetCidrBlock );
      }


      #endregion internal TryCreate static

      #region internal TryDelete static
      internal static bool TryDeleleVPC( string vpcid, out string message, bool delDependents = false )
      {
         return EC2Instance.TryDelVpc( vpcid, out message, delDependents );
      }

      internal static bool TryDeleleSubnet( string subnetid, out string message )
      {
         return EC2Instance.TryDleteSN( subnetid, out message );
      }

      internal static bool TryDeleleSG( out string message, string groupname = null, string groupid = null )
      {
         return EC2Instance.TryDelSG2( out message, groupname, groupid );
      }

      internal static bool TryDeleteInternetGateway( out string message, string internetgatewayid )
      {
         return EC2Instance.TryDeleteInternetGateway2( out message, internetgatewayid );
      }

      internal static bool TryDeleteKeyPair( string keyName, out string message )
      {
         return EC2Instance.TryDelteKP( keyName, out message );
      }

      internal static bool TryDeleteRouteTable( out string message, string routeTableId )
      {
         return EC2Instance.TryDeleteRT( out message, routeTableId );
      }

      internal static bool TryDeleteRoute( out string message, string routeTableId, string targetCidrBlock )
      {
         return EC2Instance.TryDeleteR( out message, routeTableId, targetCidrBlock );
      }


      #endregion internal TryDelete static

      internal static void TerminateInstances( out string message, string instanceIds )
      {
         EC2Instance.TerminateEC2( out message, instanceIds );
      }

      internal static bool TryAssociateIamInstanceProfile( out string message, string instanceId, string profileName )
      {
         return EC2Instance.TryAssociateIamInstanceProfile2( out message, instanceId, profileName );
      }

      #region internal instance static
      internal static bool TryLaunchInatances(
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
         List<Amazon.EC2.Model.Tag> tags = null )
      {
         return EC2Instance.TryLaunchEC2Inatances( out message, out instances, amiID, keyName, sg, iamRole, subnetid, instanceType, deviceIndex, associatepublicip, minCount, maxCount, userData, tags );
      }

      #endregion internal inatance static

      #region internal others static
      internal static bool TryGetWinPassword( out string message, out string password, string instanceId, string privatekeyData )
      {
         return EC2Instance.TryGetPWD( out message, out password, instanceId, privatekeyData );
      }

      #endregion internal others static

      #endregion
      private bool TrySetCurrentEC2InatanceLocal( string instanceId, out string error )
      {
         var success = false;
         this.currentEC2Instance = null;
         success = this.TryGetInstance( instanceId, out Instance instance, out error );
         if ( success )
         {
            this.currentEC2Instance = instance;
            CommonShared.Utilities.SetValueOfEnvVar( Common.AWS_CURRENT_EC2_INSTANCE_ID, instanceId );
         }
         return success;
      }

      public static bool TryCheckIfInstanceExists( string instanceId, out string error )
      {
         return EC2Instance.TryGetInstance( instanceId, out _, out error );
      }

      private string TagValueByName( string name )
      {
         var tag = this.CurrentEC2Instance.Tags.FirstOrDefault( t => t.Key.Equals(name, StringComparison.OrdinalIgnoreCase) );
         return tag == null ? string.Empty : tag.Value;
      }

      #region VPC

      private bool TryGetPWD( out string message, out string password, string instanceId, string privatekeydata )
      {
         var success = false;
         message = string.Empty;
         password = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               success = this.TryGetWindowsPassword( out message, out password, ec2Client, instanceId, privatekeydata );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to get windows password Error:{ Common.ParseException( e ) }";
         }
         return success;
      }

      private bool TryLaunchEC2Inatances(
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
         List<Amazon.EC2.Model.Tag> tags = null )
      {
         var launched = false;
         var launchedInstances = new List<Instance>();
         instances = string.Empty;

         using ( var ec2Client = this.EC2Client() )
         {
            launchedInstances = this.LaunchInatances( out message, out launched, ec2Client, amiID, keyName, sg, iamRole, subnetid, instanceType, deviceIndex, associatepublicip, minCount, maxCount, userData, tags );
            if ( launched )
               instances = Common.ConvertToJSON( launchedInstances );

         }
         return launched;
      }

      private void TerminateEC2( out string message, string instanceIds )
      {
         using ( var ec2Client = this.EC2Client() )
         {
            this.Terminate( out message, ec2Client, instanceIds );
         }
      }

      private bool TryAssociateIamInstanceProfile2( out string message, string instanceId, string profileName )
      {
         var associated = false;
         using ( var ec2Client = this.EC2Client() )
         {
            this.AssociateIamInstanceProfile( out message, out associated, ec2Client, instanceId, profileName );
         }
         return associated;
      }

      #endregion

      private bool TrySetTagsLocal( out string message, string resources, List<Amazon.EC2.Model.Tag> tags )
      {
         var tagged = false;
         message = string.Empty;
         if ( !string.IsNullOrEmpty( resources ) )
            //create tag
            using ( var ec2Client = this.EC2Client() )
            {
               tagged = this.TrySetTagsLocal( out message, ec2Client, resources, tags );
            }
         else
            message = "The request must contain the parameter resources";
         return tagged;
      }

      //special characters in tags
      //+ - = . _ : / @
      private bool TrySetTagsLocal( out string message, AmazonEC2Client ec2Client, string instanceID, List<Amazon.EC2.Model.Tag> tags )
      {
         var tagged = false;
         message = string.Empty;
         try
         {
            if ( tags != null && tags.Count > 0 )
            {  //create tag
               var response = ec2Client.CreateTagsAsync( new CreateTagsRequest()
               {
                  Resources = instanceID.Split( Common.ResourceDelimeters, StringSplitOptions.RemoveEmptyEntries ).ToList(),
                  Tags = tags
               } ).Result;
               tagged = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            else
               message = "Cannot set null tags.";
         }
         catch ( Exception e )
         {
            message = $"Unable to create Tags.. Error:{ Common.ParseException( e ) }";
         }
         return tagged;
      }

      private AmazonEC2Client EC2Client()
      {
         if ( Common.TryGetCredentials( out AWSCredentials awsCredentials ) )
            return new AmazonEC2Client( awsCredentials, Common.DefaultRegionEndpoint );
         else
            return new AmazonEC2Client( new AmazonEC2Config() { RegionEndpoint = Common.DefaultRegionEndpoint, DisableLogging = true } );
      }

      #region Describe

      private List<T> DescribeLocal<T>( string filters = null )
      {
         List<T> desc;
         using ( var ec2Client = this.EC2Client() )
         {
            desc = this.DescribeGeneric<T>( ec2Client, filters );
         }
         if ( desc == null )
            desc = new List<T>();
         return desc;
      }

      private List<SecurityGroup> SecurityGroups( AmazonEC2Client ec2Client, string vpcid = null, string groupid = null, string groupname = null )
      {
         var filters = string.Empty;
         if ( !string.IsNullOrEmpty( vpcid ) )
         {
            Utilities.AddToFilterString( ref filters, "vpc-id", vpcid );
         }

         if ( !string.IsNullOrEmpty( groupid ) )
         {
            Utilities.AddToFilterString( ref filters, "group-id", groupid );
         }

         if ( !string.IsNullOrEmpty( groupname ) )
         {
            Utilities.AddToFilterString( ref filters, "group-name", groupname );
         }

         return DescribeGeneric<SecurityGroup>( ec2Client, filters );
      }

      private List<KeyPairInfo> KeyPairs( AmazonEC2Client ec2Client, string keyname )
      {
         var filters = string.Empty;
         if ( !string.IsNullOrEmpty( keyname ) )
         {
            Utilities.AddToFilterString( ref filters, "key-name", keyname );
         }
         return DescribeGeneric<KeyPairInfo>( ec2Client, filters );
      }

      private List<Vpc> Vpcs( AmazonEC2Client ec2Client, string vpcid = null )
      {
         var filters = string.Empty;
         if ( !string.IsNullOrEmpty( vpcid ) )
         {
            Utilities.AddToFilterString( ref filters, "vpc-id", vpcid );
         }
         return DescribeGeneric<Vpc>( ec2Client, filters );
      }

      private List<RouteTable> RouteTables( AmazonEC2Client ec2Client, string routeTableId = null )
      {
         var filters = string.Empty;
         if ( !string.IsNullOrEmpty( routeTableId ) )
         {
            Utilities.AddToFilterString( ref filters, "route-table-id", routeTableId );
         }
         return DescribeGeneric<RouteTable>( ec2Client, filters );
      }

      private List<Subnet> Subnets( AmazonEC2Client ec2Client, string subnetid = null, string vpcid = null )
      {
         var filters = string.Empty;
         if ( !string.IsNullOrEmpty( vpcid ) )
         {
            Utilities.AddToFilterString( ref filters, "vpc-id", vpcid );
         }

         if ( !string.IsNullOrEmpty( subnetid ) )
         {
            Utilities.AddToFilterString( ref filters, "subnet-id", subnetid );
         }

         return DescribeGeneric<Subnet>( ec2Client, filters );
      }

      private List<Image> Images( AmazonEC2Client ec2Client, string imageid = null )
      {
         var filters = string.Empty;
         if ( !string.IsNullOrEmpty( imageid ) )
         {
            Utilities.AddToFilterString( ref filters, "image-id", imageid );
         }

         return DescribeGeneric<Image>( ec2Client, filters );
      }

      #endregion Describe

      #region Create

      #region TryCreate private
      private bool TryCreateVpc( out string message, out string vpcid, string ipv4CIDR, string enableDNSSupportFlag = null, string enableDNSHostNameFlag = null, string name = null, string instanceTenancy = null )
      {
         var created = false;
         message = string.Empty;
         vpcid = string.Empty;
         try
         {
            if ( string.IsNullOrEmpty( instanceTenancy ) )
               instanceTenancy = @"Default";

            using ( var ec2Client = this.EC2Client() )
            {
               var vpc = this.CreateVpc( out message, out created, ec2Client, ipv4CIDR, enableDNSSupportFlag, enableDNSHostNameFlag, name, instanceTenancy );
               if ( created )
                  vpcid = vpc.VpcId;
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to create VPC. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }

      private bool TryCreateSN( out string message, out string subnetid, string cidrBlock, string vpcid, string name = null )
      {
         var created = false;
         message = string.Empty;
         subnetid = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               var sn = this.CreateSubnet( out message, out created, ec2Client, cidrBlock, vpcid, name );
               if ( created )
                  subnetid = sn.SubnetId;
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to create subnet. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }

      private bool TryCreateIPRP( out string message, string groupId, bool egress = false, string cidrIp = null, string ipProtocol = null, int fromPort = 0, int toPort = 0 )
      {
         var created = false;
         message = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               this.CreateIPRangePermission( out message, out created, ec2Client, groupId, egress, cidrIp, ipProtocol, fromPort, toPort );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to create an IP Range permission. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }

      private bool TryCreateKP( out string message, string keyName, string saveKeyToFilePath )
      {
         var created = false;
         message = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               var response = ec2Client.CreateKeyPairAsync( new CreateKeyPairRequest( keyName ) ).Result;
               // Save the private key in a .pem file
               File.WriteAllTextAsync( saveKeyToFilePath, response.KeyPair.KeyMaterial );
               created = true;
               //save to cache
               CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_KP_ID_KEY, response.KeyPair.KeyPairId );
               CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_KP_NAME_KEY, response.KeyPair.KeyName );
               AWSCacheManager.SaveKeyMaterialToCache( response.KeyPair.KeyName, response.KeyPair.KeyMaterial );
            }
         }
         catch ( AmazonEC2Exception e )
         {
            // Check the ErrorCode to see if the key already exists.
            if ( "InvalidKeyPair.Duplicate" == e.ErrorCode )
            {
               message = $"The key pair [{keyName}] already exists.";
            }
            else
            {
               message = $"Unable to create key pair. Error:{ Common.ParseException( e ) }";
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to create key pair. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }

      private bool TryCreateSG2( out string message, out string groupid, string groupname, string description, string vpcid = null )
      {
         var created = false;
         message = string.Empty;
         groupid = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               var sg = this.CreateSecurityGroup( out message, out created, ec2Client, groupname, description, vpcid );
               if ( created )
                  groupid = sg.GroupId;
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to create Security Group. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }

      private bool TryCreateAndAtachIG( out string message, out string internetgatewayid, string vpcid )
      {
         var attached = false;
         message = string.Empty;
         internetgatewayid = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               var ig = this.CreateAndAttachIG( out message, out attached, ec2Client, vpcid );
               if ( attached )
                  internetgatewayid = ig.InternetGatewayId;
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to create and attach Internet Gateway Id Error:{ Common.ParseException( e ) }";
         }
         return attached;
      }

      private bool TryCreateRTForVPC( out string message, out string routeTableid, string vpcid )
      {
         var attached = false;
         message = string.Empty;
         routeTableid = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               this.CreateRouteTableForVPC( out message, out attached, ec2Client, vpcid );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to create and attach Route Table Id Error:{ Common.ParseException( e ) }";
         }
         return attached;
      }

      private bool TryCreateR( out string message, string routeTableid, string gaterwayid, string targetCidrBlock )
      {
         var created = false;
         message = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               this.CreateRoute( out created, ec2Client, routeTableid, gaterwayid, targetCidrBlock );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to create route. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }

      #endregion TryCreate private

      #region Create Private
      private SecurityGroup CreateSecurityGroup( out string message, out bool created, AmazonEC2Client ec2Client, string groupname, string description, string vpcid = null )
      {
         message = string.Empty;
         created = false;
         SecurityGroup sg = this.SecurityGroups( ec2Client, null, null, groupname ).FirstOrDefault();
         if ( sg != null )
         {
            message = $"SecurityGroup [{groupname}] already exists.";
            //save to cache
            CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_SG_ID_KEY, sg.GroupId );
         }
         else
         {
            var create = true;
            var createRequest = new CreateSecurityGroupRequest();
            createRequest.GroupName = groupname;
            createRequest.Description = description;
            if ( !string.IsNullOrEmpty( vpcid ) )
            {
               //find the vpc
               var vpc = this.Vpcs( ec2Client, vpcid ).FirstOrDefault();
               if ( vpc == null )
               {
                  message = $"Vpc [{vpcid}] does not exist.";
                  create = false;
               }
               else
                  createRequest.VpcId = vpcid;
            }
            if ( create )
            {
               var createResponse = ec2Client.CreateSecurityGroupAsync(createRequest).Result;
               sg = this.SecurityGroups( ec2Client, null, createResponse.GroupId ).FirstOrDefault();
               created = sg != null;
               if ( created )
                  //save to cache
                  CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_SG_ID_KEY, sg.GroupId );
            }
         }

         return sg;
      }

      private Vpc CreateVpc( out string message, out bool created, AmazonEC2Client ec2Client, string ipv4CIDR, string enableDNSSupportFlag = null, string enableDNSHostNameFlag = null, string name = null, string instanceTenancy = null )
      {
         message = string.Empty;
         created = false;

         var vpcRequest = new CreateVpcRequest();
         vpcRequest.CidrBlock = ipv4CIDR;

         if ( !string.IsNullOrEmpty( instanceTenancy ) )
            vpcRequest.InstanceTenancy = instanceTenancy;

         var vpc = ec2Client.CreateVpcAsync( vpcRequest ).Result.Vpc;
         created = vpc != null;
         if ( created )
         {
            if ( !string.IsNullOrEmpty( name ) )
               this.TrySetTagsLocal( out message, vpc.VpcId, new List<Amazon.EC2.Model.Tag> { Utilities.CreateTag( @"Name", name ) } );
            if ( !string.IsNullOrEmpty( enableDNSSupportFlag ) )
            {
               var modRequest = new ModifyVpcAttributeRequest() { VpcId = vpc.VpcId };
               modRequest.EnableDnsSupport = enableDNSSupportFlag == "1";
               var modResp = ec2Client.ModifyVpcAttributeAsync( modRequest ).Result;
               created = modResp.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            if ( !string.IsNullOrEmpty( enableDNSHostNameFlag ) )
            {
               var modRequest2 = new ModifyVpcAttributeRequest() { VpcId = vpc.VpcId };
               modRequest2.EnableDnsHostnames = enableDNSHostNameFlag == "1";
               var modResp = ec2Client.ModifyVpcAttributeAsync( modRequest2 ).Result;
               created = modResp.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }

            //save to cache
            CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_VPC_ID_KEY, vpc.VpcId );
         }
         return vpc;
      }

      private Subnet CreateSubnet( out string message, out bool created, AmazonEC2Client ec2Client, string cidrBlock, string vpcid, string name = null )
      {
         message = string.Empty;
         created = false;

         var snRequest = new CreateSubnetRequest();
         snRequest.CidrBlock = cidrBlock;
         snRequest.VpcId = vpcid;

         var sn = ec2Client.CreateSubnetAsync( snRequest ).Result.Subnet;
         created = sn != null;
         if ( created )
         {
            if ( !string.IsNullOrEmpty( name ) )
               this.TrySetTagsLocal( out message, sn.SubnetId, new List<Amazon.EC2.Model.Tag> { Utilities.CreateTag( @"Name", name ) } );
            //save to cache
            CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_SN_ID_KEY, sn.SubnetId );
         }
         return sn;
      }

      private InternetGateway CreateAndAttachIG( out string message, out bool attached, AmazonEC2Client ec2Client, string vpcid )
      {
         message = string.Empty;
         attached = false;
         InternetGateway ig = null;
         var vpc = this.Vpcs( ec2Client, vpcid ).FirstOrDefault();
         if ( vpc == null )
            message = $"Vpc [{vpcid}] does not exist.";
         else
         {
            ig = ec2Client.CreateInternetGatewayAsync( new CreateInternetGatewayRequest() ).Result.InternetGateway;
            attached = ig != null;
            if ( attached )
            {
               //save to cache
               CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_IG_ID_KEY, ig.InternetGatewayId );
               var attRequest = new AttachInternetGatewayRequest( ){ InternetGatewayId = ig.InternetGatewayId, VpcId = vpcid };
               var attresp = ec2Client.AttachInternetGatewayAsync( attRequest ).Result;
               attached = attresp.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
         }
         return ig;
      }

      private void CreateIPRangePermission( out string message, out bool created, AmazonEC2Client ec2Client, string groupId, bool egress = false, string cidrIp = null, string ipProtocol = null, int fromPort = 0, int toPort = 0 )
      {
         message = string.Empty;
         created = false;
         var sg = this.SecurityGroups( ec2Client, null, groupId ).FirstOrDefault();
         if ( sg == null )
         {
            if ( string.IsNullOrEmpty( groupId ) )
               groupId = @"no group id is passed";
            message = $"SecurityGroup [{groupId}] does not exist.";
         }
         else
         {
            if ( string.IsNullOrEmpty( cidrIp ) )
               cidrIp = @"0.0.0.0/0";

            if ( fromPort <= 0 )
               fromPort = 22;

            if ( toPort <= 0 )
               toPort = 22;

            if ( string.IsNullOrEmpty( ipProtocol ) )
               ipProtocol = @"tcp";

            groupId = sg.GroupId;
            var ipPermission = new IpPermission
            {
               IpProtocol = ipProtocol,
               FromPort = fromPort,
               ToPort = toPort,
               Ipv4Ranges = new List < IpRange > { new IpRange { CidrIp = cidrIp } }
            };

            if ( egress )
            {
               var egressRequest = new AuthorizeSecurityGroupEgressRequest
               {
                  GroupId = groupId
               };
               egressRequest.IpPermissions.Add( ipPermission );

               var response = ec2Client.AuthorizeSecurityGroupEgressAsync( egressRequest ).Result;
               created = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
               if ( !created )
                  message = response.ToString();
            }
            else
            {
               var ingressRequest = new AuthorizeSecurityGroupIngressRequest
               {
                  GroupId = groupId
               };
               ingressRequest.IpPermissions.Add( ipPermission );

               var response = ec2Client.AuthorizeSecurityGroupIngressAsync( ingressRequest ).Result;
               created = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
               if ( !created )
                  message = response.ToString();
            }
         }
      }

      private void CreateRoute( out bool created, AmazonEC2Client ec2Client, string routetableid, string internetgatewayid, string targetCidrBlock )
      {
         var response = ec2Client.CreateRouteAsync( new CreateRouteRequest(){ GatewayId = internetgatewayid, DestinationCidrBlock = targetCidrBlock, RouteTableId = routetableid } ).Result;
         created = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
      }

      private void CreateRouteTableForVPC( out string message, out bool created, AmazonEC2Client ec2Client, string vpcid )
      {
         message = string.Empty;
         created = false;
         RouteTable rt = null;
         var vpc = this.Vpcs( ec2Client, vpcid ).FirstOrDefault();
         if ( vpc == null )
            message = $"Vpc [{vpcid}] does not exist.";
         else
         {
            rt = ec2Client.CreateRouteTableAsync( new CreateRouteTableRequest() { VpcId = vpc.VpcId } ).Result.RouteTable;
            created = rt != null;
            if ( created )
            {
               //save to cache
               CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_RT_ID_KEY, rt.RouteTableId );
            }
         }
      }


      #endregion Create Private

      #region Associate

      private void AssociateToRoutetable( out string message, out bool associated, out string associationid, AmazonEC2Client ec2Client, string routeTableId, string subnetid, string internetgatewayid )
      {
         message = string.Empty;
         associated = false;
         associationid = string.Empty;
         var rt = this.RouteTables( ec2Client, routeTableId ).FirstOrDefault();
         if ( rt == null )
            message = $"Route Table [{routeTableId}] does not exist.";
         else
         {
            var request = new AssociateRouteTableRequest(){ RouteTableId = rt.RouteTableId };
            if ( string.IsNullOrEmpty( subnetid ) )
               request.SubnetId = subnetid;

            if ( string.IsNullOrEmpty( internetgatewayid ) )
               request.GatewayId = internetgatewayid;

            var response = ec2Client.AssociateRouteTableAsync( request ).Result;
            associated = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            if ( associated )
               associationid = response.AssociationId;
         }
      }


      private void DisassociateToRoutetable( out string message, out bool disassociated, AmazonEC2Client ec2Client, string routeTableId, string subnetid, string internetgatewayid )
      {
         message = string.Empty;
         disassociated = false;
         var rt = this.RouteTables( ec2Client, routeTableId ).FirstOrDefault();
         if ( rt == null )
            message = $"Route Table [{routeTableId}] does not exist.";
         else
         {
            RouteTableAssociation assoc = null;
            if ( string.IsNullOrEmpty( subnetid ) )
               assoc = rt.Associations.Where( a => a.SubnetId == subnetid ).FirstOrDefault();

            if ( string.IsNullOrEmpty( internetgatewayid ) )
               assoc = rt.Associations.Where( a => a.GatewayId == internetgatewayid ).FirstOrDefault();

            var request = new DisassociateRouteTableRequest(){ AssociationId = assoc.RouteTableAssociationId };
            var response = ec2Client.DisassociateRouteTableAsync( new DisassociateRouteTableRequest(){ AssociationId = assoc.RouteTableAssociationId } ).Result;
            disassociated = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
         }
      }

      private void AssociateIamInstanceProfile( out string message, out bool associated, AmazonEC2Client ec2Client, string instanceID, string instanceProfileName )
      {
         message = string.Empty;
         associated = false;
         try
         {
            var response = ec2Client.AssociateIamInstanceProfileAsync(
            new AssociateIamInstanceProfileRequest()
            {
               InstanceId = instanceID,
               IamInstanceProfile = new IamInstanceProfileSpecification() { Name = instanceProfileName }
            } ).Result;

            associated = response.IamInstanceProfileAssociation != null;
            if ( !associated )
               message = response.ResponseMetadata.ToString();
         }
         catch ( Exception e )
         {
            message = $"Unable to associate IamInstanceProfile  { instanceProfileName } to instance id { instanceID }. Error:{ Common.ParseException( e ) }";
         }
      }

      private void DisassociateIamInstanceProfile( AmazonEC2Client ec2Client, string instanceID )
      {
         var instanceProfile = EC2InatanceProfileAssociation( ec2Client, instanceID ).FirstOrDefault();
         if ( instanceProfile != null ) //disassociate
         {
            ec2Client.DisassociateIamInstanceProfileAsync(
            new DisassociateIamInstanceProfileRequest()
            {
               AssociationId = instanceProfile.AssociationId
            } );
         }
      }

      private void ReplaceIamInstanceProfile( AmazonEC2Client ec2Client, string instanceID, string instanceProfileName )
      {
         var instanceProfile = EC2InatanceProfileAssociation( ec2Client, instanceID ).FirstOrDefault();
         if ( instanceProfile != null ) //disassociate
         {
            ec2Client.ReplaceIamInstanceProfileAssociationAsync(
            new ReplaceIamInstanceProfileAssociationRequest()
            {
               AssociationId = instanceProfile.AssociationId,
               IamInstanceProfile = new IamInstanceProfileSpecification() { Name = instanceProfileName }
            } );
         }
      }

      #endregion Associate

      #endregion Create

      #region Delete

      #region TryDelete private
      private bool TryDelVpc( string vpcid, out string message, bool delDependents = false )
      {
         var deleted = false;
         message = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               this.DeleteVpc( out message, out deleted, ec2Client, vpcid, delDependents );
            }
         }
         catch ( Exception e )
         {
            CommonShared.StringUtils.AddString( ref message, $"Unable to delete VPC { vpcid}. Error:{ Common.ParseException( e ) }", Environment.NewLine );
         }
         return deleted;
      }

      private bool TryDleteSN( string subnetid, out string message )
      {
         var deleted = false;
         message = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               this.DeleteSubnet( out message, out deleted, ec2Client, subnetid );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to delete subnet { subnetid }. Error:{ Common.ParseException( e ) }";
         }
         return deleted;
      }

      private bool TryDelteKP( string keyName, out string message )
      {
         var deleted = false;
         message = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               var delResponse = ec2Client.DeleteKeyPairAsync( new DeleteKeyPairRequest( keyName ) ).Result;
               deleted = delResponse.HttpStatusCode == System.Net.HttpStatusCode.OK;
               if ( !deleted )
                  message = delResponse.ToString();
               else
                  CommonShared.CacheManager.RemoveFromCache( keyName );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to delete key pair { keyName}. Error:{ Common.ParseException( e ) }";
         }
         return deleted;
      }

      private bool TryDelSG2( out string message, string groupname = null, string groupid = null )
      {
         var deleted = false;
         message = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               this.DeleteSecurityGroup( out message, out deleted, ec2Client, groupname, groupid );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to delete Security Group. Error:{ Common.ParseException( e ) }";
         }
         return deleted;
      }

      private bool TryDeleteInternetGateway2( out string message, string internetgatewayid )
      {
         var deleted = false;
         message = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               this.DeleteInternetGateway( out message, out deleted, ec2Client, internetgatewayid );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to delete Internet Gateway. Error:{ Common.ParseException( e ) }";
         }
         return deleted;
      }

      private bool TryDeleteRT( out string message, string routetableid )
      {
         var deleted = false;
         message = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               this.DeleteRouteTable( out deleted, ec2Client, routetableid );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to delete route table. Error:{ Common.ParseException( e ) }";
         }
         return deleted;
      }

      private bool TryDeleteR( out string message, string routetableid, string targetCidrBlock )
      {
         var deleted = false;
         message = string.Empty;
         try
         {
            using ( var ec2Client = this.EC2Client() )
            {
               this.DeleteRoute( out deleted, ec2Client, routetableid, targetCidrBlock );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to delete route. Error:{ Common.ParseException( e ) }";
         }
         return deleted;
      }

      #endregion TryDelete private

      #region Delete Private
      private void DeleteInternetGateway( out string message, out bool deleted, AmazonEC2Client ec2Client, string internetgatewayid = null )
      {
         message = string.Empty;
         deleted = false;

         var delResponse = ec2Client.DeleteInternetGatewayAsync( new DeleteInternetGatewayRequest() { InternetGatewayId= internetgatewayid } ).Result;
         deleted = delResponse.HttpStatusCode == System.Net.HttpStatusCode.OK;
         if ( !deleted )
            message = delResponse.ToString();
      }

      private void DeleteSecurityGroup( out string message, out bool deleted, AmazonEC2Client ec2Client, string groupname = null, string groupid = null )
      {
         message = string.Empty;
         deleted = false;
         if ( string.IsNullOrEmpty( groupname ) && string.IsNullOrEmpty( groupid ) )
            message = $"Cannot delete SG. Please enter groupname or groupid.";
         else
         {
            var delRequest = new DeleteSecurityGroupRequest();
            if ( !string.IsNullOrEmpty( groupid ) )
               delRequest.GroupId = groupid;
            if ( !string.IsNullOrEmpty( groupname ) )
               delRequest.GroupName = groupname;
            var delResponse = ec2Client.DeleteSecurityGroupAsync(delRequest).Result;
            deleted = delResponse.HttpStatusCode == System.Net.HttpStatusCode.OK;
            if ( !deleted )
               message = delResponse.ToString();
         }
      }

      private void DeleteVpc( out string message, out bool deleted, AmazonEC2Client ec2Client, string vpcid, bool deleteDependents = false )
      {
         message = string.Empty;
         deleted = false;
         if ( deleteDependents )
         {
            //delete subnets
            DeleteSubnetsByVPCID( out message, ec2Client, vpcid );
            //delete sg
            DeleteSecurityGroupByVPCID( out string message2, ec2Client, vpcid );

            CommonShared.StringUtils.AddString( ref message, message2, Environment.NewLine );
            //delete ig
            //DeleteInternetGatewayByVPCID( out string message3, ec2Client, vpcid );

            //CommonShared.StringUtils.AddString( ref message, message3, Environment.NewLine );
         }

         var delRequest = new DeleteVpcRequest();
         delRequest.VpcId = vpcid;

         var delResponse = ec2Client.DeleteVpcAsync( delRequest ).Result;
         deleted = delResponse.HttpStatusCode == System.Net.HttpStatusCode.OK;
         if ( !deleted )
            CommonShared.StringUtils.AddString( ref message, delResponse.ToString(), Environment.NewLine );
      }

      private void DeleteSubnet( out string message, out bool deleted, AmazonEC2Client ec2Client, string subnetid )
      {
         message = string.Empty;
         deleted = false;
         var delResponse = ec2Client.DeleteSubnetAsync( new DeleteSubnetRequest() { SubnetId = subnetid } ).Result;
         deleted = delResponse.HttpStatusCode == System.Net.HttpStatusCode.OK;
         if ( !deleted )
            message = delResponse.ToString();
      }

      private void DeleteSecurityGroupByVPCID( out string message, AmazonEC2Client ec2Client, string vpcid )
      {
         message = string.Empty;
         var sgs = this.SecurityGroups( ec2Client, vpcid );
         if ( sgs != null && sgs.Count > 0 )
         {
            foreach ( var sg in sgs )
            {
               try
               {
                  if ( !sg.GroupName.Equals( Common.DEFAULT ) )
                  {
                     var delResponse = ec2Client.DeleteSecurityGroupAsync(new DeleteSecurityGroupRequest(){  GroupId = sg.GroupId } ).Result;
                     if ( !( delResponse.HttpStatusCode == System.Net.HttpStatusCode.OK ) )
                        CommonShared.StringUtils.AddString( ref message, delResponse.ToString(), Environment.NewLine );
                     else
                        CommonShared.StringUtils.AddString( ref message, $"Deleted security group [{sg.GroupId}] for vpc id [{ vpcid }].", Environment.NewLine );
                  }
                  else
                     CommonShared.StringUtils.AddString( ref message, $"Default security group [{sg.GroupId}] cannot be deleted.", Environment.NewLine );
               }
               catch ( Exception ex )
               {
                  CommonShared.StringUtils.AddString( ref message, $"Error:{ Common.ParseException( ex ) }", Environment.NewLine );
               }
            }
         }
         else
         {
            message = $"No Security Group is attched to vpc id [{vpcid}] to delete.";
         }
      }


      private void DeleteRoute( out bool created, AmazonEC2Client ec2Client, string routetableid, string targetCidrBlock )
      {
         var response = ec2Client.DeleteRouteAsync( new DeleteRouteRequest(){ DestinationCidrBlock = targetCidrBlock, RouteTableId = routetableid } ).Result;
         created = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
      }

      private void DeleteRouteTable( out bool deleted, AmazonEC2Client ec2Client, string routetableid )
      {
         var response = ec2Client.DeleteRouteTableAsync( new DeleteRouteTableRequest() { RouteTableId = routetableid } ).Result;
         deleted = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
      }

      private List<T> DescribeGeneric<T>( AmazonEC2Client ec2Client, string filters = null )
      {
         List<Filter> f = null;
         object o = null;
         if ( !string.IsNullOrEmpty( filters ) )
            f = Utilities.ConvertToFilter( filters );

         switch ( typeof( T ).Name.ToLower() )
         {
            case "vpc":
               var requestvpc = new DescribeVpcsRequest();
               if ( f != null )
                  requestvpc.Filters = f;
               o = (object)ec2Client.DescribeVpcsAsync( requestvpc ).Result.Vpcs;
               break;
            case "securitygroup":
               var requestsg = new DescribeSecurityGroupsRequest();
               if ( f != null )
                  requestsg.Filters = f;
               o = (object)ec2Client.DescribeSecurityGroupsAsync( requestsg ).Result.SecurityGroups;
               break;
            case "subnet":
               var requestsn = new DescribeSubnetsRequest();
               if ( f != null )
                  requestsn.Filters = f;
               o = (object)ec2Client.DescribeSubnetsAsync( requestsn ).Result.Subnets;
               break;
            case "instance":
               var requestec2 = new DescribeInstancesRequest();
               if ( f != null )
                  requestec2.Filters = f;
               var res = this.DescribeGeneric<Reservation>(ec2Client, filters);

               var listOfEC2Instances = new List<Instance>();
               foreach ( var instanceArray in res )
               {
                  foreach ( var instance in instanceArray.Instances )
                     listOfEC2Instances.Add( instance );
               }
               o = (object)listOfEC2Instances;
               break;
            case "reservation":
               var requestrs = new DescribeInstancesRequest();
               if ( f != null )
                  requestrs.Filters = f;
               o = (object)ec2Client.DescribeInstancesAsync( requestrs ).Result.Reservations;
               break;
            case "internetgateway":
               var requestig = new DescribeInternetGatewaysRequest();
               if ( f != null )
                  requestig.Filters = f;
               o = (object)ec2Client.DescribeInternetGatewaysAsync( requestig ).Result.InternetGateways;
               break;
            case "keypairinfo":
               var requestkp = new DescribeKeyPairsRequest();
               if ( f != null )
                  requestkp.Filters = f;
               o = (object)ec2Client.DescribeKeyPairsAsync( requestkp ).Result.KeyPairs;
               break;
            case "image":
               var requestim = new DescribeImagesRequest();
               if ( f != null )
                  requestim.Filters = f;
               o = (object)ec2Client.DescribeImagesAsync( requestim ).Result.Images;
               break;
            case "tagdescription":
               var requesttag = new DescribeTagsRequest();
               if ( f != null )
                  requesttag.Filters = f;
               o = (object)ec2Client.DescribeTagsAsync( requesttag ).Result.Tags;
               break;
            case "routetable":
               var requestrt = new DescribeRouteTablesRequest();
               if ( f != null )
                  requestrt.Filters = f;
               o = (object)ec2Client.DescribeRouteTablesAsync( requestrt ).Result.RouteTables;
               break;
            default:
               //var request = new DescribeInstancesRequest();
               //if ( f != null )
               //   request.Filters = f;
               //o = (object)ec2Client.DescribeInstancesAsync( request ).Result.Reservations;
               break;
         }
         return o == null ? null : (List<T>)o;
      }

      private void DeleteSubnetsByVPCID( out string message, AmazonEC2Client ec2Client, string vpcid )
      {
         message = string.Empty;

         var sns = this.Subnets( ec2Client, null, vpcid );
         if ( sns != null && sns.Count > 0 )
         {
            foreach ( var sn in sns )
            {
               try
               {
                  var delResponse = ec2Client.DeleteSubnetAsync( new DeleteSubnetRequest() { SubnetId = sn.SubnetId } ).Result;
                  if ( !( delResponse.HttpStatusCode == System.Net.HttpStatusCode.OK ) )
                     CommonShared.StringUtils.AddString( ref message, delResponse.ToString(), Environment.NewLine );
                  else
                     CommonShared.StringUtils.AddString( ref message, $"Deleted subnet id [{sn.SubnetId}] for vpc id [{ vpcid }].", Environment.NewLine );
               }
               catch ( Exception ex )
               {
                  CommonShared.StringUtils.AddString( ref message, $"Error:{ Common.ParseException( ex ) }", Environment.NewLine );
               }
            }
         }
         else
         {
            message = $"No subnet is attched to vpc id [{vpcid}] to delete.";
         }
      }

      private void Terminate( out string message, AmazonEC2Client ec2Client, string instanceIds )
      {
         message = string.Empty;
         var instances = this.DescribeGeneric<Instance>( ec2Client, Utilities.CreateFilterString( @"instance-id", instanceIds ) );
         var toterminate = instanceIds.Split( Common.ResourceDelimeters, StringSplitOptions.RemoveEmptyEntries );
         var request = new TerminateInstancesRequest();
         request.InstanceIds = instances.Select( i => i.InstanceId ).ToList();
         var response = ec2Client.TerminateInstancesAsync(request).Result;
         foreach ( InstanceStateChange item in response.TerminatingInstances )
            message = message + $"Terminated instance: {item.InstanceId}, Instance state: {item.CurrentState.Name}{Environment.NewLine}";
         var except = toterminate.Except(  request.InstanceIds );
         if ( except != null && except.Count() > 0 )
            foreach ( var item in except )
               message = message + $"Invalid instance: { item }, cannot terminate";
      }
      #endregion Delete Private

      #endregion Delete

      private List<IamInstanceProfileAssociation> EC2InatanceProfileAssociation( AmazonEC2Client ec2Client, string instanceID )
      {
         var filters = string.Empty;
         if ( !string.IsNullOrEmpty( instanceID ) )
         {
            Utilities.AddToFilterString( ref filters, "instance-id", instanceID );
         }

         return DescribeGeneric<IamInstanceProfileAssociation>( ec2Client, filters );

         //var request = new DescribeIamInstanceProfileAssociationsRequest();
         //var filter = new Filter()
         //{
         //   Name = @"instance-id",
         //   Values = new List<string>() { instanceID }
         //};
         //request.Filters.Add( filter );

         //return ec2Client.DescribeIamInstanceProfileAssociationsAsync( request ).Result.IamInstanceProfileAssociations.FirstOrDefault();
      }

      private Instance EC2Inatance( AmazonEC2Client ec2Client, string instanceID )
      {
         return ec2Client.DescribeInstancesAsync( new DescribeInstancesRequest() { InstanceIds = new List<string>() { instanceID } } ).Result.Reservations.FirstOrDefault()?.Instances.FirstOrDefault();
      }

      private List<Instance> LaunchInatances(
         out string message,
         out bool launched,
         AmazonEC2Client ec2Client,
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
         List<Amazon.EC2.Model.Tag> tags = null )
      {
         launched = false;
         message = string.Empty;
         var launch = false;
         List<Instance> instances  = new List<Instance>(); //blank list
         //check valid ami id
         if ( string.IsNullOrEmpty( amiID ) )
            message = $"Missing ami/image id, Please use a valid image id.";
         else if ( sg == null || sg.Count == 0 )
            message = $"Missing security group, Please use valid security groups.";
         else if ( string.IsNullOrEmpty( keyName ) )
            message = $"Missing Key Name, Please use valid Key Name.";
         else
         {
            var hasSubnet = false;
            launch = true;
            if ( !string.IsNullOrEmpty( subnetid ) ) //launch in vpc
            {
               var sn = this.Subnets( ec2Client, subnetid ).FirstOrDefault();
               hasSubnet = sn != null;
               if ( !hasSubnet )
                  message = $"Invalid subnet id [{subnetid}], Please use a valid subnet id.";
               launch = hasSubnet;
            }

            if ( launch )
            {
               var image = this.Images( ec2Client, amiID ).FirstOrDefault();

               if ( image != null )
               {
                  //check sg
                  var sgs = this.SecurityGroups( ec2Client, null, String.Join('|', sg)  );
                  if ( sgs != null && sgs.Count == sg.Count )
                  {
                     //check key pair name
                     var kp = this.KeyPairs( ec2Client, keyName ).FirstOrDefault();
                     if ( kp != null )
                     {
                        if ( string.IsNullOrEmpty( instanceType ) )
                           instanceType = @"t2.micro";

                        var launchRequest = new RunInstancesRequest()
                        {
                           ImageId = amiID,
                           InstanceType = InstanceType.FindValue( instanceType ),
                           MinCount = minCount,
                           MaxCount = maxCount,
                           KeyName = keyName
                        };

                        if ( hasSubnet )
                        {
                           var eni = new InstanceNetworkInterfaceSpecification()
                           {
                              DeviceIndex = deviceIndex,
                              SubnetId = subnetid,
                              Groups = sg,
                              AssociatePublicIpAddress = associatepublicip
                           };
                           List<InstanceNetworkInterfaceSpecification> enis = new List<InstanceNetworkInterfaceSpecification>() {eni};
                           launchRequest.NetworkInterfaces = enis;
                        }
                        else
                           launchRequest.SecurityGroupIds = sg;

                        //user data
                        if ( !string.IsNullOrEmpty( userData ) )
                           launchRequest.UserData = userData;

                        if ( !string.IsNullOrEmpty( iamRole ) )
                           launchRequest.IamInstanceProfile = new IamInstanceProfileSpecification() { Name = iamRole };

                        //launch
                        var launchResponse = ec2Client.RunInstancesAsync(launchRequest).Result;
                        instances = launchResponse.Reservation.Instances;
                        launched = instances.Count > 0;
                        if ( launched )
                        {
                           var ids = string.Join( Common.ResourceDelimeter, instances.Select( i => i.InstanceId ) );
                           if ( tags != null && tags.Count > 0 )
                           {
                              this.TrySetTagsLocal( out message, ids, tags );
                           }

                           CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_LAUNCHED_INATANCES_KEY, ids );
                        }
                     }
                     else
                        message = $"Invalid Key Name [{keyName}], Please use a valid key name.";
                  }
                  else
                     message = $"Invalid security group(s) [{String.Join( ',', sg )}], Please use valid security group(s).";

               }
               else
                  message = $"Invalid ami/image id [{amiID}], Please use a valid image id.";
            }
         }
         return instances;
      }

      private bool TryGetInstance( string instanceID, out Instance instance, out string error )
      {
         var success = false;
         instance = null;
         error = string.Empty;
         try
         {
            if ( !currentInstances.TryGetValue( instanceID, out instance ) )
            {
               using ( var ec2Client = this.EC2Client() )
               {
                  instance = this.EC2Inatance( ec2Client, instanceID );
                  if ( instance != null )
                  {
                     currentInstances.TryAdd( instanceID, instance );
                     success = true;
                  }
                  else
                     error = $"Unable to get ec2 instance for instance id [{instanceID}]. Please check the instance id.";
               }
            }
            else
               success = true;
         }
         catch ( Exception e )
         {
            instance = null;
            error = $"Unable to get ec2 instance for instance id [{instanceID}]. Error:{ Common.ParseException( e ) }";
         }
         return success;
      }

      private bool TryGetWindowsPassword( out string message, out string password, AmazonEC2Client ec2Client, string instanceId, string privatekey )
      {
         message = string.Empty;
         password = string.Empty;
         var request = new GetPasswordDataRequest();
         request.InstanceId = instanceId;
         var success = false;
         var response = ec2Client.GetPasswordDataAsync(request).Result;
         if ( null != response.PasswordData )
         {
            password = response.GetDecryptedPassword( privatekey );
            success = true;
         }
         else
            message = $"The password is not available. The password for instance {instanceId} is either not ready, or it is not a Windows instance.";

         return success;
      }

      public Instance CurrentEC2Instance
      {
         get
         {
            if ( this.currentEC2Instance == null )
            {
               if ( !string.IsNullOrEmpty( currentEC2InstanceID ) )
               {
                  if ( this.TrySetCurrentEC2InatanceLocal( this.currentEC2InstanceID, out string error ) )
                     return this.currentEC2Instance;
                  else
                     throw new Exception( $"Error setting current EC2 instance for instance id:[{this.currentEC2InstanceID}], Error: {error}" );
               }
               else
                  throw new Exception( "Current EC2 instance is not set! Please set the instance id before using." );
            }
            else
               return this.currentEC2Instance;
         }
      }

      static string GetProjectInstallationKey( string projectName )
      {
         return $"{projectName}-agent-installation-token";
      }

      //private Common awsCommon = Common.CommonInstance;

      private static readonly EC2 EC2Instance = new EC2();

      private Instance currentEC2Instance = null;

      private string currentEC2InstanceID = CommonShared.Utilities.GetValueOfEnvVar( Common.AWS_CURRENT_EC2_INSTANCE_ID );

      private ConcurrentDictionary<string, Instance> currentInstances = new ConcurrentDictionary<string, Instance>( StringComparer.OrdinalIgnoreCase);
   }
}
