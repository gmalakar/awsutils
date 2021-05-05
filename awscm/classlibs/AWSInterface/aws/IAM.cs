using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.Runtime;

namespace AWSCM.AWSInterface
{
   internal class IAM
   {
      private IAM() { }

      private AmazonIdentityManagementServiceClient IAMClient()
      {
         if ( Common.TryGetCredentials( out AWSCredentials awsCredentials ) )
            return new AmazonIdentityManagementServiceClient( awsCredentials, Common.DefaultRegionEndpoint );
         else
            return new AmazonIdentityManagementServiceClient( new AmazonIdentityManagementServiceConfig() { RegionEndpoint = Common.DefaultRegionEndpoint } );
      }

      #region get

      internal static bool TryGetRole( string roleName, out Role role )
      {
         return IAMInstance.TryGetRole2( roleName, out role );
      }

      internal static bool TryGetInsProfile( string profileName, out InstanceProfile profile )
      {
         return IAMInstance.TryGetInsProfile2( profileName, out profile );
      }

      #endregion get

      internal static string DescribePolicy( string policyName = null )
      {
         return IAMInstance.DescribePolicy2( policyName );
      }

      internal static string DescribeRoles( string roleName = null )
      {
         return IAMInstance.DescribeRoles2( roleName );
      }
      
      internal static string DescribeInsProfile( string profileName = null )
      {
         return IAMInstance.DescribeInsProfile2( profileName );
      }
      
      internal static string DescribePolicies( string policyName = null )
      {
         return IAMInstance.DescribePolicies2( policyName );
      }
      
      internal static bool TryCreateRole( out string message, string roleName, string assumePolicy, string description = null )
      {
         return IAMInstance.TryCreateIAMRole( out message, roleName, assumePolicy, description );
      }

      internal static bool TryCreatePolicy( out string message, string policyName, string jsonPolicyDocument, string description = null )
      {
         return IAMInstance.TryCreateIAMPolicy( out message, policyName, jsonPolicyDocument, description );
      }
      
      internal static bool TryDeleteRole( out string message, string roleName )
      {
         return IAMInstance.TryDelIAMRole( out message, roleName );
      }
      
      internal static bool TryDeletePolicy( out string message, string policyName )
      {
         return IAMInstance.TryDelIAMPolicy( out message, policyName );
      }
      
      internal static bool TryAddPoliciesToRole( out string message, string roleName, List<string> policyNames )
      {
         return IAMInstance.TryAddPoliciesToRole2( out message, roleName, policyNames );
      }
      
      internal static bool TryCreateInstanceProfile( out string message, string profileName, string roleNameToAdd = null )
      {
         return IAMInstance.TryCreateInsProfile( out message, profileName, roleNameToAdd );
      }

      internal static bool TryDeleteInstanceProfile( out string message, string profileName )
      {
         return IAMInstance.TryDelInsProfile( out message, profileName );
      }
      
      internal static bool TryAddRolesToInstanceProfile( out string message, string profileName, List<string> roleNames )
      {
         return IAMInstance.TryAddRolesToInstanceProfile2( out message, profileName, roleNames );
      }

      #region tryget
      private bool TryGetRole2( string roleName, out Role role )
      {
         var success = false;

         using ( var iamClient = this.IAMClient() )
         {
            role = this.IAMRole( out _, iamClient, roleName );
            success = role != null;
         }
         return success;
      }

      private bool TryGetInsProfile2( string profileName, out InstanceProfile profile )
      {
         var success = false;

         using ( var iamClient = this.IAMClient() )
         {
            profile = this.IAMInsProfile( out _, iamClient, profileName );
            success = profile != null;
         }
         return success;
      }
      #endregion tryget

      private string DescribeRoles2( string rolename = null )
      {
         var allRoles = new List<Role>();

         using ( var iamClient = this.IAMClient() )
         {
            allRoles = this.IAMRoles( iamClient, rolename );
         }
         return Common.ConvertToJSON( allRoles );
      }

      private string DescribePolicy2( string policyName = null )
      {
         var alPolicies = new List<ManagedPolicy>();

         using ( var iamClient = this.IAMClient() )
         {
            alPolicies = this.IAMPolicies( iamClient, policyName );
         }
         return Common.ConvertToJSON( alPolicies );
      }

      private string DescribeInsProfile2( string profileName = null )
      {
         var allRoles = new List<InstanceProfile>();

         using ( var iamClient = this.IAMClient() )
         {
            allRoles = this.IAMInsProfiles( iamClient, profileName );
         }
         return Common.ConvertToJSON( allRoles );
      }
      
