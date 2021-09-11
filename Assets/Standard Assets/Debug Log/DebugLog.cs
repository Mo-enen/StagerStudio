namespace DebugLog {
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;


	public static class DebugLog {


		// Data
		private static StreamWriter LogWriter = null;
		public static bool UseLog = true;


		// API
		public static string Init (string folderPath, int maxFileCount = 64, string extension = ".txt") {

			string errorMessage = "";

			try {
				Util.CreateFolder(folderPath);
			} catch (System.Exception ex) {
				errorMessage += ex.Message + "\r\n";
			}

			// Delete to Max
			try {
				if (Util.GetFileCount(folderPath, $"*{extension}", SearchOption.TopDirectoryOnly) > maxFileCount) {
					var files = Util.GetFilesIn(folderPath, true, $"*{extension}");
					int deleteCount = files.Length - maxFileCount;
					for (int i = 0; i < deleteCount; i++) {
						try {
							Util.DeleteFile(files[i].FullName);
						} catch (System.Exception ex) {
							errorMessage += ex.Message + "\r\n";
						}
					}
				}
			} catch (System.Exception ex) {
				errorMessage += ex.Message + "\r\n";
			}

			// Try Create File
			try {
				string logPath = Util.CombinePaths(folderPath, $"{System.DateTime.Now.ToString("yyyy_MM_dd")}{extension}");
				if (!File.Exists(logPath)) {
					var tempStream = File.Create(logPath);
					tempStream.Close();
				}
				// Stream
				FileStream fs = new FileStream(logPath, FileMode.Append);
				LogWriter = new StreamWriter(fs, Encoding.UTF8);
				LogWriter.AutoFlush = true;
			} catch (System.Exception ex) {
				errorMessage += ex.Message + "\r\n";
			}

			return errorMessage;
		}


		public static void CloseLogStream () {
			if (LogWriter != null) {
				var fs = LogWriter.BaseStream;
				LogWriter.Close();
				fs.Close();
			}
			LogWriter = null;
		}


		public static void Log (string message, bool timeLabel = true) => LogAsync(message, timeLabel);


		public static void LogException (string title, string sub, System.Exception ex) {
			if (LogWriter == null || !UseLog) { return; }
			LogFormat(title, sub, true, ("Exception", ex.Message));
		}


		public static void LogFormat (string title, string sub, bool forceSingleLine, params (string name, object obj)[] messages) {
			if (LogWriter == null || !UseLog) { return; }
			var builder = new StringBuilder();
			builder.Append($"{title},");
			builder.Append($"{sub},");
			int len = messages.Length;
			for (int i = 0; i < len; i++) {
				var (name, obj) = messages[i];
				builder.Append(name);
				builder.Append(':');
				builder.Append(obj != null ? obj.ToString() : "null");
				builder.Append(i < len - 1 ? ',' : ';');
			}
			LogAsync(forceSingleLine ? builder.ToString().Replace("\r", "\\r").Replace("\n", "\\n") : builder.ToString(), true);
		}


		private static async void LogAsync (string message, bool timeLabel) {
			if (LogWriter == null || !UseLog) { return; }
			try {
				await LogWriter.WriteLineAsync($"{(timeLabel ? System.DateTime.Now.ToString("HH-mm-ss") : "")}, {message}");
			} catch { }
		}


	}
}