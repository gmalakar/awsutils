using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Amazon.EC2;
using Amazon.Runtime;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System.Threading;

namespace AWSCM.AWSInterface
{
   internal class SSM
   {
      private SSM() { }

      private AmazonSimpleSystemsManagementClient SSMClient()
      {
         if ( Common.TryGetCredentials( out AWSCredentials awsCredentials ) )
            return new AmazonSimpleSystemsManagementClient( awsCredentials, Common.DefaultRegionEndpoint );
         else
            return new AmazonSimpleSystemsManagementClient( new AmazonSimpleSystemsManagementConfig() { RegionEndpoint = Common.DefaultRegionEndpoint } );
      }

      internal static string DescribeInstanceInfo()
      {
         return SSMInstance.DescribeInstanceInfo2();
      }

      internal static string DescribeAutomationExecution( string executionIds = null, string documentNamePrefix = null )
      {
         return SSMInstance.DescribeAutomationExecution2( executionIds, documentNamePrefix );
      }

      internal static bool TryCreateAssociation( out string message, string associationName, string targetKey, List<string> targetValues, string documentName, Dictionary<string, List<string>> parameters = null )
      {
         return SSMInstance.TryCreateAssociation2( out message, associationName, targetKey, targetValues, documentName, parameters );
      }

      internal static bool TryCreateCommandDocument( out string message, string name )
      {
         return SSMInstance.TryCreateCommandDocument2( out message, name );
      }

      internal static bool TryRunCommands(
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
         return SSMInstance.TryRunCommands2( out message, out commandId, out output, documentName, instanceIDs, parameters, comment, outputS3BucketName, outputS3KeyPrefix, outputS3Region, commandOutput );
      }

      internal static bool TryStartAutomationExecution( out string message, out string executionId, string documentName, Dictionary<string, List<string>> parameters = null, string targetKey = null, List<string> targetValues = null )
      {
         return SSMInstance.TryStartAutomationExecution2( out message, out executionId, documentName, parameters, targetKey, targetValues );
      }

      internal static bool TryGetCommandOutputs( out string message, out string output, string commandId, string instanceId = null, bool reursive = false )
      {
         return SSMInstance.TryGetCommandOutputs2( out message, out output, commandId, instanceId, reursive );
      }


      private string DescribeInstanceInfo2()
      {
         var insInfo = new List<InstanceInformation>();

         using ( var ssmClient = this.SSMClient() )
         {
            insInfo = this.DescribeInstanceInformation( ssmClient );
         }
         return Common.ConvertToJSON( insInfo );
      }

      private string DescribeAutomationExecution2( string executionIds = null, string documentNamePrefix = null )
      {
         var exeutionInfo = new List<AutomationExecutionMetadata>();

         using ( var ssmClient = this.SSMClient() )
         {
            exeutionInfo = this.DescribeAutomationExecution( ssmClient, executionIds, documentNamePrefix );
         }
         return Common.ConvertToJSON( exeutionInfo );
      }

