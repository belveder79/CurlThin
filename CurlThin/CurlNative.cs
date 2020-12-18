using System;
using System.Runtime.InteropServices;
using CurlThin.Enums;
using CurlThin.SafeHandles;

#if UNITY
using System.Text;
using System.IO;
using UnityEngine;
#endif
namespace CurlThin
{
    /// <remarks>
    ///     Type mappings (C -> C#):
    ///     - size_t -> UIntPtr
    ///     - int    -> int
    ///     - long   -> int
    /// </remarks>
    public static class CurlNative
    {
#if !UNITY
        private const string LIBCURL = "libcurl";

        [DllImport(LIBCURL, EntryPoint = "curl_global_init")]
        public static extern CURLcode Init(CURLglobal flags = CURLglobal.DEFAULT);

        [DllImport(LIBCURL, EntryPoint = "curl_global_cleanup")]
        public static extern void Cleanup();

        public static class Easy
        {
            public delegate UIntPtr DataHandler(IntPtr data, UIntPtr size, UIntPtr nmemb, IntPtr userdata);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_init")]
            public static extern SafeEasyHandle Init();

            [DllImport(LIBCURL, EntryPoint = "curl_easy_cleanup")]
            public static extern void Cleanup(IntPtr handle);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_perform")]
            public static extern CURLcode Perform(SafeEasyHandle handle);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_reset")]
            public static extern void Reset(SafeEasyHandle handle);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, int value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, IntPtr value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_setopt", CharSet = CharSet.Ansi)]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, string value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, DataHandler value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_getinfo")]
            public static extern CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, out int value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_getinfo")]
            public static extern CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, out IntPtr value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_getinfo")]
            public static extern CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, out double value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_getinfo", CharSet = CharSet.Ansi)]
            public static extern CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, IntPtr value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_strerror")]
            public static extern IntPtr StrError(CURLcode errornum);
        }

        public static class Multi
        {
            [DllImport(LIBCURL, EntryPoint = "curl_multi_init")]
            public static extern SafeMultiHandle Init();

            [DllImport(LIBCURL, EntryPoint = "curl_multi_cleanup")]
            public static extern CURLMcode Cleanup(IntPtr multiHandle);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_add_handle")]
            public static extern CURLMcode AddHandle(SafeMultiHandle multiHandle, SafeEasyHandle easyHandle);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_remove_handle")]
            public static extern CURLMcode RemoveHandle(SafeMultiHandle multiHandle, SafeEasyHandle easyHandle);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_setopt")]
            public static extern CURLMcode SetOpt(SafeMultiHandle multiHandle, CURLMoption option, int value);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_info_read")]
            public static extern IntPtr InfoRead(SafeMultiHandle multiHandle, out int msgsInQueue);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_socket_action")]
            public static extern CURLMcode SocketAction(SafeMultiHandle multiHandle, SafeSocketHandle sockfd,
                CURLcselect evBitmask,
                out int runningHandles);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct CURLMsg
            {
                public CURLMSG msg; /* what this message means */
                public IntPtr easy_handle; /* the handle it concerns */
                public CURLMsgData data;

                [StructLayout(LayoutKind.Explicit)]
                public struct CURLMsgData
                {
                    [FieldOffset(0)] public IntPtr whatever; /* (void*) message-specific data */
                    [FieldOffset(0)] public CURLcode result; /* return code for transfer */
                }
            }

        #region curl_multi_setopt for CURLMOPT_TIMERFUNCTION

            public delegate int TimerCallback(IntPtr multiHandle, int timeoutMs, IntPtr userp);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_setopt")]
            public static extern CURLMcode SetOpt(SafeMultiHandle multiHandle, CURLMoption option, TimerCallback value);

        #endregion

        #region curl_multi_setopt for CURLMOPT_SOCKETFUNCTION

            public delegate int SocketCallback(IntPtr easy, IntPtr s, CURLpoll what, IntPtr userp, IntPtr socketp);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_setopt")]
            public static extern CURLMcode SetOpt(SafeMultiHandle multiHandle, CURLMoption option,
                SocketCallback value);

        #endregion
        }

        public static class Slist
        {
            [DllImport(LIBCURL, EntryPoint = "curl_slist_append")]
            public static extern SafeSlistHandle Append(SafeSlistHandle slist, string data);
            
            [DllImport(LIBCURL, EntryPoint = "curl_slist_free_all")]
            public static extern void FreeAll(SafeSlistHandle pList);
        }
