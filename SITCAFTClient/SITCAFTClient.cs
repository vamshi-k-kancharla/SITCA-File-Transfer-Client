using System;
using System.IO;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SITCAFileTransferClient
{
    class SITCAFTClient
    {
        public static FileStream fileDestination;

        /// <summary>
        /// Loads the input file contents into mongoDB through REST API.
        /// </summary>
        /// 
        /// <param name="inputFileName"> Name of the input file to load the contents from.</param>
        /// 
        /// <returns> An integer denoting the status code of REST API.</returns>

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

        /// <summary>
        /// Writes file contents retrieved from query to the destination file.
        /// </summary>
        /// 
        /// <param name="inputFileName"> Name of the input file to write the contents to.</param>
        /// 
        /// <returns> An integer denoting the number of file parts of the targeted file.</returns>

        static public async Task<int> WriteContentsToAFile(string inputFileName)
        {
            int returnValue = 0;
            FileStream fileDestination;

            try
            {
                int numberOfFileParts = 0;
                int currentOffset = 0;

                HttpClient httpSitcaClient = new HttpClient();
                HttpResponseMessage httpResponseMesssage = new HttpResponseMessage();

                // Create File and Fill it up

                numberOfFileParts = await retrieveNumberOfFilePartsThroughQuery();

                long totalFileSize = SITCAFTClientInputs.fileSize; //numberOfFileParts * SITCAFTClientInputs.chunkSize;

                fileDestination = File.Create(SITCAFTClientInputs.fileDestinationDir + SITCAFTClientInputs.sitcaTransferFileName,
                    (int)totalFileSize, FileOptions.RandomAccess);

                //int numberOfPartsInSubGroup = numberOfFileParts / SITCAFTClientInputs.numberOfFileWriteThreads;
                List<Thread> fileWriteThreads = new List<Thread>();

                /*

                for( int i = 0; i < SITCAFTClientInputs.numberOfFileWriteThreads; i++)
                {
                    int startPart = i * numberOfPartsInSubGroup;

                    Console.WriteLine(" start Part = " + startPart);

                    int numberOfSubGroupParts = 0;

                    if (i == SITCAFTClientInputs.numberOfFileWriteThreads-1)
                    {

                        numberOfSubGroupParts = ((numberOfFileParts % SITCAFTClientInputs.numberOfFileWriteThreads) == 0) ?
                            numberOfPartsInSubGroup : numberOfFileParts - ((SITCAFTClientInputs.numberOfFileWriteThreads - 1) *
                            numberOfPartsInSubGroup);

                    }
                    else
                    {
                        numberOfSubGroupParts = numberOfPartsInSubGroup;
                    }

                    Console.WriteLine(" numberOfSubGroupParts = " + numberOfSubGroupParts);

                */

                long numOfThreads = (SITCAFTClientInputs.fileSize % SITCAFTClientInputs.chunkSize == 0) ?
                    (SITCAFTClientInputs.fileSize / SITCAFTClientInputs.chunkSize) :
                    ((SITCAFTClientInputs.fileSize / SITCAFTClientInputs.chunkSize) + 1);

                long numberOfThreads = numOfThreads;

                if ( numberOfFileParts != numOfThreads )
                {

                    Console.WriteLine("Number of FileParts " + numberOfThreads + 
                        "retrieved from query doesn't match the thread count : " + numOfThreads + "exiting.."
                        + inputFileName );

                    throw new InvalidDataException("Number of FileParts retrieved from query doesn't match the thread count : exiting..");

                }

                for (long threadNum = 0; threadNum < numberOfThreads; threadNum++)
                {
                    SITCAThreadParameters currentParametersOfThread = new SITCAThreadParameters();

                    currentParametersOfThread.inputFileName = SITCAFTClientInputs.sitcaTransferFileName;
                    currentParametersOfThread.numberOfFileParts = numberOfFileParts;
                    currentParametersOfThread.fileDestination = fileDestination;
                    currentParametersOfThread.startFilePart = (int)threadNum;


                    Console.WriteLine("Thread is being fired with the following context => fileName = " + 
                        currentParametersOfThread.inputFileName +  " ,numberOfFileParts = " + numberOfFileParts);

                    Thread currentIterationThread = new Thread(SITCAClientThread.WriteContentsToTheFileThread);
                    currentIterationThread.Start(currentParametersOfThread);

                    fileWriteThreads.Add(currentIterationThread);
                }

                while(true)
                {
                    if ( AreAllThreadsStopped(fileWriteThreads) )
                    {
                        break;
                    }

                    Console.WriteLine(" Some of the file write threads are still running...Sleep for some time");

                    Thread.Sleep(2000);
                }

                fileDestination.Close();

                returnValue = 0;

                // Replace 'Space + newline" with "\n" by reading from and writing to output files.

            }
            catch (Exception e)
            {

                Console.WriteLine("Exception occured while retrieving the input file contents and while writing to file Stream : " + 
                    inputFileName + " , Message  = " + e.Message);

                returnValue = -1;
            }

            return returnValue;

        }

        /// <summary>
        /// Checks whether all the threads supplied to it are stopped.
        /// </summary>
        /// 
        /// <param name="fileWriteThreads"> List of all the threads whose status need to be checked for.</param>
        /// 
        /// <returns> A boolean with Yes ( for all stopped ) & No ( not all threads stopped ) values.</returns>

        static public bool AreAllThreadsStopped(List<Thread> fileWriteThreads)
        {
            int i = 0;

            for (; i < fileWriteThreads.Count; i++)
            {

                if (fileWriteThreads[i].ThreadState == ThreadState.Running)
                {
                    break;
                }
            }

            if (i == fileWriteThreads.Count)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves total number of file parts to get from the server using query.
        /// </summary>
        /// 
        /// 
        /// <returns> An integer denoting the number of file parts of the targeted file.</returns>

        static public async Task<int> retrieveNumberOfFilePartsThroughQuery()
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

                if (SITCAFTClientInputs.bDebugFlag)
                {
                    Console.WriteLine(httpResponseContent[0] + "." + httpResponseContent[1] + "." +
                        httpResponseContent[2]);
                }

                numberOfFileParts = retrieveNumberOfFilePartsFromHTTPResponse(httpResponseContent);
                Console.WriteLine("Number of File Parts = " + numberOfFileParts);
            }

            else
            {
                Console.WriteLine("Error Response While Retrieving File Contents : Number of Parts = " + httpResponseMesssage.StatusCode);
                throw new ArgumentException("Error occured while retrieving file contents : Number of Parts");
            }

            return numberOfFileParts;
        }

        /// <summary>
        /// Retrieves total number of file parts to get from the server.
        /// </summary>
        /// 
        /// <param name="httpResponseContent"> http query response containing file parts count data.</param>
        /// 
        /// <returns> An integer denoting the number of file parts of the targeted file.</returns>

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

