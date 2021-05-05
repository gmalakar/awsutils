#region history
//*****************************************************************************
// EC2Instance.cs:
//
// History:
// 08/06/20 - Goutam Malakar 
//*****************************************************************************
#endregion history
using System;
using System.Security;
using System.Linq;

namespace AWSCM.AWSConfigManager.Utilities
{
   public class EC2Instance
   {
      private Arguments parameters;

      public EC2Instance( Arguments args )
      {
         parameters = args;
         switch ( Common.GetService( parameters ) )
         {
            case @"check":
            case @"chk":
               this.DoCheck();
               break;
            case @"tags":
               this.DoTags();
               break;
            case @"vpc":
               this.DoVpc();
               break;
            case @"sn":
            case @"subnet":
               this.DoSubnet();
               break;
            case @"sg":
               this.DoSg();
               break;
            case @"iprange":
               this.DoIP();
               break;
            case @"kp":
            case @"keypair":
               this.DoKP();
               break;
            case @"ec2":
               this.DoEC2();
               break;
            case @"ig":
            case "internetgateway":
               this.DoIG();
               break;
            case @"rt":
            case "routetable":
               this.DoRT();
               break;
            case @"r":
            case "route":
               this.DoR();
               break;
            default:
               Common.ThrowError( "Invalid service!" );
               break;
         }
      }

      private void DoCheck()
      {
         var instanceid = parameters.GetArgumentValue( @"instanceid" );
         if ( CommonShared.Utilities.IsUseLast( instanceid ) )
         {
            instanceid = AWSInterface.Utilities.LastLaunchedEC2Instance;
            if ( string.IsNullOrEmpty( instanceid ) )
               ThrowLastCreatedError( "EC2 Instance", "EC2" );
         }
         if ( !AWSInterface.Utilities.TrySetCurrentEC2InatanceID( instanceid, out string error ) )
            Common.WriteMessage( error );
         else
            Common.WriteMessage( @"Valid instance!" );
      }

      private void DoVpc()
      {
         var vpcid = string.Empty;
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"last":
               vpcid = AWSInterface.Utilities.LastCreatedVpcID;
               if ( string.IsNullOrEmpty( vpcid ) )
                  vpcid = NoLastCreatedMessage( "VPC ID" );
               Common.WriteMessage( vpcid );
               break;
            case @"set":
            case @"create":
               var name = parameters.GetArgumentValue( @"name", false );
               var ipv4cidr = parameters.GetArgumentValue( @"ipv4cidr" );
               var tenancy = parameters.GetArgumentValue( @"tenancy", false );
               var enableDNSSupportFlag = parameters.GetArgumentValue( @"dnssupportflag", false );
               var enableDNSHostNameFlag  = parameters.GetArgumentValue( @"dnshostnameflag", false );
               if ( AWSInterface.Utilities.TryCreateVPC( out string message, out vpcid, ipv4cidr, enableDNSSupportFlag, enableDNSHostNameFlag, name, tenancy ) )
               {
                  Common.WriteMessage( $"VPC is created. VpcId:[{ vpcid }]" );
                  Common.WriteMessage( message );
               }
               else
                  Common.WriteMessage( $"Cannot create Vpc name:{ name }, Error:{ message}" );
               break;
            case @"del":
            case @"delete":
               vpcid = parameters.GetArgumentValue( @"vpcid" );
               var delDependents = parameters.GetArgumentValueAsBoolean("deldependents", false );
               if ( CommonShared.Utilities.IsUseLast( vpcid ) )
               {
                  vpcid = AWSInterface.Utilities.LastCreatedVpcID;
                  if ( string.IsNullOrEmpty( vpcid ) )
                     ThrowLastCreatedError( "VPD ID", "VPC" );
               }
               if ( AWSInterface.Utilities.TryDeleteVPC( out message, vpcid, delDependents ) )
               {
                  Common.WriteMessage( message );
                  Common.WriteMessage( $"VPC is deleted. VPC id:[{ vpcid }]" );
               }
               else
                  Common.WriteMessage( $"Error:{ message}" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }
      }

