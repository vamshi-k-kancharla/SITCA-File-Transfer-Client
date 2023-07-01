using System;
using System.IO;

namespace SITCAFileTransferClient
{
    class SITCAFileTransferMainClient
    {

        static async Task<int> Main(string[] args)
        {

            Console.WriteLine("Hello, World! Again");

            int httpStatusCodeResponse = await SITCAFTClient.RetrieveAFile(SITCAFTClientInputs.sitcaTransferFileName);
            Console.WriteLine("Received the responseCode for loading a file = " + httpStatusCodeResponse);
            
            return 1;
        }

    }

}

