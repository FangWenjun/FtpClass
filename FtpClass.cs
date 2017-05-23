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
	public class FTPConnection
	{

		public FTPConnection() { }

		/// <summary>
		/// 连接FTP服务器函数
		/// </summary>
		/// <param name="strServer">服务器IP</param>
		/// <param name="strUser">用户名</param>
		/// <param name="strPassword">密码</param>
		public bool FTPIsConnected(string strServer, string strUser, string strPassword)
		{
			using(FtpClient ftp = new FtpClient())
			{
				ftp.Host = strServer;
				ftp.Credentials = new NetworkCredential(strUser, strPassword);
				ftp.Connect();
				return ftp.IsConnected;
			}
		}


		/// <summary>
		/// FTP下载文件
		/// </summary>
		/// <param name="strServer">服务器IP</param>
		/// <param name="strUser">用户名</param>
		/// <param name="strPassword">密码</param>
		/// <param name="Serverpath">服务器路径，例子："/Serverpath/"</param>
		/// <param name="localpath">本地保存路径</param>
		/// <param name="filetype">所下载的文件类型,例子：".rte"</param>
		public bool FTPIsdownload(string strServer, string strUser, string strPassword, string Serverpath, string localpath, string filetype)
		{

			FtpClient ftp = new FtpClient();
			ftp.Host = strServer;
			ftp.Credentials = new NetworkCredential(strUser, strPassword);
			ftp.Connect();

			string path = Serverpath;
			string destinationDirectory = localpath;
			List<string> documentname = new List<string>();
			bool DownloadStatus = false;

			if(Directory.Exists(destinationDirectory))
			{
				#region  从FTP服务器下载文件
				foreach(var ftpListItem in ftp.GetListing(path, FtpListOption.Modify | FtpListOption.Size)
				  .Where(ftpListItem => string.Equals(Path.GetExtension(ftpListItem.Name), filetype)))
				{
					string destinationPath = string.Format(@"{0}\{1}", destinationDirectory, ftpListItem.Name);
					using(Stream ftpStream = ftp.OpenRead(ftpListItem.FullName))
					using(FileStream fileStream = File.Create(destinationPath, ( int ) ftpStream.Length))
					{
						var buffer = new byte[200 * 1024];
						int count;
						while((count = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
						{
							fileStream.Write(buffer, 0, count);
						}
					}
					documentname.Add(ftpListItem.Name);
				}
				#endregion

				#region  验证本地是否有该文件
				string[] files = Directory.GetFiles(localpath, "*"+filetype);
				int filenumber = 0;
				foreach(string strfilename in files)
				{
					foreach(string strrecievefile in documentname)
					{
						if(strrecievefile == Path.GetFileName(strfilename))
						{
							filenumber++;
							break;
						}
					}
				}
				if(filenumber == documentname.Count)
				{
					DownloadStatus = true;
				}
				#endregion
			}
			return DownloadStatus;
		}

	}

}
