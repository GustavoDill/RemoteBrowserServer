using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static EverythingApi.EverythingApiImport;
using static EverythingApi.EverythingFlags;
namespace EverythingApi
{
    public class EverythingQueryResult
    {
        private List<EverythingQueryResultItem> _items;

        public EverythingQueryResultItem[] Items
        {
            get { return _items.ToArray(); }
        }

        public uint TotalResults { get; }
        public uint TotalFileResults { get; }
        public uint TotalFolderResults { get; }
        public bool Success { get; }

        public EverythingQueryResult(bool success)
        {
            if (success)
            {
                _items = new List<EverythingQueryResultItem>();
                for (uint i = 0; i < EverythingApiImport.Everything_GetNumResults(); i++)
                {
                    var item = new EverythingQueryResultItem(i);
                    item.Attributes = EverythingApiImport.Everything_GetResultAttributes(i);
                    Everything_GetResultDateAccessed(i, out long dateAccessed);
                    Everything_GetResultDateCreated(i, out long dateCreated);
                    Everything_GetResultDateModified(i, out long dateModified);
                    Everything_GetResultDateRecentlyChanged(i, out long recentChange);
                    Everything_GetResultDateRun(i, out long dateRun);
                    StringBuilder fpath = null;
                    try
                    {
                        fpath = new StringBuilder(Everything.MaxPathSize);
                        Everything_GetResultFullPathName(i, fpath, (uint)Everything.MaxPathSize);
                    }
                    catch { }
                    var extension = Everything_GetResultExtension(i);
                    var fListfName = Everything_GetResultFileListFileName(i);
                    string fName = null;
                    try { fName = Marshal.PtrToStringUni(Everything_GetResultFileName(i)); } catch { }
                    Everything_GetResultSize(i, out long size);
                    TotalResults = Everything_GetTotResults();
                    TotalFileResults = Everything_GetTotFileResults();
                    TotalFolderResults = Everything_GetTotFolderResults();
                    if (dateAccessed == 0)
                        item.DateAccessed = DateTime.MinValue;
                    else
                        item.DateAccessed = DateTime.FromFileTime(dateAccessed);
                    if (dateCreated == 0)
                        item.DateCreated = DateTime.MinValue;
                    else
                        item.DateCreated = DateTime.FromFileTime(dateCreated);
                    if (dateModified == 0)
                        item.DateModified = DateTime.MinValue;
                    else
                        item.DateModified = DateTime.FromFileTime(dateModified);
                    if (recentChange == 0)
                        item.RecentlyChanged = DateTime.MinValue;
                    else
                        item.RecentlyChanged = DateTime.FromFileTime(recentChange);
                    if (dateRun == 0)
                        item.DateRun = DateTime.MinValue;
                    else
                        item.DateRun = DateTime.FromFileTime(dateRun);
                    if (extension != IntPtr.Zero)
                        item.Extension = Marshal.PtrToStringUni(extension);
                    item.FileListFileName = fListfName;
                    item.FileName = fName;
                    if (fpath != null)
                        item.FullPath = fpath.ToString();
                    _items.Add(item);
                }
            }
            Success = success;
        }
        public class EverythingQueryResultItem
        {
            public EverythingQueryResultItem(uint index)
            {
                Index = index;
            }
            public uint Attributes { get; set; }
            public uint Index { get; }
            public DateTime DateAccessed { get; set; }
            public DateTime DateCreated { get; internal set; }
            public DateTime DateModified { get; internal set; }
            public DateTime RecentlyChanged { get; internal set; }
            public DateTime DateRun { get; internal set; }
            public string FullPath { get; internal set; }
            public string Extension { get; internal set; }
            public IntPtr FileListFileName { get; internal set; }
            public string FileName { get; internal set; }
        }
    }
    public static class Everything
    {
        private static string _search;
        public static int MaxPathSize = 500;
        public static string Search
        {
            get { return _search; }
            set { _search = value; SetSearchW(Search); }
        }
        public static bool MatchPath { get => GetMatchPath(); set => SetMatchPath(value); }
        public static bool MatchCase { get => GetMatchCase(); set => SetMatchCase(value); }
        public static bool MatchWholeWord { get => GetMatchWholeWord(); set => SetMatchWholeWord(value); }
        public static bool Regex { get => GetRegex(); set => SetRegex(value); }
        public static uint Max { get => GetMax(); set => SetMax(value); }
        public static uint Offset { get => GetOffset(); set => SetOffset(value); }
        public static uint LastError { get => GetLastError(); }
        #region Properties
        private static UInt32 SetSearchW(string lpSearchString)
        {
            return EverythingApiImport.Everything_SetSearchW(lpSearchString);
        }
        private static void SetMatchPath(bool bEnable)
        {
            EverythingApiImport.Everything_SetMatchPath(bEnable);
        }
        private static void SetMatchCase(bool bEnable)
        {
            EverythingApiImport.Everything_SetMatchCase(bEnable);
        }
        private static void SetMatchWholeWord(bool bEnable)
        {
            EverythingApiImport.Everything_SetMatchWholeWord(bEnable);
        }
        private static void SetRegex(bool bEnable)
        {
            EverythingApiImport.Everything_SetRegex(bEnable);
        }
        private static void SetMax(UInt32 dwMax)
        {
            EverythingApiImport.Everything_SetMax(dwMax);
        }
        private static void SetOffset(UInt32 dwOffset)
        {
            EverythingApiImport.Everything_SetOffset(dwOffset);
        }

