#region history
//*****************************************************************************
// Lambda.cs:
//
// History:
// 06/18/19 - gm  -  Started
// 06/18/19 - gm  -  RS8786 - Block EC2 Metadata Access
// 10/11/19 - gm  -  RS8707 - Automation, Encryption and Rotation of OAuth keys
//*
#endregion history
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;


namespace AWSCM.AWSInterface
{
   internal class Lambda
   {
      private Lambda() { }

      private AmazonLambdaClient LambdaClient()
      {
         if ( Common.TryGetCredentials( out AWSCredentials awsCredentials ) )
            return new AmazonLambdaClient( awsCredentials, Common.DefaultRegionEndpoint );
         else
            return new AmazonLambdaClient( new AmazonLambdaConfig() { RegionEndpoint = Common.DefaultRegionEndpoint, DisableLogging = true } );
      }

      private static readonly Lambda LambdaInstance = new Lambda();
   }
}
