namespace StagerStudio.Saving {

	using UnityEngine;


	public abstract class Saving<T> {

		public T Value {
			get {
				return _Value;
			}
			set {
				if (_Value != null && !_Value.Equals(value)) {
					_Value = value;
					SetValueToPref();
				}
			}
		}
		public string Key;

		protected T DefaultValue;

		protected T _Value;


		public Saving (string key, T defaultValue) {
			Key = key;
			DefaultValue = defaultValue;
			_Value = defaultValue;
		}


		public void Load () {
			_Value = GetValueFromPref();
		}



		public abstract void Reset ();
		protected abstract T GetValueFromPref ();
		protected abstract void SetValueToPref ();


	}




}