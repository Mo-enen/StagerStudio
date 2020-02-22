namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class WelcomeUI : MonoBehaviour {




		// Ser
		[SerializeField] private Image m_Cover = null;
		[SerializeField] private Text m_Title = null;
		[SerializeField] private Text m_Author = null;
		[SerializeField] private Text m_Description = null;



		private void Update () {
			if (Input.anyKeyDown) {
				Close();
			}
		}


		public void Init (string title, string backgourndAuthor, string musicAuthor, string beatmapAuthor, string description, Sprite cover) {
			// Null Fix
			title = string.IsNullOrEmpty(title) ? "(untitled)" : title;
			description = string.IsNullOrEmpty(description) ? "(no description)" : description;
			backgourndAuthor = string.IsNullOrEmpty(backgourndAuthor) ? "-" : backgourndAuthor;
			musicAuthor = string.IsNullOrEmpty(musicAuthor) ? "-" : musicAuthor;
			beatmapAuthor = string.IsNullOrEmpty(beatmapAuthor) ? "-" : beatmapAuthor;
			// Do it
			m_Cover.sprite = cover;
			m_Cover.color = cover ? Color.white : Color.clear;
			m_Title.text = title;
			m_Description.text = description;
			m_Author.text = $"{beatmapAuthor} | {musicAuthor} | {backgourndAuthor} | ";
			if (cover && cover.rect.height > 0f) {
				m_Cover.GetComponent<AspectRatioFitter>().aspectRatio = cover.rect.width / cover.rect.height;
			}
		}


		public void Close () {
			if (!enabled) { return; }
			enabled = false;
			GetComponent<Animator>().SetTrigger("Close");
			GetComponent<Graphic>().enabled = false;
			CancelInvoke();
			Invoke("Invoke_End", 0.5f);
		}



		private void Invoke_End () {
			transform.parent.gameObject.SetActive(false);
			transform.parent.parent.InactiveIfNoChildActive();
			DestroyImmediate(gameObject, false);
		}


	}
}