      private void DoSubnet()
      {
         var subnetid = string.Empty;

         switch ( Common.GetAction( this.parameters ) )
         {
            case @"last":
               subnetid = AWSInterface.Utilities.LastCreatedSubnetID;
               if ( string.IsNullOrEmpty( subnetid ) )
                  subnetid = NoLastCreatedMessage( "Subnet ID" );
               Common.WriteMessage( subnetid );
               break;
            case @"set":
            case @"create":
               var name = parameters.GetArgumentValue( @"name", false );
               var cidrBlock = parameters.GetArgumentValue( @"cidrblock" );
               var vpcid = parameters.GetArgumentValue( @"vpcid" );
               if ( CommonShared.Utilities.IsUseLast( vpcid ) )
               {
                  vpcid = AWSInterface.Utilities.LastCreatedVpcID;
                  if ( string.IsNullOrEmpty( vpcid ) )
                     ThrowLastCreatedError( "VPD ID", "VPC" );
               }
               if ( AWSInterface.Utilities.TryCreateSubnet( out string message, out subnetid, cidrBlock, vpcid, name ) )
               {
                  Common.WriteMessage( $"Subnet is created. SubnetID:[{ subnetid }]" );
                  Common.WriteMessage( message );
               }
               else
                  Common.WriteMessage( $"Cannot create subnet name:{ name }, Error:{ message}" );
               break;
            case @"del":
            case @"delete":
               subnetid = parameters.GetArgumentValue( @"subnetid" );
               if ( !string.IsNullOrEmpty( subnetid ) && CommonShared.Utilities.IsUseLast( subnetid ) )
               {
                  subnetid = AWSInterface.Utilities.LastCreatedSubnetID;
                  if ( string.IsNullOrEmpty( subnetid ) )
                     ThrowLastCreatedError( "Subnet ID", "Subnet" );
               }
               if ( AWSInterface.Utilities.TryDeleteSubnet( out message, subnetid ) )
                  Common.WriteMessage( $"Subnet is deleted. SubnetID:[{ subnetid }]" );
               else
                  Common.WriteMessage( $"Error:{ message}" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }
      }

      private void DoSg()
      {
         var grpname = string.Empty;
         var message = string.Empty;
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"last":
               var sgid = AWSInterface.Utilities.LastCreatedSecurityGroupID;
               if ( string.IsNullOrEmpty( sgid ) )
                  sgid = NoLastCreatedMessage( "Security Group ID" );
               Common.WriteMessage( sgid );
               break;
            case @"set":
            case @"create":
               grpname = parameters.GetArgumentValue( @"groupname" );
               var desc = parameters.GetArgumentValue( @"description" );
               var vpcid = parameters.GetArgumentValue( @"vpcid", false );
               if ( CommonShared.Utilities.IsUseLast( vpcid ) )
               {
                  vpcid = AWSInterface.Utilities.LastCreatedVpcID;
                  if ( string.IsNullOrEmpty( vpcid ) )
                     ThrowLastCreatedError( "VPD ID", "VPC" );
               }
               if ( AWSInterface.Utilities.TryCreateSG( out message, out string groupid, grpname, desc, vpcid ) )
                  Common.WriteMessage( $"SG is created. groupid:[{ groupid }]" );
               else
                  Common.WriteMessage( $"Cannot create SG group name:{ grpname }, Error:{ message}" );
               break;
            case @"del":
            case @"delete":
               grpname = parameters.GetArgumentValue( @"groupname", false );
               var grpid = parameters.GetArgumentValue( @"groupid", false );
               if ( string.IsNullOrEmpty( grpid ) && string.IsNullOrEmpty( grpname ) )
                  Common.WriteMessage( $"Missing parameters. Pass either groupname or groupid" );
               else
               {
                  if ( !string.IsNullOrEmpty( grpid ) && CommonShared.Utilities.IsUseLast( grpid ) )
                  {
                     grpid = AWSInterface.Utilities.LastCreatedSecurityGroupID;
                     if ( string.IsNullOrEmpty( grpid ) )
                        ThrowLastCreatedError( "Security Group ID", "Security Group" );
                  }

                  if ( AWSInterface.Utilities.TryDeleteSG( out message, grpname, grpid ) )
                     Common.WriteMessage( $"SG is deleted." );
                  else
                     Common.WriteMessage( $"Error:{ message}" );
               }
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }
      }