#else
#if UNITY_STANDALONE_OSX

        public const int RTLD_LAZY = 0x0001;
        public const int RTLD_NOW = 0x0002;
        public const int RTLD_GLOBAL = 0x0100;
        public const int RTLD_LOCAL = 0x0000;
        public const int RTLD_NOSHARE = 0x1000;
        public const int RTLD_EXE = 0x2000;
        public const int RTLD_SCRIPT = 0x4000;

        [DllImport("__Internal")]
        public static extern IntPtr dlopen(string filename, int flag);

        [DllImport("__Internal")]
        public static extern string dlerror();

        [DllImport("__Internal")]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("__Internal")]
        public static extern int dlclose(IntPtr handle);

        // function delegates
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLcode CURLcode_CURLglobal_Delegate(CURLglobal flags);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Void_Void_Delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate SafeEasyHandle SafeEasyHandle_Void_Delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Void_IntPtr_Delegate(IntPtr p);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLcode CURLcode_SafeEasyHandle_Delegate(SafeEasyHandle d);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLcode CURLcode_SafeEasyHandle_CURLoption_Int_Delegate(SafeEasyHandle handle, CURLoption option, int value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLcode CURLcode_SafeEasyHandle_CURLoption_IntPtr_Delegate(SafeEasyHandle handle, CURLoption option, IntPtr value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate CURLcode CURLcode_SafeEasyHandle_CURLoption_String_Delegate(SafeEasyHandle handle, CURLoption option, string value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLcode CURLcode_SafeEasyHandle_CURLoption_DataHandler_Delegate(SafeEasyHandle handle, CURLoption option, Easy.DataHandler value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLcode CURLcode_SafeEasyHandle_CURLINFO_OutInt_Delegate(SafeEasyHandle handle, CURLINFO option, out int value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLcode CURLcode_SafeEasyHandle_CURLINFO_OutIntPtr_Delegate(SafeEasyHandle handle, CURLINFO option, out IntPtr value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLcode CURLcode_SafeEasyHandle_CURLINFO_OutDouble_Delegate(SafeEasyHandle handle, CURLINFO option, out double value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate CURLcode CURLcode_SafeEasyHandle_CURLINFO_IntPtr_Delegate(SafeEasyHandle handle, CURLINFO option, IntPtr value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr IntPtr_CURLcode_Delegate(CURLcode errornum);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Void_SafeEasyHandle_Delegate(SafeEasyHandle e);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate SafeMultiHandle SafeMultiHandle_Void_Delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLMcode CURLMcode_IntPtr_Delegate(IntPtr multiHandle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLMcode CurlMcode_SafeMultiHandle_SafeEasyHandle_Delegate(SafeMultiHandle multiHandle, SafeEasyHandle easyHandle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLMcode CurlMcode_SafeMultiHandle_CURLMoption_Int_Delegate(SafeMultiHandle multiHandle,
            CURLMoption option, int value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr IntPtr_SafeMultiHandle_OutInt_Delegate (SafeMultiHandle multiHandle, out int msgsInQueue);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLMcode CURLMcode_SafeMultiHandle_SafeSocketHandle_CURLcselect_OutInt_Delegate(SafeMultiHandle multiHandle,
            SafeSocketHandle sockfd, CURLcselect evBitmask, out int runningHandles);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLMcode CURLMcode_SafeMultiHandle_CURLMcode_TimerCallback_Delegate(SafeMultiHandle multiHandle,
            CURLMoption option, Multi.TimerCallback value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate CURLMcode CURLMcode_SafeMultiHandle_CURLMcode_SocketCallback_Delegate(SafeMultiHandle multiHandle,
            CURLMoption option, Multi.SocketCallback value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate SafeSlistHandle SafeSlistHandle_SafeSlistHandle_String_Delegate(SafeSlistHandle slist, string data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Void_SafeSlistHandle(SafeSlistHandle pList);

        // fn delegates end

        private static IntPtr handle = IntPtr.Zero;

        private static bool IsInitialized = false;

        private static T GetFunctionPtr<T>(string name) where T : class
        {
            return Marshal.GetDelegateForFunctionPointer(dlsym(handle, name), typeof(T)) as T;
        }

        private static bool InitPlugin()
        {
            StringBuilder dllname = new StringBuilder();
            string extension = "dylib";
            string prefix = "lib";
            FileInfo[] fileList = new DirectoryInfo(Application.dataPath).GetFiles(prefix + "curl." + extension, SearchOption.AllDirectories);
            if (fileList.Length > 0)
                dllname.Append(fileList[0].DirectoryName + "/" + prefix + "curl." + extension);

            handle = dlopen(dllname.ToString(), RTLD_LAZY);
            bool initOK = false;
            if (handle != IntPtr.Zero)
            {
                // GLOBAL
                Init = GetFunctionPtr<CURLcode_CURLglobal_Delegate>("curl_global_init");
                Cleanup = GetFunctionPtr<Void_Void_Delegate>("curl_global_cleanup");
                // EASY
                Easy.Init = GetFunctionPtr<SafeEasyHandle_Void_Delegate>("curl_easy_init");
                Easy.Cleanup = GetFunctionPtr<Void_IntPtr_Delegate>("curl_easy_cleanup");
                Easy.Perform = GetFunctionPtr<CURLcode_SafeEasyHandle_Delegate>("curl_easy_perform");
                Easy.Reset = GetFunctionPtr<Void_SafeEasyHandle_Delegate>("curl_easy_reset");

                Easy.SetOpt1 = GetFunctionPtr <CURLcode_SafeEasyHandle_CURLoption_Int_Delegate>("curl_easy_setopt");
                Easy.SetOpt2 = GetFunctionPtr <CURLcode_SafeEasyHandle_CURLoption_IntPtr_Delegate>("curl_easy_setopt");
                Easy.SetOpt3 = GetFunctionPtr <CURLcode_SafeEasyHandle_CURLoption_String_Delegate>("curl_easy_setopt");
                Easy.SetOpt4 = GetFunctionPtr <CURLcode_SafeEasyHandle_CURLoption_DataHandler_Delegate>("curl_easy_setopt");

                Easy.GetInfo1 = GetFunctionPtr <CURLcode_SafeEasyHandle_CURLINFO_OutInt_Delegate>("curl_easy_getinfo");
                Easy.GetInfo2 = GetFunctionPtr <CURLcode_SafeEasyHandle_CURLINFO_OutIntPtr_Delegate>("curl_easy_getinfo");
                Easy.GetInfo3 = GetFunctionPtr <CURLcode_SafeEasyHandle_CURLINFO_OutDouble_Delegate>("curl_easy_getinfo");
                Easy.GetInfo4 = GetFunctionPtr <CURLcode_SafeEasyHandle_CURLINFO_IntPtr_Delegate>("curl_easy_getinfo");

                Easy.StrError = GetFunctionPtr<IntPtr_CURLcode_Delegate>("curl_easy_strerror");

                // MULTI
                Multi.Init = GetFunctionPtr<SafeMultiHandle_Void_Delegate>("curl_multi_init");
                Multi.Cleanup = GetFunctionPtr<CURLMcode_IntPtr_Delegate>("curl_multi_cleanup");
                Multi.AddHandle = GetFunctionPtr <CurlMcode_SafeMultiHandle_SafeEasyHandle_Delegate>("curl_multi_add_handle");
                Multi.RemoveHandle = GetFunctionPtr <CurlMcode_SafeMultiHandle_SafeEasyHandle_Delegate>("curl_multi_remove_handle");
                Multi.SetOptI = GetFunctionPtr <CurlMcode_SafeMultiHandle_CURLMoption_Int_Delegate>("curl_multi_setopt");
                Multi.InfoRead = GetFunctionPtr <IntPtr_SafeMultiHandle_OutInt_Delegate>("curl_multi_info_read");
                Multi.SocketAction = GetFunctionPtr <CURLMcode_SafeMultiHandle_SafeSocketHandle_CURLcselect_OutInt_Delegate>("curl_multi_socket_action");

                // NOT SURE IF THIS ACTUALLY WORKS!!!
                Multi.SetOptT = GetFunctionPtr<CURLMcode_SafeMultiHandle_CURLMcode_TimerCallback_Delegate > ("curl_multi_setopt");
                Multi.SetOptS = GetFunctionPtr<CURLMcode_SafeMultiHandle_CURLMcode_SocketCallback_Delegate > ("curl_multi_setopt");

                // SLIST
                Slist.Append = GetFunctionPtr<SafeSlistHandle_SafeSlistHandle_String_Delegate>("curl_slist_append");
                Slist.FreeAll = GetFunctionPtr<Void_SafeSlistHandle>("curl_slist_free_all");

                if (Init != null)
                    initOK = true;
            }
            else
            {
                Debug.Log("InitPlugin failed! " + dlerror());
            }
            return initOK;
        }

        private static void DeinitPlugin()
        {
            if (handle != IntPtr.Zero)
            {
                dlclose(handle);
                dlclose(handle); // needs twice for some mysterious reason!
                handle = IntPtr.Zero;
                Init = null;
                Cleanup = null;
                Easy.Init = null;
                Easy.Reset = null;
                Easy.Perform = null;
                Easy.Cleanup = null;

                Easy.SetOpt1 = null;
                Easy.SetOpt2 = null;
                Easy.SetOpt3 = null;
                Easy.SetOpt4 = null;

                Easy.GetInfo1 = null;
                Easy.GetInfo2 = null;
                Easy.GetInfo3 = null;
                Easy.GetInfo4 = null;

                Easy.StrError = null;

                Multi.Init = null;
                Multi.Cleanup = null;
                Multi.AddHandle = null;
                Multi.RemoveHandle = null;
                Multi.SetOptI = null;
                Multi.InfoRead = null;
                Multi.SocketAction = null;

                Multi.SetOptS = null;
                Multi.SetOptT = null;

                Slist.Append = null;
                Slist.FreeAll = null;
            }
            else
            {
                Debug.Log("DeinitPlugin failed: " + dlerror());
            }
        }

        public static void Curl_initializeImpl()
        {
            if (!IsInitialized)
            {
                IsInitialized = InitPlugin();
            }
        }
        public static void Curl_destroyImpl()
        {
            if (IsInitialized)
            {
                DeinitPlugin();
                IsInitialized = false;
            }
        }

        public static CURLcode_CURLglobal_Delegate Init = null;
        public static Void_Void_Delegate Cleanup = null;

        public static class Easy
        {
            public delegate UIntPtr DataHandler(IntPtr data, UIntPtr size, UIntPtr nmemb, IntPtr userdata);

            public static SafeEasyHandle_Void_Delegate Init = null;
            public static CURLcode_SafeEasyHandle_Delegate Perform = null;
            public static Void_SafeEasyHandle_Delegate Reset = null;
            public static Void_IntPtr_Delegate Cleanup = null;

            public static CURLcode_SafeEasyHandle_CURLoption_Int_Delegate SetOpt1 = null;
            public static CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, int value)
            {
                return SetOpt1(handle, option, value);
            }

            public static CURLcode_SafeEasyHandle_CURLoption_IntPtr_Delegate SetOpt2 = null;
            public static CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, IntPtr value)
            {
                return SetOpt2(handle, option, value);
            }
            public static CURLcode_SafeEasyHandle_CURLoption_String_Delegate SetOpt3 = null;
            public static CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, string value)
            {
                return SetOpt3(handle, option, value);
            }
            public static CURLcode_SafeEasyHandle_CURLoption_DataHandler_Delegate SetOpt4 = null;
            public static CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, DataHandler value)
            {
                return SetOpt4(handle, option, value);
            }

            public static CURLcode_SafeEasyHandle_CURLINFO_OutInt_Delegate GetInfo1 = null;
            public static CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, out int value)
            {
                return GetInfo1(handle, option, out value);
            }

            public static CURLcode_SafeEasyHandle_CURLINFO_OutIntPtr_Delegate GetInfo2 = null;
            public static CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, out IntPtr value)
            {
                return GetInfo2(handle, option, out value);
            }

            public static CURLcode_SafeEasyHandle_CURLINFO_OutDouble_Delegate GetInfo3 = null;
            public static CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, out double value)
            {
                return GetInfo3(handle, option, out value);
            }

            public static CURLcode_SafeEasyHandle_CURLINFO_IntPtr_Delegate GetInfo4 = null;
            public static CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, IntPtr value)
            {
                return GetInfo4(handle, option, value);
            }

            public static IntPtr_CURLcode_Delegate StrError = null;
        }
        public static class Multi
        {
            public static SafeMultiHandle_Void_Delegate Init = null;
            public static CURLMcode_IntPtr_Delegate Cleanup = null;
            public static CurlMcode_SafeMultiHandle_SafeEasyHandle_Delegate AddHandle = null;
            public static CurlMcode_SafeMultiHandle_SafeEasyHandle_Delegate RemoveHandle = null;

            public static CURLMcode SetOpt(SafeMultiHandle multiHandle, CURLMoption option, int value)
            {
                return Multi.SetOptI(multiHandle, option, value);
            }

            public static CurlMcode_SafeMultiHandle_CURLMoption_Int_Delegate SetOptI = null;
            public static IntPtr_SafeMultiHandle_OutInt_Delegate InfoRead = null;
            public static CURLMcode_SafeMultiHandle_SafeSocketHandle_CURLcselect_OutInt_Delegate SocketAction = null;

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct CURLMsg
            {
                public CURLMSG msg; /* what this message means */
                public IntPtr easy_handle; /* the handle it concerns */
                public CURLMsgData data;

                [StructLayout(LayoutKind.Explicit)]
                public struct CURLMsgData
                {
                    [FieldOffset(0)] public IntPtr whatever; /* (void*) message-specific data */
                    [FieldOffset(0)] public CURLcode result; /* return code for transfer */
                }
            }

#region curl_multi_setopt for CURLMOPT_TIMERFUNCTION
            public static CURLMcode SetOpt(SafeMultiHandle multiHandle,
                CURLMoption option, Multi.TimerCallback value)
            {
                return Multi.SetOptT(multiHandle, option, value);
            }
            public delegate int TimerCallback(IntPtr multiHandle, int timeoutMs, IntPtr userp);
            public static CURLMcode_SafeMultiHandle_CURLMcode_TimerCallback_Delegate SetOptT = null;
#endregion

#region curl_multi_setopt for CURLMOPT_SOCKETFUNCTION
            public static CURLMcode SetOpt(SafeMultiHandle multiHandle,
                CURLMoption option, Multi.SocketCallback value)
            {
                return Multi.SetOptS(multiHandle, option, value);
            }
            public delegate int SocketCallback(IntPtr easy, IntPtr s, CURLpoll what, IntPtr userp, IntPtr socketp);
            public static CURLMcode_SafeMultiHandle_CURLMcode_SocketCallback_Delegate SetOptS = null;
#endregion

        }
        public static class Slist
        {
            public static SafeSlistHandle_SafeSlistHandle_String_Delegate Append = null;
            public static Void_SafeSlistHandle FreeAll = null;
        }
#else
#if UNITY_IOS
            private const string LIBCURL = "__Internal";
#else
            private const string LIBCURL = "curl";
#endif

        [DllImport(LIBCURL, EntryPoint = "curl_global_init")]
        public static extern CURLcode Init(CURLglobal flags = CURLglobal.DEFAULT);

        [DllImport(LIBCURL, EntryPoint = "curl_global_cleanup")]
        public static extern void Cleanup();

        public static class Easy
        {
            public delegate UIntPtr DataHandler(IntPtr data, UIntPtr size, UIntPtr nmemb, IntPtr userdata);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_init")]
            public static extern SafeEasyHandle Init();

            [DllImport(LIBCURL, EntryPoint = "curl_easy_cleanup")]
            public static extern void Cleanup(IntPtr handle);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_perform")]
            public static extern CURLcode Perform(SafeEasyHandle handle);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_reset")]
            public static extern void Reset(SafeEasyHandle handle);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, int value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, IntPtr value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_setopt", CharSet = CharSet.Ansi)]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, string value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, DataHandler value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_getinfo")]
            public static extern CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, out int value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_getinfo")]
            public static extern CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, out IntPtr value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_getinfo")]
            public static extern CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, out double value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_getinfo", CharSet = CharSet.Ansi)]
            public static extern CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, IntPtr value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_strerror")]
            public static extern IntPtr StrError(CURLcode errornum);
        }

        public static class Multi
        {
            [DllImport(LIBCURL, EntryPoint = "curl_multi_init")]
            public static extern SafeMultiHandle Init();

            [DllImport(LIBCURL, EntryPoint = "curl_multi_cleanup")]
            public static extern CURLMcode Cleanup(IntPtr multiHandle);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_add_handle")]
            public static extern CURLMcode AddHandle(SafeMultiHandle multiHandle, SafeEasyHandle easyHandle);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_remove_handle")]
            public static extern CURLMcode RemoveHandle(SafeMultiHandle multiHandle, SafeEasyHandle easyHandle);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_setopt")]
            public static extern CURLMcode SetOpt(SafeMultiHandle multiHandle, CURLMoption option, int value);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_info_read")]
            public static extern IntPtr InfoRead(SafeMultiHandle multiHandle, out int msgsInQueue);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_socket_action")]
            public static extern CURLMcode SocketAction(SafeMultiHandle multiHandle, SafeSocketHandle sockfd,
                CURLcselect evBitmask,
                out int runningHandles);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct CURLMsg
            {
                public CURLMSG msg; /* what this message means */
                public IntPtr easy_handle; /* the handle it concerns */
                public CURLMsgData data;

                [StructLayout(LayoutKind.Explicit)]
                public struct CURLMsgData
                {
                    [FieldOffset(0)] public IntPtr whatever; /* (void*) message-specific data */
                    [FieldOffset(0)] public CURLcode result; /* return code for transfer */
                }
            }

        #region curl_multi_setopt for CURLMOPT_TIMERFUNCTION

            public delegate int TimerCallback(IntPtr multiHandle, int timeoutMs, IntPtr userp);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_setopt")]
            public static extern CURLMcode SetOpt(SafeMultiHandle multiHandle, CURLMoption option, TimerCallback value);

        #endregion

        #region curl_multi_setopt for CURLMOPT_SOCKETFUNCTION

            public delegate int SocketCallback(IntPtr easy, IntPtr s, CURLpoll what, IntPtr userp, IntPtr socketp);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_setopt")]
            public static extern CURLMcode SetOpt(SafeMultiHandle multiHandle, CURLMoption option,
                SocketCallback value);

        #endregion
        }

        public static class Slist
        {
            [DllImport(LIBCURL, EntryPoint = "curl_slist_append")]
            public static extern SafeSlistHandle Append(SafeSlistHandle slist, string data);

            [DllImport(LIBCURL, EntryPoint = "curl_slist_free_all")]
            public static extern void FreeAll(SafeSlistHandle pList);
        }
#endif  // UNITY_STANDALONE_OSX
#endif // !UNITY
    }
}