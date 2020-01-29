namespace StagerStudio {
	public static class NAudioUtil {
		public static void Mp3ToWav (string mp3, string wave) {
			using (NAudio.Wave.Mp3FileReader reader = new NAudio.Wave.Mp3FileReader(mp3))
				NAudio.Wave.WaveFileWriter.CreateWaveFile(wave, reader);
		}
	}
}