      private string DescribePolicies2( string policyName = null )
      {
         var allPolicies = new List<ManagedPolicy>();

         using ( var iamClient = this.IAMClient() )
         {
            allPolicies = this.IAMPolicies( iamClient, policyName );
         }
         return Common.ConvertToJSON( allPolicies );
      }
      
      private bool TryCreateIAMPolicy( out string message, string policyName, string jsonPolicyDocument, string description = null )
      {
         var created = false;
         message = string.Empty;
         try
         {
            using ( var iamCLient = this.IAMClient() )
            {
               this.CreateIAMPolicy( out message, out created, iamCLient, policyName, jsonPolicyDocument, description );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to create Policy { policyName}. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }
      
      private bool TryCreateIAMRole( out string message, string roleName, string assumePolicy, string description = null )
      {
         var created = false;
         message = string.Empty;
         try
         {
            using ( var iamCLient = this.IAMClient() )
            {
               this.CreateIAMRole( out message, out created, iamCLient, roleName, assumePolicy, description );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to create Role { roleName}. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }

      private bool TryDelIAMPolicy( out string message, string policyName )
      {
         var created = false;
         message = string.Empty;
         try
         {
            using ( var iamCLient = this.IAMClient() )
            {
               this.DeleteIAMPolicy( out message, out created, iamCLient, policyName );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to delete Policy { policyName}. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }

      private bool TryDelIAMRole( out string message, string roleName )
      {
         var created = false;
         message = string.Empty;
         try
         {
            using ( var iamCLient = this.IAMClient() )
            {
               this.DeleteIAMRole( out message, out created, iamCLient, roleName );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to delete Role { roleName}. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }


      private bool TryAddPoliciesToRole2( out string message, string roleName, List<string> policyNames )
      {
         message = string.Empty;
         try
         {
            using ( var iamCLient = this.IAMClient() )
            {
               this.AddPoliciesToRole( out message, iamCLient, roleName, policyNames );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to Add Policy to Role: { roleName}. Error:{ Common.ParseException( e ) }";
         }
         return string.IsNullOrEmpty( message );
      }

      private bool TryAddRolesToInstanceProfile2( out string message, string profileName, List<string> roleNames )
      {
         message = string.Empty;
         try
         {
            using ( var iamCLient = this.IAMClient() )
            {
               this.AddRolesToInstanceProfile( out message, iamCLient, profileName, roleNames );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to Add Role to Profile: { profileName}. Error:{ Common.ParseException( e ) }";
         }
         return string.IsNullOrEmpty( message );
      }

      private bool TryCreateInsProfile( out string message, string profileName, string roleName = null )
      {
         var created = false;
         message = string.Empty;
         try
         {
            using ( var iamCLient = this.IAMClient() )
            {
               this.CreateInstanceProfile( out message, out created, iamCLient, profileName, roleName );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to create Instance Profile { profileName}. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }

      private bool TryDelInsProfile( out string message, string profileName )
      {
         var created = false;
         message = string.Empty;
         try
         {
            using ( var iamCLient = this.IAMClient() )
            {
               this.DeleteInstanceProfile( out message, out created, iamCLient, profileName );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to delete Profile { profileName}. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }

      private void CreateIAMRole( out string message, out bool created, AmazonIdentityManagementServiceClient iamClient, string roleName, string assumePolicy, string description = null )
      {
         created = false;
         message = string.Empty;
         if ( string.IsNullOrEmpty( roleName ) )
            message = @"Missing Role Name to create!";
         else
         {
            var role = this.IAMRole(  out _, iamClient, roleName );
            if ( role == null )
            {
               if ( assumePolicy.Equals( TRUST_POLICY, StringComparison.OrdinalIgnoreCase ) )
                  assumePolicy = DefaultTrustPolicy;

               var createRoleRequest = new CreateRoleRequest()
               {
                  RoleName = roleName,
                  AssumeRolePolicyDocument = assumePolicy
               };

               if ( !string.IsNullOrEmpty( description ) )
                  createRoleRequest.Description = description;

               role = iamClient.CreateRoleAsync( createRoleRequest ).Result.Role;
               created = role != null;
               if ( created )
               {
                  //save to cache
                  CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_ACCESSED_ROLE_NAME_KEY, role.RoleName );
               }
            }
            else
            {
               message = $"Rome Name: [{ roleName }] already exists.";
               //save to cache
               CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_ACCESSED_ROLE_NAME_KEY, role.RoleName );
            }
         }
      }

      private void CreateIAMPolicy( out string message, out bool created, AmazonIdentityManagementServiceClient iamClient, string policyName, string jsonPolicyDocument, string description = null )
      {
         created = false;
         message = string.Empty;
         if ( string.IsNullOrEmpty( policyName ) )
            message = @"Missing Policy Name to create!";
         else
         {
            var policy = this.IAMPolicy(  iamClient, policyName );
            if ( policy == null )
            {
               var createRoleRequest = new CreatePolicyRequest()
               {
                  PolicyName = policyName,
                  PolicyDocument = jsonPolicyDocument
               };

               if ( !string.IsNullOrEmpty( description ) )
                  createRoleRequest.Description = description;

               policy = iamClient.CreatePolicyAsync( createRoleRequest ).Result.Policy;
               created = policy != null;
               if ( created )
               {
                  //save to cache
                  CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_POLICY_NAME_KEY, policy.PolicyName );
               }
            }
            else
            {
               message = $"Policy Name: [{ policyName }] already exists.";
               //save to LAST_CREATED_POLICY_NAME_KEY
               CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_ACCESSED_ROLE_NAME_KEY, policy.PolicyName );
            }
         }
      }


      private void DeleteIAMPolicy( out string message, out bool deleted, AmazonIdentityManagementServiceClient iamClient, string policyName )
      {
         deleted = false;
         message = string.Empty;
         if ( string.IsNullOrEmpty( policyName ) )
            message = @"Missing Policy Name to delete!";
         else
         {
            var policy = this.IAMPolicy( iamClient, policyName );
            if ( policy == null )
               message = $"IAM Policy Name: [{policyName}] does not exists.";
            else
            {

               //detach all policies.
               //DetachPoliciesFromRole( out message, iamClient, policyName );

               var request = new DeletePolicyRequest()
               {
                  PolicyArn = policy.Arn
               };

               var response = iamClient.DeletePolicyAsync( request ).Result;
               deleted = response != null && response.HttpStatusCode == System.Net.HttpStatusCode.OK;
               if ( deleted )
               {
                  //remove to cache
                  CommonShared.CacheManager.RemoveFromCache( AWSCacheManager.Constants.LAST_CREATED_ACCESSED_ROLE_NAME_KEY );
               }
            }
         }
      }


      private void DeleteIAMRole( out string message, out bool deleted, AmazonIdentityManagementServiceClient iamClient, string roleName )
      {
         deleted = false;
         message = string.Empty;
         if ( string.IsNullOrEmpty( roleName ) )
            message = @"Missing Role Name to delete!";
         else
         {
            //detach all policies.
            DetachPoliciesFromRole( out message, iamClient, roleName );

            var request = new DeleteRoleRequest()
            {
               RoleName = roleName
            };

            var response = iamClient.DeleteRoleAsync( request ).Result;
            deleted = response != null && response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            if ( deleted )
            {
               //remove to cache
               CommonShared.CacheManager.RemoveFromCache( AWSCacheManager.Constants.LAST_CREATED_ACCESSED_ROLE_NAME_KEY );
            }
         }
      }

      private void AddPoliciesToRole( out string message, AmazonIdentityManagementServiceClient iamClient, string roleName, List<string> policyNames )
      {
         message = string.Empty;
         var policies = this.IAMPolicies( iamClient, policyNames );
         foreach ( var policy in policies )
         {
            //attach policy
            var response = iamClient.AttachRolePolicyAsync( new AttachRolePolicyRequest() { PolicyArn = policy.Arn, RoleName = roleName } ).Result;
            var ok = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            if ( !ok )
               message = $"{message}Could not add policy: [{ policy.PolicyName}] to role: [{ roleName }]{ Environment.NewLine}";
         }
         var pList = policies.Select( p => p.PolicyName ).ToList();
         var notIn = policyNames.Except( pList );
         if ( notIn.Count() > 0 )
            message = $"Could not find policies : [{ string.Join( ',', policyNames ) }] to attach { Environment.NewLine}";
      }


      private void DetachPoliciesFromRole( out string message, AmazonIdentityManagementServiceClient iamClient, string roleName )
      {
         message = string.Empty;
         var role = this.IAMRole(  out _, iamClient, roleName );
         if ( role == null )
            message = $"IAM Role Name: [{roleName}] does not exists.";
         else
         {
            foreach ( var policy in this.IAMPolicies( iamClient, string.Empty ) )
            {
               //detach policy
               try
               {
                  var response = iamClient.DetachRolePolicyAsync( new DetachRolePolicyRequest() { PolicyArn = policy.Arn, RoleName = roleName } ).Result;
                  var ok = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
                  if ( ok )
                     message = $"{message}Detached policy: [{ policy.PolicyName}] from role: [{ roleName }]{ Environment.NewLine}";
               }
               catch //skip if the policy is not attached
               {

               }
            }
         }
      }

      private void CreateInstanceProfile( out string message, out bool created, AmazonIdentityManagementServiceClient iamClient, string profileName, string roleName = null )
      {
         created = false;
         message = string.Empty;
         if ( string.IsNullOrEmpty( profileName ) )
            message = @"Missing Profile Name to create!";
         else
         {
            var profile = this.IAMInsProfile( out _, iamClient, profileName );
            if ( profile == null )
            {
               var request = new CreateInstanceProfileRequest()
               {
                  InstanceProfileName = profileName
               };

               profile = iamClient.CreateInstanceProfileAsync( request ).Result.InstanceProfile;
               created = profile != null;
               if ( created )
               {
                  //add roles 
                  try
                  {
                     if ( !string.IsNullOrEmpty( roleName ) )
                        this.AddRolesToInstanceProfile( out message, iamClient, profile.InstanceProfileName, new List<string>() { roleName } );
                  }
                  catch ( Exception e )
                  {
                     message = $"Unable to Add Role:[{roleName}] to Profile: { profileName}. Error:{ Common.ParseException( e ) }";
                  }
                  //save to cache
                  CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_ACCESSED_PROFILE_KEY, profile.InstanceProfileName );
               }
            }
            else
            {
               message = $"Instance Profile: [{ profile }] already exists.";
               //save to cache
               CommonShared.CacheManager.SaveToCache( AWSCacheManager.Constants.LAST_CREATED_ACCESSED_PROFILE_KEY, profile.InstanceProfileName );

            }
         }
      }

      private void DeleteInstanceProfile( out string message, out bool deleted, AmazonIdentityManagementServiceClient iamClient, string profileName )
      {
         deleted = false;
         message = string.Empty;
         if ( string.IsNullOrEmpty( profileName ) )
            message = @"Missing Profile Name to delete!";
         else
         {
            DeleteRolesFromInstanceProfile( out message, iamClient, profileName );
            var request = new DeleteInstanceProfileRequest()
            {
               InstanceProfileName = profileName
            };

            var response = iamClient.DeleteInstanceProfileAsync( request ).Result;
            deleted = response != null && response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            if ( deleted )
            {
               //remove to cache
               CommonShared.CacheManager.RemoveFromCache( AWSCacheManager.Constants.LAST_CREATED_ACCESSED_PROFILE_KEY );
            }
         }
      }

      private void AddRolesToInstanceProfile( out string message, AmazonIdentityManagementServiceClient iamClient, string profileName, List<string> roleNames )
      {
         message = string.Empty;
         foreach ( var role in this.IAMRoles( iamClient, roleNames ) )
         {
            //attach role

            var response = iamClient.AddRoleToInstanceProfileAsync( new AddRoleToInstanceProfileRequest() { InstanceProfileName = profileName, RoleName = role.RoleName } ).Result;
            var ok = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            if ( !ok )
               message = $"{message}Could not add role: [{ role.RoleName}] to instance profile: [{ profileName }]{ Environment.NewLine}";
         }
      }

      private void DeleteRolesFromInstanceProfile( out string message, AmazonIdentityManagementServiceClient iamClient, string profileName )
      {
         message = string.Empty;
         var roles = this.IAMInsProfile( out message, iamClient, profileName )?.Roles.ToArray();
         if ( roles != null && roles.Count() > 0 )
            foreach ( var role in roles )
            {
               //detach role
               try
               {
                  var response = iamClient.RemoveRoleFromInstanceProfileAsync( new RemoveRoleFromInstanceProfileRequest() { InstanceProfileName = profileName, RoleName = role.RoleName } ).Result;
                  var ok = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
                  if ( ok )
                     message = $"{message}Detached role: [{ role.RoleName}] instance profile: [{ profileName }]{ Environment.NewLine}";
               }
               catch //skip if the role is not attached
               {

               }
            }
      }

      private List<ManagedPolicy> IAMPolicies( AmazonIdentityManagementServiceClient iamClient, string policyName = null )
      {
         var policies = iamClient.ListPoliciesAsync( new ListPoliciesRequest() { MaxItems = 1000 } ).Result.Policies;
         if ( !string.IsNullOrEmpty( policyName ) )
            policies = policies?.Where( r => r.PolicyName.Equals( policyName, StringComparison.OrdinalIgnoreCase ) ).ToList();
         return policies;
      }

      private List<ManagedPolicy> IAMPolicies( AmazonIdentityManagementServiceClient iamClient, List<string> policyNames = null )
      {
         var policies = iamClient.ListPoliciesAsync( new ListPoliciesRequest() { MaxItems = 1000 } ).Result.Policies;
         if ( policyNames != null && policyNames.Count > 0 )
            policies = policies?.Where( r => policyNames.Contains( r.PolicyName, StringComparer.OrdinalIgnoreCase ) ).ToList();
         return policies;
      }

      private ManagedPolicy IAMPolicy( AmazonIdentityManagementServiceClient iamClient, string policyName )
      {
        return IAMPolicies( iamClient, policyName )?.FirstOrDefault( r => r.PolicyName.Equals( policyName, StringComparison.OrdinalIgnoreCase ) );
      }

      private Role IAMRole( out string message, AmazonIdentityManagementServiceClient iamClient, string roleName )
      {
         Role role = null;
         message= string.Empty;
         try
         {
            var response = iamClient.GetRoleAsync( new GetRoleRequest() { RoleName = roleName } ).Result;
            var ok = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            if ( ok )
               role = response.Role;
         }
         catch ( Exception ex )
         {
            message = $"Error:{ Common.ParseException( ex )}.";
         }
         return role;
      }

      private List<Role> IAMRoles( AmazonIdentityManagementServiceClient iamClient, List<string> roleNames = null )
      {
         if ( roleNames != null && roleNames.Count == 1 )
            return IAMRoles( iamClient, roleNames[0] );
         else
         {
            var roles = iamClient.ListRolesAsync().Result.Roles;
            if ( roles != null && roles.Count> 0  && roleNames != null && roleNames.Count > 0 )
               roles = roles?.Where( r => roleNames.Contains( r.RoleName, StringComparer.OrdinalIgnoreCase ) ).ToList();
            return roles;
         }
      }

      private List<Role> IAMRoles( AmazonIdentityManagementServiceClient iamClient, string roleName = null )
      {
         List<Role> roles = null;

         if ( !string.IsNullOrEmpty( roleName ) )
         {
            var role = IAMRole(  out _, iamClient, roleName );
            if ( role != null )
               roles = new List<Role>() { role };
         }
         else
            roles = iamClient.ListRolesAsync( new ListRolesRequest() ).Result.Roles;
         if ( roles == null )
            roles = new List<Role>();
         return roles;
      }

      private InstanceProfile IAMInsProfile( out string message, AmazonIdentityManagementServiceClient iamClient, string profileName )
      {
         InstanceProfile profile = null;
         message = string.Empty;
         try
         {
            var response = iamClient.GetInstanceProfileAsync( new GetInstanceProfileRequest() { InstanceProfileName = profileName } ).Result;
            var ok = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            if ( ok )
               profile = response.InstanceProfile;
         }
         catch ( Exception ex )
         {
            message = $"Error:{ Common.ParseException( ex )}.";
         }
         return profile;
      }

      private List<InstanceProfile> IAMInsProfiles( AmazonIdentityManagementServiceClient iamClient, string profileName = null )
      {
         List<InstanceProfile> profiles = null;

         if ( !string.IsNullOrEmpty( profileName ) )
         {
            var role = IAMInsProfile( out _, iamClient, profileName );
            if ( role != null )
               profiles = new List<InstanceProfile>() { role };
         }
         else
            profiles = iamClient.ListInstanceProfilesAsync( new ListInstanceProfilesRequest() ).Result.InstanceProfiles;
         if ( profiles == null )
            profiles = new List<InstanceProfile>();
         return profiles;
      }
            
      private static readonly IAM IAMInstance = new IAM();
      private const string TRUST_POLICY = @"trustpolicy";
      private const string DefaultTrustPolicy =   "{\"Version\": \"2012-10-17\", \"Statement\": [{\"Effect\": \"Allow\",\"Principal\":{\"Service\":[\"ec2.amazonaws.com\"]}, \"Action\": \"sts:AssumeRole\" }]}";
   }
}