      private void DoIP()
      {
         switch ( Common.GetAction( this.parameters ) )
         {
            //TryCreateIPRangePermission( out string message, string groupId = null, string cidrIp = null, string ipProtocol = null, int fromPort = 22, int toPort = 22 )
            case @"set":
            case @"create":
               var groupId = parameters.GetArgumentValue( @"groupid" );
               if ( CommonShared.Utilities.IsUseLast( groupId ) )
               {
                  groupId = AWSInterface.Utilities.LastCreatedSecurityGroupID;
                  if ( string.IsNullOrEmpty( groupId ) )
                     ThrowLastCreatedError( "Security Group ID", "Security Group" );
               }
               var cidrIp = parameters.GetArgumentValue( @"cidrip", false );
               var ipProtocol = parameters.GetArgumentValue( @"ipprotocol", false );

               var fromPort = - 1;
               Int32.TryParse( parameters.GetArgumentValue( @"fromport", false ), out fromPort );

               var toPort = -1;
               Int32.TryParse( parameters.GetArgumentValue( @"toport", false ), out toPort );

               var egress  = parameters.GetArgumentValueAsBoolean( @"egress", false );

               if ( AWSInterface.Utilities.TryCreateIPRangePermission( out string message, groupId, egress, cidrIp, ipProtocol, fromPort, toPort ) )
                  Common.WriteMessage( $"IP Range permission is created." );
               else
                  Common.WriteMessage( $"Cannot create IP Range permission, Error:{ message}" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }
      }

      private void DoIG()
      {
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"set":
            case @"create":
               var vpcid = parameters.GetArgumentValue( @"vpcid", false );
               if ( CommonShared.Utilities.IsUseLast( vpcid ) )
               {
                  vpcid = AWSInterface.Utilities.LastCreatedVpcID;
                  if ( string.IsNullOrEmpty( vpcid ) )
                     ThrowLastCreatedError( "VPC ID", "VPC" );
               }

               if ( AWSInterface.Utilities.TryCreateAndAtachInternetGateway( out string message, out string igid, vpcid ) )
                  Common.WriteMessage( $"Internet Gateway [{igid}] is created for vpc id [{ vpcid }]." );
               else
                  Common.WriteMessage( $"Cannot create Internet Gateway, Error:{ message}" );
               break;
            case @"del":
            case @"delete":
               var igid2 = parameters.GetArgumentValue( @"internetgatewayid" );
               if ( AWSInterface.Utilities.TryDeleteInternetGateway( out message, igid2 ) )
                  Common.WriteMessage( $"Internet Gateway is deleted." );
               else
                  Common.WriteMessage( $"Error:{ message}" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }
      }

      private void DoRT()
      {
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"set":
            case @"create":
               var vpcid = parameters.GetArgumentValue( @"vpcid", false );
               if ( CommonShared.Utilities.IsUseLast( vpcid ) )
               {
                  vpcid = AWSInterface.Utilities.LastCreatedVpcID;
                  if ( string.IsNullOrEmpty( vpcid ) )
                     ThrowLastCreatedError( "VPC ID", "VPC" );
               }

               if ( AWSInterface.Utilities.TryCreateRouteTableForVPC( out string message, out string routetableid, vpcid ) )
                  Common.WriteMessage( $"Route Table [{routetableid}] is created for vpc id [{ vpcid }]." );
               else
                  Common.WriteMessage( $"Cannot create Route Table, Error:{ message}" );
               break;
            case @"del":
            case @"delete":
               var routetableid2 = parameters.GetArgumentValue( @"routetableid" );
               if ( CommonShared.Utilities.IsUseLast( routetableid2 ) )
               {
                  routetableid2 = AWSInterface.Utilities.LastCreatedRouteTableID;
                  if ( string.IsNullOrEmpty( routetableid2 ) )
                     ThrowLastCreatedError( "Route Table ID", "Route Table" );
               }
               if ( AWSInterface.Utilities.TryDeleteRouteTable( out message, routetableid2 ) )
                  Common.WriteMessage( $"Route Table is deleted." );
               else
                  Common.WriteMessage( $"Error:{ message}" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }
      }

      private void DoR()
      {
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"set":
            case @"create":
               var routetableid = parameters.GetArgumentValue( @"routetableid" );
               if ( CommonShared.Utilities.IsUseLast( routetableid ) )
               {
                  routetableid = AWSInterface.Utilities.LastCreatedRouteTableID;
                  if ( string.IsNullOrEmpty( routetableid ) )
                     ThrowLastCreatedError( "Route Table ID", "Route Table" );
               }

               var gatewayid = parameters.GetArgumentValue( @"internetgatewayid" );
               if ( CommonShared.Utilities.IsUseLast( gatewayid ) )
               {
                  gatewayid = AWSInterface.Utilities.LastCreatedGatewayID;
                  if ( string.IsNullOrEmpty( routetableid ) )
                     ThrowLastCreatedError( "Internet Gateway ID", "Internet Gateway" );
               }

               var dest = parameters.GetArgumentValue( @"destination" );

               if ( AWSInterface.Utilities.TryCreateRoute( out string message, routetableid, gatewayid, dest ) )
                  Common.WriteMessage( $"Route is created." );
               else
                  Common.WriteMessage( $"Cannot create Route Error:{ message}" );
               break;
            case @"del":
            case @"delete":
               var routetableid2 = parameters.GetArgumentValue( @"routetableid" );
               if ( CommonShared.Utilities.IsUseLast( routetableid2 ) )
               {
                  routetableid2 = AWSInterface.Utilities.LastCreatedRouteTableID;
                  if ( string.IsNullOrEmpty( routetableid2 ) )
                     ThrowLastCreatedError( "Route Table ID", "Route Table" );
               }

               var dest2 = parameters.GetArgumentValue( @"destination" );
               if ( AWSInterface.Utilities.TryDeleteRoute( out message, routetableid2, dest2 ) )
                  Common.WriteMessage( $"Route is deleted." );
               else
                  Common.WriteMessage( $"Error:{ message}" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }
      }

      private void DoKP()
      {
         var keyname = string.Empty;
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"last":
               var kpname = AWSInterface.Utilities.LastCreatedKeyPairName;
               if ( string.IsNullOrEmpty( kpname ) )
                  kpname = NoLastCreatedMessage( "Key Pair Name" );
               Common.WriteMessage( kpname );
               break;
            case @"set":
            case @"create":
               keyname = parameters.GetArgumentValue( @"keyname" );
               var filename = CommonShared.Utilities.ValidKeyPairFilePath( parameters.GetArgumentValue( @"filename" ) );
               if ( AWSInterface.Utilities.TryCreateKeyPair( out string message, keyname, filename ) )
                  Common.WriteMessage( $"Key Pair is created at [{filename}]." );
               else
                  Common.WriteMessage( $"Cannot create key pair, Error:{ message}" );
               break;
            case @"del":
            case @"delete":
               keyname = parameters.GetArgumentValue( @"keyname" );
               if ( CommonShared.Utilities.IsUseLast( keyname ) )
               {
                  keyname = AWSInterface.Utilities.LastCreatedKeyPairName;
                  if ( string.IsNullOrEmpty( keyname ) )
                     ThrowLastCreatedError( "Key Pair Name", "Key Pair" );
               }
               if ( AWSInterface.Utilities.TryDeleteKeyPair( out message, keyname ) )
                  Common.WriteMessage( $"Key Pair is deleted. Key Name:[{ keyname }]" );
               else
                  Common.WriteMessage( $"Error:{ message}" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }
      }

      private void DoEC2()
      {
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"chk":
               this.DoCheck();
               break;
            case @"password":
            case @"pwd":
               this.DoPWD();
               break;
            case @"launch":
               this.DoLaunch();
               break;
            case @"assoc":
            case @"associate":
               this.DoAssociate();
               break;
            case @"terminate":
               this.DoTerminate();
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }
      }

