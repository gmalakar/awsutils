#region history
//*****************************************************************************
// IAM.cs:
//
// History:
// 08/06/20 - Goutam Malakar 
//*****************************************************************************
#endregion history
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace AWSCM.AWSConfigManager.Utilities
{
   public class SSMInstance
   {
      private Arguments parameters;
      public SSMInstance( Arguments args )
      {
         parameters = args;
         switch ( Common.GetService( parameters ) )
         {
            case @"associate":
            case @"assoc":
               this.DoAssoc();
               break;
            case @"doc":
            case @"document":
               this.DoDoc();
               break;
            case @"command":
            case @"cmd":
               this.DoCommand();
               break;
            case @"auto":
            case @"automation":
               this.DoAuto();
               break;
            default:
               Common.ThrowError( "Invalid service!" );
               break;
         }
      }

      private void DoDoc()
      {
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"set":
            case @"create":
               var name = parameters.GetArgumentValue( @"name" );

               if ( AWSInterface.Utilities.TryCreateCommandDocument( out string message, name ) )
               {
                  Common.WriteMessage( $"SSM document is created for name: [{ name }]" );
                  Common.WriteMessage( message );
               }
               else
                  Common.WriteMessage( $"Cannot create SSM document for :{ name }, Error:{ message}" );
               break;
            case @"del":
            case @"delete":
               Common.ThrowError( "Not Implemented" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }

      }

      private void DoAssoc()
      {
         var instanceid = string.Empty;
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"set":
            case @"create":
               instanceid = parameters.GetArgumentValue( @"instanceids" );
               if ( CommonShared.Utilities.IsUseLast( instanceid ) )
               {
                  instanceid = AWSInterface.Utilities.LastLaunchedEC2Instance;
                  if ( string.IsNullOrEmpty( instanceid ) )
                     Common.ThrowLastCreatedError( "Instance ID", "Instance" );
               }

               var assocname = parameters.GetArgumentValue( @"associationname" );
               var docname = parameters.GetArgumentValue( @"documentname" );
               var runParams = parameters.GetArgumentValue( @"parameters", false );
               if ( AWSInterface.Utilities.TryCreateAssociation( out string message, assocname, Common.EC2_ASSOC_TARGET_KEY, CommonShared.StringUtils.ValueAsList( instanceid ), docname, string.IsNullOrEmpty( runParams ) ? null : Common.GetRunParameters( runParams ) ) )
               {
                  Common.WriteMessage( $"SSM association is created for instance id :[{ instanceid }]" );
                  Common.WriteMessage( message );
               }
               else
                  Common.WriteMessage( $"Cannot create SSM association for :{ instanceid }, Error:{ message}" );
               break;
            case @"del":
            case @"delete":
               Common.ThrowError( "Not Implemented" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }

      }

      private void DoCommand()
      {
         var commandid = string.Empty;
         var message = string.Empty;
         var outputs = string.Empty;
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"run":
               var documentName = parameters.GetArgumentValue( @"documentname" );
               var instanceid = parameters.GetArgumentValue( @"instanceids" );
               if ( CommonShared.Utilities.IsUseLast( instanceid ) )
               {
                  instanceid = AWSInterface.Utilities.LastLaunchedEC2Instance;
                  if ( string.IsNullOrEmpty( instanceid ) )
                     Common.ThrowLastCreatedError( "Instance ID", "Instance" );
               }
               var instanceids = instanceid.Split('|').ToList();
               var runParams = Common.GetRunParameters(parameters.GetArgumentValue( @"parameters" ));
               if ( AWSInterface.Utilities.TryRunCommands( 
                  out message, 
                  out commandid, 
                  out outputs, 
                  documentName, 
                  instanceids, 
                  runParams, 
                  parameters.GetArgumentValue( @"comment", false ), 
                  parameters.GetArgumentValue( @"outputs3bucketname", false ), 
                  parameters.GetArgumentValue( @"outputs3prefix", false ), 
                  parameters.GetArgumentValue( @"outputs3region", false ), 
                  parameters.GetArgumentValueAsBoolean( @"commandoutput", false ) ) )
               {
                  Common.WriteMessage( $"Running command id:[{ commandid }]" );
                  if ( !string.IsNullOrEmpty( outputs ) )
                     Common.WriteMessage( outputs );
               }
               else
                  Common.WriteMessage( $"Cannot run commands for Document Name :{ documentName }, Error:{ message}" );
               break;
            case @"output":
               commandid = parameters.GetArgumentValue( @"commandid" );
               if ( AWSInterface.Utilities.TryGetCommandOutputs( out message, out outputs, commandid, parameters.GetArgumentValue( @"instanceid", false ), parameters.GetArgumentValueAsBoolean( @"recursive", false ) ) )
               {
                  Common.WriteMessage( $"Output of command id:[{ commandid }]" );
                  Common.WriteMessage( outputs );
               }
               else
                  Common.WriteMessage( $"Cannot get output for Command ID :{ commandid }, Error:{ message}" );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }

      }

      private void DoAuto()
      {
         var executionId = string.Empty;
         var message = string.Empty;
         var outputs = string.Empty;
         switch ( Common.GetAction( this.parameters ) )
         {
            case @"execution":
            case @"exe":
               var documentName = parameters.GetArgumentValue( @"documentname" );
               var runParams = Common.GetRunParameters(parameters.GetArgumentValue( @"parameters", false ));
               var targetKey = parameters.GetArgumentValue( @"targetkey", false );
               var targetValues = parameters.GetArgumentValue( @"targetvalues", false );
               if ( AWSInterface.Utilities.TryStartAutomationExecution( out message, out executionId, documentName, runParams, targetKey,
                  string.IsNullOrEmpty( targetValues ) ? null : CommonShared.StringUtils.ValueAsList( targetValues ) ) )
               {
                  Common.WriteMessage( $"Aumation Execution ID id:[{ executionId }]" );
               }
               else
                  Common.WriteMessage( $"Cannot start Automation for :{ documentName }, Error:{ message}" );
               break;
            case @"desc":
            case @"description":
               Common.WriteMessage( AWSInterface.Utilities.DescribeAutomationExecution( parameters.GetArgumentValue( @"executionId", false ), parameters.GetArgumentValue( @"documentprefix", false ) ) );
               break;
            default:
               Common.ThrowError( "Invalid action!" );
               break;
         }

      }
   }
}
