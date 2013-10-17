// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.Sync
{
    using System;
    using System.Collections.Generic;
    using Internal.Common;
    using Microsoft.WindowsAzure.Storage;
    using Sync.Upload;
    using Tools.Vhd.Model;

    public class Program
    {
        static public ISyncOutputEvents SyncOutput
        {
            get
            {
                return RawEvents;
            } 
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                RawEvents = value;
            }
        }
//        static public IProgramConfiguration Configuration { get; private set; }
        private static ISyncOutputEvents RawEvents = new SyncOutputEvents();
    }

    public interface ISyncOutputEvents
    {
        void MessageCreatingNewPageBlob(long pageBlobSize);
        void MessageResumingUpload();
        void ErrorUploadFailedWithExceptions(IList<Exception> exjceptions);


        void ProgressCopyComplete(TimeSpan elapsed);
        void ProgressCopyStatus(double precentComplete, double avgThroughputMbps, TimeSpan remainingTime);
        void ProgressCopyStatus(ProgressRecord record);

        void ProgressUploadStatus(ProgressRecord record);
        void ProgressUploadStatus(double precentComplete, double avgThroughputMbps, TimeSpan remainingTime);
        void ProgressUploadComplete(TimeSpan elapsed);

        void ProgressDownloadStatus(ProgressRecord record);
        void ProgressDownloadStatus(double precentComplete, double avgThroughputMbps, TimeSpan remainingTime);
        void ProgressDownloadComplete(TimeSpan elapsed);

        void ProgressOperationStatus(ProgressRecord record);
        void ProgressOperationStatus(double percentComplete, double avgThroughputMbps, TimeSpan remainingTime);
        void ProgressOperationComplete(TimeSpan elapsed);

        void MessageCalculatingMD5Hash(string filePath);
        void MessageMD5HashCalculationFinished();

        void MessageRetryingAfterANetworkDisruption();
        void DebugRetryingAfterException(Exception lastException);

        void MessageDetectingActualDataBlocks();
        void MessageDetectingActualDataBlocksCompleted();
        void MessagePrintBlockRange(IndexRange range);
        void DebugEmptyBlockDetected(IndexRange range);
        void ProgressEmptyBlockDetection(int processedRangeCount, int totalRangeCount);

        void WriteVerboseWithTimestamp(string message, params object[] args);
    }

    internal class SyncOutputEvents : ConsoleApplicationStandardOutputEvents, ISyncOutputEvents
    {
        public void ProgressUploadStatus(ProgressRecord record)
        {
            ProgressUploadStatus(record.PercentComplete, record.AvgThroughputMbPerSecond, record.RemainingTime);
        }

        private string FormatDuration(TimeSpan ts)
        {
            if (ts.Days == 0)
            {
                return String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            }
            else
            {
                return String.Format("{0} days {1:00}:{2:00}:{3:00}", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
            }
        }
        public void ProgressUploadStatus(double precentComplete, double avgThroughputMbps, TimeSpan remainingTime)
        {
            LogProgress("Uploading: {0:0.0}% complete; Remaining Time: {1}; Throughput: {2:0.0}Mbps",
                precentComplete,
                FormatDuration(remainingTime),
                avgThroughputMbps);
        }

        public void MessageCreatingNewPageBlob(long pageBlobSize)
        {
            LogMessage("Creating new page blob of size {0}...", pageBlobSize);
        }

        public void MessageResumingUpload()
        {
            LogMessage("Found existing page blob. Resuming upload...");
        }

        public void ProgressUploadComplete(TimeSpan elapsed)
        {
            LogProgress("");
            LogMessage("Elapsed time for upload: {0}", 
                FormatDuration(elapsed));
        }

        public void ProgressDownloadStatus(ProgressRecord record)
        {
            ProgressDownloadStatus(record.PercentComplete, record.AvgThroughputMbPerSecond, record.RemainingTime);
        }

        public void ProgressDownloadStatus(double precentComplete, double avgThroughputMbps, TimeSpan remainingTime)
        {
            LogProgress("Downloading: {0:0.0}% complete; Remaining Time: {1}; Throughput: {2:0.0}Mbps",
                precentComplete,
                FormatDuration(remainingTime),
                avgThroughputMbps);
        }

        public void ProgressDownloadComplete(TimeSpan elapsed)
        {
            LogProgress("");
            LogMessage("Elapsed time for download: {0}",
                FormatDuration(elapsed));
        }

        public void ProgressOperationStatus(ProgressRecord record)
        {
            ProgressOperationStatus(record.PercentComplete, record.AvgThroughputMbPerSecond, record.RemainingTime);
        }

        public void ProgressOperationStatus(double percentComplete, double avgThroughputMbps, TimeSpan remainingTime)
        {
            LogProgress("Progressing: {0:0.0}% complete; Remaining Time: {1}; Throughput: {2:0.0}Mbps",
                percentComplete,
                FormatDuration(remainingTime),
                avgThroughputMbps);
        }

        public void ProgressOperationComplete(TimeSpan elapsed)
        {
            LogProgress("");
            LogMessage("Elapsed time for the operation: {0}",
                FormatDuration(elapsed));
        }

        public void ProgressCopyStatus(ProgressRecord record)
        {
            ProgressCopyStatus(record.PercentComplete, record.AvgThroughputMbPerSecond, record.RemainingTime);
        }

        public void ProgressCopyComplete(TimeSpan elapsed)
        {
            LogProgress("");
            LogMessage("Elapsed time for the copy: {0}",
                FormatDuration(elapsed));
        }

        public void ProgressCopyStatus(double precentComplete, double avgThroughputMbps, TimeSpan remainingTime)
        {
            LogProgress("Progressing: {0:0.0}% complete; Remaining Time: {1}; Throughput: {2:0.0}Mbps",
                precentComplete,
                FormatDuration(remainingTime),
                avgThroughputMbps);
        }

        public void ErrorUploadFailedWithExceptions(IList<Exception> exceptions)
        {
            LogError("Upload failed with exceptions:");
            foreach (var exception in exceptions)
            {
                LogError(exception.ToString());
            }
        }

        public void MessageCalculatingMD5Hash(string filePath)
        {
            LogMessage("MD5 hash is being calculated for the file '{0}'.", filePath);
        }

        public void MessageMD5HashCalculationFinished()
        {
            LogMessage("MD5 hash calculation is completed.");
        }

        public void MessageRetryingAfterANetworkDisruption()
        {
            LogMessage("Network disruption occured, retrying.");
        }

        public void DebugRetryingAfterException(Exception lastException)
        {
            LogDebug(lastException.ToString());
            
            var storageException = lastException as StorageException;
            var message = ExceptionUtil.DumpStorageExceptionErrorDetails(storageException);
            if(message != String.Empty)
            {
                LogDebug(message);
            }
        }

        public void MessageDetectingActualDataBlocks()
        {
            LogMessage("Detecting the empty data blocks in the local file.");
        }

        public void MessageDetectingActualDataBlocksCompleted()
        {
            LogMessage("Detecting the empty data blocks completed.");
        }

        public void MessagePrintBlockRange(IndexRange range)
        {
            LogMessage("Range of the block is {0}, Length: {1}", range, range.Length);
        }

        public void DebugEmptyBlockDetected(IndexRange range)
        {
            LogDebug("Empty block detected: {0}", range.ToString());
        }

        public void ProgressEmptyBlockDetection(int processedRangeCount, int totalRangeCount)
        {
            if (processedRangeCount >= totalRangeCount)
            {
                LogProgress("");
                LogMessage("");
                return;
            }
            LogProgress("Total block count: {0}, Processed block count: {1}", totalRangeCount, processedRangeCount);
        }

        public void WriteVerboseWithTimestamp(string message, params object[] args)
        {
            LogMessage(message, args);
        }
    }
}
