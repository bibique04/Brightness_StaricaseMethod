using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileIO
{
    class Logger
    {
            //log file variable
            string path;
            StreamWriter log;

            public Logger(string filePath)
            {
                path = filePath;
                log = new StreamWriter(path,true);
            }
            public void Write(string txt)
            {
                log.WriteLine(txt);
                log.Flush();
            }
            public void Close()
            {
                log.Close();
            }
        }
    }
