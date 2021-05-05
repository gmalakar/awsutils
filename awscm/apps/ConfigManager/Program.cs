using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AWSCM.AWSConfigManager.Utilities;

namespace AWSCM.AWSConfigManager
{
   class Program
   {      
      internal const  string BATCHFILE =@"-BATCHFILE:";
      internal const  string BATCH =@"-BATCH:";
      internal const  string DEBUG =@"-DEBUG";
      internal const  string UTILITY =@"-U:";
      internal const  string UTILITYPARAM =@"U";
      internal const  string CONFIGURE =@"CONFIGURE:";
      static internal  Arguments arguments;
      static internal  IList<string[]> batchArguments = null;
      static string batchFileName = string.Empty;
      static bool debug = false;
      internal static bool IsInDebugMode { get; private set; }

      static void Main( string[] args )
      {
#if DEBUG
         System.Diagnostics.Debugger.Launch();
#endif
         try
         {
            if ( IsValidArguments( args ) )
            {
               var runningMode = @"Normal";
#if DEBUG
               IsInDebugMode = IsDebug( args );
#endif               
               arguments = new Arguments( args, IsInDebugMode );

               if (IsInDebugMode )
                  runningMode  = @"Debug";

               Console.WriteLine( $"Running in { runningMode} mode ......" );
               Console.WriteLine( string.Format( @"Output Redirection ......{0}", Console.IsOutputRedirected ) );

               if (arguments.HasParam(@"printparam"))
               {
                  Console.WriteLine( "Printing all parameters ......" );
                  arguments.PrintAllParams();
               }

               StartUtilities();
               //Program.CommandLineMain( arguments );
            }
            else
            {
               Common.WriteMessage( @"Invalid argumants!!" );
            }
         }
         catch ( Exception ex )
         {
            Common.WriteMessage( ex.Message );
            #if DEBUG
            throw new Exception( "Exception", ex );
            #endif
         }

      }

      private static void StartUtilities()
      {
         var logFileName = "awscm.log";
         var logToFile = false;
         var logOut = Console.Out;
         var pressEnter = true;
         if ( arguments["log"] != null )
         {
            logToFile = true;
            if ( !arguments.IsParamWithNoValue( "log" ) )
               logFileName = arguments.GetArgumentValue( "log" );
#if DEBUG
            Common.WriteMessage( string.Format( "LOG to file {0}", logFileName ) );
#endif
         }

         if ( IsInDebugMode || ( !logToFile && !Console.IsOutputRedirected ) )
         {
#if DEBUG
            Common.WriteMessage( "Using StreamWriter " );
#endif
            using ( var sw = new System.IO.StreamWriter( System.Console.OpenStandardOutput() ) )
            {
               if ( IsInDebugMode ) //increase read line length
                  Console.SetIn( new StreamReader( Console.OpenStandardInput(), Console.InputEncoding, false, bufferSize: 2048 ) );
               sw.AutoFlush = true;
               Console.SetOut( sw );
               Console.SetError( sw );
               Program.ExecuteUtility();
            }
            Console.SetOut( logOut );
#if DEBUG
            Common.WriteMessage( "End using StreamWriter " );
#endif
         }
         else if ( logToFile )
         {
            try
            {
               using ( var logFile = File.Open( CommonShared.Utilities.ValidLogFilePath( logFileName ), FileMode.OpenOrCreate, FileAccess.ReadWrite ) )
               {
                  using ( var logWriter = new StreamWriter( logFile, Encoding.UTF8 ) )
                  {
#if DEBUG
                     Common.WriteMessage( string.Format( "File stream created {0}", logFileName ) );
#endif
                     logWriter.AutoFlush = true;
                     Console.SetOut( logWriter );
                     Console.SetError( logWriter );
                     Program.ExecuteUtility();
                  }
               }
               Console.SetOut( logOut );
#if DEBUG
               Common.WriteMessage( string.Format( "File stream closed {0}", logFileName ) );
               PostEnterKey( "In debug and has log file" );
               pressEnter = false;
               //System.Windows.Forms.SendKeys.SendWait( "{ENTER}" );
#endif
            }
            catch ( Exception e )
            {
               Common.WriteMessage( string.Format( "Cannot open {0) for writing", logFileName ) );
               Common.WriteMessage( "Using Console Instead" );
               Common.WriteMessage( e.Message );
            }
         }
         else
         {
#if DEBUG
            Common.WriteMessage( $"Debug Mode: {IsInDebugMode}, " +
               $"OutputRedirected: {Console.IsOutputRedirected} " );
#endif
            Program.ExecuteUtility();
         }
         if ( !IsInDebugMode && !Console.IsOutputRedirected )
         {
            if ( pressEnter )
               PostEnterKey( "Output not redirected!" );
         }
      }

      private static void PostEnterKey( string from )
      {
#if DEBUG
         Common.WriteMessage( $"Enter Key Pressed from {from}" );
#endif
      }
      
