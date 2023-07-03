using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SITCAFileTransferClient
{
    class SITCAFTClient
    {

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
            catch(Exception e)
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
            try
            {
                int numberOfFileParts = 0;

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

                    numberOfFileParts = 0;

                    for( int i = 0; i < httpResponseContent.Length; i++)
                    {
                        if(httpResponseContent[i] == '"')
                        {
                            continue;
                        }

                        numberOfFileParts = numberOfFileParts * 10 + ( httpResponseContent[i] - '0' );

                    }

                    Console.WriteLine("Number of File Parts = " + numberOfFileParts);

                }

                else
                {
                    Console.WriteLine("Error Response While Retrieving File Contents : Number of Parts = " + httpResponseMesssage.StatusCode);
                    throw new ArgumentException("Error occured while retrieving file contents : Number of Parts");
                }

                // Retrieve Number of Parts from the client

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
                        Console.WriteLine(httpResponseContent);
                    }

                    else
                    {
                        Console.WriteLine("Error Response While Retrieving File Contents = " + httpResponseMesssage.StatusCode);
                        throw new ArgumentException("Error occured while retrieving file contents");
                    }

                    Console.WriteLine("=========================================================================");
                }

                return (int)httpResponseMesssage.StatusCode;

            }
            catch (Exception e)
            {

                Console.WriteLine("Exception occured while retrieving the input file contents : " + inputFileName +
                    " , Message  = " + e.Message);

                return -1;
            }

        }
    }

}

