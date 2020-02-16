namespace StagerStudio.Saving {
	using UnityEngine;



	public class SavingBool : Saving<bool> {

		public SavingBool (string key, bool defaultValue) : base(key, defaultValue) { }

		protected override bool GetValueFromPref () {
			return PlayerPrefs.GetInt(Key, DefaultValue ? 1 : 0) == 1;
		}

		protected override void SetValueToPref () {
			PlayerPrefs.SetInt(Key, Value ? 1 : 0);
		}

		public static implicit operator bool (SavingBool value) {
			return value.Value;
		}

		protected override void DeleteKey () {

			PlayerPrefs.DeleteKey(Key);
		}

	}





	public class SavingInt : Saving<int> {

		public SavingInt (string key, int defaultValue) : base(key, defaultValue) { }

		protected override int GetValueFromPref () {
			return PlayerPrefs.GetInt(Key, DefaultValue);
		}

		protected override void SetValueToPref () {
			PlayerPrefs.SetInt(Key, Value);
		}

		public static implicit operator int (SavingInt value) {
			return value.Value;
		}
		protected override void DeleteKey () {

			PlayerPrefs.DeleteKey(Key);
		}
	}





	public class SavingString : Saving<string> {

		public SavingString (string key, string defaultValue) : base(key, defaultValue) { }

		protected override string GetValueFromPref () {
			return PlayerPrefs.GetString(Key, DefaultValue);
		}

		protected override void SetValueToPref () {
			PlayerPrefs.SetString(Key, Value);
		}

		public static implicit operator string (SavingString value) {
			return value.Value;
		}
		protected override void DeleteKey () {

			PlayerPrefs.DeleteKey(Key);
		}
	}





	public class SavingFloat : Saving<float> {

		public SavingFloat (string key, float defaultValue) : base(key, defaultValue) { }

		protected override float GetValueFromPref () {
			return PlayerPrefs.GetFloat(Key, DefaultValue);
		}

		protected override void SetValueToPref () {
			PlayerPrefs.SetFloat(Key, Value);
		}

		public static implicit operator float (SavingFloat value) {
			return value.Value;
		}
		protected override void DeleteKey () {
			PlayerPrefs.DeleteKey(Key);
		}
	}





	public class SavingVector2 : Saving<Vector2> {

		public SavingVector2 (string key, Vector2 defaultValue) : base(key, defaultValue) { }

		protected override Vector2 GetValueFromPref () {
			return new Vector2(
				PlayerPrefs.GetFloat(Key + ".x", DefaultValue.x),
				PlayerPrefs.GetFloat(Key + ".y", DefaultValue.y)
			);
		}

		protected override void SetValueToPref () {
			PlayerPrefs.SetFloat(Key + ".x", Value.x);
			PlayerPrefs.SetFloat(Key + ".y", Value.y);
		}

		public static implicit operator Vector2 (SavingVector2 value) {
			return value.Value;
		}
		protected override void DeleteKey () {
			PlayerPrefs.DeleteKey(Key + ".x");
			PlayerPrefs.DeleteKey(Key + ".y");
		}
	}




	public class SavingVector3 : Saving<Vector3> {

		public SavingVector3 (string key, Vector3 defaultValue) : base(key, defaultValue) { }

		protected override Vector3 GetValueFromPref () {
			return new Vector3(
				PlayerPrefs.GetFloat(Key + ".x", DefaultValue.x),
				PlayerPrefs.GetFloat(Key + ".y", DefaultValue.y),
				PlayerPrefs.GetFloat(Key + ".z", DefaultValue.z)
			);
		}

		protected override void SetValueToPref () {
			PlayerPrefs.SetFloat(Key + ".x", Value.x);
			PlayerPrefs.SetFloat(Key + ".y", Value.y);
			PlayerPrefs.SetFloat(Key + ".z", Value.z);
		}

		public static implicit operator Vector3 (SavingVector3 value) {
			return value.Value;
		}
		protected override void DeleteKey () {
			PlayerPrefs.DeleteKey(Key + ".x");
			PlayerPrefs.DeleteKey(Key + ".y");
			PlayerPrefs.DeleteKey(Key + ".z");
		}
	}





	public class SavingColor : Saving<Color> {

		public SavingColor (string key, Color defaultValue) : base(key, defaultValue) { }

		protected override Color GetValueFromPref () {
			return new Color(
				PlayerPrefs.GetFloat(Key + ".r", DefaultValue.r),
				PlayerPrefs.GetFloat(Key + ".g", DefaultValue.g),
				PlayerPrefs.GetFloat(Key + ".b", DefaultValue.b),
				PlayerPrefs.GetFloat(Key + ".a", DefaultValue.a)
			);
		}

		protected override void SetValueToPref () {
			PlayerPrefs.SetFloat(Key + ".r", Value.r);
			PlayerPrefs.SetFloat(Key + ".g", Value.g);
			PlayerPrefs.SetFloat(Key + ".b", Value.b);
			PlayerPrefs.SetFloat(Key + ".a", Value.a);
		}

		public static implicit operator Color (SavingColor value) {
			return value.Value;
		}
		protected override void DeleteKey () {
			PlayerPrefs.DeleteKey(Key + ".r");
			PlayerPrefs.DeleteKey(Key + ".g");
			PlayerPrefs.DeleteKey(Key + ".b");
			PlayerPrefs.DeleteKey(Key + ".a");
		}
	}



}