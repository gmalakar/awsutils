using System;
using Amazon.EC2;
using Amazon.Runtime;

namespace AWSCM.AWSHelper
{
   public static class EC2Helper
   {
      private static AmazonEC2Client EC2Client()
      {
         return new AmazonEC2Client();
      }
   }
}