      private void DoPWD()
      {
         var id = parameters.GetArgumentValue( @"instanceid" );
         var keyfile = parameters.GetArgumentValue( @"keyfilepath", false );
         var keydata = parameters.GetArgumentValue( @"privatekey", false );
         if ( !string.IsNullOrEmpty( keyfile ) )
         {
            if ( AWSInterface.Utilities.TryGetWinPasswordUsingKeyFile( out string message, out string password, id, keyfile ) )
            {
               Common.WriteMessage( $"Passord: { password}" );
            }
            else
               Common.WriteMessage( $"Cannot Get, Error:{ message}" );
         }
         else if ( !string.IsNullOrEmpty( keydata ) )
         {
            if ( AWSInterface.Utilities.TryGetWinPassword( out string message, out string password, id, keydata ) )
            {
               Common.WriteMessage( $"Passord: { password}" );
            }
            else
               Common.WriteMessage( $"Cannot Get, Error:{ message}" );
         }
         else
            Common.WriteMessage( $"Missing parameter \"keyfilepath\" or \"privatekey\"." );
      }

      private void DoLaunch()
      {
         var amiID = parameters.GetArgumentValue( @"amiid" );
         var keyName = parameters.GetArgumentValue( @"keyname" );
         if ( CommonShared.Utilities.IsUseLast( keyName ) )
         {
            keyName = AWSInterface.Utilities.LastCreatedKeyPairName;
            if ( string.IsNullOrEmpty( keyName ) )
               ThrowLastCreatedError( "Key Pair Name", "Key Pair" );
         }

         var group = parameters.GetArgumentValue( @"groups" );
         if ( CommonShared.Utilities.IsUseLast( group ) )
         {
            group = AWSInterface.Utilities.LastCreatedSecurityGroupID;
            if ( string.IsNullOrEmpty( group ) )
               ThrowLastCreatedError( "Security Group ID", "Security Group" );
         }
         var sg = group.Split('|').ToList();

         var subnetid = parameters.GetArgumentValue( @"subnetid", false );
         if ( !string.IsNullOrEmpty( subnetid ) && CommonShared.Utilities.IsUseLast( subnetid ) )
         {
            subnetid = AWSInterface.Utilities.LastCreatedSubnetID;
            if ( string.IsNullOrEmpty( subnetid ) )
               ThrowLastCreatedError( "Subnet ID", "Subnet" );
         }

         var iamRole = parameters.GetArgumentValue( @"iamrole", false );
         if ( !string.IsNullOrEmpty( iamRole ) && CommonShared.Utilities.IsUseLast( iamRole ) )
         {
            iamRole = AWSInterface.Utilities.LastCreatedInstanceProfile;
            if ( string.IsNullOrEmpty( iamRole ) )
               ThrowLastCreatedError( "IAM Role", "IAM Role" );
         }

         var instanceType = parameters.GetArgumentValue( @"instancetype", false );
         var minCountStr = parameters.GetArgumentValue( @"mincount", false );
         var maxCountStr = parameters.GetArgumentValue( @"maxcount", false );
         var deviceIndexStr = parameters.GetArgumentValue( @"deviceindex", false );
         var tags = parameters.GetArgumentValue( @"tags", false );
         var minCount = 1;
         var maxCount = 1;
         var deviceindex = 0;
         var associatepublicip = parameters.GetArgumentValue( @"assocpublicip", false ).ToLowerInvariant() == @"true";
         var infoall = parameters.IsParamWithNoValue( @"infoall");
         var userData = parameters.GetArgumentValue( @"userdata", false );
         if ( !string.IsNullOrEmpty( userData ) )
            userData = Common.GetUserData( userData );

         if ( !string.IsNullOrEmpty( minCountStr ) && !Int32.TryParse( minCountStr, out minCount ) )
            Common.WriteMessage( $"Invalid Minimum Count [{minCountStr}]." );
         else if ( !string.IsNullOrEmpty( maxCountStr ) && !Int32.TryParse( maxCountStr, out maxCount ) )
            Common.WriteMessage( $"Invalid Maximum Count [{maxCountStr}]." );
         else if ( !string.IsNullOrEmpty( deviceIndexStr ) && !Int32.TryParse( deviceIndexStr, out deviceindex ) )
            Common.WriteMessage( $"Invalid device index [{deviceIndexStr}]." );
         else if ( AWSInterface.Utilities.TryLaunchInatances( out string message, out string instances, amiID, keyName, sg, iamRole, subnetid, instanceType, deviceindex, associatepublicip, minCount, maxCount, userData, tags ) )
         {
            Common.WriteMessage( $"Instance(s) is(are) launched:" );
            if ( infoall )
               Common.WriteMessage( instances );
            else
               Common.WriteMessage( AWSInterface.Utilities.LastLaunchedEC2Instance );

         }
         else
            Common.WriteMessage( $"Cannot launch instance(s), Error:{ message}" );
      }

