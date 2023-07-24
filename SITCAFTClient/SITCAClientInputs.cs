
using System.Diagnostics;

namespace SITCAFileTransferClient
{
    class SITCAFTClientInputs
    {

        public static string fileDestinationDir = "I:/SITCA File Transfer/SITCA Web Service/DestinationDirectory/";

        public static int noOfFileTransferThreads = 100;

        public static string sitcaClientLoadFileURI = "https://localhost:7199/FileTransfer/LoadFile/";

        public static string sitcaClientFilePartRetrievalURI = "https://localhost:7199/FileTransfer/GetFilePartData/";

        public static string sitcaTransferFileName = "SITCAInputFile.txt";

        public static int chunkSize = 30;

        public static bool bDebugFlag = false;

    }
}

