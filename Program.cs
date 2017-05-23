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
			FTPConnection ftpconn = new FTPConnection("127.0.0.1","Administrator","218818");
			ftpconn.FTPIsConnected();
			ftpconn.FTPIsdownload("", @"D:\光纤传感监测系统\FtpClient",".jpg");
			ftpconn.FTPUpload("",@"D:\光纤传感监测系统\Windows 窗体中的事件顺序.pdf");


		}
	}
}




	