      private void DoTerminate()
      {
         var instanceids = parameters.GetArgumentValue( @"instanceids" );
         if ( !string.IsNullOrEmpty( instanceids ) && CommonShared.Utilities.IsUseLast( instanceids ) )
         {
            instanceids = AWSInterface.Utilities.LastLaunchedEC2Instance;
            if ( string.IsNullOrEmpty( instanceids ) )
               ThrowLastCreatedError( "Instance ID", "Instance" );
         }

         AWSInterface.Utilities.TerminateInstances( out string message, instanceids );
         Common.WriteMessage( message );
      }

      private void DoAssociate()
      {
         switch ( Common.GetType( this.parameters ) )
         {
            case @"instanceprofile":
            case @"insprofile":
               var instanceid = parameters.GetArgumentValue( @"instanceid" );
               if ( CommonShared.Utilities.IsUseLast( instanceid ) )
               {
                  instanceid = AWSInterface.Utilities.LastLaunchedEC2Instance;
                  if ( string.IsNullOrEmpty( instanceid ) )
                     ThrowLastCreatedError( "Instance ID", "EC2 Instance" );
               }
               var profileName = parameters.GetArgumentValue( @"profilename" );
               if ( !string.IsNullOrEmpty( profileName ) && CommonShared.Utilities.IsUseLast( profileName ) )
               {
                  profileName = AWSInterface.Utilities.LastCreatedInstanceProfile;
                  if ( string.IsNullOrEmpty( profileName ) )
                     ThrowLastCreatedError( "Profile Name", "Instance Profile" );
               }
               if ( AWSInterface.Utilities.TryAssociateIamInstanceProfile( out string message, instanceid, profileName ) )
                  Common.WriteMessage( $"Instance Profile:[{ profileName }] has been associated with instnace id: [{ instanceid }]." );
               else
                  Common.WriteMessage( message );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }
      }


      private void DoTags()
      {
         var error = string.Empty;
         var resources = parameters.GetArgumentValue( @"resources");
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"set":
            case @"create":
               var tagName = parameters.GetArgumentValue( @"key" );
               var value = parameters.GetArgumentValue( @"value" );
               if ( AWSInterface.Utilities.TrySetTag( resources, tagName, value, out error ) )
                  Common.WriteMessage( $"Tag is set for Name:{ tagName }, Value:{ value }" );
               else
                  Common.WriteMessage( $"Cannot set tag for tag name:{ tagName }, Error:{ error}" );
               break;
            case @"setbyjson":
               var tagsJsonString = parameters.GetArgumentValue( @"tags" );
               if ( AWSInterface.Utilities.TrySetTags( resources, tagsJsonString, out error ) )
                  Common.WriteMessage( $"Tag(s) is(are) set for." );
               else
                  Common.WriteMessage( $"Cannot set tag for tags, Error:{ error}" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }
      }

      private void ThrowLastCreatedError( string header, string header2 )
      {
         Common.ThrowLastCreatedError( header, header2 );
      }

      private string NoLastCreatedMessage( string header )
      {
         return Common.NoLastCreatedMessage( header );
      }
   }
}
