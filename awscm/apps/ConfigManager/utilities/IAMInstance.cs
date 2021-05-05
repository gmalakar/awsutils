#region history
//*****************************************************************************
// IAM.cs:
//
// History:
// 08/06/20 - Goutam Malakar 
//*****************************************************************************
#endregion history
using System;
using System.Linq;
using System.Security;

namespace AWSCM.AWSConfigManager.Utilities
{
   public class IAMInstance
   {
      private Arguments parameters;
      public IAMInstance( Arguments args )
      {
         parameters = args;
         switch ( Common.GetService( parameters ) )
         {
            case @"role":
               this.DoRole();
               break;
            case @"profile":
               this.DoProfile();
               break;
            case @"policy":
               this.DoPolicy();
               break;
            default:
               Common.ThrowError( "Invalid service!" );
               break;
         }
      }

      private void DoRole()
      {
         var rolename = string.Empty;
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"last":
               rolename = AWSInterface.Utilities.LastCreatedIAMRole;
               if ( string.IsNullOrEmpty(rolename) )
                  rolename = Common.NoLastCreatedMessage( "IAM Role" );
               Common.WriteMessage( rolename );
               break;
            case @"set":
            case @"create":
               rolename = parameters.GetArgumentValue( @"rolename" );
               var assumePolicyDoc = parameters.GetArgumentValue( @"assumepolicy" );
               var desc = parameters.GetArgumentValue( @"desc", false );
               if ( AWSInterface.Utilities.TryCreateIAMRole( out string message, rolename, assumePolicyDoc, desc ) )
               {
                  Common.WriteMessage( $"IAM Role is created. Role Name:[{ rolename }]" );
                  Common.WriteMessage( message );
               }
               else
                  Common.WriteMessage( $"Cannot create IAM Role:{ rolename }, Error:{ message}" );
               break;
            case @"attach":
               rolename = parameters.GetArgumentValue( @"rolename" );
               if ( CommonShared.Utilities.IsUseLast( rolename ) )
               {
                  rolename = AWSInterface.Utilities.LastCreatedIAMRole;
                  if ( string.IsNullOrEmpty( rolename ) )
                     Common.ThrowLastCreatedError( "Role Name", "IAM Role" );
               }
               
               var policies = parameters.GetArgumentValue( @"policynames" );
               if ( AWSInterface.Utilities.TryAddPoliciesToRole( out message, rolename, policies.Split('|').ToList() ) )
               {
                  Common.WriteMessage( $"IAM Policies are added to IAM Role:[{ rolename }]" );
                  Common.WriteMessage( message );
               }
               else
                  Common.WriteMessage( $"Cannot add IAM Policies to IAM Role:{ rolename }, Error:{ message}" );
               break;
            case @"del":
            case @"delete":
               rolename = parameters.GetArgumentValue( @"rolename" );
               if ( CommonShared.Utilities.IsUseLast( rolename ) )
               {
                  rolename = AWSInterface.Utilities.LastCreatedIAMRole;
                  if ( string.IsNullOrEmpty( rolename ) )
                     Common.ThrowLastCreatedError( "Role Name", "IAM Role" );
               }
               if ( AWSInterface.Utilities.TryDeleteRole( out message, rolename ) )
               {
                  Common.WriteMessage( $"IAM Role is deleted. Role:[{ rolename }]" );
                  Common.WriteMessage( message );
               }
               else
                  Common.WriteMessage( $"Error:{ message}" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }

      }