      private bool TryCreateAssociation2( out string message, string associationName, string targetKey, List<string> targetValues, string documentName, Dictionary<string, List<string>> parameters = null )
      {
         var created = false;
         message = string.Empty;
         try
         {
            using ( var ssmClient = this.SSMClient() )
            {
               this.CreateAssociation( out message, out created, ssmClient, associationName, targetKey, targetValues, documentName, parameters );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to create association for target: [{targetKey }] [{ string.Join( ',', targetValues ) }]. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }

      private bool TryCreateCommandDocument2( out string message, string name )
      {
         var created = false;
         message = string.Empty;
         try
         {
            using ( var ssmClient = this.SSMClient() )
            {
               this.CreateCommandDocument( out message, out created, ssmClient, name );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to create command document: [{ name}]. Error:{ Common.ParseException( e ) }";
         }
         return created;
      }
      
      private bool TryRunCommands2( 
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
         message = string.Empty;
         commandId = string.Empty;
         output = string.Empty;
         try
         {
            using ( var ssmClient = this.SSMClient() )
            {
               this.RunCommands( out message, out commandId, out output, ssmClient, documentName, instanceIDs, parameters, comment, outputS3BucketName, outputS3KeyPrefix, outputS3Region, commandOutput );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to run command: [{ documentName}]. Error:{ Common.ParseException( e ) }";
         }
         return !string.IsNullOrEmpty( commandId );
      }
      
      private bool TryStartAutomationExecution2( out string message, out string executionId, string documentName, Dictionary<string, List<string>> parameters = null, string targetKey = null, List<string> targetValues = null )
      {
         message = string.Empty;
         executionId = string.Empty;
         var started = false;
         try
         {
            using ( var ssmClient = this.SSMClient() )
            {
               this.StartAutomationExecution( out started, out executionId, ssmClient, documentName, parameters, targetKey, targetValues );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to run command: [{ documentName}]. Error:{ Common.ParseException( e ) }";
         }
         return started;
      }

      private bool TryGetCommandOutputs2( out string message, out string output, string commandId, string instanceId = null, bool reursive = false )
      {
         message = string.Empty;
         output = string.Empty;
         try
         {
            using ( var ssmClient = this.SSMClient() )
            {
               this.CommandOutputs( out message, out output, commandId, ssmClient, instanceId, reursive );
            }
         }
         catch ( Exception e )
         {
            message = $"Unable to get command output for Command ID { commandId}. Error:{ Common.ParseException( e ) }";
         }
         return !string.IsNullOrEmpty( output );
      }

      private void CreateAssociation( out string message, out bool created, AmazonSimpleSystemsManagementClient smsClient, string associationName, string targetKey, List<string> targetValues, string documentName, Dictionary<string, List<string>> parameters = null )
      {
         created = false;
         message = string.Empty;
         var request = new CreateAssociationRequest();
         request.AssociationName = associationName;
         request.Targets = new List<Target>() { new Target() { Key = targetKey, Values = targetValues } };
         request.Name = documentName;
         if ( parameters != null && parameters.Count > 0 )
            request.Parameters = parameters;

         var association = smsClient.CreateAssociationAsync( request ).Result;
         created = association.HttpStatusCode == System.Net.HttpStatusCode.OK;
      }

      private void CreateCommandDocument( out string message, out bool created, AmazonSimpleSystemsManagementClient smsClient, string name)
      {
         created = false;
         message = string.Empty;
         var request = new CreateDocumentRequest();
         request.Name = name;
         request.DocumentType = DocumentType.Command;
         var doc = smsClient.CreateDocumentAsync( request ).Result;
         created = doc.HttpStatusCode == System.Net.HttpStatusCode.OK;
      }
      
      private void RunCommands( 
         out string message, 
         out string commandId, 
         out string output, 
         AmazonSimpleSystemsManagementClient smsClient, 
         string documentName, 
         List<string> instanceIDs,
         Dictionary<string, List<string>> parameters,
         string comment = null,
         string outputS3BucketName = null,
         string outputS3KeyPrefix = null,
         string outputS3Region = null,
         bool commandOutput = false )
      {
         message = string.Empty;
         commandId = string.Empty;
         output = string.Empty;
         var request = new SendCommandRequest() { DocumentName = documentName, InstanceIds = instanceIDs, Parameters = parameters };
         if ( !string.IsNullOrEmpty( comment ) )
            request.Comment = comment;

         if ( !string.IsNullOrEmpty( outputS3BucketName ) )
            request.OutputS3BucketName = outputS3BucketName;

         if ( !string.IsNullOrEmpty( outputS3KeyPrefix ) )
            request.OutputS3KeyPrefix = outputS3KeyPrefix;

         if ( !string.IsNullOrEmpty( outputS3Region ) )
            request.OutputS3Region = outputS3Region;

         var response = smsClient.SendCommandAsync( request ).Result;
         var ok = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
         if ( ok )
         {
            commandId = response.Command.CommandId;
            if ( commandOutput )
               CommandOutputs( out message, out output, commandId, smsClient, null, true );
         }
      }

      private void StartAutomationExecution( out bool started, out string executionId, AmazonSimpleSystemsManagementClient smsClient, string documentName, Dictionary<string, List<string>> parameters = null, string targetKey = null, List<string> targetValues = null )
      {
         executionId = string.Empty;
         var request = new StartAutomationExecutionRequest() { DocumentName = documentName };
         if ( !string.IsNullOrEmpty( targetKey ) && targetValues != null )
            request.Targets = new List<Target>() { new Target() { Key = targetKey, Values = targetValues } };
         if ( parameters != null && parameters.Count > 0 )
            request.Parameters = parameters;

         var response = smsClient.StartAutomationExecutionAsync( request ).Result;
         started = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
         if ( started )
            executionId = response.AutomationExecutionId;
      }
      private List<AutomationExecutionMetadata> DescribeAutomationExecution( AmazonSimpleSystemsManagementClient smsClient, string executionIds = null, string documentNamePrefix = null )
      {
         var request = new DescribeAutomationExecutionsRequest() {};
         if ( !string.IsNullOrEmpty( executionIds ) )
            request.Filters.Add( new AutomationExecutionFilter() { Key = AutomationExecutionFilterKey.ExecutionId, Values = executionIds.Split( '|' ).ToList() } );
         if ( !string.IsNullOrEmpty( documentNamePrefix ) )
            request.Filters.Add( new AutomationExecutionFilter() { Key = AutomationExecutionFilterKey.DocumentNamePrefix, Values = documentNamePrefix.Split( '|' ).ToList() } );

         return smsClient.DescribeAutomationExecutionsAsync( request ).Result.AutomationExecutionMetadataList;
      }

      private List<InstanceInformation> DescribeInstanceInformation( AmazonSimpleSystemsManagementClient ssmClient )
      {
         return ssmClient.DescribeInstanceInformationAsync().Result.InstanceInformationList;
      }

      private void CommandOutputs( out string message, out string output, string commandId, AmazonSimpleSystemsManagementClient smsClient, string instanceID = null, bool recursive = false )
      {
         message = string.Empty;
         output = string.Empty;
         var cont = false;
         var request = new ListCommandInvocationsRequest() { CommandId = commandId };
         CommandInvocation command = null;
         if ( !string.IsNullOrEmpty( instanceID ) )
            request.InstanceId = instanceID;
         do
         {
            cont = false;
            var response = smsClient.ListCommandInvocationsAsync( request ).Result;
            var ok = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            if ( ok )
            {
               command = response.CommandInvocations.FirstOrDefault( c => c.CommandId == commandId );
               cont = command.Status == CommandInvocationStatus.InProgress;
            }
            if ( cont )
               Thread.Sleep( 2000 ); //sleep for 2 seconds
            else
               break;

         } while ( cont && recursive );
         if ( command != null )
         {
            var alloutput = command.CommandPlugins?.Select( p=> p.Output ).ToList();
            if ( alloutput != null )
               output = string.Join( Environment.NewLine, alloutput );
         }
      }

      private static readonly SSM SSMInstance = new SSM();

   }
}
