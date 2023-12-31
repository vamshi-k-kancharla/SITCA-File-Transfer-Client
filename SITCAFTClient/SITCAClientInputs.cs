﻿
namespace SITCAFileTransferClient
{
    class SITCAFTClientInputs
    {

        public static string fileDestinationDir = "I:/SITCA File Transfer/SITCA Web Service/DestinationDirectory/";

        public static string sitcaClientLoadFileURI = "https://localhost:7199/FileTransfer/LoadFile/";

        public static string sitcaClientFilePartRetrievalURI = "https://localhost:7199/FileTransfer/GetFilePartData/";

        public static bool bDebugFlag = false;

        public static bool bFirstLevelDebugFlag = false;

        public static Mutex writeThreadSyncMutex = new Mutex();



        public static string sitcaTransferFileName = "2GB.bin";

        public static int chunkSize = 10000000;

        public static long fileSize = 2000000000;

        public static long numberOfThreads = 40;

    }

    class SITCAThreadParameters
    {
        public string inputFileName;

        public long startFilePart;

        public long numOfPartsForAThread;

        public long numberOfFileParts;

        public FileStream fileDestination;

    }
        
}