      private void DoPolicy()
      {
         var policyName = string.Empty;
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"last":
               policyName = AWSInterface.Utilities.LastCreatedIAMPolicy;
               if ( string.IsNullOrEmpty( policyName ) )
                  policyName = Common.NoLastCreatedMessage( "IAM Policy" );
               Common.WriteMessage( policyName );
               break;
            case @"set":
            case @"create":
               policyName = parameters.GetArgumentValue( @"policyname" );
               var jsonPolicyDoc = parameters.GetArgumentValue( @"policydocument" );
               if ( !string.IsNullOrEmpty( jsonPolicyDoc ) )
                  jsonPolicyDoc = Common.GetJsonString( jsonPolicyDoc );
               var desc = parameters.GetArgumentValue( @"desc", false );
               if ( AWSInterface.Utilities.TryCreateIAMPolicy( out string message, policyName, jsonPolicyDoc, desc ) )
               {
                  Common.WriteMessage( $"IAM Policy is created. Role Name:[{ policyName }]" );
                  Common.WriteMessage( message );
               }
               else
                  Common.WriteMessage( $"Cannot create IAM Policy:{ policyName }, Error:{ message}" );
               break;
            case @"del":
            case @"delete":
               policyName = parameters.GetArgumentValue( @"policyname" );
               if ( CommonShared.Utilities.IsUseLast( policyName ) )
               {
                  policyName = AWSInterface.Utilities.LastCreatedIAMPolicy;
                  if ( string.IsNullOrEmpty( policyName ) )
                     Common.ThrowLastCreatedError( "Policy Name", "IAM Policy" );
               }
               if ( AWSInterface.Utilities.TryDeleteRole( out message, policyName ) )
               {
                  Common.WriteMessage( $"IAM Role is deleted. Role:[{ policyName }]" );
                  Common.WriteMessage( message );
               }
               else
                  Common.WriteMessage( $"Error:{ message}" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }

      }

     private void DoProfile()
      {
         var profilename = string.Empty;
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"last":
               profilename = AWSInterface.Utilities.LastCreatedInstanceProfile;
               if ( string.IsNullOrEmpty(profilename) )
                  profilename = Common.NoLastCreatedMessage( "Instance Profile" );
               Common.WriteMessage( profilename );
               break;
            case @"set":
            case @"create":
               profilename = parameters.GetArgumentValue( @"profilename" );
               if ( AWSInterface.Utilities.TryCreateInstanceProfile( out string message, profilename, parameters.GetArgumentValue( @"rolename", false ) ) )
               {
                  Common.WriteMessage( $"Inatance Profile is created. Profile Name:[{ profilename }]" );
                  Common.WriteMessage( message );
               }
               else
                  Common.WriteMessage( $"Cannot create Inatance Profile :{ profilename }, Error:{ message}" );
               break;
            case @"attach":
               profilename = parameters.GetArgumentValue( @"profilename" );
               if ( CommonShared.Utilities.IsUseLast( profilename ) )
               {
                  profilename = AWSInterface.Utilities.LastCreatedInstanceProfile;
                  if ( string.IsNullOrEmpty( profilename ) )
                     Common.ThrowLastCreatedError( "Instance Profile Name", "Instance Profile" );
               }
               
               var roles = parameters.GetArgumentValue( @"rolenames" );
               if ( CommonShared.Utilities.IsUseLast( roles ) )
               {
                  roles = AWSInterface.Utilities.LastCreatedIAMRole;
                  if ( string.IsNullOrEmpty( roles ) )
                     Common.ThrowLastCreatedError( "Role Name", "IAM Role" );
               }


               if ( AWSInterface.Utilities.TryAddRolesToInstanceProfile( out message, profilename, roles.Split( '|' ).ToList() ) )
               {
                  Common.WriteMessage( $"IAM Roles are added to Instance Profile:[{ profilename }]" );
                  Common.WriteMessage( message );
               }
               else
                  Common.WriteMessage( $"Cannot add IAM Roles to IAM Profile:{ profilename }, Error:{ message}" );
               break;
            case @"del":
            case @"delete":
               profilename = parameters.GetArgumentValue( @"profilename" );
               if ( CommonShared.Utilities.IsUseLast( profilename ) )
               {
                  profilename = AWSInterface.Utilities.LastCreatedInstanceProfile;
                  if ( string.IsNullOrEmpty( profilename ) )
                     Common.ThrowLastCreatedError( "Profile Name", "Instance Profile" );
               }
               if ( AWSInterface.Utilities.TryDeleteInstanceProfile( out message, profilename ) )
               {
                  Common.WriteMessage( $"Profile is deleted. Profile:[{ profilename }]" );
                  Common.WriteMessage( message );
               }
               else
                  Common.WriteMessage( $"Error:{ message}" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }

      }
   }
}
