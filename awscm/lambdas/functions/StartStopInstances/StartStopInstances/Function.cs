using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using AWSCM.AWSOperations.Common;
using AWSCM.AWSOperations.EC2;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer( typeof( Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer ) )]

namespace AWSCM.StartStopInstances
{
   public class Function
   {

      /// <summary>
      /// A simple function that takes a string and does a ToUpper
      /// </summary>
      /// <param name="input"></param>
      /// <param name="context"></param>
      /// <returns></returns>
      public async Task FunctionHandler( string input, ILambdaContext context )
      {
         LambdaLogger.Log( $"Executing the { context.FunctionName } for argument: [{input}] function with a { context.MemoryLimitInMB } MB limit." );

         //input
         //<operation>|<filters>|<others>

         Common.Others.Filters.ParseFunctionInput( input, out string operation, out string filters, out _ );

         var helper = new EC2Operations();

         // First, obtain instance ids to start
         var desc = await helper.GetInstancesByTags(input);
         LambdaLogger.Log( desc.Respone );

         // start instances based on the returned ids
         var performOperation = await helper.StartStopEC2InstancesByInstanceIds(desc.InstanceIDs, operation );
         LambdaLogger.Log( performOperation.Respone );

         LambdaLogger.Log( $"Finished executing the { context.FunctionName } function." );

      }
   }
}
