using System.Collections;
using System;
using UnityEngine.Video;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIAwardPlayer : UIBase
{
	[SerializeField]
	private VideoClip _movie;

	[SerializeField]
	private RawImage _texture;

	[SerializeField]
	private VideoPlayer _videoPlayer;

	[SerializeField]
	private AudioSource _audioSource;

	private Coroutine _coroutine;

	public void PlayeVideo(string _video, Action callback = null)
	{
		Debug.Log("Video -> " + _video);
		_movie = Resources.Load<VideoClip>(_video);
		if (_movie == null)
		{
			callback?.Invoke();
			return;
		}

		StartCoroutine(PlayVideo(callback));
	}

	private IEnumerator PlayVideo(Action callback = null)
	{
		_videoPlayer.playOnAwake = false;
		_videoPlayer.isLooping = false;
		_audioSource.playOnAwake = false;
		_audioSource.loop = false;
		_audioSource.volume = 0.5f;
		_videoPlayer.source = VideoSource.VideoClip;
		_videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

		_videoPlayer.EnableAudioTrack(0, true);
		_videoPlayer.SetTargetAudioSource(0, _audioSource);

		_videoPlayer.clip = _movie;
		_videoPlayer.Prepare();

		while (!_videoPlayer.isPrepared)
		{
			yield return null;
		}

		_texture.texture = _videoPlayer.texture;

		_videoPlayer.Play();
		_audioSource.Play();

		while (_videoPlayer.isPlaying)
		{
			if (_videoPlayer.time >= 0.1f && _texture.color.a == 0.0f)
			{
				_texture.DOFade(1.0f, 1.0f);
			}
			yield return null;
		}
		callback?.Invoke();
	}

	private void OnDestroy()
	{
		_videoPlayer.Stop();
		_audioSource.Stop();
		Destroy(_videoPlayer);
		Destroy(_audioSource);
		_videoPlayer = null;
		_audioSource = null;
	}
}