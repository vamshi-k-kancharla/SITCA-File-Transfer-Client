using System;
using System.IO;
using System.Net;
using System.Net.Http;


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

    }

}