        private static bool GetMatchPath()
        {
            return EverythingApiImport.Everything_GetMatchPath();
        }
        private static bool GetMatchCase()
        {
            return EverythingApiImport.Everything_GetMatchCase();
        }
        private static bool GetMatchWholeWord()
        {
            return EverythingApiImport.Everything_GetMatchWholeWord();
        }
        private static bool GetRegex()
        {
            return EverythingApiImport.Everything_GetRegex();
        }
        private static UInt32 GetMax()
        {
            return EverythingApiImport.Everything_GetMax();
        }
        private static UInt32 GetOffset()
        {
            return EverythingApiImport.Everything_GetOffset();
        }
        private static IntPtr GetSearchW()
        {
            return EverythingApiImport.Everything_GetSearchW();
        }
        private static UInt32 GetLastError()
        {
            return EverythingApiImport.Everything_GetLastError();
        }
        #endregion
        public static bool QueryW(bool bWait)
        {
            return EverythingApiImport.Everything_QueryW(bWait);
        }

        public static void SortResultsByPath()
        {
            EverythingApiImport.Everything_SortResultsByPath();
        }
        public static EverythingQueryResult Query()
        {
            var qres = EverythingApiImport.Everything_QueryW(true);
            return new EverythingQueryResult(qres);
        }
        public static UInt32 GetNumFileResults()
        {
            return EverythingApiImport.Everything_GetNumFileResults();
        }
        public static UInt32 GetNumFolderResults()
        {
            return EverythingApiImport.Everything_GetNumFolderResults();
        }
        public static UInt32 GetNumResults()
        {
            return EverythingApiImport.Everything_GetNumResults();
        }
        public static UInt32 GetTotFileResults()
        {
            return EverythingApiImport.Everything_GetTotFileResults();
        }
        public static UInt32 GetTotFolderResults()
        {
            return EverythingApiImport.Everything_GetTotFolderResults();
        }
        public static UInt32 GetTotResults()
        {
            return EverythingApiImport.Everything_GetTotResults();
        }
        public static bool IsVolumeResult(UInt32 nIndex)
        {
            return EverythingApiImport.Everything_IsVolumeResult(nIndex);
        }
        public static bool IsFolderResult(UInt32 nIndex)
        {
            return EverythingApiImport.Everything_IsFolderResult(nIndex);
        }
        public static bool IsFileResult(UInt32 nIndex)
        {
            return EverythingApiImport.Everything_IsFileResult(nIndex);
        }
        public static void GetResultFullPathName(UInt32 nIndex, StringBuilder lpString, UInt32 nMaxCount)
        {
            EverythingApiImport.Everything_GetResultFullPathName(nIndex, lpString, nMaxCount);
        }
        public static void Reset()
        {
            EverythingApiImport.Everything_Reset();
        }

