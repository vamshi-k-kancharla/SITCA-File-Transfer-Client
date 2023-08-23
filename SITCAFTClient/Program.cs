using System;
using System.IO;

namespace SITCAFileTransferClient
{
    class SITCAFileTransferMainClient
    {

        static async Task<int> Main(string[] args)
        {

            Console.WriteLine("Hello Start of File Transfer " + DateTime.Now);

            int httpStatusCodeResponse = await SITCAFTClient.RetrieveAFile(SITCAFTClientInputs.sitcaTransferFileName);
            Console.WriteLine(DateTime.Now + "Received the responseCode for loading/reading a file = " + 
                httpStatusCodeResponse);

            httpStatusCodeResponse = await SITCAFTClient.WriteContentsToAFile(SITCAFTClientInputs.sitcaTransferFileName);
            Console.WriteLine(DateTime.Now + "Received the responseCode for retrieving and writing the file contents to destination  = " + 
                httpStatusCodeResponse);

            return 1;
        }

    }

}

