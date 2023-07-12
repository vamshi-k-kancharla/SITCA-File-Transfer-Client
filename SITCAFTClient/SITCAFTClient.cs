using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SITCAFileTransferClient
{
    class SITCAFTClient
    {
        public static FileStream fileDestination;

        static public async Task<int> RetrieveAFile(string inputFileName)
        {
            try
            {

                HttpClient httpSitcaClient = new HttpClient();

                string fileRetrievalURI = SITCAFTClientInputs.sitcaClientLoadFileURI +
                    SITCAFTClientInputs.sitcaTransferFileName;

                HttpResponseMessage httpResponseMesssage = await httpSitcaClient.GetAsync(fileRetrievalURI);

                if (httpResponseMesssage.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Input File has been loaded successfully");
                    Console.WriteLine("Contents of Response = ");

                    string httpResponseContent = await httpResponseMesssage.Content.ReadAsStringAsync();
                    Console.WriteLine(httpResponseContent);
                }

                else
                {
                    Console.WriteLine("Error Response While Loading the file " + httpResponseMesssage.StatusCode);
                }

                return (int)httpResponseMesssage.StatusCode;

            }
            catch (Exception e)
            {

                Console.WriteLine("Exception occured while retrieving the input file : " + inputFileName +
                    " , Message  = " + e.Message);

                return -1;
            }

        }

        /************************************************************************************************************
         * Retrieve file contents based on preset protocol and create new file
         * 
         ************************************************************************************************************/

        static public async Task<int> WriteContentsToAFile(string inputFileName)
        {
            int returnValue = 0;

            try
            {
                int numberOfFileParts = 0;
                int currentOffset = 0;

                HttpClient httpSitcaClient = new HttpClient();

                string fileContentsNoOfPartsURI = SITCAFTClientInputs.sitcaClientFilePartRetrievalURI +
                    SITCAFTClientInputs.sitcaTransferFileName + "/NumberOfFileParts";

                HttpResponseMessage httpResponseMesssage = await httpSitcaClient.GetAsync(fileContentsNoOfPartsURI);

                // Retrieve Number of Parts from the client

                if (httpResponseMesssage.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Number of File Part contents = ");

                    string httpResponseContent = await httpResponseMesssage.Content.ReadAsStringAsync();
                    Console.WriteLine(httpResponseContent);

                    Console.WriteLine(httpResponseContent[0] + "." + httpResponseContent[1] + "." +
                        httpResponseContent[2]);

                    numberOfFileParts = retrieveNumberOfFilePartsFromHTTPResponse(httpResponseContent);
                    Console.WriteLine("Number of File Parts = " + numberOfFileParts);

                }

                else
                {
                    Console.WriteLine("Error Response While Retrieving File Contents : Number of Parts = " + httpResponseMesssage.StatusCode);
                    throw new ArgumentException("Error occured while retrieving file contents : Number of Parts");
                }

                // Create File and Fill it up

                int totalFileSize = numberOfFileParts * SITCAFTClientInputs.chunkSize;

                fileDestination = File.Create(SITCAFTClientInputs.fileDestinationDir + SITCAFTClientInputs.sitcaTransferFileName,
                    totalFileSize, FileOptions.RandomAccess);


                // Retrieve data for each part from file part queries

                string fileContentsRetrievePartURI = SITCAFTClientInputs.sitcaClientFilePartRetrievalURI +
                    SITCAFTClientInputs.sitcaTransferFileName + "/File-Part-";

                for (int i = 0; i < numberOfFileParts; i++)
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

                        httpProcessedResponse = httpProcessedResponse.Replace("\\r\\n", "\n");

                        Console.WriteLine("httpResponseContent after response being processed and after replacement : " + httpProcessedResponse);

                        for (int j = 0; j < httpProcessedResponse.Length; j++)
                        {
                            Console.WriteLine("Letter No : " + j + " ,Char value = " + (char)httpProcessedResponse[j] +
                                "Byte Value : " + httpProcessedResponse[j] + " ,int value = " + (int)httpProcessedResponse[j]);

                        }

                        Console.WriteLine("");

                        byte[] httpProcessedResponseByteArray = new byte[httpProcessedResponse.Length];
                        
                        for( int j = 0; j < httpProcessedResponseByteArray.Length; j++)
                        {
                            httpProcessedResponseByteArray[j] = (byte)httpProcessedResponse[j];
                        }

                        /*
                        byte[] httpProcessedResponseByteArray = new byte[SITCAFTClientInputs.chunkSize];

                        httpResponseContent.Read(httpProcessedResponseByteArray);
                        */

                        fileDestination.Write(httpProcessedResponseByteArray); //, currentOffset, SITCAFTClientInputs.chunkSize);
                        currentOffset += SITCAFTClientInputs.chunkSize;
                    }

                    else
                    {
                        Console.WriteLine("Error Response While Retrieving File Contents = " + httpResponseMesssage.StatusCode);
                        throw new ArgumentException("Error occured while retrieving file contents");
                    }

                    Console.WriteLine("=========================================================================");
                }

                returnValue = (int)httpResponseMesssage.StatusCode;

            }
            catch (Exception e)
            {

                Console.WriteLine("Exception occured while retrieving the input file contents : " + inputFileName +
                    " , Message  = " + e.Message);

                returnValue = -1;
            }

            fileDestination.Close();
            return returnValue;

        }


        static public int retrieveNumberOfFilePartsFromHTTPResponse(string httpResponseContent)
        {
            int numberOfFileParts = 0;

            try
            {

                for (int i = 0; i < httpResponseContent.Length; i++)
                {
                    if (httpResponseContent[i] == '"')
                    {
                        continue;
                    }

                    numberOfFileParts = numberOfFileParts * 10 + (httpResponseContent[i] - '0');
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("Exception occured while retrieving number of File Parts : " + e.Message);

                return 0;
            }

            return numberOfFileParts;

        }

    }
}