        public static IntPtr GetResultFileName(UInt32 nIndex)
        {
            return EverythingApiImport.Everything_GetResultFileName(nIndex);
        }

        // Everything 1.4
        public static SortFlags Sorting { get => GetSort(); set => SetSort(value); }
        private static void SetSort(SortFlags sortFlags)
        {
            EverythingApiImport.Everything_SetSort((uint)sortFlags);
        }
        private static SortFlags GetSort()
        {
            return (SortFlags)EverythingApiImport.Everything_GetSort();
        }
        public static UInt32 GetResultListSort()
        {
            return EverythingApiImport.Everything_GetResultListSort();
        }
        public static RequestFlags RequestFlags { get => GetRequestFlags(); set => SetRequestFlags(value); }
        private static void SetRequestFlags(RequestFlags dwRequestFlags)
        {
            EverythingApiImport.Everything_SetRequestFlags((uint)dwRequestFlags);
        }
        private static RequestFlags GetRequestFlags()
        {
            return (RequestFlags)EverythingApiImport.Everything_GetRequestFlags();
        }
        public static UInt32 GetResultListRequestFlags()
        {
            return EverythingApiImport.Everything_GetResultListRequestFlags();
        }
        public static IntPtr GetResultExtension(UInt32 nIndex)
        {
            return EverythingApiImport.Everything_GetResultExtension(nIndex);
        }
        public static bool GetResultSize(UInt32 nIndex, out long lpFileSize)
        {
            return EverythingApiImport.Everything_GetResultSize(nIndex, out lpFileSize);
        }
        public static bool GetResultDateCreated(UInt32 nIndex, out long lpFileTime)
        {
            return EverythingApiImport.Everything_GetResultDateCreated(nIndex, out lpFileTime);
        }
        public static bool GetResultDateModified(UInt32 nIndex, out long lpFileTime)
        {
            return EverythingApiImport.Everything_GetResultDateModified(nIndex, out lpFileTime);
        }
        public static bool GetResultDateAccessed(UInt32 nIndex, out long lpFileTime)
        {
            return EverythingApiImport.Everything_GetResultDateAccessed(nIndex, out lpFileTime);
        }
        public static UInt32 GetResultAttributes(UInt32 nIndex)
        {
            return EverythingApiImport.Everything_GetResultAttributes(nIndex);
        }
        public static IntPtr GetResultFileListFileName(UInt32 nIndex)
        {
            return EverythingApiImport.Everything_GetResultFileListFileName(nIndex);
        }
        public static UInt32 GetResultRunCount(UInt32 nIndex)
        {
            return EverythingApiImport.Everything_GetResultRunCount(nIndex);
        }
        public static bool GetResultDateRun(UInt32 nIndex, out long lpFileTime)
        {
            return EverythingApiImport.Everything_GetResultDateRun(nIndex, out lpFileTime);
        }
        public static bool GetResultDateRecentlyChanged(UInt32 nIndex, out long lpFileTime)
        {
            return EverythingApiImport.Everything_GetResultDateRecentlyChanged(nIndex, out lpFileTime);
        }
        public static IntPtr GetResultHighlightedFileName(UInt32 nIndex)
        {
            return EverythingApiImport.Everything_GetResultHighlightedFileName(nIndex);
        }
        public static IntPtr GetResultHighlightedPath(UInt32 nIndex)
        {
            return EverythingApiImport.Everything_GetResultHighlightedPath(nIndex);
        }
        public static IntPtr GetResultHighlightedFullPathAndFileName(UInt32 nIndex)
        {
            return EverythingApiImport.Everything_GetResultHighlightedFullPathAndFileName(nIndex);
        }
        public static UInt32 GetRunCountFromFileName(string lpFileName)
        {
            return EverythingApiImport.Everything_GetRunCountFromFileName(lpFileName);
        }
        public static bool SetRunCountFromFileName(string lpFileName, UInt32 dwRunCount)
        {
            return EverythingApiImport.Everything_SetRunCountFromFileName(lpFileName, dwRunCount);
        }
        public static UInt32 IncRunCountFromFileName(string lpFileName)
        {
            return EverythingApiImport.Everything_IncRunCountFromFileName(lpFileName);
        }
    }
    public class EverythingFlags
    {
        public enum RequestResultFlags
        {
            OK = 0,
            ERROR_MEMORY = 1,
            ERROR_IPC = 2,
            ERROR_REGISTERCLASSEX = 3,
            ERROR_CREATEWINDOW = 4,
            ERROR_CREATETHREAD = 5,
            ERROR_INVALIDINDEX = 6,
            ERROR_INVALIDCALL = 7,
        }
        public enum RequestFlags
        {
            REQUEST_FILE_NAME = 0x00000001,
            REQUEST_PATH = 0x00000002,
            REQUEST_FULL_PATH_AND_FILE_NAME = 0x00000004,
            REQUEST_EXTENSION = 0x00000008,
            REQUEST_SIZE = 0x00000010,
            REQUEST_DATE_CREATED = 0x00000020,
            REQUEST_DATE_MODIFIED = 0x00000040,
            REQUEST_DATE_ACCESSED = 0x00000080,
            REQUEST_ATTRIBUTES = 0x00000100,
            REQUEST_FILE_LIST_FILE_NAME = 0x00000200,
            REQUEST_RUN_COUNT = 0x00000400,
            REQUEST_DATE_RUN = 0x00000800,
            REQUEST_DATE_RECENTLY_CHANGED = 0x00001000,
            REQUEST_HIGHLIGHTED_FILE_NAME = 0x00002000,
            REQUEST_HIGHLIGHTED_PATH = 0x00004000,
            REQUEST_HIGHLIGHTED_FULL_PATH_AND_FILE_NAME = 0x00008000,
        }
        public enum SortFlags
        {
            SORT_NAME_ASCENDING = 1,
            SORT_NAME_DESCENDING = 2,
            SORT_PATH_ASCENDING = 3,
            SORT_PATH_DESCENDING = 4,
            SORT_SIZE_ASCENDING = 5,
            SORT_SIZE_DESCENDING = 6,
            SORT_EXTENSION_ASCENDING = 7,
            SORT_EXTENSION_DESCENDING = 8,
            SORT_TYPE_NAME_ASCENDING = 9,
            SORT_TYPE_NAME_DESCENDING = 10,
            SORT_DATE_CREATED_ASCENDING = 11,
            SORT_DATE_CREATED_DESCENDING = 12,
            SORT_DATE_MODIFIED_ASCENDING = 13,
            SORT_DATE_MODIFIED_DESCENDING = 14,
            SORT_ATTRIBUTES_ASCENDING = 15,
            SORT_ATTRIBUTES_DESCENDING = 16,
            SORT_FILE_LIST_FILENAME_ASCENDING = 17,
            SORT_FILE_LIST_FILENAME_DESCENDING = 18,
            SORT_RUN_COUNT_ASCENDING = 19,
            SORT_RUN_COUNT_DESCENDING = 20,
            SORT_DATE_RECENTLY_CHANGED_ASCENDING = 21,
            SORT_DATE_RECENTLY_CHANGED_DESCENDING = 22,
            SORT_DATE_ACCESSED_ASCENDING = 23,
            SORT_DATE_ACCESSED_DESCENDING = 24,
            SORT_DATE_RUN_ASCENDING = 25,
            SORT_DATE_RUN_DESCENDING = 26,
        }
        public enum TargetMachineFlags
        {
            TARGET_MACHINE_X86 = 1,
            TARGET_MACHINE_X64 = 2,
            TARGET_MACHINE_ARM = 3
        }
    }
    public class EverythingApiImport
    {
        public const string DllPath = "Everything32.dll";

