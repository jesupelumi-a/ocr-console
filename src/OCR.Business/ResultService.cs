using OCR.Abstractions.Models;
using OCR.Abstractions.Services;
using OMV.Layouts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OCR.Business
{
    public class ResultService : IResultService
    {
        public void CreateCSV(List<Thumbnail> thumbnails, string filePath)
        {
            // Step 1: Get OMV.Layouts dll
            var layoutsPath = Path.Combine(Environment.CurrentDirectory, @"Layouts\OMV.Layouts.dll");
            var layoutsAssembly = Assembly.LoadFrom(layoutsPath);

            //Get List of Classes
            Type[] types = layoutsAssembly.GetTypes();
                        
            var _types = types.Where(t => !t.GetTypeInfo().IsDefined(typeof(CompilerGeneratedAttribute), true));

            foreach (Type tc in _types)
            {
                // create an instance of the object
                object ClassObj = Activator.CreateInstance(tc);

                bool isMatched = (bool)tc.InvokeMember("MatchAndCreateCSV", BindingFlags.Default | BindingFlags.InvokeMethod, null, ClassObj, new object[] { thumbnails, filePath });

                if (isMatched) break;
            }
        }

        public void TestCreateCSV(List<Thumbnail> thumbnails, string filePath)
        {
            var layoutA = new LayoutA();
            var isLayoutA = layoutA.MatchAndCreateCSV(thumbnails, filePath);
            if (isLayoutA) return;

            var layoutB = new LayoutB();
            var isLayoutB = layoutB.MatchAndCreateCSV(thumbnails, filePath);
            if (isLayoutB) return;


        }
    }
}
