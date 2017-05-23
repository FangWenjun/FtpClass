using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.FtpClient;
using System.Net;
using System.IO;

namespace Ftp_Client
{
	class Program
	{
		static void Main(string[] args)
		{
			FTPConnection ftpconn = new FTPConnection();
			ftpconn.FTPIsConnected("127.0.0.1","Administrator","218818");
			ftpconn.FTPIsdownload("127.0.0.1", "Administrator", "218818","", @"D:\光纤传感监测系统\FtpClient",".jpg");


		}
	}
}




	
