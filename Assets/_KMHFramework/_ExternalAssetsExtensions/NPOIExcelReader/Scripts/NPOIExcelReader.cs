using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;
using UnityEngine;

namespace _KMH_Framework
{ 
    public static class NPOIExcelReader
    {
        private const string LOG_FORMAT = "<color=white><b>[NPOIExcelReader]</b></color> {0}";

        public delegate void ReadExcelCallback(ISheet _sheet);
        private static ReadExcelCallback OnReadExcel;

        public static void ReadExcel(string streamingAssetExcelUri, string streamingAssetExcelName, string sheetName, ReadExcelCallback _onReadExcel = null)
        {
            Debug.Assert(string.IsNullOrEmpty(streamingAssetExcelUri) == false);
            Debug.Assert(string.IsNullOrEmpty(streamingAssetExcelName) == false);
            Debug.Assert(string.IsNullOrEmpty(sheetName) == false);

            string _streamingAssetExcelName;
            if (streamingAssetExcelName.Contains(".xls") == true)
            {
                _streamingAssetExcelName = streamingAssetExcelName;
            }
            else
            {
                _streamingAssetExcelName = streamingAssetExcelName + ".xls";
            }
            string excelFullPath = Application.streamingAssetsPath + "/" + streamingAssetExcelUri + "/" + _streamingAssetExcelName;

            Debug.LogFormat(LOG_FORMAT, "ReadExcel(), excelFullPath : <color=white><b>" + excelFullPath + "</b></color>");

            HSSFWorkbook _workbook;
            using (FileStream _fileStream = new FileStream(excelFullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                _workbook = new HSSFWorkbook(_fileStream);
            }

            ISheet readSheet = _workbook.GetSheet(sheetName);
            if (_onReadExcel != null)
            {
                OnReadExcel += _onReadExcel;
                OnReadExcel(readSheet);
                OnReadExcel -= _onReadExcel;
            }
        }
    }
}