      private static bool IsDebug( string[] args )
      {
         var isDebug = false;
#if DEBUG
         var arg = string.Empty;
         if ( args.Length > 0 )
         {
            arg = args[0].ToUpper();
            isDebug = arg.StartsWith( Program.DEBUG );
         }
#endif
         return isDebug;
      }

      private static bool IsValidArguments( string[] args )
      {
         var isValidArgs = false;
         var arg = string.Empty;
         if ( args.Length > 0 )
         {
            arg = args[0].ToUpper();
            isValidArgs = arg.StartsWith( Program.UTILITY ) ||
                            arg.StartsWith( Program.DEBUG ) ||
                            arg.StartsWith( Program.BATCH ) ||
                            arg.StartsWith( Program.BATCHFILE ) ||
                             arg.Equals( Program.CONFIGURE );
         }
         return isValidArgs;
      }
      //https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp     
      //private static string[] SplitArgs( string unsplitArgumentLine )
      //{
      //   unsplitArgumentLine = unsplitArgumentLine.Replace( @"""""", @"~|!~" );
      //   int numberOfArgs;
      //   IntPtr ptrToSplitArgs;
      //   string[] splitArgs;

      //   ptrToSplitArgs = CommandLineToArgvW( unsplitArgumentLine, out numberOfArgs );
      //   if ( ptrToSplitArgs == IntPtr.Zero )
      //      throw new ArgumentException( "Unable to split argument.",
      //        new Win32Exception() );
      //   try
      //   {
      //      splitArgs = new string[numberOfArgs];
      //      for ( int i = 0; i < numberOfArgs; i++ )
      //         splitArgs[i] = Marshal.PtrToStringUni( Marshal.ReadIntPtr( ptrToSplitArgs, i * IntPtr.Size ) ).Replace( @"~|!~", @"""" );
      //      return splitArgs;
      //   }
      //   finally
      //   {
      //      LocalFree( ptrToSplitArgs );
      //   }
      //}

      internal static void ExecuteUtility()
      {
         try
         {
            Program.ExecuteUtilityLocal();
         }
         catch ( Exception ex )
         {
            Common.WriteMessage( ex.Message );
         }
      }

      private static void ExecuteUtilityLocal()
      {
         try
         {
            Common.IsBatchArguments = arguments.TryGetBatchArgs( out batchArguments, out batchFileName );

            if ( !Common.IsBatchArguments && arguments[UTILITYPARAM] == null && !debug )
              Console.WriteLine( "Please define utility as -U:XXXXXX" );
            else
            {

               RunUtilities();
               if ( IsInDebugMode )
                  Console.WriteLine( "Running in DEBUG Mode...." );
            }
         }
         catch ( Exception ex )
         {
            Common.WriteMessage( "Error:" + ex.Message );
         }

#if DEBUG
         if ( arguments["debug"] != null && arguments["debug"] == "true" )
         {
            debug = true;
         }
         if ( arguments["exit"] != null && arguments["exit"] == "true" )
         {
            debug = false;
         }
#endif

         if ( debug )
         {
            Common.WriteMessage( "DEBUG Mode - Enter arguments:" );
            var debugArg = Console.ReadLine();
         }

      }

      static void RunUtilities()
      {
         if ( Common.IsBatchArguments )
         {
#if DEBUG
            Common.WriteMessage( "Running in BATCH ..." );
            if ( !string.IsNullOrEmpty( batchFileName ) )
               Common.WriteMessage( "Using batch file : " + batchFileName );
#endif
            try
            {
               foreach ( var arg in batchArguments )
               {
                  try
                  {
                     if ( arg != null )
                     {
                        Utilities( new Arguments( arg, debug ) );
                     }
                  }
                  catch ( Exception ex ) //for batch processing we will log error but continue to next
                  {
                     Common.WriteMessage( "Error:" + ex.Message );
                  }
               }
            }
            catch ( Exception ex )
            {
               Common.WriteMessage( "Error:" + ex.Message );
            }
         }
         else
         {
            Utilities( arguments );
         }
      }

      static void Utilities( Arguments args )
      {
         //System.Diagnostics.Debugger.Launch();
         args = args ?? arguments;
         if ( args.TryGetArgumentValue( UTILITYPARAM, out string utility ) )
         {

#if DEBUG
            Common.WriteMessage( "Utility : " + utility.ToUpper() );
#endif
            switch ( utility.ToUpper() )
            {
               case "DESCRIBE":
               case "DESC":
                  Common.Describe( args );
                  break;
               case "GENERAL":
               case "GEN":
                  Common.General( args );
                  break;
               case "EC2":
               case "INSTANCE":
                  Common.EC2Instance( args );
                  break;
               case "CE":
                  Common.CloudEndure( args );
                  break;
               case "IAM":
                  Common.IAMInstance( args );
                  break;
               case "SSM":
                  Common.SSMInstance( args );
                  break;
               default:
                  if ( !IsInDebugMode || debug )
                     Common.WriteMessage( "Invalid/Missing Command Line Utility!! " );
                  break;
            }
         }
         else
         {
#if DEBUG
            Common.WriteMessage( "Invalid/Missing Command Line Utility!! " );
#endif
         }
      }

   }
}
