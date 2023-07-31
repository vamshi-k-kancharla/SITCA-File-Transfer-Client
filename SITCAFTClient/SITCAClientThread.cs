using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SITCAFileTransferClient
{
    class SITCAClientThread
    {
        /// <summary>
        /// A thread that writes the contents into a file for each subgroup of File Parts.
        /// </summary>
        /// 
        /// <param name="inputFileName"> Name of the input file to write contents into.</param>
        /// <param name="startFilePart"> Start of File Part Number.</param>
        /// <param name="numOfPartsForAThread"> Total number of file parts of the sub group.</param>
        /// <param name="numberOfFileParts"> Total number of file parts of entire file.</param>
        /// <param name="fileDestination"> File Stream object of destination file.</param>
        /// 
        /// <returns> Status code denoting the success/failure of the process.</returns>

        static public async void WriteContentsToTheFileThread(object threadStartParamObject)
        {
            FileStream fileDestination = null;

            try
            {
                SITCAThreadParameters threadParams = (SITCAThreadParameters)threadStartParamObject;

                string inputFileName = threadParams.inputFileName;
                int startFilePart = threadParams.startFilePart;
                int numOfPartsForAThread = threadParams.numOfPartsForAThread;
                int numberOfFileParts = threadParams.numberOfFileParts;
                fileDestination = threadParams.fileDestination;

                HttpClient httpSitcaClient = new HttpClient();
                HttpResponseMessage httpResponseMesssage = new HttpResponseMessage();

                // Create File and Fill it up

                int totalFileSize = numberOfFileParts * SITCAFTClientInputs.chunkSize;

                // Retrieve data for each part from file part queries

                string fileContentsRetrievePartURI = SITCAFTClientInputs.sitcaClientFilePartRetrievalURI +
                    SITCAFTClientInputs.sitcaTransferFileName + "/File-Part-";

                for (int i = startFilePart; i < startFilePart + numOfPartsForAThread; i++)
                {
                    
                    Console.WriteLine("=========================================================================");

                    Console.WriteLine("fileContentRetrievalURI = " + fileContentsRetrievePartURI + i);

                    string fileContentRetrievalURI = fileContentsRetrievePartURI + i;

                    httpResponseMesssage = await httpSitcaClient.GetAsync(fileContentRetrievalURI);

                    if (httpResponseMesssage.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine("File contents of Part Num = " + i);

                        string httpResponseContent = await httpResponseMesssage.Content.ReadAsStringAsync();

                        string httpProcessedResponse = "";

                        Console.WriteLine("httpResponseContent retrieved from http query : ");
                        
                        for (int j = 0; j < httpResponseContent.Length; j++)
                        {
                            if( j == 0 || j == httpResponseContent.Length -1 )
                            {
                                continue;
                            }

                            httpProcessedResponse += httpResponseContent[j];
                            Console.Write((char)httpResponseContent[j]);
                        }
                        Console.WriteLine("");

                        Console.WriteLine("httpResponseContent after response being processed before being replaced : " + 
                            httpProcessedResponse);

                        httpProcessedResponse = httpProcessedResponse.Replace("\\r\\n", " \n");

                        Console.WriteLine("httpResponseContent after response being processed and after replacement : " + httpProcessedResponse);

                        if ( SITCAFTClientInputs.bDebugFlag )
                        {
                            for (int j = 0; j < httpProcessedResponse.Length; j++)
                            {
                                Console.WriteLine("Letter No : " + j + " ,Char value = " + (char)httpProcessedResponse[j] +
                                    "Byte Value : " + httpProcessedResponse[j] + " ,int value = " + (int)httpProcessedResponse[j]);
                            }
                        }

                        byte[] httpProcessedResponseByteArray = new byte[httpProcessedResponse.Length];
                        
                        for( int j = 0; j < httpProcessedResponseByteArray.Length; j++)
                        {
                            httpProcessedResponseByteArray[j] = (byte)httpProcessedResponse[j];
                        }

                        Console.WriteLine("writing httpProcessedResponseByteArray.length = " + httpProcessedResponseByteArray.Length);

                        int currentWriteOffset = (i * SITCAFTClientInputs.chunkSize);

                        Console.WriteLine("Current offset value = " + currentWriteOffset);

                        SITCAFTClientInputs.writeThreadSyncMutex.WaitOne();

                        fileDestination.Seek(currentWriteOffset, SeekOrigin.Begin);
                        fileDestination.Write(httpProcessedResponseByteArray);

                        SITCAFTClientInputs.writeThreadSyncMutex.ReleaseMutex();

                        Console.WriteLine("After writing the string value to destination file");

                    }

                    else
                    {
                        Console.WriteLine("Error Response While Retrieving File Contents = " + httpResponseMesssage.StatusCode);
                        throw new ArgumentException("Error occured while retrieving file contents");
                    }

                    Console.WriteLine("=========================================================================");
                }

            }
            catch (Exception e)
            {

                Console.WriteLine("Exception occured while retrieving & writing the file contents : " + 
                    " , Message  = " + e.Message);

            }

            /*
            if( fileDestination != null )
            {
                fileDestination.Close();
            }*/

        }

    }
}

