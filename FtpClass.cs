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

		private string ftpServerIP;	  
		private string ftpUserID;
		private string ftpPassword;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="FtpServerIP">ip地址</param>
		/// <param name="FtpUserID">登录认证用户名</param>
		/// <param name="FtpPassword">密码</param>
		public FTPConnection(string FtpServerIP,  string FtpUserID, string FtpPassword)
		{
			ftpServerIP = FtpServerIP;
			ftpUserID = FtpUserID;
			ftpPassword = FtpPassword;
		}

	
		public bool FTPIsConnected()
		{
			using(FtpClient ftp = new FtpClient())
			{
				ftp.Host = ftpServerIP;
				ftp.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
				ftp.Connect();
				return ftp.IsConnected;
			}
		}


		/// <summary>
		/// FTP下载文件
		/// </summary>
		/// <param name="Serverpath">服务器路径，例子："/Serverpath/"</param>
		/// <param name="localpath">本地保存路径</param>
		/// <param name="filetype">所下载的文件类型,例子：".rte"</param>
		public bool FTPIsdownload( string Serverpath, string localpath, string filetype)
		{

			FtpClient ftp = new FtpClient();
			ftp.Host = ftpServerIP;
			ftp.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
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




		/// <summary>
		/// 上传
		/// </summary>
		/// <param name="ftpRemotePath">服务器路径，例子："/Serverpath/"</param>
		/// <param name="filename">上传的文件路径</param>
		public void FTPUpload(string Serverpath, string filename)
		{
			FileInfo fileInf = new FileInfo(filename);
			string ftpURI = "ftp://" + ftpServerIP + "/" + Serverpath + "/";
			string uri = ftpURI + fileInf.Name;
			FtpWebRequest reqFTP;

			reqFTP = ( FtpWebRequest ) FtpWebRequest.Create(new Uri(uri));
			reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
			reqFTP.KeepAlive = false;
			reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
			reqFTP.UseBinary = true;
			reqFTP.UsePassive = false;
			reqFTP.ContentLength = fileInf.Length;
			int buffLength = 2048;
			byte[] buff = new byte[buffLength];
			int contentLen;
			FileStream fs = fileInf.OpenRead();
			try
			{
				Stream strm = reqFTP.GetRequestStream();
				contentLen = fs.Read(buff, 0, buffLength);
				while(contentLen != 0)
				{
					strm.Write(buff, 0, contentLen);
					contentLen = fs.Read(buff, 0, buffLength);
				}
				strm.Close();
				fs.Close();
			}
			catch(Exception ex)
			{
				throw new Exception("Ftphelper Upload Error --> " + ex.Message);
			}
		}


		/// <summary>
		/// 删除文件
		/// </summary>
		/// <param name="fileName"></param>
		public void Delete(string Serverpath, string fileName)
		{
			try
			{
				string ftpURI = "ftp://" + ftpServerIP + "/" + Serverpath + "/";
				string uri = ftpURI + fileName;
				FtpWebRequest reqFTP;
				reqFTP = ( FtpWebRequest ) FtpWebRequest.Create(new Uri(uri));

				reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
				reqFTP.KeepAlive = false;
				reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
				reqFTP.UsePassive = false;

				string result = String.Empty;
				FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
				long size = response.ContentLength;
				Stream datastream = response.GetResponseStream();
				StreamReader sr = new StreamReader(datastream);
				result = sr.ReadToEnd();
				sr.Close();
				datastream.Close();
				response.Close();
			}
			catch(Exception ex)
			{
				throw new Exception("FtpHelper Delete Error --> " + ex.Message + "  文件名:" + fileName);
			}
		}

		/// <summary>
		/// 删除文件夹
		/// </summary>
		/// <param name="folderName"></param>
		public void RemoveDirectory(string Serverpath, string folderName)
		{
			try
			{
				string ftpURI = "ftp://" + ftpServerIP + "/" + Serverpath + "/";
				string uri = ftpURI + folderName;
				FtpWebRequest reqFTP;
				reqFTP = ( FtpWebRequest ) FtpWebRequest.Create(new Uri(uri));

				reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
				reqFTP.KeepAlive = false;
				reqFTP.Method = WebRequestMethods.Ftp.RemoveDirectory;
				reqFTP.UsePassive = false;

				string result = String.Empty;
				FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
				long size = response.ContentLength;
				Stream datastream = response.GetResponseStream();
				StreamReader sr = new StreamReader(datastream);
				result = sr.ReadToEnd();
				sr.Close();
				datastream.Close();
				response.Close();
			}
			catch(Exception ex)
			{
				throw new Exception("FtpHelper Delete Error --> " + ex.Message + "  文件名:" + folderName);
			}
		}

		/// <summary>
		/// 获取当前目录下明细(包含文件和文件夹)
		/// </summary>
		/// <returns></returns>
		public string[] GetFilesDetailList(string Serverpath)
		{
			string ftpURI = "ftp://" + ftpServerIP + "/" + Serverpath + "/";
			string[] downloadFiles;
			try
			{
				StringBuilder result = new StringBuilder();
				FtpWebRequest ftp;
				ftp = ( FtpWebRequest ) FtpWebRequest.Create(new Uri(ftpURI));
				ftp.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
				ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
				ftp.UsePassive = false;
				WebResponse response = ftp.GetResponse();
				StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);

				//while (reader.Read() > 0)
				//{

				//}
				string line = reader.ReadLine();
				//line = reader.ReadLine();
				//line = reader.ReadLine();

				while(line != null)
				{
					result.Append(line);
					result.Append("\n");
					line = reader.ReadLine();
				}
				result.Remove(result.ToString().LastIndexOf("\n"), 1);
				reader.Close();
				response.Close();
				return result.ToString().Split('\n');
			}
			catch(Exception ex)
			{
				downloadFiles = null;
				throw new Exception("FtpHelper  Error --> " + ex.Message);
			}
		}

		/// <summary>
		/// 获取当前目录下文件列表(仅文件)
		/// </summary>
		/// <returns></returns>
		public string[] GetFileList(string Serverpath,string mask)
		{
			string[] downloadFiles;
			StringBuilder result = new StringBuilder();
			FtpWebRequest reqFTP;
			string ftpURI = "ftp://" + ftpServerIP + "/" + Serverpath + "/";
			try
			{
				reqFTP = ( FtpWebRequest ) FtpWebRequest.Create(new Uri(ftpURI));
				reqFTP.UseBinary = true;
				reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
				reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
				reqFTP.UsePassive = false;
				WebResponse response = reqFTP.GetResponse();
				StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);

				string line = reader.ReadLine();
				while(line != null)
				{
					if(mask.Trim() != string.Empty && mask.Trim() != "*.*")
					{

						string mask_ = mask.Substring(0, mask.IndexOf("*"));
						if(line.Substring(0, mask_.Length) == mask_)
						{
							result.Append(line);
							result.Append("\n");
						}
					}
					else
					{
						result.Append(line);
						result.Append("\n");
					}
					line = reader.ReadLine();
				}
				result.Remove(result.ToString().LastIndexOf('\n'), 1);
				reader.Close();
				response.Close();
				return result.ToString().Split('\n');
			}
			catch(Exception ex)
			{
				downloadFiles = null;
				if(ex.Message.Trim() != "远程服务器返回错误: (550) 文件不可用(例如，未找到文件，无法访问文件)。")
				{
					throw new Exception("FtpHelper GetFileList Error --> " + ex.Message.ToString());
				}
				return downloadFiles;
			}
		}

		/// <summary>
		/// 获取当前目录下所有的文件夹列表(仅文件夹)
		/// </summary>
		/// <returns></returns>
		public string[] GetDirectoryList(string Serverpath)
		{
			string[] drectory = GetFilesDetailList(Serverpath);
			string m = string.Empty;
			foreach(string str in drectory)
			{
				int dirPos = str.IndexOf("<DIR>");
				if(dirPos > 0)
				{
					/*判断 Windows 风格*/
					m += str.Substring(dirPos + 5).Trim() + "\n";
				}
				else if(str.Trim().Substring(0, 1).ToUpper() == "D")
				{
					/*判断 Unix 风格*/
					string dir = str.Substring(54).Trim();
					if(dir != "." && dir != "..")
					{
						m += dir + "\n";
					}
				}
			}

			char[] n = new char[] { '\n' };
			return m.Split(n);
		}

		/// <summary>
		/// 判断当前目录下指定的子目录是否存在
		/// </summary>
		/// <param name="RemoteDirectoryName">指定的目录名</param>
		public bool DirectoryExist(string Serverpath,string RemoteDirectoryName)
		{
			string[] dirList = GetDirectoryList(Serverpath);
			foreach(string str in dirList)
			{
				if(str.Trim() == RemoteDirectoryName.Trim())
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 判断当前目录下指定的文件是否存在
		/// </summary>
		/// <param name="RemoteFileName">远程文件名</param>
		public bool FileExist(string Serverpath, string RemoteFileName)
		{
			string[] fileList = GetFileList(Serverpath, "*.*");
			foreach(string str in fileList)
			{
				if(str.Trim() == RemoteFileName.Trim())
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 创建文件夹
		/// </summary>
		/// <param name="dirName"></param>
		public void MakeDir(string Serverpath, string dirName)
		{
			FtpWebRequest reqFTP;
			string ftpURI = "ftp://" + ftpServerIP + "/" + Serverpath + "/";
			try
			{
				// dirName = name of the directory to create.
				reqFTP = ( FtpWebRequest ) FtpWebRequest.Create(new Uri(ftpURI + dirName));
				reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
				reqFTP.UseBinary = true;
				reqFTP.UsePassive = false;
				reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
				FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
				Stream ftpStream = response.GetResponseStream();

				ftpStream.Close();
				response.Close();
			}
			catch(Exception ex)
			{
				throw new Exception("FtpHelper MakeDir Error --> " + ex.Message);
			}
		}

		/// <summary>
		/// 获取指定文件大小
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		public long GetFileSize(string Serverpath, string filename)
		{
			FtpWebRequest reqFTP;
			long fileSize = 0;
			string ftpURI = "ftp://" + ftpServerIP + "/" + Serverpath + "/";
			try
			{
				reqFTP = ( FtpWebRequest ) FtpWebRequest.Create(new Uri(ftpURI + filename));
				reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
				reqFTP.UseBinary = true;
				reqFTP.UsePassive = false;
				reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
				FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
				Stream ftpStream = response.GetResponseStream();
				fileSize = response.ContentLength;

				ftpStream.Close();
				response.Close();
			}
			catch(Exception ex)
			{
				throw new Exception("FtpHelper GetFileSize Error --> " + ex.Message);
			}
			return fileSize;
		}

		/// <summary>
		/// 改名
		/// </summary>
		/// <param name="currentFilename"></param>
		/// <param name="newFilename"></param>
		public void ReName(string Serverpath, string currentFilename, string newFilename)
		{
			FtpWebRequest reqFTP;
			string ftpURI = "ftp://" + ftpServerIP + "/" + Serverpath + "/";
			try
			{
				reqFTP = ( FtpWebRequest ) FtpWebRequest.Create(new Uri(ftpURI + currentFilename));
				reqFTP.Method = WebRequestMethods.Ftp.Rename;
				reqFTP.RenameTo = newFilename;
				reqFTP.UseBinary = true;
				reqFTP.UsePassive = false;
				reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
				FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
				Stream ftpStream = response.GetResponseStream();

				ftpStream.Close();
				response.Close();
			}
			catch(Exception ex)
			{
				throw new Exception("FtpHelper ReName Error --> " + ex.Message);
			}
		}

		/// <summary>
		/// 移动文件
		/// </summary>
		/// <param name="currentFilename"></param>
		/// <param name="newFilename"></param>
		public void MovieFile(string Serverpath, string currentFilename, string newDirectory)
		{
			ReName(Serverpath, currentFilename, newDirectory);
		}

		/// <summary>
		/// 切换当前目录
		/// </summary>
		/// <param name="DirectoryName"></param>
		/// <param name="IsRoot">true 绝对路径   false 相对路径</param>
		public string GotoDirectory(string Serverpath, string DirectoryName, bool IsRoot)
		{
			string ftpRemotePath = "";
			string ftpURI;
			if(IsRoot)
			{
				ftpRemotePath = DirectoryName;
			}
			else
			{
				ftpRemotePath += DirectoryName + "/";
			}
			ftpURI = "ftp://" + ftpServerIP + "/" + Serverpath + "/";
			return ftpURI;
		}

	}

}
