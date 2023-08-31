using System.Net;

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

        static public void WriteContentsToTheFileThread(object threadStartParamObject)
        {
            FileStream fileDestination = null;

            try
            {
                SITCAThreadParameters threadParams = (SITCAThreadParameters)threadStartParamObject;

                string inputFileName = threadParams.inputFileName;
                long startFilePart = threadParams.startFilePart;
                long numberOfSubParts = threadParams.numberOfFileParts;
                long numberOfPartsPerThread = threadParams.numOfPartsForAThread;
                fileDestination = threadParams.fileDestination;

                HttpClient httpSitcaClient = new HttpClient();
                httpSitcaClient.Timeout = new TimeSpan(0, 4, 10);

                HttpResponseMessage httpResponseMesssage = new HttpResponseMessage();

                // Create File and Fill it up

                for (long currentThreadPart = startFilePart; currentThreadPart < startFilePart + numberOfPartsPerThread;
                    currentThreadPart++)
                {

                    long totalFileSize = numberOfSubParts * SITCAFTClientInputs.chunkSize;

                    // Retrieve data for each part from file part queries

                    string fileContentsRetrievePartURI = SITCAFTClientInputs.sitcaClientFilePartRetrievalURI +
                        SITCAFTClientInputs.sitcaTransferFileName + "/File-Part-";

                    if (SITCAFTClientInputs.bFirstLevelDebugFlag == true)
                    {

                        Console.WriteLine("=========================================================================");
                        Console.WriteLine("fileContentRetrievalURI = " + fileContentsRetrievePartURI + currentThreadPart);
                    }

                    string fileContentRetrievalURI = fileContentsRetrievePartURI + currentThreadPart;

                    httpResponseMesssage = Task.Run(() => httpSitcaClient.GetAsync(fileContentRetrievalURI)).Result;

                    if (httpResponseMesssage.StatusCode == HttpStatusCode.OK)
                    {

                        if (SITCAFTClientInputs.bFirstLevelDebugFlag == true)
                        {

                            Console.WriteLine("File contents of Part Num = " + currentThreadPart);
                        }

                        //string httpResponseContent = await httpResponseMesssage.Content.ReadAsStringAsync();

                        byte[] httpResponseContent = Task.Run(() => httpResponseMesssage.Content.ReadAsByteArrayAsync()).Result;
                        long currentWriteOffset = (currentThreadPart * SITCAFTClientInputs.chunkSize);

                        RandomAccess.Write(fileDestination.SafeFileHandle, httpResponseContent, currentWriteOffset);
                    }

                    else
                    {
                        Console.WriteLine("Error Response While Retrieving File Contents = " + httpResponseMesssage.StatusCode);
                        throw new ArgumentException("Error occured while retrieving file contents and writing to destination file");
                    }

                    if (SITCAFTClientInputs.bFirstLevelDebugFlag == true)
                    {

                        Console.WriteLine("=========================================================================");
                    }
                }

            }
            catch (Exception e)
            {

                Console.WriteLine("Exception occured while retrieving & writing the file contents : " + 
                    " , Message  = " + e.Message);
            }

        }

    }

}

