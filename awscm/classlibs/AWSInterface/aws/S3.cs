//*****************************************************************************
// s3.cs:
//
// History:
// 06/01/19 - gm  -  Started
// 06/18/19 - gm  -  RS8786 - Block EC2 Metadata Access
//*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using Amazon;
using Amazon.Runtime;
using Amazon.S3.IO;
using System.Security;
using System.Net;
using System.Security.Permissions;
using System.Globalization;
using Amazon.S3.Util;
using Mongoose.Core.Cloud.AWSInterface;
using System.Threading.Tasks;

namespace Mongoose.Core.Cloud.AWSInterfaceService
{
   public class S3
   {
      private S3() { }

      [SecuritySafeCritical]
      private AmazonS3Client S3Client( AWSInterfaceConfig config )
      {
         var setRegion = string.Empty;
         var setServiceUrl = string.Empty;
         var hasCredentials = false;
         if ( config != null )
         {
            if ( !string.IsNullOrEmpty( config.AWSRegion ) )
               setRegion = config.AWSRegion;
            hasCredentials = config.AWSHasCredentials;
 
            if ( !string.IsNullOrEmpty( config.AWSServiceUrl ) )
               setServiceUrl = config.AWSServiceUrl;
         }
         var awsConfig = new AmazonS3Config() { ServiceURL = awsCommon.AWSEnvironment.DefaultServiceUrl, RegionEndpoint = awsCommon.AWSEnvironment.DefaultRegionEndpoint, DisableLogging = true };

         if ( string.IsNullOrEmpty( setServiceUrl ) ) //by default service url
            setServiceUrl = this.awsCommon.AWSEnvironment.DefaultServiceUrl;

        if ( !string.IsNullOrEmpty( setRegion ) ) //by region endpoint
            awsConfig.RegionEndpoint = RegionEndpoint.GetBySystemName( setRegion );

         if ( !string.IsNullOrEmpty( setServiceUrl ) ) // by service url
            awsConfig.ServiceURL = setServiceUrl;

         AmazonS3Client s3Client;
         if ( hasCredentials )
         {
            if ( string.IsNullOrEmpty( config.AWSSessionToken ) )
               s3Client = new AmazonS3Client( config.AWSAccessKeyId, config.AWSSecretAccessKey, awsConfig );
            else
               s3Client = new AmazonS3Client( config.AWSAccessKeyId, config.AWSSecretAccessKey, config.AWSSessionToken, awsConfig );
         }
         else if ( this.awsCommon.AWSEnvironment.GetAWSCredentialsFromFile ||
            ( !this.awsCommon.AWSEnvironment.IsEC2Instance && Configuration.UseAWSInterfaceService ) )
         {
            var success = Common.TryGetAWSCredentialsInNonEC2( out string awsAccessKeyId, out string awsSecretAccessKey, out string token, out AWSInterfaceError error );
            if ( success )
               s3Client = new AmazonS3Client( awsAccessKeyId, awsSecretAccessKey, token, awsConfig );
            else
               throw new AmazonClientException( $"Unable to find credentials in dev/test/non ec2 environment Error{ error.Message}." );
         }
         else
            s3Client = new AmazonS3Client( awsConfig );
         return s3Client;
      }

      private S3DirectoryInfo GetDirectory( AmazonS3Client s3Client, string bucketName, string path )
      {
         return new S3DirectoryInfo( s3Client, bucketName, path );
      }

      private S3FileInfo GetFile( AmazonS3Client s3Client, string bucketName, string path )
      {
         return new S3FileInfo( s3Client, bucketName, path );
      }

      private List<S3FileInfo> GetFiles( S3DirectoryInfo directory, string[] searchPatterns = null, SearchOption searchOption = SearchOption.TopDirectoryOnly, int maxRecursionCount = -1, List<S3FileInfo> list = null )
      {
         var files = list == null ? new List<S3FileInfo>() : list;
         if ( searchPatterns == null || searchPatterns.Count() == 0 )
            files.AddRange( directory.GetFiles( "*", searchOption ) );
         else
         {
            foreach ( var sp in searchPatterns )
            {
               var gfiles = directory.GetFiles( sp, searchOption );
               files.AddRange( gfiles );
            }
         }
         if ( maxRecursionCount > 0 && ( searchOption == SearchOption.TopDirectoryOnly ) )
         {
            var localDir = this.GetDirectories( directory, searchPatterns );
            if ( localDir != null && localDir.Count() > 0 )
               foreach ( var dir in localDir )
                  this.GetFiles( dir, searchPatterns, searchOption, maxRecursionCount - 1, files );
         }
         return files;
      }

