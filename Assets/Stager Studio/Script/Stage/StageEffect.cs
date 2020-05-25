namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Saving;


	public class StageEffect : MonoBehaviour {




		#region --- SUB ---




		#endregion




		#region --- VAR ---


		// Ser
		[SerializeField] private Transform[] m_Containers = null;
		[SerializeField] private Transform m_Root = null;
		[SerializeField] private ParticleSystem m_CreateParticle = null;
		[SerializeField] private ParticleSystem m_DeleteParticle = null;

		// Data
		public SavingBool UseEffect = new SavingBool("StageEffect.UseEffect", true);


		#endregion




		#region --- API ---


		public void SpawnCreateEffect (int itemType, int itemIndex) {
			if (!UseEffect) { return; }
			SpawnEffect(m_CreateParticle, itemType, itemIndex, false);
		}


		public void SpawnDeleteEffect (int itemType, int itemIndex) {
			if (!UseEffect) { return; }
			SpawnEffect(m_DeleteParticle, itemType, itemIndex, true);
		}


		#endregion




		#region --- LGC ---


		private Transform GetMapItem (int itemType, int itemIndex) {
			if (itemType < 0 || itemType >= m_Containers.Length) { return null; }
			var con = m_Containers[itemType];
			int itemCount = con.childCount;
			if (itemIndex < 0 || itemIndex >= itemCount) { return null; }
			return con.GetChild(itemIndex);
		}


		private void SpawnEffect (ParticleSystem prefab, int itemType, int itemIndex, bool immediate) {
			if (immediate) {
				Spawn();
			} else {
				StartCoroutine(Spawning());
			}
			// Func
			IEnumerator Spawning () {
				yield return new WaitForEndOfFrame();
				yield return new WaitForEndOfFrame();
				Spawn();
			}
			void Spawn () {
				var item = GetMapItem(itemType, itemIndex);
				if (item == null) { return; }
				var target = item.GetChild(0);
				var par = Instantiate(prefab, m_Root);
				par.transform.position = target.position;
				par.transform.rotation = target.rotation;
				par.transform.localScale = Vector3.one;
				var main = par.main;
				main.startSizeXMultiplier = target.localScale.x;
				main.startSizeYMultiplier = target.localScale.y;
			}
		}


		#endregion




	}
}