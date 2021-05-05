using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Amazon.EC2;
using Amazon.EC2.Model;
using AWSCM.AWSOperations.Common;
using AWSCM.AWSOperations.Models;
using AWSCM.Common.Extensions;
using AWSCM.Common.Others;

namespace AWSCM.AWSOperations.EC2
{
   public class EC2Operations
   {

      public async Task<DescribeEC2> GetInstancesByTags( string filters )
      {
         var desc = new DescribeEC2();

         try
         {
            // Establish an AmazonEC2Client and use the DescribeInstancesRequest/DescribeInstancesResponse objects to find instances by tag
            using ( AmazonEC2Client ec2Client = new AmazonEC2Client() )
            {
               var describeRequest = new DescribeInstancesRequest
               {
                  Filters = Filters.ConvertToFilter( filters )
               };

               var describeResponse = await ec2Client.DescribeInstancesAsync(describeRequest);

               // The response stores instance details in a Reservation wrapper, so drill down as required to obtain the instance ids
               if ( describeResponse?.Reservations?.Count > 0 )
               {
                  describeResponse.Reservations.ForEach( reservation =>
                   {
                      if ( reservation?.Instances?.Count > 0 )
                      {
                         reservation.Instances.ForEach( instance =>
                               {
                                  // Add discovered instance ids to the describeOperation helper object
                                  desc.InstanceIDs.Add( instance.InstanceId );
                               } );
                      }
                   } );
               }

               // Set the OperationReport property for logging purposes (to be handled by the caller) - details how this operation went
               desc.Respone = describeResponse != null
                   ? describeResponse.HttpStatusCode.StatusMessage()
                   : Constants.NULL_RESPONSE_MESSAGE;
            }
         }
         catch ( Exception ex )
         {
            // Get a 'friendly', formatted version of the exception on error (storing it against the OperationReport property on the returned object)
            desc.Respone = ex.ParseException();
         }

         return desc;
      }

      /// <summary>
      /// Method that returns a custom ManipulateEC2Operation object that holds details
      /// on the attempted operation to 'start' EC2 instances.
      /// </summary>
      /// <param name="instanceIds">The list of EC2 instance ids to start.</param>
      /// <returns>A Task containing a custom ManipulateEC2Operation object (containing details on the start operation).</returns>
      public async Task<PerformEC2Operation> StartStopEC2InstancesByInstanceIds( List<string> instanceIds, string operation )
      {
         var performOperation = new PerformEC2Operation();

         try
         {
            // Establish an AmazonEC2Client and use the StartInstancesRequest/StartInstancesResponse objects to attempt to start the instances passed in (by id)
            using ( AmazonEC2Client ec2Client = new AmazonEC2Client() )
            {

               switch ( operation.ToLower() )
               {
                  case "start": //start instances
                     var startRequest = new StartInstancesRequest(instanceIds);

                     var startResponse = await ec2Client.StartInstancesAsync(startRequest);
                     performOperation.Respone = startResponse != null
                         ? startResponse.HttpStatusCode.StatusMessage()
                         : Constants.NULL_RESPONSE_MESSAGE;

                     break;
                  case "stop": //stop instances
                     var stopRequest = new StopInstancesRequest(instanceIds);

                     var stopResponse = await ec2Client.StopInstancesAsync(stopRequest);
                     performOperation.Respone = stopResponse != null
                         ? stopResponse.HttpStatusCode.StatusMessage()
                         : Constants.NULL_RESPONSE_MESSAGE;

                     break;
                  default:
                     performOperation.Respone = $"Invalid operation [{ operation}].";
                     break;
               }

            }
         }
         catch ( Exception ex )
         {
            // Get a 'friendly', formatted version of the exception on error (storing it against the OperationReport property on the returned object)
            performOperation.Respone = ex.ParseException();
         }

         return performOperation;
      }
   }
}
