using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _KMH_Framework
{
    public class NPOIExcelReadHandler : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[NPOIExcelReadHandler]</b></color> {0}";

        protected static NPOIExcelReadHandler _instance;
        public static NPOIExcelReadHandler Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [SerializeField]
        protected string excelUri;
        [SerializeField]
        protected string excelName;

        [Space(10)]
        [SerializeField]
        protected string sheetName;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected bool _isExcelReadComplete = false;
        public bool IsExcelReadComplete
        {
            get
            {
                return _isExcelReadComplete;
            }
            protected set
            {
                _isExcelReadComplete = value;
            }
        }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Instance Overlapped");
                Destroy(this.gameObject);
                return;
            }

            Debug.Assert(IsExcelReadComplete == false);

            NPOIExcelReader.ReadExcel(excelUri, excelName, sheetName, OnReadExcel);
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        protected virtual void OnReadExcel(ISheet readSheet)
        {
            Debug.LogFormat(LOG_FORMAT, "OnReadExcel()");

            for (int i = 1; i <= readSheet.LastRowNum; i++)
            {
                IRow _row = readSheet.GetRow(i);

                for (int j = 0; j < _row.LastCellNum; j++)
                {
                    Debug.Log(_row.GetCell(j).ToString());
                }
            }

            IsExcelReadComplete = true;
        }
    }
}