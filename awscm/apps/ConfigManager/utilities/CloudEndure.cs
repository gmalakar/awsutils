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

namespace AWSCM.AWSConfigManager.Utilities
{
   public class CloudEndure
   {
      private Arguments parameters;
      public CloudEndure( Arguments args )
      {
         parameters = args;
         switch ( Common.GetService( parameters ) )
         {
            case @"login":
               this.DoLogin();
               break;
            case @"logout":
               this.DoLogout();
               break;
            case @"desc":
            case @"describe":
               this.DoDescribe();
               break;
            case @"projects":
               this.DoProjects();
               break;
            case @"repl":
            case @"replication":
               this.DoReplication();
               break;
            case @"clouds":
               this.DoClouds();
               break;
            case @"ins-token":
            case @"instoken":
               this.GetInstallationToken();
               break;
            default:
               Common.ThrowError( "Invalid service!" );
               break;
         }
      }

      private void DoLogin()
      {
         var response = string.Empty;
         if ( AWSCM.CEInterface.Utilities.CanTryLogin )
            AWSCM.CEInterface.Utilities.TryLogin( out response );
         else
            if ( !CEInterface.Utilities.TryLogin( parameters.GetArgumentValue( @"username" ), parameters.GetArgumentValue( @"password" ), out response ) )
            Console.WriteLine( response );
         else
            Console.WriteLine( @"Successfully logged in and created CloudeEndure session!" );
      }

      private void DoLogout()
      {
         AWSCM.CEInterface.Utilities.TryLogout( out string response );

         Common.WriteMessage( response );
      }

      private void DoDescribe()
      {
         var response = string.Empty;
         var name = string.Empty;
         switch ( Common.GetType( this.parameters ) )
         {
            case @"project":
               name = parameters.GetArgumentValue( @"projectname", false );
               if ( string.IsNullOrEmpty( name ) )
                  AWSCM.CEInterface.Utilities.TryGetAllProjects( out response );
               else
                  AWSCM.CEInterface.Utilities.TryGetProject( name, out response );
               Common.WriteMessage( response );
               break;
            case @"cloud":
               name = parameters.GetArgumentValue( @"cloudname", false );
               if ( string.IsNullOrEmpty( name ) )
                  AWSCM.CEInterface.Utilities.TryGetAllClouds( out response );
               else
                  AWSCM.CEInterface.Utilities.TryGetCloud( name, out response );
               Common.WriteMessage( response );
               break;
            case @"creds":
            case @"credential":
               name = parameters.GetArgumentValue( @"cloudname", false );
               var keyname = parameters.GetArgumentValue( @"publickey", false );
               if ( string.IsNullOrEmpty( name ) && string.IsNullOrEmpty( keyname ) )
                  AWSCM.CEInterface.Utilities.TryGetAllCreds( out response );
               else
                  AWSCM.CEInterface.Utilities.TryGetCreds( name, keyname, out response );
               Common.WriteMessage( response );
               break;
            case @"region":
               name = parameters.GetArgumentValue( @"cloudname", false );
               var region = parameters.GetArgumentValue( @"region", false );

               AWSCM.CEInterface.Utilities.TryGetRegions( name, region, out response );
               //else
               //AWSCM.CEInterface.Utilities.TryGetCreds( name, keyname, out response );
               Common.WriteMessage( response );
               break;
            case @"replconfig":
            case @"replication":
               name = parameters.GetArgumentValue( @"projectname" );
               AWSCM.CEInterface.Utilities.TryGetReplicationConfigs( name, out response );
               //else
               //AWSCM.CEInterface.Utilities.TryGetCreds( name, keyname, out response );
               Common.WriteMessage( response );
               break;

            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }

      }

      private void DoProjects()
      {
         var name = parameters.GetArgumentValue( @"projectname", false );
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"get":
               var response = string.Empty;
               if ( string.IsNullOrEmpty( name ) )
                  AWSCM.CEInterface.Utilities.TryGetAllProjects( out response );
               else
                  AWSCM.CEInterface.Utilities.TryGetProject( name, out response );
               Common.WriteMessage( response );
               break;
            case @"set":
            case @"create":
               AWSCM.CEInterface.Utilities.TryCreateProject( name, out response );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }

      }

      private void DoReplication()
      {
         var name = parameters.GetArgumentValue( @"projectname", false );
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"get":
               var response = string.Empty;
               if ( string.IsNullOrEmpty( name ) )
                  AWSCM.CEInterface.Utilities.TryGetAllProjects( out response );
               else
                  AWSCM.CEInterface.Utilities.TryGetProject( name, out response );
               Common.WriteMessage( response );
               break;
            case @"set":
            case @"create":
               AWSCM.CEInterface.Utilities.TryCreateProject( name, out response );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }

      }

      private void DoClouds()
      {
         var name = parameters.GetArgumentValue( @"cloudname", false );
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"get":
               var response = string.Empty;
               if ( string.IsNullOrEmpty( name ) )
                  AWSCM.CEInterface.Utilities.TryGetAllClouds( out response );
               else
                  AWSCM.CEInterface.Utilities.TryGetProject( name, out response );
               Common.WriteMessage( response );
               break;
            //case @"set":
            //case @"create":
            //   AWSCM.CEInterface.Utilities.TryCreateProject( name, out response );
            //   break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }

      }
      
      private void GetInstallationToken()
      {
         AWSCM.CEInterface.Utilities.TryGetInstalationToken( parameters.GetArgumentValue( @"projectname" ), out string response );
         Common.WriteMessage( response );
      }
   }
}
