using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CurlThin.Helpers
{
    public class XferInfoCallback : IDisposable
    {
        public XferInfoCallback()
        {
            XferHandler = (client, dltotal, dlnow, ultotal, ulnow) =>
            {
                if ((double)ultotal != 0)
                    UploadProgress = (double)ulnow / (double)ultotal;
                if((double)dltotal != 0)
                    DownloadProgress = (double)dlnow / (double)dltotal;
                return (UIntPtr) 0;
            };
        }

        public void Reset()
        {
            UploadProgress = 0.0;
            DownloadProgress = 0.0;
        }

        public CurlNative.Easy.XferHandler XferHandler { get; }

        public void Dispose()
        {

        }
        public double UploadProgress { get; private set; }
        public double DownloadProgress { get; private set; }
    }
}