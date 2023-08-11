
namespace SITCAFileTransferClient
{
    class SITCAFTClientInputs
    {

        public static string fileDestinationDir = "I:/SITCA File Transfer/SITCA Web Service/DestinationDirectory/";

        public static int noOfFileTransferThreads = 1049;

        public static string sitcaClientLoadFileURI = "https://localhost:7199/FileTransfer/LoadFile/";

        public static string sitcaClientFilePartRetrievalURI = "https://localhost:7199/FileTransfer/GetFilePartData/";

        public static string sitcaTransferFileName = "1GB.bin";

        public static int chunkSize = 1000000;

        public static bool bDebugFlag = false;

        public static int  numberOfFileWriteThreads = 1049;

        public static Mutex writeThreadSyncMutex = new Mutex();

    }

    class SITCAThreadParameters
    {
        public string inputFileName;

        public int startFilePart;

        public int numOfPartsForAThread;

        public int numberOfFileParts;

        public FileStream fileDestination;

    }
        
}

