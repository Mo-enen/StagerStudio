namespace DebugLog {
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;

	public static class DebugLog {


		// Data
		private static StreamWriter LogWriter = null;
		public static bool UseLog = true;


		// API
		public static bool Init (string folderPath, int maxFileCount = 64, string extension = ".txt") {

			// Delete to Max
			try {
				if (Util.GetFileCount(folderPath, $"*{extension}", SearchOption.TopDirectoryOnly) > maxFileCount) {
					var files = Util.GetFilesIn(folderPath, true, $"*{extension}");
					int deleteCount = files.Length - maxFileCount;
					for (int i = 0; i < deleteCount; i++) {
						try {
							Util.DeleteFile(files[i].FullName);
						} catch { }
					}
				}
			} catch { }

			// Try Create File
			try {
				string logPath = Util.CombinePaths(folderPath, $"{System.DateTime.Now.ToString("yyyy_MM_dd")}{extension}");
				Util.CreateFolder(Directory.GetParent(folderPath).FullName);
				if (!File.Exists(logPath)) {
					var tempStream = File.Create(logPath);
					tempStream.Close();
				}
				// Stream
				FileStream fs = new FileStream(logPath, FileMode.Append);
				LogWriter = new StreamWriter(fs, Encoding.UTF8);
				LogWriter.AutoFlush = true;
			} catch { return false; }

			return true;
		}


		public static void CloseLogStream () {
			if (LogWriter != null) {
				var fs = LogWriter.BaseStream;
				LogWriter.Close();
				fs.Close();
			}
			LogWriter = null;
		}


		public static void Log (string message, bool timeLabel = true) {
			LogAsync(message, timeLabel);
		}


		public static void LogFormat (string title, string sub, bool forceSingleLine, params (string name, object obj)[] messages) {
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
			LogAsync(forceSingleLine ? builder.ToString().Replace("\r", "").Replace("\n", "") : builder.ToString(), true);
		}


		private static async void LogAsync (string message, bool timeLabel) {
			if (LogWriter == null || !UseLog) { return; }
			try {
				await LogWriter.WriteLineAsync($"{(timeLabel ? System.DateTime.Now.ToString("HH-mm-ss") : "")}, {message}");
			} catch { }
		}


	}
}