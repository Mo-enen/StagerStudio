namespace StagerStudio.Saving {
	public abstract class Saving<T> {


		// API
		public T Value {
			get {
				if (!Loaded) {
					_Value = GetValueFromPref();
					Loaded = true;
				}
				return _Value;
			}
			set {
				if (_Value != null && !_Value.Equals(value)) {
					_Value = value;
					Loaded = true;
					SetValueToPref();
				}
			}
		}
		public string Key { get; private set; }
		public T DefaultValue { get; private set; }

		// Data
		private T _Value;
		private bool Loaded;


		// API
		public Saving (string key, T defaultValue) {
			Key = key;
			DefaultValue = defaultValue;
			_Value = defaultValue;
			Loaded = false;
		}

		public void Reset () {
			_Value = DefaultValue;
			DeleteKey();
		}


		// ABS
		protected abstract void DeleteKey ();

		protected abstract T GetValueFromPref ();

		protected abstract void SetValueToPref ();


	}
}