        [DllImport(DllPath, CharSet = CharSet.Unicode)]
        public static extern UInt32 Everything_SetSearchW(string lpSearchString);
        [DllImport(DllPath)]
        public static extern void Everything_SetMatchPath(bool bEnable);
        [DllImport(DllPath)]
        public static extern void Everything_SetMatchCase(bool bEnable);
        [DllImport(DllPath)]
        public static extern void Everything_SetMatchWholeWord(bool bEnable);
        [DllImport(DllPath)]
        public static extern void Everything_SetRegex(bool bEnable);
        [DllImport(DllPath)]
        public static extern void Everything_SetMax(UInt32 dwMax);
        [DllImport(DllPath)]
        public static extern void Everything_SetOffset(UInt32 dwOffset);

        [DllImport(DllPath)]
        public static extern bool Everything_GetMatchPath();
        [DllImport(DllPath)]
        public static extern bool Everything_GetMatchCase();
        [DllImport(DllPath)]
        public static extern bool Everything_GetMatchWholeWord();
        [DllImport(DllPath)]
        public static extern bool Everything_GetRegex();
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetMax();
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetOffset();
        [DllImport(DllPath)]
        public static extern IntPtr Everything_GetSearchW();
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetLastError();

        [DllImport(DllPath)]
        public static extern bool Everything_QueryW(bool bWait);

