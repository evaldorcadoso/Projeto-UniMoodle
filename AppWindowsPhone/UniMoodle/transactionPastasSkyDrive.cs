using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace UniMoodle
{
    public class transactionPastasSkyDrive
    {
        private string size;
        
        
        public string id { get; set; }
        public string name { get; set; }
        public string tipo { get; set; }
        
        public string Size 
        { 
            get
            {
                int aux = Convert.ToInt32(size);
                string s="";
                if (aux < 1024)
                {
                    s = aux.ToString("0.00") + " bytes";
                }
                else
                {
                    if (aux < 1024000)
                    {
                        s = ConvertBytesToKbytes(aux).ToString("0.00") + " KB";
                    }
                    else
                    {
                        if (aux < 1024000000)
                        {
                            s = ConvertBytesToMegabytes(Convert.ToInt32(size)).ToString("0.00") + " MB";
                        }
                        else
                        {
                            s = ConvertBytesToGigabytes(Convert.ToInt32(size)).ToString("0.00") + " GB";
                        }
                    }
                }
                return s;
            }
            set
            {
                size = value;
            }
        }

        public transactionPastasSkyDrive(string id,string name, string type,string sizep)
        {
            this.id = id;
            this.name = name;
            this.tipo = type;
            this.Size = sizep;
        }
        
        public transactionPastasSkyDrive()
        {
        }

        static double ConvertBytesToGigabytes(long bytes)
        {
            return ((bytes / 1024f) / 1024f) / 1024f;
        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        static double ConvertBytesToKbytes(long bytes)
        {
            return (bytes / 1024f);
        }
    }
}