      private List<IS3FileSystemInfo> GetFileSystemEntries( S3DirectoryInfo directory, string[] searchPatterns = null, SearchOption searchOption = SearchOption.TopDirectoryOnly, int maxRecursionCount = -1, List<IS3FileSystemInfo> list = null )
      {
         var files = list == null ? new List<IS3FileSystemInfo>() : list;
         if ( searchPatterns == null || searchPatterns.Count() == 0 )
            files.AddRange( directory.GetFileSystemInfos( "*", searchOption ) );
         else
         {
            foreach ( var sp in searchPatterns )
               files.AddRange( directory.GetFileSystemInfos( sp, searchOption ) );
         }

         if ( maxRecursionCount > 0 && ( searchOption == SearchOption.TopDirectoryOnly ) )
         {
            var localDir = this.GetDirectories( directory, searchPatterns );
            if ( localDir != null && localDir.Count() > 0 )
               foreach ( var dir in localDir )
                  this.GetFileSystemEntries( dir, searchPatterns, searchOption, maxRecursionCount - 1, files );
         }
         return files;

      }

      private List<S3DirectoryInfo> GetDirectories( S3DirectoryInfo directory, string[] searchPatterns = null, SearchOption searchOption = SearchOption.TopDirectoryOnly, int maxRecursionCount = -1, List<S3DirectoryInfo> list = null )
      {

         var directories = list == null ? new List<S3DirectoryInfo>() : list;
         //add root path
         if ( list == null )
            directories.Add( directory );

         S3DirectoryInfo[] localDir = null;
         if ( searchPatterns == null || searchPatterns.Count() == 0 )
            localDir = directory.GetDirectories( "*", searchOption );
         else
         {
            foreach ( var sp in searchPatterns )
               localDir = directory.GetDirectories( sp, searchOption );
            //directories.AddRange( directory.GetDirectories( sp, searchOption ) );
         }
         if ( localDir != null )
            directories.AddRange( localDir );

         if ( maxRecursionCount > 0 && ( localDir != null && localDir.Count() > 0 ) && ( searchOption == SearchOption.TopDirectoryOnly ) )
         {
            foreach ( var dir in localDir )
               this.GetDirectories( dir, searchPatterns, searchOption, maxRecursionCount - 1, directories );
         }
         return directories;
      }