        [DllImport(DllPath)]
        public static extern void Everything_SortResultsByPath();

        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetNumFileResults();
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetNumFolderResults();
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetNumResults();
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetTotFileResults();
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetTotFolderResults();
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetTotResults();
        [DllImport(DllPath)]
        public static extern bool Everything_IsVolumeResult(UInt32 nIndex);
        [DllImport(DllPath)]
        public static extern bool Everything_IsFolderResult(UInt32 nIndex);
        [DllImport(DllPath)]
        public static extern bool Everything_IsFileResult(UInt32 nIndex);
        [DllImport(DllPath, CharSet = CharSet.Unicode)]
        public static extern void Everything_GetResultFullPathName(UInt32 nIndex, StringBuilder lpString, UInt32 nMaxCount);
        [DllImport(DllPath)]
        public static extern void Everything_Reset();

        [DllImport(DllPath, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultFileName(UInt32 nIndex);

        // Everything 1.4
        [DllImport(DllPath)]
        public static extern void Everything_SetSort(UInt32 dwSortType);
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetSort();
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetResultListSort();
        [DllImport(DllPath)]
        public static extern void Everything_SetRequestFlags(UInt32 dwRequestFlags);
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetRequestFlags();
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetResultListRequestFlags();
        [DllImport(DllPath, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultExtension(UInt32 nIndex);
        [DllImport(DllPath)]
        public static extern bool Everything_GetResultSize(UInt32 nIndex, out long lpFileSize);
        [DllImport(DllPath)]
        public static extern bool Everything_GetResultDateCreated(UInt32 nIndex, out long lpFileTime);
        [DllImport(DllPath)]
        public static extern bool Everything_GetResultDateModified(UInt32 nIndex, out long lpFileTime);
        [DllImport(DllPath)]
        public static extern bool Everything_GetResultDateAccessed(UInt32 nIndex, out long lpFileTime);
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetResultAttributes(UInt32 nIndex);
        [DllImport(DllPath, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultFileListFileName(UInt32 nIndex);
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetResultRunCount(UInt32 nIndex);
        [DllImport(DllPath)]
        public static extern bool Everything_GetResultDateRun(UInt32 nIndex, out long lpFileTime);
        [DllImport(DllPath)]
        public static extern bool Everything_GetResultDateRecentlyChanged(UInt32 nIndex, out long lpFileTime);
        [DllImport(DllPath, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedFileName(UInt32 nIndex);
        [DllImport(DllPath, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedPath(UInt32 nIndex);
        [DllImport(DllPath, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedFullPathAndFileName(UInt32 nIndex);
        [DllImport(DllPath)]
        public static extern UInt32 Everything_GetRunCountFromFileName(string lpFileName);
        [DllImport(DllPath)]
        public static extern bool Everything_SetRunCountFromFileName(string lpFileName, UInt32 dwRunCount);
        [DllImport(DllPath)]
        public static extern UInt32 Everything_IncRunCountFromFileName(string lpFileName);
    }
}
