﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace NBi.Core.ResultSet
{
    public class ResultSet
    {
        protected DataTable table;

        internal DataTable Table { get { return table; } }
        
        public DataColumnCollection Columns
        {
            get { return table.Columns; }
        }


        public DataRowCollection Rows
        {
            get { return table.Rows; }
        }

        public ResultSet()
        {
        }

        public void Load(DataSet dataSet)
        {
            Load(dataSet.Tables[0]);
        }

        public void Load(DataTable table)
        {
            this.table = table;
        }

        public void Load(IEnumerable<DataRow> rows)
        {
            table = new DataTable();
            rows.CopyToDataTable<DataRow>(table, LoadOption.OverwriteChanges);

            //display for debug
            ConsoleDisplay();
        }

        public void Load(string record)
        {
            table = new DataTable();
            var fields = record.Split(';');

            //if > 0 row
            if (fields.Count() > 0)
            {
                //Build structure
                for (int i = 0; i < fields.Length; i++)
                    Columns.Add(string.Format("Column{0}", i), typeof(string));
                
                //load each row one by one
                table.BeginLoadData();
                //Transform (null) [string] into null
                for (int i = 0; i < fields.Count(); i++)
                {
                    if (fields[i] != null && fields[i].ToString().ToLower() == "(null)".ToLower())
                        fields[i] = null;
                }
                table.LoadDataRow(fields, LoadOption.OverwriteChanges);
                table.EndLoadData();  
            }
        }

        public void Load(IEnumerable<object[]> objects)
        {
            table = new DataTable();

            //if > 0 row
            if (objects.Count() > 0)
            {

                //Build structure
                for (int i = 0; i < objects.First().Length; i++)
                {
                    if (objects.First().ElementAt(i) == null)
                        Columns.Add(string.Format("Column{0}", i), typeof(string));
                    else
                        Columns.Add(string.Format("Column{0}", i), objects.First().ElementAt(i).GetType());
                }

                //load each row one by one
                table.BeginLoadData();
                foreach (var obj in objects)
                {
                    //Transform (null) [string] into null
                    for (int i = 0; i < obj.Count(); i++)
                    {
                        if (obj[i] != null && obj[i].ToString().ToLower() == "(null)".ToLower())
                            obj[i] = null;
                    }

                    table.LoadDataRow(obj, LoadOption.OverwriteChanges);
                }
                table.EndLoadData();
            }

            //display for debug
            ConsoleDisplay();
        }

        public void Load(IList<IRow> rows)
        {
            var objs = new List<object[]>();

            foreach (var row in rows)
            {
                var cells = row.Cells.ToArray<ICell>();
                var contentCells = new List<Object>();
                foreach (var cell in cells)
                    contentCells.Add(cell.Value);

                objs.Add(contentCells.ToArray());
            }

            this.Load(objs);
        }

        protected void ConsoleDisplay()
        {
            if (!NBiTraceSwitch.TraceVerbose)
                return;

            Trace.WriteLine(string.Format(new string('-', 30)));
            foreach (DataRow row in Rows)
            {
                foreach (object cell in row.ItemArray)
                    Trace.Write(string.Format("| {0}\t", cell.ToString()));
                Trace.WriteLine(string.Format("|"));
            }
            Trace.WriteLine(string.Format(new string('-', 30)));
            Trace.WriteLine(string.Format(""));
        }

    }
}