      private Stream GetObject( AmazonS3Client s3Client, string bucketName, string filePath, out AWSInterfaceError error )
      {
         var fileContent = default( MemoryStream );
         error = new AWSInterfaceError();
         try
         {
            filePath = InterfaceUtils.MassageFilePath( filePath );
            var s3request = new GetObjectRequest
            {
               BucketName = bucketName,
               Key = filePath,
            };
            using ( var s3response = s3Client.GetObject( s3request ) )
            {
               using ( var responseStream = s3response.ResponseStream )
               {
                  fileContent = new MemoryStream();
                  responseStream.CopyTo( fileContent );
                  fileContent.Position = 0;
               }
            }

         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return fileContent;
      }


      private Stream ReadFile( AmazonS3Client s3Client, string bucketName, string filePath, out AWSInterfaceError error )
      {
         var fileContent = default( MemoryStream );
         error = new AWSInterfaceError();
         try
         {
            filePath = InterfaceUtils.MassageFilePath( filePath );
            var s3FileInfo = new S3FileInfo( s3Client, bucketName, filePath );
            using ( var stream = s3FileInfo.OpenRead() )
            {
               fileContent = new MemoryStream();
               stream.CopyTo( fileContent );
               fileContent.Position = 0;
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return fileContent;
      }

      private bool TryReadFileText( AmazonS3Client s3Client, string bucketName, string filePath, out string fileContent, out AWSInterfaceError error )
      {
         fileContent = string.Empty;
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            filePath = InterfaceUtils.MassageFilePath( filePath );
            var s3FileInfo = new S3FileInfo( s3Client, bucketName, filePath );

            using ( var stream = s3FileInfo.OpenRead() )
            {
               using ( StreamReader reader = new StreamReader( stream ) )
               {
                  fileContent = reader.ReadToEnd();
                  success = true;
               }
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }
      
      private bool TryGetTags( AmazonS3Client s3Client, string bucketName, string filePath, out IDictionary<string, string> tags, out AWSInterfaceError error )
      {
         return this.TryGetObjectTagging( s3Client, bucketName, filePath, out tags, out error );
      }

      private bool TryTagFile( AmazonS3Client s3Client, string bucketName, string filePath, IDictionary<string, string> tags, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         var success = false;
         if ( tags != null && tags.Count > 0 )
         {
            try
            {
               filePath = InterfaceUtils.MassageFilePath( filePath );
               var tag = tags.Select( t => new Tag() { Key = t.Key, Value = t.Value } ).ToList();
               success = this.TryPutObjectTagging( s3Client, bucketName, InterfaceUtils.MassageFilePath( filePath ), new Tagging() { TagSet = tag }, out error );
            }
            catch ( Exception e )
            {
               Common.SetException( e, error );
            }
         }
         return success;
      }

      private bool TrySaveFile( AmazonS3Client s3Client, string bucketName, string filePath, Stream inputStream, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            var stream = default( Stream );
            var s3FileInfo = new S3FileInfo( s3Client, bucketName, filePath );
            if ( s3FileInfo.Exists )
               stream = s3FileInfo.OpenWrite();
            else
               stream = s3FileInfo.Create();
            using ( stream )
            {
               inputStream.CopyTo( stream );
               success = true;
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      private bool TrySaveFileText( AmazonS3Client s3Client, string bucketName, string filePath, string fileContent, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            filePath = InterfaceUtils.MassageFilePath( filePath );

            var stream = default( Stream );
            var s3FileInfo = new S3FileInfo( s3Client, bucketName, filePath );

            if ( s3FileInfo.Exists )
               stream = s3FileInfo.OpenWrite();
            else
               stream = s3FileInfo.Create();
            using ( var writer = new StreamWriter( stream ) )
            {
               writer.Write( fileContent );
               success = true;
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      private bool TryGetObjectTagging( AmazonS3Client s3Client, string bucketName, string filePath, out IDictionary<string, string> tags, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         tags = null;
         var success = false;
         try
         {
            filePath = InterfaceUtils.MassageFilePath( filePath );

            var s3request = new GetObjectTaggingRequest
            {
               BucketName = bucketName,
               Key = filePath,
            };
            var tag = s3Client.GetObjectTagging( s3request );
            if ( tag.Tagging != null )
            {
               tags = tag.Tagging.ToDictionary( x => x.Key, x => x.Value );
               success = true;
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      private bool TryPutObjectTagging( AmazonS3Client s3Client, string bucketName, string filePath, Tagging tag, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            if ( tag != null && tag.TagSet != null )
            {
               filePath = InterfaceUtils.MassageFilePath( filePath );

               var s3request = new PutObjectTaggingRequest
               {
                  BucketName = bucketName,
                  Key = filePath,
                  Tagging = tag
               };
               var response = s3Client.PutObjectTagging( s3request );
               success = response != null && response.HttpStatusCode == HttpStatusCode.OK;
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      private bool TryObjectExists( AmazonS3Client s3Client, string bucketName, string objectKey, out GetObjectMetadataResponse objectMetaData, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         var exists = false;
         objectMetaData = null;
         //this.ResetException();
         try
         {
            objectMetaData = s3Client.GetObjectMetadata( bucketName, objectKey );
            exists = objectMetaData != null;

         }
         catch ( AmazonS3Exception e )
         {
            if ( e.ErrorCode.Equals( NOT_FOUND ) || e.StatusCode == HttpStatusCode.NotFound )
               exists = false;
            else
               Common.SetException( e, error );
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return exists;
      }

      private bool TryPutObject( out AWSInterfaceError error, AWSInterfaceConfig config, AmazonS3Client s3Client, string bucketName, string objectKey, string contentBody = "", Stream inputStream = null, string contentType = "", TimeSpan? timeout = null )
      {
         error = new AWSInterfaceError();
         var success = false;
         var fileContent = default(MemoryStream);
         try
         {
            objectKey = InterfaceUtils.MassageFilePath( objectKey );

            var s3request = new PutObjectRequest
            {
               BucketName = bucketName,
               Key = objectKey
            };

            if ( config.ServerSideEncryptionMethod.ToUpper() != "NONE" )
            {
               s3request.ServerSideEncryptionMethod = InterfaceUtils.GetEncryptionMethod( config.ServerSideEncryptionMethod );
               if ( config.ServerSideEncryptionMethod == AWSKMS && !string.IsNullOrEmpty( config.ServerSideEncryptionKey ) )
                  s3request.ServerSideEncryptionKeyManagementServiceKeyId = config.ServerSideEncryptionKey;
            }

            if ( inputStream != null )
            {
               fileContent = new MemoryStream();
               inputStream.CopyTo( fileContent );
               fileContent.Position = 0;
               s3request.InputStream = fileContent;
            }

            if ( !string.IsNullOrEmpty( contentBody.Trim() ) )
               s3request.ContentBody = contentBody;

            if ( timeout != null && timeout.HasValue )
               s3request.Timeout = timeout.Value;

            if ( !string.IsNullOrEmpty( contentType.Trim() ) )
               s3request.ContentType = contentType;

            var response = s3Client.PutObject( s3request );
            //TODO need to check ettag for successfull put
            success = response != null && !string.IsNullOrWhiteSpace( response.ETag );// && response.HttpStatusCode == HttpStatusCode.OK
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         finally
         {
            fileContent = null;
         }
         return success;
      }

      private static T ToEnum<T>( string value, T defaultValue )
          where T : struct, IConvertible
      {
         T result;

         if ( !Enum.TryParse<T>( value, true, out result )
            || !Enum.IsDefined( typeof( T ), result ) && !result.ToString().Contains( "," ) )
            result = defaultValue;

         return result;
      }

      private bool TryGetPreSignedURL( out string url, out AWSInterfaceError error, AWSInterfaceConfig config, AmazonS3Client s3Client, string bucketName, string objectKey, AWSS3HttpVerb verb, int expires = -1 )
      {
         error = new AWSInterfaceError();
         url = string.Empty;
         var success = false;
         try
         {
            objectKey = InterfaceUtils.MassageFilePath( objectKey );
            if ( expires <= 0 )
               expires = EC2.MongooseS3ExpiresAfterSeconds;
            var httpVerb = ToEnum<HttpVerb>( verb.ToString(), HttpVerb.HEAD );
            var s3request = new GetPreSignedUrlRequest
            {
               BucketName = bucketName,
               Key = objectKey,
               Verb = httpVerb,
               Expires = DateTime.Now.AddSeconds( expires > 0 ? expires : EXPIRES_AFTER_SECONDS )
            };
            var addEncryptionMethod = false;
            var addEncryptionKey = false;
            if ( httpVerb == HttpVerb.PUT && config.ServerSideEncryptionMethod.ToUpper() != "NONE" )
            {
               addEncryptionMethod = true;
               s3request.ServerSideEncryptionMethod = InterfaceUtils.GetEncryptionMethod( config.ServerSideEncryptionMethod );
               if ( config.ServerSideEncryptionMethod == AWSKMS && !string.IsNullOrEmpty( config.ServerSideEncryptionKey ) )
               {
                  addEncryptionKey = true;
                  s3request.ServerSideEncryptionKeyManagementServiceKeyId = config.ServerSideEncryptionKey;
               }
            }
            url = s3Client.GetPreSignedURL( s3request );
            if ( addEncryptionMethod )
            {
               var sep = string.Join( string.Empty, InterfaceUtils.PROP_DELIMETER );
               url = url + sep + config.ServerSideEncryptionMethod;
               if ( addEncryptionKey )
                  url = url + sep + config.ServerSideEncryptionKey;
            }
            success = true;
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      private string FilePrefix( string bucket, string fullPath )
      {
         return bucket + PREFIX_SEPERATOR + fullPath;
      }

      private string StripFilePrefix( string filePath, string prefix )
      {
         var strippedText = filePath;
         if ( !string.IsNullOrEmpty( filePath ) && prefix.Length > 0 && filePath.Length >= prefix.Length && filePath.StartsWith( prefix, StringComparison.OrdinalIgnoreCase ) )
         {
            strippedText = filePath.Substring( prefix.Length );
            if ( strippedText.Length == 0 )
               strippedText = InterfaceUtils.S3ListDelimeter.ToString();
         }
         return strippedText;
      }


      internal static Stream ReadFile( string bucketName, string filePath, AWSInterfaceConfig config, out AWSInterfaceError error )
      {
         var fileContent = default( Stream );
         error = new AWSInterfaceError();
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               fileContent = S3BucketInstance.ReadFile( s3Client, bucketName, filePath, out error );
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return fileContent;
      }

      internal static Stream GetObject( string bucketName, string filePath, AWSInterfaceConfig config, out AWSInterfaceError error )
      {
         var fileContent = default( Stream );
         error = new AWSInterfaceError();
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               fileContent = S3BucketInstance.GetObject( s3Client, bucketName, filePath, out error );
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return fileContent;
      }
      
      internal static bool TryReadFileText( string bucketName, string filePath, AWSInterfaceConfig config, out string fileContent, out AWSInterfaceError error )
      {
         fileContent = null;
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               success = S3BucketInstance.TryReadFileText( s3Client, bucketName, filePath, out fileContent, out error );
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static bool TryGetTags( string bucketName, string filePath, AWSInterfaceConfig config, out IDictionary<string, string> tags, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         tags = null;
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               success = S3BucketInstance.TryGetTags( s3Client, bucketName, filePath, out tags, out error );
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static bool TryTagFile( string bucketName, string filePath, AWSInterfaceConfig config, IDictionary<string, string> tags, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               success = S3BucketInstance.TryTagFile( s3Client, bucketName, filePath, tags, out error );
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static bool FileExists( string bucketName, string filePath, AWSInterfaceConfig config, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               var s3FileInfo = S3BucketInstance.GetFile( s3Client, bucketName, filePath );
               success = s3FileInfo.Exists;
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static bool TryFileInfo( string bucketName, string filePath, AWSInterfaceConfig config, out S3FileInfo fileInfo, out AWSInterfaceError error )
      {
         fileInfo = null;
         var success = false;
         error = new AWSInterfaceError();
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               fileInfo = S3BucketInstance.GetFile( s3Client, bucketName, filePath );
               success = fileInfo.Exists;
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static bool TryGetLastWriteTimeAsDateTime( string bucketName, string filePath, AWSInterfaceConfig config, out DateTime lastWriteTimeUtc, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         lastWriteTimeUtc = DateTime.MinValue;
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               var s3FileInfo = S3BucketInstance.GetFile( s3Client, bucketName, filePath );
               if ( s3FileInfo.Exists )
               {
                  lastWriteTimeUtc = s3FileInfo.LastWriteTimeUtc;
                  success = true;
               }
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }
      internal static List<string> GetFiles(
         out AWSInterfaceError error,
         AWSListType action,
         string bucketName,
         string bucketKey,
         string filePrefixToTrim,
         string[] extensions,
         AWSInterfaceConfig config,
         int maxRecursionCount = 3 )
      {
         error = new AWSInterfaceError();
         var files = new List<string>();
         var dirInfo = default( S3DirectoryInfo );
         try
         {
            bucketKey = InterfaceUtils.MassageFilePath( bucketKey, false ).TrimEnd( InterfaceUtils.S3ListDelimeter );
            filePrefixToTrim = InterfaceUtils.MassageFilePath( filePrefixToTrim, false );
            var filePrefix = S3BucketInstance.FilePrefix( bucketName, filePrefixToTrim );
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               dirInfo = S3BucketInstance.GetDirectory( s3Client, bucketName, bucketKey );
               if ( dirInfo != null )
               {
                  switch ( action )
                  {
                     case AWSListType.File:
                        files.AddRange( S3BucketInstance.GetFiles( dirInfo, extensions, SearchOption.TopDirectoryOnly, maxRecursionCount ).Select( o => o.Name ).ToList() );
                        break;
                     case AWSListType.FileFullPath:
                        files.AddRange( S3BucketInstance.GetFiles( dirInfo, extensions, SearchOption.TopDirectoryOnly, maxRecursionCount ).Select( o => S3BucketInstance.StripFilePrefix( o.FullName, filePrefix ) ).ToList() );
                        break;
                     case AWSListType.Directory:
                        files.AddRange( S3BucketInstance.GetDirectories( dirInfo, extensions, SearchOption.TopDirectoryOnly, maxRecursionCount ).Select( o => o.Name ).ToList() );
                        break;
                     case AWSListType.DirectoryFullPath:
                        files.AddRange( S3BucketInstance.GetDirectories( dirInfo, extensions, SearchOption.TopDirectoryOnly, maxRecursionCount ).Select( o => S3BucketInstance.StripFilePrefix( o.FullName, filePrefix ) ).ToList() );
                        break;
                     case AWSListType.DirectoryTreeFullPath:
                        files.AddRange( S3BucketInstance.GetDirectories( dirInfo, extensions, SearchOption.AllDirectories ).Select( o => S3BucketInstance.StripFilePrefix( o.FullName, filePrefix ) ).ToList() );
                        break;
                     case AWSListType.All:
                        files.AddRange( S3BucketInstance.GetFileSystemEntries( dirInfo, extensions, SearchOption.TopDirectoryOnly, maxRecursionCount ).Select( o => o.Name ).ToList() );
                        break;
                     case AWSListType.AllFullPath:
                        files.AddRange( S3BucketInstance.GetFileSystemEntries( dirInfo, extensions, SearchOption.TopDirectoryOnly, maxRecursionCount ).Select( o => S3BucketInstance.StripFilePrefix( o.FullName, filePrefix ) ).ToList() );
                        break;
                     default:
                        files.AddRange( S3BucketInstance.GetFiles( dirInfo, extensions, SearchOption.TopDirectoryOnly, maxRecursionCount ).Select( o => o.Name ).ToList() );
                        break;
                  }
               }
            }
         }
         catch ( Exception ex )
         {
            Common.SetException( ex, error );
         }
         return files;
      }

      internal static bool TryDirectoryExists( out AWSInterfaceError error, string bucketName, string directoryPath, AWSInterfaceConfig config )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               var dirInfo = S3BucketInstance.GetDirectory( s3Client, bucketName, directoryPath );
               success = dirInfo != null && dirInfo.Exists;
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static bool TryObjectExists( out AWSInterfaceError error, string bucketName, string objectKey, AWSInterfaceConfig config )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               success = S3BucketInstance.TryObjectExists( s3Client, bucketName, objectKey, out GetObjectMetadataResponse mt, out error );
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static DirtoryInfoResponse DirectoryInfo( string bucketName, string directoryPath, AWSInterfaceConfig config )
      {
         var response = new DirtoryInfoResponse();
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               response.DirInfo = InterfaceUtils.GetAWSDirInfo( S3BucketInstance.GetDirectory( s3Client, bucketName, directoryPath ) );
               response.Success = response.DirInfo != null && response.DirInfo.Exists;
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, response.InterfaceError );
         }
         return response;
      }

      internal static bool DoesS3BucketExist( string bucketName, AWSInterfaceConfig config, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               success = AmazonS3Util.DoesS3BucketExistV2( s3Client, bucketName );
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static bool TryDeleteFile( string bucketName, string filePath, AWSInterfaceConfig config, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               var s3FileInfo = new S3FileInfo( s3Client, bucketName, filePath );
               if ( s3FileInfo.Exists )
               {
                  s3FileInfo.Delete();
                  success = true;
               }
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static bool TryDeleteObject( string bucketName, string filePath, AWSInterfaceConfig config, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               filePath = InterfaceUtils.MassageFilePath( filePath );

               var s3request = new DeleteObjectRequest
               {
                  BucketName = bucketName,
                  Key = filePath,
               };
               var response =  s3Client.DeleteObject( s3request );
               //TODO need to check ettag for successfull put
               success = response != null;// && response.HttpStatusCode == HttpStatusCode.OK;
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static bool TryGetPreSignedURL( out string url, out AWSInterfaceError error, string bucketName, string objectKey, AWSS3HttpVerb verb, AWSInterfaceConfig config, int expires = -1 )
      {
         error = new AWSInterfaceError();
         url = string.Empty;
         var success = false;
         var awsKms = config.ServerSideEncryptionMethod == AWSKMS;
         var currentAWSConfigsS3UseSignatureVersion4 = AWSConfigsS3.UseSignatureVersion4;
         try
         {
            if ( awsKms )
               AWSConfigsS3.UseSignatureVersion4 = true;
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
               success = S3BucketInstance.TryGetPreSignedURL( out url, out error, config, s3Client, bucketName, objectKey, verb );
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         finally
         {
            if ( awsKms )
               AWSConfigsS3.UseSignatureVersion4 = currentAWSConfigsS3UseSignatureVersion4;
         }
         return success;
      }

      internal static bool TrySaveFile( string bucketName, string filePath, System.Xml.XmlDocument xmlDocument, AWSInterfaceConfig config, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            using ( var xmlStream = new MemoryStream() )
            {
               xmlDocument.Save( xmlStream );
               xmlStream.Flush();//Adjust this if you want read your data 
               xmlStream.Position = 0;
               success = TrySaveFile( bucketName, filePath, xmlStream, config, out error );
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static bool TrySaveFile( string bucketName, string filePath, Stream inputStream, AWSInterfaceConfig config, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               if ( config.ServerSideEncryptionMethod != "NONE" ) //put object with encryption
                  success = S3BucketInstance.TryPutObject( out error, config, s3Client, bucketName, filePath, "", inputStream );
               else
                  success = S3BucketInstance.TrySaveFile( s3Client, bucketName, filePath, inputStream, out error );
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static bool TrySaveFile( string bucketName, string filePath, string fileContent, AWSInterfaceConfig config, out AWSInterfaceError error )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               if ( config.ServerSideEncryptionMethod != "NONE" ) //put object with encryption
                  success = S3BucketInstance.TryPutObject( out error, config, s3Client, bucketName, filePath, fileContent );
               else
                  success = S3BucketInstance.TrySaveFileText( s3Client, bucketName, filePath, fileContent, out error );
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static bool TryPutObject( out AWSInterfaceError error, string bucketName, string filePath, Stream fileContent, AWSInterfaceConfig config, string contentType = "", TimeSpan? timeout = null )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               success = S3BucketInstance.TryPutObject( out error, config, s3Client, bucketName, filePath, string.Empty, fileContent );
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }


      internal static bool TryPutObject( out AWSInterfaceError error, string bucketName, string filePath, string fileContent, AWSInterfaceConfig config, string contentType = "", TimeSpan? timeout = null )
      {
         error = new AWSInterfaceError();
         var success = false;
         try
         {
            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               success = S3BucketInstance.TryPutObject( out error, config, s3Client, bucketName, filePath, fileContent, null, contentType, timeout );
            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }

      internal static bool TryCreateFolder( out AWSInterfaceError error, out bool exists, string bucketName, string folderPath, AWSInterfaceConfig config )
      {
         error = new AWSInterfaceError();
         var success = false;
         exists = false;
         try
         {
            //we will check a dummy file
            //if not exists, we will create
            folderPath = InterfaceUtils.CombinePath( folderPath, @"mgs3directorycheck.txt" );

            using ( var s3Client = S3BucketInstance.S3Client( config ) )
            {
               var s3FileInfo = S3BucketInstance.GetFile( s3Client, bucketName, folderPath );
               if ( !s3FileInfo.Exists )
               {
                  var stream = s3FileInfo.Create();
                  using ( var writer = new StreamWriter( stream ) )
                  {
                     writer.Write( Encoding.ASCII.GetBytes( @"This is file is saved to test of bucket and key is valid" ) );
                     success = true; //write success
                  }
               }
               else
                  success = true; //read success
               //now we delete the file
               if ( success )
               {
                  s3FileInfo.Delete();
                  success = true; //delete success
               }

            }
         }
         catch ( Exception e )
         {
            Common.SetException( e, error );
         }
         return success;
      }


      private static readonly S3 S3BucketInstance = new S3();
      private Common awsCommon = Common.CommonInstance;
      private static readonly string PREFIX_SEPERATOR = ":\\";
      private const string NOT_FOUND = "NotFound";
      private const Int16 EXPIRES_AFTER_SECONDS = 60;
      private const string AWSKMS = "AWSKMS";
   }
}